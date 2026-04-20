# Slide 10 - 최하단 버튼

## 이미지 정보 이용 버전

```text
PPT 10번 슬라이드 이미지를 기준으로 RE001 최하단 버튼 UI를 구현해줘.

참조 이미지:
C:\stopresent\Unity\RE001\tmp\ppt_slides\슬라이드10.PNG

구현 목적:
- 화면 최하단에 주요 Popup을 여는 6개 버튼 그룹을 만든다.
- 게임 진행도에 따라 버튼이 순차적으로 해금되도록 한다.
- 버튼 클릭 시 연결된 Popup을 호출하고, 해당 Popup이 열린 동안 선택 버튼을 하이라이트한다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

버튼 구성:
- 최하단 버튼은 총 6종류다.
- 버튼 순서는 PPT 예시와 동일하게 왼쪽부터 다음 순서로 배치한다.
  1. 상태
  2. 가방
  3. 물약
  4. 장비
  5. 기술
  6. 도감
- 각 버튼은 아이콘과 텍스트 라벨을 가진다.
- 화면 최하단에 가로로 정렬한다.
- 모바일/세로형 UI 기준에서도 6개 버튼이 겹치지 않도록 고정 크기와 간격을 유지한다.

초기 해금 상태:
- 게임 첫 시작 시 [상태] 버튼만 해금된 상태다.
- 나머지 버튼은 퀘스트 진행도에 따라 순차적으로 해금된다.
- 잠금 상태 버튼은 클릭할 수 없어야 한다.
- 해금 조건은 QuestProgress 또는 UnlockSystem과 연결한다.
- 각 버튼별 해금 퀘스트 번호/조건은 데이터로 분리해 관리할 수 있게 한다.

버튼 동작:
- 각 버튼 클릭 시 연관된 Popup 창을 호출한다.
- [상태] 버튼은 상태정보 Popup을 호출한다.
- [가방] 버튼은 소지품 Popup을 호출한다.
- [물약] 버튼은 물약창 Popup을 호출한다.
- [장비] 버튼은 장비 Popup을 호출한다.
- [기술] 버튼은 기술 Popup을 호출한다.
- [도감] 버튼은 도감 Popup을 호출한다.
- 아직 구현되지 않은 Popup은 연결 지점을 남기되, null 참조 오류가 나지 않게 방어한다.
- Popup 호출은 기존 PopupManager 또는 PopupBase 시스템을 사용한다.

선택/하이라이트 규칙:
- 최하단 버튼을 클릭하여 Popup 창을 호출하면 해당 버튼이 하이라이트된다.
- 해당 Popup 창을 닫으면 버튼은 원래 상태로 돌아간다.
- 한 번에 하나의 버튼만 하이라이트 상태가 되도록 한다.
- 다른 최하단 버튼을 누르면 기존 Popup을 닫거나 선택 상태를 해제한 뒤 새 Popup을 연다.
- Popup이 외부 클릭 또는 닫기 버튼으로 닫혀도 버튼 하이라이트가 해제되어야 한다.

최하단 버튼 하이라이트 애니메이션:
- DOTween Color
  - Duration: 0.3
  - Ease: OutQuad
  - To: FFDA01
- DOTween Scale
  - Duration: 0.3
  - Ease: OutQuad
  - To: 1.1
- 가능한 경우 버튼 오브젝트에 붙어 있는 기존 DOTween Color/Scale 컴포넌트를 재사용한다.
- Popup이 닫힐 때는 Color와 Scale을 원래 값으로 되돌린다.
- 되돌림도 기존 DOTween 컴포넌트의 Rewind 또는 DOPlayBackwards를 우선 사용한다.

해금 표시:
- 해금된 버튼은 정상 아이콘/텍스트 색상과 상호작용 가능 상태로 표시한다.
- 잠긴 버튼은 어둡게 보이거나 비활성화 상태로 표시한다.
- 해금 직후 필요한 경우 Slide 04의 NEW 표시를 붙일 수 있게 한다.
- NEW 표시는 처음 사용하거나 Popup을 확인하면 사라지게 연결할 수 있어야 한다.

구현 구조:
- BottomNavController 컴포넌트를 만든다.
- BottomNavButton 컴포넌트를 각 버튼에 붙인다.
- BottomNavType enum을 만든다.
  예: Status, Inventory, Potion, Equipment, Skill, Codex
- BottomNavButton은 타입, Button, Icon Image, Label Text, 잠금 상태, 선택 상태, 연결 Popup ID를 가진다.
- BottomNavController는 현재 선택된 버튼과 열린 Popup을 관리한다.
- UnlockSystem 또는 QuestProgress 변경 이벤트를 구독해 버튼 해금 상태를 갱신한다.
- Popup이 닫힐 때 BottomNavController가 선택 상태를 해제할 수 있도록 Popup Close 이벤트를 연결한다.

완료 기준:
- 화면 최하단에 6개 버튼이 지정 순서로 표시된다.
- 게임 시작 시 [상태] 버튼만 클릭 가능하다.
- 퀘스트 진행도에 따라 나머지 버튼이 순차 해금된다.
- 버튼 클릭 시 연결된 Popup이 열린다.
- Popup이 열린 동안 해당 버튼은 FFDA01 색상과 1.1 Scale로 하이라이트된다.
- Popup을 닫으면 버튼 하이라이트가 원래 상태로 돌아간다.
- 한 번에 하나의 최하단 버튼만 선택 상태가 된다.
```

## 텍스트 설명만 사용하는 버전

```text
RE001의 최하단 버튼 UI를 Unity에서 구현해줘.

구현 목적:
- 화면 최하단에 주요 메뉴 Popup을 여는 6개 버튼을 배치한다.
- 버튼은 퀘스트 진행도에 따라 순차적으로 해금된다.
- 버튼을 누르면 연결된 Popup이 열리고, 열린 동안 해당 버튼이 하이라이트된다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

버튼 종류와 순서:
- 최하단 버튼은 총 6개다.
- 왼쪽부터 다음 순서로 배치한다.
  1. 상태
  2. 가방
  3. 물약
  4. 장비
  5. 기술
  6. 도감
- 각 버튼은 아이콘과 텍스트 라벨을 가진다.
- 화면 최하단에 가로로 정렬한다.

해금 규칙:
- 게임 첫 시작 시 [상태] 버튼만 해금되어 있다.
- 나머지 버튼은 퀘스트 진행도에 따라 순차적으로 해금된다.
- 잠긴 버튼은 클릭할 수 없다.
- 버튼별 해금 조건은 데이터로 분리해서 관리한다.
- QuestProgress 또는 UnlockSystem 변경 이벤트가 발생하면 버튼 상태를 갱신한다.

버튼 클릭 동작:
- 각 버튼은 연결된 Popup을 호출한다.
- 상태 버튼은 상태정보 Popup을 연다.
- 가방 버튼은 소지품 Popup을 연다.
- 물약 버튼은 물약창 Popup을 연다.
- 장비 버튼은 장비 Popup을 연다.
- 기술 버튼은 기술 Popup을 연다.
- 도감 버튼은 도감 Popup을 연다.
- Popup 호출은 기존 PopupManager 또는 PopupBase 시스템을 사용한다.
- 연결 Popup이 아직 없을 경우 null 참조 오류 없이 처리한다.

선택 상태:
- 버튼 클릭으로 Popup이 열리면 해당 버튼이 선택 상태가 된다.
- 선택 상태 버튼은 하이라이트 애니메이션을 재생한다.
- Popup이 닫히면 버튼 선택 상태를 해제한다.
- 한 번에 하나의 버튼만 선택 상태가 될 수 있다.
- 다른 버튼을 누르면 기존 선택을 해제하고 새 버튼을 선택한다.
- Popup이 외부 클릭으로 닫혀도 선택 상태가 해제되어야 한다.

하이라이트 애니메이션:
- DOTween Color
  - Duration: 0.3
  - Ease: OutQuad
  - To: FFDA01
- DOTween Scale
  - Duration: 0.3
  - Ease: OutQuad
  - To: 1.1
- 버튼에 같은 역할의 DOTweenAnimation 컴포넌트가 있으면 해당 컴포넌트를 재사용한다.
- Popup이 닫힐 때는 Rewind 또는 DOPlayBackwards로 원래 색상과 Scale로 되돌린다.

NEW 표시 연동:
- 버튼이 새로 해금되었을 때 Slide 04의 NEW 표시를 붙일 수 있어야 한다.
- NEW 표시는 해당 버튼을 누르거나 연결 Popup을 확인하면 사라지게 한다.

구현 방식:
- BottomNavController 컴포넌트를 만든다.
- 각 버튼에는 BottomNavButton 컴포넌트를 붙인다.
- BottomNavType enum을 만든다.
  예: Status, Inventory, Potion, Equipment, Skill, Codex
- BottomNavButton은 타입, Button, Icon, Label, 연결 Popup ID, 잠금 상태, 선택 상태를 가진다.
- BottomNavController는 현재 선택된 버튼과 열린 Popup을 관리한다.
- Popup Close 이벤트를 받아 선택 상태를 해제한다.
- UnlockSystem 또는 QuestProgress와 연결해 해금 상태를 갱신한다.

완료 기준:
- 최하단에 상태, 가방, 물약, 장비, 기술, 도감 버튼이 순서대로 표시된다.
- 시작 시 상태 버튼만 클릭 가능하다.
- 퀘스트 진행에 따라 버튼이 순차 해금된다.
- 버튼 클릭 시 연결 Popup이 열린다.
- 열린 Popup에 해당하는 버튼은 FFDA01 색상과 1.1 Scale로 하이라이트된다.
- Popup이 닫히면 버튼은 원래 상태로 돌아간다.
- 한 번에 하나의 버튼만 선택 상태가 된다.
```
