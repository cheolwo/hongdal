# 주문자 수입 여정 다이어그램 활용 계획

> **문서 책임:** 이 문서는 [수입 공동구매 의향과 같이 수입 원장](ImportGroupPurchaseIntent.md)의 업무·원장 경계를 바꾸지 않고, 기존 원장을 주문자 화면에 투영하는 구현 제안과 검증 순서만 다룬다.

## 제안 요약

기존 `group-import` 다이어그램을 주문자에게 그대로 노출하는 대신, 하나의 원장·하나의 안정적인 node key를 다음 세 화면에 서로 다른 깊이로 투영한다.

1. **주문 상세의 수입 여정 요약**: 지금 어디까지 왔는지 한눈에 확인
2. **수입 여정 상세**: 단계별 책임, 근거, 비용·시간 영향과 다음 행동 확인
3. **전문 다이어그램**: 원장 관계, 업무 node와 인계를 전체 구조로 탐색

화면 표시명은 `같이 주문 수입 여정`처럼 주문자가 이해하기 쉬운 용어를 사용하되, 저장 contract와 호환 key인 `group-import`는 유지한다. 이 화면은 수입 승인이나 통관 완료를 추정하지 않고 서버에 기록된 상태만 설명하는 읽기 중심 화면으로 시작한다.

## 제안 배경

현재 저장소에는 수입 과정을 설명하고 상태를 연결할 수 있는 자산이 이미 있다.

- `CommunityDiagramWorkbenchScreen`의 `group-import` 원장에는 다음 7개 node가 정의되어 있다.
  - `group-source-purchase`: 원천 공동구매
  - `group-import-decision`: 수입 결정
  - `group-overseas-shipment`: 해외 선적
  - `group-customs-release`: 통관/반출
  - `group-third-party-inbound`: 3PL 입고
  - `group-household-distribution`: 세대 분배
  - `group-settlement-receipt`: 정산/수령
- 다이어그램 Route는 원장 template, 선택 node, 확대 비율, filter와 돌아갈 화면을 query 문맥으로 보존한다.
- `DiagramLedgerChangedResponse`와 협업 Hub는 같은 원장 ID의 변경을 다른 화면이 다시 읽을 수 있는 연결점을 제공한다.
- 주문자 API에는 같이 수입 준비 자료, HS 코드·수출입 단가, Incoterms 도움말, 해외 선적 조회가 나뉘어 존재한다.
- `/community/group-import` 화면은 현재 HS 코드 기반 후보 구성과 비구속 참여에 집중되어 있다.

따라서 새로운 독립 다이어그램을 하나 더 만드는 것보다, 기존 업무 다이어그램을 주문자 관점의 진행 여정으로 투영하는 것이 적합하다.

## 현재 구조에서 보완할 문제

### 업무 구조와 사용자 설명의 간격

기존 node 이름은 업무 구조를 표현하기에는 적합하지만, 수입 경험이 없는 주문자가 자신의 상품 상태를 이해하기에는 설명이 부족하다. 예를 들어 `통관/반출`만 표시하면 현재 세관 확인 중인지, 서류 보완 대기인지, 국내 반출이 가능한지 구분하기 어렵다.

### 정적 절차와 실제 주문 상태의 혼동

현재 다이어그램 palette는 가능한 업무 순서를 보여 준다. 주문자는 이 절차도와 자신이 참여한 주문의 실제 진행 상태를 혼동할 수 있다. 따라서 다음 둘을 명확히 구분해야 한다.

- `전체 과정 보기`: 일반적인 수입 절차
- `내 주문 진행 보기`: 특정 `groupImportLedgerId`에 기록된 실제 상태

### 정보가 여러 API와 화면에 분산

공급자·견적·Incoterms·HS 분류·해외 선적·통관·3PL 입고 정보가 서로 다른 contract에 존재한다. 주문자 화면에서는 이를 한 문서로 합쳐 저장하지 않고, 원장 ID를 기준으로 조회용 projection에서 조립해야 한다.

### 전문 상태를 성공으로 오인할 위험

견적 확보, 포워더 인계, 선적서류 등록과 통관 완료는 서로 다른 상태다. 자료가 있다는 이유만으로 뒤 단계가 완료된 것처럼 표시하면 안 된다. 각 node에는 상태의 근거와 기준 시각을 함께 보여 줘야 한다.

## 목표 사용자 경험

주문자는 수입 관련 상품을 보거나 같이 주문에 참여할 때 다음 질문에 답을 얻는다.

1. 이 상품은 왜 수입 과정이 필요한가?
2. 지금 전체 과정 중 어디에 있는가?
3. 현재 누가 무엇을 확인하고 있는가?
4. 가격과 도착 예정일에 영향을 주는 요소는 무엇인가?
5. 기다리면 되는가, 내가 확인하거나 동의할 것이 있는가?
6. 상태의 근거는 무엇이며 언제 확인된 정보인가?
7. 문제가 생기면 어느 단계에서 왜 멈췄는가?

## 화면 구조 제안

### 1. 주문 상세의 수입 여정 요약

주문 상세 또는 같이 주문 상세에서 다음 내용을 작은 여정 카드로 보여 준다.

- 출발국가 → 도착국가
- 현재 단계와 마지막 확인 시각
- 완료 단계 수
- 예상 도착 범위
- 현재 비용 변동 또는 일정 지연의 핵심 사유
- `수입 과정 자세히 보기` 행동

이 카드는 전체 다이어그램을 축소 렌더링하지 않는다. 모바일에서도 읽을 수 있는 7단계 진행 strip 또는 세로 목록을 사용한다.

### 2. 수입 여정 상세

상단에는 전체 7단계를 표시하고, 선택한 단계 아래에는 설명 panel을 연다.

단계 상세에는 다음 항목을 공통으로 둔다.

- 쉬운 단계 설명
- 현재 상태와 상태 근거
- 담당 역할
- 시작·마지막 확인·완료 시각
- 관련 서류 또는 공식 데이터의 종류와 출처
- 가격·일정에 미치는 영향
- 차단 사유와 미확인 항목
- 주문자가 할 수 있는 다음 행동
- 전문가용 상세 화면 또는 다이어그램으로 이동하는 link

### 3. 전문 다이어그램

기존 `/diagram` 화면을 재사용한다. 수입 여정에서 선택한 node key와 돌아갈 주문 상세 경로를 navigation context로 전달한다.

```text
/diagram
  ?ledgerTemplate=group-import
  &node=group-customs-release
  &from=/orders/{orderLedgerId}/import-journey
```

전문 다이어그램은 원장 관계와 업무 인계를 보여 주는 화면이며 주문자의 기본 진입 화면으로 사용하지 않는다.

## 기존 node의 주문자 표시안

| 안정적인 node key | 현재 업무명 | 주문자 표시명 | 주문자에게 보여 줄 핵심 질문 |
| --- | --- | --- | --- |
| `group-source-purchase` | 원천 공동구매 | 같이 주문 수요가 모였어요 | 어떤 상품·수량·배송권이 수입 검토의 근거가 되었나? |
| `group-import-decision` | 수입 결정 | 공급·수입 조건을 확인해요 | 공급자, 견적, MOQ, Incoterms, HS 후보와 예상 총비용이 준비되었나? |
| `group-overseas-shipment` | 해외 선적 | 해외에서 출발을 준비하거나 이동 중이에요 | 발주·포장·Invoice·Packing List·BL/AWB와 출발 상태가 확인되었나? |
| `group-customs-release` | 통관/반출 | 세관과 국내 반출 상태를 확인해요 | 신고·검사·보완·수리·반출 중 어느 상태이며 근거 시각은 언제인가? |
| `group-third-party-inbound` | 3PL 입고 | 국내 보관 장소에 입고해요 | 어느 창고가 인수하고 수량·온도·파손·검수를 어떻게 확인했나? |
| `group-household-distribution` | 세대 분배 | 배송권 안에서 나누어 배송해요 | 거점 분배, 국내 운송과 개인 수령 일정이 어떻게 정해졌나? |
| `group-settlement-receipt` | 정산/수령 | 수령과 비용 확인을 마쳐요 | 내가 수령했는지, 비용 차이와 남은 문제가 정리되었는가? |

표시명은 번역 resource로 관리한다. 내부 node key, 원장 template key와 Event 식별자는 화면 문구 변경 때문에 바꾸지 않는다.

## 상태 표현 규칙

모든 node는 다음 상태 중 하나로만 표시한다.

| 상태 | 의미 | 표시 원칙 |
| --- | --- | --- |
| `NotStarted` | 앞 단계가 끝나지 않음 | 회색, 예상 정보와 실제 정보를 구분 |
| `Ready` | 시작 조건은 갖췄지만 담당자가 시작하지 않음 | 다음 담당 역할과 필요한 확인을 표시 |
| `InProgress` | 내부 작업 또는 외부 절차가 진행 중 | 마지막 확인 시각과 현재 확인 주체를 표시 |
| `WaitingForHuman` | 동의·검토·업체 회신을 기다림 | 자동 처리로 오해하지 않게 사람 대기임을 표시 |
| `WaitingForExternal` | 선사·항공사·세관·창고 등 외부 상태 대기 | 외부 source와 조회 기준 시각을 표시 |
| `Blocked` | 누락·불일치·기한 만료로 진행할 수 없음 | 차단 사유와 해소 조건을 함께 표시 |
| `Completed` | 서버에 완료 근거가 기록됨 | 완료 시각과 근거 유형을 표시 |
| `Cancelled` | 진행을 중단함 | 중단 사유와 주문·환불 관련 별도 상태 link를 표시 |

`진척도 70%` 같은 임의 계산은 사용하지 않는다. 완료 node 수는 보조 정보로만 쓰고, 현재 node와 차단 사유를 우선한다.

## 정보 층위

### 기본 설명

전문 용어 없이 현재 상황을 한두 문장으로 설명한다.

> 상품이 해외에서 출발했고 선적 문서가 등록되었습니다. 다음 단계는 도착지 세관 확인입니다.

### 도움말

`?` 도움말에서 CIF·FOB·DDP, HS·HTS, BL·AWB, LCL·FCL처럼 해당 node에서 필요한 용어만 설명한다. 기존 Incoterms 도움말 API를 재사용하고 모든 Incoterms를 한 화면에 펼치지 않는다.

### 근거 상세

전문 사용자는 원출처, 문서 식별자, 기준 시각, 통화·단위, 견적 유효기간과 규제 검토 상태를 펼쳐 본다. 사용자별 연락처, 상세주소와 계약 문서는 권한이 없는 응답에 포함하지 않는다.

## 조회 projection 제안

새 원장을 만들지 않고 기존 `group-import` 원장과 관련 조회를 조립하는 `ImportJourneyProjection`을 둔다.

```text
groupImportLedgerId
  → 원천 같이 주문 원장
  → 1.5 준비 block과 평가
  → 공급자·견적·원가·HS/HTS·Incoterms 근거
  → 해외 선적 상태
  → 통관·반출 상태
  → 3PL 입고·국내 운송·수령 원장
  → 주문자 역할에 맞춘 node projection
```

권장 읽기 API:

```http
GET /api/v1/orderer/group-imports/{groupImportLedgerId}/journey
```

응답의 최소 필드는 다음과 같다.

```text
JourneyId
GroupImportLedgerId
OrderLedgerId
OriginCountryCode
DestinationCountryCode
CurrentNodeKey
OverallStatus
LastConfirmedAtUtc
EstimatedArrivalWindow
Nodes[]
  NodeKey
  Status
  PlainLanguageSummary
  ResponsibleRoleLabel
  StartedAtUtc
  LastConfirmedAtUtc
  CompletedAtUtc
  EvidenceSummary[]
  CostAndScheduleEffects[]
  Blockers[]
  NextAction
  DetailRoute
  HelpTopicKeys[]
```

projection은 상태를 확정하지 않는다. 각 source의 확인된 상태를 읽어 주문자 표시 상태로 변환하고, 서로 충돌하면 `Blocked` 또는 `확인 필요`로 보여 준다.

## 기존 API 재사용 지도

| 사용자 질문 | 우선 재사용할 현재 API·contract |
| --- | --- |
| 수입 준비가 어디까지 되었나? | `GET /api/v1/orderer/group-imports/{groupImportLedgerId}/readiness` |
| 어떤 HS 코드와 공식 근거를 참고했나? | `GET /api/v1/customs/hs-codes`, HS 코드별 공공데이터 조회 |
| 통계 단가는 어느 정도인가? | HS·국가·기간 수출입 통계 단가 조회 |
| FOB·CIF·DDP의 차이는 무엇인가? | `GET /api/v1/orderer/trade/incoterms/help` |
| 해외 선적은 어디까지 왔나? | `GET /api/v1/orderer/group-purchase-overseas-shipments/lookup` 계열 |
| 수입 원장이 실제 연결되었나? | 같이 주문 투표의 `group-import-ledger` 조회 |

새 journey API는 위 계약을 대체하지 않는다. 주문자 페이지가 여러 요청을 직접 조립하지 않게 서버의 읽기 UseCase에서 합친다.

## 국가별 차이를 반영하는 방법

한국과 미국에 서로 다른 전체 다이어그램을 복제하지 않는다. 공통 7개 node는 유지하고 node 내부의 하위 checkpoint, 책임 역할, 기관 근거와 문구를 시장별 policy로 바꾼다.

- 한국 도착 여정: 한국 HSK, 수입자·관세사 검토, 세관 신고·검사·반출, 국내 3PL과 배송권 분배 근거
- 미국 도착 여정: HTSUS, importer of record·customs broker, CBP와 상품별 관계 기관 확인, 미국 내 fulfillment·배송 근거
- 식품·축산물·일반 상품의 추가 요건은 상품 분류별 checkpoint로 표시
- 법적 자격과 공식 source는 기존 시장별 조사 문서와 검증된 공공데이터에 연결하고 화면 문구에 고정된 법률 판단을 넣지 않음

국가별 checkpoint가 확인되지 않았을 때는 해당 node를 완료 처리하지 않고 `전문가 확인 필요`로 남긴다.

## Figma 적용 제안

`02 Orderer`에 다음 세 frame을 먼저 둔다.

1. `수입 여정 요약`: 주문 상세 안의 7단계 진행 카드
2. `수입 여정 상세`: node 선택과 쉬운 설명, 근거·비용·일정·다음 행동
3. `전문 다이어그램 연결`: 선택 node를 유지한 전체 다이어그램과 돌아가기

Figma에는 세 가지 sample 상태를 함께 만든다.

- 정상 진행: 해외 선적 완료, 통관 대기
- 사람 확인 대기: 포워더 회신 또는 주문자 추가 동의 대기
- 차단: HS 분류·서류·견적 유효기간 문제

화면은 실제 server contract 필드만 사용한다고 표시하고, 아직 API가 없는 값은 `계획 필드` badge로 구분한다. MAUI 앱은 이번 계획 단계에서 수정하지 않는다.

## 페이지별 수직 구현 순서

### Slice 1. 수입 여정 요약 한 페이지

- 입력: `groupImportLedgerId`, 로그인 주문자
- 서버: 기존 readiness·선적·통관 원장의 읽기 projection
- 화면: 7단계, 현재 단계, 마지막 확인 시각, 예상 도착 범위
- 경계: 읽기 전용, 개인정보 제외, 외부 효과 없음
- 완료 기준: 새로고침 뒤 같은 원장 ID로 같은 상태를 다시 읽음

### Slice 2. node 상세

- 선택 node의 설명·담당 역할·근거·비용·일정 영향·차단 사유
- Incoterms와 HS 도움말 연결
- loading, empty, stale, conflicting evidence, retry 상태
- 완료 기준: 근거가 없는 상태를 완료로 표시하지 않음

### Slice 3. 전문 다이어그램 왕복

- 주문 상세 → 선택 node가 열린 `/diagram`
- `/diagram` → 원래 주문 상세의 같은 scroll·node 문맥으로 복귀
- 완료 기준: 모바일과 Web에서 deep link와 돌아가기가 동일하게 동작

### Slice 4. 여러 역할 화면의 같은 상태

- 관리자: 준비·전문가 인계·오류 해소
- 관세사·물류 역할: 자신에게 허용된 node와 근거만 조회
- 주문자: 쉬운 설명과 본인 행동만 조회
- 완료 기준: 한 상태 전이 성공 뒤 각 화면이 같은 원장을 재조회하고 역할별로 허용된 투영을 표시

### Slice 5. 알림과 완료 환류

- 장시간 대기, 보완 필요, 국내 반출, 배송권 분배 시작과 수령 가능 상태 알림
- 완료 뒤 사용자가 동의한 경우에만 개인정보를 줄인 경험·편익 사례 초안 생성
- 완료 기준: 알림 재처리 멱등성과 공개 동의 분리

## 실행 경계

- 현재 기본 집중 단계 `0.0`에서는 일반적인 수입 절차 안내와 공개 근거만 노출할 수 있다.
- 특정 주문의 `1.5` 준비·선적·통관 상태는 관련 feature가 켜진 환경에서만 노출한다.
- `Simulation`에서는 계약, 결제, 신고, 포워더 자동 선정, 외부 전송, 운송 지시와 창고 변경을 실행하지 않는다.
- 화면의 `다음 행동`은 권한이 있는 기존 화면으로 이동하는 link이며 상태를 우회 변경하는 버튼이 아니다.
- 지리적 가까움이나 주문자 집단 소속만으로 업체 선정·운송 계약·통관 결정을 자동 확정하지 않는다.

## 개인정보와 공개 범위

- 주문자는 자신의 주문과 공개 가능한 집계 상태만 본다.
- 다른 참여자의 이름, 연락처, 상세주소와 결제 정보는 여정 응답에 넣지 않는다.
- 포워더·관세사·창고에 제공된 정보는 전달 범위와 동의 근거가 있을 때만 `전달됨`으로 표시한다.
- 다이어그램 node 권한은 화면에서 숨기는 것뿐 아니라 서버 projection에서 차단한다.
- 실시간 원장 변경 메시지에는 민감 원문을 담지 않고 원장 ID, revision, 변경 node와 기준 시각만 전달한다.

## 검증 기준

### 계약·서버

- node key와 원장 ID가 재시작 뒤에도 안정적이다.
- 동일 Event 재처리가 중복 완료·중복 알림을 만들지 않는다.
- 현재 node 계산이 readiness, 선적, 통관과 후속 원장의 실제 상태보다 앞서지 않는다.
- 오래된 견적·문서·공공데이터는 기준 시각과 함께 경고된다.
- 주문자가 접근할 수 없는 원장과 node는 조회되지 않는다.

### 화면

- 수입 경험이 없는 사용자가 현재 단계와 다음 행동을 한 번의 화면에서 찾을 수 있다.
- 모바일에서 가로 다이어그램을 강제로 축소하지 않고 세로 단계 목록으로 읽을 수 있다.
- loading, empty, stale, blocked, cancelled, error와 retry가 서로 다른 상태로 표현된다.
- 도움말을 닫아도 원래 선택 node와 scroll 문맥이 유지된다.
- 실제 계약·신고·통관·입금 완료가 아닌 상태를 완료처럼 표현하지 않는다.

### 역할 간 일치

- 주문자, 관리자와 전문 역할 화면이 같은 원장 revision을 기준으로 한다.
- 역할별 문구와 행동은 달라도 현재 node와 완료 근거는 모순되지 않는다.
- 상태 전이 성공 뒤 화면이 optimistic 표시만 유지하지 않고 원장을 다시 조회한다.

## 우선순위와 권장 첫 작업

첫 구현은 `수입 여정 요약` 한 페이지로 제한하는 것이 좋다.

1. Figma `02 Orderer`에 기존 7개 node를 사용한 주문자용 요약 frame을 만든다.
2. 현재 `같이수입준비주문자조회응답`과 해외 선적 contract로 표시 가능한 필드를 연결한다.
3. 부족한 값은 새 저장 필드가 아니라 journey 읽기 projection의 계획 필드로 목록화한다.
4. 정상·사람 대기·차단 sample을 검토한 뒤 서버의 읽기 API를 추가한다.
5. 실제 원장 ID 재조회까지 통과한 후 node 상세 페이지로 넘어간다.

이 순서라면 기존 다이어그램 자산을 보존하면서도, 주문자가 수입 전문 용어를 먼저 학습해야만 현재 상태를 이해하는 문제를 줄일 수 있다.

## 관련 기준 문서

- [수입 공동구매 의향과 같이 수입 원장](ImportGroupPurchaseIntent.md)
- [전체 로드맵 조화형 페이지 원칙](WholeRoadmapPagePrinciple.md)
- [미국 구매자 같이 수입 배송 여정](../Changes/2026-07-18-us-buyer-collective-import.md)
- [수입육 전통시장 가공·배송 여정](../Changes/2026-07-18-traditional-market-imported-meat.md)
- [다이어그램 Route와 Screen 책임 분리](../Changes/2026-07-22-diagram-route-screen-srp.md)
- [다이어그램 node navigation adapter](../Changes/2026-07-22-diagram-node-navigation-adapter.md)
- [주문자 Incoterms 그림 도움말](../Changes/2026-07-26-orderer-incoterms-help.md)
- [상품·HS/HTS·수출입 통계 단가 판단](../Changes/2026-07-26-orderer-trade-unit-price.md)
