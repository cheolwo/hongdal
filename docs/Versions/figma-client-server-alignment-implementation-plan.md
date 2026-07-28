# Figma·클라이언트·서버 코드 정렬 실행 계획

- 작성일: 2026-07-28
- 상태: 진행 중 · `04 Driver` 실행 프로필 경계, `05 Warehouse` 1C,
  `07 Restaurant` 복구 핵심 구현
- Figma 기준:
  [ssalddle](https://www.figma.com/design/0KhuQLc1MleUBIQnARC21Z/ssalddle?node-id=0-1)
- 선행 제안:
  [서버·클라이언트 변화의 Figma 수렴 제안](figma-code-convergence-proposal.md)
- 반영된 화면:
  [Figma 서버·클라이언트 수렴](../Changes/2026-07-28-figma-code-convergence.md)

## 목적

Figma에 반영된 역할별 여정과 실제 클라이언트 route, 서버 contract·권한·상태
전이를 같은 의미로 정렬한다. 화면을 한꺼번에 다시 만드는 방식이 아니라
**원장 또는 업무 node 하나씩 저장·상태 전이·재조회·화면·검증을 세로로
완성**한다.

이 계획은 다음 문제를 피하는 것을 우선한다.

1. Figma에는 완료 화면이 있지만 서버에는 영속 상태가 없는 경우
2. 클라이언트가 서버 성공 전 로컬 상태를 완료로 바꾸는 경우
3. 같은 상태를 앱마다 다른 문자열과 시간 기준으로 판정하는 경우
4. 다른 역할 앱이 같은 주문번호·운송의뢰 ID를 다시 조회하지 못하는 경우
5. 코드에는 기능이 있지만 역할 색상·route·오류 상태가 Figma와 다른 경우

## 정렬의 단일 기준

| 구분 | 기준 책임 |
| --- | --- |
| Figma | 화면 계층, 사용자 행동, 버튼명, 역할별 색상, loading·empty·error·disabled 상태 |
| 공유 contract | route 상수, stable ID, DTO 필드, 상태 코드, 서버 시각과 재조회 키 |
| 서버 | 인증·소유권, 현재 상태 검증, 영속 전이, 멱등성, Event·Outbox·감사 |
| 클라이언트 | 서버 상태 표시, Command 요청, 성공 뒤 같은 stable ID 재조회, 재연결·재시도 |
| 검증 기록 | build·test, 실제 앱 렌더, Figma 캡처를 서로 다른 증거로 보존 |

서버 상태가 Figma 표현과 다르면 서버의 권한·영속 전이를 먼저 바로잡는다.
반대로 색상·정보 위계만 다르면 서버를 변경하지 않고 client token과 component만
수정한다.

## 현재 기준선과 차이

| 영역 | Figma 기준 | 현재 코드 | 남은 차이 |
| --- | --- | --- | --- |
| `02 Orderer` | `02M.01~07` 같이 수입 통관·3PL | 7개 route와 신규 물류 검토 화면·DTO·필터가 현재 작업 트리에 있음 | 미커밋 변경을 먼저 안정화하고 실제 API 오류·빈 후보·재조회·렌더 검증 필요 |
| `03 Shipper` | `03P1.01~07` 계약 검토부터 쌍방 서명 | 인증된 비용 미리보기 API와 화면만 있음 | 초안 저장, 문서 버전, 당사자별 서명, 활성화 상태가 영속되지 않음 |
| `04 Driver` | `04P1.01~06` 만료·재연결·세션 복구 | 서버 만료시각, 1초 countdown, 수락 차단, SignalR 자동 재연결, 30초 조회, 토큰 갱신이 있으며 FDriver 운행 상태·시작·종료·위치는 음식 배달 feature 전용 route를 사용함 | 재연결 완료 즉시 원장 재조회와 연결 상태의 typed contract·통합 검증 필요 |
| `05 Warehouse` | `05P1.01~06` 하차지 검증부터 기사 인계 | 출고예정 기반 실제 주소·일정·차량 조건 저장, 멱등 운송의뢰, 기사 수락·등록 차량·할당수량·예약재고 검증, 멱등 출고 완료와 같은 원장 재조회까지 연결 | 주소 원천·확인 주체와 실제 인증 화면 렌더, Driver·Shipper 교차 재조회 E2E 필요 |
| `07 Restaurant` | `07P1.01~06` 재시작·복구·소유권·업무 재개 | 미처리 의미·최근 변경시각·paging, `OrderNo` upsert, typed 복구 출처, 재연결 즉시 조회와 30초 오류 복구를 연결하고 별도 E2E DB에서 앱 종료 중 주문 복구부터 기사 전달 완료까지 실제 계정으로 검증함 | 401·403·SignalR 강제 단절과 재연결을 포함한 실패 경로 통합 검증 필요 |

## 현재 남은 우선순위

1. `P1 · 04 Driver`: SignalR 재연결 완료 즉시 원장을 다시 읽고 연결 상태를
   문자열이 아닌 typed contract로 통일한다.
2. `P1 · 05 Warehouse`: 실제 인증 계정으로 하차지 확인부터 기사 인계까지
   화면을 렌더하고 Driver·Shipper가 같은 `의뢰Id`를 읽는 E2E를 남긴다.
3. `P1 · 07 Restaurant`: 401, 403, SignalR 강제 단절, 재연결 중 주문 유입을
   포함한 실패 경로 통합 검증을 추가한다.
4. `P1 · 03 Shipper`: 물류대행 계약 초안·문서 버전·당사자별 서명·Simulation
   활성 후보 상태를 영속화한다.
5. `P2 · 04 Cargo Driver`: 화물 운행 시작 뒤 Opinet·추천 전송 실패가 이미
   저장된 운행 상태를 400으로 보이지 않도록 추천 효과를 Outbox 또는 명시적
   부분 성공 상태로 분리한다.
| `08 Seller` | 독립 빨간색 판매자 앱 | 독립 앱과 핵심 route·서버 API·30초 주문 조회가 있음 | 실제 client primary가 청록색이므로 Figma 루비 계열과 불일치, 화면별 오류·빈 상태 렌더 재검증 필요 |
| `09 Admin Mobile` | 자주색 모바일 관리자와 원장 추적 | 로그인·토큰 복구, 운영 개요, 운송·기사, 같이 수입, 30초 조회가 있음 | 전역 자주색 token, 주문번호 기반 음식→배차→운송 추적 상세, 오프라인·권한 복구 화면이 부족 |

### 이미 재사용할 서버 기반

새 Controller를 먼저 만들지 않고 다음 기존 경계를 우선 사용한다.

- 물류대행 비용 검토:
  `api/v1/logistics-service-contracts/cost-preview`
- 창고 출고예정 검토:
  `api/v1/warehouse-operations/outbound-plan-reviews`
- 창고 운송 생성:
  `창고작업Controller.재위탁운송생성`과
  `창고작업UseCase.재위탁운송생성Async`
- 기사 추천 만료:
  `추천만료시각`, `배차응답가능정책`, 배차대기 원장 전환 서비스
- 음식점 복구:
  `api/v1/food-orders/restaurant/inbox`,
  `RestaurantOrderHub`, `음식점음식주문조회UseCase`
- 판매자·관리자:
  현재 `SellerApp`, `SsalddelAdminApp`의 인증 client와 기존 read API

## 공통 정렬 규약

### stable ID

화면 이동과 앱 간 재조회에는 다음 식별자를 유지한다.

| 업무 | 기준 식별자 |
| --- | --- |
| 같이 수입 준비 | `GroupImportLedgerId` |
| 물류대행 계약 | `ContractId`와 변경 불가능한 `DocumentVersion` |
| 출고예정 | `OutboundPlanId` |
| 운송의뢰·배차·운송 실행 | `TransportRequestId` 또는 현재 호환 `의뢰Id` |
| 음식 주문 | `OrderNo` |
| 기사 추천 | `RecommendationId`와 현행 `추천만료시각`의 UTC 의미 |

화면 제목이나 사용자 표시명은 stable ID를 대신하지 않는다. 동일 업무에
`DraftId`, `ContractNumber`, `의뢰Id`가 함께 필요하면 contract에 각 역할을
명시하고 임의 변환하지 않는다.

### 공통 상태 표현

각 클라이언트는 최소한 다음 상태를 같은 의미로 제공한다.

- `Loading`: 초기 필수 원장 조회 중
- `Empty`: 서버 조회 결과 없음, sample fallback 금지
- `Ready`: 현재 사용자가 다음 Command를 수행할 수 있음
- `Blocked`: 누락 정보·권한·선행 상태 때문에 서버가 차단
- `Submitting`: Command 처리 중, 중복 입력 차단
- `Refreshing`: Command 성공 또는 실시간 Event 뒤 같은 원장 재조회 중
- `Offline`: 서버 연결 실패, 마지막 정상 조회 시각 표시
- `Unauthorized`: 토큰 갱신 실패, 업무 버튼 잠금과 로그인 복구
- `Expired`: 서버 기준시각으로 만료, 수행 버튼 비활성

문구는 앱별로 자연스럽게 다르게 쓸 수 있지만 상태 코드는 공유 contract 또는
typed client state로 유지한다. UI가 문자열 포함 여부로 상태를 판정하지 않는다.

### Command 이후

```text
버튼
  → Controller
  → UseCase/Command
  → 영속 상태·Event/Outbox
  → 성공 응답
  → 같은 stable ID 재조회
  → 화면과 다른 역할 앱이 같은 상태 표시
```

실시간 알림은 재조회를 촉발하는 힌트다. 알림 payload만으로 원장을 완료 상태로
확정하지 않는다.

## 실행 순서

동시에 여러 역할 앱을 수정하지 않는다. 아래 slice 하나를 완료하고 실제 렌더와
관련 test를 남긴 뒤 다음 slice로 이동한다.

### 0단계 · 현재 작업 트리 안정화

현재 주문자 통관·3PL과 물류대행 비용 미리보기 변경이 작업 트리에 함께 있다.
먼저 파일 소유 범위를 나누고 현재 변경을 잃지 않는 기준선을 만든다.

작업:

1. `02 Orderer` 통관·3PL 변경 파일과 `03 Shipper` 비용 미리보기 파일을 분류한다.
2. 각 route·contract·Controller·DI·test를 별도 검증한다.
3. 실패하는 항목은 새 P1 작업 전에 수정하거나 명시적 blocker로 기록한다.
4. 구현 커밋은 사용자가 요청할 때 `Orderer`, `Warehouse contract`,
   `Shipper client`, `docs/Figma` 맥락으로 분리한다.

완료 기준:

- 현재 변경 파일 목록과 테스트 범위가 확정된다.
- 다음 slice에서 관련 없는 변경을 stage하거나 수정할 필요가 없다.

### 1단계 · `05 Warehouse` 실제 하차지와 운송 인계

가장 먼저 닫을 slice다. 서버 주소 차단과 운송 생성 경계가 이미 있으므로 새
Controller보다 기존 workflow를 연결하는 일이 중심이다.

#### 1A · 실제 하차지 차단 상태

- `출고예정검토응답`에 상차지·하차지 준비 여부와 차단 사유 코드를 명시한다.
- 마트 자동 흐름의 `상차주소없음`, `배송목적지없음` 결과를 client가 표시할 수
  있는 contract로 노출한다.
- `주문자:{id}` 같은 임시 문자열이나 다른 주문의 주소를 대체값으로 사용하지
  않는다.
- 주소 원천, 마지막 확인 시각, 확인 주체를 저장하되 화면에는 필요한 최소
  범위만 표시한다.

#### 1B · 로컬 초안을 실제 저장 흐름으로 연결

- 현재 `SsalddelTransportRequestDraftWorkspace`의 로컬 검토를 유지한다.
- 별도 확인 버튼 뒤 기존 `재위탁운송생성Async`에 저장한다.
- 요청 DTO에 `OutboundPlanId`와 idempotency key를 연결해 중복 생성 여부를
  서버가 판정한다.
- 성공 후 반환된 `의뢰Id`로 출고예정과 운송의뢰를 모두 재조회한다.

#### 1C · 기사 수락과 출고 해제

- `GeneralTransportHandoff`에서 `추천 중`, `기사 수락`, `차량 확인`,
  `출고 가능`을 서버 상태로 표시한다.
- 기사 수락 전에는 반출 Command를 잠근다.
- 인계 완료 뒤 Warehouse, Driver, Shipper가 같은 `의뢰Id`를 재조회한다.
- 기사·차량·수량·사진 증빙은 필요한 단계에서만 공개한다.

현재 구현:

- `POST /api/v1/warehouse-operations/outbound-plan-reviews/{id}/handoff-complete`
  Command를 기존 창고 Controller와 출고예정 원장에 연결했다.
- 기사 본인 수락, 활동 중 기사 등록 차량과 요청 차량의 일치, 운송 상품
  할당수량, 예약재고를 서버가 다시 확인한다.
- 성공 뒤 예약재고를 한 번만 출고로 전환하고 같은 `OutboundPlanId`와
  `의뢰Id`를 공용 UI가 다시 조회한다.
- Windows Hybrid 본문은 이번 실제 실행에서 흰 화면으로 남아 신규 확인 카드의
  실제 렌더는 미확인이다. 공용 Razor build와 관련 ViewModel·API 테스트로
  간접 검증했다.

완료 기준:

- 실제 하차지 없이 운송·배차가 생성되지 않는다.
- 저장 성공 뒤 client 로컬 객체가 아니라 서버 원장을 표시한다.
- 같은 `OutboundPlanId`로 재시도해도 운송의뢰가 중복 생성되지 않는다.
- `05P1.01~06`의 버튼·상태가 실제 route와 일치한다.

### 2단계 · `07 Restaurant` 재시작 복구

인증·음식점 소유권·서버 수신함은 이미 있으므로 이를 다시 만들지 않는다.

#### 서버

- 음식점 수신함에 `처리상태`, `updatedAfter`, paging을 추가한다.
- 신규·수락·조리·준비 완료 등 “미처리” 범위를 contract에서 정의한다.
- 다른 음식점 주문은 목록·상세·Hub 그룹 모두 같은 음식점 claim으로 차단한다.
- 재시작 복구 조회가 반복돼도 주문 상태를 바꾸거나 알림을 중복 발행하지 않는다.

#### 클라이언트

- 최초 시작 순서를 `토큰 복구 → 서버 수신함 → SignalR 연결`로 유지한다.
- `Reconnected` 뒤 현재 음식점 그룹 재가입과 서버 수신함 재조회를 즉시
  수행한다.
- 실시간 payload와 서버 목록은 `OrderNo`로 upsert한다.
- 복구 출처를 `실시간`, `서버 재조회`, `재연결 재조회` typed 값으로 기록한다.
- 30초 조회 한 번이 실패해도 monitoring loop가 종료되지 않게 한다.
- 401은 업무 버튼을 잠그고 로그인 화면으로 복구하며, 403은 음식점 소유권
  오류로 구분한다.

완료 기준:

- 앱을 종료한 동안 들어온 주문이 재시작 후 서버에서 복구된다.
- 같은 주문이 실시간과 서버 조회로 동시에 와도 한 항목만 보인다.
- 재연결 직후 30초를 기다리지 않고 최신 상태가 표시된다.
- `07P1.01~06`의 recovery·denied·ready 상태가 실제 앱 렌더로 확인된다.

### 3단계 · `04 Driver` 만료·재연결·세션 복구

현재 구현이 가장 많이 갖춰진 영역이므로 새 기능보다 상태 계약과 검증을
닫는다.

#### 보완

- SignalR 상태 문자열을 `Connecting`, `Connected`, `Reconnecting`,
  `Disconnected`, `Unauthorized` typed 상태로 바꾼다.
- 재연결 성공 Event에서 즉시 workspace를 재조회한다.
- countdown은 서버의 현행 `추천만료시각`을 UTC로 해석하고 기기 기준시각
  보정 정책을 test로 고정한다. 공개 contract 이름은 호환성 검토 없이
  일괄 변경하지 않는다.
- 만료 버튼 비활성뿐 아니라 서버 수락 Command도 동일 정책으로 거절한다.
- 401 토큰 갱신 실패 뒤 monitoring·위치 전송·수락을 모두 중단하고 로그인
  복구 상태를 표시한다.
- 수락·거절·픽업·전달 뒤 현재처럼 workspace를 다시 조회하는 규칙을
  regression test로 고정한다.

완료 기준:

- 연결 단절 중에도 30초 보완 조회가 유지된다.
- 재연결 직후 즉시 재조회가 한 번 수행된다.
- 만료된 추천은 client와 server 양쪽에서 수락할 수 없다.
- 재로그인 뒤 기존 수락 운송은 서버 원장에서 복구되고 임시 선택만 초기화된다.

### 4단계 · `03 Shipper` 물류대행 계약

현재 비용 미리보기는 비영속 검토안이다. 쌍방 서명을 한 번에 구현하지 않고
네 개 slice로 나눈다.

#### 4A · 비용 미리보기 정렬

- 현재 `cost-preview`와 `03P1.01~05`의 서비스 범위·요율·조건·책임 항목을
  1:1로 맞춘다.
- 냉장·냉동·식품 취급·lot 추적·예외 작업을 contract의 명시적 필드로 둔다.
- 계산 결과에 요율 버전, 통화, 세금, 기준시각과 경고를 표시한다.

이 단계에서는 저장·서명·활성화를 수행하지 않는다.

#### 4B · 계약 초안 저장

- 비용 미리보기와 분리된 `계약 초안 저장·조회` Command/Query를 추가한다.
- RDB에는 소유권·당사자·문서 버전·서명·활성 상태를 저장한다.
- Mongo 공동 원장에는 표시용 계약 node를 멱등 투영한다.
- 같은 입력의 재시도는 idempotency key로 중복 초안을 만들지 않는다.

#### 4C · 당사자별 서명

- 화주와 물류대행업체가 자기 계정으로 같은 `DocumentVersion`에 서명한다.
- client가 `PartyId`나 처리 사용자 ID를 임의로 지정하지 않는다.
- 문서가 수정되면 이전 서명은 새 버전에 승계하지 않는다.
- 취소·거절·서명 만료와 감사 기록을 상태 전이에 포함한다.

#### 4D · 활성화 경계

- 두 필수 당사자의 같은 버전 서명과 운영 요건이 모두 있어야 `Active` 후보가
  된다.
- 기본 `Simulation`에서는 활성 판정과 작업 준비 상태만 저장한다.
- `Operational` 외부 효과는 허가·제휴·정산·보험·보관 책임 검증 전까지
  비활성으로 유지한다.
- 계약 활성 Event가 입고·보관·정산을 자동 실행하지 않고 후속 업무를
  시작할 자격만 제공한다.

완료 기준:

- `03P1.06`은 저장된 초안, `03P1.07`은 실제 서명 상태를 표시한다.
- 양측 서명 전 `CanActivate=false`이며 업무 실행 버튼이 잠긴다.
- 다른 계약 당사자의 문서·서명에 접근할 수 없다.

### 5단계 · `02 Orderer`, `08 Seller`, `09 Admin Mobile` 화면 정렬

#### `02 Orderer`

- 현재 작업 트리의 `02M.01~07` route를 먼저 완성하고 중복 페이지를 만들지
  않는다.
- 통관·3PL 검토 결과는 원장 재조회로 복구할 수 있게 한다.
- 한국 후보가 없으면 미국 공개자료 후보를 한국 운영 업체처럼 표시하지 않는다.
- 구매 목적·식품·과세·상온/냉장/냉동 조건이 인계·동의 route까지 유지되는지
  검증한다.

#### `08 Seller`

- `SellerApp/wwwroot/app.css`의 청록색 primary를 Figma의 토마토·루비 계열
  token으로 교체한다.
- 색상 변경은 서버 contract와 분리해 별도 client commit으로 한다.
- 집단 수요, 한국 수입식품 준비, API 자격증명, 주문의 loading·empty·error와
  30초 조회 상태를 실제 모바일 렌더로 확인한다.
- API key 원문은 client 재표시·로그·캡처에 포함하지 않는다.

#### `09 Admin Mobile`

- 전역 MudBlazor theme과 app CSS를 자주색 역할 token으로 통일한다.
- 기존 `/operations`는 읽기 중심으로 유지한다.
- 주문번호 하나로 음식 주문→배차대기→추천→운송→알림·Outbox 상태를 조회하는
  상세 화면을 추가한다.
- 현재 API 조합으로 과도한 N+1 조회가 생길 때만 서버에 관리자 전용 read
  projection을 추가한다.
- 오프라인, 토큰 만료, 권한 없음과 재시도 상태를 공통 관리자 shell에서
  처리한다.

완료 기준:

- Seller는 빨간색, Admin은 자주색으로 Figma와 실제 client가 일치한다.
- 각 Figma 화면 코드가 실제 route 또는 명시적 상태 component에 매핑된다.
- 없는 서버 기능을 화면에서 작동 가능한 버튼으로 보이지 않는다.

### 6단계 · 교차 앱 폐쇄 루프

개별 앱을 닫은 뒤 마지막으로 실제 다중 앱 흐름을 검증한다.

#### 화물·창고

```text
Warehouse 출고예정
  → 실제 하차지 확인
  → 운송의뢰 생성
  → Driver 추천·수락
  → Warehouse 기사·차량 확인
  → 출고 인계
  → Shipper·Admin 같은 의뢰Id 재조회
```

#### 음식 주문

```text
Orderer 주문
  → Restaurant 서버 수신함·수락
  → 배차대기
  → FDriver 추천·수락·픽업·전달
  → Orderer·Restaurant·Admin 같은 OrderNo 재조회
```

#### 물류대행 계약

```text
Shipper 비용 검토
  → 초안 저장
  → 화주 서명
  → 물류대행업체 서명
  → Simulation 활성 후보
  → Warehouse 후속 입고 준비
```

완료 기준:

- 앱 재시작·토큰 만료·네트워크 단절을 각 흐름에 한 번 이상 포함한다.
- 상태 변경 뒤 모든 참여 앱이 같은 stable ID로 서버를 다시 읽는다.
- Mongo 투영 실패는 Outbox로 재처리되고 RDB 상태와 장기 불일치하지 않는다.

## 변경 단위와 커밋 제안

구현 요청이 내려오면 다음 맥락을 섞지 않는다.

1. `fix(warehouse):` 실제 하차지 차단 contract와 서버 test
2. `feat(warehouse-app):` 운송의뢰 저장·기사 인계 재조회
3. `fix(restaurant):` 수신함 복구·소유권·재연결
4. `fix(fdriver):` 추천 만료·typed 재연결·세션 복구
5. `feat(logistics-contract):` 계약 초안 영속화
6. `feat(logistics-contract):` 당사자 서명과 활성화 경계
7. `style(seller-app):` 루비 역할 token과 실제 렌더
8. `feat(admin-app):` 자주색 shell과 원장 추적 상세
9. `test(e2e):` 음식·화물·계약 다중 앱 폐쇄 루프
10. `docs(figma):` 실제 렌더와 Figma node 대응 갱신

현재 작업 트리가 섞여 있으므로 `git add -A`를 사용하지 않고 각 slice의 정확한
파일만 stage한다. commit과 push는 사용자가 명시적으로 요청할 때만 수행한다.

## 검증 계획

### 각 slice 직후

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File eng/validate-changes.ps1 `
  -Level Fast `
  -Paths <이번 slice 파일들>
```

### 역할 slice 완료 전

- shared contract와 서버 targeted test
- Controller 인증·소유권·상태 전이 test
- Event·Outbox 멱등·재처리 test
- 수정한 client project build
- 소비하는 다른 client 최소 한 곳 build
- 모바일 폭과 desktop 폭 실제 렌더
- loading·empty·error·offline·expired·unauthorized 캡처

### 단계 완료

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File eng/validate-changes.ps1 `
  -Level Task `
  -Paths <단계 전체 파일들>
```

### 다중 앱 E2E

- sample 사용자와 FakePG만 사용한다.
- 실제 개인정보·주소·토큰·API key·문서 원문을 capture하지 않는다.
- 프로세스 재시작 전후 RDB, Mongo 투영, Outbox, 각 client 재조회를 비교한다.
- build 성공, 실제 앱 렌더, Figma 캡처를 서로 대신하는 증거로 사용하지 않는다.

## 배포 게이트

| 기능 | 기본 공개 | Operational 허용 조건 |
| --- | --- | --- |
| 계약 비용 미리보기 | 검토용으로 가능 | 확정 견적·계약으로 표현하지 않음 |
| 물류대행 전자서명 | 기능 플래그 뒤 Simulation | 당사자 인증·감사·문서 보존·법률 검토 |
| 창고 운송의뢰 | Simulation 우선 | 실제 주소·권한·차량·결제·운영 파트너 |
| 기사 추천 | 현재 실행 모드 정책 유지 | 허가·배달권·위치 동의·안전 운영 |
| Admin 조정 Command | 읽기 우선 | 별도 권한·감사·되돌리기·승인 정책 |

Figma의 활성 버튼이 Operational 외부 효과를 자동으로 허용하지 않는다. 실제
배포에서는 `SsalddelExecution:Mode`와 version feature flag를 함께 확인한다.

## 이번 계획에서 하지 않는 일

- 모든 앱을 한 번에 리뉴얼하지 않는다.
- Figma 화면 수를 늘리기 위한 중복 route를 만들지 않는다.
- 새 배차 engine이나 자동 계약 중개를 추가하지 않는다.
- 서버 실패를 sample data로 숨기지 않는다.
- 물류대행 계약 서명을 단순 checkbox나 client local state로 구현하지 않는다.
- 실제 앱 렌더를 확인하지 않고 Figma와 일치한다고 완료 처리하지 않는다.

## 최종 완료 정의

다음 질문에 모두 같은 stable ID와 상태 코드로 답할 수 있을 때 정렬 완료로
판정한다.

1. 이 화면은 어느 route에서 열리는가?
2. 어떤 버튼이 어떤 Controller·Command를 호출하는가?
3. 서버가 어떤 권한과 현재 상태를 확인하는가?
4. 성공 뒤 어느 원장을 다시 조회하는가?
5. 다른 역할 앱은 같은 변경을 어떻게 알게 되는가?
6. 만료·단절·401·403·누락 정보에서는 어떤 화면이 표시되는가?
7. Figma, 실제 client 렌더, server test의 증거가 각각 남아 있는가?

이 기준으로 한 slice씩 완료하면 Figma가 별도 그림으로 남지 않고
클라이언트와 서버가 같은 업무 원장을 설명하는 구조가 된다.
