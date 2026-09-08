import argparse, ast, contextlib, datetime, io, json, pathlib, re, shutil, sys
sys.stdout.reconfigure(encoding="utf-8")
BASE = pathlib.Path("C:/Users/Mycom/.codex/plugins/cache")
BASH = pathlib.Path("C:/Program Files/Git/bin/bash.exe")
PLUGINS = {
    "omc": "omc/oh-my-claudecode/5.3.0",
    "security": "claude-plugins-official/security-guidance/2.0.7",
    "ralph": "claude-plugins-official/ralph-loop/1.0.0",
    "superpowers": "claude-plugins-official/superpowers/6.3.0",
}

def repair_command(command, plugin):
    if plugin == "omc":
        return re.sub(r'"\$CLAUDE_PLUGIN_ROOT"(/[^\s"]+)', r'"$CLAUDE_PLUGIN_ROOT\1"', command).replace('$CLAUDE_PLUGIN_ROOT/', '${CLAUDE_PLUGIN_ROOT}/')
    if plugin in ("security", "ralph") and command.startswith("bash "):
        return '& "' + BASH.as_posix() + '" ' + command[5:]
    if plugin == "superpowers":
        return '& "' + BASH.as_posix() + '" "${CLAUDE_PLUGIN_ROOT}/hooks/session-start"'
    return command

def planned_changes():
    assert BASH.is_file(), "Git Bash is missing"
    changes = []
    for plugin, relative in PLUGINS.items():
        root = BASE / relative
        path = root / "hooks/hooks.json"
        before = path.read_bytes()
        data = json.loads(before)
        count = 0
        for entries in data["hooks"].values():
            for entry in entries:
                for hook in entry["hooks"]:
                    old = hook["command"]
                    hook["command"] = repair_command(old, plugin)
                    if plugin == "superpowers":
                        hook.pop("shell", None)
                    count += old != hook["command"]
        if count:
            after = (json.dumps(data, indent=2, ensure_ascii=False) + "\n").encode("utf-8")
            json.loads(after)
            changes.append((path, before, after, str(count) + " hook commands"))
        scripts = {"security": "sg-python.sh", "ralph": "stop-hook.sh", "superpowers": "session-start"}
        if plugin in scripts:
            path = root / "hooks" / scripts[plugin]
            before = path.read_bytes()
            after = before.replace(b"\r\n", b"\n")
            if plugin == "ralph":
                if b'export PATH="/usr/bin:$PATH"' not in after:
                    after = after.replace(b'set -euo pipefail\n', b'set -euo pipefail\nexport PATH="/usr/bin:$PATH"\n', 1)
                for line in after.splitlines():
                    if line.lstrip().startswith(b'echo ') and (b'Max iterations (' in line or b'Detected <promise>' in line) and not line.endswith(b'>&2'):
                        after = after.replace(line, line + b' >&2')
            if before != after:
                changes.append((path, before, after, "LF, Unix PATH and diagnostic stderr"))
        if plugin == "security":
            path = root / "hooks/security_reminder_hook.py"
            before = path.read_bytes()
            source = before.decode("utf-8").replace("\r\n", "\n")
            old = '    print(json.dumps(out), flush=True)'
            if 'out.pop("metrics", None)' not in source:
                assert source.count(old) == 1, "Security emitter changed; review before patching"
                source = source.replace(old, '    # Codex accepts security feedback without Claude-only telemetry.\n    out.pop("metrics", None)\n    out.pop("rewakeSummary", None)\n' + old)
                after = source.encode("utf-8")
                changes.append((path, before, after, "Preserve feedback; remove unsupported telemetry"))
            check_security_output(source)
    return changes

def check_security_output(source):
    func = next(n for n in ast.parse(source).body if isinstance(n, ast.FunctionDef) and n.name == "emit_metrics")
    scope = {"sys": sys, "json": json, "_PV": 20007, "_usage_metrics": lambda: {}}
    exec(compile(ast.Module(body=[func], type_ignores=[]), "security-emitter-check", "exec"), scope)
    for event, guidance in [("Stop", None), ("Stop", "Fix the vulnerability"), ("SubagentStop", "Fix the vulnerability"), ("PostToolUse", "Review the edit")]:
        stdout, stderr = io.StringIO(), io.StringIO()
        with contextlib.redirect_stdout(stdout), contextlib.redirect_stderr(stderr):
            scope["emit_metrics"]({"skipped": True}, rewake_summary="Review", additional_context=guidance, hook_event_name=event)
        result = json.loads(stdout.getvalue())
        assert not ({"metrics", "rewakeSummary"} & result.keys()), result
        if guidance and event in ("Stop", "SubagentStop"):
            assert result == {"decision": "block", "reason": guidance}
            assert stderr.getvalue() == guidance
        elif guidance:
            assert result["hookSpecificOutput"]["additionalContext"] == guidance
        else:
            assert result == {}

def self_check():
    assert repair_command('node "$CLAUDE_PLUGIN_ROOT"/scripts/run.cjs "$CLAUDE_PLUGIN_ROOT"/scripts/test.mjs', "omc") == 'node "${CLAUDE_PLUGIN_ROOT}/scripts/run.cjs" "${CLAUDE_PLUGIN_ROOT}/scripts/test.mjs"'
    assert repair_command('node "$CLAUDE_PLUGIN_ROOT/scripts/run.cjs"', "omc") == 'node "${CLAUDE_PLUGIN_ROOT}/scripts/run.cjs"'
    assert repair_command('bash "${CLAUDE_PLUGIN_ROOT}/hooks/stop-hook.sh"', "ralph") == '& "C:/Program Files/Git/bin/bash.exe" "${CLAUDE_PLUGIN_ROOT}/hooks/stop-hook.sh"'

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()
    self_check()
    changes = planned_changes()
    stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
    report = []
    for path, before, after, detail in changes:
        report.append({"path": str(path), "change": detail})
        if args.apply:
            assert path.read_bytes() == before, "File changed while preparing repair"
            backup = path.with_name(path.name + ".before-windows-hook-repair-" + stamp + ".bak")
            assert not backup.exists()
            report[-1]["backup"] = str(backup)
            shutil.copy2(path, backup)
            path.write_bytes(after)
            assert path.read_bytes() == after
    print(json.dumps({"applied": args.apply, "changes": report, "self_check": "passed"}, ensure_ascii=False))
