# 문화교통 0.5 Migration Notes

## 2026-07-24 · 개별주문 단계 신설

- 제품 순서를 `0.0 커뮤니티·공공데이터 → 0.5 개별주문 → 1.0 공동주문 → 1.5 공급·무역 준비`로 변경했습니다.
- `IndividualOrderVersion = "0.5"`와 `SsalddelProductVersion.V0_5`를 추가했습니다.
- 개별 수요·원함 업무 게시판, OrdererApp의 내 원함과 전체 개별주문 원장 화면, 개별주문 관점 API를 `0.5`로 분류했습니다.
- `1.0`의 표시 의미는 공동구매 전반이 아니라 **동의한 개별주문의 공동주문·주문자 집단화**로 좁혔습니다.
- 기존 HTTP `/api/v1/...` 경로, 공개 contract 이름, 원장 stable ID와 저장 데이터는 변경하지 않습니다.
- 기존 `GroupPurchaseDemandWorkflow`는 호환을 위해 유지합니다. 0.5를 독립 배포하기 전 개별 원장 Command와 1.0 집단화 Command의 Feature flag 분리가 필요합니다.
- 이번 단계 등록 자체는 DB schema migration을 요구하지 않습니다.
