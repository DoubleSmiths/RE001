"""재생성 에셋의 PNG 내용·크기·GUID 보존을 검사한다. Unity 플레이 검증과는 별도다."""
import hashlib
import json
from collections import Counter
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "Documents/AIImageAudit"


def main():
    plan = json.loads((DOCS / "export-plan.json").read_text(encoding="utf-8"))["entries"]
    assert len(plan) == len({e["target"] for e in plan}) == 63
    for entry in plan:
        path = ROOT / entry["target"]
        before = DOCS / "before" / (entry["id"] + ".png")
        meta = path.with_suffix(".png.meta")
        # 기록된 해시는 CRLF 체크아웃에서 쟀다 — 줄바꿈을 CRLF 로 맞춰 비교해야 LF 체크아웃에서도 같은 판정이 난다
        meta_crlf = meta.read_bytes().replace(b"\r\n", b"\n").replace(b"\n", b"\r\n")
        assert hashlib.sha256(meta_crlf).hexdigest() == entry["meta_sha256"], f"Importer changed: {path}"
        with Image.open(path) as image, Image.open(before) as old:
            assert image.size == old.size == tuple(entry["size"]), f"Dimensions changed: {path}"
            assert image.mode == "RGBA", f"Missing alpha channel: {path}"
            alpha = image.getchannel("A")
            assert alpha.getbbox() is not None, f"Empty: {path}"
            if entry["kind"] == "opaque":
                assert alpha.getextrema() == (255, 255), f"Holes in scene: {path}"
            else:
                low, high = alpha.getextrema()
                assert low == 0 and high >= 250, f"Invalid transparent cutout: {path}"
                if entry["kind"] == "medallion":
                    # 원래 메달 안쪽까지 제거되던 결함을 직접 검사한다.
                    assert alpha.crop((16, 16, 48, 48)).getextrema()[0] >= 240, f"Transparent medal interior: {path}"
            assert image.tobytes() != old.convert("RGBA").tobytes(), f"Not replaced: {path}"
    inventory = json.loads((DOCS / "inventory.json").read_text(encoding="utf-8"))
    damaged_scenes = [e["name"] for e in inventory["entries"]
                      if (e["group"] in ("region", "village", "interior") or e["name"].startswith("RegionL_Far_"))
                      and e["transparent_pixels"] > 0]
    assert not damaged_scenes, f"Scene alpha defects remain: {damaged_scenes}"
    report = {"offline_status": "passed", "inspected_images": inventory["total"],
              "replaced_images": len(plan), "kinds": dict(Counter(e["kind"] for e in plan)),
              "changed_importer_files": 0, "empty_replacements": 0, "remaining_transparent_full_scenes": 0,
              "unity_import_and_play": "not_checked_editor_unavailable"}
    (DOCS / "verification.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
