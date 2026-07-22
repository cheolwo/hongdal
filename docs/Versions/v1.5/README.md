# Ssalddel 1.5

## 목표

`1.5`는 **공급·가격·무역 준비** 단계입니다. `1.0`에서 확인된 수요에 국내외 공급자·관련 기업 근거, 견적, 원가, HS·HTS 후보와 수입 준비 체크포인트를 연결합니다.

## 핵심 결과

- 출처와 확인 시각이 있는 공급자·기업 후보
- 통화, 단위, 최소수량, 유효기간이 명시된 견적
- 상품 원가, 예상 관세·세금·국제 운송비와 국내 이행비를 분리한 예상 총원가
- 재료·상품과 연결된 HS·HTS 후보 및 검토 근거
- 판매자, 수입자, 관세사, 운송 수행자와 플랫폼의 책임 초안
- 실제 계약·신고 전 준비 완료 여부와 미확인 항목

`1.5`의 자료는 의사결정 지원 정보입니다. 전문 품목분류, 수입 적격성, 식품 규제, 신고 대행과 계약 판단을 자동 확정하지 않습니다.

## 구현 흐름

```text
1.0 승인후속대기 수요 집단
  → 공급자·기업 원출처 근거
  → 견적·MOQ·납기·포장·Incoterms 후보
  → 상품·국제운송보험·관세·세금·국내이행 예상비
  → 한국 HSK 또는 미국 HTSUS 후보
  → 국가별 규제 검토와 책임 초안
  → 자격 있는 검토자 인계 가능 상태
```

- 독립 빌드 단위: `Ssalddel.v1.5.slnx`
- 관리자 작업대: `SsalddelAdmin`과 `SsalddelAdminApp`의 `/trade-readiness`
- 관리자 API: `GET|PUT api/v1/admin/orderer/group-purchase-demand-os/groups/{autoGroupId}/trade-readiness`
- 저장 선행 조건: `1.0` 인계 상태 `승인후속대기`, 승인자·승인 시각·인계 ID 존재
- 저장 안전장치: `Idempotency-Key`, 기대 `Revision`, 관리자 정책, `CustomsAndTradeDataWorkflow`
- 추적성: 생성된 결정적 1.5 원장 ID를 원천 1.0 OS의 `대상원장Id`에 멱등 연결
- 실행 경계: 계약 서명, 결제, 신고, 공급자 자동 선정, 운송 지시와 창고 변경은 항상 `false`

주문자 앱의 `/group-purchase/import-review/{ProductId}`는 공식 기업·HSK·HTSUS 후보와 예상 원가를 읽기 전용으로 보여 줍니다. 준비 원장 저장은 관리자 API에서만 수행합니다.

현재 배포 준비도와 남은 운영 게이트는 [2026-07-23 평가서](../deployment-readiness-assessment-2026-07-23.md)를 기준으로 합니다.
