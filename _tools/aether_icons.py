# -*- coding: utf-8 -*-
"""AetherForge API로 장비/스킬 아이콘을 생성하고 게임용 스프라이트로 후처리한다.

API 계약 (실측으로 확인 — 공식 문서의 JSON 예시는 실제와 다르다)
  POST https://api.aetherforgeai.com/public/v1/generate/image
       Authorization: Bearer <key>
       **form-encoded** (application/json 으로 보내면 422) : prompt, ai_model
       → {"job_id": "..."}
  GET  https://api.aetherforgeai.com/public/v1/job/{job_id}
       → {"job_id","status":"Pending"|"Succeed","image_urls":[...]}

후처리
  생성물은 1024x1024에 **불투명 회색 배경**으로 온다(프롬프트로 투명 요청해도 동일).
  → 네 모서리 색을 배경으로 보고 유사색을 알파 0 처리 → 여백 크롭 → 64x64 NEAREST 축소
    (픽셀아트라 NEAREST가 아니면 뭉개진다)

키는 .env.local 의 AETHER_API_KEY 에서만 읽는다. 인자로 넘기지 않는다(히스토리 노출 방지).

사용:
  python _tools/aether_icons.py equipment      # 장비 60종
  python _tools/aether_icons.py skill          # 스킬 아이콘
  python _tools/aether_icons.py equipment --limit 5   # 일부만 (비용 확인용)
이미 파일이 있으면 건너뛴다(중단 후 재실행 가능).
"""
import io
import json
import os
import sys
import time
import urllib.parse
import urllib.request

from PIL import Image

BASE = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
API = "https://api.aetherforgeai.com/public/v1"
MODEL = "GPT Image 1.5"
# 기본 urllib UA는 403으로 막힌다(curl은 통과) — 브라우저 UA로 위장
UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0 Safari/537.36"
ICON_SIZE = 64
SUBMIT_GAP = 3.0   # 제출 간격(초). 0.4로 몰아넣었다가 100장 넘기며 429를 맞았다
OUT_EQUIP = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Icon", "Equipment", "Generated")
OUT_SKILL = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Icon", "Skill", "Generated")
OUT_MONSTER = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Monster", "Generated")
OUT_MATERIAL = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Icon", "Material", "Generated")

# 전 아이콘 공통 스타일 — 한 세트로 보이게 하는 핵심. 바꾸면 기존 아이콘과 톤이 어긋난다.
STYLE = ("16-bit pixel art game inventory icon, single centered object, "
         "chunky readable pixels, bold dark outline, soft top-left light, "
         "flat plain background, no text, no border frame, no shadow on ground")


def api_key():
    path = os.path.join(BASE, ".env.local")
    with io.open(path, encoding="utf-8") as f:
        for line in f:
            if line.startswith("AETHER_API_KEY="):
                return line.split("=", 1)[1].strip()
    raise SystemExit(".env.local 에 AETHER_API_KEY 가 없습니다")


def post(path, fields, key):
    data = urllib.parse.urlencode(fields).encode()
    req = urllib.request.Request(API + path, data=data,
                                 headers={"Authorization": "Bearer " + key, "User-Agent": UA})
    with urllib.request.urlopen(req, timeout=120) as r:
        return json.loads(r.read().decode())


def get(path, key):
    req = urllib.request.Request(API + path, headers={"Authorization": "Bearer " + key, "User-Agent": UA})
    with urllib.request.urlopen(req, timeout=120) as r:
        return json.loads(r.read().decode())


def postprocess(raw_bytes, out_path, size=None):
    """회색 배경 제거 → 크롭 → 64x64 축소."""
    im = Image.open(io.BytesIO(raw_bytes)).convert("RGBA")
    px = im.load()
    w, h = im.size

    # 네 모서리 평균색을 배경색으로 추정
    corners = [px[0, 0], px[w - 1, 0], px[0, h - 1], px[w - 1, h - 1]]
    bg = tuple(sum(c[i] for c in corners) // 4 for i in range(3))

    tol = 42
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if abs(r - bg[0]) <= tol and abs(g - bg[1]) <= tol and abs(b - bg[2]) <= tol:
                px[x, y] = (r, g, b, 0)

    bbox = im.getbbox()
    if bbox:
        im = im.crop(bbox)

    # 정사각 캔버스에 중앙 배치 후 축소 (비율 유지)
    side = max(im.size)
    canvas = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    canvas.paste(im, ((side - im.size[0]) // 2, (side - im.size[1]) // 2))
    px_size = size or ICON_SIZE
    canvas = canvas.resize((px_size, px_size), Image.NEAREST)

    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    canvas.save(out_path)


def run(items, out_dir, limit=None, size=None):
    """items: [(파일명, 프롬프트본문)] — 파일이 이미 있으면 건너뜀."""
    key = api_key()
    todo = [(n, p) for n, p in items
            if not os.path.exists(os.path.join(out_dir, n + ".png"))]
    if limit:
        todo = todo[:limit]
    if not todo:
        print("생성할 항목 없음 (전부 존재)")
        return

    print(f"생성 대상 {len(todo)}개 / 전체 {len(items)}개")
    jobs = []
    for name, prompt in todo:
        # 429(Too Many Requests)는 잠시 쉬면 풀린다. 지수 백오프로 최대 4회 재시도.
        delay = SUBMIT_GAP
        for attempt in range(4):
            try:
                r = post("/generate/image", {"prompt": f"{prompt}. {STYLE}", "ai_model": MODEL}, key)
                jobs.append((name, r["job_id"]))
                print(f"  제출 {name} → {r['job_id'][:8]}")
                break
            except Exception as e:
                if "429" in str(e) and attempt < 3:
                    wait = 20 * (attempt + 1)
                    print(f"  …429, {wait}s 대기 후 재시도 ({name})")
                    time.sleep(wait)
                    continue
                print(f"  !제출실패 {name}: {e}")
                break
        time.sleep(delay)        # 연속 호출 간격 — 좁으면 429가 난다

    done, failed = 0, []
    pending = list(jobs)
    for _ in range(80):          # 최대 ~13분 폴링
        if not pending:
            break
        time.sleep(10)
        still = []
        for name, jid in pending:
            try:
                r = get(f"/job/{jid}", key)
            except Exception:
                still.append((name, jid))
                continue
            st = r.get("status", "")
            if st in ("Succeed", "Completed", "Success"):
                urls = r.get("image_urls") or []
                if not urls:
                    failed.append(name)
                    continue
                dl = urllib.request.Request(urls[0], headers={"User-Agent": UA})
                with urllib.request.urlopen(dl, timeout=120) as resp:
                    postprocess(resp.read(), os.path.join(out_dir, name + ".png"), size)
                done += 1
                print(f"  완료 {name} ({done}/{len(jobs)})")
            elif st in ("Failed", "Error"):
                failed.append(name)
            else:
                still.append((name, jid))
        pending = still

    print(f"\n완료 {done}개, 실패 {len(failed)}개, 미완 {len(pending)}개")
    if failed:
        print("  실패:", ", ".join(failed[:20]))


# ── 대상 목록 ────────────────────────────────────────────────────────────────
TIER_LOOK = ["worn crude wooden", "polished bronze", "tempered steel", "engraved silver",
             "glowing blue mithril", "red dragonscale", "golden sacred", "dark abyssal purple",
             "radiant dawn white-gold", "mythic prismatic"]
SLOT_LOOK = [("Weapon", "sword"), ("Helmet", "helmet"), ("Armor", "chestplate"),
             ("Gloves", "gauntlets"), ("Legs", "greaves"), ("Accessory", "amulet")]


def equipment_items():
    out = []
    for t in range(1, 11):
        for si, (slot, obj) in enumerate(SLOT_LOOK):
            did = 30001 + (t - 1) * len(SLOT_LOOK) + si
            out.append((f"Equip{did}", f"a {TIER_LOOK[t - 1]} fantasy RPG {obj}"))
    return out


# 능력은 StatType/Effect 조합으로 형상을 나눈다. 같은 프롬프트를 쓰면 아이콘이 전부 똑같아진다.
STAT_LOOK = {
    "PhysicalAtk":           "a burning greatsword",
    "MagicAtk":              "a swirling arcane orb",
    "CritRate":              "a pierced target with a dagger",
    "AtkSpeed":              "a winged blade in motion",
    "Accuracy":              "a glowing archer's eye sigil",
    "MaxHp":                 "a radiant heart shield",
    "Def":                   "a fortified tower shield",
    "Evasion":               "a swirling wind cloak",
    "ExpGain":               "an open glowing tome with rising sparks",
    "GuaranteedCatchReduce": "a glowing capture chain and ring",
}
EFFECT_LOOK = {
    "Heal": "surrounded by green healing motes",
    "Buff": "surrounded by rising golden runes",
    "BonusDamage": "cracking with red impact sparks",
}


def skill_items():
    """아이콘이 없는(IconLabel 공백) 능력만 생성 대상. 파일명 = Skill{DataId}."""
    path = os.path.join(BASE, "Assets", "@Resources", "Data", "JsonData", "SkillData.json")
    with io.open(path, encoding="utf-8-sig") as f:
        skills = json.load(f)["skills"]
    out = []
    for s in skills:
        label = (s.get("IconLabel") or "").strip()
        # 생성 대상 = 아이콘이 아예 없거나, IconLabel이 Skill{id}인데 파일이 아직 없는 경우.
        # (IconLabel을 미리 채워둔 신규 능력도 잡히도록 파일 존재 여부로 판단한다)
        if label and not label.startswith("Skill"):
            continue
        name = f"Skill{s['DataId']}"
        if os.path.exists(os.path.join(OUT_SKILL, name + ".png")):
            continue
        shape = STAT_LOOK.get(s.get("StatType", ""), None)
        if shape is None:
            eff = s.get("Effect", "")
            shape = {"Heal": "a radiant healing chalice",
                     "Buff": "a rising banner of power",
                     "BonusDamage": "a shattering impact rune"}.get(eff, "an ornate magical sigil")
        extra = EFFECT_LOOK.get(s.get("Effect", ""), "")
        prompt = f"{shape} {extra}, framed in a circular medallion, fantasy RPG skill icon".replace("  ", " ")
        out.append((name, prompt))
    return out


# 지역별 몬스터 원형 30종. 500종 각각을 생성하는 대신 원형 + 티어 색변주로 구분한다
# (색 변주는 애니메이션 클립의 SpriteRenderer 색 커브로 준다 — 텍스처를 늘리지 않는다).
MONSTER_ARCHETYPES = [
    ("BreezingWindForest", ["slime blob", "forest spider", "wild wolf", "toadstool mushroom creature",
                            "wind sprite", "wild boar", "black crow", "animated vine creeper",
                            "red fox", "giant hornet"]),
    ("WhisperingCave", ["cave bat", "giant mole", "miner wraith ghost", "crystal golem",
                        "giant centipede", "moss horror", "stone sentry statue", "shadow figure",
                        "cave salamander", "glowing worm"]),
    ("RuinedSanctum", ["skeletal revenant", "stone knight", "fallen priest", "chimera beast",
                       "gargoyle", "armored warden", "ghostly phantasm", "tainted spirit",
                       "altar guardian construct", "void creature"]),
]


def monster_items():
    out = []
    for region, bases in MONSTER_ARCHETYPES:
        for i, b in enumerate(bases):
            out.append((f"Mon_{region}_{i}",
                        f"a fantasy RPG monster, {b}, full body side view facing right, standing pose"))
    return out


# 몬스터 원형 30종에 1:1 대응하는 재료 아이콘 (gen_world.py의 MATERIALS와 순서 동일)
MATERIAL_LOOK = [
    "a translucent green slime jelly blob", "a coil of spider silk thread", "a curved wolf fang",
    "a red toadstool mushroom cap", "a swirling wind essence orb", "a rolled boar hide",
    "a black crow feather", "a coil of tough vine", "a tuft of orange fox fur",
    "a barbed hornet stinger",
    "a leathery bat wing membrane", "a thick mole claw", "an old miner's lantern relic",
    "a glowing crystal shard", "a segmented centipede shell plate", "a clump of damp glowing moss",
    "a broken stone statue fragment", "a wisp of condensed shadow", "an iridescent salamander scale",
    "a jar of luminous glowing slime",
    "a pile of grey revenant ash", "an engraved knight's crest badge", "a tainted golden relic",
    "a tuft of chimera mane hair", "a chipped gargoyle stone piece", "a glowing warden core orb",
    "a translucent phantom sliver", "a corrupted purple essence vial", "a carved altar fragment",
    "a dark void crystal",
]
FAMILIES = ["BreezingWindForest", "WhisperingCave", "RuinedSanctum"]


def material_items():
    out = []
    for i, look in enumerate(MATERIAL_LOOK):
        fam = FAMILIES[i // 10]
        out.append((f"Mat_{fam}_{i % 10}", f"{look}, single crafting material item"))
    return out


# v2 유물 30종(ADR0035) — _tools/gen_relics.py 의 RELICS 와 같은 순서·같은 DataId. 파일명 = Relic{DataId}
OUT_RELIC = os.path.join(BASE, "Assets", "@Resources", "Sprite", "Icon", "Relic", "Generated")
RELIC_LOOK = [
    (8001, "a rusty old sword hilt relic"), (8002, "a wolf fang charm on a leather cord"),
    (8003, "an engraved bronze warrior crest"), (8004, "a runestone glowing with red flame runes"),
    (8005, "an ornate golden gauntlet set with a ruby"), (8006, "a radiant golden sun crown with light rays"),
    (8007, "a smooth mossy green pebble"), (8008, "a small glass jar of red berries"),
    (8009, "a glowing green acorn"), (8010, "a large crystallized red heart"),
    (8011, "a luminous golden-green world tree leaf"),
    (8012, "a broken piece of a wooden shield"), (8013, "a small turtle shell"),
    (8014, "an iron shoulder pauldron"), (8015, "a silver chalice with a blue gem"),
    (8016, "a sharp chipped flint stone"), (8017, "a hawk eye amber pendant"),
    (8018, "a pair of vampire fangs on a silver chain"), (8019, "a cursed black and violet dagger with a glowing edge"),
    (8020, "an old copper coin with a square hole"), (8021, "a small leather coin purse"),
    (8022, "a golden lucky horseshoe"), (8023, "a golden toad statue holding a coin"),
    (8024, "an old worn leather journal"), (8025, "round brass sage spectacles"),
    (8026, "an ornate hourglass with glowing blue sand"), (8027, "an unrolled scroll covered in glowing star charts"),
    (8028, "a torn burlap loot sack"), (8029, "a brass grappling hook"),
    (8030, "a small clay honey pot dripping golden honey"),
]


def relic_items():
    return [(f"Relic{did}", f"{look}, a mysterious ancient relic treasure") for did, look in RELIC_LOOK]


def main():
    what = sys.argv[1] if len(sys.argv) > 1 else "equipment"
    limit = None
    if "--limit" in sys.argv:
        limit = int(sys.argv[sys.argv.index("--limit") + 1])
    if what == "equipment":
        run(equipment_items(), OUT_EQUIP, limit)
    elif what == "skill":
        run(skill_items(), OUT_SKILL, limit)
    elif what == "material":
        run(material_items(), OUT_MATERIAL, limit)
    elif what == "monster":
        run(monster_items(), OUT_MONSTER, limit, size=128)  # 인게임 캐릭터라 아이콘보다 크게
    elif what == "relic":
        run(relic_items(), OUT_RELIC, limit)
    else:
        raise SystemExit("사용법: python _tools/aether_icons.py [equipment|skill|monster|material|relic] [--limit N]")


if __name__ == "__main__":
    main()
