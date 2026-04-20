# Slide 08 - Popup 시 선행 작업

## 이미지 정보 이용 버전

```text
PPT 8번 슬라이드 이미지를 기준으로 RE001의 모든 Popup에 공통 적용되는 선행 작업 시스템을 구현해줘.

참조 이미지:
C:\stopresent\Unity\RE001\tmp\ppt_slides\슬라이드8.PNG

구현 목적:
- 모든 Popup이 열릴 때 화면 전체 크기의 어두운 BlackBG_Layer를 함께 표시해 플레이어가 Popup에 더 집중할 수 있게 한다.
- BlackBG_Layer를 클릭하면 해당 Popup이 Disable 애니메이션을 재생한 뒤 닫히게 한다.
- Popup마다 동일한 흐름을 재사용할 수 있는 공통 Popup Base 구조를 만든다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

레이어 구조:
- Main Screen 위에 BlackBG_Layer를 생성/활성화한다.
- BlackBG_Layer 위에 Popup_Layer를 생성/활성화한다.
- 구조는 다음 순서를 따른다.
  1. Main Screen
  2. BlackBG_Layer
  3. Popup_Layer
- BlackBG_Layer는 해당 스크린 전체 크기를 덮어야 한다.
- Popup_Layer는 BlackBG_Layer보다 위에 배치되어야 한다.
- Popup에 플레이어가 더 집중할 수 있도록 Popup 외 영역은 어둡게 처리한다.

BlackBG_Layer 설정:
- Sprite: None
- Color: 000000
- Alpha: 220
- 전체 화면 Stretch Anchor로 설정한다.
- Raycast Target을 활성화해 클릭 입력을 받을 수 있게 한다.
- BlackBG_Layer 클릭은 Popup 외부 클릭으로 취급한다.

BlackBG_Layer Fade 애니메이션:
- DOTween Fade 사용
- Duration: 0.3
- Delay: 0
- Ease: OutQuad
- Loops: 1
- From: 0
- OnEnable 시 Restart
- OnDisable 시 Rewind
- 가능하면 BlackBG_Layer에 붙어 있는 동일 역할의 DOTweenAnimation 컴포넌트를 사용한다.

닫기 규칙:
- BlackBG_Layer를 클릭하면 해당 Popup은 Disable 애니메이션을 재생한 후 꺼진다.
- BlackBG_Layer 생성 후 0.5초가 지난 뒤부터 유저 클릭으로 닫을 수 있다.
- 0.5초 이전에는 BlackBG_Layer 클릭을 무시한다.
- 닫기 중복 호출을 막기 위해 closing 상태를 둔다.
- Popup Disable 애니메이션이 끝나기 전까지 같은 Popup에 대한 추가 Close 요청은 무시한다.

구현 구조:
- PopupBase 또는 PopupController 공통 클래스를 만든다.
- PopupBase는 BlackBG_Layer 참조, Popup_Layer 참조, 닫기 가능 딜레이, 닫기 상태를 관리한다.
- Open() 호출 시:
  - BlackBG_Layer 활성화
  - BlackBG_Layer Fade Restart
  - Popup_Layer 활성화
  - Popup별 OnActive 애니메이션 재생
  - 0.5초 뒤 외부 클릭 닫기 허용
- Close() 호출 시:
  - 중복 Close 방지
  - Popup별 Disable 애니메이션 재생
  - BlackBG_Layer Rewind 또는 Fade Out 처리
  - 애니메이션 종료 후 Popup 전체 비활성화
- BlackBG_Layer 클릭 이벤트는 PopupBase.Close()를 호출한다.
- 개별 Popup은 PopupBase를 상속하거나 포함해서 공통 레이어/닫기 규칙을 재사용한다.

상호작용 조건:
- Popup_Layer 내부 클릭은 BlackBG_Layer 클릭으로 처리되면 안 된다.
- Popup_Layer 내부 버튼과 입력 요소는 정상적으로 클릭 가능해야 한다.
- BlackBG_Layer만 클릭했을 때 외부 닫기 동작이 실행된다.
- 필요 시 개별 Popup에서 외부 클릭 닫기를 비활성화할 수 있는 옵션을 둔다.

완료 기준:
- 모든 Popup은 열릴 때 화면 전체를 덮는 검은 반투명 BlackBG_Layer를 가진다.
- BlackBG_Layer는 0.3초 Fade In으로 나타난다.
- Popup이 열린 뒤 0.5초가 지나야 BlackBG_Layer 클릭으로 닫을 수 있다.
- BlackBG_Layer 클릭 시 Popup Disable 애니메이션이 재생된 뒤 Popup이 닫힌다.
- Popup 내부 클릭은 외부 닫기로 오작동하지 않는다.
- 여러 Popup에서 같은 PopupBase/BlackBG_Layer 규칙을 재사용할 수 있다.
```

## 텍스트 설명만 사용하는 버전

```text
RE001의 모든 Popup에 공통으로 적용되는 Popup 선행 작업 시스템을 Unity에서 구현해줘.

구현 목적:
- Popup이 열릴 때 전체 화면을 덮는 검은 반투명 배경 레이어를 함께 표시한다.
- 배경 레이어를 클릭하면 Popup이 닫히도록 한다.
- 모든 Popup에서 동일한 배경, Fade, 외부 클릭 닫기 규칙을 재사용한다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

레이어 구조:
- 모든 Popup은 Main Screen 위에 BlackBG_Layer를 가진다.
- BlackBG_Layer 위에 Popup_Layer가 배치된다.
- 정렬 순서는 Main Screen, BlackBG_Layer, Popup_Layer 순서다.
- BlackBG_Layer는 해당 스크린 전체 크기를 덮는다.

BlackBG_Layer:
- Sprite는 사용하지 않는다.
- Color는 000000으로 설정한다.
- Alpha는 220으로 설정한다.
- 전체 화면 Stretch Anchor를 사용한다.
- Raycast Target을 켜서 클릭 입력을 받을 수 있게 한다.

BlackBG_Layer 애니메이션:
- DOTween Fade를 사용한다.
- Duration: 0.3
- Delay: 0
- Ease: OutQuad
- Loops: 1
- From: 0
- OnEnable 시 Restart한다.
- OnDisable 시 Rewind한다.
- 같은 역할의 DOTweenAnimation 컴포넌트가 이미 있으면 해당 컴포넌트를 재사용한다.

닫기 규칙:
- BlackBG_Layer를 클릭하면 해당 Popup을 닫는다.
- Popup은 즉시 꺼지지 않고 Disable 애니메이션을 재생한 뒤 닫힌다.
- BlackBG_Layer가 생성/활성화된 뒤 0.5초가 지나야 클릭으로 닫을 수 있다.
- 0.5초 이전의 BlackBG_Layer 클릭은 무시한다.
- 닫기 중복 실행을 막기 위해 closing 상태를 관리한다.

구현 방식:
- PopupBase 또는 PopupController 공통 클래스를 만든다.
- PopupBase는 BlackBG_Layer, Popup_Layer, 외부 클릭 닫기 가능 여부, 닫기 가능 딜레이, closing 상태를 관리한다.
- Open()에서 BlackBG_Layer와 Popup_Layer를 활성화한다.
- Open()에서 BlackBG_Layer Fade In 애니메이션을 재생한다.
- Open()에서 Popup별 OnActive 애니메이션을 재생한다.
- Open() 후 0.5초 뒤 외부 클릭 닫기를 허용한다.
- Close()에서 Popup별 Disable 애니메이션을 재생한다.
- Close()에서 BlackBG_Layer를 Rewind 또는 Fade Out 처리한다.
- Close() 애니메이션 종료 후 Popup 전체를 비활성화한다.
- BlackBG_Layer 클릭 이벤트는 PopupBase.Close()를 호출한다.
- 개별 Popup은 이 공통 구조를 상속하거나 포함해서 사용한다.

상호작용 조건:
- Popup 내부 영역을 클릭했을 때는 BlackBG_Layer 클릭으로 처리하면 안 된다.
- Popup 내부 버튼, 슬롯, 입력 요소는 정상적으로 동작해야 한다.
- BlackBG_Layer만 클릭했을 때 외부 닫기가 실행된다.
- 특정 Popup은 옵션으로 외부 클릭 닫기를 비활성화할 수 있어야 한다.

완료 기준:
- Popup이 열리면 전체 화면 검은 반투명 배경이 Fade In 된다.
- Popup은 BlackBG_Layer 위에 표시된다.
- Popup이 열린 뒤 0.5초가 지나야 배경 클릭으로 닫을 수 있다.
- 배경 클릭 시 Popup Disable 애니메이션 후 닫힌다.
- Popup 내부 클릭은 닫기 이벤트로 오작동하지 않는다.
- 모든 Popup에서 이 공통 구조를 재사용할 수 있다.
```
