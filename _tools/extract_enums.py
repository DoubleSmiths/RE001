"""
Extract enum definitions from UI C# scripts.
Output: a JSON-ish dict of {script_path: {enum_name: [members]}}.
Used to know which child object names are referenced by name in bind().
"""
import re
from pathlib import Path

UI_ROOT = Path(r"C:\stopresent\Unity\RE001\Assets\@Scripts\UI")

# Match: enum Foo { A, B, C }
ENUM_RE = re.compile(r"\benum\s+(\w+)\s*\{([^}]*)\}", re.DOTALL)
MEMBER_RE = re.compile(r"^\s*([A-Za-z_]\w*)\s*(?:=\s*[^,]+)?\s*,?\s*(?://.*)?$", re.MULTILINE)


def extract(text: str):
    enums = {}
    for m in ENUM_RE.finditer(text):
        name = m.group(1)
        body = m.group(2)
        members = []
        for line in body.splitlines():
            line = line.split("//", 1)[0].strip().rstrip(",").strip()
            if not line:
                continue
            mm = re.match(r"([A-Za-z_]\w*)", line)
            if mm:
                members.append(mm.group(1))
        enums[name] = members
    return enums


def main():
    out = {}
    for cs in sorted(UI_ROOT.rglob("*.cs")):
        text = cs.read_text(encoding="utf-8", errors="ignore")
        enums = extract(text)
        if enums:
            rel = cs.relative_to(UI_ROOT)
            out[str(rel)] = enums
    for path, enums in out.items():
        print(f"\n=== {path} ===")
        for name, members in enums.items():
            print(f"  enum {name}: {len(members)} members")
            for m in members:
                print(f"    {m}")


if __name__ == "__main__":
    main()
