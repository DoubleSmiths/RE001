"""PowerShell 인용부호 손실 없이 C#을 unity-cli의 단일 인자로 전달한다."""
import pathlib
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8")
args = sys.argv[1:]
options = []
if "--allow-async" in args:
    args.remove("--allow-async")
    options.append("--allow-async")
code = pathlib.Path(args[0]).read_text(encoding="utf-8-sig") if args else sys.stdin.read()
cli = pathlib.Path.home() / "AppData/Local/unity-cli/unity-cli.exe"
result = subprocess.run(
    [str(cli), "exec", code, "--usings", "System.Linq", "--project",
     str(pathlib.Path(__file__).resolve().parent.parent), "--timeout", "30000"] + options,
    capture_output=True,
)
sys.stdout.write(result.stdout.decode("utf-8", errors="replace"))
sys.stderr.write(result.stderr.decode("utf-8", errors="replace"))
sys.exit(result.returncode)
