# -*- coding: utf-8 -*-
# 손편집 JSON 마스터데이터 → .xlsx 시트 생성 (Row0=필드명, Row1=타입, Row2=한글설명, Row3+=데이터)
# 기존 파이프라인(DataTransformer)의 PlayerStatusData 포맷과 동일.
import json, os
from openpyxl import Workbook

JSON_DIR = os.path.join(os.path.dirname(__file__), "..", "Assets", "@Resources", "Data", "JsonData")
XLSX_DIR = os.path.join(os.path.dirname(__file__), "..", "Assets", "@Resources", "Data", "Excel")

def cell(v):
    if v is None: return ""
    if isinstance(v, bool): return "TRUE" if v else "FALSE"
    if isinstance(v, list): return ",".join(str(x) for x in v)   # 리스트는 콤마 구분
    return v

# 각 시트: (출력파일명, JSON파일, 루트키, [(필드, 타입, 한글설명), ...])
SHEETS = [
 ("SkillData","SkillData.json","skills",[
   ("DataId","int","능력 고유 ID"),("Name","string","이름(한)"),("Description","string","설명(한)"),
   ("NameEng","string","이름(영)"),("DescEng","string","설명(영)"),("IconLabel","string","아이콘 라벨"),
   ("Category","enum","분류(Talent 등)"),("SkillActiveType","enum","Active/Passive"),
   ("StatType","string","버프 대상 스탯"),("StatBonus","float","레벨당 버프 수치"),
   ("MaxLevel","int","최대 등급"),("MemoryCapacity","int","기억력 점유(0=재능)"),
   ("Story","string","능력 이야기(한)"),("StoryEng","string","능력 이야기(영)"),
   ("Source","enum","출처(Class/Minion/Equip/Npc/Unlimited)"),
   ("RequiredMinionId","int","요구 미니몬 CreatureId"),("RequiredEquipId","int","요구 장비 DataId"),
   ("RequiredItemId","int","요구 재료 ItemId"),("RequiredItemCount","int","요구 재료 개수"),
   ("NpcLocation","string","학습 NPC 장소"),("RoleTypes","list","종류(Skill,Spell,Def,Util 콤마)"),
   ("TriggerType","enum","발동(None/Probability/Time)"),("Conditions","list","발동조건(콤마)"),
   ("ProcChance","float","확률 발동 %"),("Cooldown","float","시간 발동 쿨(초)"),
   ("ActiveContext","enum","맥락(InBattle/OutOfBattle/Always)"),("ConditionValue","float","조건 파라미터(LowHp%)"),
   ("Effect","enum","효과(Heal/BonusDamage/Buff)"),("EffectStat","string","Buff 대상 스탯"),
   ("EffectValue","float","효과 수치"),("EffectDuration","float","Buff 지속(초,0=전투끝)"),
 ]),
 ("EquipmentData","EquipmentData.json","equipments",[
   ("DataId","int","장비 고유 ID"),("Name","string","이름"),("SlotType","enum","부위(Weapon 등)"),
   ("Grade","enum","등급(Basic 등)"),("SpriteName","string","스프라이트"),("MaxDurability","int","최대 내구도"),
   ("BaseAtk","int","기본 물리공격"),("BaseCritRate","float","기본 치명타율"),("BaseMagicAtk","int","기본 마법공격"),
 ]),
 ("EquipEnhanceData","EquipEnhanceData.json","enhances",[
   ("DataId","int","강화 데이터 ID"),("EquipDataId","int","대상 장비 ID"),("Level","int","강화 레벨"),
   ("AtkBonus","int","물공 보너스"),("CritBonus","float","치명 보너스"),("MagicAtkBonus","int","마공 보너스"),
   ("RequiredMaterialId","int","요구 재료 ID"),("RequiredMaterialCount","int","요구 재료 개수"),
 ]),
 ("MinMonData","MinMonData.json","minMons",[
   ("DataId","int","미니몬 고유 ID"),("Name","string","이름"),("IconLabel","string","아이콘 라벨"),
   ("PrefabLabel","string","프리팹 라벨"),("PetAnimLabel","string","펫 애니 라벨"),("MonsterDataId","int","원본 몬스터 ID"),
   ("BaseAtk","float","기본 공격력"),("BaseAtkSpeed","float","기본 공격속도"),("BaseCritRate","float","기본 치명타율"),
   ("AtkGrade","int","공격 등급"),("AtkSpeedGrade","int","공속 등급"),("CritGrade","int","치명 등급"),
   ("MaxTameCount","int","최대 길들이기 횟수"),("CatchChance","float","포획 확률(0~1)"),
 ]),
 ("HPPotionData","HPPotion.json","potions",[
   ("DataId","int","물약 ID"),("Name","string","이름 로컬키"),("Grade","int","등급"),("Description","string","설명 로컬키"),
   ("SpriteName","string","스프라이트"),("WhereTo","string","획득처 로컬키"),("Step","int","단계"),
   ("FlatRecov","int","고정 회복량"),("MaxHPRecov","int","최대체력 % 회복"),("Cooldown","float","쿨타임(초)"),
   ("NeededItemIDs","list","제작 재료 ID(콤마)"),("NeededItemAmounts","list","제작 재료 수(콤마)"),
 ]),
 ("SpeedPotionData","SpeedPotion.json","potions",[
   ("DataId","int","물약 ID"),("Name","string","이름 로컬키"),("Grade","int","등급"),("Description","string","설명 로컬키"),
   ("SpriteName","string","스프라이트"),("WhereTo","string","획득처 로컬키"),("Step","int","단계"),
   ("AtkSpeedBonus","float","공속 증가(분수,0.2=20%)"),("Duration","float","지속(초)"),("Cooldown","float","쿨타임(초)"),
   ("NeededItemIDs","list","제작 재료 ID(콤마)"),("NeededItemAmounts","list","제작 재료 수(콤마)"),
 ]),
 ("ComboData","ComboData.json","stages",[
   ("Level","int","콤보 단계"),("NameKey","string","이름 로컬키"),("HitsRequired","int","필요 타격수"),
   ("MaxTimer","float","유지 시간(초)"),("HitGain","float","타격당 게이지"),("AtkSpeedBuff","int","공속 버프%"),
   ("DmgBoostBuff","int","피해 증폭%"),("ColorHex","string","색상 HEX"),
 ]),
]

made=[]
for out_name, json_file, root, cols in SHEETS:
    jp = os.path.join(JSON_DIR, json_file)
    with open(jp, "r", encoding="utf-8-sig") as f:
        data = json.load(f)
    entries = data[root]
    wb = Workbook(); ws = wb.active; ws.title = out_name
    ws.append([c[0] for c in cols])   # Row0 필드명
    ws.append([c[1] for c in cols])   # Row1 타입
    ws.append([c[2] for c in cols])   # Row2 한글설명
    for e in entries:                 # Row3+ 데이터
        ws.append([cell(e.get(c[0])) for c in cols])
    op = os.path.join(XLSX_DIR, out_name + ".xlsx")
    wb.save(op)
    made.append(f"{out_name}.xlsx ({len(entries)} rows, {len(cols)} cols)")

print("생성 완료:")
for m in made: print("  -", m)
