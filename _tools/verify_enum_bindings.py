"""
Sanity check: for every UI prefab that has a matching C# script with bind enums,
confirm that every enum member appears exactly 0 or 1 times as a GameObject m_Name
in the prefab. (0 = bind would log "Failed to bind", 1 = ok, >1 = duplicate problem.)
"""
import re
from pathlib import Path
from collections import defaultdict

PROJECT_ROOT = Path(r"C:\stopresent\Unity\RE001")
PREFAB_ROOT = PROJECT_ROOT / "Assets" / "@Resources" / "Prefab" / "UI"
SCRIPT_ROOT = PROJECT_ROOT / "Assets" / "@Scripts" / "UI"

ENUM_RE = re.compile(r"\benum\s+(\w+)\s*\{([^}]*)\}", re.DOTALL)
NAME_RE = re.compile(r"^  m_Name: (.+?)\s*$", re.MULTILINE)


def extract_enum_names(cs_path: Path) -> set:
    text = cs_path.read_text(encoding="utf-8", errors="ignore")
    names = set()
    for m in ENUM_RE.finditer(text):
        body = m.group(2)
        for line in body.splitlines():
            line = line.split("//", 1)[0].strip().rstrip(",").strip()
            mm = re.match(r"([A-Za-z_]\w*)", line)
            if mm:
                names.add(mm.group(1))
    return names


def scan_prefab_names(prefab_path: Path) -> dict:
    text = prefab_path.read_text(encoding="utf-8", errors="ignore")
    counts = defaultdict(int)
    for m in NAME_RE.finditer(text):
        n = m.group(1).strip()
        if (n.startswith("'") and n.endswith("'")) or (n.startswith('"') and n.endswith('"')):
            n = n[1:-1]
        counts[n] += 1
    return counts


def main():
    issues = 0
    for prefab in sorted(PREFAB_ROOT.rglob("*.prefab")):
        cs = SCRIPT_ROOT / "Popup" / f"{prefab.stem}.cs"
        if not cs.exists():
            cs = SCRIPT_ROOT / "Scene" / f"{prefab.stem}.cs"
        if not cs.exists():
            cs = SCRIPT_ROOT / "SubItem" / f"{prefab.stem}.cs"
        if not cs.exists():
            cs = SCRIPT_ROOT / "Toast" / f"{prefab.stem}.cs"
        if not cs.exists():
            continue

        enum_names = extract_enum_names(cs)
        if not enum_names:
            continue

        counts = scan_prefab_names(prefab)
        missing = []
        dupes = []
        for n in enum_names:
            c = counts.get(n, 0)
            if c == 0:
                missing.append(n)
            elif c > 1:
                dupes.append((n, c))

        if missing or dupes:
            print(f"\n=== {prefab.relative_to(PROJECT_ROOT).as_posix()} (script: {cs.name}) ===")
            for n in missing:
                print(f"  [MISSING] enum '{n}' has no matching GameObject")
            for n, c in dupes:
                print(f"  [DUPLICATE] enum '{n}' matches {c} GameObjects")
                issues += 1

    if issues == 0:
        print("\nOK: no enum→duplicate-name conflicts in any modified prefab.")


if __name__ == "__main__":
    main()
