# 0013. Codex 시작 훅의 플랫폼 판정과 경고 출력

- 상태: 채택
- 날짜: 2026-09-09

## 맥락
Ponytail은 정상 모드 상태를 systemMessage로 출력해 UI 경고를 만들었다.
별도로 Superpowers 시작 훅은 PLUGIN_DATA만 있고 CLAUDE_PLUGIN_ROOT가 없는
환경에서 최상위 additionalContext를 출력했다. 이 형식은 Codex SessionStart에서
지원하지 않는다. 기존 검사는 환경변수를 강제로 주입하고 JSON 문법만 검사해 놓쳤다.

## 결정
Ponytail의 Codex 출력에서 상태 경고만 제거하고 모드 지침은 유지한다.
Superpowers는 PLUGIN_DATA로도 Codex를 식별해 hookSpecificOutput 안에 지침을 넣는다.
기존 복구 도구에 두 패치를 포함하고, 검사는 루트 환경변수 없이 실행해
SessionStart의 필드 구조와 지침 보존까지 확인한다.

## 결과
플러그인 캐시 수정 전 백업을 남긴다. 업데이트로 원복되면
`python _tools/repair_codex_plugin_hooks.py --apply` 후
`python _tools/verify_codex_plugin_hooks.py`로 복구·확인한다.
이미 대화에 기록된 경고를 삭제하지 않으며 다음 훅 실행부터 바뀐 출력이 적용된다.
