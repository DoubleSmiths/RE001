# 개인정보 처리방침 / Privacy Policy — 초안

> **게시 전에 이 인용 블록을 지운다.** 공개 URL(HTML 페이지, PDF 불가)에 게시하고, 그 링크를 Play Console과 게임 안(설정 또는 크레딧 화면)에 모두 넣는다. Google Play 정책이 둘 다 요구한다.
>
> 1. **Unity 진단 데이터: 끔(2026-09-29).** `UnityConnectSettings.asset` 의 `m_EngineDiagnosticsEnabled: 0` 을 커밋했다.
>    (서비스 연결 `m_Enabled: 1` 은 에디터 로그인 때 생긴 로컬 변경이다.) 빌드한 뒤 네트워크 모니터로 `*.unity3d.com` 통신이
>    없는지 한 번 확인하면 본문의 A안(수집 없음)이 사실로 확정된다. 진단을 다시 켜면 **[선택 조항 A]**와 데이터 보안 B안으로 바꾼다.
> 2. 저장 경로의 `<회사명>\<제품명>` 은 현재 임시값 `DoubleSmiths\Minimon Quest` 다. 확정되면 여기와 본문을 같이 바꾼다.
> 3. `[개발자/회사명]`, `[연락처 이메일]`, `[시행일]`을 채운다. 스토어에 표시되는 개발자 이름이 본문에 그대로 나와야 한다. Google Play 요건이다.
> 4. 이 초안은 "유료, 광고 없음, 인앱 결제 없음"을 전제로 한다. 무료+광고나 인앱 결제로 바꾸면 방침을 다시 쓴다. 광고·결제 SDK는 데이터를 수집한다.

---

## 한국어

# 「미니몬 퀘스트」 개인정보 처리방침

- 시행일: [시행일]
- 개발자: [개발자/회사명]
- 문의: [연락처 이메일]

[개발자/회사명](이하 "개발자")은 「미니몬 퀘스트」(이하 "게임") 이용자의 개인정보를 수집하지 않습니다. 이 방침은 게임이 다루는 정보와 그 정보가 저장되는 곳을 설명합니다.

### 1. 수집하는 개인정보

게임은 어떠한 개인정보도 수집하지 않으며 개발자에게 전송하지도 않습니다. 이름, 이메일, 전화번호, 계정, 위치, 연락처, 사진, 광고 식별자, 기기 식별자가 모두 여기에 해당합니다. 회원가입과 로그인이 없고, 광고와 인앱 결제도 없습니다.

### 2. 기기에만 저장되는 정보

이어서 플레이할 수 있도록 다음 정보를 이용자의 기기에만 저장합니다. 이 정보는 개발자에게 전송되지 않으며, 개발자는 이 정보를 볼 수 없습니다.

- 진행 데이터: 레벨과 능력치, 아이템과 장비, 미니몬과 도감 기록, 퀘스트 진행, 게임 설정, 방치 보상을 계산하기 위한 마지막 접속 시각
- 언어 설정: 처음 실행할 때 기기의 언어를 읽어 기본 언어를 정합니다

저장 위치는 다음과 같습니다.

- Android: 앱 전용 저장 공간(`Android/data/<패키지명>/files/SaveData.json`)과 앱 설정 저장소
- Windows: `%USERPROFILE%\AppData\LocalLow\<회사명>\<제품명>\SaveData.json`과 백업 파일 `SaveData.json.bak`. 언어 설정은 Windows 레지스트리 `HKEY_CURRENT_USER\Software\<회사명>\<제품명>`에 저장합니다.

### 3. 네트워크 통신

게임은 인터넷 연결 없이 동작하며, 게임 기능을 위해 서버와 통신하지 않습니다. Android 앱 정보에 인터넷 권한이 표시될 수 있습니다. 이 권한은 게임 엔진(Unity)이 기본으로 포함한 것이며, 게임은 이 권한으로 데이터를 전송하지 않습니다.

**[선택 조항 A — Unity 진단 데이터를 켠 경우에만 넣는다]** 게임 엔진 제공사 Unity Technologies는 게임의 안정성을 개선하기 위해 오류가 발생했을 때 충돌 로그, 기기와 운영체제 정보 같은 진단 데이터를 수집할 수 있습니다. 이 데이터는 Unity의 개인정보 처리방침(https://unity.com/legal/privacy-policy)에 따라 처리됩니다.

### 4. 제3자 제공 및 처리 위탁

개발자는 개인정보를 수집하지 않으므로 제3자에게 제공하지 않고, 처리를 위탁하지도 않습니다.

### 5. 플랫폼 사업자

게임을 구매하고 내려받아 실행하는 과정에서 Google(Google Play)과 Valve(Steam)가 결제 정보, 구매 기록, 플레이 시간, 충돌 보고 등을 처리할 수 있습니다. 이 처리는 각 플랫폼의 개인정보 처리방침을 따르며 개발자는 관여하지 않습니다.

- Google 개인정보처리방침: https://policies.google.com/privacy
- Steam 개인정보 처리방침: https://store.steampowered.com/privacy_agreement/

이용자가 Android 백업이나 Steam 클라우드를 켜 두었다면 저장 파일이 이용자 본인의 Google 계정이나 Steam 계정에 백업될 수 있습니다. 이 백업도 각 플랫폼이 처리하며 개발자는 접근할 수 없습니다.

### 6. 아동의 개인정보

게임은 누구의 개인정보도 수집하지 않습니다. 만 14세 미만 아동의 개인정보도 마찬가지로 수집하지 않습니다.

### 7. 정보의 보관과 삭제

기기에 저장된 정보는 이용자가 지우기 전까지 기기에 남습니다. 다음 방법으로 언제든 지울 수 있습니다.

- Android: 게임을 삭제하거나 [설정 > 애플리케이션 > 게임 > 저장공간 > 데이터 삭제]
- Windows: 위 저장 폴더를 삭제합니다. Steam 클라우드를 쓰는 경우 Steam에 남은 클라우드 저장도 정리합니다.

### 8. 방침의 변경

이 방침이 바뀌면 새 시행일과 함께 이 페이지에 게시합니다.

### 9. 문의

개인정보 관련 문의는 [연락처 이메일]로 보내 주세요.

---

## English

# Minimon Quest Privacy Policy

- Effective date: [Effective Date]
- Developer: [Developer/Company Name]
- Contact: [Contact Email]

[Developer/Company Name] ("we") does not collect personal information from players of Minimon Quest (the "Game"). This policy explains what information the Game handles and where it is stored.

### 1. Information we collect

The Game does not collect personal information of any kind, and it does not send any to us. This includes your name, email address, phone number, account details, location, contacts, photos, advertising ID and device identifiers. The Game has no sign-up or login, no ads and no in-app purchases.

### 2. Information stored only on your device

So that you can continue where you left off, the Game stores the following only on your device. It is not sent to us, and we cannot see it.

- Progress data: level and stats, items and equipment, Minimons and bestiary records, quest progress, game settings, and the time you last played (used to calculate idle rewards)
- Language setting: on first launch, the Game reads your device language to choose a default language

Storage locations:

- Android: app-specific storage (`Android/data/<package name>/files/SaveData.json`) and app preferences
- Windows: `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\SaveData.json` and a backup file, `SaveData.json.bak`. The language setting is stored in the Windows registry at `HKEY_CURRENT_USER\Software\<Company>\<Product>`.

### 3. Network communication

The Game works without an internet connection and does not contact any server for its features. Android may list an internet permission for the app. The game engine (Unity) includes this permission by default, and the Game does not use it to transmit data.

**[Optional clause A — include only if Unity diagnostics is enabled]** Unity Technologies, the provider of the game engine, may collect diagnostic data when an error occurs, such as crash logs and device and operating system information, to improve stability. This data is handled under Unity's Privacy Policy (https://unity.com/legal/privacy-policy).

### 4. Sharing and processors

Because we collect no personal information, we do not share it with third parties or engage anyone to process it.

### 5. Platform providers

When you buy, download and run the Game, Google (Google Play) and Valve (Steam) may process information such as payment details, purchase history, playtime and crash reports. That processing is governed by their own privacy policies, and we are not involved in it.

- Google Privacy Policy: https://policies.google.com/privacy
- Steam Privacy Policy: https://store.steampowered.com/privacy_agreement/

If you have enabled Android backup or Steam Cloud, your save file may be backed up to your own Google or Steam account. The platform handles those backups, and we cannot access them.

### 6. Children

The Game collects no personal information from anyone, including children.

### 7. Retention and deletion

Information stored on your device stays there until you delete it. You can delete it at any time:

- Android: uninstall the Game, or go to Settings > Apps > the Game > Storage > Clear data
- Windows: delete the save folder listed above. If you use Steam Cloud, also remove the cloud save through Steam.

### 8. Changes to this policy

If this policy changes, we will post the new version on this page with a new effective date.

### 9. Contact

For privacy questions, contact [Contact Email].
