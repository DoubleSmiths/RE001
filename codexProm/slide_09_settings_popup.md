# Slide 09 - 환경설정 Popup

## 이미지 정보 이용 버전

```text
PPT 9번 슬라이드 이미지를 기준으로 RE001 환경설정 Popup을 구현해줘.

참조 이미지:
C:\stopresent\Unity\RE001\tmp\ppt_slides\슬라이드9.PNG

구현 목적:
- 기본적인 게임 설정을 변경할 수 있는 환경설정 Popup 창을 만든다.
- 언어, 소리, 화면효과 관련 설정을 토글/버튼으로 조작할 수 있게 한다.
- Slide 08의 Popup 공통 선행 작업 구조 위에서 동작하게 한다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

사용 UI 리소스:
- Popup 프레임: Frame_square_256
- 붉은 글로우: Lobby_CharacterInfo_RedGlow
- 배경: message_popup_bg_default
- 확인/체크 버튼: Mythic_Button_ (29)
- 닫기/X 버튼: Mythic_Button_ (28)
- 선택/강조 Glow: fx_glow
- fx_glow 색상:
  - 활성/선택: 14FF00
  - 강조/효과용: C53F2A

Popup 레이어:
- Slide 08의 PopupBase 또는 PopupController를 사용한다.
- Popup이 열릴 때 BlackBG_Layer가 함께 표시되어야 한다.
- Popup 외부 BlackBG_Layer 클릭 시 Disable 애니메이션 후 닫힌다.
- Popup 내부 클릭은 외부 닫기로 처리되지 않아야 한다.

Open & Close 애니메이션:
- Popup Anim 정보:
  - [OnActive]
  - DOTween Move
  - Duration: 0.5
  - Delay: 0
  - Ease: OutBack
  - Loops: 1
  - From: x 0, y 350, z 0
  - [OnDisable]
  - Rewind
- 가능하면 Popup 루트 오브젝트의 기존 DOTween Move 컴포넌트를 재사용한다.
- Open 시 Restart, Close 시 Rewind 또는 DOPlayBackwards를 호출한다.

UI 구성:
- Popup 상단에는 "SETTING" 제목을 표시한다.
- 첫 섹션은 언어설정이다.
- 언어 선택 항목으로 "한국어"와 "ENG"를 표시한다.
- 두 번째 섹션은 소리다.
- 소리 항목은 "배경음", "효과음"을 가진다.
- 세 번째 섹션은 화면효과다.
- 화면효과 항목은 "피해숫자 생성", "화면 흔들림"을 가진다.
- 각 설정 항목 오른쪽에는 상태 버튼을 배치한다.
- 활성 상태는 체크 아이콘/초록 계열 버튼으로 표시하고, 비활성 상태는 X 아이콘/회색 계열 버튼으로 표시한다.

언어 변경:
- 언어 버튼 클릭 시 선택 언어를 변경한다.
- 한국어 선택 시 한국어 버튼 쪽 fx_glow를 On하고 ENG 쪽 glow는 Off한다.
- ENG 선택 시 ENG 버튼 쪽 fx_glow를 On하고 한국어 쪽 glow는 Off한다.
- 언어 전환 시 Font Policy와 Localization 시스템에 변경 이벤트를 전달한다.
- 환경설정 Popup의 언어 선택 텍스트는 Slide 03 예외 규칙에 따라 고정 표기한다.
- 언어 변경 상태는 저장되어 재접속/재시작 후에도 유지되어야 한다.

Mythic Button 클릭 시 Sprite Change:
- Mythic Button 클릭 시 상태에 맞게 Sprite를 변경한다.
- On 상태는 Mythic_Button_ (29) 또는 체크 상태 Sprite를 사용한다.
- Off 상태는 Mythic_Button_ (28) 또는 X 상태 Sprite를 사용한다.
- 클릭한 버튼은 Button Click 애니메이션을 재생한다.

Button Anim:
- [OnClick]
- DOTween Scale
- Duration: 0.07
- Delay: 0
- Ease: OutQuad
- Loops: 2
- Loop Type: Yoyo
- To: 0.8
- 같은 역할의 DOTweenAnimation 컴포넌트가 있으면 해당 컴포넌트를 재사용한다.

설정 데이터:
- SettingsData 또는 GameSettings 모델을 만든다.
- 저장 항목:
  - Language: Korean 또는 English
  - BGM Enabled
  - SFX Enabled
  - DamageNumber Enabled
  - ScreenShake Enabled
- 변경 즉시 메모리 상태를 갱신하고 저장한다.
- 기존 저장 시스템이 있다면 그 시스템을 사용한다.
- 없다면 PlayerPrefs 또는 프로젝트의 설정 저장 방식에 맞춰 최소 구현한다.

구현 구조:
- SettingsPopupController를 만든다.
- PopupBase를 상속하거나 포함한다.
- 각 설정 버튼은 ToggleSetting(settingKey) 방식으로 처리한다.
- RefreshView()에서 현재 설정값을 기준으로 텍스트, 버튼 Sprite, fx_glow On/Off를 갱신한다.
- Open 시 저장된 설정값을 읽고 RefreshView()를 호출한다.
- 닫기 버튼 또는 BlackBG_Layer 클릭 시 PopupBase.Close()를 호출한다.

완료 기준:
- 상단 재화 BAR의 환경설정 버튼을 누르면 환경설정 Popup이 열린다.
- Popup은 y 350 위치에서 내려오는 OutBack Move 애니메이션으로 열린다.
- BlackBG_Layer와 함께 표시되고 외부 클릭으로 닫을 수 있다.
- 한국어/ENG 선택 시 fx_glow가 선택 항목에만 표시된다.
- BGM, SFX, 피해숫자 생성, 화면 흔들림 설정을 On/Off 할 수 있다.
- 각 버튼 클릭 시 Scale Yoyo 애니메이션과 Sprite Change가 적용된다.
- 설정 변경 값이 저장되고 다시 열었을 때 유지된다.
```

## 텍스트 설명만 사용하는 버전

```text
RE001의 환경설정 Popup을 Unity에서 구현해줘.

구현 목적:
- 게임의 기본 설정을 변경할 수 있는 Popup을 만든다.
- 설정 항목은 언어, 배경음, 효과음, 피해숫자 생성, 화면 흔들림이다.
- Popup은 공통 PopupBase 구조와 BlackBG_Layer 규칙을 사용한다.

DOTween 구현 공통 규칙:
- DOTween 애니메이션을 구현할 때 해당 오브젝트에 같은 역할을 하는 기존 DOTween 컴포넌트가 있다면 그 컴포넌트를 우선 사용한다.
- 유지보수를 위해 코드에서 Sequence를 직접 조립하는 방식은 가급적 피한다.
- 필요한 경우 기존 DOTweenAnimation/DOTween 관련 컴포넌트 설정값을 코드에서 Play, Restart, Rewind, DOPlayBackwards 하는 방식으로 제어한다.
- 동일 역할의 컴포넌트가 없을 때만 최소 범위에서 코드 기반 Tween을 작성한다.

Popup 공통 규칙:
- Slide 08의 PopupBase 또는 PopupController를 사용한다.
- Popup이 열릴 때 BlackBG_Layer를 함께 표시한다.
- BlackBG_Layer 클릭 시 Disable 애니메이션 후 Popup을 닫는다.
- Popup 내부 클릭은 외부 닫기로 처리하지 않는다.

사용 리소스:
- Popup 프레임: Frame_square_256
- 붉은 글로우: Lobby_CharacterInfo_RedGlow
- Popup 배경: message_popup_bg_default
- 체크/확인 버튼: Mythic_Button_ (29)
- X/비활성 버튼: Mythic_Button_ (28)
- 선택 Glow: fx_glow
- 선택 Glow 색상은 14FF00을 사용한다.
- 강조 효과 Glow 색상은 C53F2A를 사용할 수 있게 한다.

Open & Close 애니메이션:
- Popup 루트에 DOTween Move 애니메이션을 사용한다.
- OnActive:
  - Duration: 0.5
  - Delay: 0
  - Ease: OutBack
  - Loops: 1
  - From: x 0, y 350, z 0
- OnDisable:
  - Rewind
- 같은 역할의 DOTweenAnimation 컴포넌트가 이미 있으면 그 컴포넌트를 재사용한다.

UI 구성:
- 상단 제목은 "SETTING"으로 표시한다.
- 언어설정 섹션에 "한국어", "ENG" 선택 버튼을 둔다.
- 소리 섹션에 "배경음", "효과음" On/Off 버튼을 둔다.
- 화면효과 섹션에 "피해숫자 생성", "화면 흔들림" On/Off 버튼을 둔다.
- 각 설정 버튼은 현재 상태에 따라 체크 또는 X Sprite를 표시한다.

언어 설정:
- 한국어를 선택하면 한국어 항목의 fx_glow를 켜고 ENG 항목의 fx_glow를 끈다.
- ENG를 선택하면 ENG 항목의 fx_glow를 켜고 한국어 항목의 fx_glow를 끈다.
- 언어 변경 시 Localization 시스템과 Font Policy에 변경 이벤트를 전달한다.
- 언어 선택 텍스트는 언어 설정과 무관하게 고정 표기한다.
- 선택 언어는 저장되어야 한다.

On/Off 설정:
- 배경음, 효과음, 피해숫자 생성, 화면 흔들림은 각각 독립적으로 On/Off 가능해야 한다.
- On 상태는 체크 Sprite를 사용한다.
- Off 상태는 X Sprite를 사용한다.
- 버튼 클릭 시 Sprite를 즉시 갱신한다.
- 설정 변경 값은 저장되어 다시 Popup을 열었을 때 유지되어야 한다.

Button 클릭 애니메이션:
- DOTween Scale 사용
- Duration: 0.07
- Delay: 0
- Ease: OutQuad
- Loops: 2
- Loop Type: Yoyo
- To: 0.8
- 같은 역할의 DOTweenAnimation 컴포넌트가 버튼에 있으면 해당 컴포넌트를 재사용한다.

구현 방식:
- SettingsPopupController 컴포넌트를 만든다.
- PopupBase를 상속하거나 포함한다.
- GameSettings 또는 SettingsData 모델을 만든다.
- 저장 항목:
  - Language
  - BGM Enabled
  - SFX Enabled
  - DamageNumber Enabled
  - ScreenShake Enabled
- Open 시 저장된 설정을 읽고 RefreshView()를 호출한다.
- 각 버튼 클릭 시 설정값 변경, 저장, RefreshView(), 버튼 애니메이션 재생을 수행한다.
- 설정 저장은 기존 저장 시스템이 있으면 해당 시스템을 사용하고, 없으면 PlayerPrefs로 최소 구현한다.

완료 기준:
- 환경설정 버튼으로 Popup을 열 수 있다.
- Popup은 BlackBG_Layer와 함께 표시된다.
- Popup은 y 350에서 내려오는 OutBack Move 애니메이션으로 열린다.
- 외부 클릭 또는 닫기 버튼으로 닫을 수 있다.
- 언어 선택 시 선택 항목에만 fx_glow가 표시된다.
- 네 개의 On/Off 설정은 각각 독립적으로 토글된다.
- 버튼 클릭 시 Scale Yoyo 애니메이션과 Sprite Change가 적용된다.
- 설정 값은 저장되고 다시 열었을 때 유지된다.
```
