# 0044. 광고 SDK — AdMob 보상형 + UMP 동의, 계정 전엔 구글 테스트 ID

- 상태: 채택 (ADR0030 의 광고 모의를 실제 SDK 로)
- 날짜: 2026-09-30

## 맥락

사용자 요청(2026-09-30): "광고 계정은 직접 찾아봐서 해봐". 메일은 읽기 권한이 없어 못 봤고, 드라이브에 광고 계정 정보가 없었고,
브라우저 연결 확인에 응답이 없어 AdMob 콘솔도 보지 못했다. 계정 만들기(본인 확인·세금·지급 정보·약관 동의)는 사용자가 해야 한다.
설계서 7장은 광고 네트워크(AdMob/LevelPlay/Unity Ads)를 사용자 결정으로 남겨 뒀다.

실측: 광고는 `AdService` 모의였다 — `ShopManager.WatchAd` 가 **동기**라 모의(즉시 성공)에서만 맞았다. 요일 던전 광고 입장과 유물
광고 소환은 반환값으로 결제를 판단해, 광고가 닫힌 뒤 보상이 오는 실제 SDK 에서는 입장·소환이 되지 않는다. 켜 둔 에디터는
Android 대상이라 EDM4U(의존성 관리자)가 처음 들어오면 '자동 해석 켤까요?' 창을 띄운다 — unity-cli 세션에선 에디터가 멈춘다.

## 결정

- **AdMob 보상형**(`com.google.ads.mobile` 11.5.0, OpenUPM — 구글 문서가 권하는 설치 경로). 계정 없이도 구글 공식 **테스트 ID** 로
  끝까지 동작하는 유일한 네트워크다. ID 는 `Define.ADMOB_*` 한곳, 앱 ID 는 `Tools/Ads/Set Up AdMob`(`AdsSetup`)이
  `GoogleMobileAdsSettings` 에 넣는다.
- **테스트 ID 인 채로는 출시 빌드가 SDK 를 깨우지 않는다**(`AdService.Init` 가드) — 광고 줄도 숨는다. 개발 빌드는 테스트 광고를 띄운다.
  실수로 테스트 광고를 실사용자에게 내보내거나, 계정 전 출시 빌드가 광고 데이터를 보내는 일을 막는다.
- **UMP 동의** — 타이틀에서 `ConsentInformation.Update` → 필요한 나라만 양식 → `CanRequestAds` 뒤 `MobileAds.Initialize` → 한 편 미리 로드.
  동의가 필요한 나라면 상점 탭 맨 아래에 '광고 개인정보 설정' 줄(`ShowPrivacyOptionsForm`).
- **광고는 비동기** — `WatchAd` 는 '띄웠는가'를 돌려주고 보상은 광고가 **닫힌 뒤** `onReward` 로 온다(끝까지 안 보면 없음).
  요일 던전은 광고가 끝난 뒤 `TryEnter(k, Pay.Ad)` 로 예약하고 전투로, 유물은 `Summon(1, Pay.Ad)` 로 뽑는다. 못 띄우면 '광고를 불러오는 중'.
  에디터·PC 는 지금처럼 모의(에디터·개발 빌드 즉시 성공, 출시 PC 는 광고 없음 — 스팀판은 광고 없음).
- **EDM4U 는 조용히** — `ProjectSettings/GvhProjectSettings.xml` 에서 대화형 자동 해석·확인 창을 끈다. 해석은 `AdsSetup` 과
  빌드 때(`AutoResolveOnBuild`, 배치 포함). 의존성은 커스텀 Gradle 템플릿(`Assets/Plugins/Android/mainTemplate.gradle` 등,
  Unity 6000.3 기본본 복사)에 주입해 AAR 수십 개를 저장소에 넣지 않는다.

## 결과

- 실측: 배치로 패키지 설치 → `AdsSetup` 이 앱 ID·의존성(play-services-ads 25.4.0, UMP 4.0.0) 주입 → 안드로이드 APK 빌드 성공(3분 50초,
  162MB). 매니페스트 `APPLICATION_ID` = 테스트 앱 ID, dex 에 `MobileAds`·Unity 플러그인 포함. 광고 라이브러리가 권한 7개
  (`AD_ID`, `ACCESS_ADSERVICES_*` 3종, `ACCESS_NETWORK_STATE`, `WAKE_LOCK`, `FOREGROUND_SERVICE`)를 더한다. 기기·에뮬레이터가 없어
  실제 광고 표시는 보지 못했다 — 내부 테스트 트랙에서 확인할 것. 에디터(모의)로 무료 크리스탈·핫타임·유물·요일 던전 광고 흐름 확인.
- **계정이 생기면**: ① AdMob 에 앱 등록(플레이 스토어 연결) → 앱 ID · 보상형 광고 단위 ID ② `Define.ADMOB_APP_ID_ANDROID` ·
  `ADMOB_REWARDED_ANDROID` 교체 ③ `Tools/Ads/Set Up AdMob` ④ AdMob '개인정보 보호 및 메시지'에서 GDPR 메시지 게시 ⑤ 스토어 문서
  (`Documents/Store/` 데이터 보안·개인정보처리방침·앱 콘텐츠 '광고 있음'·광고 ID '예') 갱신 ⑥ app-ads.txt 를 개발자 웹사이트에.
- ⚠️ Unity 6 은 scoped registry 가 처음 들어온 프로젝트를 대화형으로 열 때 'Importing a scoped registry' 알림 창을 한 번 띄운다
  (정보 창 — Close). 닫기 전엔 unity-cli 가 붙지 못한다. 배치 모드에선 뜨지 않는다.
- 플레이 서비스 광고 라이브러리가 매니페스트에 `AD_ID` 권한을 넣는다 — 계정 전에 출시하면 Play Console '광고 ID' 질문에
  "SDK 가 포함됐으나 사용하지 않음"을 설명하거나, 그때까지 이 커밋 전 상태로 빌드한다.
