# 마을 화면 교체 연출 수명 검증

Unity 6000.3.10f1의 격리 마을 세션에서 `EnterAreaWithFade(Village1)`로 화면을 교체하자 이전 그룹의 버튼 등장·알림 콜백과 장소명 이동이 파괴된 Button/RectTransform/GameObject를 참조했다.

`UI_Village1BtnGroup`의 Sequence·DelayedCall을 해당 그룹에 연결하고 누락된 `base.OnDestroy()` 호출을 복원했다. `UI_AreaBtnGroup`의 장소명·해금 Sequence도 그룹 수명에 연결했다. 해금 연출의 정상 완료와 취소는 입력 블로커·EventSystem을 한 번만 복원한다. 완료 콜백이 다음 해금을 시작해도 이전 Tween의 자동 종료가 새 입력 잠금을 풀지 않는다.

검증: `_tools/verify_village_tween_lifetime.cs`.

- 수정 전 실제 그룹 교체에서 파괴된 RectTransform 접근으로 RED.
- 수정 후 컴파일 오류 0.
- 버튼 등장 중·지연 해금 대기 중·해금 연출 중 실제 마을 교체 3회 PASS. 이전 콜백 실행 없음, 소유 Tween 종료, 입력·블로커 복원, 무관한 Tween 생존.
- 새 그룹의 6개 버튼 정상 등장 PASS.
- 정상 해금 콜백 1회·버튼 활성·입력 복원 PASS.
- 완료 콜백에서 두 번째 해금을 시작하는 연쇄 실행: 첫 autoKill 뒤 입력 잠금 유지, 최종 콜백 2회·입력 복원·블로커 제거 PASS.
- 최종 콘솔 Error/Warning 모두 0.

검증은 저장 경로를 비활성화하고 GameData 복사본을 사용했다. 실제 Android 기기 검증은 아니다.
