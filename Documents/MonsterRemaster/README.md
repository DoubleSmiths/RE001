# 몬스터 스프라이트 교체 기록

2026-09-29. 내장 **image_gen** 편집 도구를 사용했다. CLI/API 대체 경로는 사용하지 않았다.

## 교체 범위

- `Assets/@Resources/Sprite/Monster/Anim`: 30종 × 5동작 × 4프레임 = **600장**.
- `Assets/@Resources/Sprite/Monster/Generated`: 새 idle 기반 원형 아이콘 **30장**.
- `Assets/@Resources/Sprite/Monster/Card`: 새 아키타입을 쓰는 도감 카드 **500장**. 기존 등급 색상 유지.
- 레거시 고블린·초기 미니몬 카드와 CH1 시트는 이번 AI 아키타입 교체 대상에 포함하지 않았다.

프레임은 384×256 RGBA, 하단 중앙 기준이다. 기존 64 PPU, Point 필터, PNG 이름과 GUID,
애니메이터와 Addressables 연결을 유지했다. 각 동작 4프레임의 실제 포즈 차이는 보존하며,
몬스터 하나의 20프레임 전체에 동일 배율을 적용했다.

## 확인

[preview.html](preview.html)을 브라우저에서 열면 실제 Assets의 교체 PNG를 동작별로 재생한다.
일시 정지, 프레임 이동, 재생 속도, 밝은/어두운/체크 배경을 선택할 수 있다.
이 화면은 이미지 확인용이며 Unity 런타임 검증은 아니다.

[verification.json](verification.json)은 PNG 내용·빈 프레임·크기·하단 기준·임포트 메타 설정과
150개 클립의 600개 스프라이트 GUID 연결을 검사한 결과다.

**Unity Editor와 unity-cli 실행 파일을 찾지 못해 실제 임포트·컴파일 콘솔·플레이 모드는 미검증이다.**
Editor를 연 뒤 `editor refresh --compile` → `console --type error` → 플레이 모드에서
30종의 5동작 전환, 스프라이트 로드, 도감 표시를 확인해야 한다.

## 원본과 재현

- `sheets/*.png`: 선택한 투명 배경 통합 원본 30장.
- [manifest.json](manifest.json): 몬스터별 전체 생성 프롬프트와 추가 수정 프롬프트.
- `frame-plan.json`: 검사한 실제 프레임 사각형, 공통 배율, 아이콘 파생 규칙.
- 기존 `Anim/**/_raw_*.png`는 과거 원본이다. 구형 remaster/reslice 도구로 되돌리지 않는다.

저장소 루트에서 실행한다(Python 3 + Pillow, Windows PowerShell + System.Drawing):

```powershell
py -3.12 _tools/inspect_monster_sheets.py
# 실패가 없을 때 미리보기 폴더로 내보내기
powershell -NoProfile -ExecutionPolicy Bypass -File _tools/export_monster_frames.ps1
# 기존 에셋 교체
powershell -NoProfile -ExecutionPolicy Bypass -File _tools/export_monster_frames.ps1 -Apply
py -3.12 _tools/verify_monster_remaster.py
```

검출된 프레임 겹침은 모델에 간격 수정을 요청해 재생성했고, 지네의 방향 혼합도 재생성했다.
검사를 통과하지 못한 시트는 자동으로 덮어쓰지 않는다.
