# 0005. AetherForge 이미지 생성 API 사용법 (공식 문서와 다름)

- 상태: 채택
- 날짜: 2026-08-14

## 맥락

아이콘·몬스터 원형을 AetherForge API로 생성하기로 했는데,
공식 문서(`api.aetherforgeai.com/getting-started/authentication`)의 예시대로 호출하면 실패한다.
실측으로 계약을 확정해야 했다.

## 결정

실제 계약은 다음과 같다. `_tools/aether_icons.py`가 이 규칙을 구현한다.

```
POST https://api.aetherforgeai.com/public/v1/generate/image
  Authorization: Bearer <key>
  application/x-www-form-urlencoded  ← JSON으로 보내면 422
  prompt, ai_model
  → {"job_id": "..."}

GET https://api.aetherforgeai.com/public/v1/job/{job_id}
  → {"status": "Pending" | "Succeed", "image_urls": [...]}
```

주의점:
- **JSON 본문은 422로 거절된다.** form-encoded만 받는다(문서 예시는 JSON).
- 응답은 이미지가 아니라 `job_id`다. 완료까지 폴링해야 한다.
- Python `urllib` 기본 User-Agent는 **403**으로 막힌다(curl은 통과). 브라우저 UA가 필요하다.
- 결과물은 1024×1024에 **불투명 회색 배경**이다. 프롬프트로 투명 배경을 요청해도 동일하다.
  → 모서리색 기준 배경 제거 → 크롭 → **NEAREST** 축소(픽셀아트라 다른 필터면 뭉개진다)
- 연속 대량 요청은 **429**가 난다. 100장 이상 몰아서 넣지 말고 나눠 실행한다.

키는 `.env.local`의 `AETHER_API_KEY`에서만 읽는다. **커밋 금지**(`.gitignore` 등록됨).
명령줄 인자로 넘기지 않는다 — 셸 히스토리·프로세스 목록에 남는다.

## 결과

- 전 아이콘에 **공통 스타일 프롬프트**를 고정한다. 이게 한 세트로 보이게 만드는 핵심이라
  바꾸면 기존 아이콘과 톤이 어긋난다.
- 같은 프롬프트를 여러 대상에 쓰면 그림이 전부 같아진다. 능력은 StatType/Effect,
  장비는 티어 재질로 형상을 나눈다.
- 스크립트는 이미 있는 파일을 건너뛴다. 429나 중단 후 재실행하면 남은 것만 채운다.
