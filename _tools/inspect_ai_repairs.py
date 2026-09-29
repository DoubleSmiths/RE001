"""AI 수정 결과를 읽어 기존 PNG 규격으로 내보낼 계획을 작성한다. 이미지 편집은 하지 않는다."""
import hashlib
import json
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "Documents/AIImageAudit"


def main():
    manifest = json.loads((DOCS / "repairs.json").read_text(encoding="utf-8"))
    entries = []
    for item in manifest["entries"]:
        source = DOCS / item["source"]
        target = ROOT / item["target"]
        with Image.open(target) as existing:
            size = existing.size
        with Image.open(source) as raw:
            image = raw.convert("RGBA")
            alpha = image.getchannel("A")
            if item["kind"] == "opaque":
                assert alpha.getextrema() == (255, 255), f"Background still transparent: {item['id']}"
                rect = [0, 0, image.width, image.height]
                dest = [0, 0, *size]
            else:
                bounds = alpha.point(lambda a: 255 if a >= 8 else 0).getbbox()
                assert bounds is not None and alpha.getextrema()[0] == 0, f"Invalid cutout: {item['id']}"
                x0, y0, x1, y1 = bounds
                rect = [x0, y0, x1 - x0, y1 - y0]
                if item["kind"] == "layer":
                    # 배경 레이어의 게임 내 폭과 높이를 보존한다.
                    dest = [0, 0, *size]
                else:
                    scale = min((size[0] - 4) / rect[2], (size[1] - 4) / rect[3])
                    width, height = max(1, round(rect[2] * scale)), max(1, round(rect[3] * scale))
                    dest = [(size[0] - width) // 2, (size[1] - height) // 2, width, height]
            entries.append({**item, "size": size, "rect": rect, "dest": dest,
                            "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
                            "meta_sha256": hashlib.sha256(target.with_suffix('.png.meta').read_bytes()).hexdigest()})
    (DOCS / "export-plan.json").write_text(json.dumps({"entries": entries}, indent=2) + "\n", encoding="utf-8")
    print(f"Ready: {len(entries)} generated images")


if __name__ == "__main__":
    main()
