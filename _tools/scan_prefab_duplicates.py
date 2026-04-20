"""
Scan Unity UI prefabs for duplicate child GameObject names.
Each Unity prefab is a YAML file. GameObjects have m_Name fields.
We collect every m_Name in each prefab and find duplicates.
"""
import os
import re
import sys
from pathlib import Path
from collections import defaultdict

ROOT = Path(r"C:\stopresent\Unity\RE001\Assets\@Resources\Prefab\UI")

# Match m_Name lines inside GameObject blocks.
# In Unity YAML, GameObject blocks contain "m_Name:" near the top.
# Pattern: lines starting with optional indentation and "m_Name: <value>"
NAME_RE = re.compile(r"^\s*m_Name:\s*(.+)$", re.MULTILINE)
GAMEOBJECT_RE = re.compile(r"^GameObject:\s*$", re.MULTILINE)


def extract_gameobject_names(text: str):
    """
    Walk through YAML doc-by-doc. Each top-level "--- !u!1 &xxxx" block is a GameObject.
    Collect their m_Name values.
    """
    names = []
    # Split on document separators of the form "--- !u!<class>&<id>"
    docs = re.split(r"^--- !u!(\d+) &(\d+).*$", text, flags=re.MULTILINE)
    # docs[0] is a header; then groups of (classId, fileId, body)
    i = 1
    while i + 2 < len(docs):
        class_id = docs[i]
        file_id = docs[i + 1]
        body = docs[i + 2]
        if class_id == "1":  # GameObject class id
            m = NAME_RE.search(body)
            if m:
                names.append((file_id, m.group(1).strip()))
        i += 3
    return names


def scan(root: Path):
    results = {}
    for prefab in sorted(root.rglob("*.prefab")):
        try:
            text = prefab.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            text = prefab.read_text(encoding="utf-8", errors="ignore")
        names = extract_gameobject_names(text)
        counts = defaultdict(list)
        for file_id, name in names:
            counts[name].append(file_id)
        dups = {n: ids for n, ids in counts.items() if len(ids) > 1}
        results[prefab] = {
            "all_names": names,
            "duplicates": dups,
        }
    return results


def main():
    results = scan(ROOT)
    # Print summary of duplicates
    total_dup_prefabs = 0
    total_dup_groups = 0
    for prefab, info in results.items():
        if info["duplicates"]:
            total_dup_prefabs += 1
            total_dup_groups += len(info["duplicates"])
            rel = prefab.relative_to(ROOT)
            print(f"\n=== {rel} ===")
            print(f"  total GameObjects: {len(info['all_names'])}")
            print(f"  duplicate name groups: {len(info['duplicates'])}")
            for name, ids in info["duplicates"].items():
                print(f"    '{name}' x{len(ids)}  (fileIDs: {', '.join(ids)})")
    print(f"\n--- SUMMARY ---")
    print(f"prefabs with duplicates: {total_dup_prefabs}")
    print(f"total duplicate name groups: {total_dup_groups}")


if __name__ == "__main__":
    main()
