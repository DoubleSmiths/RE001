

# 전체 박자표 (원작 비주얼 스크립팅 그래프 + Forge Smith 클립 AnimationEvent)

## 원작
■ 대본을 모는 주체는 3개다.

(A) `Enhace Popup` 의 ScriptMachine — fileID **347291926** (`?/UI/FIXED/In-Frame/Popup/Enhace Popup`)
  OnButtonClick(target=`Enhance_Btn` 604068287) → 확률 굴림(Random.Range(1,100001)) → 변수 저장 → SaveVariables
  → 성공/실패 분기 → **`EnhanceProgress Popup`(1779563732).SetActive(true)` → TriggerCustomEvent("Success"/"Fail")** 로 넘긴다.
  즉 [강화]를 누른 쪽은 **아무 연출도 하지 않는다.** 연출은 전부 EnhanceProgress Popup 안에서 벌어진다.
  (건너뛰기 체크 시에만 분기가 달라져 `FX_splash_Success(Skip)` / `Flash_Success` 만 켜고
   Close Btn·Enhance_Btn `interactable=false` → Timer **0.5** → `true` 로 되돌린다.)

(B) `EnhanceProgress Popup` 의 ScriptMachine — fileID **1779563734**
  CustomEvent "Success"/"Fail" → graph변수 `isSuccess` = true/false (설정만, 즉시)
  OnEnable → PlaySFX("SharpThin") → Timer **0.1** → Icon.DORestartById("4")
            → Timer **0.3** → Icon.DORestartById("2") → Timer **0.3**
            → `Hammer(IncludeAnim)`.Animator.Play(isSuccess ? "Success" : "Fail")
  OnDisable → Flash on / God_ray_02 off / Success Txt Group off / Bg off (다음 회차용 초기화)
  ⇒ **클립 재생 시각 = 팝업이 켜진 지 정확히 0.700초.**

(C) `Hammer(IncludeAnim)` 의 ScriptMachine — fileID **901440561**
  BoltUnityEvent 11개가 클립의 AnimationEvent(`functionName: TriggerUnityEvent`, `data:` 가 이름)를 받는다.

■ Forge Smith.controller (guid 2c5cd03d46ad88c489dd23e2a2df215a)
  상태 4개, **파라미터 0개·전이 0개**. 기본 상태는 모션 없는 `New State` → Animator.Play 로만 몰린다.
  Normal(speed **1.2**, 3.0167s) / Success(speed 1, **5.016667s**) / Fail(speed 1, **5.016667s**). 샘플레이트 60.

■ 클립 AnimationEvent 전문 (RE001_Share/Assets/Animation/UI/Forge Smith/)
  Success.anim : 0.0833 SmashSound / 0.25 Curious / 0.3333 Shake / 1.0833 SmashSound / 1.3333 Shake
                 / 2.0833 SmashSound / 2.3333 ChargeSuccess / 3.75 Serious / 3.8333 Success / 5.0 SuccessDone
  Fail.anim    : …동일… / 2.3333 ChargeFail / 3.75 Serious / 3.85 Fail / 3.90 FailSound / 5.0 FailDone
  Normal.anim  : 0.0833 SmashSound / 0.3333 Shake / 1.1666 SmashSound / 1.4166 Shake / 2.25 SmashSound
                 / 2.3333 Done / 2.5 ShakeLast
  ★ 타격음(SmashSound)은 **시각적 타격보다 0.25초 먼저** 난다(0.0833→0.3333, 1.0833→1.3333, 2.0833→2.3333).
  ★ 성공/실패 클립에는 **Shake가 2번뿐**이다. 세 번째 타격 자리는 ChargeSuccess/ChargeFail 이 대신한다.

■ 초 단위 대본 — t=0 은 [강화]를 누른 순간(=EnhanceProgress Popup SetActive(true)). 괄호는 클립 로컬시각.

  t=0.000  팝업 SetActive(true) — **페이드 없이 한 프레임 컷**. PlaySFX("SharpThin")
  t=0.100  Icon.DORestartById("4")  = PunchScale (0.3,0.3) 0.3s ease OutBack
  t=0.400  Icon.DORestartById("2")  = LocalMove →(0,0) 0.3s ease OutBack  (0.700 에 모루 착지)
  t=0.700  Hammer(IncludeAnim).Animator.Play("Success" | "Fail", 0, 0)
  t=0.783 (0.0833) SmashSound  → PlaySFX("EnhanceHammer")            [1타 소리]
  t=0.950 (0.25)   Curious     → PlaySFX("Curious")
  t=1.033 (0.3333) Shake       → Icon.id1 ShakePosition(4,4) 0.25s
                                 + `Icon/forAnim/Flash`.SetActive(true)
                                 + TriggerCustomEvent("HammerSmash") → `UI/FIXED/In-Frame/FX`
                                 + 자기(Hammer) DOTweenAnimation.DORestart() = ShakePosition(2.5,2.5) 0.25s
                                 + `Floor`.DORestart() = PunchPosition(2,1.5) 0.3s      [1타 그림]
  t=1.783 (1.0833) SmashSound
  t=2.033 (1.3333) Shake  (위와 동일 5종)                              [2타]
  t=2.783 (2.0833) SmashSound
  t=3.033 (2.3333) ChargeSuccess / ChargeFail — 담금질. 공통:
                    `Bg`.SetActive(true); Bg.id51 = Color→(0.5882,0.5882,0.5882,1) **1.4s Linear**
                    Hammer 자체 shake; Floor punch 0.3s; TriggerCustomEvent("HammerSmashLong")→FX
                    Icon.id5 = LocalMove→(0,50) **1.4s Linear** + Scale→1.4 **1.4s Linear**
                    `Icon/forAnim`.DORestart() = ShakePosition(1.5,1.5) **1.7s**
                    `Icon/forAnim/SuccessFlash`.id1 = Color→(1,1,1,**0.627451**) **1.4s**
  t=4.450 (3.75)   Serious     → PlaySFX("Serious")

  ── 성공 가지 ──
  t=4.533 (3.8333) Success → PlaySFX("Success")
                             Bg.id52 = Color→(0.8627,0.8627,0.8627,1) **0.15s Linear**  (→ t=4.683 최대 백광)
                             Icon.id6 = LocalMove→(0,80) 0.3s ease OutBack
  t=4.534 (3.033+Timer 1.501) TriggerCustomEvent("HammerSmashSuccess")→FX
  t=4.733 (Success+Timer 0.2)  Bg.id53 = Color→검정 **1.8s InOutQuad**  (t=6.533 에 완전 검정)
  t=4.744 (+Timer 0.21)        SuccessFlash.id2 = Color→a 1.0, 0.3s
  t=5.033 (Success+0.2+0.3)    Icon.id4 = PunchScale(0.3,0.3) 0.3s
  t=5.054 (+Timer 0.31)        SuccessFlash.id3 = Color→a 0, 0.5s
  t=5.700 (5.0)    SuccessDone → Timer 0.2
  t=5.900          `Success Txt Group`.SetActive(true)
                   Icon.id7 = LocalMove→(0,20) **1.2s** + Scale→1.0 **1.0s**
                   `God_ray_02`.SetActive(true); Glow/Image(1)·Image(2) material 교체
  t=7.600 (+1.7)   `Enh Att Detail`.SetActive(true)
  t=7.850 (+0.25)  `Enh Att Detail/Att Real Text`.SetActive(true)
  t=9.700 (+1.85)  **`Exit Btn`.SetActive(true)**  ← 여기 전까지는 닫을 수 없다

  ── 실패 가지 ──
  t=4.534 (3.033+1.501) Bg.id54 = Color→검정 **0.3s ease 9(InCubic)**
                        TriggerCustomEvent("HammerSmashFail")→FX
                        SuccessFlash.color = **(0,0,0,0.980392)** 즉시 대입
  t=4.544 (+0.01)  SuccessFlash.id4 = Fade **1.0s Linear**
  t=4.550 (3.85)   Fail → Icon.id8 = LocalMove→(0,0) **0.1s ease OutBack** + Scale→1.0 1.0s
  t=4.600 (3.90)   FailSound → PlaySFX("FailSound")
  t=5.700 (5.0)    FailDone → `Fail Txt Group`.SetActive(true)
  t=6.000 (+0.3)   Icon.id9 = LocalMove→(0,20) 0.5s
  t=7.100 (+1.1)   `Poten Group`.SetActive(true)
  t=7.300 (+0.2)   PlaySFX("FlameOn")
  t=9.350 (+2.05)  **`Exit Btn`.SetActive(true)**

■ SFX 이름 원문 (Sound Player 로 TriggerCustomEvent("PlaySFX", arg0)) —
  "SharpThin"(t=0) / "EnhanceHammer"(타격 3회) / "Curious" / "Serious" / "Success" / "FailSound" / "FlameOn"

■ 영상 검산 (ref30, 프레임 N → 200.0+N*0.1s, 암전 f173=217.3s 을 t=0 으로)
  예측 t → 실측 프레임
   1.033 → 218.33 : f183 mean 8.62→12.76 (diff 8.88) ✔
   2.033 → 219.33 : f193 8.62→11.15 (diff 6.09) ✔
   3.033 → 220.33 : f203 8.55→13.08 (diff 8.68), 이후 밝기가 단조 상승 시작 ✔ (=Bg id51 1.4s 램프)
   4.433 (id51 종료) → 221.73 : f217 98.6 ✔
   4.683 (id52 종료=최대) → 221.98 : **f220 mean 137.89 가 전 구간 최대** ✔✔
   4.733~6.533 (id53) → 222.03~223.83 : f220→f231 단조 하강 137.9→30.06 ✔
   5.900 (Success Txt) → 223.20 : **f232 diff 16.77 로 급변** ✔✔
   7.600/7.850 (Enh Att Detail / Att Real Text) → 224.90/225.15 : f249~f253 상승 재개 ✔
  6개 독립 지점이 전부 10fps 한 프레임 이내로 맞는다 → 0.700 리드인과 이벤트 표는 확정.
  ※ 과제문의 "218.3·219.3·220.3 망치 3타"는 정확히는 **Shake·Shake·ChargeSuccess** 이고,
    실제 SmashSound 는 그보다 0.25초 앞(218.08/219.08/220.08)이다.
  ※ 과제문의 "224s 결과창"은 실측 **223.2s(성공 배너)** 이며, 224.9 능력치 라벨 / 225.15 수치 / 227.0 나가기 버튼이다.

## 우리
■ 구조 — 원작과 다르게 **두 팝업으로 쪼개져 있다.**
  `UI_EnhacePopup` 이 망치 연출까지 끌고 가다가(CoSmithMotion) 끝나면
  `Managers.UI.ShowPopupUI<UI_EnhanceProgressPopup>()` 로 **결과창을 새로 띄운다**
  (UI_EnhacePopup.cs:900). 원작은 처음부터 끝까지 `EnhanceProgress Popup` 하나다.

■ 애니메이션 에셋은 **원작과 바이트 단위로 동일**하다 (실측: `diff` 결과 무출력).
  Assets/@Resources/Animation/UI/Forge Smith/{Success,Fail,Normal}.anim — 이벤트 시각·이름·클립 길이(5.016667/3.0166667) 전부 일치.
  Forge Smith.controller 도 동일(New State 기본, 전이 0).

■ 프리팹도 원작 저작값을 **이미 전부 갖고 있다.** Assets/@Resources/Prefab/UI/Popup/UI_EnhanceProgressPopup.prefab:
  Icon id1(Shake 4,4 0.25) id2(LocalMove 0,0 0.3 ease26) id3 id4(PunchScale .3 0.3) id5(→0,50 1.4 + Scale 1.4)
  id6(→0,80 0.3 ease27) id7(→0,20 1.2 + Scale 1.0) id8(→0,0 0.1) id9(→0,20 0.5)
  Bg id51(→0.5882 1.4 Linear) id52(→0.8627 0.15) id53(→검정 1.8 ease6) id54(→검정 0.3 ease9)
  SuccessFlash id1(a .627451 1.4) id2(a 1 0.3) id3(a 0 0.5) id4(Fade 1.0)
  Floor(Punch 2,1.5 0.3) / forAnim(Shake 1.5,1.5 1.7) / Hammer_IncludeAnim(Shake 2.5,2.5 0.25)
  → **우리 코드는 이 중 극히 일부만 재생하고, 나머지는 UI_EnhacePopup 쪽에서 코드 DOTween으로 다시 만들었다.**

■ 코드 박자
  UI_EnhacePopup.cs:864  `const float SMITH_LEAD_IN = 0.57f;`  (HEAD 에는 이 상수 자체가 없음 = 리드인 0)
  UI_EnhacePopup.cs:871  EnterCeremonyView() → :147 `Blackout(STAGE_CEREMONY, 0.35f)` + :148 LandCraftIcon()(0.2s)
  UI_EnhacePopup.cs:877  `WaitForSecondsRealtime(SMITH_LEAD_IN)` → :887 `anim.Play(state,0,0f)`
  UI_EnhacePopup.cs:894  `WaitForSecondsRealtime(wait)` — wait = 클립 length **5.0167** (SuccessDone 이벤트 5.000 이 아님)
  UI_EnhacePopup.cs:897-900 `anim.Play("Normal")` → ExitCeremonyView() → 결과 팝업 생성
  UI_EnhacePopup.cs:918-925 EVENT_SFX 표: SmashSound→SFX_EnhanceHammer / Curious→SFX_CuriousCaution2 /
        Serious→SFX_Serious / Success→SFX_Success(Base) / FailSound→SFX_Fail
  UI_EnhacePopup.cs:930  `FLAME_DELAY = 1.6f` (FailDone→FlameOn) — 원작 0.3+1.1+0.2 와 **정확히 일치** ✔
  UI_EnhacePopup.cs:954-1002 OnSmithAnimEvent: Shake→ShakeFrame(0.22,2)+PopImpactFx / ChargeX→RaiseCraftIcon /
        Success→PopCraftIcon+PopResultFlash(true) / Fail→ShakeFrame(0.35,2.5)+RestoreCraftIcon(0.1)+PopResultFlash(false)
  UI_EnhacePopup.cs:252  `SUCCESS_WASH = 1.5f`, :1176-1182 PopResultFlash = Blackout(흰,0) → Blackout(검정,1.5)
  UI_EnhanceProgressPopup.cs:71-73 STEP_TITLE 0.20 / STEP_DETAIL 1.70 / STEP_TABLE 0.25 — 원작 Timer 값과 일치 ✔
  UI_EnhanceProgressPopup.cs:86-87 WASH_LEFT 0.18 / WASH_TAIL 0.45 (원작 id53 잔여분 대체)
  UI_EnhanceProgressPopup.cs:171 PlaySFX("SFX_Upgrade") / :278 PlaySFX("SFX_Fail") — 원작에 없는 추가 사운드
  UI_EnhanceProgressPopup.cs:109 ExitBtn onClick 등록, 프리팹 `ExitBtn` m_IsActive=**1**, 활성 지연 코드 없음

■ 우리 최신 녹화 실측 (Screenshots/demo30, 15fps, 암전 f151=10.067s 를 t=0)
  hit1 f155~156 → t=+0.30 / hit2 f170~171 → +1.33 / hit3 f185~187 → +2.33
  백광 정점 f211 (14.067) → +4.00 / 결과창 f234 (15.6) → +5.53
  → **3연타 간격 1.000s 는 정확하나, 전 구간이 원작보다 0.70초 이르다.**
  (이 녹화는 01:20 촬영이고 SMITH_LEAD_IN 은 02:58 에 워킹트리에 추가된 미커밋 변경 —
   `git show HEAD:…UI_EnhacePopup.cs | grep SMITH_LEAD_IN` 은 exit 1)

## 차이
- **[치명]** 리드인이 없다/모자란다 — 원작은 팝업이 켜지고 **0.700초** 뒤에 클립을 돌린다(Timer 0.1+0.3+0.3). 그 사이 아이콘이 PunchScale(0.1s)하고 모루로 내려앉는다(0.4~0.7s). 우리는 HEAD 기준 0초, 워킹트리 기준 0.57초.
  - 근거: 원작: 씬 fileID 1779563734 그래프 OnEnable → Timer(0.1)#67 → InvokeMember DORestartById("4")#41 → Timer(0.3)#25 → DORestartById("2")#59 → Timer(0.3)#53 → Animator.Play#55. 우리: UI_EnhacePopup.cs:864 SMITH_LEAD_IN=0.57f (HEAD엔 부재). 영상 실측: 원작 암전 f173(217.3)→1타 f183(218.3)=+1.00, 우리 암전 f151(10.067)→1타 f156(10.40)=+0.33.
- **[치명]** 연출과 결과가 **다른 팝업**으로 쪼개져, 인계 지점에서 저작 트윈 대부분이 버려지고 인스턴스화 지연이 얹힌다. 원작은 EnhanceProgress Popup 하나가 t=0부터 t=9.7까지 전부 담당한다.
  - 근거: 원작: Enhace Popup 그래프(347291926) #94/#181 이 EnhanceProgress Popup.SetActive(true) 한 뒤 CustomEvent만 보낸다 — 이후 모든 노드가 EnhanceProgress Popup 소속. 우리: UI_EnhacePopup.cs:900 ShowPopupUI<UI_EnhanceProgressPopup>(). 실측 지연: 예측 +5.217 대비 실측 f234=+5.533 (0.32s 초과).
- **[치명]** 결과창 `Exit Btn` 이 처음부터 켜져 있어 연출 도중 아무 때나 탭으로 닫힌다. 원작은 성공 t=9.700 / 실패 t=9.350 에야 켠다.
  - 근거: 원작: g_hammer #83 SuccessDone→…→Timer(1.85)#131→#127 ExitBtn.SetActive(true); #186 FailDone→…→Timer(2.05)#89→#75. 우리: UI_EnhanceProgressPopup.prefab 의 ExitBtn m_IsActive: 1, UI_EnhanceProgressPopup.cs:109 에서 리스너만 달고 활성 지연 없음.
- **[치명]** t=0 의 개막 효과음 "SharpThin" 이 없고, 대신 원작에 없는 결과음이 한 번 더 겹쳐 난다(성공음이 t≈4.53 과 t≈5.72 두 번).
  - 근거: 원작: g_prog #65 TriggerCustomEvent(PlaySFX, Literal#51="SharpThin") 가 OnEnable 첫 노드. 우리: UI_EnhacePopup.cs 전체 PlaySFX 호출 6곳 중 연출 개시 지점 없음(354/368/517 은 버튼음). 중복: UI_EnhacePopup.cs:921 Success→SFX_Success(Base) + UI_EnhanceProgressPopup.cs:171 SFX_Upgrade / :925 FailSound→SFX_Fail + :278 SFX_Fail.
- **[눈에띔]** 타격 시 흔드는 대상이 다르다. 원작은 아이콘(4,4 0.25s)·망치(2.5,2.5 0.25s)·바닥(Punch 2,1.5 0.3s) 세 개를 따로 흔들고 **창은 흔들지 않는다.** 우리는 팝업 프레임 전체를 흔든다.
  - 근거: 원작: g_hammer ROOT#10 Shake → #110 Icon.DORestartById("1") → #172 Flash.SetActive(true) → #224 HammerSmash → #95 DORestart(self=Hammer) → #252 Floor.DORestart(). 프리팹 저작값: Icon id1 type12 (4,4,0) dur .25 / Hammer_IncludeAnim type12 (2.5,2.5,1) dur .25 / Floor type9 (2,1.5,1) dur .3. 우리: UI_EnhacePopup.cs:988 ShakeFrame(0.22f, 2f).
- **[눈에띔]** 백광(Bg) 곡선이 3단이 아니라 2단이다. 원작 = id51(1.4s→0.5882 Linear, 담금질 내내 서서히) → id52(0.15s→0.8627, 확정 순간) → +0.2s id53(1.8s→검정 InOutQuad). 우리 = Blackout(흰,0s) 즉시 + Blackout(검정,1.5s OutQuad) + 결과창에서 0.18→검정 0.45s.
  - 근거: 원작 프리팹/씬 Bg DOTweenAnimation id51/52/53 값. 영상 검산: 실측 최대밝기 프레임 f220(222.0, mean 137.89)이 id52 종료 시각 221.98 과 일치. 우리: UI_EnhacePopup.cs:252 SUCCESS_WASH=1.5f, :1176-1182; UI_EnhanceProgressPopup.cs:86-87 WASH_LEFT .18/WASH_TAIL .45. 우리 녹화는 f209(10.17)→f211(144.66) 로 2프레임 만에 튄다 — 원작의 1.4초 예열 구간이 없다.
- **[눈에띔]** 암전이 0.35초 페이드다. 원작은 SetActive 한 방 = 한 프레임 컷.
  - 근거: 원작 ref30: f172 mean 17.15 → f173 9.23 (한 프레임). 우리 demo30: f150 11.17 → f151 5.94 → f153 4.83 (+ UI_EnhacePopup.cs:147 Blackout(STAGE_CEREMONY, 0.35f) OutQuad).
- **[눈에띔]** 아이콘 착지 박자. 원작은 t=0.100 PunchScale 0.3s → t=0.400 LocalMove→(0,0) 0.3s ease OutBack(=0.700 착지). 우리는 t=0 에 OutQuad 0.2s 한 번.
  - 근거: 원작 g_prog #41(id"4")/#59(id"2") + 프리팹 Icon id4 type11 dur .3 ease26 / id2 type2 dur .3 ease26. 우리: UI_EnhacePopup.cs:148 LandCraftIcon(), :1065 ICON_LAND_DUR=0.2f, Ease.OutQuad.
- **[눈에띔]** 아이콘 위 흰 덮개(SuccessFlash) 3단 타이밍이 통째로 다르다. 원작 성공 = 3.033 a→0.627(1.4s) / 4.744 a→1(0.3s) / 5.054 a→0(0.5s). 실패 = 4.534 색 (0,0,0,0.98) 대입 → 4.544 Fade 1.0s.
  - 근거: 원작 g_hammer #145 ChargeSuccess 체인의 SuccessFlash id1 → Timer 1.501 → Timer 0.21 → id2 → Timer 0.31 → id3; #202 ChargeFail 체인의 #56 SetMember color=(0,0,0,0.9803922) → Timer 0.01 → id4. 우리: UI_EnhacePopup.cs:1119 Whiten(1f, ICON_RISE_DUR=1.4) / :1141-1146 hold 0.25 → out 0.55.
- **[사소]** 결과창 대기를 SuccessDone 이벤트(클립 5.000)가 아니라 클립 전체 길이(5.016667)로 잡는다.
  - 근거: 원작: g_hammer ROOT#83 이 BoltUnityEvent(SuccessDone)에서 출발. 우리: UI_EnhacePopup.cs:888-894 `if (c.name == state) { wait = c.length; }` → WaitForSecondsRealtime(wait).
- **[사소]** 실패 결과창에서 아이콘 id9 를 +0.2 에 누른다. 원작은 FailDone +0.3.
  - 근거: 원작 g_hammer #186 FailDone → Timer(0.3)#176 → #48 Icon.DORestartById("9"). 우리: UI_EnhanceProgressPopup.cs:295-296 Wait(STEP_TITLE=0.20f) → PlayAuthored(Icon, "9").
- **[사소]** 실패 시 `Poten Group` 을 켜지 않는다(의도적 결정). 원작은 FailDone +1.4 (=0.3+1.1) 에 켠 뒤 +0.2 에 FlameOn 을 울린다.
  - 근거: 원작 g_hammer #186 → Timer .3 → id9 → Timer 1.1 → #91 PotenGroup.SetActive(true) → Timer .2 → PlaySFX("FlameOn"). 우리: UI_EnhanceProgressPopup.cs:290 `SetGroupActive(GameObjects.PotenGroup, false); // 실패 — 잠재력 변화 없음`. FlameOn 만 UI_EnhacePopup.cs:930 FLAME_DELAY=1.6 로 정확히 살아 있다.

## 수정안
■ 1) 리드인 0.57 → 0.700 으로, 그리고 그 안을 원작대로 채운다
Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs:864 를 상수 세 개로 교체:

    // 원작 EnhanceProgress Popup 그래프(fileID 1779563734) OnEnable 체인 그대로.
    const float LEAD_PUNCH = 0.10f;   // → 아이콘 PunchScale(0.3,0.3) 0.3s OutBack   (저작 id "4")
    const float LEAD_LAND  = 0.30f;   // → 아이콘 LocalMove→(0,0) 0.3s OutBack        (저작 id "2")
    const float LEAD_PLAY  = 0.30f;   // → Animator.Play  (합계 0.700)

CoSmithMotion(UI_EnhacePopup.cs:866~) 을 이렇게:

    _resolving = true;
    HookSmithEvents();
    EnterCeremonyView();                       // ★ 여기서 LandCraftIcon() 은 빼고 (아래 :148 수정)
    Managers.Sound?.PlaySFX("SFX_SharpThin");  // ★ 원작 t=0 개막음 (없으면 등록 필요)

    yield return new WaitForSecondsRealtime(LEAD_PUNCH);
    PunchCraftIcon();                          // 신규: rt.DOPunchScale(new Vector3(.3f,.3f,0f), 0.3f, 10, 1f).SetEase(Ease.OutBack)

    yield return new WaitForSecondsRealtime(LEAD_LAND);
    LandCraftIcon();                           // :1101 ICON_LAND_DUR 0.2f → 0.3f, Ease.OutQuad → Ease.OutBack

    yield return new WaitForSecondsRealtime(LEAD_PLAY);
    // … 기존 anim.Play(state, 0, 0f) …

그리고 :147-148 EnterCeremonyView 에서 `LandCraftIcon();` 한 줄 삭제(위로 옮겼으므로).
:1065  `const float ICON_LAND_DUR = 0.2f;` → `= 0.3f;` (원작 Icon id2 dur=0.3 ease26=OutBack)

■ 2) 암전을 컷으로
UI_EnhacePopup.cs:147  `Blackout(STAGE_CEREMONY, 0.35f);` → `Blackout(STAGE_CEREMONY, 0f);`
(원작 ref30 f172→f173 이 한 프레임 컷이다. ExitCeremonyView :175 의 0.30f 는 그대로 둬도 무방 —
 원작은 팝업이 통째로 사라질 뿐이라 대응물이 없다.)

■ 3) 백광을 3단으로 (원작 Bg id51/52/53/54 수치 그대로)
UI_EnhacePopup.cs:252 `SUCCESS_WASH` 를 버리고 아래 상수로 교체:

    static readonly Color WASH_CHARGE = new Color(0.5882353f, 0.5882353f, 0.5882353f, 1f);
    static readonly Color WASH_PEAK   = new Color(0.8627451f, 0.8627451f, 0.8627451f, 1f);
    const float WASH_CHARGE_DUR = 1.4f;   // Ease.Linear
    const float WASH_PEAK_DUR   = 0.15f;  // Ease.Linear
    const float WASH_OUT_DELAY  = 0.20f;
    const float WASH_OUT_DUR    = 1.8f;   // Ease.InOutQuad
    const float WASH_FAIL_DUR   = 0.3f;   // Ease.InCubic  (원작 id54)

OnSmithAnimEvent(:954) 의 `case "ChargeSuccess": case "ChargeFail":` 에 추가:
    _blackout... → Blackout(WASH_CHARGE, WASH_CHARGE_DUR) 를 Ease.Linear 로 (Blackout 은 현재 OutQuad 고정 :218 —
    ease 인자를 받도록 시그니처를 `void Blackout(Color target, float duration, Ease ease = Ease.OutQuad)` 로 넓힐 것)

PopResultFlash(:1176) 성공 경로:
    Blackout(WASH_PEAK, WASH_PEAK_DUR, Ease.Linear);
    DOVirtual.DelayedCall(WASH_OUT_DELAY, () => Blackout(STAGE_CEREMONY, WASH_OUT_DUR, Ease.InOutQuad)).SetUpdate(true);
실패 경로:
    Blackout(STAGE_CEREMONY, WASH_FAIL_DUR, Ease.InCubic);   // 현재 0.35f OutQuad

결과창 이어받기(UI_EnhanceProgressPopup.cs:86-87)는 새 곡선에 맞춰 다시 계산:
    결과창이 열리는 시각은 Success 이벤트 +1.167s. id53(0.8627→0, 1.8s InOutQuad) 진행률 0.648 →
    남은 색 = 0.8627 × (1 − InOutQuad(0.648)) = **0.214**, 남은 시간 = **0.633s**.
    `WASH_LEFT = 0.18f` → **0.214f**, `WASH_TAIL = 0.45f` → **0.633f**, Ease.OutQuad → **Ease.InOutQuad**.

■ 4) 개막/결과 SFX 정리
- UI_EnhacePopup.cs CoSmithMotion 맨 앞에 `PlaySFX("SFX_SharpThin")` 추가(위 1번에 포함).
- UI_EnhanceProgressPopup.cs:171 `Managers.Sound?.PlaySFX("SFX_Upgrade");` **삭제**
  (원작은 t=4.533 "Success" 이벤트에서 이미 울렸다 — UI_EnhacePopup.cs:922 가 담당).
- UI_EnhanceProgressPopup.cs:278 `Managers.Sound?.PlaySFX("SFX_Fail");` **삭제**
  (원작은 t=4.600 "FailSound" — UI_EnhacePopup.cs:925 가 담당).

■ 5) 흔들림 대상 교정 (창 흔들기 → 아이콘·망치·바닥)
UI_EnhacePopup.cs:987-989 `case "Shake":` 에서 `ShakeFrame(0.22f, 2f);` 를 지우고:

    CraftIcon()?.DOShakeAnchorPos(0.25f, new Vector3(4f, 4f, 0f), 10, 90f, false, true).SetUpdate(true);
    GetObject((int)GameObjects.HammerNormalAnim)?.transform
        .DOShakeAnchorPos... (RectTransform, 0.25f, new Vector3(2.5f, 2.5f, 0f), 10, 90f, false, true)
    // 바닥: UI_EnhacePopup 프리팹의 Floor/1/2 노드에 PunchPosition(2,1.5) 0.3s 를 저작하거나
    //       floorRT.DOPunchAnchorPos(new Vector2(2f, 1.5f), 0.3f, 10, 1f)
`case "ShakeLast":`(:993) 도 같은 3종으로. `case "Fail":`(:998) 의 ShakeFrame(0.35f,2.5f) 는
원작에 대응물이 없다 — 원작 Fail 이벤트는 Icon id8 하나뿐이므로 삭제 권장.

■ 6) 결과창 나가기 버튼 지연
UI_EnhanceProgressPopup.cs:109 뒤에 `GetButton((int)Buttons.ExitBtn).gameObject.SetActive(false);` 추가하고,
CoRevealSuccess 마지막(:272 부근)에 `yield return Wait(1.85f); GetButton((int)Buttons.ExitBtn).gameObject.SetActive(true);`
CoRevealFail 마지막(:303 부근)에 `yield return Wait(2.05f - STEP_TITLE); … SetActive(true);`
(원작 절대시각: 성공 t=9.700 / 실패 t=9.350. 결과창은 t=5.700 에 열리므로 각각 +4.000 / +3.650 이지만,
 원작 체인 그대로 따르면 성공은 AttRealText 켠 뒤 +1.85, 실패는 FlameOn 뒤 +2.05 다.)

■ 7) 클립 길이 대신 이벤트로 인계
UI_EnhacePopup.cs:888-894 의 `wait = c.length` (5.016667) 대신
OnSmithAnimEvent 의 `case "SuccessDone": case "FailDone":` 에서 인계 코루틴을 깨우는 편이 원작과 같다(차이 17ms — 우선순위 낮음).

■ 검증법
수정 후 `unity-cli editor refresh --compile` → `console --type error` → 플레이 녹화(15fps) 후 아래가 나와야 한다
(암전 프레임을 t=0 으로):
  1타 +1.033 / 2타 +2.033 / 담금질 +3.033 / 백광 정점 +4.683 / 성공 배너 +5.900 /
  능력치 라벨 +7.600 / 수치 +7.850 / 나가기 버튼 +9.700
원작 실측 대응 프레임(ref30): f183 / f193 / f203 / f220 / f232 / f249 / f251 / (f270).


# 불꽃 (Flame Img) — 강화창 잠재력 상자 위로 타오르는 불

## 원작
**정체 확정: 원작의 "아래에서 위로 올라오는 불꽃" = `Enhace Popup/Potential Box/Flame Img` 다. Btn Embers(불티)가 아니다.**

▸ 화면 위치 특정 (ref30 전 프레임 주황 픽셀 전수 스캔, f110~f285)
  화면에 지속적으로 존재하는 불은 이것 하나뿐이다. 그 외 주황 덩어리는 ① 망치 나무 손잡이(ref x80~95,y384~415 — f175 암전 중에도 남아 있어 불이 아님을 확인), ② 낱개 불티 스파크(20px 미만)뿐.
  불꽃 위치 = ref px x[251..296] y[363..404] 안쪽 바닥에서 시작해 위로 y=328까지 뻗는다.

▸ 오브젝트 (SampleScene.unity)
  GameObject 1186907021 `Flame Img`, m_IsActive: **0** (기본 꺼짐)
  RectTransform 1186907022 : localScale (0.8,0.8,1) / anchorMin=anchorMax (0.5,**0**) / anchoredPosition (0.5,**5**) / sizeDelta (35,45) / pivot (0.5,**0**)
    → **피벗이 바닥**이라 스프라이트가 커질수록 위로만 자란다. 이것이 "아래에서 위로".
  Image 1186907024 : sprite guid 100fd5d0def7f8a45b3cfc77b8a41b0e (= flame17), color (1,1,1,1), material 0, raycastTarget 0
  Animator 1186907023 : controller guid c1620e7a1a075eb458011d4be89e77f9, cullingMode 0
  부모 `Potential Box` 1416030573 : anchor (0.5,0.5), anchoredPosition (50,21), sizeDelta (22,22), active=0
    + DOTweenAnimation 1416030580 : animationType 2(LocalMove), isFrom 1, endValueV3 (50,40,0), duration 0.5, delay 0, easeType 27, autoPlay 1 → 19u 위에서 0.5초에 걸쳐 내려앉는다

▸ 컨트롤러 (Flame Img.controller)
  DefaultState = **Flame_Start** → (exitTime 1, transitionDuration 0, 조건 없음) → **Flame_Loop**(무한)

▸ 클립 (sampleRate 60)
  Flame_Start (stopTime 0.5833, loopTime 0) : flame1@0 / flame1@0.0333 / flame2@0.0667 / flame3@0.15 / flame4@0.2333 / flame5@0.3167 / flame6@0.4 / flame7@0.4833 / flame7@0.5667  — 위치·스케일·알파 커브 없음. **m_Sprite PPtr 커브 단 하나**
  Flame_Loop (stopTime 1.35, loopTime 1) : flame8~flame23 을 1/12초(0.083333) 간격, 마지막 flame23@1.3333

▸ 스프라이트 (RE001_Share/Assets/Sprite/Effect/Flame/flame1~23.png, **아틀라스 아님 · 낱장 47x75 RGBA**)
  알파 내용의 상단 높이(바닥 기준, px):  flame1 26 → flame2 26 → flame3 36 → flame4 41 → flame5 49 → flame6 52 → flame7 56 → (loop) flame8~23 은 63~73 사이 명멸
  즉 "솟아오름"은 트윈이 아니라 **스프라이트 시퀀스에 그려져 있다**.

▸ 영상 실측 (ref30, 10fps, 상자 바닥 = ref y404)
  f80 망치 등장(인트로 t=0) / f85 모루 / f91~93 잠재 상자 내려앉음 / **f94 불꽃 점화**
  f94 : 흰 코어 x[258..288] y[374..396] (31x23px) — 불꽃 높이 상자바닥 대비 **8px ≒ 4u**
  f95 : x[252..293] y[363..395] — 41px ≒ **21.5u**
  f96 : x[252..293] y[350..392] — 가장 넓다 42px = **22u** (flame3~5의 뭉툭한 불덩이)
  f97,98 : y[347..393]
  f99 : x[253..294] y[334..405] — 70px ≒ **36.7u** (최대)
  f100~ : Flame_Loop, top 328~340 사이 명멸
  **상자 착지(f93) → 점화(f94) 간격 0.1초, 상승 f94→f99 = 0.5초.**

## 우리
▸ 에셋은 **이미 완전히 이식돼 있다** (이름만 FlameImg로 바뀜)
  `Assets/@Resources/Sprite/Effect/Flame/flame1~23.png` (47x75, 낱장) — 원작과 동일
  `Assets/@Resources/Animation/UI/Flame Img/{Flame Img.controller, Flame_Start.anim, Flame_Loop.anim}`
    controller guid **c1620e7a1a075eb458011d4be89e77f9** — 원작과 같은 guid, DefaultState=Flame_Start, 전이 exitTime 1/duration 0 동일
    Flame_Start stopTime 0.5833 / loopTime 0, 스프라이트 순서·시각 flame1,1,2,3,4,5,6,7,7 — **원작과 완전 일치**
    Flame_Loop stopTime 1.35 / loopTime 1, flame8~23 — **완전 일치**

▸ 프리팹 `UI_EnhacePopup.prefab`
  GO &620945196355749948 `FlameImg`, m_IsActive: **0** (prefab:5812~5814)
  RectTransform &2068330600222078370 (prefab:5818~5836): scale (0.8,0.8,1) / anchor (0.5,0) / anchoredPos (0.5,5) / sizeDelta (35,45) / pivot (0.5,0) — **원작과 수치 동일**
  Image &2314811955970940721 sprite guid 100fd5d0…(flame17), Animator &8788771029525221070 controller c1620e7a… — 동일
  부모 `PotentialBox` &3868084374872672363 (prefab:14617): anchor (0.5,0.5), anchoredPos (50,21), size (22,22) + DOTweenAnimation &1890780721558180411 (LocalMove/isFrom/(50,40,0)/0.5s/ease27) — **원작과 동일**
  계층: FlameImg(active 0) ← PotentialBox(1) ← EnhacePopup(1) ← In_Frame ← FIXED ← UI_EnhacePopup — 부모 비활성 없음

▸ 코드
  `UI_EnhacePopup.cs:94`  `GetObject((int)GameObjects.FlameImg)?.SetActive(true);` — **Init() 시점(t=0)에 켠다**
  `UI_EnhacePopup.cs:264~272` `INTRO_STEPS` 에 `(GameObjects.PotentialBox, 0.80f)`
  `UI_EnhacePopup.cs:278~304` `PlayIntro()` — 각 스텝에 CanvasGroup을 붙여 **`cg.alpha = 0f`** 로 만든 뒤 `DOFade(1, INTRO_FADE=0.28f).SetDelay(delay)`
  → 잠재 상자는 **0.80s 동안 alpha 0**, 1.08s에 완전히 보인다. 원작에 없는 우리만의 연출이다.
  `UI_EnhanceProgressPopup.prefab:7407` 의 FlameImg 는 m_IsActive 0 이고 켜는 코드 없음 — 원작도 실패 연출용이라 이번 성공 영상엔 안 나옴(차이 아님)

▸ 영상 실측 (demo30, 15fps, 1u=5.18px, 상자 = native x[743..857] y[1055..1169])
  f23 팝업 오픈(강화 버튼 영역 밝기 31.2→73.7)
  **f23~f30 잠재 상자 영역(x740..860, y1040..1170) 평균 밝기 정확히 0.00** — 상자도 불꽃도 화면에 없다
  f31 21.71 → f32 41.42 → f34 40.61(정착)
  불꽃 최초 가시 프레임 f31, top=1036 (상자바닥 대비 25.7u) / f32 top=971 (38.2u) / f33 top=954
  → **첫 프레임부터 이미 Flame_Loop의 가늘고 긴 리본 형태**다. 원작 f96~f98의 뭉툭한 불덩이(flame3~5)는 화면에 **한 프레임도 나오지 않는다**.
  루프 자체 크기는 정상: 폭 15~18u / 높이 28~33u (원작 13~18u / 29~39u)

## 차이
- **[치명]** Flame_Start(0.583초, flame1→7의 '작은 불씨→뭉툭한 불덩이→긴 불길')가 화면에 전혀 나오지 않는다. UI_EnhacePopup.cs:94가 Init()(t=0)에 FlameImg를 켜는데, PlayIntro()가 부모 PotentialBox의 CanvasGroup alpha를 0으로 만들고 delay 0.80f 뒤에야 페이드인하므로, 상자가 보이기 시작할 때 Flame_Start는 이미 다 타고 Flame_Loop에 들어가 있다. 결과적으로 '아래에서 위로 솟는' 탄생 연출이 통째로 잘려 나간다.
  - 근거: 코드: Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs:94 (SetActive(true) at Init) vs :269 (PotentialBox, 0.80f) + :294~301 (cg.alpha=0f; DOFade(1, INTRO_FADE=0.28f).SetDelay(delay)). 영상: demo30 f23 팝업 오픈, 잠재상자 영역(x740..860,y1040..1170) 평균휘도가 f23~f30 전부 0.00, f31=21.71에서 처음 등장 — 그 f31의 불꽃 실루엣이 이미 Flame_Loop형(폭 15.4u/높이 28.4u, top y=1036). 원작 ref30은 f94 점화(폭 31px≒16u, 상자바닥 대비 4u) → f96 최대폭 42px=22u(뭉툭) → f99 최대높이 70px=36.7u 로 0.5초에 걸쳐 자란다.
- **[눈에띔]** 점화 타이밍 순서가 다르다. 원작은 '잠재 상자가 내려앉은 뒤 0.1초 있다가 불이 붙는다'. 우리는 상자와 불꽃이 같은 프레임에 동시에, 그것도 이미 다 자란 상태로 나타난다.
  - 근거: 원작 ref30: f91~f93 상자 슬라이드(잠재상자 영역 휘도 7.42→9.50→11.88→10.30), f94에 처음으로 불꽃 픽셀(흰 코어 x[258..288] y[374..396]). 우리 demo30: f31에 상자 테두리와 불꽃이 동시 출현(휘도 0.00→21.71).
- **[사소]** 강화 연출이 끝날 때(ExitCeremonyView) PotentialBox를 다시 SetActive(true) 하므로 Animator가 리바인드되어 Flame_Start가 매번 재생된다. 원작은 재등장 시 곧바로 Flame_Loop다.
  - 근거: 코드: UI_EnhacePopup.cs:136 CEREMONY_HIDE 에 GameObjects.PotentialBox 포함 → SetActive(false)/(true). 프리팹 Animator m_KeepAnimatorStateOnDisable: 0 (prefab:5893 부근). 원작 ref30 f281(강화창 재등장 첫 프레임) 불꽃 top=335, 즉 이미 최대치(f99 상당) — 상승 없음.
- **[사소]** 잠재 상자 자체의 등장 방식이 다르다. 원작은 프리팹 저작 DOTweenAnimation(LocalMove, isFrom (50,40,0), 0.5s, ease 27) 하나로 19u 위에서 내려앉을 뿐 알파 페이드가 없다. 우리는 그 저작 트윈을 rt.DOKill()로 죽이고 코드가 만든 24u 드롭 + 0.28s 알파 페이드로 대체한다.
  - 근거: 원작 Potential Box 1416030580: animationType 2, isFrom 1, endValueV3 (50,40,0), duration 0.5, delay 0, easeType 27, autoPlay 1. 우리 프리팹에도 같은 컴포넌트(&1890780721558180411)가 있으나 UI_EnhacePopup.cs:298~301 의 cg.DOKill(); rt.DOKill(); 로 무효화된 뒤 INTRO_DROP=24f / INTRO_FADE=0.28f 로 다시 만들어진다.

## 수정안
파일: `Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs`  (수정 1곳, 추가 1곳. 프리팹·애니메이션·스프라이트는 손댈 것이 없다 — 이미 원작과 동일하다.)

■ 1) 93~94행 — Init()에서 즉시 켜지 않는다 (프리팹이 이미 m_IsActive:0 이므로 삭제만 하면 된다)

  삭제:
```csharp
        // 잠재 박스의 불꽃 — 원본은 이 자리에서 불길이 계속 타오른다.
        GetObject((int)GameObjects.FlameImg)?.SetActive(true);
```

■ 2) `PlayIntro()` (278~304행) 의 foreach 루프 **직후**, 메서드 닫는 `}` (304행) 바로 앞에 추가:

```csharp
        // 불꽃은 잠재 슬롯이 자리에 앉은 다음에 붙는다.
        //  원본(ref30, 10fps): f91~93 상자 내려앉음 → f94 점화 → f99까지 0.5초에 걸쳐 솟는다.
        //  Init()에서 켜 두면 Flame_Start(0.583s)가 alpha=0인 0.80초 안에서 다 타 버려서
        //  상자가 보일 땐 이미 Flame_Loop다 — "아래에서 위로"가 사라졌던 원인.
        var flameGo = GetObject((int)GameObjects.FlameImg);
        if (flameGo != null)
        {
            flameGo.SetActive(false);
            DOVirtual.DelayedCall(FLAME_IGNITE, () =>
            {
                if (flameGo == null) return;
                flameGo.SetActive(true);
                // 부모가 껐다 켜지는 경로(연출 종료)에서도 확실히 처음부터 돌린다.
                flameGo.GetComponent<Animator>()?.Play("Flame_Start", 0, 0f);
            }).SetUpdate(true);
        }
```

■ 3) 306~307행의 상수 옆에 추가:

```csharp
    // 잠재 슬롯 등장(0.80f)이 끝나는 시점 = 원본의 상자 착지 직후 점화 지점
    const float FLAME_IGNITE = 0.80f + INTRO_FADE;   // 1.08s
```

주의/검증
- `DOVirtual` 은 이미 이 파일에서 쓰고 있다(955행) — using 추가 불필요.
- `PlayIntro()` 는 `_introPlayed` 로 인스턴스당 1회이고 팝업은 열 때마다 새로 Instantiate 되므로 항상 실행된다. 혹시 실행되지 않는 경로가 있어도 1)을 지운 상태면 불꽃이 꺼진 채 남으니, 안전을 원하면 1)을 지우는 대신 남겨 두고 2)의 `flameGo.SetActive(false)` 가 되돌리게 해도 결과는 같다.
- Animator 기본 상태가 이미 Flame_Start 이므로 `SetActive(true)` 만으로도 재생된다. `Play(...)` 는 보험.
- 검증법: `unity-cli editor refresh --compile` → `console --type error` → 플레이 모드에서 강화창을 열고 15fps 이상으로 캡처해 잠재 상자 영역(1080x2640 기준 x740..860, y1040..1200)의 주황 픽셀 bbox 상단 y를 프레임별로 재라. **첫 등장 프레임의 불꽃 폭이 20u 이상(뭉툭한 flame3~5)** 이고, 이후 5~8프레임에 걸쳐 상단이 상자바닥 대비 약 4u → 36.7u 로 올라가면 원작과 같다.


# 강화 성공 시 전체 연출 (담금질 백열 → 결과 백색광 → 결과창 등장 대본)

## 원작
■ 구조 (가장 중요)
원작은 성공/실패 연출 **전체**(망치 3타 + 담금질 + 백색광 + 결과 표시)를 `EnhanceProgress Popup` **한 창 안**에서 한다.
- `Enhace Popup/Enhance Btn` 그래프(AI/forge_ref/g_enhbtn.txt): `[21] If(isSkip)` → **ifFalse** → `[181] SetActive(EnhanceProgress Popup,true)` → `[252] TriggerCustomEvent("Success")`. **ifTrue**(건너뛰기) 쪽에서만 `[211] SetActive(Flash_Success,true)`.
  → 원작 정상 성공 연출에 전체화면 Flash_Success는 **없다**. 창 교체도 없다.
- `Enhace Popup/Hammer Normal Anim` = Normal.anim(3.02s) 대기 루프 전용. 연출용 클립(Success/Fail 5.0167s)은 `EnhanceProgress Popup/Hammer(IncludeAnim)`에 붙어 있다(Animator guid 2c5cd03d46ad88c489dd23e2a2df215a = Forge Smith.controller).

■ 초 단위 대본 — T=0 은 EnhanceProgress Popup SetActive(true) (= 화면 암전 순간)
루트 그래프 `EnhanceProgress Popup / "Anim"`:
 T+0.00 Image(전체 검정, RGBA 0,0,0,1) Color **isFrom (0,0,0,0) / 0.25s / InOutQuad** → 암전
 T+0.00 PlaySFX
 T+0.10 Icon.DORestartById("4") = PunchScale 0.3s OutBack, punch(0.3,0.3,0), elasticity 0.367
 T+0.40 Icon.DORestartById("2") = LocalMove→(0,0,0) 0.3s OutBack
 T+0.70 Hammer(IncludeAnim).Animator.Play("Success")   ← Success.anim 시작

Success.anim(60fps, 5.0167s) 이벤트 → `Hammer(IncludeAnim)` 그래프(AI/forge_ref/g_hammer.txt):
 clip 0.083 (T+0.783) SmashSound
 clip 0.250 (T+0.950) Curious SFX
 clip 0.333 (T+1.033) **Shake** → Icon id"1" ShakePosition 0.25s (4,4,0) vib90 / `Icon/forAnim/Flash` SetActive(true)(Fade 0.25s OutBack, autoPlay) / HammerSmash SFX / Hammer 자기 ShakePosition 0.25s (2.5,2.5,1) vib90 / Floor PunchPosition 0.3s (2,1.5,1)
 clip 1.083 (T+1.783) SmashSound
 clip 1.333 (T+2.033) Shake (2타, 위와 동일)
 clip 2.083 (T+2.783) SmashSound
 clip 2.333 (T+3.033) **ChargeSuccess**
    · Bg SetActive(true); Bg id"51" Color→RGBA(0.5882,0.5882,0.5882,1) **1.4s InSine** [T+3.033→T+4.433]
    · Hammer ShakePosition 0.25s / Floor PunchPosition 0.3s / HammerSmashLong SFX
    · Icon id"5" LocalMove→(0,50,0) 1.4s InSine **+** Scale→1.4 1.4s InSine
    · Icon/forAnim ShakePosition 1.7s (1.5,1.5,1) vib90
    · SuccessFlash id"1" Color→(1,1,1,0.627451) 1.4s InOutQuad
    · Timer 1.501 → T+4.534 HammerSmashSuccess SFX
    · Timer 0.21  → T+4.744 SuccessFlash id"2" Color→(1,1,1,1) 0.3s InOutQuad
    · Timer 0.31  → T+5.054 SuccessFlash id"3" Color→(1,1,1,0) 0.5s InOutQuad
 clip 3.750 (T+4.450) Serious SFX
 clip 3.833 (T+4.533) **Success**
    · PlaySFX; **Bg id"52" Color→(0.8627,0.8627,0.8627,1) 0.15s InSine** [T+4.533→T+4.683]  ← 백색광 정점 계단
    · Icon id"6" LocalMove→(0,80,0) 0.3s InOutBack
    · Timer 0.2 → T+4.733 **Bg id"53" Color→(0,0,0,1) 1.8s InOutQuad** [→T+6.533]
    · Timer 0.3 → T+5.033 Icon id"4" PunchScale 0.3s OutBack
 clip 5.000 (T+5.700) **SuccessDone**
    · Timer 0.2 → **T+5.90**: SetActive(Success Txt Group) / Icon id"7" LocalMove→(0,20,0) **1.2s InSine** + Scale→1 **1.0s InSine** / SetActive(God_ray_02) / W Icon·Flash_USETHIS material 리셋
    · Timer 1.7  → **T+7.60** SetActive(Enh Att Detail)   (Att Text: LocalMove isFrom(0,-80,0) 0.5s InOutBack + Fade→1 0.5s InOutQuad)
    · Timer 0.25 → **T+7.85** SetActive(Att Real Text)
    · Timer 1.85 → **T+9.70** SetActive(Exit Btn)   ← 그전엔 닫을 수 없다

`Success Txt Group` 자체 그래프(OnEnable 기준, 그룹 켜짐 = T+5.90):
 +0.1 (T+6.00) -0 / -1 / -2 SetActive(true)
   -0 "ENHACE\n\nSUCCEEDED" (UI.Text, font 30, lineSpacing 0.8, scale 0.5, anchoredPos (0,120)): Fade isFrom 0 **0.5s InSine delay 0.3** + Scale isFrom **2** 0.5s InOutBack delay 0.3
   -1(0,145) / -2(5,95) 가로선 Image(guid 33bcb4e77f751b548abea745e0c836cf): Fade isFrom 0.4s InSine
 +0.8 (T+6.80) -3 (장비명, font 22 bold, (0,78), scale 0.5) : Fade→1 0.5s InSine + LocalMove isFrom(0,88,0) 0.5s InOutBack
 +0.5 (T+7.30) -4 (강화된 장비명) : Fade 0.3s InSine + LocalMove isFrom(0,105,0) 0.3s InOutBack + PunchScale 0.3s (0.3,0.3) elasticity 1
 +0.1 (T+7.40) -3.DORestartById("1") : LocalMove→(0,55,0) 0.3s InOutQuad + Fade 0.3s InOutQuad (원래 이름이 밀려나며 사라짐)

■ Flash_Success / Flash_Fail (Enhace Popup/Skip Flash Group) — 건너뛰기 전용
 둘 다 RectTransform stretch(anchor 0,0~1,1 / sizeDelta 0,0), Image **m_Sprite = 0(무지 사각형)**, Material 0, RaycastTarget 1.
 Flash_Success color (1,1,1,**a=0**), Flash_Fail color (**0,0,0**,a=0)  ← 실패는 흰색이 아니라 **검정**
 Animator controller = Flash.controller (guid 83c1a7f9bfd0c0642a0e75a25a1dbdd3), 파라미터 없음/트랜지션 없음, DefaultState = "Flash"
 Flash.anim (0.3333s, 60fps, LoopTime 0) — m_Color.a 커브: 0s:0 → 0.0333s:**0.25** → 0.0833s:0.25 → 0.3333s:0. 이벤트 t=0.3333 TriggerUnityEvent("Done") → 자기 SetActive(false)
 Fast Flash.anim (0.1667s, 미사용 상태) — a: 0 → 0.0333:**0.3** → 0.05:0.3 → 0.1667:0. 이벤트 없음. Flash.controller의 두 번째 스테이트지만 전이가 없어 Animator.Play("Fast Flash") 없이는 재생되지 않는다.

■ God_ray_02 (EnhanceProgress Popup) — 파티클 저작값
 duration 5s, looping 1, **prewarm 1**(켜는 순간 이미 화면 가득), maxParticles 30, scalingMode 1, localScale 1.2
 startLifetime random 3~4 / startSpeed 0.2 / startSize 1.5 / startRotation 0
 startColor random-between-colors 흰색 a=0.7059 ~ a=0.4706, ColorOverLifetime α 0→0.5882
 Shape type 4(Cone) angle 0 length 5, Transform localRotation q(0.48296294,-0.22414385,0.1294095,0.8365163)
 Material guid 6346087c85fad534d937c56fd3e79138

■ 실측 (ref30, T0 = f173 = 217.30s, 상단밴드 y60..140 평균휘도)
 T+1.00 f183 1타 섬광 p99 123→**253** (상승 0.1s, 하강 0.2s에 123 복귀)
 T+2.05 f193~194 2타
 T+3.00 f203 담금질 시작 (p99 253)
 T+3.10→T+4.40 상단밴드 12 → 150   [Bg id51 램프와 일치]
 T+4.50→**T+4.70 f220 최대 218/255**  [Bg id52 = 0.8627×255 = 220 ✓]
 T+4.70→T+6.30 218 → 11 (바닥)    [Bg id53 1.8s ✓]
 **T+5.90 f232 God ray 한 프레임에 등장** (38 → 58.6), 컷 없음
 T+6.85 f241/242 이름 밴드 최저→상승 (=-3 등장, 예측 T+6.80)
 T+7.70 f250 "능력치 변화" 등장 (예측 T+7.60)
 T+8.00 f253~ 표 네 열이 0.1s 간격으로

## 우리
■ 구조
망치 연출은 `UI_EnhacePopup`(GameObjects.HammerNormalAnim에 Forge Smith Animator를 Play), 결과는 `UI_EnhanceProgressPopup` — **두 창으로 쪼개져 있다**.
UI_EnhacePopup.cs:867-901 `CoSmithMotion` → 클립 길이만큼 기다린 뒤 `ExitCeremonyView()` + `ShowPopupUI<UI_EnhanceProgressPopup>()`.

■ 현재 상수 (읽은 시점 기준. UI_EnhacePopup.cs는 다른 세션이 방금 손댔다 — git status M)
UI_EnhacePopup.cs:865  SMITH_LEAD_IN = 0.37
UI_EnhacePopup.cs:1206 CHARGE_PEAK = 0.38  / :1207 CHARGE_RAMP = 1.50 / :1208 WASH_OUT = 1.00
UI_EnhacePopup.cs:1239 ChargeGlow() = GlowWash(0.38, 1.50, **Ease.InQuad**)
UI_EnhacePopup.cs:1173-1191 PopResultFlash(true) = GlowWash(**0**, 1.00, OutQuad) + Blackout(0.35). Flash_Success는 이제 호출하지 않음(주석으로 건너뛰기 전용이라 명시) ✔ 이미 고쳐짐
UI_EnhacePopup.cs:1149-1150 ICON_WHITE_HOLD 0.25 / ICON_WHITE_OUT 0.55
UI_EnhanceProgressPopup.cs:71-73 STEP_TITLE 0.20 / STEP_DETAIL 1.70 / STEP_TABLE 0.25  ← 원작 Timer 0.2/1.7/0.25와 일치 ✔
UI_EnhanceProgressPopup.cs:86-87 WASH_LEFT 0.18 / WASH_TAIL 0.45 (Ease.OutQuad)
UI_EnhanceProgressPopup.cs:576-578 ICON_SETTLE_Y 20 / DUR 1.2 / SCALE_DUR 1.0, 단 :646-650 DescendIcon()은 **Ease.Linear**
UI_EnhanceProgressPopup.cs:675-678 RAY_IN 0.10 → RAY_LOW 0.075(0.9s) → RAY_HIGH 0.22(2.0s) → RAY_REST 0.12(1.6s)
UI_EnhanceProgressPopup.cs:195-217 STEP_TITLE 시점에 SuccessTxtGroup **통째로** Reveal (제목·가로선·장비명이 동시에 뜬다). 이름 교체(PlayAuthoredOn("1") + Texts._4)는 :219-226 STEP_DETAIL 뒤.
ExitBtn: :46 Buttons.ExitBtn, :109 리스너만 등록. GameObjects에 없고 지연 활성화도 없다 → 연출 시작부터 아무 데나 누르면 닫힌다.

■ 실측 (Screenshots/demo30, 이전 녹화본 1080x2640/15fps, T0 = F0154 = 10.267s. ※ 이 폴더는 지금 다른 세션이 재녹화 중이라 파일이 교체됐다. 아래 수치는 CHARGE_PEAK=0.525 시절 빌드다)
 T+0.93 F0168 1타 섬광 시작, **T+1.27 F0173 정점 p99 184** (상승 0.33s / 하강 0.4s)
 T+2.06→T+2.33 2타 (정점 p99 185)
 T+3.13 F0201 3타 정점 p99 **122**
 T+3.27→T+5.00 램프 (mean 16 → **172.2**), p99 255가 **0.55초간 포화**
 **T+6.27 F0248 팝업 교체 컷 — 한 프레임에 mean 53.4 → 14.9 (-38)**
 T+6.53 F0252 God ray 등장

## 차이
- **[치명]** 창 교체 컷 — 원작은 망치질부터 결과까지 EnhanceProgress Popup 한 창이라 화면이 끊기지 않는데, 우리는 강화창→결과창으로 갈아타며 백열 잔광이 한 프레임에 뚝 끊긴다
  - 근거: 원작: AI/forge_ref/g_enhbtn.txt [21] If(isSkip).ifFalse → [181] SetActive(EnhanceProgress Popup) → [252] CustomEvent("Success") — 클립도 God ray도 Bg도 전부 이 창 소속. 우리: UI_EnhacePopup.cs:895-901 ExitCeremonyView() 후 ShowPopupUI. 실측 demo30 F0247→F0248 mean 53.42→14.94 (한 프레임 -38.5, |diff| 40.51). 같은 구간 ref30은 f235(30.03)→f236(26.68) 매끄러운 감쇠
- **[치명]** 결과 확정 순간의 '백색광 정점 계단'이 없다. 원작은 담금질 평지(0.5882)에서 0.15초 만에 0.8627로 한 번 더 치솟은 뒤 0.2초 지나 1.8초에 걸쳐 검정으로 내려간다. 우리는 0.38까지 단조 상승 후 곧바로 1.0초 하강 — 성공의 '한 방'이 통째로 빠져 있다
  - 근거: 원작 Bg DOTweenAnimation: id"51" Color→(0.5882,0.5882,0.5882,1) 1.4s InSine / id"52" →(0.8627,…) 0.15s InSine / id"53" →(0,0,0,1) 1.8s InOutQuad (AI/forge_ref/dt_progress.txt). 그래프 순서 g_hammer.txt: 238"Success"→28(id52)→135→264(T0.2)→36(id53). 실측 ref30 상단밴드 f217 150 → f220 218(=0.8627×255=220) → f231 38. 우리: UI_EnhacePopup.cs:1239 GlowWash(0.38,1.50,InQuad) → :1180 GlowWash(0,1.00,OutQuad), 계단 없음
- **[눈에띔]** 장비명(-3)·강화명(-4)이 0.9초 일찍/0.3초 늦게 뜬다. 원작 Success Txt Group은 자체 그래프로 0.1 / +0.8 / +0.5 / +0.1 계단을 밟는데 우리는 그룹을 한 번에 켠다
  - 근거: 원작 그래프(EnhanceProgress Popup/Success Txt Group): OnEnable→T0.1→-0/-1/-2 on→T0.8→-3 on→T0.5→-4 on→T0.1→-3.DORestartById("1"). 절대시각 T+6.00 / T+6.80 / T+7.30 / T+7.40. 실측 ref30 name밴드(y263..292) 최저 f241~242(224.1~224.2)에서 상승 시작 = T+6.85. 우리: UI_EnhanceProgressPopup.cs:203 Reveal(SuccessTxtGroup) 한 방(D+0.20), 이름 교체는 :224-226 D+1.90
- **[눈에띔]** 결과창 인계 시 남은 백열값이 틀렸다. WASH_LEFT 0.18은 원작 곡선의 해당 시점 값(0.370)의 절반이고, 꼬리도 0.45초로 짧다(원작 잔여 0.833초)
  - 근거: 원작 Bg id"53"은 clip 4.033부터 1.8s InOutQuad. 우리 결과창이 열리는 SuccessDone(clip 5.0)은 그 중 0.967s 경과 지점 = u 0.537 → InOutQuad(u)=1-2(1-u)^2=0.5713 → 잔여색 0.8627×(1-0.5713)=0.370, 잔여시간 1.8-0.967=0.833s. 우리: UI_EnhanceProgressPopup.cs:86-87 WASH_LEFT 0.18 / WASH_TAIL 0.45 OutQuad
- **[눈에띔]** 망치 타격 섬광이 '팍' 터지지 않고 뭉근하게 부푼다. 원작은 0.1초 만에 최대(p99 253)에 도달해 0.2초에 사라지는 하드 팝인데 우리는 0.33초 상승·0.4초 하강에 최대 184다. 3타는 특히 약해서 122까지밖에 안 간다
  - 근거: ref30 p99: f182 129 → f183 253 → f185 140 → f186 124. demo30 p99: F0168 101 → F0170 136 → F0173 184(정점) → F0177 123 → F0179 76. 3타 ref f203 253 vs demo F0201 122. 원작 저작값: Icon/forAnim/Flash = Fade 0.25s OutBack autoPlay(SetActive 순간 저작 알파로 즉시 켜졌다가 0.25초에 빠짐)
- **[눈에띔]** 백색광 정점이 원작보다 밝고(전체평균 172 vs 138) 0.55초간 완전 백색으로 포화되며, 시점도 0.3초 늦다
  - 근거: ref30 f220(T+4.70) mean 137.89 / p99 255는 f218~f219 두 프레임뿐. demo30 F0229(T+5.00) mean 172.22 / p99 255가 F0226~F0233 8프레임(0.53s) 연속
- **[눈에띔]** God ray 등장이 0.63초 늦다. 원작은 SuccessDone+0.2에 prewarm된 파티클이 한 프레임에 화면을 채운다
  - 근거: ref30 상단밴드 f231 38.00 → f232 58.57 (T+5.90, 한 프레임). demo30 F0251 7.85 → F0252 42.66 (T+6.53). 지연분은 위의 창 교체 컷과 같은 원인. 원작 God_ray_02: prewarm 1, maxParticles 30, startSize 1.5, life 3~4, startColor α0.47~0.71, ColorOverLifetime α0→0.588, scale 1.2
- **[사소]** 아이콘 하강 이징이 다르다 — 원작 InSine, 우리 Linear
  - 근거: 원작 Icon id"7": LocalMove→(0,20,0) 1.2s Ease.InSine + Scale→1 1.0s Ease.InSine (AI/forge_ref/dt_progress.txt). 우리: UI_EnhanceProgressPopup.cs:646-650 .SetEase(Ease.Linear) 두 번
- **[사소]** Exit Btn이 처음부터 켜져 있어 연출 도중 아무 데나 누르면 결과창이 닫힌다. 원작은 표까지 다 나온 뒤(T+9.70)에야 켠다
  - 근거: 원작 g_hammer.txt: 40(Att Real Text on)→131 Timer 1.85→127 SetActive(Exit Btn,true). 우리: UI_EnhanceProgressPopup.cs:46 Buttons.ExitBtn 은 GameObjects 목록에 없고 :109에서 리스너만 등록, 지연 활성화 코드 없음
- **[사소]** 결과창 인계 시 사라지는 아이콘 백열(SuccessFlash) 값이 다르다
  - 근거: 원작 Icon/forAnim/SuccessFlash (stretch, sizeDelta(-2,-2), localRotation z=90°, color(1,1,1,0)): id"1" →α0.6275 1.4s InOutQuad @ChargeSuccess / id"2" →α1.0 0.3s InOutQuad @+1.711 / id"3" →α0 0.5s InOutQuad @+2.021. 우리: UI_EnhacePopup.cs:1149-1150 ICON_WHITE_HOLD 0.25 + ICON_WHITE_OUT 0.55 (Whiten Ease.Linear)
- **[사소]** Flash_Fail은 흰색이 아니라 검정이다(실패 시 화면이 어두워지는 연출)
  - 근거: share_scene_dump --dump Flash_Fail: m_Color {r:0,g:0,b:0,a:0}, m_Sprite fileID 0, stretch. Flash_Success는 {1,1,1,0}. 둘 다 Flash.controller(83c1a7f9…) DefaultState "Flash" = 0.333s에 α 0→0.25→0.25→0, 끝에서 자기 SetActive(false). 우리 CoSkipFlash(UI_EnhacePopup.cs:1267-1281)는 0.6초 뒤 강제 SetActive(false) — 애니 이벤트가 이미 끄므로 무해하나 값 확인용

## 수정안
■ 1. 백색광 정점 계단 복원 (최우선)  — Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs
현재 CHARGE_PEAK 0.38은 리니어 렌더 보정치이므로 **절대값이 아니라 원작의 비율·시간·이징**을 옮긴다.
원작 비율 0.8627 / 0.5882 = 1.467.

  // :1206~1208 교체
  const float CHARGE_PLATEAU = 0.259f;  // 담금질 평지 = 원작 Bg id"51" 0.5882 (정점 0.38 기준 비율 환산)
  const float CHARGE_RAMP    = 1.40f;   // 원작 id"51" 1.4s  (1.50 → 1.40)
  const float CHARGE_PEAK    = 0.38f;   // 결과 정점 = 원작 Bg id"52" 0.8627 (계측 보정 유지)
  const float PEAK_RISE      = 0.15f;   // 원작 id"52" 0.15s InSine
  const float PEAK_HOLD      = 0.05f;   // 원작 Timer 0.2 − 0.15
  const float WASH_OUT       = 1.80f;   // 원작 id"53" 1.8s  (1.00 → 1.80)

  // :1239 ChargeGlow 교체 — InQuad → InSine
  void ChargeGlow() => GlowWash(CHARGE_PLATEAU, CHARGE_RAMP, Ease.InSine);

  // :1173~1191 PopResultFlash(success == true) 교체
  GlowWash(CHARGE_PEAK, PEAK_RISE, Ease.InSine);                       // 0.15s 급상승
  DOVirtual.DelayedCall(PEAK_RISE + PEAK_HOLD,
      () => GlowWash(0f, WASH_OUT, Ease.InOutQuad)).SetUpdate(true);   // 0.2s 뒤 1.8s 하강
  Blackout(STAGE_CEREMONY, 0.35f);      // 그대로
(실패 경로는 원작이 Bg id"54" →(0,0,0,1) 0.3s **InOutCubic** + SuccessFlash 색을 (0,0,0,0.9804)로 바꾼 뒤 id"4" Fade→0 1.0s InSine 이다. 현재 GlowWash(0, 0.30, OutQuad)는 시간만 맞고 이징이 다르다.)

■ 2. 창 교체 컷 없애기 — 두 가지 중 택1
(a) 최소 수정: 인계값을 넘긴다.
  UI_EnhacePopup.cs — GlowWash용 `_glow` 의 현재 알파를 public 프로퍼티(예: `public float CurrentWash => _glow != null ? _glow.color.a : 0f;`)로 노출하고,
  :899 `ShowPopupUI<UI_EnhanceProgressPopup>().SetInfo(result, _equip)` → `SetInfo(result, _equip, CurrentWash, WASH_OUT - elapsedSinceSuccess)` 로 확장.
  UI_EnhanceProgressPopup.cs:86-87 상수를 인자로 대체하되, 인자가 없을 때의 기본값은 원작 계산값으로:
      const float WASH_LEFT = 0.370f;   // 0.8627 × (1 − InOutQuad(0.967/1.8)) = 0.8627 × 0.4287
      const float WASH_TAIL = 0.833f;   // 1.8 − 0.967
  그리고 :199-203 `bg.DOColor(Color.black, WASH_TAIL).SetEase(Ease.OutQuad)` → `.SetEase(Ease.InOutQuad)`.
(b) 근본 수정(원작과 동형): 망치 클립(Forge Smith Success/Fail)을 UI_EnhanceProgressPopup으로 옮기고, 강화창은 `ShowPopupUI<UI_EnhanceProgressPopup>()` 만 하게 한다. 그러면 루트 대본이 원작 그대로가 된다 — 팝업 OnEnable 후 0.1s Icon id"4", 0.4s Icon id"2", **0.7s Animator.Play("Success")**. (우리 프리팹 Icon에 id 1~9 트윈이 이미 다 있어 SnapAuthored 꼼수를 전부 버릴 수 있다.)

■ 3. Success Txt Group 계단 복원 — UI_EnhanceProgressPopup.cs CoRevealSuccess (:195~226)
`Reveal(GameObjects.SuccessTxtGroup)` 한 방을 원작 그래프대로 쪼갠다. 새 상수:
  const float TXT_LEAD   = 0.10f;   // 그룹 켜짐 → -0/-1/-2
  const float TXT_NAME   = 0.80f;   // → -3 (장비명)
  const float TXT_ENH    = 0.50f;   // → -4 (+n 장비명)
  const float TXT_PUSH   = 0.10f;   // → -3 id"1" 밀어내기
순서(팝업 오픈 기준 절대시각):
  D+0.20  SetGroupActive(SuccessTxtGroup, true) + 자식 -0/-1/-2/-3/-4 는 **꺼 둔 채**로
          DescendIcon(); ShowGodRays();
  D+0.30  Reveal(Txt_SuccessTxtGroup_01) / Reveal(Img_SuccessTxtGroup_02) / Reveal(_2)
  D+1.10  Reveal(Txt_SuccessTxtGroup_03)          ← 지금 D+0.20에 뜬다
  D+1.60  SetRevealText(Texts._4, nmEnh) + Reveal ← 지금 D+1.90
  D+1.70  PlayAuthoredOn(Txt_SuccessTxtGroup_03, "1")  ← 지금 D+1.90
  D+1.90  Reveal(EnhAttDetail)   (STEP_DETAIL 그대로)
  D+2.15  AttRealText            (STEP_TABLE 그대로)
※ -0 은 원작이 Fade isFrom 0 / Scale isFrom **2** / **delay 0.3** / dur 0.5 / Fade InSine·Scale InOutBack 이다. 우리 프리팹에 이 저작값이 없으면 그대로 넣을 것.

■ 4. 아이콘 하강 이징 — UI_EnhanceProgressPopup.cs:648-650
  .SetEase(Ease.Linear)  →  .SetEase(Ease.InSine)   (두 줄 모두. 값 (0,20)/1.2s, scale 1/1.0s 는 이미 원작과 같다)

■ 5. Exit Btn 지연 활성화 — UI_EnhanceProgressPopup.cs
  GameObjects enum에 `ExitBtn` 추가(프리팹 노드 이름 일치), SetInfo에서 `SetGroupActive(GameObjects.ExitBtn, false)`,
  CoRevealSuccess 맨 끝(현재 `PlayAuthored(AttRealText, "2")` 뒤)에
      yield return Wait(EXIT_DELAY);  //  const float EXIT_DELAY = 1.85f;  ← 원작 Timer 1.85
      SetGroupActive(GameObjects.ExitBtn, true);
  (원작 절대시각 D+4.00 = 팝업 오픈 후 4.0초)

■ 6. 타격 섬광을 하드 팝으로 — UI_EnhacePopup.cs PopImpactFx(:1008~)
  현재 `Whiten()`/UIFx 버스트가 0.33초에 걸쳐 부푼다. 원작 `Icon/forAnim/Flash` 는 **SetActive 즉시 최대 → Fade 0.25s Ease.OutBack → 0** 이다.
  Whiten 호출을 `Whiten(1f, 0f); DOVirtual.DelayedCall(0f, () => Whiten(0f, 0.25f))` 형태(=즉시 최대 후 0.25s OutBack)로 바꾸고, `Whiten()` 의 `.SetEase(Ease.Linear)` 를 `Ease.OutBack` 으로.
  3타(clip 2.083 SmashSound)에는 원작에 Shake 이벤트가 없다 — 그 자리의 밝은 섬광은 clip 2.333 **ChargeSuccess** 의 담금질 시작이다. 현재 ChargeSuccess 케이스(:975~985)에 Floor PunchPosition(0.3s, (2,1.5,1)) 과 Hammer ShakePosition(0.25s, (2.5,2.5,1), vib 90) 이 빠져 있으니 추가할 것.

■ 7. 건너뛰기 경로 값 확인 — UI_EnhacePopup.cs:1267-1281
  원작 Flash.anim 은 0.3333초에 스스로 꺼진다(이벤트 Done → SetActive(false)). 우리 코루틴의 0.6초 대기는 그보다 길다 — 0.35f 로 줄이면 원작과 같아진다.
  Flash_Fail 이 **검정**(0,0,0)인지 프리팹에서 확인할 것. 흰색으로 포팅돼 있으면 (0,0,0,0)으로 고친다.

■ 참고 산출물 (커밋 대상 아님)
  AI/forge_ref/dump_progress.txt  — EnhanceProgress Popup 전체 YAML
  AI/forge_ref/dt_progress.txt    — 그 안 DOTweenAnimation 55개 원문
  AI/forge_ref/g_hammer.txt       — Hammer(IncludeAnim) 마스터 그래프(디코드)
  AI/forge_ref/g_enhbtn.txt       — Enhace Popup "Enhance Btn" 그래프(isSkip 분기)
  AI/forge_ref/godray.txt         — God_ray_02 파티클 원문


# 아래에서 위로 올라오는 불티 (Btn Embers / Fire_Embers 파티클)

## 원작
원작은 진짜 ParticleSystem 4개 묶음이다. 경로 `?/UI/FIXED/In-Frame/Popup/Enhace Popup/Btn Light/Btn Light1/Btn Embers` (SampleScene.unity GameObject 23249075). EpicVictoryEffects의 `Fire_Embers_01` + `Fire_Embers_02` 프리팹을 합친 것.

■ 계층·트랜스폼 (부모 Btn Light1 은 anchoredPosition (0,-36.48), stretch)
  Btn Embers  Transform pos(0,-50,0.5) scale(3,3,3)  ← 팝업 프레임 중심 기준 y=-86.48 캔버스유닛
   ├ Dust_01           pos(-1.4,-0.01,-0.1) rot=Rx(-90°)  [quat -0.7071068,0,0,0.7071068]
   └ GlowDot_02        pos(0.62,0.3,0)  rot quat(-0.25339475, 0.099828415, 0.81386006, 0.5132809)
       └ GlowDot_02_Light pos(0,0,0) rot 단위

■ Btn Embers (루트, PS &23249078) — **연출용 아님, 껍데기**
  lengthInSec 0.05 / looping 1 / prewarm 1 / playOnAwake 1 / scalingMode 1(Local)
  lifetime 0.0001~0.0001, speed 0, startSize X 0 / Y 10, color (1,1,1,a=0.1176), gravity 0, maxParticles 4
  EmissionModule enabled=0, ShapeModule enabled=0, **Renderer m_Enabled = 0** → 아무것도 안 그린다
  Renderer: mode 0(Billboard) sortingOrder 200, material GlowDot_01_Light.mat(guid f60e279a68ed89f41966c8993b9bbc6d)

■ Dust_01 (PS &1764607653) — 오른쪽으로 흐르는 소수의 굵은 불티
  lengthInSec 0.15 / looping 1 / prewarm 1 / scalingMode Local
  lifetime rand 0.5~2 / startSpeed rand 0~0.5 / startSize rand 0.04~0.15 / gravityModifier -0.05 / maxParticles 350
  Emission: rateOverTime 0, **Burst t=0 count rand 1~4, cycles 1, interval 0.01, prob 1** (0.15s 루프 → 초당 7~27개)
  Shape: Cone(type 4) radius 5.5, angle 40°, arc 360, radiusThickness 1, pos/rot 0
       → 로컬 +Z 방출, Rx(-90) 때문에 **월드 +Y(위)**
  VelocityOverLifetime(local): x rand 1~2, y 0, z 0 → 로컬 X = 월드 X = **오른쪽 1~2 units/s** (스케일3 → 3~6)
  ColorOverLifetime: RGB (0.7264,0,0.0287) → (1,0.4838,0), alpha 0@0 / 1@0.20 / 1@0.80 / 0@1.0
  SizeOverLifetime: 1.0 → 0.4947 (선형)
  Noise: strength 1, frequency 0.5, damping 1, quality 2(High), scrollSpeed 0
  Renderer: mode 0(Billboard) lengthScale 2 velocityScale 0 maxParticleSize 0.5 sortingOrder 200, material GlowDot_01_Light.mat

■ GlowDot_02 (PS &1369996086) — **화면을 채우는 주력 불티. 이게 위로 올라간다**
  lengthInSec 0.15 / looping 1 / prewarm 1 / scalingMode Local
  lifetime rand 0.5~1 / startSpeed rand 0~4 / startSize rand 0~0.15 / gravity 0 / maxParticles 350
  Emission: rateOverTime **100/s**, burst 없음
  Shape: Cone radius 5, angle 40°, arc 360 → 로컬 +Z = 월드 (-0.310, +0.423, +0.852) (거의 카메라 쪽, 약간 위)
  VelocityOverLifetime(local): x rand 2~3 → 로컬 X축 = 월드 **(-0.345, +0.785, -0.515)**
       ⇒ 월드 속도 = 위 (0.785×2~3)×3 = **+4.7 ~ +7.1 units/s**, 왼쪽 (0.345×2~3)×3 = 2.1~3.1 units/s
  ColorOverLifetime: RGB (1,0.3305,0) → (1,0.3432,0) [주황], alpha 0@0 / 1@0.20 / 1@0.80 / 0@1.0
  SizeOverLifetime: 1.0 → 0.4947
  Noise: strength 1, freq 0.5, damping 1, quality 2
  Renderer: **mode 1(Stretched Billboard)**, lengthScale 1, **velocityScale 0.03**, maxParticleSize 0.5, sortingOrder 200, material GlowDot_02_Light.mat(guid 06ad6dce7faad1144a5e7355c1983c1a)

■ GlowDot_02_Light (PS &1790794996) — 위와 동일 궤적의 밝은 겹
  GlowDot_02와 모든 값 동일. 차이: startSize rand 0.05~0.25 / startColor (1,1,1, **a=0.3529**) /
  ColorOverLifetime RGB (1,0.8706,0)→(1,0.4078,0) [노랑→주황] / Renderer velocityScale **-0.01**

■ 재질/텍스처 (RE001_Share/Assets/IMPORTS/EpicVictoryEffects)
  GlowDot_01_Light.mat guid f60e279a68ed89f41966c8993b9bbc6d → _MainTex GlowDot_01.png (guid 96c77a8606a85af43a4af1dad22f4e7a)
  GlowDot_02_Light.mat guid 06ad6dce7faad1144a5e7355c1983c1a → _MainTex GlowDot_02.png (guid 54af37098c5644c40829ebd1b80be564)
  둘 다 built-in Legacy Particles 셰이더(m_Shader fileID 10720), _EmisColor (1, 0.8907, 0.2794, 1)
  Assets/Particles/Materials 의 ember_1.mat / Ember.psd 는 이 연출에 **안 쓰인다**(대장간 불티 아님)

■ 같은 팝업의 God_ray_01/02 하위 Dust_01 은 불티가 아니다
  God_ray_01 rot=Rx(+90) → 콘이 월드 **-Y(아래)**, 자식 Dust_01 lifetime 5~8, rate 80/s, 색 (1,0.8745,0.4549) = 빛기둥 속 먼지
  God_ray_02 rot quat(0.48296294,-0.22414385,0.1294095,0.8365163), 자식 Dust_01 색 (0.3294,0.7569,1.0) = **하늘색** 먼지
  → 위로 오르는 주황 불티는 오직 Btn Embers 하위 3종

■ 영상 실측 (ref30, 344x854, 프레임 f174~f209 = 217.4~220.9s, 배경 최소값 차감 후 어두운 영역만)
  · 프레임당 검출 64.7개 (임계 diff≥70)
  · bbox 폭/높이 중앙값 2px, 면적 중앙값 4px, p90 17px  → 캔버스 1.05~2.4 유닛짜리 점
  · y 분포: p5 379 / p50 517 / p95 731 (화면 0.44~0.86), 250px 위로는 거의 없음
  · x 분포: p5 22 / p50 164 / p95 314 → **화면 폭 거의 전부**
  · 밝은 화소 색 중앙값 RGB(110,109,90), 상위20% 평균 RGB(182,166,143) — 작고 additive라 창백한 주황
  · 이동(고립 불티 상호최근접 매칭 90쌍): **dy 평균 -6.22 px/0.1s = -62 px/s(위), 71%가 위로**
    dx 평균 -1.86 px/0.1s = -19 px/s(왼쪽). 빠른 개체는 f177(323,387) dy-21.0, f178(324,366) dy-27.6
  · 캔버스 단위 환산(1 ref px = 0.5233 캔버스유닛): **위로 32.5 유닛/s, 왼쪽 9.7 유닛/s**
    → GlowDot_02 이론값(위 27~41, 왼쪽 12~18 유닛/s)과 일치. 파티클 1 world unit ≈ 11 ref px ≈ **5.75 캔버스유닛**
  · 방출 디스크: Shape radius 5 × scale 3 = 15 world unit = 86 캔버스유닛 반경(캔버스 반폭 90) → 화면 전폭
  · 존재 구간: f150(215.0s) 이전부터 f295(229.5s)까지 **연속**. 백색광(f210~225) 동안만 대비에 묻힘

## 우리
■ 결정적 사실: **원작 파티클이 이미 1:1로 이식돼 있는데 꺼져 있다.**
  Assets/@Resources/Prefab/UI/Popup/UI_EnhacePopup.prefab
    EnhacePopup > BtnLight(active=1) > **BtnLight1 (GameObject &3844141278252671996, 줄 23303, m_IsActive: 0)**
       > **BtnEmbers (GameObject &4101053665091121537, 줄 24257, m_IsActive: 0)**  [Transform pos(0,-50,0.5) scale(3,3,3)]
           ├ VFX_BtnEmbers_04 (&338763094091761623, PS &6335928813207645242) = 원작 Dust_01 그대로
           │    pos(-1.4,-0.01,-0.1) rot quat(-0.7071068,0,0,0.7071068), lifetime 0.5~2, burst 1~4/0.15s,
           │    Shape Cone r5.5 a40, Velocity local x 1~2, gravity -0.05, material f60e279a68…
           └ GlowDot_02 (PS &4987199510031674102)  pos(0.62,0.3,0) rot quat(-0.25339475,0.099828415,0.81386006,0.5132809)
                 lifetime 0.5~1, rate 100/s, Shape Cone r5 a40, Velocity local x 2~3, mode 1, velScale 0.03
                └ GlowDot_02_Light (PS &2790261323145191745) startColor a=0.3529, size 0.05~0.25, velScale -0.01
  → 원작 YAML과 **모든 모듈 값이 완전 동일**(위 original 항목의 숫자와 한 글자도 다르지 않음).
  → 그런데 `BtnEmbers` / `BtnLight1` 어느 것도 코드에서 참조되지 않는다:
     `grep -n "BtnEmbers\|BtnLight" Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs` → 0건.
     UI_EnhacePopup.cs 의 enum GameObjects 에도 없음. 즉 **한 번도 켜진 적이 없다.**

■ 실제로 화면에 나오는 것: UI Image 흉내
  Assets/@Scripts/UI/UIFx.cs:327  `public static void SpawnEmber(RectTransform host, float halfWidth = 78f)`
    :331  x = Random.Range(-78, 78)
    :332  y = Random.Range(-40f, 120f)
    :334  크기 (1.6, Random 2.5~5) — **세로로 긴 막대**
    :336-338 색 25% (1,0.95,0.75,0.9) / 75% (1, 0.45~0.75, 0.15, 0.9)
    :340  localRotation = Euler(0,0, Random 30~60°) — 고정 사선
    :346  life = Random.Range(1.4f, 2.6f)
    :347-348 fall = (x ± 14, **y - Random.Range(70f, 150f)**)  ← 아래로 70~150 유닛
    :351  DOAnchorPos(fall, life).SetEase(Ease.InQuad)   ← **낙하**
  호출: UI_EnhacePopup.cs:235 (EMBER_INTERVAL 0.16f, EMBER_PER_TICK 2 → 12.5개/s)
        UI_EnhanceProgressPopup.cs:721 (동일 상수)

■ 우리 녹화 실측 (Screenshots/demo30, 1080x2640 15fps, f160~f211)
  · 프레임당 39.4개, 면적 중앙값 48px(≈7×7 native px = 1.2 캔버스유닛)
  · y p5 938 / p50 1414 / p95 1622 (화면 0.36~0.61) → **모루 위 좁은 띠**
  · x p5 262 / p95 880 (화면 0.24~0.81)
  · 상호최근접 1608쌍: **dy 평균 +4.80 px/frame = +72 native px/s (아래로)**, dx ≈ 0
  · 색 중앙 RGB(179,141,105)

■ 인프라는 이미 갖춰져 있다
  · Packages/manifest.json:3 `"com.coffee.ui-particle": "https://github.com/mob-sakai/ParticleEffectForUGUI.git"` (설치됨)
  · UIFx.cs:151 `PlayParticle(RectTransform, Vector2, string prefabKey, float lifetime)` — UIParticle 래퍼 기존 경로
  · 선례: UI_AreaBtnGroup.cs:64 / UI_GameScene.cs:870 이 "GlowDot_01" 어드레서블(UI.asset:67, PreLoad)로 동일 방식 사용 중
  · 재질/텍스처 GUID 4개 모두 우리 프로젝트에 동일 GUID로 존재 (Assets/IMPORTS/EpicVictoryEffects/…)
  · Fire_Embers_01.prefab(root+Dust_01) / Fire_Embers_02.prefab(root+GlowDot_02+_Light) 도 그대로 있음

## 차이
- **[치명]** 이동 방향이 정반대다. 원작 불티는 위로(약간 왼쪽으로) 오르고, 우리 것은 아래로 떨어진다.
  - 근거: 원작 ref30 f174~f209 고립 불티 상호최근접 90쌍: dy 평균 -6.22 px/0.1s = -62 ref px/s, 71%가 위로 (캔버스 32.5 유닛/s 상승). 이론값도 GlowDot_02 VelocityOverLifetime local x(2~3)이 월드 (-0.345,+0.785,-0.515) 방향 → 위 4.7~7.1 units/s. / 우리: UIFx.cs:347-348 `y - Random.Range(70f,150f)`, :351 DOAnchorPos + Ease.InQuad. demo30 f160~f211 상호매칭 1608쌍 dy 평균 +72 native px/s(아래).
- **[치명]** 원작 파티클 3종이 우리 프리팹에 완벽히 이식돼 있는데 부모 두 개가 비활성이라 한 번도 재생되지 않는다. 그래서 Image 흉내(SpawnEmber)가 대신 붙었다.
  - 근거: UI_EnhacePopup.prefab 줄 23303 GameObject &3844141278252671996 'BtnLight1' m_IsActive: 0 / 줄 24257 &4101053665091121537 'BtnEmbers' m_IsActive: 0. 하위 VFX_BtnEmbers_04·GlowDot_02·GlowDot_02_Light 의 모든 모듈 값이 원작 SampleScene &1764607653/&1369996086/&1790794996 과 동일. grep 'BtnEmbers|BtnLight' UI_EnhacePopup.cs → 0건.
- **[눈에띔]** 불티가 퍼지는 범위가 좁다. 원작은 화면 폭 전체 + 모루 아래 아치 속까지 채우고, 우리는 모루 위 좁은 띠에만 있다.
  - 근거: 원작 y p5 379 / p95 731 of 854 (0.44~0.86), x p5 22 / p95 314 of 344 (0.06~0.91). 우리 y p5 938 / p95 1622 of 2640 (0.36~0.61), x p5 262 / p95 880 of 1080 (0.24~0.81). 원인: 원작 방출 디스크는 Shape radius 5×scale 3 = 15 world unit ≈ 86 캔버스유닛 반경(캔버스 반폭 90), 우리는 UIFx.cs:331 halfWidth 78 + :332 y -40~+120 의 사각 밴드.
- **[눈에띔]** 밀도와 수명이 다르다. 원작은 짧게 살고 많이 나며, 우리는 오래 살고 적게 난다 — 그래서 '반짝이는 재의 장막'이 아니라 '떠다니는 몇 개'로 보인다.
  - 근거: 원작 방출량 GlowDot_02 100/s + GlowDot_02_Light 100/s + Dust_01 버스트 1~4개/0.15s(7~27/s), 수명 0.5~1s(Dust 0.5~2s) → 동시 생존 약 150~200. 실측 프레임당 64.7개. / 우리 EMBER_INTERVAL 0.16f × EMBER_PER_TICK 2 = 12.5개/s, 수명 UIFx.cs:346 1.4~2.6s → 동시 약 25개. 실측 프레임당 39.4개(대신 개체가 3배 큼).
- **[사소]** 입자 모양이 다르다. 원작은 회전 0의 둥근 글로우 점(속도 방향으로 아주 살짝 늘어남)이고, 우리는 30~60°로 고정 회전한 세로 막대다.
  - 근거: 원작 Renderer m_RenderMode 1(Stretched Billboard) + m_VelocityScale 0.03(Light는 -0.01) + m_LengthScale 1, startRotation const 0, 텍스처 GlowDot_02.png. 실측 bbox 폭/높이 중앙값 2px/2px(등방). / 우리 UIFx.cs:334 sizeDelta (1.6, 2.5~5) 세로 3:1 막대, :340 localRotation Euler(0,0,30~60°).
- **[사소]** 색이 더 노랗고 진하다. 원작은 additive 두 겹이 겹쳐 창백한 주황으로 보인다.
  - 근거: 원작 GlowDot_02 ColorOverLifetime RGB (1,0.3305,0)→(1,0.3432,0), GlowDot_02_Light (1,0.8706,0)→(1,0.4078,0) 에 startColor alpha 0.3529 를 곱해 겹침. 실측 밝은 화소 중앙값 RGB(110,109,90), 상위20% 평균 (182,166,143). / 우리 UIFx.cs:336-338 (1,0.45~0.75,0.15,0.9) 단일 겹, 실측 색 중앙값 RGB(179,141,105).

## 수정안
■ 권장안 A — 이미 이식된 진짜 파티클을 켠다 (원작과 값이 동일하므로 좌표 변환 불필요)

  두 프로젝트 캔버스가 같은 단위계다: FIXED sizeDelta 원작 (180, 439.62) / 우리 (180, 440).
  원작 1유닛 = 1.9111 ref px, 우리 1유닛 = 6 native px. 즉 프리팹 값 그대로 쓰면 된다.

  ⚠️ 함정: BtnLight1 을 그냥 켜면 안 된다. 그 노드의 Image(comp &7915134431723578864)는
     m_Color {r:1, g:0, b:0, a:1} 에 스프라이트가 물려 있어 **빨간 판**이 화면을 덮는다.
     그래서 BtnEmbers 만 프레임 밑으로 옮겨 켠다.

  1) Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs — CoEmbers(226~238줄)를 통째로 아래로 교체하고
     149줄 `_embers = StartCoroutine(CoEmbers());` 는 `SetupEmbers();` 로 바꾼다.

        // 원작 Btn Embers 를 그대로 켠다. 프리팹에 값까지 1:1로 이식돼 있으나
        // 부모(BtnLight1)가 빨간 Image를 달고 있어 프레임 밑으로 옮겨 붙인다.
        void SetupEmbers()
        {
            var embers = Util.FindChild(gameObject, "BtnEmbers", true);
            if (embers == null) return;

            embers.transform.SetParent(_frameRT, false);
            embers.transform.localPosition = new Vector3(0f, -86.5f, 0.5f);  // 원작 -36.48(BtnLight1) + -50
            embers.transform.localScale    = Vector3.one * 3f;               // 원작 Btn Embers scale
            embers.transform.SetAsLastSibling();
            embers.SetActive(true);

            var uip = embers.AddComponent<Coffee.UIExtensions.UIParticle>();
            uip.scale = 5.75f;          // 실측: 파티클 1 world unit = 11 ref px = 5.75 캔버스유닛
            uip.RefreshParticles();
        }

  2) UI_EnhanceProgressPopup 도 동일 처리. 이 프리팹에는 BtnEmbers 가 없으니
     UI_EnhacePopup.prefab 의 BtnEmbers 를 복사해 붙이거나,
     기존 UIFx.PlayParticle 경로를 쓴다(어드레서블 등록 필요):
       · Assets/IMPORTS/EpicVictoryEffects/Prefabs/BG_Effects/Fire_Embers_02.prefab 을
         Addressables UI 그룹에 m_Address: "Fire_Embers_02", 라벨 PreLoad 로 등록
         (UI.asset:66-70 의 GlowDot_01 항목과 같은 형식)
       · UI_EnhanceProgressPopup.cs:721 의 SpawnEmber 루프를 한 줄로 교체:
           UIFx.PlayParticle(host, new Vector2(0f, -86.5f), "Fire_Embers_02", 999f);
         단 UIFx.cs:186 의 `p.Emit(30)` 은 looping 시스템에 불필요하니 그대로 둬도 무해.

  3) UIFx.cs:327~354 SpawnEmber 와 두 팝업의 EMBER_INTERVAL/EMBER_PER_TICK/CoEmbers 는 삭제.

  4) 검증: `unity-cli editor refresh --compile` → `console --type error` 후 플레이 모드에서
     불티 무리가 팝업 폭 전체(반경 약 86유닛 = 516 native px)를 덮고,
     개체 하나가 6~15 native px, 초당 약 195 native px 로 **위로+약간 왼쪽**으로 오르는지 확인.
     UIParticle.scale 5.75 로 크기가 안 맞으면 5~7 사이에서 위 두 수치를 맞춘다.


■ 대안 B — 파티클을 못 쓸 경우, SpawnEmber 를 원작 실측값으로 뒤집는다 (UIFx.cs:327~354 교체)

        public static void SpawnEmber(RectTransform host, float halfRadius = 86f)
        {
            if (host == null) return;

            // 원작 방출 디스크: Shape Cone radius 5 × scale 3 = 15 world unit = 86 캔버스유닛 반경.
            // 팝업 프레임 중심 기준 (0,-86.5)에 원점을 두고 원 안에 균등 분포.
            float ang = UnityEngine.Random.value * Mathf.PI * 2f;
            float rad = halfRadius * Mathf.Sqrt(UnityEngine.Random.value);
            var pos = new Vector2(Mathf.Cos(ang) * rad, -86.5f + Mathf.Sin(ang) * rad * 0.85f);

            // 원작 실측: 밝은 화소 중앙값 RGB(110,109,90), 상위20% (182,166,143).
            // GlowDot_02(주황 1,0.34,0)와 GlowDot_02_Light(노랑→주황, alpha 0.35)가 겹친 결과다.
            float s = UnityEngine.Random.Range(1.0f, 2.5f);      // 실측 bbox 2px(p90 4.5px) = 1.05~2.4 유닛
            bool light = UnityEngine.Random.value < 0.5f;
            var col = light ? new Color(1f, UnityEngine.Random.Range(0.41f, 0.87f), 0f, 0.35f)
                            : new Color(1f, 0.34f, 0f, 1f);

            var img = CreateImage(host, "Ember", pos, new Vector2(s, s), col, Dot());
            img.rectTransform.localRotation = Quaternion.identity;   // 원작 startRotation = 0

            // 원작 실측 속도: 위 32.5 유닛/s, 왼쪽 9.7 유닛/s. 수명 0.5~1.0s(GlowDot_02).
            float life = UnityEngine.Random.Range(0.5f, 1.0f);
            var rise = pos + new Vector2(-UnityEngine.Random.Range(6f, 14f) * life,
                                          UnityEngine.Random.Range(27f, 41f) * life);

            var seq = DOTween.Sequence().SetUpdate(true).SetLink(img.gameObject);
            seq.Join(img.rectTransform.DOAnchorPos(rise, life).SetEase(Ease.Linear));   // 원작은 등속
            // alpha 커브 0@0 / 1@0.20 / 1@0.80 / 0@1.0 (원작 ColorOverLifetime)
            img.color = new Color(col.r, col.g, col.b, 0f);
            seq.Insert(0f,          img.DOFade(col.a, life * 0.2f).SetEase(Ease.Linear));
            seq.Insert(life * 0.8f, img.DOFade(0f,    life * 0.2f).SetEase(Ease.Linear));
            seq.OnComplete(() => { if (img != null) UnityEngine.Object.Destroy(img.gameObject); });
        }

     그리고 밀도를 원작에 맞춘다 (UI_EnhacePopup.cs:223-224, UI_EnhanceProgressPopup.cs:709-710):
        const float EMBER_INTERVAL = 0.05f;   // 원작 동시 생존 ~150~200개, 실측 프레임당 64.7개
        const int   EMBER_PER_TICK = 4;       // = 80개/s × 수명 0.75s ≈ 동시 60개

     ※ 대안 B는 Dust_01(오른쪽으로 3~6 유닛/s 흐르는 굵은 불티, 초당 7~27개, 수명 0.5~2s)을 뺀 근사다.
       더 붙이려면 10%의 확률로 rise 대신 (+3~6 유닛/s 오른쪽, 수명 0.5~2s, 크기 0.7~2.6 유닛) 개체를 만든다.


# 망치 타격 파티클 (대장간 강화 연출)

## 원작
■ 결론부터: 지시서에 적힌 `Enhace Popup/Hammer Normal Anim/GlowDot_01 (1)` 은 타격 이펙트가 아니다. 영상의 흰 구체/링/연기/청록불티는 전부 전역 FX 디스패처가 켜는 `FX_splash_crystal_air` 다. 영상의 망치질은 `Enhace Popup`이 아니라 `EnhanceProgress Popup/Hammer(IncludeAnim)`(GameObject 901440559)이 돌린다.

■ 트리거 체인 (SampleScene.unity)
1) Hammer(IncludeAnim) ScriptMachine &901440561:
   BoltUnityEvent "Shake"[10] → DORestartById(Icon,"1")[110] → SetActive(Icon/forAnim/Flash 1894263891,true)[172] → TriggerCustomEvent "HammerSmash" on FX(503745716)[224]
   "ChargeSuccess"[145] → ... → TriggerCustomEvent "HammerSmashLong"[196]
   "Success" → "HammerSmashSuccess", "Fail"/"ChargeFail" → "HammerSmashFail"
2) FX(503745716) ScriptMachine — 라운드로빈 3발: base 꺼져있으면 base 켬 / base 켜졌고 (1) 꺼졌으면 (1) / 아니면 (2)
3) 각 인스턴스 자체 ScriptMachine: OnEnable → Timer 1.01s → SetActive(self,false). 1.0s 간격 타격이라 인스턴스 3개가 필요한 이유.

■ 배치
FX(In-Frame stretch) → FX_HammerSmash(RectTransform stretch, anchoredPos 0,0) → FX_splash_crystal_air / (1) / (2), 전부 Transform localPosition (0,-25,0), localScale (1,1,1), 초기 active=0.
즉 타격 FX는 팝업 안이 아니라 In-Frame(화면) 중앙에서 y=-25 unit. 픽셀 실측과 일치: ref30 f183 흰 구체 중심 ref(176.5,473.5), 캔버스 중앙(172,427) → (+2.4, -24.3) unit.

■ 구성: 팩 프리팹 8자식 중 5개만 남기고 particle / center_light / shining_highlight 를 지웠고, 루트 파티클도 사실상 껐다.
루트 PS &446320751: lengthInSec 0.05, startLifetime 0.0001, startColor (0.5608,1,0.9534, a=0) → 무효화. Renderer: mat 108fc744edacd2c4db287a334fe8aa0a, sortingOrder 150, maxParticleSize 5, RenderMode 0.

[1] center_light_circle — 흰 구체 본체
 lengthInSec 1 / looping 0 / playOnAwake 1 / scalingMode Local / cullingMode 0
 startLifetime const 0.4, startSpeed rand[1..0], startColor (1,1,1,1), startSize const 3.5, startSizeY const 2.3, startRotation rand[6.28318..0], maxParticles 20, gravity 0
 Shape: Circle(type4) radius 0.0001 radiusThickness 0 rot(-90,0,0), arc 360
 Emission: rateOverTime 0, burst t=0 count 1, burst t=1 count 1 (둘 다 cycles1 interval0.01 prob1)
 SizeOverLifetime x: curve x1 [(0,0);(0.120164,0.662083);(1,1)]  y: [(0,0.25);(1,1)]  z: x0.5 [(0,0.550301);(1,0)]
 ColorOverLifetime: OFF
 Rotation: separateAxes1, z rand2curve x12.5664 max[(0,-0.5);(1,0)] min[(0,-1);(1,0)]
 ClampVelocity on (dampen 0.2), UV anim on: tiles 8x8, fps30, frameOverTime const 0.34375, cycles1, rowMode1
 Renderer: mat 1d3d3c120d03597448e56b97749c8bf2 (Mat_fx_elements_set_01_highlight_inner), RenderMode 0(Billboard), sortingOrder 150, maxParticleSize 5

[2] shining_center — 구체를 하얗게 태우는 누적 광원 (알파 0.05 x 15장 겹침 → 255 포화)
 lengthInSec 0.5
 startLifetime rand[0.5..0.45], startSpeed 0, startColor (1,1,1, a=0.050980393), startSize rand[10..7], startSizeY rand[7..3], startRotation rand[0..6.283185], maxParticles 15
 Shape: Circle radius 0.0001, rot(0,0,0)
 Emission: rateOverTime curve x5 [(0,1);(0.75,0)], 버스트 없음
 SizeOverLifetime: x curve x1 [(0,0);(1,1)], y [(0,0.75);(1,1)], z x2.5 [(0,0);(1,1)]
 ColorOverLifetime: grad C0.20=(0.613,0.613,0.613) C1.00=(1,1,1), A0.00=1 A1.00=1
 UV anim on. Renderer: mat 108fc744edacd2c4db287a334fe8aa0a (Mat_fx_elements_set_02_highlight_inner), sortingOrder 150

[3] sharp_circle — 밖으로 퍼지는 흰 링 (f204~205에서 뚜렷)
 lengthInSec 1
 startLifetime rand[0.3..0.27], startSpeed rand[0..0], startColor (1,1,1,1)
 startSize rand2curve x5 max[(0,0.15)] min[(0,0.681692);(0.164897,1)]
 startSizeY rand2curve x5 max[(0,1);(0.297382,0.1)] min[(0,0.805809);(0.297382,0.1)]
 maxParticles 20
 Shape: type5(Box/Edge) m_Scale(0.01,0.5,0.01), radiusThickness 0
 Emission: rateOverTime 0, burst t=0.05 count 7 / t=1 count 2 / t=1 count 3 / t=1 count 2
 SizeOverLifetime separateAxes1: x,y rand2curve x1 max[(0,0.5);(1,1)] min[(0,0);(1,1)], z x0.5 max[(0,0.674118);(1,0)] min[(0,1);(1,0)]
 ColorOverLifetime: grad C0.00=(1,1,1) C1.00=(1,1,1), A0.90=1 A1.00=0
 Rotation separateAxes1: z rand2curve x18.849556 max[(0,1);(1,0)] min[(0,0.481697);(1,0)]
 Renderer: mat 1d3d3c12..., RenderMode 4(Mesh), Mesh guid 11b98836bae571640b8104137d25d6b1 (fbx_fx_cylinder_03.FBX, submesh -951608786083511892), RenderAlignment 2(Local), sortingOrder 150, maxParticleSize 5

[4] wind — 회색 연기
 lengthInSec 1
 startLifetime rand[0.3..0.4], startSpeed rand[-3..3], startColor (0.6698113,0.6698113,0.6698113,1), startSize rand[5.5..7], startSizeY rand[10..5], maxParticles 20
 Shape: Circle radius 0.0001, rot(-90,0,0)
 Emission: rateOverTime 0, burst t=0.05 count 5 / t=0.08 count 5 / t=0.11 count 5
 SizeOverLifetime separateAxes1: x,y rand2curve x1 [(0,0);(1,1)], z x1.5 max[(0,0);(0.15,0.605199);(1,1)] min[(0,0);(1,0.4)]
 ColorOverLifetime: grad C0.00=(1,1,1) C1.00=(1,1,1), A0.20=1 A0.50=0.196
 Rotation separateAxes1: z rand2curve x27.925268 max[(0,1);(0.604534,0.078875)] min[(0,0.481697);(0.604534,0.078875)]
 Renderer: mat 1d3d3c12..., RenderMode 4(Mesh, 같은 cylinder 메시), RenderAlignment 2, sortMode 1, sortingOrder 150

[5] twinkle — 청록 4각별 반짝임
 lengthInSec 1
 startLifetime rand[0.3..1], startSpeed rand[1..10], startColor (1,1,1, a=0.7058824), startSize rand[1.5..0.5], startSizeY rand[2..3], maxParticles 20
 Shape: Sphere(type0) radiusThickness 1, rot(-90,0,0)
 Emission: rateOverTime 0, burst t=0 count 10, burst t=1 count 10
 SizeOverLifetime: 19키 톱니 curve (0,0);(0.0503,1);(0.1038,0.4319);(0.1497,0.8845);(0.2,0.3395);(0.2633,0.7608);(0.3198,0.2309);(0.3685,0.6329);(0.4289,0.1686);(0.5078,0.5444);(0.5543,0.0947);(0.6148,0.4588);(0.6646,0.0462);(0.7212,0.3833);(0.7858,0.0254);(0.8301,0.2744);(0.8879,0);(0.9407,0.1638);(1,0)  ← 깜빡임
 ColorOverLifetime: grad C0.00=(0,1,0.682) C1.00=(0,0.698,0.633), A0.00=1 A1.00=1  ← 청록의 출처
 Velocity: y rand[1..3] (위로 뜬다), ClampVelocity dampen 0.2
 UV anim on: 8x8, fps30, frameOverTime const 0.28125
 Renderer: mat 1d3d3c12..., RenderMode 0, sortMode 1, sortingOrder 150

■ 팩 원본 프리팹 대비 씬에서 손댄 값 (우리 repo에 있는 프리팹을 그대로 쓰면 파랑 결정 이펙트가 나온다 — 반드시 이 6가지를 덮어써야 흰/회색 대장간 톤이 된다)
 center_light_circle: lengthInSec 2→1, startColor (0.5,0.5,1,1)→(1,1,1,1), 2번째 burst t 1.5→1
 shining_center: lengthInSec 2→0.5, ColorOverLifetime (0.506,0.745,1)@0.2/(0,0.253,1)@1 → (0.613,0.613,0.613)@0.2/(1,1,1)@1
 sharp_circle: lengthInSec 2→1, startColor (0.5,0.5,1,1)→(1,1,1,1), burst t 1.5/1.55/1.6 → 1/1/1
 wind: lengthInSec 3→1, startColor (0.4238,0.5454,0.8396,1)→(0.6698,0.6698,0.6698,1)
 twinkle: lengthInSec 2→1, startColor (0.5,0.5,1,1)→(1,1,1,0.7058824), 2번째 burst t 1.5→1
 루트: lengthInSec 3→0.05, startLifetime 1.5→0.0001, startColor alpha 1→0

■ 영상 실측 (ref30, 344x854, frame N = 200.0 + N*0.1s)
 f182 218.2 망치 하강, FX 없음
 f183/f184 218.3~218.4 흰 원반 90x90px (bbox x132-221 y427-516, 중심 176.5/471.5), 코어 RGB 255, 중앙만 220~240으로 살짝 눌림(장비 아이콘 타일이 비침). 모루 상판(y=487)보다 13.5px 위.
 f185 218.5 원반 사라지고 회색 연기 판 + 흰 가로 줄무늬(=sharp_circle 링을 옆에서 봄) + 청록 4각별 4개
 f186~187 218.6~218.7 연기가 폭 128~141px로 퍼지고 청록별 잔존, 흰 픽셀 179→126개로 급감
 f188~191 218.8~219.1 짙은 회색 연기 잔상만 드리프트, 흰 점 소수
 2타: f193 218px 원반(왼쪽 망치머리에 가림) → f194 102x111 링 단계 → f195 114x135 확산
 3타(=ChargeSuccess, FX_splash_LONG): f203 원반 95x102 → f204/f205 속이 빈 흰 링 외경 118px/117px
 즉 원반은 약 0.15~0.2초, 링은 0.3~0.4초, 연기는 약 0.6초, 청록별이 가장 오래 남는다.
 모루 상판 y=487(ref) / 우리 y=1467(native). 원작 원반 중심은 모루 상판 13.5 ref-px 위.

■ 애니 클립 이벤트 (RE001_Share/Assets/Animation/UI/Forge Smith/, 우리 포트 클립과 이벤트 완전 동일함 — 확인함)
 Success.anim (stop 5.016667): 0.0833 SmashSound / 0.25 Curious / 0.3333 Shake / 1.0833 SmashSound / 1.3333 Shake / 2.0833 SmashSound / 2.3333 ChargeSuccess / 3.75 Serious / 3.8333 Success / 5.0 SuccessDone. 커브는 전부 루트(스미스 Image) 대상: m_Sprite PPtr 37키, m_AnchoredPosition.x 2.3333~3.8333 사이 ±0.5 지그재그(30fps), m_Color.rgb 0.8627451→1@3.5, m_ScaleCurves 1→1.05@3.0→1.15@3.85→1@4.4667. GameObject active 커브는 없음(자식 on/off는 전부 ScriptMachine이 함).
 Normal.anim (stop 3.0166667): 0.0833 SmashSound / 0.3333 Shake / 1.1667 SmashSound / 1.4167 Shake / 2.25 SmashSound / 2.3333 Done / 2.5 ShakeLast. ("ShakeLast"를 듣는 그래프는 Hammer(IncludeAnim)에 없다.)
 Fail.anim (stop 5.016667): Success와 동일하되 2.3333 ChargeFail / 3.85 Fail / 3.9 FailSound / 5.0 FailDone, m_Color가 3.8333 이후 0.9728→0.6@4.0→0.3@4.6667로 어두워짐.

■ 참고: Enhace Popup 쪽 GlowDot_01 (1) (fileID 2000120129)도 실측해 뒀다 — 강화창(선택 화면)의 연출이며 값은 우리 프리팹과 바이트 단위로 동일하다. Transform (0,-15) scale 1.6, PS len 0.2/lifetime 0.2/speed 0/size rand[4..5]/color (1,1,1,0.784)/max 1, size 0→1, color (1,1,1)→(0.4057)x3 with A0.5=1 A1.0=0. Dust_01 len0.15 delay0.1 lifetime rand[0.3..0.7] speed rand[4..6] size rand[0.05..0.1] max60 rate300 sphere r0.5 ColorModule OFF. Ring_01 len0.2 delay0.1 lifetime 0.4 speed0 size 2(sizeCurve x1.25 0→1) max1 color (0.8396)x3, A0=0 A0.5=1 A0.9=0. mat GlowDot_01_Light / Ring_01, sortingOrder 200, maxParticleSize 0.5. 자체 ScriptMachine OnEnable→Timer 0.7→SetActive(false). 이건 이번 문제와 무관하다.

## 우리
■ 코드
Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs:989 case "Shake" → ShakeFrame(0.22f,2f) + PopImpactFx()
Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs:994 case "ShakeLast" → PopImpactFx()
Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs:1004 case "ChargeSuccess"/"ChargeFail" → PopImpactFx() + RaiseCraftIcon()
Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs:1029 PopImpactFx():
  - Util.FindChild로 "Flash", "GlowDot_01_1" 을 SetActive(false)→(true) 토글
  - 타격점을 WIcon(장비 아이콘) 월드좌표를 _frameRT 로컬로 변환해서 잡음
  - UIFx.PlayBurst(host, at, UIFx.FxType.ForgeSmash)
  - 아이콘 DOPunchScale(0.18,-0.12, 0.24s, vib10, elas0.8)
Assets/@Scripts/UI/UIFx.cs:265 SpawnForgeSmash(): 전부 손으로 그린 Image + DOTween
  - SmashOrb: 46x46 unit, Dot() 스프라이트, scale 0.35→1.25 (0.16s OutQuad), DOFade→0 (0.32s)
  - SmashSmoke0~6: 24x18, color (0.86,0.87,0.92,0.55), 좌우 ±(12+i*5.5) 로 0.55s, scale→1.8
  - SmashLine0~3: 22x2.4 흰 선, ±34 로 0.28s, sizeDelta→(46,1.2)
  - SmashSpark0~7: 9x9 Star(), color (0.62,1,0.94,1), 반경 32~50 로 0.6s
  - 총 수명 0.7s 반환

■ 프리팹
Assets/@Resources/Prefab/UI/Popup/UI_EnhacePopup.prefab
  GlowDot_01_1 (GameObject &5882113800268201201, Transform &7110327413866843555): localPosition (0,-15), localScale (1.6,1.6,1), 초기 m_IsActive 0
  자식 2개: VFX_GlowDot_01_1_03 (=원작 Dust_01을 리네임), Ring_01
  세 PS(&432515689675432383 / &580408649047493947 / &1233440441140460658) 값은 원작과 완전히 동일(len/lifetime/speed/color/size/max 모두 일치 확인)
  UIParticle(Coffee.UIExtensions) 컴포넌트 없음 → Screen Space Camera 캔버스에 렌더되지 않음. 즉 PopImpactFx의 GlowDot_01_1 토글은 화면에 아무것도 안 그린다.
FX_splash_crystal_air 프리팹은 이미 리포에 있다: Assets/IMPORTS/SkillEf/Prefabs/Stylized_Element_Splash_vol.2/FX_splash_crystal_air.prefab (guid 3af6c4d23945a1c449b1e9d883dc9ddf), 자식 8개(sharp_circle, particle, twinkle, wind, shining_center, center_light, shining_highlight, center_light_circle). 재질(Mat_fx_elements_set_01/02_highlight_inner), 셰이더(Fx_dissolve_particle_apb.shadergraph), 메시(fbx_fx_cylinder_03.FBX) 전부 존재.
UIParticle 래핑 헬퍼도 이미 있다: Assets/@Scripts/UI/UIFx.cs:204 WrapInPlace(), :238 Replay(), :151 PlayParticle(prefabKey). Assets/@Scripts/UI/Popup/UI_EnhanceProgressPopup.cs:638 에서 God_ray_02를 이 방식으로 살린 선례가 있다.
Addressable VFX 그룹(Assets/AddressableAssetsData/AssetGroups/VFX.asset)에 FX_splash_spark_floor / FX_splash_spark_air 가 이미 등록돼 있다.

■ demo30 실측 (1080x2640, 15fps, 암전 155 = 217.3s)
f170 218.30 흰 픽셀 789개, bbox 65x39 (x509-573 y1376-1414)
f171 218.37 677개, 189x36
f172 218.43 137개, 271x44
f173 218.50 흰 픽셀 0 → 이후 밝은 회백색 뭉게구름만 0.5초 남음
2타 f186~f188 동일 패턴. 3타(f200~202)는 흰 픽셀 0, mean만 5.4→6.1로 미미하게 상승.
모루 상판 y=1467.

## 차이
- **[치명]** 원작의 타격 이펙트 자체가 없다. 원작은 FX_splash_crystal_air(5개 파티클 시스템)를 켜는데, 우리는 UIFx.SpawnForgeSmash가 Image+DOTween으로 손그림 대체품을 그린다. 형태·색·타이밍이 전부 다르다.
  - 근거: 원작 SampleScene.unity FX(503745716) 그래프 node[224] TriggerCustomEvent "HammerSmash" → FX_HammerSmash/FX_splash_crystal_air(1448006468556875671 / 446320748 / 878604834). 우리: Assets/@Scripts/UI/UIFx.cs:265 SpawnForgeSmash 전부 CreateImage+DOTween
- **[치명]** 흰 구체가 없다. 원작은 f183에서 지름 90x90 ref-px(=47 canvas unit, RGB 255 포화)의 원반이 2프레임(약0.15~0.2s) 번쩍인다. 우리는 흰 픽셀이 최대 789개(65x39px)뿐이고 f173에 이미 0이 된다. 우리 원반은 원작의 약 1/4 크기이고 원형이 아니다.
  - 근거: ref30/183.png 흰(>235) 4937px bbox x132-221 y431-516 / demo30/0170.png 흰 789px bbox x509-573 y1376-1414
- **[치명]** 연기 색이 뒤집혔다. 원작 wind는 startColor (0.6698,0.6698,0.6698) 회색에 alpha가 A0.20=1 → A0.50=0.196로 빠르게 죽어 어두운 회색 연기로 읽힌다. 우리 SmashSmoke는 (0.86,0.87,0.92,0.55)의 밝은 흰 구름이 0.55초 내내 떠 있어 무대가 하얗게 뜬다.
  - 근거: wind PS ColorOverLifetime grad A0.20=1 A0.50=0.196 / Assets/@Scripts/UI/UIFx.cs:283 new Color(0.86f,0.87f,0.92f,0.55f), ref30 f187 흰픽셀 126개 vs demo30 f172 이후 회백 구름 지속
- **[눈에띔]** 밖으로 퍼지는 흰 링(sharp_circle)이 없다. 원작은 f204/f205에서 외경 118px의 속이 빈 흰 원이 뚜렷하다. 우리는 SmashLine 4개의 직선 줄무늬로 대체돼 있다.
  - 근거: ref30/204.png y=473 프로파일이 x120-140과 x225-237만 밝고 중앙이 어두운 도넛 형태 / Assets/@Scripts/UI/UIFx.cs:288-298 SmashLine (22x2.4 직선)
- **[눈에띔]** 청록 반짝임의 색·움직임이 다르다. 원작 twinkle은 ColorOverLifetime (0,1,0.682)→(0,0.698,0.633)에 startSpeed rand[1..10], velocity y rand[1..3](위로 뜸), 19키 톱니 size 커브로 깜빡인다. 우리는 (0.62,1,0.94) 고정색으로 방사형 8방향에 y를 0.65배 눌러 옆으로만 퍼뜨리고 깜빡이지 않는다.
  - 근거: twinkle PS ColorOverLifetime / VelocityModule y rand[1..3] / SizeOverLifetime 19키 / Assets/@Scripts/UI/UIFx.cs:301-311
- **[눈에띔]** 타격 위치가 다르다. 원작은 In-Frame 중앙 + (0,-25) unit 고정. 우리는 WIcon(장비 아이콘)의 위치를 매번 계산해 쓰는데, 아이콘은 ChargeSuccess에서 위로 떠오르므로 3타에서 타격점이 따라 올라간다.
  - 근거: 원작 Transform &446320749 m_LocalPosition {0,-25,0} (3개 인스턴스 모두 동일) / Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs:1042-1052 icon.rectTransform.position 기반
- **[눈에띔]** 프리팹에 저작된 GlowDot_01_1을 토글하는 코드가 죽은 코드다. UIParticle 래퍼가 없어 Screen Space Camera 캔버스에 한 픽셀도 그려지지 않는다.
  - 근거: Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs:1034 foreach { "Flash", "GlowDot_01_1" } / UI_EnhacePopup.prefab 에 Coffee/UIParticle 문자열 0건, 반면 UI_EnhanceProgressPopup.cs:638은 같은 문제를 UIFx.WrapInPlace로 해결해 뒀다
- **[사소]** 3타의 종류가 다르다. 원작은 1·2타가 FX_splash_crystal_air, 3타(ChargeSuccess)는 FX_splash_LONG(자식에 particle이 추가되고 twinkle 대신 center_light)이다. 우리는 3타 모두 같은 ForgeSmash를 쓴다(그나마 3타는 화면에 거의 안 보인다).
  - 근거: Hammer(IncludeAnim) 그래프 [145] ChargeSuccess → [196] TriggerCustomEvent "HammerSmashLong" → FX 그래프 [23] SetActive(FX_splash_LONG) / demo30 f200-202 흰픽셀 0
- **[사소]** 이펙트가 팝업 마스크 안에서 터진다. 원작은 In-Frame 최상위 FX 노드에서 재생돼 팝업 프레임에 잘리지 않는다(sortingOrder 150).
  - 근거: 원작 ?/UI/FIXED/In-Frame/FX/FX_HammerSmash / 우리 UI_EnhacePopup.cs:1039 host = _frameRT

## 수정안
■ 방향: 손그림(UIFx.SpawnForgeSmash)을 버리고, 이미 리포에 있는 FX_splash_crystal_air 프리팹을 원작 씬 값으로 튜닝해 UIParticle로 재생한다. 선례(UI_EnhanceProgressPopup.ShowGodRays → UIFx.WrapInPlace/Replay)와 같은 경로다.

[1] 프리팹 생성 — Assets/@Resources/Prefab/SFX/Effects/FX_HammerSmash.prefab
 Assets/IMPORTS/SkillEf/Prefabs/Stylized_Element_Splash_vol.2/FX_splash_crystal_air.prefab 를 복제한 뒤:
 (a) 자식 3개 삭제: particle, center_light, shining_highlight  (남길 5개: center_light_circle, twinkle, shining_center, wind, sharp_circle)
 (b) 루트 ParticleSystem 무효화: lengthInSec 0.05 / startLifetime 0.0001 / startColor (0.56078434, 1, 0.9533545, 0)
 (c) 자식 값 덮어쓰기 (팩 기본값 → 원작 씬 값)
   center_light_circle : lengthInSec 2→1, startColor (0.5,0.5,1,1)→(1,1,1,1), Emission 2번째 burst time 1.5→1.0
   shining_center      : lengthInSec 2→0.5, ColorOverLifetime 그라디언트를 C@0.20=(0.6132076,0.6132076,0.6132076), C@1.00=(1,1,1), A@0.00=1, A@1.00=1 로
   sharp_circle        : lengthInSec 2→1, startColor (0.5,0.5,1,1)→(1,1,1,1), burst time 1.5/1.55/1.6 → 1.0/1.0/1.0 (첫 burst t=0.05 count 7은 그대로)
   wind                : lengthInSec 3→1, startColor (0.4237718,0.54540795,0.8396226,1)→(0.6698113,0.6698113,0.6698113,1)
   twinkle             : lengthInSec 2→1, startColor (0.5,0.5,1,1)→(1,1,1,0.7058824), 2번째 burst time 1.5→1.0
 (d) 나머지(Shape/Size/Rotation/UV/Renderer/버스트 개수)는 팩 프리팹 그대로가 원작과 일치한다 — 건드리지 말 것.
 (e) Addressable VFX 그룹(Assets/AddressableAssetsData/AssetGroups/VFX.asset)에 주소 "FX_HammerSmash" 로 등록. 기존 FX_splash_spark_air 항목 옆에 붙이면 된다.

[2] UIFx에 burst 파티클용 재생 함수 추가 (Assets/@Scripts/UI/UIFx.cs)
 기존 PlayParticle(:151)은 마지막에 모든 자식 PS에 p.Emit(30)을 때린다. FX_HammerSmash는 burst 설계라 이러면 입자가 30개씩 덧붙어 뭉개진다. emit 파라미터를 추가하고 기본 0으로 바꾸거나(호출부 2곳: UI_PotionDetailPopup.cs:914, :922 는 emit:30 명시), 아래처럼 전용 오버로드를 하나 둔다:
   public static GameObject PlayParticle(RectTransform parent, Vector2 localPos, string prefabKey, float lifetime = 2f, int emit = 0)
 그리고 UIParticle 스케일은 기본값 m_Scale3D=(10,10,10)을 그대로 쓴다 (Library/PackageCache/com.coffee.ui-particle@87dca448ce96/Runtime/UIParticle.cs:67). 실측 검증 기준: 흰 구체 코어 지름이 47 canvas unit(우리 1080폭 기준 약 190~245 native px)이면 맞다. 지금은 65px이다.

[3] UI_EnhacePopup.PopImpactFx 교체 (Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs:1029)
 - "GlowDot_01_1" 토글 제거(죽은 코드). "Flash" 토글은 원작도 하므로 유지 (원작 node[172] SetActive(Icon/forAnim/Flash,true)).
 - WIcon 좌표 계산 블록 전부 제거. 대신 화면 고정 좌표:
     var host = _frameRT;   // 팝업이 In-Frame 전체를 덮는 stretch RectTransform이면 그대로. 아니면 In-Frame 상당 노드를 쓸 것
     UIFx.PlayParticle(host, new Vector2(0f, IMPACT_Y), "FX_HammerSmash", 1.05f);
   원작 저작값은 y = -25 unit(In-Frame 중앙 기준). 우리 화면 실측 보정치는 y ≈ -23 unit(모루 상판 native y=1467 기준 약 28px 위 = native y≈1439). 상수로 빼 둘 것:
     const float IMPACT_Y = -23f;   // 원작 In-Frame 기준 -25. 모루 상판 위 약 7unit
   수명 1.05초는 원작 인스턴스의 자멸 타이머 1.01초에 맞춘 것.
 - 아이콘 DOPunchScale은 원작에 없는 우리 추가분이다. 남겨도 되지만, 원작은 대신 Icon 의 DOTweenAnimation id "1" 을 DORestartById 한다.

[4] UIFx.SpawnForgeSmash / FxType.ForgeSmash 삭제 (Assets/@Scripts/UI/UIFx.cs:265~, :26 enum, :54 switch case). 다른 호출부 없음.

[5] (선택) 3타 구분 — 원작은 ChargeSuccess에서 FX_splash_LONG을 쓴다. 같은 절차로 Assets/IMPORTS/SkillEf/.../FX_splash_LONG 상당 프리팹을 하나 더 만들어 "FX_HammerSmashLong"으로 등록하고, UI_EnhacePopup.cs:1004 의 ChargeSuccess/ChargeFail 분기에서 그걸 재생. 우선순위는 낮다(1·2타부터 고칠 것).

■ 검증 방법
 unity-cli editor refresh --compile → console --type error 후 플레이. 강화 연출을 15fps로 녹화해 타격 프레임에서
  (1) 흰 픽셀(>235) bbox가 약 190~245 native px 정사각형에 가까울 것 (현재 65x39)
  (2) 타격 +0.2s 프레임에 속이 빈 흰 링이 보일 것
  (3) 연기가 회백색 구름이 아니라 어두운 회색이고 0.6초 안에 사라질 것
  (4) 청록 별이 위로 뜨며 깜빡일 것


# 장비 아이콘(Craft Equip Icon / Icon)과 모루의 자리

## 원작
■ 모루는 오브젝트가 아니다 — **망치 스프라이트에 같이 그려져 있다**
  RE001_Share/Assets/Sprite/UI/Smith/Loop.png (1280x128, 128x128 10프레임, PPU 100, spriteMode 2)
  프레임 0(Loop_0, internalID 3852229755048302361) 안에서
    · 망치 = 행 29~84, x 3~36
    · **모루 = 행 99~127, x 28~103**  ← 모루 윗면 = 행 99 (색 35,38,46 / 행101이 하이라이트 107,123,142)
  붙어 있는 오브젝트: `?/UI/FIXED/In-Frame/Popup/EnhanceProgress Popup/Hammer(IncludeAnim)`
  (강화창 쪽은 `Enhace Popup/Hammer Normal Anim`, 알파 0으로 시작)
    RectTransform: anchorMin=anchorMax=(0.5,0) / anchoredPosition=(0,156.5) / sizeDelta=(130,130) / pivot=(0.5,0) / scale=1
    Image color = (0.8627,0.8627,0.8627,a) — 클립 Normal.anim이 m_Color와 m_Sprite만 애니메이션(트랜스폼 커브 없음)
  ⇒ 1 스프라이트px = 130/128 = 1.015625 유닛
  ⇒ 모루 윗면 = rect 바닥 위 29×1.015625 = **29.45유닛** = In-Frame 바닥 위 185.95유닛
  ⇒ In-Frame(174 x 432.7, FIXED 180x440에서 sizeDelta -6/-7.3) 중앙 기준 **y = -30.40유닛**
  ⇒ 모루 상판 폭 = 76px × 1.015625 = **77.19유닛** (아이콘 판 30유닛의 2.57배)

■ `Enhace Popup/Floor`(fileID 164417139)는 모루가 아니라 **뒤쪽 돌 아치**다
  sprite = Assets/Sprite/Background/Hell_obj_png.png / Hell_obj_png_0 (atlas rect x322 y256 510x240)
  RectTransform: anchor 중앙 / anchoredPosition=(0.37965995, -100.00001) / sizeDelta=(249.04, 117.2) / pivot=(0.5,0.5) / **localScale=(0.7,0.7,1)**
  Image color = (0.77254903, 0.80784315, 0.8392157, 1)
  ⇒ 실효 174.33 x 82.04유닛, **윗변 y = -58.98** (모루 윗면보다 28.6유닛 아래)
  자식 '1','2'는 Image가 없는 **트윈 전용 노드**(둘 다 targetGO=Floor):
    '1' = ShakeRotation(13) endValue(6,0,1) dur 0.8 ease OutCubic vibrato50 randomness90 fadeOut
    '2' = LocalMove(2) isFrom=1 endValue(0,-190,0) dur 0.8 ease OutCirc  ← 등장 시 아래에서 솟아오름

■ `Enhace Popup/Craft Equip Icon` (fileID 2004910487)
  RectTransform: anchorMin=anchorMax=(0.5,0.5) / **anchoredPosition=(0,20)** / **sizeDelta=(30,30)** / **pivot=(0.5,1)** / localScale=1
  (앵커가 부모 피벗과 같으므로 anchoredPosition == localPosition. pivot이 윗변이라 y가 곧 판 윗변)
  MonoBehaviour 5개의 정체(m_Script GUID로 분류):
    &2004910490 guid e741851cba3ad425c91ecf922cc6b379 = VisualScripting **Variables** ("Icon","E Number" 오브젝트 참조)
    &2004910489 guid d2fe5546f775c47d2820aa5e8a715f53 = VisualScripting **ScriptMachine**
    &2004910488 guid d2fe5546f775c47d2820aa5e8a715f53 = **ScriptMachine 2번째** (Material_Craft / Material_NotEquip 참조 6개 보유)
    &2004910493 guid aa0b1eebb5db27a419fa4564bbe5c9c5 fileID 40306178 = **DOTweenVisualManager** (onEnableBehaviour: 2 = Restart, onDisableBehaviour: 0)
    &2004910492 guid 4d0390bd8b8ffd640b34fe25065ff1df = **DOTweenAnimation ― 딱 1개뿐**
      id="" / LocalMove(2) / isFrom=1 / endValueV3=(0,0,0) / duration 0.5 / delay 0 / ease 27=OutBack / loops 1 / autoPlay 1 / autoKill 0 / targetIsSelf 1
      ⇒ 강화창의 아이콘은 "등장 시 (0,0)에서 (0,20)으로 튀어오름" 하나만 저작돼 있다.
        **연출(하강·타격·담금질·결과)은 강화창이 아니라 결과창이 통째로 맡는다.**

■ 연출의 진짜 주인 = `EnhanceProgress Popup/Icon` (fileID 1740154040) — RectTransform 값은 위와 동일 (0,20)/30x30/pivot(0.5,1)
  DOTweenAnimation **12개, 전부 autoPlay=0** (VS 그래프가 DORestartById로 지휘):
    id 1  ShakePosition (4,4,0)   dur 0.25 OutQuad  vibrato 50 randomness 90
    id 2  LocalMove    (0,0,0)    dur 0.30 **InBack**            ← 모루 위 자리
    id 3  ShakePosition (1.5,1.5,1) dur 1.40 OutQuad vibrato 50 randomness 90
    id 4  PunchScale   (0.3,0.3,0) dur 0.30 InBack elasticity 0.367 vibrato 11
    id 5  LocalMove    (0,50,0)   dur 1.40 Linear  +  Scale→1.4  dur 1.40 Linear
    id 6  LocalMove    (0,80,0)   dur 0.30 OutBack
    id 7  LocalMove    (0,20,0)   dur 1.20 Linear  +  Scale→1.0  dur 1.00 Linear
    id 8  LocalMove    **(0,0,0)** dur 0.10 OutBack +  Scale→1.0  dur 1.00 OutQuad   ← 실패 = 모루 위로 툭
    id 9  LocalMove    (0,20,0)   dur 0.50 OutQuad
  자식 구성: forAnim/{Frame(스트레치, guid d9adca53…), Background(검정 a=1, size -3), Glow(knob 흰색, size -2)/{Image(2) 검정 10x5 y=2.5, Image(1) (0.596,0.0157,0.0157) 40x10 y=5}, W Icon(스트레치), SuccessFlash, Flash(off), Flash_USETHIS}
  ⇒ 판 안쪽 배경은 **검정 + 가운데 흰 라디얼 글로우 + 아래 붉은 띠**다.

■ 프레임 실측(ref30, 1유닛=1.9111 ref-px, In-Frame 중앙 = ref y 427 / x ≈ 175)
  · 판 크기 실측 57 ref-px = 29.8유닛 ✔ (30유닛)
  · 모루 상판 실측 x105~253 = 149 ref-px = 78.0유닛 ✔ (77.19유닛)
  · 모루 윗면 실측 ref y 484~486 (스프라이트 행99↔ref486, 행101↔ref490, 행103↔ref493) = y −30.3유닛 ✔ (계산 −30.40)
  · 접지 실측(f190, x=160 열): 판 아래 테두리 y481(130)·482(248)·483(122) → **틈 y484(60)·485(81) 2px** → 모루 y486(200)
    ⇒ 원작 틈 = 2 ref-px ≈ **1.0유닛**

## 우리
■ 프리팹은 원작을 그대로 옮겨 놨다 — 자리/크기는 이미 정확하다
  Assets/@Resources/Prefab/UI/Popup/UI_EnhacePopup.prefab
    In_Frame/EnhacePopup/**CraftEquipIcon** : anchor(0.5,0.5) pos(0,20) size(30,30) pivot(0.5,1) scale 1  ✔ 원작 동일
    …/**Floor** : pos(0.37965995,-100.00001) size(249.04,117.2) pivot(0.5,0.5) scale(0.7,0.7,1) 색(0.7725,0.8078,0.8392,1) 스프라이트 동일  ✔
    …/**HammerNormalAnim** : anchor(0.5,0) pos(0,156.5) size(130,130) pivot(0.5,0) sprite Loop_0 색 a=0  ✔
    FIXED = 180x440, In_Frame = sizeDelta(-6,-7.3) → 174x432.7  ✔ 원작 동일
  Assets/@Resources/Prefab/UI/Popup/UI_EnhanceProgressPopup.prefab
    Icon / Floor / Hammer_IncludeAnim 전부 동일 값, **Icon에 id 1~9 DOTweenAnimation 12개가 원작 수치 그대로 이식돼 있음** (덤프로 1:1 확인)

■ 그래서 계산상 접지는 이미 원작과 같거나 더 붙어 있다
  Icon y=0 → 판 아랫변 y=−30.0, 모루 윗면 y=−30.40 → **틈 0.40유닛** (원작 실측 1.0유닛)
  프레임 실측(Screenshots/demo30/0164.png, 1유닛=5.182 native px, x=480 열):
    판 아래 테두리 1466(121)·1467~1469(241)·1470~1473(51) → **틈 1474·1475(둘 다 0) 2px** → 모루 1476(98)
    ⇒ 우리 틈 = 2 native px = **0.39유닛**.  "12px(2.32유닛) 떠 있다"는 이미 해소돼 있다.
  ※ 무대가 원작보다 작아 보이는 것(아치 900/1080=83.3% vs 원작 336/344=97.7%)은 **녹화 레터박스**다.
     캡처 1080x2640 안에 게임 뷰가 933x2280(0.864배)로 들어가 있다(Screenshots/demo30/0020.png 사방 검은 띠). 포트 문제 아님.

■ 진짜로 어긋난 곳은 코드다 — Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs
  L1066~1074 상수:  ICON_HOME_Y=20 / ICON_ANVIL_Y=0 / ICON_LAND_DUR=0.2 / ICON_RISE_Y=50 / ICON_RISE_SC=1.4 / ICON_RISE_DUR=1.4 / ICON_POP_Y=80 / ICON_POP_DUR=0.3 / ICON_DROP_DUR=0.1
  L1108 LandCraftIcon(): DOAnchorPos((0,0), 0.2f).SetEase(**Ease.OutQuad**) + DOScale(1f, 0.2f, OutQuad)
  L147  EnterCeremonyView()에서 Blackout(0.35f)과 **동시에** LandCraftIcon() 호출
  L1021 실패 분기: RestoreCraftIcon(ICON_DROP_DUR)  ← 주석은 "원본 id 8"이라고 적혀 있는데
  L1174~1177 RestoreCraftIcon(): 목적지가 **new Vector2(0f, ICON_HOME_Y)** = (0,**20**)
  L1062 타격 반응: rt.DOPunchScale(new Vector3(0.18f,-0.12f,0f), 0.24f, 10, 0.8f)  ← 위치가 아니라 크기를 눌렀다
  강화창 프리팹 CraftEquipIcon에는 DOTweenAnimation이 **원작과 똑같이 1개(id="")뿐**이라, 연출 수치가 전부 코드 상수로 복제돼 있다.

## 차이
- **[치명]** 실패 강화에서 아이콘이 모루를 떠난다 — 원작 id 8은 LocalMove (0,0,0)인데 우리는 (0,20)으로 간다. 판 아랫변이 모루 윗면에서 20.4유닛(≈106 native px) 뜬 채로 실패 결과를 본다. '아직 모루 위에 있지 않다'의 가장 유력한 정체.
  - 근거: 원작: EnhanceProgress Popup/Icon &1740154056 id='8' LocalMove endValueV3={x:0,y:0,z:0} dur 0.1 ease OutBack (+ &1740154055 id='8' Scale→1.0 dur 1.0 OutQuad). 우리: UI_EnhacePopup.cs:1021 RestoreCraftIcon(ICON_DROP_DUR) → 같은 파일 1174~1177에서 목적지가 new Vector2(0f, ICON_HOME_Y=20f)
- **[눈에띔]** 착지 이징이 다르다. 원작 id 2 = 0.3초 InBack(먼저 위로 살짝 물렸다가 가속해 '쿵' 얹힌다). 우리는 0.2초 OutQuad(스르르 멈춘다) + 원작에 없는 DOScale까지 함께 돈다. 얹히는 무게감이 안 산다.
  - 근거: 원작 &1740154044 id='2' LocalMove (0,0,0) duration 0.3 easeType 26=InBack. 오버슈트 실측: ref f177 판 윗변 387 / f178 386 (기준 388보다 1~2 ref-px 위로 물림) → f179 411 → f180 427. 우리: UI_EnhacePopup.cs:1112~1115 DOAnchorPos(...,0.2f).SetEase(Ease.OutQuad) + DOScale(1f,0.2f,OutQuad)
- **[눈에띔]** 착지 타이밍이 이르다. 원작은 무대가 검어진 뒤 0.5~0.6초 지나서 내려앉는데, 우리는 [강화]를 누른 그 프레임에 암전과 동시에 내려간다.
  - 근거: 원작 실측: 암전 ref f173(217.3s) → 판 윗변 388 유지가 f178(217.8s)까지 → f179(217.9s) 411 → f180(218.0s) 427. 첫 타격은 f183(218.3s). 우리: UI_EnhacePopup.cs:146~147 Blackout(STAGE_CEREMONY, 0.35f); LandCraftIcon(); 이 나란히 호출됨(지연 없음)
- **[눈에띔]** 타격 반응이 '위치 흔들림'이 아니라 '크기 눌림'이다. 원작은 아이콘이 모루 위에서 자리를 지키며 ±2유닛 떨고(그래서 모루에 박혀 있다는 느낌이 난다), 우리는 판이 찌그러졌다 편다.
  - 근거: 원작 &1740154041 id='1' ShakePosition (4,4,0) dur 0.25 OutQuad vibrato 50 randomness 90, 흔들림 뒤 id='2'로 (0,0) 복귀. 프레임 실측: 판 윗변 f183 427 / f185 428 / f188 430 / f194 423 / f197 427 로 ±4 ref-px(±2유닛) 진동, 폭은 57px 고정(스케일 변화 없음). 우리: UI_EnhacePopup.cs:1062 DOPunchScale(new Vector3(0.18f,-0.12f,0f), 0.24f, 10, 0.8f)
- **[사소]** 담금질 구간의 잔진동(id 3)이 통째로 빠져 있다. 원작은 1.4초 상승 내내 아이콘이 미세하게 떨며 올라간다.
  - 근거: 원작 &1740154043 id='3' ShakePosition (1.5,1.5,1) dur 1.4 OutQuad vibrato 50 randomness 90. 우리 RaiseCraftIcon()(UI_EnhacePopup.cs:1120~1130)에는 DOAnchorPos+DOScale+Whiten만 있음
- **[눈에띔]** 판 안쪽 그림이 다르다. 원작 판은 검정 배경 + 가운데 흰 라디얼 글로우 + 아래 붉은 띠에 아이템이 여백을 두고 작게 들어간다. 우리는 판 전체가 통짜 진홍색이고 검이 프레임을 꽉 채워 밖으로 삐져나온다 — 모루 위 '물건'이 아니라 인벤토리 배지로 읽힌다.
  - 근거: 원작 구조 Icon/forAnim/{Background 검정 a=1 size(-3,-3), Glow=knob(fileID 10917) 흰색 size(-2,-2), Image(1) (0.59607846,0.015686275,0.015686275) 40x10 y=5, W Icon 스트레치}. 우리 프리팹도 같은 구조지만 런타임에 꽂히는 WIcon 스프라이트가 붉은 배경판째 그려진 그림. 비교 이미지 AI/forge_ref/cmp3.png(원작 190 / 우리 0164, 모루 기준 정렬)
- **[사소]** (정정) 세로 접지 간격 자체는 이미 원작보다 붙어 있다 — 여기를 더 내리면 오히려 파묻힌다.
  - 근거: 원작 f190 x=160 열: 판 481·482·483 → 틈 484·485 → 모루 486 (2 ref-px = 1.0유닛). 우리 0164 x=480 열: 판 1466~1473 → 틈 1474·1475 → 모루 1476 (2 native px = 0.39유닛)

## 수정안
A. 실패 복귀 목적지 (치명) — Assets/@Scripts/UI/Popup/UI_EnhacePopup.cs

  1) L1021 을 아래로 교체 (RestoreCraftIcon은 창을 닫을 때 쓰는 대기자리 복귀로만 남긴다):
        DropCraftIconToAnvil();   // 원본 id 8 — 실패는 모루 위로 툭 떨어진다

  2) #region 장비 아이콘 궤적 안에 신규 메서드 추가 (원작 id 8 값 그대로):
        /// <summary>실패 — 모루 위(0,0)로 툭 떨어진다. 원본 저작 id "8".</summary>
        void DropCraftIconToAnvil()
        {
            var rt = CraftIcon();
            if (rt == null) return;

            rt.DOKill();
            rt.DOAnchorPos(new Vector2(0f, ICON_ANVIL_Y), ICON_DROP_DUR)   // (0,0) / 0.1초
              .SetEase(Ease.OutBack).SetUpdate(true);
            rt.DOScale(1f, ICON_DROP_SCALE_DUR).SetEase(Ease.OutQuad).SetUpdate(true);  // 1.0 / 1.0초
        }
        const float ICON_DROP_SCALE_DUR = 1.0f;   // 저작 id "8" Scale

     (ICON_DROP_DUR = 0.1f 는 그대로 두면 된다 — 원작 id 8 duration과 같다)

B. 착지 이징/시간 (눈에띔) — 같은 파일 L1108~1118 LandCraftIcon()

     rt.DOKill();
     rt.DOAnchorPos(new Vector2(0f, ICON_ANVIL_Y), ICON_LAND_DUR)
       .SetEase(Ease.InBack).SetUpdate(true);      // ← OutQuad에서 InBack으로
     // DOScale 줄은 삭제 — 원본 id "2"는 Move만이다(크기는 이미 1).

     상수: const float ICON_LAND_DUR = 0.3f;       // ← 0.2f 에서 0.3f (저작 id "2" duration)

C. 착지 타이밍 (눈에띔) — 같은 파일 L146~147 EnterCeremonyView()

     Blackout(STAGE_CEREMONY, 0.35f);
     // 원본은 무대가 검어진 뒤 0.55초 지나서야 장비가 내려앉는다(ref f173 암전 → f179 하강 시작).
     DOVirtual.DelayedCall(ICON_LAND_DELAY, LandCraftIcon).SetUpdate(true);
     ...
     const float ICON_LAND_DELAY = 0.55f;

     ※ 3타의 첫 타격(ChargeSuccess 이전 첫 "Shake")까지 0.4초 이상 여유가 있으니 순서는 안 꼬인다.

D. 타격 반응 (눈에띔) — 같은 파일 L1058~1064 (PopImpactFx 안의 아이콘 반응)

     기존 rt.DOPunchScale(new Vector3(0.18f,-0.12f,0f), 0.24f, 10, 0.8f) 를 원작 id 1 + id 4 조합으로:

        rt.DOComplete();
        // 원본 id "1" — 자리에서 떤다(위치). 모루에 박혀 있다는 느낌은 여기서 나온다.
        rt.DOShakeAnchorPos(0.25f, new Vector2(4f, 4f), 50, 90f, false, true)
          .SetEase(Ease.OutQuad).SetUpdate(true)
          .OnComplete(() =>                              // 원본 id "2" — 다시 (0,0)으로
              rt.DOAnchorPos(new Vector2(0f, ICON_ANVIL_Y), 0.3f)
                .SetEase(Ease.InBack).SetUpdate(true));
        // 원본 id "4" — 살짝 눌림(크기)
        rt.DOPunchScale(new Vector3(0.3f, 0.3f, 0f), 0.3f, 11, 0.367f)
          .SetEase(Ease.InBack).SetUpdate(true);

E. 담금질 잔진동 (사소) — RaiseCraftIcon() 안에 원작 id 3 추가

     rt.DOShakeAnchorPos(ICON_RISE_DUR, new Vector2(1.5f, 1.5f), 50, 90f, false, true)
       .SetEase(Ease.OutQuad).SetUpdate(true);
     ※ 단, 이 Shake는 같은 RectTransform의 DOAnchorPos(+50)와 충돌한다.
        가장 안전한 길은 F안(아래)으로 저작 트윈을 쓰는 것이고, 코드로 가려면
        Icon 아래에 흔들림 전용 자식을 하나 두고 그 자식만 Shake시켜라.

F. (권장, 값 이중관리 제거) 강화창 CraftEquipIcon에 결과창 Icon의 저작 트윈을 그대로 복사
     UI_EnhanceProgressPopup.prefab 의 Icon에는 id "1"~"9" DOTweenAnimation 12개가
     이미 원작 수치 그대로 들어 있다(autoPlay=0). 같은 12개를
     UI_EnhacePopup.prefab 의 CraftEquipIcon 에 복사해 붙이고,
     UI_EnhacePopup.cs 의 Land/Raise/Pop/Restore 를 UI_EnhanceProgressPopup.cs 와 같은
     PlayAuthored(GameObjects.CraftEquipIcon, "2"/"5"/"6"/"8") 호출로 바꾸면
     이징·시간·오버슈트·진동이 전부 원작 값 한 곳에서 나오고, A~E가 한꺼번에 해결된다.
     (이때 id "5"/"7"/"8"은 Scale 짝이 붙어 있으니 GameObject 단위 DORestartById가 둘 다 재생한다)

G. 손대지 말 것
     ICON_ANVIL_Y = 0f, CraftEquipIcon/Icon 의 pos(0,20)·size(30,30)·pivot(0.5,1),
     Hammer* 의 pos(0,156.5)·size(130,130)·pivot(0.5,0), Floor 의 pos(0.38,-100)·scale 0.7.
     전부 원작과 1:1이고, 이 값이면 판 아랫변 −30.0 / 모루 윗면 −30.40 으로 원작(틈 1.0유닛)보다
     오히려 0.6유닛 더 붙는다. 여기서 더 내리면 판이 모루에 파묻힌다.
