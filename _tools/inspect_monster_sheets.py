"""새 몬스터 시트의 알파 경계를 검사한다. 이미지는 수정하지 않는다."""
import argparse
import ast
import hashlib
from collections import deque
import json
from pathlib import Path
from statistics import median

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "Documents/MonsterRemaster"
STATES = ["Idle", "Move", "Attack", "Die", "OnHit"]
CANVAS = (384, 256)


def icon_plan(entries):
    # 기존 카드 생성기의 등급 색상 규칙을 그대로 읽는다. 생성기를 실행하지 않는다.
    tree = ast.parse((ROOT / "_tools/gen_moncards.py").read_text(encoding="utf-8"))
    grade = next(ast.literal_eval(node.value) for node in tree.body
                 if isinstance(node, ast.Assign)
                 and any(isinstance(t, ast.Name) and t.id == "GRADE" for t in node.targets))
    data_root = ROOT / "Assets/@Resources/Data/JsonData"

    def rows(name):
        return next(iter(json.loads((data_root / name).read_text(encoding="utf-8-sig")).values()))

    creatures = {r["DataId"]: r for r in rows("CreatureData.json")}
    localization = {r["TextKey"]: r["Kor"] for r in rows("LocalizationData.json")}
    known = {e["id"]: e for e in entries}
    icons = []
    for entry in entries:
        icons.append({"id": entry["id"], "target": f"Generated/{entry['id']}.png",
                      "size": 128, "bottom": True, "grade": [1, 1, [1, 1, 1]]})
    for row in rows("MinMonData.json"):
        archetype = row.get("PetAnimLabel", "")
        if archetype not in known:
            continue
        creature = creatures[row["DataId"]]
        name = localization.get(creature["Name"], "")
        modifier = next((key for key in grade if name.startswith(key + " ")), "")
        icons.append({"id": archetype, "target": f"Card/Mon_{row['DataId']}.png",
                      "size": 96, "bottom": False, "grade": grade.get(modifier, [1, 1, [1, 1, 1]])})
    return icons


def intervals(occupied):
    runs = []
    start = None
    for i, value in enumerate([*occupied, False]):
        if value and start is None:
            start = i
        elif not value and start is not None:
            runs.append((start, i))
            start = None
    return runs


def split_axis(counts, expected):
    # 작은 알파 노이즈 대신 실체 사이의 투명한 간격으로 구분한다.
    occupied = [count > 2 for count in counts]
    runs = intervals(occupied)
    if len(runs) < expected:
        raise ValueError(f"Separated groups: expected {expected}, found {len(runs)}")
    gaps = [(runs[i + 1][0] - runs[i][1], runs[i][1], runs[i + 1][0])
            for i in range(len(runs) - 1)]
    largest = sorted(gaps, reverse=True)[:expected - 1]
    if any(length < 3 for length, _, _ in largest):
        raise ValueError("No safe transparent frame boundary")
    cuts = [0, *sorted((a + b) // 2 for _, a, b in largest), len(counts)]
    return list(zip(cuts, cuts[1:]))


def components(mask):
    width, height = mask.size
    pixels = bytearray(mask.tobytes())
    found = []
    for seed in range(len(pixels)):
        if pixels[seed] == 0:
            continue
        pixels[seed] = 0
        queue = deque([seed])
        left = right = seed % width
        top = bottom = seed // width
        area = 0
        while queue:
            current = queue.popleft()
            x, y = current % width, current // width
            left, right = min(left, x), max(right, x)
            top, bottom = min(top, y), max(bottom, y)
            area += 1
            for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
                if 0 <= nx < width and 0 <= ny < height:
                    index = ny * width + nx
                    if pixels[index]:
                        pixels[index] = 0
                        queue.append(index)
        if area >= 400:
            found.append((area, (left, top, right + 1, bottom + 1)))
    return found


def inspect(entry):
    source = DOCS / entry["source"]
    with Image.open(source) as im:
        if im.mode != "RGBA":
            raise ValueError(f"Missing alpha: {source}")
        alpha = im.getchannel("A")
        mask = alpha.point(lambda a: 255 if a >= 128 else 0)
        width, height = im.size
        rows = split_axis([mask.crop((0, y, width, y + 1)).histogram()[255]
                           for y in range(height)], 5)
        frames = []
        for state, (top, bottom) in zip(STATES, rows):
            try:
                cols = split_axis([mask.crop((x, top, x + 1, bottom)).histogram()[255]
                                   for x in range(width)], 4)
                boxes = []
                for left, right in cols:
                    x0, y0, x1, y1 = mask.crop((left, top, right, bottom)).getbbox()
                    boxes.append((left + x0, top + y0, left + x1, top + y1))
            except ValueError:
                parts = components(mask.crop((0, top, width, bottom)))
                if len(parts) != 4:
                    raise ValueError(f"{state}: expected 4 separate silhouettes, found {len(parts)}")
                boxes = [(x0, top + y0, x1, top + y1)
                         for _, (x0, y0, x1, y1) in sorted(parts, key=lambda p: p[1][0])]
                # 사각형이 겹치면 옆 캐릭터 조각이 섞이므로 재생성이 필요하다.
                for a, b in zip(boxes, boxes[1:]):
                    if a[2] > b[0] and min(a[3], b[3]) > max(a[1], b[1]):
                        raise ValueError(f"{state}: overlapping sprite bounds")
            for index, (x0, y0, x1, y1) in enumerate(boxes):
                rect = [x0, y0, x1 - x0, y1 - y0]
                frames.append({"state": state, "index": index, "rect": rect})
        # 죽거나 웅크린 자세를 프레임별로 확대하지 않는다. 모든 동작이 한 배율을 공유한다.
        idle_height = median(f["rect"][3] for f in frames[:4])
        scale = min(128 / idle_height,
                    (CANVAS[0] - 8) / max(f["rect"][2] for f in frames),
                    (CANVAS[1] - 8) / max(f["rect"][3] for f in frames))
        for frame in frames:
            frame["size"] = [max(1, round(n * scale)) for n in frame["rect"][2:]]
        return {"id": entry["id"], "source": entry["source"],
                "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
                "source_size": [width, height], "scale": scale, "frames": frames}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--only")
    args = parser.parse_args()
    manifest = json.loads((DOCS / "manifest.json").read_text(encoding="utf-8"))
    entries = [e for e in manifest["entries"] if not args.only or e["id"] == args.only]
    result = {"canvas": CANVAS, "entries": [], "failures": []}
    for entry in entries:
        try:
            item = inspect(entry)
            result["entries"].append(item)
            print(f"OK {entry['id']}: 5 x 4, scale={item['scale']:.4f}")
        except ValueError as exc:
            result["failures"].append({"id": entry["id"], "error": str(exc)})
            print(f"FAIL {entry['id']}: {exc}")
    result["icons"] = icon_plan(result["entries"])
    (DOCS / "frame-plan.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    if result["failures"]:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
