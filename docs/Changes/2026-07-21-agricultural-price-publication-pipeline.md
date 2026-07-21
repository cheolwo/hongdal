# 농수축산물 가격 수집 후 커뮤니티 발행 파이프라인

## 변경 요약

| 변경 | 시각 영향 | 확인 |
| --- | --- | --- |
| KAMIS 일별·USDA NASS 월별 가격 수집 성공 뒤 검증된 가격 요약을 커뮤니티 시스템 글로 저장하는 Quartz 파이프라인 추가 | 화면 없음 — 기존 게시판이 저장된 시스템 글을 일반 자동 작성 표시로 노출 | 서버 빌드, 배치 순서·USDA 초안·중복 방지 단위 테스트로 간접 확인 |

## 실행 순서

1. 기존 공공 API 수집 서비스가 관측값과 수집 실행 이력을 먼저 저장한다.
2. 수집이 성공했고 `PublishCommunityPriceBriefs=true`인 경우에만 원천별 초안을 만든다.
3. KAMIS는 최신 일별 조사일, USDA는 최신 월별 전국 `PRICE RECEIVED` 통합 계열을 사용한다.
4. 게시 Publisher가 `sourceKey + periodKey`로 기존 글을 확인한 뒤 미게시 글만 저장한다.
5. 게시 실패 시 Quartz가 전체 작업을 재실행하며 수집 upsert와 게시 키가 중복을 막는다.

## 운영 경계

- 기본 설정은 수집과 자동 게시 모두 비활성이다.
- 원천 자료가 없거나 기준일·단위·숫자를 검증할 수 없으면 빈 안내 글을 만들지 않는다.
- USDA 값은 생산자 수취가격과 원문 단위로 표시하며 미국 소매가격, 한국 유통가격 또는 판매 견적으로 표현하지 않는다.
- 공식 음식 레시피 후보는 기존 정책대로 검토 대기에 남고 이 파이프라인이 자동 게시하지 않는다.
- 독립 KAMIS·USDA 편집 일정은 같은 기준기간 키를 재확인하므로 수집 직후 게시가 성공한 경우 중복 글을 만들지 않는다.

## 설정 키

- `AgriculturalFisheriesBatch:Enabled`
- `AgriculturalFisheriesBatch:PublishCommunityPriceBriefs`
- `CommunityEditorialBatch:KamisPriceBriefEnabled`
- `CommunityEditorialBatch:UsdaNassPriceBriefEnabled`
- `CommunityEditorialBatch:UsdaNassPriceBriefMaxItems`
