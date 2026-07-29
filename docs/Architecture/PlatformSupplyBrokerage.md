# 플랫폼 공급조건 계약과 개별 발주 중개

## 목적

플랫폼은 공급자와 음식점·살들마트가 반복해서 같은 조건을 협의하지 않도록 공통 공급조건을
계약하고 관리한다. 실제 상품 거래는 공급자와 각 이용조직의 개별 발주로 성립한다.

플랫폼은 이 흐름에서 판매자, 재판매자 또는 매수인이 아니다. 플랫폼은 공급조건 계약,
이용조직의 동의, 개별 발주 전달과 공급자 응답 기록을 중개한다.

```text
공급자 <-> 플랫폼 공급조건 계약
              |
              +-> 음식점 이용등록 -> 음식점 명의 개별 발주 -> 공급자 응답
              |
              +-> 살들마트 이용등록 -> 살들마트 명의 개별 발주 -> 공급자 응답
```

## 당사자와 책임

| 구분 | 역할 |
| --- | --- |
| 공급자 | 개별 발주의 판매자, 공급 가능 수량과 수락·거절 결정 |
| 음식점·살들마트 | 개별 발주의 매수인, 품목·수량·납품지·희망일 선택 |
| 플랫폼 | 공급조건 계약 관리, 조직 접근 검증, 발주 전달, 공급자 응답 증거 기록 |

플랫폼 공급조건 계약은 이용조직의 개별 발주를 자동 생성하지 않는다. 이용조직은 계약 문서
버전과 플랫폼의 중개 역할을 확인한 뒤 계약 이용등록을 하고, 발주마다 다시 명시적으로
확인한다.

## 원장과 상태

| 원장 | 핵심 상태 |
| --- | --- |
| `플랫폼공급조건계약` | `Draft -> Active -> Suspended -> Terminated` |
| `공급계약이용등록` | `Active -> Suspended -> Cancelled` |
| `조직개별공급발주` | `SubmittedToSupplier -> SupplierAccepted / SupplierPartiallyAccepted / SupplierRejected` |
| `조직개별공급발주` 철회 | 공급자 응답 전 `SubmittedToSupplier -> Withdrawn` |

개별 발주는 계약번호, 계약 문서 버전, 공급자, 품목, 단위, 단가와 통화를 스냅샷으로
보존한다. 계약이 나중에 변경되어도 제출된 발주의 조건을 덮어쓰지 않는다.

## 실행 경계

개별 발주 제출과 공급자 응답 기록은 다음 상태를 자동으로 만들거나 변경하지 않는다.

- 플랫폼 매출 또는 판매 주문
- 결제·정산 실행
- 재고 예약·차감
- 음식점 또는 살들마트 입고
- 피킹·포장·배송

공급자가 발주를 수락한 뒤 실제 납품을 진행할 때 음식점 인수 원장 또는 살들마트
도심창고 입고 원장으로 별도 인계한다. 해당 인계는 납품지, 검수 책임, 수락 수량과
공급자 응답 근거를 다시 검증해야 한다.

## API

- 관리자 계약 초안: `POST api/v1/admin/supply-brokerage/agreements`
- 관리자 계약 활성화: `POST api/v1/admin/supply-brokerage/agreements/{agreementId}/activation`
- 공급자 응답 기록: `POST api/v1/admin/supply-brokerage/orders/{orderId}/supplier-response`
- 이용 가능 계약 조회: `GET api/v1/supply-brokerage/agreements`
- 조직 계약 이용등록: `POST api/v1/supply-brokerage/agreements/{agreementId}/participations`
- 조직 발주 조회·제출: `GET|POST api/v1/supply-brokerage/orders`
- 공급자 응답 전 철회: `POST api/v1/supply-brokerage/orders/{orderId}/withdrawal`

이 API는 `WarehouseFulfillmentWorkflow`가 활성화된 검증 profile에서만 노출한다. 음식점과
살들마트 접근 범위는 각각 서버가 발급한 음식점 ID Claim과 살들마트 ID Claim으로
결정하며 요청 본문에서 임의 조직 ID를 받지 않는다.
