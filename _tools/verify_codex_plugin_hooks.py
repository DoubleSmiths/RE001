"""Windows 후크 회귀 검사. 실제 프로젝트 상태 대신 임시 디렉터리에서 실행한다.

python _tools/verify_codex_plugin_hooks.py
캐시 갱신으로 명령이 원복되면 repair_codex_plugin_hooks.py --apply 후 재실행.
SDK 설치 후크 및 조건부 보안 리뷰는 외부 접근 가능성이 있어 이 검사에서 제외한다.
"""
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import time

from repair_codex_plugin_hooks import BASE, PLUGINS, planned_changes, self_check

sys.stdout.reconfigure(encoding="utf-8")
self_check()
results = []
drift = [str(path) for path, *_ in planned_changes()]
plugins = PLUGINS
with tempfile.TemporaryDirectory(prefix="codex-hook-check-") as work:
    Path(work, ".omc-workspace").touch()
    Path(work, "security").mkdir()
    # 설치 검사는 제외한다. 테스트 상태에만 최근 설치 시도 표식을 둬 분리 설치를 방지한다.
    Path(work, "security", ".sdk_bootstrap_spawned").touch()
    for plugin, relative in plugins.items():
        root = BASE / relative
        manifest = root / "hooks" / ("claude-codex-hooks.json" if plugin == "ponytail" else "hooks.json")
        data = json.loads(manifest.read_text(encoding="utf-8"))
        for event, entries in data["hooks"].items():
            if event not in ("SessionStart", "UserPromptSubmit", "SubagentStart", "PreToolUse", "PostToolUse", "Stop"):
                continue
            for entry in entries:
                if entry.get("matcher") in ("init", "maintenance"):
                    continue
                for hook in entry["hooks"]:
                    if hook.get("if") or "ensure_agent_sdk.py" in hook["command"]:
                        continue
                    command = hook["command"].replace("${CLAUDE_PLUGIN_ROOT}", root.as_posix()).replace("$CLAUDE_PLUGIN_ROOT", root.as_posix())
                    env = dict(os.environ, CLAUDE_PLUGIN_ROOT=root.as_posix(), CLAUDE_CONFIG_DIR=work + "/config",
                               PLUGIN_DATA=work + "/plugin-data/" + plugin,
                               OMC_STATE_DIR=work + "/omc-state",
                               SECURITY_WARNINGS_STATE_DIR=work + "/security", PYTHONUTF8="1")
                    if plugin in ("superpowers", "ponytail"):
                        # 명령의 경로 치환과 자식 프로세스 환경변수 주입은 별개다.
                        env.pop("CLAUDE_PLUGIN_ROOT", None)
                        env.pop("COPILOT_PLUGIN_DATA", None)
                        env.pop("CURSOR_PLUGIN_ROOT", None)
                        env["PONYTAIL_DEFAULT_MODE"] = "full"
                    payload = json.dumps({"hook_event_name": event, "session_id": "hook-check", "cwd": work,
                                          "source": "startup", "prompt": "Explain the project.", "tool_name": "Read",
                                          "tool_input": {"file_path": work + "/sample.txt"},
                                          "tool_response": {"content": "test"}, "stop_hook_active": False})
                    result = {"plugin": plugin, "event": event, "manifest": str(manifest)}
                    started = time.monotonic()
                    try:
                        run = subprocess.run(["powershell.exe", "-NoProfile", "-Command", command], input=payload.encode(),
                                             capture_output=True, cwd=work, env=env, timeout=hook.get("timeout", 10))
                        result["exit_code"] = run.returncode
                        result["stderr"] = run.stderr.decode("utf-8", errors="replace").strip()
                        stdout = run.stdout.decode("utf-8-sig").strip()
                        if stdout:
                            # 두 JSON 또는 로그+JSON 출력도 json.loads가 거부한다.
                            output = json.loads(stdout)
                            assert isinstance(output, dict), "Hook output must be one JSON object"
                            if event == "SessionStart" or plugin == "ponytail":
                                allowed = {"continue", "stopReason", "systemMessage", "suppressOutput", "hookSpecificOutput"}
                                assert not (output.keys() - allowed), f"Unsupported {event} fields: {output.keys() - allowed}"
                                specific = output.get("hookSpecificOutput", {})
                                assert isinstance(specific, dict)
                                assert not (specific.keys() - {"hookEventName", "additionalContext"})
                                if specific:
                                    assert specific.get("hookEventName") == event
                                    assert isinstance(specific.get("additionalContext", ""), str)
                            if plugin == "ponytail":
                                assert "systemMessage" not in output, "Ponytail mode must not emit a UI warning"
                            if plugin in ("ponytail", "superpowers") and event == "SessionStart":
                                assert output["hookSpecificOutput"]["additionalContext"], "Mode instructions were lost"
                        result["ok"] = run.returncode == 0 and "Error:" not in result["stderr"]
                    except (subprocess.TimeoutExpired, UnicodeError, ValueError, AssertionError) as error:
                        result.update(ok=False, error=str(error))
                    result["seconds"] = round(time.monotonic() - started, 2)
                    results.append(result)
report = {"ok": not drift and all(r["ok"] for r in results), "manifest_drift": drift,
          "checked": len(results), "results": results,
          "excluded": ["SDK installation", "conditional security reviews"]}
print(json.dumps(report, ensure_ascii=False, indent=2))
sys.exit(not report["ok"])
