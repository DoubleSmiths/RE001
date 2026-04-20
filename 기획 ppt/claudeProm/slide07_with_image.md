# [슬라이드 7] Level UP — 기획 미완 (보류)

## 레퍼런스
- 기획서: `기획 ppt/기획서v2_04.17.pptx` 슬라이드 7
- 첨부 이미지: **없음** (추출 결과 0건).

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다 (`DOPlay()` / `DOPlayById()` / `DORestart()` 등).
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**. 기획/디자이너가 Inspector로 값을 조정할 수 없고 중복 재생·값 불일치가 생기기 쉬움.
  - 단일 트윈이 필요하면 Inspector 조정 가능한 `DOTweenAnimation`을 prefab에 추가하는 쪽을 우선 고려. 정 필요할 때만 코드 API 사용.
  - 기존 DOTweenAnimation이 `autoPlay=true`로 의도와 다르게 재생되면 prefab에서 `autoPlay=false`로 수정 (DestroyImmediate는 최후 수단).
- **사용자 Unity 종료 금지** (taskkill 금지). 테스트 시 `Application.runInBackground = true`.

## 기획 상태
슬라이드 7의 내용은 제목 `Level UP` 한 줄뿐이며, 상세 스펙(연출, 팝업, 애니메이션, 보상 UI 등)이 **아직 작성되지 않음**.

## 작업 지시
1. **이 슬라이드는 기획이 확정되기 전까지 구현하지 말 것.**
2. 사용자로부터 기획서 업데이트를 전달받거나, 이 슬라이드가 채워졌다는 공지가 있으면 그때 본 프롬프트를 다시 작성한다.
3. 현재 코드베이스의 레벨업 로직(`PlayerController.OnPlayerLevelUp` 이벤트, `UI_GameScene.OnLevelUp` 핸들러) 은 유지 — 새 Popup 추가 금지.
4. 만약 사용자가 임시 연출이라도 원하는 경우, 먼저 **참고 레퍼런스(사운드 / 화면 확대 / 파티클 / LevelUp Popup 구조)** 요청 후 진행.

## 현황 점검 (참고만)
- `Assets/@Scripts/Controllers/PlayerController.cs` — `OnPlayerLevelUp` 이벤트 존재 여부 확인용.
- `Assets/@Scripts/UI/Scene/UI_GameScene.cs:500` 근처 — 레벨업 시 `PlayerLevelText` 갱신 로직.

## 결과 보고
"슬라이드 7은 기획 미완으로 보류합니다" 한 줄 + 건드린 파일 없음 확인.
