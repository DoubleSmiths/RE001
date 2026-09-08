# 0012. 구형 UI 그래프를 에셋 스캔 밖에 보존

- 상태: 채택
- 날짜: 2026-09-09

## 맥락
Android 빌드는 성공했지만 Visual Scripting LinkerCreator의 ProcessSubGraphs에서
NullReferenceException이 발생해 코드 보존용 link.xml 생성이 실패했다.
Assets/Test/UI.prefab은 런타임 씬·프리팹·Addressables 어디에서도 참조하지 않는 이식 원본이다.
이 파일의 SubgraphUnit 378개는 현재 Assets에 없는 외부 그래프 GUID 12종을 참조하고,
Unity 2022 이후 사라진 NicerOutline 프로퍼티도 참조한다.
LinkerCreator는 런타임 의존성과 무관하게 Assets의 모든 프리팹을 스캔한다.

## 결정
UI.prefab과 기존 meta를 Documents/Legacy로 이동해 바이트 그대로 보존한다.
실제 게임 프리팹의 Visual Scripting 컴포넌트와 패키지는 유지한다.
Library 패키지 수정이나 스트리핑 비활성화로 오류를 숨기지 않는다.

## 결과
구형 원본을 비교할 때 Documents/Legacy를 사용한다.
Assets에 다시 넣으려면 누락된 서브그래프와 NicerOutline 호환성을 먼저 복구해야 한다.
원본 SHA256은 A55C9D923B36981C4BA71151D6737219297C6D6CDC5500DFBDA1DC54926FDC91이다.
