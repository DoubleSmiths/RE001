# 출시 체크리스트 — Google Play · Steam (소유자용)

기준일은 2026-09-29이다. 다음 값은 아직 확정되지 않은 가정이다.

- 게임명: `미니몬 퀘스트 / Minimon Quest` (가칭)
- 패키지명: `com.doublesmiths.minimonquest` (가칭 — 회사명 DoubleSmiths 는 GitHub 조직명에서 가져옴)
- 판매 방식: 유료, 인앱 결제 없음

개발 쪽 작업은 `../Release_Readiness_2026-09-29.md`에 있다. 요구사항의 출처는 맨 아래에 적었다.

관련 문서: `Store_Listing.md` · `Privacy_Policy.md` · `Google_Play_Data_Safety.md` · `Third_Party_Notices.md` / `.txt`

---

## 0. 되돌리기 어려운 결정부터

| 결정 | 먼저 해야 하는 이유 |
|---|---|
| 게임명 확정과 상표 검색 (KIPRIS, USPTO, EUIPO) | "Minimon" 계열의 기존 상표와 충돌하는지 확인한다. 스토어 제목, 로고, 도메인이 이름을 따라간다 |
| 패키지명 | Google Play에 출시한 뒤에는 바꿀 수 없다 |
| Player Settings의 회사명·제품명 (현재 임시값 `DoubleSmiths` / `Minimon Quest`, 2026-09-29 적용) | Windows 저장 경로 `LocalLow\<회사>\<제품>`, 레지스트리, Steam Cloud 경로, 창 제목이 여기서 정해진다. 출시 후 바꾸면 기존 세이브를 찾지 못한다. GitHub 조직명 `DoubleSmiths`를 회사명으로 쓸지도 여기서 정한다. exe 이름은 이 값과 별개로 `BuildScript`의 출력 파일명(`MinimonQuest.exe`)이 정한다 |
| 판매 방식 | Google Play는 무료로 한 번 출시하면 유료로 바꿀 수 없다. 바꾸려면 새 패키지명이 필요하다 |
| Play 개발자 계정 종류 | **개인**: 신분증 확인이 필요하다. 2023-11-13 이후 만든 계정은 비공개 테스트(12명 × 14일 연속)를 통과해야 프로덕션에 출시할 수 있다. **조직**: D-U-N-S 번호가 필요하고 이 테스트 요건은 없다 |
| 한국 등급 — Steam | Google Play는 IARC 설문으로 GRAC 등급까지 받는다. **Steam은 국내 자체등급분류사업자가 아니다.** 한국에 팔려면 게임물관리위원회(GRAC) 등급분류(유료)를 받아야 한다. 다른 방법은 자체등급분류 권한이 있는 PC 플랫폼(Microsoft Store, Epic, STOVE 등)에서 등급을 받아 쓰는 것이다. 청소년이용불가가 아닌 경우에만 가능하므로, 진행 전에 GRAC에 절차를 문의한다 |

## 1. 공통 출시 차단 항목

- [x] **포켓몬 음원 2개 제거 확인.** (커밋 `0b3d162a` — 파일·어드레서블 항목 삭제, 잡기 BGM 재생 클립 확인) `BGM_Minion_battle.mp3`와 `SFX_MinimonCatch.mp3`는 ID3 태그상 "Pokemon Red / Junichi Masuda"(트랙 24/51, 27/51)에서 추출한 음원이다. 자체 합성 칩튠 `BGM_MinimonCapture.wav`(루프라 무손실 WAV — MP3 는 Unity 가 인코더 지연을 무시해 루프마다 52ms 틈이 났다)와 `SFX_MinimonCaught.mp3`(생성기 `_tools/gen_chiptune.py`)로 교체했다. 빌드 전에 세 가지를 확인한다.
  - 옛 파일 두 개와 그 어드레서블 `Sound` 그룹 항목이 모두 사라졌다.
  - `UI_TamePopup`이 새 파일을 재생한다.
  - 빌드 산출물에 옛 주소가 남아 있지 않다.
- [x] **구매 영수증 확인.** (2026-09-30 해당 5개를 자작 효과음으로 교체 — `_tools/gen_sfx.py`, ADR0048. 영수증 확인이 필요 없어졌다) `Slime_Die.mp3`와 `Slime_OnHit.mp3`는 SmartSound FX(MONSTERS) © 2019 음원이다. 이 두 파일은 몬스터 503종 모두의 피격·사망음이다. `Sound_Miss.mp3`, `Sound_PlayerAttack.mp3`, `Sound_Smash.mp3`는 Medieval Fantasy Audio Bundle(Evil Mind Entertainment, 2013) 음원이다. 구매 내역(예: Unity Asset Store)과 라이선스의 크레딧 조건을 확인한다. 확인하지 못하면 교체한다.
- [x] **출처 미상 음원 확인.** (2026-09-30 효과음 56개 전부 자작 합성음으로 교체 — `_tools/gen_sfx.py`, ADR0048) BGM 은 전부 자작곡이다(2026-09-29 `game/remake-v2`: 타이틀·마을·필드·보스 4곡을 `_tools/gen_bgm_suite.py` 로 작곡, 옛 BGM 5곡·ENV 2개와 그 어드레서블 항목 삭제). 남은 SFX 51개는 메타데이터가 없다. 모든 음원은 초기 커밋 `e191a5ba` "[SETTING] 프로젝트 세팅"(2026-03-06)으로 한꺼번에 들어왔다. 파일마다 출처를 기록하거나 교체한다.
- [ ] 그 밖의 `UNKNOWN — VERIFY` 항목(픽셀 UI·배경 팩, 셰이더 2종)의 출처와 라이선스를 확인하고, 결과를 `Third_Party_Notices.md`에 기록한다.
- [ ] `Assets/IMPORTS/EpicVictoryEffects/Fonts/calibril.ttf`(Microsoft 폰트)를 삭제한다. (확인: 에셋 데모 씬 2개만 참조 — 빌드 목록에 없어 **출시물에는 포함되지 않는다**) EmojiOne을 계속 쓸지도 정한다.
- [x] Unity Diagnostics를 끈다(`Project Settings > Services > Diagnostics` → Disabled). (2026-09-29 `m_EngineDiagnosticsEnabled: 0` 커밋) 빌드한 뒤 네트워크 통신이 0건인지 확인한다. 이 작업이 끝나야 `Privacy_Policy.md`와 `Google_Play_Data_Safety.md`의 A안이 사실이 된다.
- [x] 게임 안 크레딧 화면에 `Assets/@Resources/Data/Credits.txt`를 표시하고(설정 → 크레딧, 어드레서블 등록 완료), 설정 또는 크레딧 화면에 개인정보처리방침 링크를 넣는다(Release_Readiness 7번). Google Play는 앱 안의 링크도 요구한다.
  - `Credits.txt`는 어드레서블에 등록해야 런타임에서 읽힌다.
  - `{version}`은 실제 버전으로 바꿔 표시한다.
  - `Documents/Store/Third_Party_Notices.txt`와 내용이 같으므로 한쪽을 고치면 다른 쪽도 고친다.
- [ ] 개인정보처리방침을 HTML로 공개 URL에 게시한다. GitHub Pages나 Notion 공개 페이지 등을 쓴다. PDF는 안 된다.
- [ ] 스토어에 공개할 연락처 이메일을 만든다.

---

## 2. Google Play

### 2-1. 계정

- [ ] Play Console에 가입한다. 등록비는 US$25이고 한 번만 낸다. 신원 확인이 필요하다.
- [ ] 유료로 판매하려면 결제 프로필(판매자 계정)을 연결한다.
- [ ] EU 디지털서비스법(DSA) 판매자(trader) 상태를 신고한다. 유료 앱을 EU에 팔면 판매자에 해당할 가능성이 높다. 판매자면 주소, 전화, 이메일이 EU 스토어에 공개된다. 공개할 주소를 정하거나, EU를 배포에서 빼기로 결정한다.

### 2-2. 빌드 (AAB)

- [x] **targetSdk 36.** (ProjectSettings `AndroidTargetSdkVersion: 36` 고정) 2026-08-31부터 신규 앱과 업데이트에 필수다(연장을 요청하면 2026-11-01까지). 현재 설정은 `AndroidTargetSdkVersion: 0`으로 설치된 최고 SDK를 따르며, 9/9 APK는 36이었다. 36으로 명시해 고정한다.
- [ ] minSdk는 현재 값 25를 그대로 써도 된다.
- [ ] App Bundle로 출력한다. `BuildScript.AndroidBundle`이 `MinimonQuest.aab`를 만든다(메뉴 `Tools/Build/안드로이드 AAB (구글 플레이)`).
  - batchmode로 실행한다: `-executeMethod BuildScript.AndroidBundle -versionCode <N>`.
  - `BuildScript.Android`가 만드는 APK(`MinimonQuest.apk`)는 기기에 직접 설치해 보는 용도다.
- [ ] **16KB 페이지 크기.** 2025-11-01부터 Android 15 이상을 대상으로 하는 앱에 필수다. Unity 6000.3은 지원한다. 업로드할 때 Play Console에 경고가 없는지 확인한다.
- [ ] ARM64만 빌드한다. 현재 `AndroidTargetArchitectures: 2`로 이미 맞다.
- [ ] **크기.** 기본 모듈의 압축 다운로드 크기는 200MB까지다. 9/9 APK는 162MB였고 그중 어드레서블 UI가 46MB, Sprites가 44MB다. 한도를 넘으면 텍스처를 압축하거나 Play Asset Delivery를 쓴다.
- [ ] versionCode는 업로드할 때마다 올린다. versionName은 게임 안에 표시되는 버전과 맞춘다.
- [ ] 릴리스 빌드에서는 Development Build를 끄고 로그를 필터한다(Release_Readiness 3번).

### 2-3. 서명 (Play App Signing + 업로드 키)

- [ ] 업로드 키스토어를 **저장소 밖에서** 만든다.
  ```
  powershell -ExecutionPolicy Bypass -File _tools/make_upload_keystore.ps1
  ```
  - 기본 출력 경로는 `%USERPROFILE%\.minimonquest\upload.keystore`이고 형식은 PKCS12다. 키 비밀번호와 저장소 비밀번호가 같다.
  - 스크립트는 저장소 안 경로를 거부한다.
- [ ] 키스토어 파일과 비밀번호를 오프라인 두 곳 이상에 백업한다. 저장소나 `.env.local`에는 넣지 않는다.
- [ ] 빌드 전에 환경변수를 설정한다: `RE001_KEYSTORE`, `RE001_KEYSTORE_PASS`, `RE001_KEY_ALIAS`(`upload`), `RE001_KEY_PASS`.
  - 설정하지 않으면 디버그 키로 서명되고, Play Console이 업로드를 거부한다.
- [ ] 첫 업로드 때 Play App Signing을 켠다. 신규 앱은 필수다. "Google에서 앱 서명 키 생성"을 권장한다.
  - 업로드 키를 잃어버려도 Play Console에서 재설정을 요청할 수 있다. 앱 서명 키는 Google이 보관한다.

### 2-4. 테스트 트랙

- [ ] 내부 테스트: 최대 100명까지 가능하며, 설치와 실행을 빠르게 확인하는 용도다.
- [ ] **비공개 테스트**(개인 계정만 해당): 테스터 12명 이상이 14일 연속 opt-in 상태를 유지해야 한다. 그다음 대시보드에서 "프로덕션 액세스 신청"을 누른다. 검토는 보통 7일 이내다.
  - 테스터 모집이 일정의 임계 경로다. 가장 먼저 시작한다.
- [ ] 사전 출시 보고서(Pre-launch report)를 확인한다. 테스트 트랙에 올리면 자동으로 생성된다. 충돌, ANR, 접근성, 보안 경고를 본다.

### 2-5. 스토어 등록정보 (`Store_Listing.md`)

- [ ] 앱 이름 30자, 짧은 설명 80자, 자세한 설명 4000자 이내로 한국어와 영어를 모두 입력한다.
- [ ] 앱 아이콘: 512×512, 32비트 PNG(알파 포함), 1MB 이하.
- [ ] 그래픽 이미지(Feature graphic): 1024×500, JPG 또는 24비트 PNG(알파 없음).
- [ ] 스크린샷: 휴대전화용 2–8장, JPG 또는 24비트 PNG. 한 변은 320–3840px이고, 긴 변이 짧은 변의 2배를 넘으면 안 된다.
  - 세로 게임이므로 **9:16, 1080×1920 이상, 3장 이상**을 넣는다. 추천 영역에 노출되는 조건이다. 실제 게임 화면이어야 한다.
- [ ] (선택) 7인치·10인치 태블릿 스크린샷, YouTube 트레일러 URL.
- [ ] 카테고리(롤플레잉), 태그, 연락처 이메일, 개인정보처리방침 URL을 입력한다.

### 2-6. 앱 콘텐츠

- [ ] `Google_Play_Data_Safety.md`의 답을 그대로 입력한다.
  - 대상 항목: 개인정보처리방침, 앱 액세스, 광고, 콘텐츠 등급(IARC), 타겟층(13세 이상), 뉴스, Data safety(수집 없음), 정부, 금융, 건강, 광고 ID.

### 2-6a. 광고 (AdMob — ADR0044, 무료+광고로 낼 때만)

코드는 들어가 있다(`AdService`, `com.google.ads.mobile` 11.5.0, UMP 동의). 지금 ID 는 구글 **테스트** ID 라 출시 빌드는 SDK 를 깨우지 않고
광고 줄도 숨긴다. 계정이 생기면 아래 순서로 켠다.

- [ ] AdMob 계정을 만든다(본인 확인·지급 정보·세금). 앱을 추가하고 Play 스토어 등록정보와 연결한다.
- [ ] 보상형 광고 단위를 하나 만든다. 앱 ID(`~`)와 광고 단위 ID(`/`)를 `Utils/Define.cs` 의 `ADMOB_APP_ID_ANDROID` ·
      `ADMOB_REWARDED_ANDROID` 에 넣고 `Tools/Ads/Set Up AdMob` 을 한 번 돌린다(앱 ID 가 설정 자산·매니페스트로 들어간다).
- [ ] AdMob **개인정보 보호 및 메시지**에서 GDPR(유럽) 동의 메시지를 만들어 게시한다 — 게임은 UMP 로 그 양식을 띄우고,
      필요한 나라에선 상점 탭 맨 아래 '광고 개인정보 설정' 줄로 다시 고르게 한다.
- [ ] 개발자 웹사이트 루트에 `app-ads.txt` 를 올린다(AdMob 이 한 줄을 준다). 스토어 등록정보의 웹사이트와 같은 도메인이어야 한다.
- [ ] `Google_Play_Data_Safety.md` 를 **C안**으로, 개인정보처리방침에 광고 조항을 넣고, 앱 콘텐츠의 광고 '예' · 광고 ID '예'로 바꾼다.
- [ ] 내부 테스트 트랙에서 개발 빌드로 테스트 광고 → 출시 빌드로 실제 광고가 한 편 나오는지(보상 지급·요일 던전 입장·유물 소환) 확인한다.
- ⚠️ 광고 라이브러리가 매니페스트에 `AD_ID` 권한을 넣는다. 광고를 켜지 않고 먼저 출시한다면 콘솔 '광고 ID' 질문의 답을 이에 맞추거나,
  광고 커밋 전 상태로 빌드한다.

### 2-7. 가격·배포

- [ ] 가격과 배포 국가를 정한다. 한국은 IARC로 GRAC 등급이 부여된다.
- [ ] 세로 고정 게임이므로 태블릿과 크롬북에 배포할지 정한다(기기 카탈로그).
- [ ] 프로덕션에 출시하고 검토를 기다린다. 신규 앱은 며칠 이상 걸릴 수 있다. 단계적 출시(예: 20%)로 시작하고 Android vitals에서 충돌률과 ANR을 확인한다.

---

## 3. Steam

### 3-1. 파트너 가입

- [ ] partner.steamgames.com에 가입하고 Steam Direct 앱 수수료 US$100을 낸다. 조정 총수익이 US$1,000을 넘으면 정산 때 돌려받는다.
- [ ] 은행 정보, 세금 인터뷰, 신원 확인을 마친다. 비미국 거주자는 W-8BEN(법인은 W-8BEN-E)을 쓰고 한미 조세조약에 따른 원천징수 감면을 확인한다.
- [ ] **수수료를 낸 뒤 30일이 지나야 출시할 수 있다.** 일정 계산은 이 날짜에서 시작한다.

### 3-2. 앱 기본 설정

- [ ] App ID를 발급받고 Depot을 하나 만든다(Windows 64-bit).
- [ ] 실행 옵션(Launch Options)을 설정한다. 실행 파일은 `MinimonQuest.exe`, OS는 Windows, 64-bit 전용이다.
  - exe 이름은 `BuildScript`의 `EXE` 상수가 정한다. 그 값을 바꾸면 여기와 아래 depot 제외 경로도 함께 바꾼다.
- [ ] 지원 OS를 Windows 10/11 64-bit로 설정한다. Steamworks SDK(업적 등)는 없어도 출시할 수 있다.

### 3-3. SteamPipe 업로드

Windows 빌드는 batchmode로 만든다: `-executeMethod BuildScript.Windows`. 메뉴로는 `Tools/Build/윈도우 (스팀)`이다.

빌드 출력은 `Builds/StandaloneWindows64/`에 생긴다. `MinimonQuest.exe`, `MinimonQuest_Data/`, `UnityPlayer.dll`, `UnityCrashHandler64.exe` 등이 들어 있다. 이 폴더를 콘텐츠 루트로 쓰고, 이름에 `DoNotShip`이나 `DontShip`이 들어간 폴더는 빼야 한다.

`ContentBuilder/scripts/app_build_<APPID>.vdf`

```
"AppBuild"
{
  "AppID" "<APPID>"
  "Desc" "Minimon Quest <versionName> Windows x64"
  "ContentRoot" "C:\\stopresent\\Unity\\RE001\\Builds\\StandaloneWindows64\\"
  "BuildOutput" "..\\output\\"
  "SetLive" "beta"
  "Depots"
  {
    "<DEPOTID>" "depot_build_<DEPOTID>.vdf"
  }
}
```

`ContentBuilder/scripts/depot_build_<DEPOTID>.vdf`

```
"DepotBuild"
{
  "DepotID" "<DEPOTID>"
  "FileMapping"
  {
    "LocalPath" "*"
    "DepotPath" "."
    "Recursive" "1"
  }
  "FileExclusion" "*.pdb"
  "FileExclusion" "MinimonQuest_BurstDebugInformation_DoNotShip\\*"
  "FileExclusion" "MinimonQuest_BackUpThisFolder_ButDontShipItWithYourGame\\*"
}
```

```
ContentBuilder\builder\steamcmd.exe +login <빌드용_Steam_계정> +run_app_build ..\scripts\app_build_<APPID>.vdf +quit
```

- 비밀번호는 명령줄에 쓰지 않는다. 쓰지 않으면 steamcmd가 묻는다. 권한을 제한한 빌드 전용 계정을 권장한다.
- `SetLive "beta"`를 쓰려면 beta 브랜치를 먼저 만들어 둔다. **default 브랜치는 자동으로 라이브할 수 없다.** Steamworks의 Builds 페이지에서 직접 라이브한다.
- 업로드 로그에서 제외 폴더가 빠졌는지 확인한다.

### 3-4. Steam Cloud (Auto-Cloud)

Steamworks에서 `Application > Steam Cloud`로 들어간다. 사용자당 용량은 10MB, 파일 수는 10개로 둔다. 실제 `SaveData.json` 크기를 보고 여유를 둔다.

| Root | Subdirectory | Pattern | OS | Recursive |
|---|---|---|---|---|
| WinAppDataLocalLow | `<회사명>/<제품명>` | `SaveData.json` | Windows | No |
| WinAppDataLocalLow | `<회사명>/<제품명>` | `SaveData.json.bak` | Windows | No |

- `WinAppDataLocalLow`는 `%USERPROFILE%\AppData\LocalLow\`를 가리킨다. Unity의 `persistentDataPath`가 여기다.
- `.tmp`와 `.corrupt-*` 파일은 동기화하지 않는다.
- **레지스트리(PlayerPrefs)는 동기화되지 않는다.** 현재 레지스트리에 있는 것은 언어 설정뿐이다. 진행에 필요한 값은 `SaveData.json`에만 둔다.
- 테스트 방법:
  1. `steam://open/console`을 연다.
  2. `testappcloudpaths <AppID>`와 `set_spew_level 4 4`를 입력한다.
  3. 저장 폴더에 `steam_autocloud.vdf`가 생기는지 확인한다.
  4. 두 번째 PC에서, 또는 로컬 저장을 지운 뒤 세이브가 복원되는지 확인한다.
  5. 끝나면 `testappcloudpaths 0`과 `set_spew_level 0 0`으로 되돌린다.

### 3-5. 스토어 페이지

- [ ] `Store_Listing.md`의 내용을 입력한다. 짧은 설명(300자 이내), About This Game, 태그, 지원 언어(한국어·영어, 인터페이스와 자막), 시스템 요구사항, 가격이다.
- [ ] 그래픽 에셋을 준비한다(PNG 또는 JPG).

| 에셋 | 크기 | 규칙 |
|---|---|---|
| Header capsule | 920×430 | 제목·로고 외의 문구(평점, 수상, 인용문)는 넣지 않는다 |
| Small capsule | 462×174 | 로고가 거의 꽉 차게 넣는다. 120×45로 줄어도 읽혀야 한다 |
| Main capsule | 1232×706 | 제목 외 문구 금지 |
| Vertical capsule | 748×896 | 제목 외 문구 금지 |
| Page background (선택) | 1438×810 | 본문을 방해하지 않는 은은한 배경 |
| Screenshots | **5장 이상, 1920×1080 이상, 16:9** | 실제 게임 화면만 넣는다. 세로 게임이므로 16:9 캔버스 가운데에 게임 화면을 놓고 배경을 채운다. 4장 이상은 "전 연령 적합"으로 표시한다 |
| Library capsule | 600×900 | 키 아트와 로고 |
| Library header | 920×430 | 없으면 Header capsule을 대신 쓴다 |
| Library hero | 3840×1240 | 글자를 넣을 수 없다. 핵심 요소는 가운데 860×380 안전 영역에 둔다 |
| Library logo | 폭 1280 또는 높이 720 이상, 투명 PNG | 로고만 넣는다 |
| 트레일러 | 권장 | 게임 화면 위주로 만든다 |

- [ ] 로고와 제목에는 Ria Sans를 쓰지 않는다. 폰트 라이선스가 BI/CI 사용을 금지한다.

### 3-6. 콘텐츠 설문과 AI 공개

- [ ] General Content와 Mature Content는 `Store_Listing.md` 3장을 기준으로 답한다.
- [ ] Generative AI 항목에서 **Pre-Generated를 "예"로 답한다.** 설명 초안(영문, 사실에 맞게 고친다):
  > Monster artwork (bestiary icons and animation frames), equipment and skill icons, region backgrounds and parts of the town art were generated with an AI image tool (AetherForge), then cleaned up, cropped and downscaled by the developer. *(해당하면 추가)* Some in-game text and the English translation were drafted with AI assistance and reviewed by the developer.
  - 대사, 설명문, 번역을 AI로 작성했다면 사실대로 포함한다. 코딩 보조 도구는 공개 대상이 아니다.
- [ ] Live-Generated는 "아니요"로 답한다. 실행 중에 생성하는 콘텐츠가 없다.

### 3-7. 심사 일정 (역산 예시)

| 시점 | 할 일 |
|---|---|
| D-45 이전 | 앱 수수료를 낸다. 30일 대기가 시작된다 |
| D-30 | 스토어 페이지 검토를 제출한다. 검토에 3–5 영업일이 걸리므로, 공개하려는 날보다 7 영업일 앞서 낸다 |
| D-21 | **Coming Soon 페이지를 공개한다.** 출시 최소 2주 전에 공개해야 한다 |
| D-14 | 빌드 검토를 제출한다. 3–5 영업일이 걸리며 7 영업일로 잡는다 |
| D-0 | default 브랜치에 빌드를 라이브하고 "Release App"을 누른다 |

### 3-8. Steam Deck 참고

- **화면.** Deck은 1280×800(16:10) 가로 화면이다. 세로 게임은 좌우 여백을 두고 표시되며, 1920 높이 기준 UI가 약 0.42배로 줄어든다. Valve 기준은 1280×800에서 가장 작은 글자가 **9px 이상**이다. 가장 작은 UI 글자로 실제로 확인한다.
- **입력.** Verified를 받으려면 기본 컨트롤러 구성만으로 모든 기능에 접근할 수 있어야 한다. 이 게임은 마우스 전용이라 트랙패드나 터치스크린으로 조작한다. 그래서 **"Playable"이 현실적인 목표**다.
  - Steamworks의 Steam Input에서 기본 구성을 마우스 기반 템플릿으로 지정해 두면 조작감이 나아진다.
- **텍스트 입력.** 런타임에 텍스트 입력 필드가 없으므로 온스크린 키보드는 고려하지 않아도 된다.
- **검토.** Valve가 자동으로 대상을 정하거나 Steamworks에서 요청할 수 있다. 결과 메일을 받고 약 1주 뒤에 공개된다.

---

## 4. 출시 당일과 직후

- [ ] 두 스토어에서 실제로 내려받아 설치하고, 새 게임 시작과 세이브 이어하기를 확인한다.
- [ ] 크레딧과 개인정보처리방침 링크가 동작하는지 확인한다.
- [ ] Google Play는 Android vitals(충돌·ANR)를, Steam은 리뷰와 토론을 모니터링한다.

---

## 출처 (2026-09-29 확인)

- Google Play — [신규 개인 계정 테스트 요건](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en) · [타깃 API 요건](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en) · [미리보기 에셋 규격](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en) · [Data safety](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en) · [사용자 데이터 정책](https://support.google.com/googleplay/android-developer/answer/10144311?hl=en) · [콘텐츠 등급](https://support.google.com/googleplay/android-developer/answer/9859655?hl=en) · [가격 설정(무료→유료 불가)](https://support.google.com/googleplay/android-developer/answer/6334373?hl=en) · [계정 유형](https://support.google.com/googleplay/android-developer/answer/13634885?hl=en) · [16KB 페이지 요건](https://android-developers.googleblog.com/2025/05/prepare-play-apps-for-devices-with-16kb-page-size.html) · [Unity Play 배포 요건(200MB)](https://docs.unity3d.com/6000.2/Documentation/Manual/android-distribution-google-play.html)
- Steam — [Steam Direct](https://partner.steamgames.com/steamdirect) · [Coming Soon](https://partner.steamgames.com/doc/store/coming_soon) · [검토 절차](https://partner.steamgames.com/doc/store/review_process) · [스토어 그래픽](https://partner.steamgames.com/doc/store/assets/standard) · [라이브러리 에셋](https://partner.steamgames.com/doc/store/assets/libraryassets) · [SteamPipe 업로드](https://partner.steamgames.com/doc/sdk/uploading) · [Steam Cloud](https://partner.steamgames.com/doc/features/cloud) · [Content Survey(AI 공개)](https://partner.steamgames.com/doc/gettingstarted/contentsurvey) · [Steam Deck 호환성 검토](https://partner.steamgames.com/doc/steamhardware/compat)
- 국내 등급 — [게임메카: 스팀 출시와 등급분류](https://www.gamemeca.com/view.php?gid=1751893) · [인디터: PC 게임 심의 방법](https://inditor.co.kr/bbs/board.php?bo_table=str&wr_id=37)
- 기타 — [AetherForge 이용약관](https://aetherforgeai.com/en/policy/service) · [Unity Diagnostics 설정](https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/editor-settings-reference/comp-manager-group/diagnostics-settings)
