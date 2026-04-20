# Slide 06 - Player BAR

## 이미지 정보 이용 버전

```text
PPT 6번 슬라이드 이미지를 기준으로 RE001 Player BAR UI를 구현해줘.

참조 이미지:
C:\stopresent\Unity\RE001\tmp\ppt_slides\슬라이드6.PNG

구현 목적:
- 플레이어의 초상화, 모험가 등급, 직업, 레벨, 현재 HP/최대 HP, EXP 진행률을 한 개의 상단 Player BAR에서 표시한다.
- HP/EXP 변화 시 Lerp와 색상/스케일 피드백으로 상태 변화를 보여준다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

사용 UI 리소스:
- 전체 BAR 배경: HP_mana_bar
- HP 빈 프레임: Hp_frame_Empty
- HP 빨간 Fill: Hp_red
- EXP 초록 Fill: 1-EG
- 초상화 프레임: HP_Portrait_frame
- 기본 플레이어 초상화: Player_01_Port
- 직업에 따라 초상화는 추후 해당 직업 Portrait로 변경될 수 있어야 한다.

레이아웃:
- 왼쪽에는 플레이어 초상화 원형 프레임을 배치한다.
- 초상화는 Mask 처리한다.
- 초상화는 Layer1, Layer2 구조처럼 프레임과 이미지가 분리되어야 한다.
- 상단 정보 라인에는 모험가 등급, 직업, 레벨을 표시한다.
- 중단에는 HP Bar와 "현재 HP / 최대 HP" 텍스트를 표시한다.
- 하단에는 EXP Bar와 "##.####%" 형식의 EXP 퍼센트 텍스트를 표시한다.

모험가 등급:
- 0부터 시작하며 최대 99까지 있다.
- 값이 0일 때는 "[    ]" BLANK로 표기한다.
- 1 이상일 때는 해당 등급 텍스트를 표시한다.
- 언어 선택과 무관하게 DOSSaemmul 폰트를 사용한다.

직업:
- 플레이어의 현재 직업명을 표시한다.
- 직업명 글자 수에 따라 레벨 표기가 오른쪽으로 밀리는 구조를 유지한다.
- 직업 텍스트가 길어져도 레벨 텍스트와 겹치지 않게 레이아웃 그룹 또는 동적 위치 보정을 적용한다.

레벨:
- 플레이어의 현재 레벨을 표시한다.
- 1부터 시작하여 999까지 있다.
- 항상 "LV.#" 형식으로 표기한다.
- 언어 선택과 무관하게 DOSSaemmul 폰트를 사용한다.
- Outline: 62007B, x 1, y 1
- 직업 글자 수에 따라 레벨 표기가 밀리는 구조를 유지한다.

HP 표시:
- 현재 HP와 최대 HP를 "현재 HP / 최대 HP" 형식으로 표시한다.
- HP Bar Fill은 현재 HP / 최대 HP 비율로 표시한다.
- Bar sprite는 Lerp 처리한다.
- HP가 감소할 때 hurt 애니메이션을 재생한다.
- HP가 증가할 때 heal 애니메이션을 재생한다.

HP_BAR Anim(hurt):
- Bar sprite는 Lerp 처리한다.
- 피해 시 0.17초에 걸쳐 빨강 FF0000으로 변환한다.
- 이후 다시 0.17초에 걸쳐 흰색 FFFFFF로 변환한다.
- 가능하면 HP Fill 오브젝트의 기존 DOTween Color 컴포넌트를 사용한다.

HP_BAR Anim(heal):
- Bar sprite는 Lerp 처리한다.
- 회복 시 0.17초에 걸쳐 초록 58FF00으로 변환한다.
- 이후 다시 0.17초에 걸쳐 흰색 FFFFFF로 변환한다.
- Scale도 동일한 타이밍으로 증가 후 원래 크기로 복귀한다.
- 가능하면 HP Fill 오브젝트의 기존 DOTween Color/Scale 컴포넌트를 사용한다.

EXP 표시:
- EXP Bar Fill은 현재 경험치 비율로 표시한다.
- EXP 퍼센트는 항상 "##.####%" 형식으로 표기한다.
- 예: 0.0000%, 40.0000%, 99.1234%
- Bar sprite는 Lerp 처리한다.

EXP_BAR Anim:
- Bar sprite는 Lerp 처리한다.
- 경험치 변화 시 0.17초에 걸쳐 초록 58FF00으로 변환한다.
- 이후 다시 0.17초에 걸쳐 흰색 FFFFFF로 변환한다.
- Scale도 동일하게 증가 후 원래 크기로 복귀한다.
- 가능하면 EXP Fill 오브젝트의 기존 DOTween Color/Scale 컴포넌트를 사용한다.

구현 구조:
- PlayerBarController 컴포넌트를 만든다.
- PlayerPortrait, RankText, JobText, LevelText, HpText, HpFill, ExpText, ExpFill을 참조한다.
- PlayerData 또는 PlayerStatus 이벤트를 구독해 자동으로 갱신한다.
- SetHp(currentHp, maxHp), SetExpRatio(ratio), SetLevel(level), SetJob(job), SetRank(rank), SetPortrait(sprite) API를 제공한다.
- HP/EXP Fill 변경은 즉시 점프하지 않고 Lerp로 부드럽게 갱신한다.
- UI 갱신과 애니메이션 재생 로직은 분리한다.
- 폰트와 outline은 Slide 03의 Font Policy를 따른다.

완료 기준:
- Player BAR에 초상화, 모험가 등급, 직업, 레벨, HP, EXP가 표시된다.
- 모험가 등급 0은 BLANK로 표시된다.
- 레벨은 항상 "LV.#" 형식으로 표시된다.
- 직업명이 길어져도 레벨 텍스트와 겹치지 않는다.
- HP 감소 시 빨간색 피드백, HP 회복 시 초록색 및 Scale 피드백이 재생된다.
- EXP 변화 시 초록색 및 Scale 피드백이 재생된다.
- HP/EXP Fill은 Lerp로 부드럽게 변경된다.
```

## 텍스트 설명만 사용하는 버전

```text
RE001의 Player BAR UI를 Unity에서 구현해줘.

구현 목적:
- 플레이어의 기본 상태 정보를 상단 Player BAR에서 보여준다.
- 표시 정보는 초상화, 모험가 등급, 직업, 레벨, 현재 HP/최대 HP, EXP 퍼센트다.
- HP와 EXP 변화는 Lerp와 색상/스케일 피드백으로 표현한다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

UI 구성:
- 왼쪽에 플레이어 초상화 프레임과 초상화 이미지를 배치한다.
- 초상화는 Mask 처리한다.
- 상단 정보 라인에는 모험가 등급, 직업, 레벨을 배치한다.
- 중단에는 HP Bar와 "현재 HP / 최대 HP" 텍스트를 배치한다.
- 하단에는 EXP Bar와 EXP 퍼센트 텍스트를 배치한다.

사용 리소스:
- 전체 BAR 배경: HP_mana_bar
- HP 빈 프레임: Hp_frame_Empty
- HP Fill: Hp_red
- EXP Fill: 1-EG
- 초상화 프레임: HP_Portrait_frame
- 기본 초상화: Player_01_Port
- 초상화는 추후 직업별 Portrait로 교체 가능하게 만든다.

모험가 등급:
- 0부터 99까지 존재한다.
- 값이 0이면 "[    ]" BLANK로 표시한다.
- 1 이상이면 해당 등급을 표시한다.
- 언어 설정과 무관하게 DOSSaemmul 폰트를 사용한다.

직업:
- 현재 플레이어 직업명을 표시한다.
- 직업명 글자 수에 따라 레벨 텍스트 위치가 오른쪽으로 밀려야 한다.
- 직업명과 레벨 텍스트가 겹치지 않도록 동적 레이아웃을 적용한다.

레벨:
- 1부터 999까지 존재한다.
- 항상 "LV.#" 형식으로 표시한다.
- 언어 설정과 무관하게 DOSSaemmul 폰트를 사용한다.
- Outline은 62007B, x 1, y 1로 설정한다.

HP:
- 현재 HP와 최대 HP를 "현재 HP / 최대 HP" 형식으로 표시한다.
- HP Fill은 currentHp / maxHp 비율을 따른다.
- Fill 값은 Lerp로 부드럽게 변경한다.
- HP 감소 시 hurt 피드백을 재생한다.
- HP 증가 시 heal 피드백을 재생한다.

HP hurt 피드백:
- HP Fill 색상을 0.17초 동안 FF0000으로 변경한다.
- 이후 0.17초 동안 FFFFFF로 복귀한다.
- 기존 DOTween Color 컴포넌트가 있으면 해당 컴포넌트를 재사용한다.

HP heal 피드백:
- HP Fill 색상을 0.17초 동안 58FF00으로 변경한다.
- 이후 0.17초 동안 FFFFFF로 복귀한다.
- Scale도 같은 흐름으로 증가 후 원래 크기로 복귀한다.
- 기존 DOTween Color/Scale 컴포넌트가 있으면 해당 컴포넌트를 재사용한다.

EXP:
- EXP Fill은 현재 경험치 비율을 따른다.
- Fill 값은 Lerp로 부드럽게 변경한다.
- EXP 텍스트는 항상 "##.####%" 형식으로 표시한다.
- 경험치 변화 시 EXP 피드백을 재생한다.

EXP 피드백:
- EXP Fill 색상을 0.17초 동안 58FF00으로 변경한다.
- 이후 0.17초 동안 FFFFFF로 복귀한다.
- Scale도 같은 흐름으로 증가 후 원래 크기로 복귀한다.
- 기존 DOTween Color/Scale 컴포넌트가 있으면 해당 컴포넌트를 재사용한다.

구현 방식:
- PlayerBarController 컴포넌트를 만든다.
- PlayerPortrait, RankText, JobText, LevelText, HpText, HpFill, ExpText, ExpFill 참조를 가진다.
- SetHp(currentHp, maxHp), SetExpRatio(ratio), SetLevel(level), SetJob(job), SetRank(rank), SetPortrait(sprite) API를 제공한다.
- PlayerData 또는 PlayerStatus 변경 이벤트가 있다면 구독해서 자동 갱신한다.
- UI 표시 갱신과 피드백 애니메이션 재생 로직을 분리한다.
- 폰트와 outline은 공통 Font Policy를 따른다.

완료 기준:
- Player BAR에 초상화, 모험가 등급, 직업, 레벨, HP, EXP가 표시된다.
- 모험가 등급 0은 BLANK로 표시된다.
- 레벨은 항상 "LV.#" 형식을 유지한다.
- 직업명이 길어져도 레벨 텍스트와 겹치지 않는다.
- HP 감소/회복과 EXP 변화에 맞는 색상 및 Scale 피드백이 재생된다.
- HP/EXP Fill은 Lerp로 부드럽게 갱신된다.
```
