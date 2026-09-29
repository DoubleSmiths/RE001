# Google Play — Data safety 양식 · 앱 콘텐츠 답안

기준일 2026-09-29. Play Console 화면은 영어 문구를 괄호로 함께 적었다. 답의 근거는 아래 "실측"에 있다. 코드나 설정이 바뀌면 다시 확인한다.

## 실측 근거

| 확인 대상 | 결과 |
|---|---|
| 런타임 코드 `Assets/@Scripts` (Editor 제외) | `UnityWebRequest`, `HttpClient`, `WebClient`, `Socket`, Analytics, Ads, Purchasing, Firebase, 광고 ID, 위치·카메라·마이크 API 사용이 **0건**이다. 예외는 `UI/Scene/UI_AreaBtnGroup.cs:3`의 `using System.Net.Http.Headers;` 한 줄뿐이고, 이를 쓰는 코드는 없다. 지워도 된다 |
| 에디터 전용 네트워크 코드 | `Editor/DataTransformer`의 WebClient(구글 시트, 폐기된 경로), unity-cli connector(`includePlatforms: ["Editor"]`). 빌드에 들어가지 않는다 |
| 패키지 (`Packages/manifest.json`) | 광고·결제·분석·로그인 SDK가 없다. UIParticle, UI Extensions, DOTween은 통신하지 않는다 |
| `ProjectSettings/UnityConnectSettings.asset` | Analytics `0`, Ads `0`, Purchasing `0`, PerformanceReporting `0`, CrashReporting `m_EnableCloudDiagnosticsReporting: 0`. **그러나** `m_Enabled: 1`(커밋되지 않은 변경, 커밋본은 `0`)이고 `InsightsSettings.m_EngineDiagnosticsEnabled: 1`이다. 또 `ProjectSettings.asset`에 `cloudProjectId`가 설정돼 있다 |
| `ProjectSettings.asset` | `submitAnalytics: 1`. 숨은 레거시 하드웨어 통계 플래그다. Unity 포럼 답변에 따르면 Analytics를 끄면 함께 꺼진다. 네트워크 모니터로 확인한다 |
| 9월 9일 APK (`Builds/Android/RE001.apk`, aapt2) | 권한은 `INTERNET` 하나다. Unity가 자동으로 넣는다. 나머지는 Android 기본 `DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`이다. `AD_ID` 권한은 없다. `android:allowBackup`은 지정하지 않았다(기본값 true) |

**전제: Unity Diagnostics를 끈다.** `Edit > Project Settings > Services > Diagnostics`를 Disabled로 두고, Build Profiles 재정의도 확인한다. Unity 문서에 따르면 Unity Cloud에 연결된 6.2+ 프로젝트는 기본 켜짐이다. 수집 항목은 충돌 로그, 원격 측정, 구성 메타데이터다. 끄지 않으면 A안이 거짓 신고가 된다.

## A안 — Diagnostics를 끈 경우 (권장)

| 질문 | 답 |
|---|---|
| 앱이 필수 사용자 데이터 유형을 수집하거나 공유하나요? (Does your app collect or share any of the required user data types?) | **아니요 (No)** |

스토어에는 "수집하는 데이터 없음 / 제3자와 공유하는 데이터 없음"으로 표시된다.

- **로컬 저장은 수집이 아니다.** Google 정의상 "수집"은 기기 밖으로 전송하는 것이다. 기기 안에서만 처리하는 데이터는 신고 대상이 아니다. 세이브 파일, 언어 설정, 마지막 접속 시각이 여기에 해당한다.
- **Android 자동 백업·Steam 클라우드는 앱이 전송하는 것이 아니다.** 이용자 계정의 플랫폼 기능이다. 개발자가 접근하지 못하므로 A안에 영향이 없다. 다만 개인정보처리방침에 한 줄 적어 두었다.
- 데이터를 수집하지 않아도 양식은 반드시 제출해야 하고 개인정보처리방침 URL도 필요하다.

## B안 — Diagnostics를 켜 둘 경우

Unity가 수집 항목을 구체적으로 공개하지 않아 정확히 신고하기 어렵다. 그래서 권장하지 않는다. 켜 둔다면 최소한 다음처럼 답하고, 개인정보처리방침의 [선택 조항 A]를 남긴다.

| 항목 | 답 |
|---|---|
| 수집 여부 | 예 |
| 데이터 유형 | 앱 정보 및 성능 → 비정상 종료 로그(Crash logs), 진단(Diagnostics). 기기 ID를 쓰는지 Unity에 확인되지 않으면 기기 또는 기타 ID(Device or other IDs)도 검토한다 |
| 공유 | 아니요. Unity는 개발자를 대신해 처리하는 서비스 제공업체다 |
| 처리 방식 | 수집됨, 필수(이용자가 끌 수 없음), 목적은 분석(Analytics) |
| 전송 중 암호화 | 예 (HTTPS). 빌드에서 확인한다 |
| 삭제 요청 방법 | 제공 방법을 적는다(Unity 대시보드). 없으면 "아니요" |

## 앱 콘텐츠 (App content) 체크리스트

| 항목 | 답 | 이유·메모 |
|---|---|---|
| 개인정보처리방침 (Privacy policy) | 공개 URL | `Privacy_Policy.md`를 HTML로 게시한다. PDF나 지역 제한 URL은 안 된다. **게임 안에도 링크나 본문이 있어야 한다** |
| 앱 액세스 권한 (App access) | 모든 기능을 특별한 액세스 권한 없이 사용 가능 (All functionality is available without special access) | 로그인과 계정이 없다 |
| 광고 (Ads) | 아니요, 앱에 광고가 없습니다 | 광고 SDK가 없다 |
| 콘텐츠 등급 (Content rating) | IARC 설문 | 답안은 `Store_Listing.md` 3장 |
| 타겟층 및 콘텐츠 (Target audience and content) | **13–15세, 16–17세, 18세 이상** | 아래 설명 |
| 뉴스 앱 (News apps) | 아니요 | |
| 데이터 보안 (Data safety) | A안 | |
| 정부 앱 (Government apps) | 아니요 | |
| 금융 기능 (Financial features) | 앱에서 금융 기능을 제공하지 않음 | |
| 건강 앱 (Health apps) | 건강 기능 없음 | |
| 광고 ID (Advertising ID) | 아니요 | `AD_ID` 권한이 없다. SDK를 추가하면 다시 확인한다 |
| 권한 신고 (Permissions declaration) | 해당 없음 | 민감 권한(위치, SMS, 저장소 전체 접근 등)이 없다 |

### 타겟층을 13세 이상으로 권장하는 이유

- 게임 구조가 청소년·성인을 전제한다. 기억력 예산 안의 능력 빌드, 확률 강화와 실패 시 내구도 감소, 14종 스탯, 300레벨 성장, 긴 한국어·영어 설명문이 그렇다. 아동을 위해 설계하지 않았다.
- 13세 미만을 고르면 가족 정책(Families Policy)이 적용된다. 아동 대상 콘텐츠 기준, 가족용 인증 SDK만 사용하는 조건, 추가 심사를 받게 된다. 유료 RPG에는 얻는 것이 없다.
- 콘솔이 "의도치 않게 아동의 관심을 끌 수 있나요?"라고 물으면 사실대로 답한다. 귀여운 픽셀 마물이라 "예"로 볼 여지가 있다. 이 경우에도 광고와 데이터 수집이 없어서 추가로 맞출 요건이 거의 없다.

## 제출 전 재확인

- [ ] 최종 AAB의 권한 목록: `aapt2 dump permissions <apk>` 또는 Play Console의 App bundle explorer. `INTERNET` 외에 새 권한이 생기면 이 문서를 고친다.
- [ ] Diagnostics를 끈 빌드를 실기기로 1회 실행하고 네트워크 모니터링으로 전송이 0건인지 확인한다. Android는 PCAPdroid 같은 도구, Windows는 리소스 모니터의 네트워크 탭을 쓴다.
- [ ] SDK(광고, 결제, 분석, Steamworks 등)를 추가하면 이 양식을 다시 작성한다. Google은 SDK가 하는 수집·공유까지 앱 책임으로 본다.

출처: [Data safety 도움말](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en) · [사용자 데이터 정책(개인정보처리방침 게시 위치)](https://support.google.com/googleplay/android-developer/answer/10144311?hl=en) · [Unity Diagnostics 설정](https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/editor-settings-reference/comp-manager-group/diagnostics-settings) · [Unity Developer Data 수집 설정](https://docs.unity.com/en-us/cloud/developer-data-framework/data-collection-settings) · [Disable HW Statistics 포럼 답변](https://discussions.unity.com/t/does-disable-hw-statistics-option-also-disable-analytics/731757/22)
