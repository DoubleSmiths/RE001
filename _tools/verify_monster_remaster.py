"""교체 PNG 내용, 임포트 설정, 애니메이션 GUID 연결을 오프라인 검사한다.

Unity 실제 임포트·플레이 검증을 대체하지 않는다.
"""
import hashlib
import json
import re
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "Documents/MonsterRemaster"
SPRITES = ROOT / "Assets/@Resources/Sprite/Monster"
CLIPS = ROOT / "Assets/@Resources/Animation/Monster/Archetype"
STATES = {"Idle": "Idle", "Run": "Move", "Attack": "Attack", "Die": "Die", "OnHit": "OnHit"}


def check_image(path, size, bottom=False):
    with Image.open(path) as image:
        assert image.mode == "RGBA", f"Missing alpha: {path}"
        assert image.size == size, f"Wrong size: {path}"
        alpha = image.getchannel("A")
        bounds = alpha.getbbox()
        assert bounds is not None, f"Empty image: {path}"
        # 생성된 투명 원본은 내부 알파가 254인 경우도 있어 유효한 실체와 투명 배경을 확인한다.
        low, high = alpha.getextrema()
        assert low == 0 and high >= 250, f"Bad transparency: {path}"
        if bottom:
            assert bounds[3] == size[1], f"Floating baseline: {path}"
        return hashlib.sha256(image.tobytes()).hexdigest()


def main():
    manifest = json.loads((DOCS / "manifest.json").read_text(encoding="utf-8"))
    plan = json.loads((DOCS / "frame-plan.json").read_text(encoding="utf-8"))
    assert not plan["failures"]
    assert len(manifest["entries"]) == len(plan["entries"]) == 30
    frames = clips = controllers = 0
    for entry in manifest["entries"]:
        folder = SPRITES / "Anim" / entry["id"]
        guid_by_state = {}
        for state, prefix in STATES.items():
            guids, hashes = [], []
            for index in range(4):
                path = folder / f"{prefix}_{index}.png"
                hashes.append(check_image(path, (384, 256), bottom=True))
                meta = path.with_suffix(".png.meta").read_text(encoding="utf-8")
                for setting in ("spriteMode: 1", "spritePixelsToUnits: 64",
                                "spritePivot: {x: 0.5, y: 0}", "filterMode: 0", "enableMipMap: 0"):
                    assert setting in meta, f"Wrong importer {setting}: {path}"
                guids.append(re.search(r"^guid: ([0-9a-f]+)$", meta, re.M)[1])
                frames += 1
            assert len(set(hashes)) > 1, f"Four identical frames: {folder}/{prefix}"
            guid_by_state[state] = guids
            clip = (CLIPS / entry["id"] / f"{state}.anim").read_text(encoding="utf-8")
            keys = re.findall(r"value: \{fileID: (\d+), guid: ([0-9a-f]+), type: 3\}", clip)
            assert keys == [("21300000", guid) for guid in guids], f"Wrong sprite references: {entry['id']}/{state}"
            clips += 1
        controller = (CLIPS / entry["id"] / f"{entry['id']}.controller").read_text(encoding="utf-8")
        names = re.findall(r"^  m_Name: (.+)$", controller, re.M)
        assert all(name in names for name in STATES), f"Missing state: {entry['id']}"
        controllers += 1
    for icon in plan["icons"]:
        check_image(SPRITES / icon["target"], (icon["size"], icon["size"]), icon["bottom"])
    report = {"offline_checks": "passed", "archetypes": controllers, "animation_clips": clips,
              "frames": frames, "frame_size": [384, 256], "empty_frames": 0,
              "icons": len(plan["icons"]), "unity_import": "not_checked_by_offline_tool",
              "unity_play_mode": "not_checked_by_offline_tool"}
    (DOCS / "verification.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
