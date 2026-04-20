# [슬라이드 7] Level UP — 기획 미완 (보류)

## 공통 규약 (모든 슬라이드 공통 — 반드시 준수)
- **CLAUDE.md 코딩 가이드** 준수: Manager Pattern, Static Property 접근, 오브젝트 풀링, UTF-8 (no BOM).
- **DOTween 애니메이션 규약** (중요):
  - 애니메이션 대상 오브젝트에 **같은 역할의 `DOTweenAnimation` 컴포넌트가 이미 붙어 있으면 그 컴포넌트를 사용**한다.
  - 코드에서 `DOTween.Sequence()`로 새 시퀀스를 짜는 것은 **지양**.
  - 단일 트윈은 prefab의 `DOTweenAnimation`이 우선.
- **사용자 Unity 종료 금지**. 테스트 시 `Application.runInBackground = true`.

## 기획 상태
슬라이드 7은 제목 "Level UP" 한 줄만 있고 **상세 스펙이 작성되지 않음** (이미지도 없음).

## 작업 지시
1. **이 슬라이드는 기획 확정 전까지 구현 금지.**
2. 사용자가 기획 업데이트를 전달하면 그때 본 프롬프트를 재작성한다.
3. 기존 레벨업 로직(`PlayerController.OnPlayerLevelUp` 이벤트, `UI_GameScene`의 `PlayerLevelText` 갱신)은 유지 — 새 Popup/연출 추가 금지.
4. 임시 연출 요청 시 참고 레퍼런스를 먼저 받고 진행.

## 결과 보고
"슬라이드 7은 기획 미완으로 보류합니다" 한 줄 + 파일 변경 없음 확인.
