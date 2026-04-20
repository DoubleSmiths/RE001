# Slide 03 - Font 설정

## 이미지 정보 이용 버전

```text
PPT 3번 슬라이드 이미지를 기준으로 RE001 인게임 폰트 설정 시스템을 구현해줘.

참조 이미지:
C:\stopresent\Unity\RE001\tmp\ppt_slides\슬라이드3.PNG

구현 목적:
- 게임 내 텍스트 종류와 언어 설정에 따라 사용할 폰트를 일관되게 분기하는 Font Policy를 만든다.
- 모든 UI Text 생성/갱신 시 이 정책을 통해 폰트, 숫자 폰트, 아웃라인 규칙이 자동 적용되도록 한다.

구현 요구사항:
1. 인게임에서 사용하는 폰트는 다음 3종이다.
   - Cafe24
   - DOSSaemmul
   - PF_Stardust

2. Cafe24 사용처:
   - Dialogue Texts 전용.
   - NPC와 대화할 때 출력되는 대사 텍스트에 사용한다.
   - 한국어/영어 언어 선택과 무관하게 동일하게 사용한다.

3. DOSSaemmul 사용처:
   - 언어 선택이 영어일 때 사용한다.
   - Dialogue Texts와 숫자를 제외한 모든 텍스트에 사용한다.
   - Menu명, Item명, Description 등 UI 일반 텍스트에 적용한다.

4. PF_Stardust 사용처:
   - 언어 선택이 한국어일 때 사용한다.
   - Dialogue Texts와 숫자를 제외한 모든 텍스트에 사용한다.
   - Menu명, Item명, Description 등 UI 일반 텍스트에 적용한다.

5. 숫자 텍스트:
   - INGAME의 모든 숫자 TEXT는 언어 선택과 무관하게 지정된 숫자 폰트로 고정한다.
   - 수량, 수치, 레벨, 퍼센트, 재화량 등 숫자 중심 텍스트는 일반 언어 폰트 변경의 영향을 받지 않게 한다.

6. Outline 기본 규칙:
   - 모든 TEXT는 특별한 언급이 없으면 검은색 OUTLINE을 적용한다.
   - 특정 텍스트가 다른 색상의 OUTLINE을 사용해야 하는 경우에는 각 UI 기획서의 개별 명시를 우선한다.
   - TextMeshPro 사용 시 Outline Color, Outline Width 또는 Material Preset으로 관리한다.

7. 예외 처리:
   - Player Bar의 Level 정보와 모험가 등급은 언어 선택과 무관하게 DOSSaemmul 폰트로 고정한다.
   - Player Bar의 Level 정보는 항상 "LV.#" 형식으로 표기한다.
   - 환경설정 Popup의 언어 선택 부분도 언어 선택과 무관하게 고정 표기한다.

8. 구현 구조:
   - TextType 또는 FontUsage enum을 만든다.
     예: Dialogue, General, Number, PlayerLevel, PlayerRank, LanguageSetting
   - 현재 언어 상태를 기준으로 General 텍스트 폰트만 DOSSaemmul 또는 PF_Stardust로 변경한다.
   - Dialogue, Number, PlayerLevel, PlayerRank, LanguageSetting은 언어 변경에 의해 폰트가 바뀌지 않도록 한다.
   - UI Text 컴포넌트가 Awake/OnEnable 또는 언어 변경 이벤트 발생 시 Font Policy를 다시 적용받게 한다.

9. 에디터/프리팹 적용:
   - 각 TMP_Text에 직접 폰트를 산발적으로 지정하지 않고, 공통 컴포넌트 또는 매니저를 통해 적용한다.
   - 기존 UI 프리팹에 붙일 수 있는 FontBinder 같은 컴포넌트를 제공한다.
   - Inspector에서 TextType을 선택하면 자동으로 알맞은 폰트와 기본 아웃라인을 적용하도록 한다.

10. 시각 기준:
   - PPT 이미지처럼 Dialogue 예시는 Cafe24, 영어 UI 예시는 DOSSaemmul, 한국어 UI 예시는 PF_Stardust 느낌이 나야 한다.
   - 숫자 텍스트와 아이템 수량 표기는 언어 설정과 무관하게 동일한 픽셀 폰트 계열로 유지한다.
```

## 텍스트 설명만 사용하는 버전

```text
RE001 인게임 폰트 설정 시스템을 Unity에서 구현해줘.

구현 목적:
- 게임 내 모든 텍스트가 용도와 언어 설정에 따라 올바른 폰트를 자동으로 사용하도록 공통 폰트 정책을 만든다.
- UI마다 개별적으로 폰트를 관리하지 않고, 공통 컴포넌트와 매니저를 통해 일관되게 적용한다.

폰트 정책:
1. Cafe24
   - Dialogue Texts 전용 폰트다.
   - NPC 대화 텍스트에 사용한다.
   - 한국어/영어 언어 설정과 무관하게 항상 Cafe24를 사용한다.

2. DOSSaemmul
   - 언어 선택이 영어일 때 일반 UI 텍스트에 사용한다.
   - Dialogue Texts와 숫자 텍스트는 제외한다.
   - Menu명, Item명, Description 등 일반 UI 텍스트에 적용한다.

3. PF_Stardust
   - 언어 선택이 한국어일 때 일반 UI 텍스트에 사용한다.
   - Dialogue Texts와 숫자 텍스트는 제외한다.
   - Menu명, Item명, Description 등 일반 UI 텍스트에 적용한다.

4. 숫자 텍스트
   - 인게임의 모든 숫자 TEXT는 언어 선택과 무관하게 지정된 숫자 폰트로 고정한다.
   - 재화량, 아이템 수량, 퍼센트, 능력치 수치, 레벨 숫자 등은 언어 변경으로 폰트가 바뀌면 안 된다.

5. 기본 아웃라인
   - 모든 텍스트는 특별한 언급이 없으면 검은색 OUTLINE을 적용한다.
   - 특정 UI 기획서에서 다른 OUTLINE 색상이 명시된 경우에는 개별 기획서 설정을 우선한다.

예외 처리:
- Player Bar에 표시되는 Level 정보와 모험가 등급은 언어 선택과 무관하게 DOSSaemmul 폰트를 사용한다.
- Player Bar의 Level 정보는 항상 "LV.#" 형식으로 표시한다.
- 환경설정 Popup의 언어 선택 부분은 언어 선택과 무관하게 고정 표기한다.

구현 방식:
- FontUsage enum을 만든다.
  예: Dialogue, General, Number, PlayerLevel, PlayerRank, LanguageSetting
- FontManager 또는 FontPolicy 클래스를 만든다.
- 현재 언어 설정을 기준으로 General 텍스트만 DOSSaemmul 또는 PF_Stardust 중 하나를 선택한다.
- Dialogue, Number, PlayerLevel, PlayerRank, LanguageSetting 타입은 언어 변경 이벤트가 발생해도 고정 폰트를 유지한다.
- TMP_Text에 붙일 수 있는 FontBinder 컴포넌트를 만든다.
- FontBinder는 Inspector에서 FontUsage를 선택할 수 있어야 한다.
- FontBinder는 Awake, OnEnable, 언어 변경 이벤트 발생 시 TMP_Text의 font와 outline 설정을 다시 적용한다.
- 기본 outline은 검은색으로 적용하되, 개별 override가 필요한 경우 Inspector에서 outline color를 덮어쓸 수 있게 한다.

완료 기준:
- 언어를 한국어로 바꾸면 일반 UI 텍스트는 PF_Stardust로 바뀐다.
- 언어를 영어로 바꾸면 일반 UI 텍스트는 DOSSaemmul로 바뀐다.
- 대화 텍스트는 항상 Cafe24를 유지한다.
- 숫자 텍스트는 언어 변경과 무관하게 고정 폰트를 유지한다.
- Player Bar의 Level 정보는 항상 DOSSaemmul 폰트와 "LV.#" 형식을 유지한다.
- 모든 텍스트에는 기본 검은색 outline이 적용된다.
```
