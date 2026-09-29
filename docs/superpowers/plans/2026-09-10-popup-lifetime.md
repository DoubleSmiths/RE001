# Popup Lifetime Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** 팝업이 닫힐 때 소멸한 Image/RectTransform을 Tween이 참조하지 않는다.
**Architecture:** UIManager의 팝업 제거 경계에서 해당 계층의 Tween만 종료한다. 공통 페이드 Sequence는 소유 Image를 target으로 지정해 같은 종료 경계에서 정리한다. 정상 애니메이션 종료 콜백과 다른 화면의 Tween은 유지한다.
**Tech Stack:** Unity 6000.3.10f1, C#, DOTween, unity-cli.
**Spec:** `Documents/Commercial_Readiness_20260910.md`, 기준 1·5.

## Global Constraints

- 사용자 세이브를 백업·복원한다. 다른 워킹트리 변경을 포함하지 않는다.
- 전체 DOTween.KillAll이나 Safe Mode 비활성화로 경고를 숨기지 않는다.
- 컴파일 후 플레이 모드에서 재현 스크립트를 실행한다.

### Task 1: 팝업 종료 생명주기

**Files:** `Assets/@Scripts/UI/UI_Base.cs`, `Assets/@Scripts/Managers/Core/UIManager.cs`, `_tools/verify_popup_lifetime.cs`.
**Interfaces:** `UI_Base.StopAnimations()`를 추가하고 `UIManager.ClosePopupUI()`가 리소스 제거 전에 호출한다.

- [x] 재현 스크립트 작성: 실제 SkillBook/SkillDetail 팝업을 열고 즉시 닫는다. 공통 FadeInOut 콜백을 추가한 후 닫는다. Application.logMessageReceived로 DOTween 소멸 대상 경고를 수집한다.
- [x] 수정 전 실행: `python _tools/unity_exec.py _tools/verify_popup_lifetime.cs --allow-async`가 경고 때문에 실패함을 확인한다.
- [x] 구현: `foreach (var component in GetComponentsInChildren<Component>(true)) if (component != null) DOTween.Kill(component, false);`를 종료 경계에서 호출한다. 공통 Sequence에 `.SetTarget(image).SetLink(image.gameObject)`를 지정한다.
- [x] `editor refresh --compile` → `console --type error` → 플레이 재실행 → 같은 재현 검증. 외부 Tween이 살아 있고 정상 닫기 이벤트가 한 번 발생하며 재오픈이 가능해야 한다.
- [x] 관련 파일과 검증 결과만 Co-Authored-By 트레일러를 포함해 커밋한다.
