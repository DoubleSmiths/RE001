"""security-guidance의 Codex SessionStart 중복 JSON과 미지원 필드를 검사/복구한다."""
import argparse
import ast
import contextlib
import datetime
import io
import json
from pathlib import Path
import shutil
import sys


TARGET = Path.home() / '.codex/plugins/cache/claude-plugins-official/security-guidance/2.0.7/hooks/ensure_agent_sdk.py'
OLD = b'    print(json.dumps({"async": True, "asyncTimeout": 180000}), flush=True)'
NEW = b'    # Codex requires one JSON response; the final response is emitted below.'
METRICS_OLD = b'    response: dict[str, object] = {"metrics": metrics}'
METRICS_NEW = b'    response: dict[str, object] = {}  # Codex rejects the Claude-only metrics field.'


def check_output(source, path):
    # SDK 설치와 알림 파일 쓰기는 대체하고 실제 출력 분기를 실행한다.
    scope = {'__name__': 'hook_check', '__file__': str(path)}
    sys.path.insert(0, str(path.parent))
    try:
        exec(compile(source, str(path), 'exec'), scope)
    finally:
        sys.path.pop(0)
    entry = ast.parse(source).body[-1]
    assert isinstance(entry, ast.If) and ast.unparse(entry.test) == "__name__ == '__main__'"
    scope['__name__'] = '__main__'
    for outcome, notice in [(scope['NOOP_SYSTEM'], None), (scope['BUILD_FAILED'], 'SDK unavailable')]:
        scope['main'] = lambda: (outcome, '', '')
        scope['_maybe_emit_user_notice'] = lambda *args: notice
        captured = io.StringIO()
        with contextlib.redirect_stdout(captured):
            exec(compile(ast.Module(body=[entry], type_ignores=[]), str(path), 'exec'), scope)
        result = json.loads(captured.getvalue())
        # Codex는 문법상 올바른 JSON이어도 미지원 필드가 있으면 거부한다.
        expected = {}
        if notice:
            expected = {'systemMessage': notice, 'hookSpecificOutput': {
                'hookEventName': 'SessionStart', 'additionalContext': notice}}
        assert result == expected, f'Unsupported SessionStart output: {result}'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply', action='store_true')
    parser.add_argument('--path', type=Path, default=TARGET)
    args = parser.parse_args()
    before = args.path.read_bytes()
    assert before.count(OLD) == 1 or (OLD not in before and NEW in before), 'Unknown hook version; inspect before repairing'
    assert before.count(METRICS_OLD) == 1 or (METRICS_OLD not in before and METRICS_NEW in before), 'Unknown response format'
    after = before.replace(OLD, NEW).replace(METRICS_OLD, METRICS_NEW)
    check_output(after.decode('utf-8'), args.path)
    # 중복 JSON과 단일 JSON의 미지원 필드 모두 회귀 검사에서 잡혀야 한다.
    for broken in (after.replace(NEW, OLD), after.replace(METRICS_NEW, METRICS_OLD)):
        try:
            check_output(broken.decode('utf-8'), args.path)
        except (json.JSONDecodeError, AssertionError):
            pass
        else:
            raise AssertionError('Invalid output unexpectedly passed')
    if before == after:
        print('Already repaired; success/failure JSON checks passed')
        return
    # 회귀 검사가 원래의 중복 JSON 출력을 실제로 잡는지 확인한다.
    try:
        check_output(before.decode('utf-8'), args.path)
    except (json.JSONDecodeError, AssertionError):
        pass
    else:
        raise AssertionError('Original hook unexpectedly passed')
    if args.apply:
        assert args.path.read_bytes() == before, 'Hook changed during repair'
        backup = args.path.with_name(args.path.name + '.before-json-repair-' + datetime.datetime.now().strftime('%Y%m%d-%H%M%S-%f') + '.bak')
        shutil.copy2(args.path, backup)
        args.path.write_bytes(after)
        assert args.path.read_bytes() == after
        print('Backup:', backup)
    print('APPLIED' if args.apply else 'DRY RUN', args.path, '; success/failure JSON checks passed')


if __name__ == '__main__':
    sys.dont_write_bytecode = True
    main()
