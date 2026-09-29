# 스토어 등록정보 초안 — Google Play · Steam

- 기준일: 2026-09-29.
- 게임명 `미니몬 퀘스트 / Minimon Quest`은 **가칭**이다. 판매 방식 **유료 · 인앱 결제 없음**도 **가정**이다. 무료+광고나 인앱 결제로 바꾸면 "광고 없음 / 인앱 결제 없음" 문장을 지운다.
- 수치는 2026-09-29 기준 데이터(`Assets/@Resources/Data/JsonData`)에서 셌다. 마물 503(`CreatureData`), 포획 가능 미니몬 503(`MinMonData`, 전부 포획 확률 > 0), 레벨 300(`LevelData`), 장비 62(`EquipmentData`, 6부위), 최대 강화 +9(`EquipEnhanceData`), 협회 임무 55(`QuestData`), 지역 25·사냥터 125(`TravelNodeData`), 방치 보상 상한 8시간(`GameManager.OFFLINE_MAX_HOURS`).
- 데이터가 바뀌면 숫자를 다시 센다. 장비는 여유를 두려고 "60종 이상"으로 썼다.
- 글자 수는 스크립트로 셌다. 코드 포인트 기준이고 줄바꿈도 1자로 센다. 한도는 Google Play 제목 30 / 짧은 설명 80 / 자세한 설명 4000, Steam 짧은 설명 300.
- 영어 용어는 게임 안 표기와 맞췄다. 공명도 = `Link`, 축성 = `Anoint`, 모험가협회 = `Adventurer's Guild`, 마물 도감 = `Bestiary`.

---

## 1. Google Play

### 앱 이름 (KO) — 7자 / 30
<!-- id:gp_title_ko -->
```text
미니몬 퀘스트
```

### App name (EN) — 13 chars / 30
<!-- id:gp_title_en -->
```text
Minimon Quest
```

제목에 키워드, 순위 표현, 이모지를 넣지 않는다. Google Play 메타데이터 정책 위반이다.

### 짧은 설명 (KO) — 41자 / 80
<!-- id:gp_short_ko -->
```text
마물을 길들여 동료로! 503종 도감을 채우는 오프라인 세로형 픽셀 RPG
```

### Short description (EN) — 79 chars / 80
<!-- id:gp_short_en -->
```text
Tame monsters as companions and fill a 503-monster bestiary. Offline pixel RPG.
```

### 자세한 설명 (KO) — 799자 / 4000
<!-- id:gp_full_ko -->
```text
미니몬 퀘스트는 황야를 누비며 마물과 싸우고, 만난 마물을 길들여 동료로 삼는 세로 화면 픽셀 RPG입니다.

나에게 관심을 보인 마물은 유혹해 길들이면 미니몬이 됩니다. 미니몬을 동행으로 데려가면 곁에서 함께 싸우고, 자신의 고유 능력을 빌려줍니다.

■ 가볍게 즐기는 실시간 전투
· 사이드뷰 자동 전투 — 다가오는 마물과 알아서 싸웁니다
· 타격을 이어 공세 단계를 올리고 치명타로 큰 피해를 노리세요
· 미니몬 능력은 원하는 순간에 직접 발동

■ 마물 도감 503종 · 미니몬 길들이기
· 처치할 때마다 도감 기록이 쌓입니다
· 관심을 보인 마물은 유혹해서 길들여 보세요
· 공명도 100%를 달성하면 그 미니몬의 능력을 영구히 익힙니다

■ 기억력으로 짜는 나만의 빌드
· 능력마다 차지하는 기억력이 다릅니다
· 한정된 기억력 안에서 무엇을 기억하고 무엇을 잊을지 고르세요

■ 제작 · 강화 · 축성
· 대장간에서 60종 이상의 장비를 제작하고 최대 +9까지 강화
· 전투로 쌓은 숙련도로 수도원에서 [축성]해 직업 능력 습득
· 연금공방의 회복·신속 물약과 자동 물약

■ 오래 이어지는 성장
· 레벨 300까지 성장
· 모험가협회 임무 55개
· 25개 지역, 125곳의 사냥터를 여정으로 개척

■ 쉬는 동안에도 사냥은 계속
· 게임을 켜 둔 채 마을에 있어도 마지막 사냥터에서 사냥이 이어집니다
· 다시 접속하면 자리를 비운 시간만큼(최대 8시간) 보상을 받습니다

■ 편하게 즐기세요
· 완전 오프라인 — 인터넷 연결과 계정이 필요 없습니다
· 광고 없음, 인앱 결제 없음
· 한국어 / English 지원
```

### Full description (EN) — 1559 chars / 4000
<!-- id:gp_full_en -->
```text
Minimon Quest is a portrait pixel-art RPG: fight your way across the wilds and tame the monsters you meet.

Monsters that take an interest in you can be lured and tamed into Minimons. Bring one along as a companion and it fights at your side and lends you its unique ability.

■ Easygoing real-time combat
· Side-view auto battles: your adventurer takes on whatever comes near
· Keep landing hits to raise your combo stage and go for big critical hits
· Trigger Minimon abilities yourself when the moment is right

■ A 503-monster bestiary and Minimon taming
· Every monster you defeat adds to your bestiary
· Lure and tame the monsters that take an interest in you
· Reach 100% Link with a Minimon to learn its ability for good

■ Builds shaped by Memory
· Every ability takes up Memory
· Choose what to remember, and what to forget, within your limit

■ Craft, enhance, anoint
· Craft 60+ pieces of gear at the Forge and enhance them up to +9
· Spend proficiency earned in battle to Anoint at the Monastery and learn class abilities
· Healing and haste potions from the Alchemy workshop, with auto-potion

■ Long-term growth
· Level up to 300
· 55 Adventurer's Guild missions
· Travel across 25 regions and 125 hunting grounds

■ The hunt goes on while you rest
· With the game open, your last hunting ground keeps paying out even while you're in town
· Come back later to collect rewards for the time you were away (up to 8 hours)

■ Play your way
· Fully offline: no internet connection or account needed
· No ads, no in-app purchases
· Korean and English
```

### 기타 등록 항목
| 항목 | 제안 |
|---|---|
| 앱/게임 | 게임 |
| 카테고리 | 롤플레잉 (Role Playing) |
| 태그 (최대 5) | 몬스터 수집 · 픽셀 아트 · 방치형 · 싱글 플레이어 · 오프라인. 콘솔의 태그 목록에서 가장 가까운 것을 고른다 |
| 연락처 | 이메일 필수: `[연락처 이메일]`. 웹사이트는 선택이고 개인정보처리방침과 같은 도메인이면 좋다 |
| 언어 | 기본 한국어(ko-KR), 번역 English(en-US). 두 언어의 등록정보를 모두 입력한다 |

---

## 2. Steam

### Short description (KO) — 152자 / 300
<!-- id:st_short_ko -->
```text
황야를 누비며 싸우고, 나에게 관심을 보인 마물을 길들여 미니몬 동료로 데려가세요. 503종 마물 도감, 기억력으로 짜는 능력 빌드, 장비 제작과 +9 강화, 레벨 300까지의 성장을 담은 세로 화면 픽셀 RPG. 자리를 비운 시간의 사냥 보상도 최대 8시간까지 받습니다.
```

### Short description (EN) — 288 chars / 300
<!-- id:st_short_en -->
```text
Fight across the wilds, then tame the monsters that take an interest in you and bring them along as Minimon companions. A portrait pixel-art RPG with a 503-monster bestiary, Memory-based builds, gear crafting and +9 enhancement, 300 levels, and up to 8 hours of rewards while you're away.
```

### About This Game (KO) — 630자
<!-- id:st_about_ko -->
```text
미니몬 퀘스트는 황야를 누비며 마물과 싸우고, 만난 마물을 길들여 동료로 삼는 세로 화면 픽셀 RPG입니다.

나에게 관심을 보인 마물은 유혹해 길들이면 미니몬이 됩니다. 미니몬을 동행으로 데려가면 곁에서 함께 싸우고, 자신의 고유 능력을 빌려줍니다. 공명도 100%를 달성하면 그 능력을 영구히 익힐 수 있습니다.

[전투]
사이드뷰 자동 전투에서 타격을 이어 공세 단계를 올리고 치명타를 노리세요. 미니몬 능력은 원하는 순간에 직접 발동합니다.

[수집]
마물 503종이 도감을 기다립니다. 처치할수록 기록이 쌓이고, 관심을 보인 마물은 길들일 수 있습니다.

[빌드]
능력마다 차지하는 기억력이 다릅니다. 한정된 기억력 안에서 무엇을 기억하고 무엇을 잊을지 고르세요. 수도원에서 [축성]하면 직업 능력을, 대장간에서는 60종 이상의 장비를 제작해 최대 +9까지 강화할 수 있습니다.

[성장]
레벨 300, 모험가협회 임무 55개, 25개 지역과 125곳의 사냥터. 게임을 켜 둔 채 마을에 있어도 마지막 사냥터에서 사냥이 이어지고, 자리를 비운 시간만큼(최대 8시간) 보상을 받습니다.

[그 밖에]
· 세로형 창 모드, 마우스 조작
· 완전 오프라인 — 인터넷 연결과 계정이 필요 없습니다
· 한국어 / English
```

### About This Game (EN) — 1240 chars
<!-- id:st_about_en -->
```text
Minimon Quest is a portrait pixel-art RPG: fight your way across the wilds and tame the monsters you meet.

Monsters that take an interest in you can be lured and tamed into Minimons. Bring one along as a companion and it fights at your side and lends you its unique ability. Reach 100% Link and that ability is yours for good.

[Combat]
Keep landing hits in side-view auto battles to raise your combo stage and go for critical hits. Trigger Minimon abilities yourself when the moment is right.

[Collect]
503 monsters are waiting for your bestiary. Every defeat adds to your records, and the monsters that take an interest in you can be tamed.

[Build]
Every ability takes up Memory. Choose what to remember, and what to forget, within your limit. Anoint at the Monastery to learn class abilities, and craft 60+ pieces of gear at the Forge and enhance them up to +9.

[Grow]
300 levels, 55 Adventurer's Guild missions, 25 regions and 125 hunting grounds. With the game open, your last hunting ground keeps paying out even while you're in town, and you collect rewards for the time you were away (up to 8 hours).

[Also]
· Portrait windowed mode, mouse controls
· Fully offline: no internet connection or account needed
· Korean and English
```

"세로형 창 모드, 마우스 조작"은 `Release_Readiness_2026-09-29.md`의 3·4번 작업(ESC 처리, PC 기본 창 모드)이 끝나야 사실이 된다. 끝나기 전에 게시하지 않는다.

### 태그·장르·언어·사양
| 항목 | 제안 |
|---|---|
| 장르 (Genre) | RPG, Casual, Indie |
| 태그 (최대 20, 중요도 순) | Creature Collector, RPG, Pixel Graphics, Idler, 2D, Singleplayer, Fantasy, Cute, Crafting, Side Scroller, Casual, Indie |
| 지원 언어 | Korean · English, 둘 다 Interface + Subtitles. 음성(Full Audio)은 없다 |
| 최소 사양 (초안) | Windows 10 64-bit, DirectX 11 GPU, RAM 4 GB, 저장 공간 `[빌드 크기 + 여유]` |
| 컨트롤러 | 지원 안 함 (마우스 전용). Steam Deck은 체크리스트 2-8 참고 |

실제 입력 가능한 태그는 Steamworks의 태그 선택기에서 고른다. 게임에 약한 요소(Loot, Character Customization 등)는 넣지 않는다.

---

## 3. 콘텐츠 등급 설문 답안 (IARC · Google Play / Steam Content Survey 공통 근거)

근거로 코드와 데이터를 확인했다.
- 로컬라이즈·대사·아이템 데이터 전체에서 피·고어·술·담배·약물·욕설·성적 표현·도박 관련 단어를 찾았고 0건이다. 예외로 물약 설명에 "맛은… 더럽게 없다" 같은 구어체 강조 한 곳이 있다. 욕설로 보지 않는다.
- 몬스터 사망 스프라이트를 표본으로 확인했다. 쓰러지거나 납작해질 뿐 피나 절단 표현은 없다.
- 적은 판타지 마물이고 일부는 동물형(멧돼지, 늑대, 까마귀 등)이나 인간형 판타지 존재(타락한 사제, 석상 기사)다. 실제 인간 NPC를 공격하지 않는다.
- 결제 수단이 없다. 강화 성공 확률, 실패 시 내구도 감소, 포획 확률은 모두 게임 내 골드와 재료로만 돌아간다. 현금으로 사는 무작위 아이템(루트박스)이 없다.
- 이용자 간 소통, 공유, 위치 기능, 브라우저가 없다.

| 설문 영역 | 답 | 메모 |
|---|---|---|
| 카테고리 | Game | |
| 폭력 (Violence) | 예 — 판타지/비사실적 | 판타지 마물·동물형 마물과 전투. 사실적인 인간 대상 폭력은 없다 |
| 피 (Blood) | 아니오 | 피격은 번쩍임과 이펙트로 표현한다 |
| 고어·신체 훼손 | 아니오 | |
| 공포 (Fear) | 아니오 | 어두운 지역 몬스터가 있으나 공포 연출은 없다 |
| 성적 내용·노출 | 아니오 | |
| 욕설·저속한 언어 | 아니오 | 위 구어체 1건은 욕설이 아니다 |
| 약물·술·담배 | 아니오 | 물약은 판타지 회복·신속 약이다 |
| 저속한 유머 | 아니오 | |
| 차별·혐오 | 아니오 | |
| 모의 도박 (Simulated gambling) | 아니오 | 카지노·카드·슬롯 형식이 없다. 강화·포획 확률은 도박으로 분류하지 않는다 |
| 실제 돈 도박 | 아니오 | |
| 인앱 구매 / 무작위 아이템 구매 | 아니오 | 유료 앱 가정. 인앱 결제로 바뀌면 다시 답한다 |
| 사용자 간 상호작용·콘텐츠 공유 | 아니오 | |
| 위치 공유 | 아니오 | |
| 개인정보 공유 | 아니오 | |
| 무제한 인터넷 접속(브라우저) | 아니오 | |

예상 등급은 참고용이며 설문 결과가 최종이다. PEGI 7, ESRB E10+ 안팎이 나올 것으로 본다. 판타지 폭력만 있기 때문이다. 콘텐츠를 바꿨는데 답이 달라지면 설문을 다시 제출한다. Google Play 정책이다.

Steam Content Survey의 "Mature Content"는 해당 없음으로 답한다. "Generative AI" 섹션은 `Release_Checklist.md` 2-6을 따른다.
