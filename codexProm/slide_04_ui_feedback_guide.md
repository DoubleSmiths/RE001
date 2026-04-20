# Slide 04 - UI Feedback Guide

## 이미지 정보 이용 버전

```text
PPT 4번 슬라이드 이미지를 기준으로 RE001에서 여러 UI에 공통으로 쓰이는 피드백 표시 시스템을 구현해줘.

참조 이미지:
C:\stopresent\Unity\RE001\tmp\ppt_slides\슬라이드4.PNG

구현 목적:
- 아이템/기능 신규 획득을 알리는 NEW 표시와, 퀘스트 목적지를 안내하는 [ ! ] 표시를 공통 UI 피드백 컴포넌트로 구현한다.
- 각 UI 기획서에서 지정한 위치에 붙일 수 있도록 재사용 가능한 Prefab/Component 구조로 만든다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

구현 대상:
1. NEW 버튼/텍스트 표시
2. [ ! ] 버튼/아이콘 표시

1. NEW 표시 요구사항:
- 아이템 또는 기능이 새로 생겼을 때 표시한다.
- 표시 위치는 각 UI 기획서에서 지정한 위치를 따른다.
- 해당 버튼을 누르거나 해당 Popup 창을 끄면 NEW 표시는 사라진다.
- 텍스트는 "NEW"로 표시한다.
- Font: DOSSaemmul
- Color: FFFF00
- 시각적으로 PPT 예시처럼 노란색 픽셀 텍스트 느낌이 나야 한다.

NEW 표시 애니메이션:
- DOTween Move 애니메이션을 적용한다.
- Duration: 0.5
- Ease: OutQuad
- Loops: -1
- Loop Type: Restart
- To: x 1, y 2, z 0
- Snapping 활성화
- OnEnable 시 애니메이션을 시작하고, OnDisable 시 정리한다.
- 기준 위치에서 작게 튀는 느낌으로 반복 이동한다.

2. [ ! ] 표시 요구사항:
- 메인/서브 퀘스트 가이드 UI로 사용한다.
- 다음 행선지 또는 목표가 있는 UI에 느낌표를 표시한다.
- 표시 위치는 각 UI 기획서에서 지정한 위치를 따른다.
- 퀘스트 번호에 맞춰 표시/비표시가 전환되어야 한다.
- 이미지 리소스는 gui_atlas의 exclamation_mark01을 사용한다.
- 색상은 상황에 따라 FFE300 또는 14FF00을 사용할 수 있게 한다.
- PPT 예시처럼 노란 느낌표 아이콘이 버튼 또는 아이템 슬롯 위에 붙는 구조여야 한다.

[ ! ] 표시 애니메이션:
- DOTween Punch/Rotation 애니메이션을 적용한다.
- Duration: 1.5
- Ease: OutQuad
- Loops: -1
- Loop Type: Restart
- To: x 0, y 0, z 15
- Vibrato: 10
- Elasticity: 0
- OnEnable 시 반복 회전 흔들림을 시작하고, OnDisable 시 정리한다.

구현 구조:
- UIFeedbackMarker 같은 공통 컴포넌트를 만든다.
- MarkerType enum을 둔다.
  예: New, Exclamation
- MarkerType.New는 NEW 텍스트와 Move 애니메이션을 사용한다.
- MarkerType.Exclamation은 느낌표 이미지와 Punch Rotation 애니메이션을 사용한다.
- 각 Marker는 Show(), Hide(), Clear() 같은 API를 제공한다.
- Popup이 닫히거나 해당 버튼이 눌렸을 때 Clear()를 호출해 표시를 제거한다.
- Quest 번호와 연결되는 경우 QuestProgressManager에서 현재 퀘스트 번호를 기준으로 Show/Hide를 제어할 수 있게 한다.
- 여러 UI에서 같은 규칙으로 사용할 수 있도록 Prefab화한다.

완료 기준:
- 신규 아이템 또는 신규 기능 상태를 부여하면 해당 UI 위치에 NEW 표시가 나타난다.
- 해당 UI 버튼을 누르거나 Popup을 닫으면 NEW 표시가 사라진다.
- 퀘스트 번호 조건에 맞는 UI에만 [ ! ] 표시가 나타난다.
- NEW 표시는 위아래로 작게 반복 이동한다.
- [ ! ] 표시는 회전으로 흔들리는 느낌을 반복한다.
- 두 표시 모두 다른 UI 프리팹에 쉽게 붙여 재사용할 수 있다.
```

## 텍스트 설명만 사용하는 버전

```text
RE001의 공통 UI 피드백 표시 시스템을 Unity에서 구현해줘.

구현 목적:
- 여러 UI에서 반복적으로 사용하는 NEW 표시와 퀘스트 느낌표 표시를 공통 컴포넌트/프리팹으로 만든다.
- 각 UI 기획서에서 지정한 위치에 배치할 수 있고, 상태 변화에 따라 표시/비표시가 자동으로 제어되게 한다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

구현 대상:
1. NEW 표시
2. 퀘스트 [ ! ] 표시

NEW 표시:
- 아이템 또는 기능이 새로 생겼을 때 표시한다.
- 텍스트는 "NEW"로 표시한다.
- Font는 DOSSaemmul을 사용한다.
- 색상은 FFFF00을 사용한다.
- 해당 버튼을 누르거나 해당 Popup 창을 닫으면 사라진다.
- 표시 위치는 개별 UI 기획서에서 지정한 위치를 따른다.

NEW 애니메이션:
- DOTween Move를 사용한다.
- Duration: 0.5
- Ease: OutQuad
- Loops: -1
- Loop Type: Restart
- To: x 1, y 2, z 0
- Snapping을 활성화한다.
- OnEnable 시 애니메이션을 시작하고 OnDisable 시 애니메이션을 정리한다.

퀘스트 [ ! ] 표시:
- 메인/서브 퀘스트 가이드 UI로 사용한다.
- 다음 행선지 또는 목표가 있는 UI에 표시한다.
- 퀘스트 번호에 맞춰 켜지고, 조건에 맞지 않으면 꺼진다.
- 이미지 리소스는 gui_atlas의 exclamation_mark01을 사용한다.
- 색상은 FFE300 또는 14FF00을 지원한다.
- 표시 위치는 개별 UI 기획서에서 지정한 위치를 따른다.

[ ! ] 애니메이션:
- DOTween Punch/Rotation을 사용한다.
- Duration: 1.5
- Ease: OutQuad
- Loops: -1
- Loop Type: Restart
- To: x 0, y 0, z 15
- Vibrato: 10
- Elasticity: 0
- OnEnable 시 반복 회전 흔들림을 시작하고 OnDisable 시 애니메이션을 정리한다.

구현 방식:
- UIFeedbackMarker 컴포넌트를 만든다.
- MarkerType enum을 만든다.
  예: New, Exclamation
- MarkerType.New는 TMP_Text 기반 NEW 텍스트를 사용한다.
- MarkerType.Exclamation은 Image 기반 느낌표 아이콘을 사용한다.
- Show(), Hide(), Clear() API를 제공한다.
- Clear()는 사용자 확인이 끝난 상태로 처리하고 표시를 끈다.
- Popup이 닫히거나 대상 버튼이 눌렸을 때 Clear()를 호출할 수 있게 한다.
- 퀘스트 느낌표는 QuestProgressManager 또는 유사한 진행도 시스템과 연결해서 현재 퀘스트 번호 기준으로 Show/Hide한다.
- Prefab으로 만들어 여러 UI에 재사용할 수 있게 한다.

완료 기준:
- 신규 아이템/기능이 생기면 대상 UI에 NEW 표시가 나타난다.
- 버튼을 누르거나 Popup을 닫으면 NEW 표시가 사라진다.
- 현재 퀘스트 번호에 해당하는 UI에만 느낌표가 표시된다.
- NEW 표시는 작은 이동 애니메이션을 반복한다.
- 느낌표는 회전 흔들림 애니메이션을 반복한다.
- 두 피드백 모두 여러 UI 프리팹에서 공통으로 재사용 가능하다.
```
