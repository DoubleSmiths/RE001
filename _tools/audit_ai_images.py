"""몬스터 외 생성 이미지의 내용과 알파를 읽어 목록과 검토 화면을 만든다. PNG는 변경하지 않는다."""
import html
import json
from collections import Counter
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SPRITES = ROOT / "Assets/@Resources/Sprite"
OUT = ROOT / "Documents/AIImageAudit"
GROUPS = {
    "equipment": ("장비", "Icon/Equipment/Generated", "*.png"),
    "skill": ("스킬", "Icon/Skill/Generated", "*.png"),
    "material": ("재료", "Icon/Material/Generated", "*.png"),
    "region": ("지역 미리보기", "Map/Region", "*.png"),
    "layers": ("전투 배경 레이어", "Background/RegionLayer", "*.png"),
    "village": ("마을 배경", "Background/VillageBG", "*.png"),
    "interior": ("건물 내부", "Background/Town", "Background_*.png"),
    "npc": ("NPC·메뉴", "", ""),
}
EXPLICIT = ["NPC/Priestess_Standing_Idle.png", "NPC/Shopkeeper_Standing_Idle.png",
            "UI/Image/NPC/priestess/priestess_profile.png", "UI/Image/NPC/shopkeeper/shopkeeper_profile.png",
            "Icon/Menu Icon/Forge_Icon.png", "Icon/Menu Icon/Shop_Icon.png"]


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    entries = []
    for group, (label, folder, pattern) in GROUPS.items():
        files = [SPRITES / p for p in EXPLICIT] if group == "npc" else sorted((SPRITES / folder).glob(pattern))
        if group == "interior":
            files = [p for p in files if p.stem in ("Background_Forge", "Background_Shop")]
        for path in files:
            with Image.open(path) as original:
                image = original.convert("RGBA")
                pixels = list(image.get_flattened_data())
                visible = sum(a > 16 for _, _, _, a in pixels)
                magenta = sum(a > 100 and r > 120 and b > 100 and g < min(r, b) * .65 for r, g, b, a in pixels)
                entries.append({"group": group, "label": label, "path": path.relative_to(ROOT).as_posix(),
                                "name": path.stem, "size": list(image.size), "visible_pixels": visible,
                                "visible_magenta_pixels": magenta, "transparent_pixels": sum(a == 0 for *_, a in pixels),
                                "empty": visible == 0})
    result = {"counts": dict(Counter(e["label"] for e in entries)), "total": len(entries), "entries": entries,
              "note": "자홍색은 의도된 보라 장비·마법색일 수 있다. 수치만으로 결함 판정하지 않는다."}
    (OUT / "inventory.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    cards = []
    for entry in entries:
        path = "../../" + entry["path"]
        cards.append(f'<article data-group="{entry["group"]}"><b>{html.escape(entry["name"])}</b>'
                     f'<div class="stage"><img src="{html.escape(path)}" alt="{html.escape(entry["name"])}"></div>'
                     f'<small>{entry["label"]} · {entry["size"][0]}×{entry["size"][1]} · 자홍 후보 {entry["visible_magenta_pixels"]}px</small></article>')
    options = ''.join(f'<option value="{key}">{label}</option>' for key, (label, _, _) in GROUPS.items())
    page = '''<!doctype html><html lang="ko"><meta charset="utf-8"><title>AI 이미지 전체 점검</title>
<style>body{font:14px system-ui;background:#202733;color:#edf2f6;margin:24px}header{position:sticky;top:0;background:#202733;padding:12px;z-index:1}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(260px,1fr));gap:14px}article{padding:12px;border:1px solid #718092}b,small{display:block}.stage{height:190px;display:flex;align-items:center;justify-content:center;background:#e0e5eb;margin:10px 0}img{max-width:100%;max-height:190px;image-rendering:pixelated}select{padding:8px}</style>
<header><h1>몬스터 외 AI 이미지 점검</h1><p>실제 PNG 알파를 합성해 표시합니다. 투명 영역의 RGB와 실제 보이는 오염을 구분하세요.</p>
<select id="group"><option value="all">전체</option>''' + options + '''</select>
<select id="bg"><option value="#e0e5eb">밝은 배경</option><option value="#202733">어두운 배경</option><option value="#67815a">초록 배경</option></select></header><main>''' + ''.join(cards) + '''</main>
<script>document.querySelector('#group').onchange=e=>document.querySelectorAll('article').forEach(a=>a.hidden=e.target.value!=='all'&&a.dataset.group!==e.target.value);document.querySelector('#bg').onchange=e=>document.querySelectorAll('.stage').forEach(s=>s.style.background=e.target.value);</script></html>'''
    (OUT / "index.html").write_text(page, encoding="utf-8")
    print(json.dumps({"total": result["total"], "counts": result["counts"], "empty": sum(e["empty"] for e in entries)}, ensure_ascii=False))


if __name__ == "__main__":
    main()
