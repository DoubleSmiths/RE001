# -*- coding: utf-8 -*-
"""v2 마을 사람들 — 이야기 장면·좋아하는 선물 생성기 (ADR0042).

  BondNpcData.xlsx   — 마을 사람 5명(BondManager.Npc 순서)의 좋아하는 재료 6가지.
  DialogueData.xlsx  — 호감도 3·6·10단계 이야기 15편을 시트로(Bond{사람}{번호}Text). 다른 시트는 건드리지 않는다.

  대사를 고칠 때는 여기를 고치고 다시 돌린 뒤 Unity Tools/ParseAllLocal.
    python _tools/gen_bond.py            # 두 엑셀을 쓴다
    python _tools/gen_bond.py --check    # 저장된 엑셀이 생성 결과와 같은지만 본다
"""
import argparse
import os
import sys

import openpyxl

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
EXCEL = os.path.join(ROOT, 'Assets', '@Resources', 'Data', 'Excel')

# BondManager.Npc 순서 — 접수원 · 연금술사 · 대장장이 · 수도원 사제 · 상점 주인
NPCS = ['Receptionist', 'Alchemist', 'Blacksmith', 'Priest', 'Shopkeeper']
NAME_KEYS = ['NpcReceptionistNameText', 'NpcAlchemistNameText', 'NpcBlacksmithNameText',
             'NpcMonasteryPriestNameText', 'NpcShopkeeperNameText']

# 좋아하는 재료 — 몬스터 재료 30종(11001~11030)을 여섯 개씩, 지역마다 고르게(초반에도 하나씩은 얻는다)
LIKED = [
    [11007, 11002, 11009, 11013, 11014, 11022],   # 접수원: 깃털(펜)·실크(서류 끈)·꼬리털(붓)·광부의 유품(기록)·수정(문진)·기사의 문장(인장)
    [11001, 11004, 11010, 11016, 11020, 11028],   # 연금술사: 젤리·독버섯·말벌 침·이끼·발광 점액·오염된 정수 — 약재
    [11003, 11006, 11012, 11015, 11019, 11025],   # 대장장이: 송곳니·가죽·발톱·외피·비늘·가고일 돌 — 쇠와 가죽
    [11017, 11018, 11021, 11023, 11027, 11029],   # 사제: 석상·그림자·망령의 재·성물·환영·제단 — 성스러운 것과 부정한 것
    [11005, 11008, 11011, 11024, 11026, 11030],   # 상점 주인: 바람 정수·덩굴·날개막·갈기·수호자의 핵·공허 결정 — 진귀한 물건
]

# 이야기 — (한국어, 영어) 줄. 인물 말투: 접수원(딱딱하지만 챙긴다) · 연금술사(장난·♡) · 대장장이(무뚝뚝한 반말) ·
#  사제(경건한 존댓말) · 상점 주인(밝은 장사꾼). 3단계 = 마음을 연다 / 사연 / 믿음.
SCENES = {
    'Receptionist': [
        [
            ('…또 오셨군요.\n요즘은 오시는 게 반갑다고 해 두죠.', "...You again.\nLet's say I'm glad to see you these days."),
            ('협회 규정상 사적인 대화는 금지지만,\n잠깐이면 괜찮겠죠.', 'Guild rules forbid personal chats,\nbut a moment should be fine.'),
            ('사실 이 일, 처음엔 사흘만 하고\n그만둘 생각이었어요.', 'Truth is, I meant to quit this job\nafter three days.'),
            ('그런데 돌아오지 않은 모험가 명단을\n정리하다 보니… 그만둘 수가 없더군요.', "Then I started filing the list of adventurers\nwho never came back... and couldn't leave."),
            ('그러니까 당신은 그 명단에 이름 올리지 마세요.\n서류가 늘어나니까요.', "So don't you end up on that list.\nIt means more paperwork."),
        ],
        [
            ('오늘은 제가 먼저 말을 걸죠.\n드문 일이니 기억해 두세요.', "Today I'll speak first.\nIt's rare, so remember it."),
            ('제 책상 서랍엔 오래된 통행증이 하나 있어요.\n제 것이었죠.', "There's an old pass in my desk drawer.\nIt was mine."),
            ('저도 모험가였어요.\n산들바람 숲 너머까지는 못 갔지만요.', 'I was an adventurer too.\nI never made it past Breezewood Forest.'),
            ('그래서 당신이 새 통행증을 받아 갈 때마다\n조금 부럽기도 해요.', "So every time you take a new pass,\nI'm a little jealous."),
            ('…방금 한 말은 서류에 남기지 마세요.', "...Don't put that in any report."),
        ],
        [
            ('당신이 없는 동안\n협회가 너무 조용했어요.', 'The guild was far too quiet\nwhile you were away.'),
            ('규정집 어디에도 없지만,\n이건 제 개인 인장이에요.', "It's in no rulebook,\nbut this is my personal seal."),
            ('이걸 찍은 서류는 제가 무조건 먼저\n처리해 드려요. 특별 대우죠.', 'Any form stamped with it goes to the top\nof my pile. Special treatment.'),
            ('…고맙다는 말, 한 번만 할게요.\n늘 무사히 돌아와 줘서 고마워요.', "...I'll say thanks just once.\nThank you for always coming back."),
            ('자, 다음 임무 받으러 가시죠.\n제 모험가님.', 'Now, go get your next assignment.\nMy adventurer.'),
        ],
    ],
    'Alchemist': [
        [
            ('어머~ 오늘은 물약 말고\n저를 보러 오신 거죠? 후훗♡', "Oh my~ You came to see me today,\nnot a potion, right? Heehee♡"),
            ('비밀 하나 알려 드릴게요.\n저, 쓴 약은 못 먹어요.', "Here's a secret.\nI can't stand bitter medicine."),
            ('그래서 제 물약엔 몰래 꿀을\n조금씩 넣어요. 다들 몰랐죠?', "So I sneak a little honey into every potion.\nNobody noticed, right?"),
            ('당신 것엔 두 방울 넣어 드릴게요.\n특별 손님이니까요♡', "Yours gets two drops.\nYou're a special customer♡"),
        ],
        [
            ('요즘 새 물약을 연구하고 있어요.\n이름은… 아직 비밀♡', "I'm working on a new potion lately.\nIts name is... a secret♡"),
            ('스승님이 늘 말씀하셨어요.\n좋은 약은 마실 사람을 생각하며 만든다고.', 'My teacher always said good medicine\nis made thinking of the one who drinks it.'),
            ('그래서 요즘 자꾸 당신 생각을 하나 봐요.\n…연구 때문이에요, 연구!', "Maybe that's why I keep thinking of you.\n...For research, of course. Research!"),
            ('완성되면 제일 먼저 맛보게 해 드릴게요.\n약속♡', "When it's done, you'll be the first to taste it.\nPromise♡"),
        ],
        [
            ("짠~ 드디어 완성했어요!\n이름은 '돌아오는 길'이에요.", "Ta-da~ It's finally done!\nI call it 'The Way Back.'"),
            ('효과요? 마시면 무슨 일이 있어도\n이 공방으로 돌아오고 싶어져요.', 'The effect? Whoever drinks it\nwill always want to come back here.'),
            ('…농담이에요, 반쯤은요.\n나머지 반은 제 진심이고요♡', "...I'm joking. Half joking.\nThe other half is how I really feel♡"),
            ('다치면 꼭 여기로 와요.\n제가 전부 고쳐 줄 테니까.', "If you ever get hurt, come straight here.\nI'll fix everything."),
            ('후훗, 오늘은 공짜예요.\n대신 내일도 와야 해요?', "Heehee, today's on the house.\nBut you have to come tomorrow too, okay?"),
        ],
    ],
    'Blacksmith': [
        [
            ('또 왔군. 앉아.\n불 좀 쬐고 가.', 'Back again. Sit.\nWarm yourself by the fire.'),
            ('칼 자국을 보면 어떻게 싸우는지 보여.\n요즘 많이 늘었어.', "Your blade's nicks tell me how you fight.\nYou've gotten better."),
            ('처음엔 막 휘두르기만 하더니,\n이젠 벨 곳을 알고 베는군.', 'At first you just swung.\nNow you know where to cut.'),
            ('…칭찬이야.\n자주 안 하니까 잘 들어 둬.', "...That was praise.\nI don't give it often, so remember it."),
        ],
        [
            ('이 망치, 우리 아버지 거야.', "This hammer was my father's."),
            ('아버진 기사단 무기를 만들었지.\n그 무기 든 기사들은 아무도 돌아오지 못했고.', 'He forged for the knights.\nNone of the knights who carried his blades came home.'),
            ('그래서 난 더 단단하게 두드려.\n들고 간 사람이 살아서 돌아오게.', 'So I strike harder.\nSo whoever carries my work comes back alive.'),
            ('넌 매번 돌아오잖아.\n그게 내 망치질이 맞다는 증거야.', "You come back every time.\nThat's proof my hammer's right."),
        ],
        [
            ('받아.\n남은 쇳조각으로 만든 부적이야.', 'Take this.\nA charm I made from leftover steel.'),
            ('모양은 투박해도,\n내가 만든 것 중 제일 단단해.', "It's crude,\nbut it's the toughest thing I've ever made."),
            ('…너 주려고 만든 거 아니야.\n그냥 쇳조각이 남아서 만든 거야.', "...I didn't make it for you.\nJust had scraps left over."),
            ('그래도 부러지면 가져와.\n몇 번이고 다시 만들어 줄 테니까.', "But if it breaks, bring it back.\nI'll make it again, as many times as it takes."),
        ],
    ],
    'Priest': [
        [
            ('어서 오세요.\n오늘은 기도 대신 이야기를 나눌까요.', 'Welcome.\nShall we talk today instead of praying?'),
            ('이 수도원의 종은 백 년 동안\n한 번도 멈춘 적이 없답니다.', "This monastery's bell hasn't stopped\nonce in a hundred years."),
            ('저는 매일 아침 그 종을 치며,\n떠난 이들이 돌아오기를 빌어요.', 'Every morning I ring it\nand pray for those who left to return.'),
            ('요즘은 그 기도에\n당신의 이름도 넣고 있습니다.', 'Lately,\nyour name is in that prayer too.'),
        ],
        [
            ('고백하자면,\n저는 한때 칼을 들었던 사람입니다.', 'I confess,\nI once carried a sword.'),
            ('지키지 못한 것이 많아서,\n칼 대신 종을 잡았지요.', 'I failed to protect too much,\nso I traded the blade for the bell.'),
            ('당신을 보면 그때의 제가 떠오릅니다.\n하지만 당신은 다르겠지요.', 'You remind me of who I was.\nBut you will be different.'),
            ('당신은 멈추지 않으니까요.\n두려워도 앞으로 가니까요.', "Because you don't stop.\nEven afraid, you keep going."),
        ],
        [
            ('오늘은 제가 당신을 위해\n축복을 올리겠습니다.', 'Today I will offer\na blessing for you.'),
            ('사람을 지키는 것은 힘이 아니라,\n돌아올 곳이 있다는 사실입니다.', 'It is not strength that protects us,\nbut having a place to return to.'),
            ('이 마을이, 그리고 이 수도원이\n당신의 돌아올 곳이 되기를.', 'May this village, and this monastery,\nbe yours to return to.'),
            ('…종소리가 들리면,\n제가 당신을 생각하고 있다는 뜻이에요.', '...When you hear the bell,\nknow that I am thinking of you.'),
        ],
    ],
    'Shopkeeper': [
        [
            ('오, 단골손님!\n오늘은 구경만 해도 돼요. 특별히요.', 'Oh, my regular!\nYou can just browse today. As a favor.'),
            ('장사에서 제일 중요한 게 뭔지 아세요?\n손님 얼굴 기억하기예요.', 'Know the most important thing in trade?\nRemembering faces.'),
            ('손님 얼굴은… 음,\n늘 흙투성이라 기억하기 쉬워요.', "Yours is... well,\nalways covered in dirt, so it's easy."),
            ('농담이에요!\n다음엔 좋은 물건 먼저 보여 드릴게요.', "Just kidding!\nNext time you'll get first look at the good stuff."),
        ],
        [
            ('사실 이 가게,\n원래는 제 할머니 가게였어요.', "Truth is,\nthis shop was my grandmother's."),
            ('할머니는 손해를 봐도\n모험가한테는 물약을 싸게 팔았죠.', 'She sold potions cheap to adventurers,\neven at a loss.'),
            ('저는 장사꾼이라 그렇게는 못 하지만…\n당신한텐 조금 깎아 드릴게요.', "I'm a merchant, so I can't do that...\nbut for you, a small discount."),
            ('비밀이에요.\n다른 손님들이 알면 저 망해요!', "Keep it secret.\nIf the others find out, I'm ruined!"),
        ],
        [
            ('이거 받으세요.\n할머니 가게에서 제일 오래된 금화예요.', "Take this.\nThe oldest gold coin from grandma's shop."),
            ('장사꾼이 돈을 그냥 주다니,\n저도 제가 믿기지 않네요.', "A merchant giving money away.\nI can't believe it myself."),
            ('당신 덕분에 이 마을에 손님이 늘었어요.\n이건 투자예요, 투자!', 'Thanks to you, this town has more customers.\nCall it an investment!'),
            ('앞으로도 우리 가게\n제일 좋은 손님으로 남아 주세요.', 'Stay my very best customer,\nokay?'),
        ],
    ],
}

BOND_HEADER = [('DataId', 'int', '번호(BondManager.Npc)'), ('NameKey', 'string', '이름 로컬라이즈 키'),
               ('LikedItems', 'string', '좋아하는 재료(아이템 DataId, 쉼표)')]


def bond_rows():
    return [(i, NAME_KEYS[i], ','.join(str(x) for x in LIKED[i])) for i in range(len(NPCS))]


def scene_sheets():
    out = {}
    for npc, scenes in SCENES.items():
        for s, lines in enumerate(scenes, 1):
            out[f'Bond{npc}{s}Text'] = [(k, ko, en) for k, (ko, en) in enumerate(lines, 1)]
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--check', action='store_true')
    args = ap.parse_args()

    flat = [x for row in LIKED for x in row]
    assert sorted(flat) == list(range(11001, 11031)), '재료 30종을 한 번씩'
    sheets = scene_sheets()
    print(f'마을 사람 {len(NPCS)}명 · 좋아하는 재료 {len(flat)}종 · 이야기 {len(sheets)}편 {sum(len(v) for v in sheets.values())}줄')

    bond_path = os.path.join(EXCEL, 'BondNpcData.xlsx')
    dlg_path = os.path.join(EXCEL, 'DialogueData.xlsx')
    if args.check:
        ok = True
        wb = openpyxl.load_workbook(bond_path)
        same = [tuple(r) for r in wb.active.iter_rows(min_row=4, values_only=True)] == bond_rows()
        print(('같음 ' if same else '다름 ') + 'BondNpcData.xlsx'); ok &= same
        wb = openpyxl.load_workbook(dlg_path)
        for name, rows in sheets.items():
            same = name in wb.sheetnames and [tuple(r) for r in wb[name].iter_rows(min_row=2, values_only=True)] == rows
            ok &= same
            if not same: print('다름 ' + name)
        print(('같음 ' if ok else '다름 ') + 'DialogueData.xlsx 이야기 시트')
        sys.exit(0 if ok else 1)

    wb = openpyxl.Workbook()
    ws = wb.active
    ws.title = 'BondNpcData'
    for i in range(3):
        ws.append([h[i] for h in BOND_HEADER])
    for r in bond_rows():
        ws.append(list(r))
    wb.save(bond_path)
    print('저장 BondNpcData.xlsx')

    wb = openpyxl.load_workbook(dlg_path)
    for name, rows in sheets.items():
        if name in wb.sheetnames:
            del wb[name]
        ws = wb.create_sheet(name)
        ws.append(['TextKey', 'Kor', 'Eng'])
        for r in rows:
            ws.append(list(r))
    wb.save(dlg_path)
    print(f'저장 DialogueData.xlsx (이야기 시트 {len(sheets)}개, 전체 {len(wb.sheetnames)}개)')


if __name__ == '__main__':
    main()
