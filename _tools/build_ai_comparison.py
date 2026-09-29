"""실제 교체 PNG와 수정 전 PNG를 나란히 확인하는 로컬 HTML을 만든다."""
import html
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "Documents/AIImageAudit"


def main():
    entries = json.loads((DOCS / "repairs.json").read_text(encoding="utf-8"))["entries"]
    cards = []
    for entry in entries:
        label = html.escape(entry["id"])
        old = "before/" + label + ".png"
        new = "../../" + html.escape(entry["target"])
        cards.append(f'<article data-kind="{entry["kind"]}"><h2>{label}</h2><div class="pair">'
                     f'<figure><figcaption>수정 전</figcaption><div class="stage"><img loading="lazy" src="{old}" alt="{label} 수정 전"></div></figure>'
                     f'<figure><figcaption>AI 재생성 후</figcaption><div class="stage"><img loading="lazy" src="{new}" alt="{label} 수정 후"></div></figure></div>'
                     f'<a href="{html.escape(entry["source"])}">생성 원본</a> · <a href="{new}">실제 교체 PNG</a></article>')
    page = '''<!doctype html><html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>RE001 AI 이미지 수정 전후</title><style>
*{box-sizing:border-box}body{margin:0;background:#1b2330;color:#edf3fa;font:15px system-ui}header{position:sticky;top:0;background:#1b2330f5;padding:22px;z-index:2;border-bottom:1px solid #526174}h1{margin:0 0 8px;font-size:24px}p{color:#bac9da}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(460px,1fr));gap:20px;padding:22px}article{border:1px solid #526174;border-radius:8px;padding:16px;min-width:0}h2{font-size:16px;margin:0 0 12px}.pair{display:grid;grid-template-columns:1fr 1fr;gap:10px}figure{margin:0}figcaption{padding:5px;color:#bdcada}.stage{height:210px;background:#e0e5eb;display:flex;align-items:center;justify-content:center;overflow:hidden}img{max-width:100%;max-height:210px;image-rendering:pixelated}article:not([data-kind=opaque]):not([data-kind=layer]) img{width:160px}select{padding:8px;margin-right:14px}a{color:#a0d2ff}article>a{display:inline-block;margin-top:12px}footer{padding:22px}@media(max-width:520px){main{grid-template-columns:1fr;padding:10px}.stage{height:160px}}
</style><header><h1>AI 이미지 63장 · 수정 전후</h1><p>401장 점검 → 장비 18 · 스킬 25 · 재료 9 · 배경 11장 교체. 실제 게임 PNG를 표시합니다.</p>
<label>종류 <select id="kind"><option value="all">전체</option><option value="opaque">불투명 배경</option><option value="cutout">장비·재료</option><option value="medallion">스킬</option><option value="layer">투명 배경 레이어</option></select></label>
<label>배경 <select id="bg"><option value="#e0e5eb">밝게</option><option value="#1b2330">어둡게</option><option value="#638052">초록</option></select></label><a href="index.html">전체 401장 보기</a></header><main>''' + ''.join(cards) + '''</main><footer>기존 PNG 크기와 .meta/GUID를 유지했습니다. 이 비교 화면과 오프라인 검사는 Unity 실제 로드·플레이 검증을 대체하지 않습니다.</footer>
<script>document.querySelector('#kind').onchange=e=>document.querySelectorAll('article').forEach(a=>a.hidden=e.target.value!=='all'&&a.dataset.kind!==e.target.value);document.querySelector('#bg').onchange=e=>document.querySelectorAll('.stage').forEach(s=>s.style.background=e.target.value);</script></html>'''
    (DOCS / "comparison.html").write_text(page, encoding="utf-8")


if __name__ == "__main__":
    main()
