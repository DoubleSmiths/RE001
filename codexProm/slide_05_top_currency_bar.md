# Slide 05 - 상단 재화 BAR

## 이미지 정보 이용 버전

```text
PPT 5번 슬라이드 이미지를 기준으로 RE001 상단 재화 BAR UI를 구현해줘.

참조 이미지:
C:\stopresent\Unity\RE001\tmp\ppt_slides\슬라이드5.PNG

구현 목적:
- 플레이어가 보유한 재화량을 상단 BAR에서 항상 확인할 수 있게 한다.
- 금화와 결정 수량을 표시하고, 우측 환경설정 버튼을 통해 환경설정 Popup을 호출한다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

사용 UI 리소스:
- BAR 배경: 1_basic_button
- 금화 아이콘: Icon38
- 결정 아이콘: Icon25
- 환경설정 버튼 아이콘/프레임: 1_armor

레이아웃:
- 화면 상단에 가로로 긴 재화 BAR를 배치한다.
- BAR 왼쪽부터 금화 아이콘, 금화 수량 텍스트, 결정 아이콘, 결정 수량 텍스트 순서로 배치한다.
- BAR 오른쪽 끝에는 환경설정 버튼을 배치한다.
- PPT 예시처럼 인게임 적용 모습은 어두운 갈색/검정 계열의 긴 픽셀 UI 바 위에 재화 아이콘과 숫자가 올라간 형태다.
- 환경설정 버튼은 BAR 우측 끝에 붙어 있으며, 별도 버튼처럼 클릭 가능해야 한다.

금화 TEXT:
- Font: PF_Stardust 고정
- Color: FFFFFF
- Outline: FFF600, alpha 80, x 0.2, y -0.2
- 언어 설정과 무관하게 폰트는 고정한다.
- 수량은 정수로 표시한다.

결정 TEXT:
- Font: PF_Stardust 고정
- Color: 00F3FF
- Outline: FFF600, alpha 80, x 0.2, y -0.2
- 언어 설정과 무관하게 폰트는 고정한다.
- 수량은 정수로 표시한다.

재화량 갱신:
- Amount가 변화하면 Text 수치를 즉시 바꾸지 말고 Lerp 방식으로 부드럽게 갱신한다.
- 증가 시 해당 재화 텍스트가 잠깐 초록색 00FF00으로 빛난다.
- 감소 시 해당 재화 텍스트가 잠깐 빨간색 FF0000으로 빛난다.
- 색상 강조가 끝나면 원래 색상으로 복귀한다.
- 금화와 결정 각각 독립적으로 갱신되어야 한다.
- 빠르게 연속 변경될 경우 현재 표시값에서 새 목표값으로 자연스럽게 이어지도록 처리한다.

환경설정 버튼:
- 클릭 시 환경설정 Popup을 호출한다.
- 클릭 시 버튼 애니메이션을 재생한다.

환경설정 버튼 클릭 애니메이션:
- DOTween Scale 애니메이션 사용
- Duration: 0.07
- Delay: 0
- Ease: OutQuad
- Loops: 2
- Loop Type: Yoyo
- To: 0.75 Uniform Scale
- 가능한 경우 버튼 오브젝트에 붙어 있는 동일 역할의 DOTweenAnimation 컴포넌트를 사용해 재생한다.

구현 구조:
- CurrencyTopBarController를 만든다.
- 금화와 결정 각각의 TMP_Text, Image 아이콘, 현재 표시값, 목표값을 관리한다.
- CurrencyType enum을 둔다.
  예: Gold, Crystal
- SetAmount(CurrencyType type, int amount) 또는 Refresh(CurrencyWallet wallet) API를 제공한다.
- 수량 변경 이벤트를 구독해 BAR를 자동 갱신한다.
- 환경설정 버튼은 Button.onClick에서 설정 Popup Manager를 호출한다.
- 설정 Popup 호출 로직은 직접 생성보다 기존 Popup 시스템이 있다면 그 시스템을 사용한다.

완료 기준:
- 상단 BAR에 금화와 결정 아이콘 및 수량이 표시된다.
- 금화 텍스트는 흰색, 결정 텍스트는 청록색으로 표시된다.
- 두 재화 텍스트 모두 PF_Stardust 폰트와 노란색 outline을 사용한다.
- 재화량 증가/감소 시 Lerp로 숫자가 갱신되고 색상이 초록/빨강으로 잠깐 강조된다.
- 환경설정 버튼 클릭 시 Scale Yoyo 애니메이션이 재생되고 환경설정 Popup이 열린다.
```

## 텍스트 설명만 사용하는 버전

```text
RE001의 상단 재화 BAR UI를 Unity에서 구현해줘.

구현 목적:
- 플레이어가 보유한 금화와 결정을 화면 상단에서 확인할 수 있게 한다.
- 재화량 변화가 있을 때 숫자를 부드럽게 갱신하고 증가/감소 상태를 색으로 피드백한다.
- 우측 환경설정 버튼을 통해 환경설정 Popup을 호출한다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

UI 구성:
- 화면 상단에 긴 가로 BAR를 배치한다.
- BAR에는 금화 아이콘, 금화 수량, 결정 아이콘, 결정 수량, 환경설정 버튼이 포함된다.
- 금화 아이콘 리소스는 Icon38을 사용한다.
- 결정 아이콘 리소스는 Icon25를 사용한다.
- BAR 배경 리소스는 1_basic_button을 사용한다.
- 환경설정 버튼에는 1_armor 리소스를 사용한다.

금화 텍스트:
- Font는 PF_Stardust로 고정한다.
- 텍스트 색상은 FFFFFF로 설정한다.
- Outline은 FFF600, alpha 80, x 0.2, y -0.2로 설정한다.
- 언어 설정과 무관하게 항상 같은 폰트를 사용한다.

결정 텍스트:
- Font는 PF_Stardust로 고정한다.
- 텍스트 색상은 00F3FF로 설정한다.
- Outline은 FFF600, alpha 80, x 0.2, y -0.2로 설정한다.
- 언어 설정과 무관하게 항상 같은 폰트를 사용한다.

재화량 변경 처리:
- 재화량이 변경되면 표시 숫자를 Lerp로 부드럽게 갱신한다.
- 증가한 경우 해당 텍스트를 잠깐 00FF00 색상으로 강조한다.
- 감소한 경우 해당 텍스트를 잠깐 FF0000 색상으로 강조한다.
- 강조가 끝나면 금화는 FFFFFF, 결정은 00F3FF로 되돌린다.
- 금화와 결정은 각각 독립적으로 갱신되어야 한다.
- 변경이 연속으로 들어오면 현재 표시값에서 새 목표값으로 자연스럽게 이어지게 한다.

환경설정 버튼:
- 클릭하면 환경설정 Popup을 호출한다.
- 클릭 시 버튼 Scale 애니메이션을 재생한다.
- 애니메이션 설정:
  - DOTween Scale
  - Duration: 0.07
  - Delay: 0
  - Ease: OutQuad
  - Loops: 2
  - Loop Type: Yoyo
  - To: 0.75 Uniform Scale
- 같은 역할의 DOTweenAnimation 컴포넌트가 버튼에 이미 있다면 해당 컴포넌트를 재사용해 재생한다.

구현 방식:
- CurrencyTopBarController 컴포넌트를 만든다.
- Gold TMP_Text, Crystal TMP_Text, 환경설정 Button 참조를 가진다.
- CurrencyType enum을 만든다.
  예: Gold, Crystal
- SetAmount(CurrencyType type, int value) API를 제공한다.
- 내부적으로 현재 표시값과 목표값을 분리해서 관리한다.
- 재화 데이터 시스템 또는 Wallet 이벤트가 있다면 해당 이벤트를 구독해 자동 갱신한다.
- 설정 Popup은 기존 Popup 시스템이 있다면 그 시스템을 통해 연다.

완료 기준:
- 상단 BAR에 금화와 결정 수량이 표시된다.
- 재화 텍스트는 지정된 폰트, 색상, outline을 사용한다.
- 재화량 변경 시 숫자가 부드럽게 갱신된다.
- 증가 시 초록색, 감소 시 빨간색 피드백이 잠깐 보인다.
- 환경설정 버튼 클릭 시 Scale Yoyo 애니메이션이 재생되고 환경설정 Popup이 열린다.
```
