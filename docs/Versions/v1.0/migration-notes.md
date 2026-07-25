# Ssalddel 1.0 Migration Notes

## 2026-07-24 · 0.5 개별주문 선행 단계 분리

- `1.0`의 표시 의미를 **공동주문·주문자 집단화**로 좁혔습니다.
- 상품·수량·수령 조건을 한 사람이 관리하는 내 원함과 개별주문 원장은 `0.5`로 이동했습니다.
- `1.0` 집단화는 공동 참여에 명시적으로 동의하고 철회되지 않은 0.5 stable 개별 원장만 입력으로 받는 방향을 기준으로 삼습니다.
- 기존 route, contract, Mongo 문서와 `GroupPurchaseDemandWorkflow`는 호환을 위해 유지하며 runtime Feature 분리는 0.5 release gate에서 진행합니다.

## 2026-07-24 · 1.0 전용 Simulation 배포 프로필

- Azure 기본 Compose는 커뮤니티 기반과 저장 없는 연습만 열고
  `GroupPurchaseDemandWorkflow` 및 모든 후속 이행 플래그를 기본 비활성화합니다.
- `compose.orderer-v10.override.yaml`을 추가해 비구속 수요·집단화만 명시적으로
  활성화하고 `1.5` 무역 준비와 운송·창고·판매·배달 기능은 계속 차단합니다.
- 1.0과 1.5 배포 script는 같은 health 확인·rollback 구현을 사용합니다. rollback도
  배포 때 선택한 프로필을 유지합니다.
- DB schema 변경은 없습니다. 배포 전후의 수요 stable ID와 기존 Mongo 원장은
  그대로 유지합니다.
- 구체적인 준비·배포·확인·rollback 절차는
  [1.0 배포 준비 절차](deployment-runbook.md)를 따릅니다.

## 2026-07-22 · 공동구매 우선 재분류

- `1.0`의 의미를 국내 화물·용달에서 공동구매·주문자 집단화로 변경했습니다.
- 공동구매 자동집단화, 수요 투표, 주문자 집단 운영 주체와 국내 생산자 협의 API 메타데이터를 `1.0`으로 이동했습니다.
- 기존 `GroupPurchaseImportWorkflow`, `OrdererGroupOrderV25`와 `ApartmentGroupOrderV25` 설정은 호환 별칭으로 유지하고 신규 설정은 `GroupPurchaseDemandWorkflow`를 사용합니다.
- HTTP `/api/v1/...` 계약 버전과 저장 데이터 식별자는 변경하지 않습니다.
- 이번 재분류 자체는 DB 스키마 마이그레이션을 요구하지 않습니다.

## 2026-07-22 · 수요 모집과 후속 이행 플래그 분리

- `GroupPurchaseDemandWorkflow`는 `1.0` 비구속 수요·자동집단화·모집만 활성화합니다.
- `1.5` 공급·HS·무역 준비는 `CustomsAndTradeDataWorkflow`, `2.0` 운송은 `DomesticTransportWorkflow`, `2.5` 창고·판매 이행은 각 이행 플래그를 사용합니다.
- 수요 플래그를 켜도 판매채널과 인사 참여 플래그가 자동 활성화되지 않습니다.
- 기존 `GroupPurchaseImportWorkflow=true`는 마이그레이션 기간 동안 `GroupPurchaseDemandWorkflow=true`와 같은 의미로만 해석합니다.

## 2026-07-22 · 비구속 수요와 모집 종료 계약

- 신규·변경 수요는 `PUT /api/v1/orderer/group-purchase-auto-groups/demands/{demandSourceKey}`와 `Idempotency-Key`를 사용합니다.
- 철회는 같은 경로의 `DELETE`와 별도 멱등 키를 사용하며, 본인 수요만 철회할 수 있습니다.
- 기존 `POST .../demands`는 호환을 위해 유지하지만 비구속 저장으로만 처리합니다. 결제 상태, 주소, 창고와 후속 주문 원장은 생성하지 않습니다.
- 자동집단은 생성 후 기본 14일간 모집합니다. 기한이 지났고 조건이 미달이면 `RecruitmentClosedTargetNotReached`로 표시하고 신규·변경 수요를 거절합니다.
- 기존 Mongo 문서에 모집 종료 시각이 없으면 생성 시각에 14일을 더해 계산하므로 별도 스키마 마이그레이션은 필요하지 않습니다.
- 공개 응답은 `모집종료시각Utc`, `모집종료여부`, `모집조건충족여부`를 제공하며 개인 주소·결제액·내부 원장은 계속 제외합니다.
