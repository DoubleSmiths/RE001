# 0017. 팝업 소멸과 Tween 수명 연결

- 상태: 채택
- 날짜: 2026-09-10

## 맥락

능력 목록/상세를 연 직후 닫으면 Image와 RectTransform 소멸 경고가 발생했다. 공통 Fade 완료 콜백도 닫힌 뒤 실행됐다. 제작/물약의 대상 없는 닫기 Sequence·DelayedCall은 이후 열린 다른 팝업을 닫을 수 있었다.

## 결정

UIManager는 팝업을 스택에서 꺼낸 뒤 리소스를 제거하기 전에 StopAnimations를 호출한다. 프리팹 DOTweenAnimation은 DOKill로 tween 참조까지 비우고, 비활성 자식을 포함한 Component/GameObject 대상 Tween을 완료 콜백 없이 종료한다. UI_Base.OnDestroy는 씬 제거 경로의 보조 정리를 맡는다.

팝업이 만든 root Sequence·DelayedCall에는 소유 팝업 target과 GameObject link를 지정한다. 공통 Fade의 target은 Image다. 전체 KillAll이나 Safe Mode 비활성화는 사용하지 않는다.

## 결과

새 연출도 소유 대상을 명시해야 한다. Sequence 안의 Image tween만 DOKill해도 부모 Sequence가 종료되는 것은 아니다. Tween 콜백 도중 종료하면 OnKill이 지연될 수 있으므로 프리팹의 참조 정리도 필요하다(DOTweenVisualManager.OnDisable의 DORewind 경고 실측).

`_tools/verify_popup_lifetime.cs`는 같은 프레임 종료 3회, 정상 닫기, 외부 Tween 유지, 닫힌 창 콜백 억제, CustomOrder/Potion 교체 창 보존을 검사한다. 2026-09-10 컴파일 및 플레이 모드에서 PASS, error/warning 빈 목록 확인.
