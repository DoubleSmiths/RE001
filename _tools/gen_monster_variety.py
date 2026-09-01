# -*- coding: utf-8 -*-
"""몬스터 503종에 개성을 넣는다 — 전투 수치 / 고유기술 / 이름 중복 해소.

배경
    503종이 있지만 전투에서 서로 구별되지 않았다. 실측하면:
      - MonsterAttackDelay 가 503종 전부 1.5초 → 모든 적이 같은 리듬으로 때린다
      - CritRate / Evasion / PhysicalResist / DmgAmp 가 전부 0 → BattleMath 가 읽는
        수치가 죄다 꺼져 있어, 차이가 HP·물공·방어 크기뿐이었다
      - UniqueSkillName 이 503종 전부 문자열 "0" → 빈 값이 아니라서 폴백이 안 걸리고
        도감에 고유기술이 "0" 으로 표시됐다
      - 표시 이름 400종이 중복 (수식어 10종 × 기본종이라 단마다 2종씩 겹친다)

    ⚠️ MoveSpeed 와 MagicResist 는 **일부러 건드리지 않는다.**
       MoveSpeed 는 대입만 되고 아무 데서도 읽지 않는 죽은 필드다. 몬스터 접근 속도는
       MonsterController.GROUND_SCROLL_SPEED(=5) 상수이고, 슬58 스펙상 지면 스크롤과
       같은 속도여야 한다 — 개체마다 다르게 하면 지면과 어긋나 보인다.
       MagicResist 는 게임에 마법 공격 경로가 없어서(플레이어·몬스터·미니몬 모두
       AttackType.Physical) 채워도 영영 읽히지 않는다.

무엇을 하는가
    ① 기본종 31개에 역할(완충/중장/표준/민첩/돌격)을 주고, 그 역할에서
       공격대기·회피·치명타·물리저항·피해증폭을 정한다. 희귀도와 난이도 단계로 가감.
    ② 기본종마다 고유기술 이름/설명을 만들어 로컬라이즈 키로 넣는다.
    ③ 난이도 사다리를 10단 → 20단으로 늘려 이름 중복을 없앤다.
       10종짜리 기본종은 짝수 칸만 쓰므로 **기존 이름이 그대로 유지된다.**

실행
    python _tools/gen_monster_variety.py          # 미리보기(쓰지 않음)
    python _tools/gen_monster_variety.py --write  # xlsx 에 반영

    반영 후 Unity 에서 `Tools/ParseAllLocal (로컬 전체 파싱)` 을 돌려야 JSON 에 들어간다.
    게임은 JSON 만 읽는다.
"""
import sys
import os
import collections

import openpyxl

EXCEL = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..',
                     'Assets', '@Resources', 'Data', 'Excel')
CREATURE = os.path.join(EXCEL, 'CreatureData.xlsx')
LOCALIZE = os.path.join(EXCEL, 'LocalizationData.xlsx')

# 기존 수식어 10종 — 이걸로 "수식어 + 기본종" 을 분해한다.
OLD_MODS = ['어린', '거친', '굶주린', '성난', '사나운',
            '오래된', '타락한', '심연의', '왕관을 쓴', '전설의']

# 20단 난이도 사다리. **짝수 칸이 기존 10종과 정확히 일치**하도록 짜여 있다 —
#  10종짜리 기본종은 짝수 칸만 쓰므로 이름이 하나도 바뀌지 않는다.
LADDER = [
    ('어린',       'Young'),
    ('설익은',     'Callow'),
    ('거친',       'Feral'),
    ('억센',       'Sturdy'),
    ('굶주린',     'Starving'),
    ('표독한',     'Vicious'),
    ('성난',       'Enraged'),
    ('흉포한',     'Ferocious'),
    ('사나운',     'Savage'),
    ('무자비한',   'Merciless'),
    ('오래된',     'Ancient'),
    ('풍화된',     'Weathered'),
    ('타락한',     'Tainted'),
    ('저주받은',   'Cursed'),
    ('심연의',     'Abyssal'),
    ('공허의',     'Voidtouched'),
    ('왕관을 쓴',  'Crowned'),
    ('불멸의',     'Undying'),
    ('전설의',     'Legendary'),
    ('태초의',     'Primordial'),
]

# 역할 — BattleMath 가 실제로 읽는 값만 담는다.
#   (공격대기초, 회피%, 치명%, 물리저항%, 피해증폭%)
ROLES = {
    '완충': (2.30,  0.0,  0.0, 26.0,  0.0),   # 느리지만 단단하다
    '중장': (1.90,  0.0,  8.0, 15.0, 12.0),   # 느리고 무겁게 때린다
    '표준': (1.50,  6.0,  4.0,  8.0,  4.0),   # 기준
    '민첩': (1.15, 15.0,  6.0,  4.0,  0.0),   # 잘 피한다
    '돌격': (0.95,  3.0, 12.0,  0.0, 10.0),   # 빠르게 자주 때린다
}
# 기본종 → 역할.
#
#  ⚠️ 처음엔 물공/HP 비율로 5분위를 갈라 자동 배정했는데 **못 쓴다.** 원본 데이터가
#     수식으로 생성돼서 30종의 비율이 0.1296~0.1443 으로 사실상 같다 — 소수점 4자리
#     잡음으로 분위가 갈려, 말벌이 2.3초 대기하는 완충형이 되고 까마귀가 맷집형이 됐다.
#     데이터에 없는 개성은 데이터에서 유도할 수 없다. 허구에 맞춰 손으로 정한다.
#     (각 역할은 그 기본종의 고유기술과 맞물린다 — 수정 갑피=완충, 그림자 잠행=민첩,
#      연속 침=돌격.)
ROLE_BY_BASE = {
    # 완충 — 자리를 지키고 버틴다
    '독버섯': '완충', '덩굴손': '완충', '수정골렘': '완충',
    '이끼괴물': '완충', '석상병': '완충', '제단수호물': '완충',
    # 중장 — 느리고 무겁게 때린다
    '멧돼지': '중장', '광부망령': '중장', '석상기사': '중장',
    '수호자': '중장', '공허체': '중장',
    # 표준 — 기준점. 초반 슬라임이 여기 있어야 첫 전투가 순하다
    '슬라임': '표준', '두더지': '표준', '도롱뇽': '표준',
    '광충': '표준', '타락사제': '표준', '타락정령': '표준',
    # 민첩 — 잘 피한다
    '숲거미': '민첩', '바람정령': '민첩', '까마귀': '민첩', '여우': '민첩',
    '동굴박쥐': '민첩', '그림자': '민첩', '망령': '민첩', '환영': '민첩',
    # 돌격 — 빠르게 자주 때린다
    '들늑대': '돌격', '말벌': '돌격', '지네': '돌격',
    '키메라': '돌격', '가고일': '돌격', '고블린': '돌격',
}

# 상한 — 플레이어 명중이 90 고정이라 회피가 높으면 빗나감이 답답해진다.
CAP = {'evasion': 18.0, 'crit': 25.0, 'resist': 40.0, 'amp': 30.0}

# 기본종 → (고유기술 한글, 영문, 설명 한글, 설명 영문)
SKILLS = {
    '슬라임':     ('점액 분출',      'Ooze Burst',       '끈적한 점액을 뿜어 발을 묶는다.',            'Spews clinging ooze that fouls your footing.'),
    '숲거미':     ('거미줄 결박',    'Web Snare',        '거미줄을 쏘아 움직임을 옭아맨다.',            'Fires webbing that binds movement.'),
    '들늑대':     ('무리 사냥',      'Pack Hunt',        '동료가 가까울수록 사납게 문다.',              'Bites harder while its pack is near.'),
    '독버섯':     ('포자 살포',      'Spore Cloud',      '독 포자를 흩뿌려 서서히 갉아먹는다.',          'Scatters toxic spores that gnaw slowly.'),
    '바람정령':   ('질풍 베기',      'Galecut',          '바람에 날을 세워 먼 곳까지 벤다.',            'Hones the wind to a blade and cuts at range.'),
    '멧돼지':     ('저돌 돌진',      'Reckless Charge',  '머리를 숙이고 곧장 들이받는다.',              'Lowers its head and barrels straight in.'),
    '까마귀':     ('눈 쪼기',        'Eye Peck',         '눈을 노려 쪼아 조준을 흐린다.',               'Pecks at the eyes to spoil your aim.'),
    '덩굴손':     ('덩굴 옭아매기',  'Vine Grasp',       '땅에서 덩굴을 뻗어 발목을 감는다.',            'Sends vines up to coil around your ankles.'),
    '여우':       ('홀리는 걸음',    'Beguiling Step',   '종잡을 수 없는 걸음으로 공격을 흘린다.',        'Slips attacks with an unreadable gait.'),
    '말벌':       ('연속 침',        'Stinger Flurry',   '짧은 간격으로 거듭 찌른다.',                  'Stabs again and again in quick succession.'),
    '동굴박쥐':   ('초음파 교란',    'Sonic Jolt',       '음파를 쏘아 감각을 어지럽힌다.',              'Looses a shriek that scrambles the senses.'),
    '두더지':     ('굴파기 기습',    'Burrow Ambush',    '땅을 파고들어 발밑에서 솟는다.',              'Tunnels under and erupts beneath you.'),
    '광부망령':   ('곡괭이 원한',    'Grudge Pick',      '생전의 곡괭이를 원한 실어 내리친다.',          'Swings its old pick, heavy with grudge.'),
    '수정골렘':   ('수정 갑피',      'Crystal Carapace', '수정 껍질이 충격을 흩어 버린다.',              'A crystal shell scatters the impact.'),
    '지네':       ('다발 물기',      'Manyfold Bite',    '수많은 다리로 붙어 연달아 문다.',              'Clings with many legs and bites in series.'),
    '이끼괴물':   ('습지 포옹',      'Bog Embrace',      '축축한 몸으로 감싸 움직임을 늦춘다.',          'Wraps you in sodden bulk and slows you.'),
    '석상병':     ('석화 응시',      'Petrifying Gaze',  '돌처럼 굳은 시선으로 노려본다.',              'Fixes you with a gaze gone to stone.'),
    '그림자':     ('그림자 잠행',    'Shadowslip',       '그림자에 스며 공격을 비껴간다.',              'Melts into shadow and lets blows pass.'),
    '도롱뇽':     ('불꽃 숨',        'Ember Breath',     '달아오른 숨을 내뿜는다.',                     'Exhales a breath gone red with heat.'),
    '광충':       ('현혹의 빛',      'Dazzling Glow',    '눈부신 빛으로 조준을 흐트러뜨린다.',           'Blooms with light that ruins your aim.'),
    '망령':       ('원혼의 손길',    'Wraithtouch',      '차가운 손이 방어를 지나 스민다.',              'A cold hand seeps past your guard.'),
    '석상기사':   ('방패 밀치기',    'Shield Bash',      '거대한 방패로 밀어붙인다.',                   'Drives forward behind a vast shield.'),
    '타락사제':   ('뒤틀린 기도',    'Twisted Prayer',   '일그러진 기도가 상처를 벌린다.',              'A warped prayer pries your wounds open.'),
    '키메라':     ('세 갈래 이빨',   'Trifang',          '세 머리가 번갈아 물어뜯는다.',                'Three heads tear at you in turn.'),
    '가고일':     ('석익 강하',      'Stonewing Dive',   '돌 날개로 급강하해 내리찍는다.',              'Folds its stone wings and dives.'),
    '수호자':     ('봉인의 일격',    'Sealing Blow',     '봉인의 힘을 실어 내리친다.',                  'Brings down a blow weighted with wards.'),
    '환영':       ('허상 분열',      'Mirage Split',     '허상을 만들어 공격을 흘린다.',                'Splits into mirages and sheds your blows.'),
    '타락정령':   ('오염된 바람',    'Blighted Gale',    '썩은 바람이 살갗을 태운다.',                  'A rotten wind sears the skin.'),
    '제단수호물': ('제단의 진노',    "Altar's Wrath",    '제단을 건드린 자에게 진노를 내린다.',          'Visits wrath on those who touch the altar.'),
    '공허체':     ('공허 삼킴',      'Void Swallow',     '닿는 것을 공허로 빨아들인다.',                'Draws whatever it touches into the void.'),
    '고블린':     ('잽싼 찌르기',    'Quick Jab',        '짧고 빠르게 찔러 넣는다.',                    'Jabs short and fast.'),
}

# 수식어가 없는 특수 개체 — 이름을 건드리지 않고 기본종만 지정한다.
SPECIAL_BASE = {2001: '슬라임', 2002: '슬라임', 2003: '고블린'}


def col_index(ws, names):
    """헤더(1행)에서 컬럼명 → 1-based 인덱스."""
    head = [c.value for c in ws[1]]
    idx = {}
    for n in names:
        if n not in head:
            raise SystemExit('컬럼 없음: %s (있는 것: %s)' % (n, head))
        idx[n] = head.index(n) + 1
    return idx


def skill_key(eng_base):
    return 'MonSkill' + ''.join(ch for ch in eng_base if ch.isalnum())


def main(write):
    lwb = openpyxl.load_workbook(LOCALIZE)
    lws = lwb[lwb.sheetnames[0]]
    loc = {}                      # key -> (row, kor, eng)
    for r in range(2, lws.max_row + 1):
        k = lws.cell(r, 1).value
        if k:
            loc[str(k)] = (r, lws.cell(r, 2).value, lws.cell(r, 3).value)

    cwb = openpyxl.load_workbook(CREATURE)
    cws = cwb[cwb.sheetnames[0]]
    ci = col_index(cws, ['DataId', 'Name', 'MaxHp', 'PhysicalAtk', 'Rarity',
                         'CritRate', 'CritDamage', 'PhysicalResist', 'Evasion',
                         'DmgAmp', 'MonsterAttackDelay',
                         'UniqueSkillName', 'UniqueSkillDesc'])

    # ── 읽기: 데이터는 4행부터 ──
    mons = []
    for r in range(4, cws.max_row + 1):
        did = cws.cell(r, ci['DataId']).value
        if did in (None, ''):
            continue
        key = str(cws.cell(r, ci['Name']).value)
        kor = loc.get(key, (None, '', ''))[1] or ''
        eng = loc.get(key, (None, '', ''))[2] or ''
        base = SPECIAL_BASE.get(int(did))
        if base is None:
            for m in OLD_MODS:
                if kor.startswith(m + ' '):
                    base = kor[len(m) + 1:]
                    break
        mons.append({
            'row': r, 'id': int(did), 'key': key, 'kor': kor, 'eng': eng,
            'base': base, 'special': int(did) in SPECIAL_BASE,
            'hp': float(cws.cell(r, ci['MaxHp']).value or 0),
            'atk': float(cws.cell(r, ci['PhysicalAtk']).value or 0),
            'rarity': str(cws.cell(r, ci['Rarity']).value or 'Common'),
        })

    unknown = [m for m in mons if not m['base']]
    if unknown:
        raise SystemExit('기본종을 못 가른 몬스터 %d종: %s'
                         % (len(unknown), [m['id'] for m in unknown[:5]]))

    # 기본종 영문명 — "Starving Slime" → "Slime"
    base_eng = {}
    for m in mons:
        if m['special'] or m['base'] in base_eng:
            continue
        parts = m['eng'].split(' ')
        base_eng[m['base']] = ' '.join(parts[1:]) if len(parts) > 1 else m['eng']
    base_eng.setdefault('고블린', 'Goblin')

    # ── 역할 배정 ──
    groups = collections.defaultdict(list)
    for m in mons:
        groups[m['base']].append(m)
    missing = [b for b in groups if b not in ROLE_BY_BASE]
    if missing:
        raise SystemExit('역할 미정의 기본종: %s' % missing)
    role_of = {b: ROLE_BY_BASE[b] for b in groups}
    # 참고용으로만 남긴다(배정에는 쓰지 않는다 — 위 주석 참조).
    ratio = {b: (sum(x['atk'] for x in v) / max(1e-9, sum(x['hp'] for x in v)))
             for b, v in groups.items()}

    # ── 계산 ──
    changes = {'name': 0, 'skill': 0, 'combat': 0}
    new_loc = {}                  # key -> (kor, eng)  신규/갱신
    plan = []

    for base, members in groups.items():
        members.sort(key=lambda m: m['id'])
        normal = [m for m in members if not m['special']]
        N = len(normal)
        step = len(LADDER) // N if N and len(LADDER) % N == 0 else None
        role = role_of[base]
        d0, ev0, cr0, pr0, amp0 = ROLES[role]

        sk = SKILLS.get(base)
        if sk is None:
            raise SystemExit('고유기술 미정의 기본종: %s' % base)
        kname, ename, kdesc, edesc = sk
        kk = skill_key(base_eng[base])
        new_loc[kk + 'Name'] = (kname, ename)
        new_loc[kk + 'Desc'] = (kdesc, edesc)

        for i, m in enumerate(members):
            # 난이도 단계 t: 0(약) ~ 1(강)
            t = (i / (len(members) - 1)) if len(members) > 1 else 0.0
            rar = m['rarity']

            delay  = d0
            evade  = ev0
            crit   = cr0 + 4.0 * t
            resist = pr0 + 6.0 * t
            amp    = amp0 + 3.0 * t

            if rar == 'Rare':
                crit *= 1.3; resist *= 1.25; evade *= 1.2; delay *= 0.95
            elif rar == 'Boss':
                crit *= 1.5; resist += 8.0; amp += 10.0
                evade = 0.0            # 보스가 빗나가게 하면 답답하기만 하다
                delay *= 1.10          # 한 방이 무겁게

            m['calc'] = dict(
                delay=round(min(2.6, max(0.8, delay)), 2),
                evade=round(min(CAP['evasion'], evade), 1),
                crit=round(min(CAP['crit'], crit), 1),
                resist=round(min(CAP['resist'], resist), 1),
                amp=round(min(CAP['amp'], amp), 1),
                skill_name=kk + 'Name', skill_desc=kk + 'Desc', role=role)

            # 이름 — 특수 개체는 손대지 않는다
            if not m['special'] and step:
                tier = normal.index(m) * step
                mod_k, mod_e = LADDER[tier]
                nk = '%s %s' % (mod_k, base)
                ne = '%s %s' % (mod_e, base_eng[base])
                if nk != m['kor'] or ne != m['eng']:
                    new_loc[m['key']] = (nk, ne)
                    changes['name'] += 1
                m['newkor'] = nk
        plan.append((base, role, ratio[base], len(members)))

    # ── 미리보기 ──
    print("=== 역할 배정 ===")
    for base, role, rt, cnt in sorted(plan, key=lambda x: x[2]):
        d, ev, cr, pr, amp = ROLES[role]
        print('  %-10s %-4s 비율%.4f %2d종  대기%.2f 회피%-5.1f 치명%-5.1f 물저%-5.1f 피증%.1f'
              % (base, role, rt, cnt, d, ev, cr, pr, amp))

    dup = collections.Counter(m.get('newkor', m['kor']) for m in mons)
    print()
    print('=== 결과 ===')
    print('  이름 변경         : %d종' % changes['name'])
    print('  이름 중복 (변경후): %d종' % sum(v for v in dup.values() if v > 1))
    print('  로컬라이즈 신규/갱신: %d행' % len(new_loc))
    print('  고유기술          : %d종' % (2 * len(groups)))

    print()
    print('=== 슬라임 계열 미리보기 ===')
    for m in sorted(groups['슬라임'], key=lambda x: x['id']):
        c = m['calc']
        print('  %s %-14s %-6s 대기%.2f 회피%-5.1f 치명%-5.1f 물저%-5.1f 피증%-5.1f %s'
              % (m['id'], m.get('newkor', m['kor']), m['rarity'],
                 c['delay'], c['evade'], c['crit'], c['resist'], c['amp'], c['role']))

    if not write:
        print()
        print('※ 미리보기만 했다. 반영하려면 --write')
        return

    # ── 쓰기 ──
    for m in mons:
        c = m['calc']
        cws.cell(m['row'], ci['MonsterAttackDelay']).value = c['delay']
        cws.cell(m['row'], ci['Evasion']).value            = c['evade']
        cws.cell(m['row'], ci['CritRate']).value           = c['crit']
        cws.cell(m['row'], ci['PhysicalResist']).value     = c['resist']
        cws.cell(m['row'], ci['DmgAmp']).value             = c['amp']
        cws.cell(m['row'], ci['UniqueSkillName']).value    = c['skill_name']
        cws.cell(m['row'], ci['UniqueSkillDesc']).value    = c['skill_desc']
        changes['combat'] += 1
    cwb.save(CREATURE)

    add = 0
    for k, (kor, eng) in new_loc.items():
        if k in loc:
            r = loc[k][0]
        else:
            r = lws.max_row + 1
            lws.cell(r, 1).value = k
            add += 1
        lws.cell(r, 2).value = kor
        lws.cell(r, 3).value = eng
    lwb.save(LOCALIZE)

    print()
    print('반영 완료 — CreatureData %d행 / Localization 갱신 %d행(신규 %d)'
          % (changes['combat'], len(new_loc), add))
    print('Unity 에서 `Tools/ParseAllLocal (로컬 전체 파싱)` 을 돌릴 것.')


if __name__ == '__main__':
    main('--write' in sys.argv)
