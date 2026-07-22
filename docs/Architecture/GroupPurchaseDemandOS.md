# 공동구매 수요·모집 OS

## 목적

`공동구매 수요·모집 OS`는 Ssalddel 1.0의 하위 OS다. 사용자가 남긴 비구속 구매 의사를 품목·배송권·수령 조건별로 안전하게 모으고, 모집 진행과 마감을 조율하며, 사람의 확인을 거쳐 `1.5` 공급·무역 준비로 인계한다.

이 OS의 결과는 주문이나 계약이 아니라 **설명 가능한 주문자 집단 후보와 모집 원장**이다.

## 제품 경계

```text
0.0 커뮤니티·음식·재료 탐색
  → 1.0 공동구매 수요·모집 OS
  → 1.5 공동주문 수입 OS / 공급·무역 준비
  → 2.0 국내 화물 운송 OS
  → 2.5 창고·판매 이행 OS
```

`공동구매 수요·모집 OS`와 `공동주문 수입 OS`는 같은 OS가 아니다. 전자는 수요와 모집을 책임지고, 후자는 확인된 모집 결과를 받아 공급·가격·HS·수입 준비와 후속 인계를 책임진다.

## 참여자 입장

| 참여자 | 기대 | 부담과 책임 |
| --- | --- | --- |
| 주문자 | 같은 조건의 구매 희망자를 찾고 현재 모집 진행을 이해한다 | 희망 수량·수령 조건을 직접 확인하고 변경·철회한다 |
| 주문자 집단 대표 | 목표와 모집 조건을 제안하고 다음 단계 필요성을 확인한다 | 목표 충족을 주문·계약 확정으로 오해시키지 않는다 |
| 플랫폼 운영자 | 집단화 정책, 마감과 예외를 일관되게 운영한다 | 자동 가입·가격 차별·묵시적 후속 실행을 막는다 |
| 공개 커뮤니티 참여자 | 개인정보 없이 모집 현황과 근거를 확인한다 | 공개 집계를 개인 정보나 보증으로 해석하지 않는다 |

## 원장 블록

| 블록 | 핵심 내용 | 공개 범위 |
| --- | --- | --- |
| 상품 문맥 | 음식·재료·상품 stable key, 규격, 출처 | 공개 가능 |
| 비구속 수요 | 희망 수량, 참여 상태, 변경·철회 이력 | 집계 공개, 개인값 보호 |
| 배송권 | 국가, 우편번호 권역, 생활권, 수령 지점 후보 | 권역만 공개 |
| 이행 조건 | 보관 온도, 수령 방식, 희망 시간창 | 공개 가능 |
| 모집 정책 | 목표 참여자 수, 목표 수량, 시작·종료 시각 | 공개 가능 |
| 집단화 판단 | 정책 버전, 적용 기준, 배치·보류 이유 | 공개 가능한 설명 제공 |
| 인계 | 사람의 확인 결과, 미확인 공급 조건, 1.5 대상 원장 ID | 권한에 따라 공개 |

사용자 ID, 상세 주소, 연락처와 결제 정보는 집단화 기준이나 공개 응답에 포함하지 않는다.

## 상태와 큐

| 상태 | OS의 처리 |
| --- | --- |
| `CollectingDemand` | 수요 등록·변경·철회 때 집단화 엔진으로 진행도를 다시 계산한다 |
| `ReadyToConfirm` | 목표 충족을 표시하되 자동 확정하지 않고 사람의 검토 큐에 올린다 |
| `RecruitmentClosedTargetNotReached` | 목표 미달 이유와 새 모집 회차·범위·시간창 대안을 표시한다 |
| `Confirmed` | 호환 코드다. 1.0에서는 주문 확정이 아니라 **모집 결과 인계 승인**으로만 해석한다 |

`1.5` 준비 원장이 실제 생성되면 별도 인계 기록과 대상 원장 ID를 남긴다. 이 상태 변경은 OS가 직접 저장하지 않고 승인 Command와 UseCase가 수행한다.

런타임 큐 코드는 다음과 같이 원장에 영속한다.

| 큐 코드 | 진입 조건 | 다음 처리 |
| --- | --- | --- |
| `Recruiting` | 아직 모집 목표를 충족하지 않은 활성 집단 | 다음 Aging 점검 또는 모집 종료 시각 중 빠른 시각에 재점검 |
| `ConfirmationReview` | 목표를 충족한 `ReadyToConfirm` 집단 | 사람의 인계 승인 대기 |
| `RecruitmentClosed` | 마감까지 목표를 충족하지 못함 | 새 모집 회차·범위·수령 방식 검토 |
| `HandoffReady` | 권한 있는 운영자가 인계를 승인함 | 기능 플래그가 허용된 별도 1.5 UseCase의 원장 생성 대기 |

## 스케줄링 정책

| 정책 | 적용 큐 | 규칙 | 기아 방지와 공정성 |
| --- | --- | --- | --- |
| Batching | 비구속수요대기 | 상품, 배송권, 보관 온도, 물류 방식이 같은 수요를 묶는다 | 목표 미달자를 제외하지 않고 보류 이유를 표시한다 |
| EDF | 모집중 | 종료가 가까운 모집을 재계산·안내 대상으로 먼저 올린다 | 마감 임박을 참여자 차별이나 자동 구매 근거로 쓰지 않는다 |
| Aging | 모집중 | 오래 정체된 모집을 운영자 검토 대상으로 올린다 | 더 넓은 모집권, 다른 시간창·수령 방식 같은 대안을 제시한다 |

국적, 언어, 인종, 성별, 가족 형태, 경제력과 같은 속성은 집단 자격·순위·가격 차별 기준으로 사용하지 않는다.

## 엔진과 실행 경계

OS는 `주문자 집단화 엔진`을 호출해 다음 결과를 받는다.

- 결정적 자동집단 ID
- 신규 집단 또는 기존 집단 배치 여부
- 적용한 상품·배송권·보관 온도·물류 방식
- 현재 진행과 수요 반영 뒤 예상 진행
- 목표 미달, 모집 종료, 조건 불일치와 보류 이유

엔진은 후보와 설명만 반환한다. 실제 수요 저장·변경·철회, 소유권 검증, 멱등 처리, 상태 전이와 Event/Outbox 기록은 API, UseCase와 저장소가 수행한다.

## 런타임 조율

`공동구매수요모집OS`는 다음 트리거를 순서 있게 처리한다.

1. `DemandChanged`: 멱등 수요 저장 뒤 Batching 정책을 기록하고 진행 상태와 큐를 다시 계산한다.
2. `DemandWithdrawn`: 본인 소유 비구속 수요 철회 뒤 남은 참여자·수량과 큐를 다시 계산한다.
3. `RecruitmentDeadlineReached`: background worker가 다음 점검 시각이 지난 집단을 모집 종료 시각 순으로 가져와 EDF와 Aging 정책을 적용한다.
4. `ManualReconcile`: 관리자가 특정 집단을 즉시 재조율한다.
5. `HandoffApproved`: 서버 관리자 권한과 요청 멱등 키를 검증한 뒤 `Confirmed`와 `HandoffReady`를 기록한다.

각 조율은 Mongo 자동집단 원장에 canonical OS ID, 정책 버전, 현재 큐, 마지막 트리거, 적용 정책, 다음 점검 시각과 인계 상태를 같은 낙관적 동시성 저장으로 남긴다. 수요 Command가 재시도되면 OS 조율 멱등 키도 함께 재사용해 중복 Event를 만들지 않는다.

background worker는 `GroupPurchaseDemandWorkflow`가 켜진 경우에만 동작한다. 기본 설정은 다음과 같으며 환경 변수의 `GroupPurchaseDemandOS__...` 키로 조정할 수 있다.

| 설정 | 기본값 | 의미 |
| --- | ---: | --- |
| `Enabled` | `true` | OS 마감·Aging background 점검 사용 여부 |
| `ScanIntervalSeconds` | `60` | 점검 주기 |
| `BatchSize` | `100` | 한 번에 조율할 최대 집단 수 |
| `AgingReviewHours` | `24` | 모집중 집단의 장기 정체 재점검 간격 |

## 1.0 지원 배치 작업 카탈로그

OS는 모집 원장만 점검하지 않고 공동구매 판단에 필요한 공개 가격 근거의 수집·검증·게시 순서도 작업 카탈로그로 관리한다. 다만 공공 API adapter와 가격 보관 DB의 소유권은 농수축산 정보 모듈에 그대로 두고, OS는 공유 작업의 등록 상태·선행 작업·스케줄·출처·게시 여부와 실행 경계를 읽어 조율한다.

| 작업 코드 | 역할 | 기본 등록 | 후속 효과 |
| --- | --- | --- | --- |
| `DemandDeadlineAndAgingReview` | 모집 마감·장기 정체 원장 재조율 | hosted worker 등록, 기능 플래그로 실행 통제 | 1.0 모집 상태·검토 큐 갱신 |
| `KamisDailyPriceCollection` | KAMIS 일별 품목·등급·단위 가격 보관 | 비활성 | 검증된 한국 가격 근거 생성 |
| `KamisMonthlyPriceCollection` | KAMIS 최근 완료 월 가격 이력 보강 | 비활성 | 가격 추세 검토 근거 생성 |
| `UsdaMonthlyPriceCollection` | USDA NASS 미국 전국 월별 생산자 수취가격 보관 | 비활성 | 검증된 미국 가격 근거 생성 |
| `CommunityKamisPriceBrief` | 최근 KAMIS 관측값을 `정보·시세` 시스템 글로 게시 | 비활성 | 같은 조사일은 멱등 게시 |
| `CommunityUsdaNassPriceBrief` | 최근 USDA NASS 관측값을 `정보·시세` 시스템 글로 게시 | 비활성 | 같은 기준월은 멱등 게시 |
| `OfficialFoodIngredientCompanyResearch` | 음식·재료 탐색용 공식 기업 근거 갱신 | 비활성 | 검토 후보만 보관, 자동 선정·연락 없음 |

`PublishCommunityPriceBriefs=true`이면 KAMIS 일별 또는 USDA 월별 수집이 성공한 뒤 해당 게시 작업으로 handoff한다. 이 경우 같은 원천의 독립 `CommunityEditorialBatch` Quartz 작업은 등록하지 않아 수집 직후 게시와 조정 일정이 중복 실행되지 않게 한다. 수집 직후 게시를 사용하지 않을 때만 독립 게시 일정을 조정 작업으로 등록한다. 게시 저장은 `sourceKey + periodKey` 시스템 작성자 키를 다시 확인하므로 재시도에도 같은 글을 중복 생성하지 않는다.

공공가격 수집·자동 게시는 source 자격 증명과 운영 검토가 필요한 외부 효과이므로 기본값을 켜지 않는다. 배포 환경에서 다음 조건을 각각 확인한 뒤 명시적으로 활성화한다.

1. `GroupPurchaseDemandWorkflow`와 `GroupPurchaseDemandOS:Enabled`
2. `AgriculturalFisheriesBatch:Enabled`와 대상 수집 작업 플래그
3. KAMIS 인증키·요청자 ID 또는 USDA NASS API key
4. 자동 게시를 사용할 경우 `AgriculturalFisheriesBatch:PublishCommunityPriceBriefs`와 원천별 `CommunityEditorialBatch` 플래그

가격 글은 판매 권고나 공동구매 확정가가 아니다. KAMIS는 조사일·KRW·품목·품종·등급·단위를, USDA NASS는 기준월·USD 원문 단위·생산자 수취가격 단계를 표시한다. 서로 다른 국가·시장 단계·단위를 하나의 가격처럼 합치지 않는다.

## 1.5 인계 조건

다음 조건을 모두 만족할 때만 인계 후보가 된다.

1. 모집 목표가 충족되었거나 운영자가 검토할 충분한 수요 근거가 있다.
2. 참여자가 비구속 수요와 다음 단계의 차이를 이해할 수 있다.
3. 상품·수량·배송권·수령 조건과 미확인 항목이 원장에 남아 있다.
4. 주문자 집단 대표 또는 권한 있는 운영자가 인계를 명시적으로 승인한다.
5. `GroupPurchaseDemandWorkflow`와 후속 기능 플래그, `SsalddelExecution:Mode` 경계를 모두 통과한다.

인계 뒤에도 공급자, 수입자, 관세사, 운송사와 창고를 자동 선정하지 않는다.

승인 시점에는 `ApprovedAwaitingGroupPurchaseImport` 인계 요청만 원장에 기록한다. `CustomsAndTradeDataWorkflow`가 꺼져 있으면 후속 원장을 만들지 않는다. 1.0 OS가 1.5 원장을 직접 생성하지 않으며, 별도 1.5 준비 원장 Service가 승인 요청을 소비한 뒤 결정적 원장 ID를 `대상원장Id`로 멱등하게 되돌려 기록한다. `Simulation`에서도 이 플랫폼 내부 추적 링크만 저장하며 계약·결제·신고·운송 실행은 열지 않는다.

## 구현 연결

| 책임 | 현재 구현 |
| --- | --- |
| OS·워크플로우 카탈로그 | `Ssalddel.ApiMetadata.SsalddelOperatingSystems`, `SsalddelWorkflow.GroupPurchaseDemand` |
| 집단화 엔진 | `I공동구매주문자집단화Engine`, `공동구매주문자집단화Engine` |
| 런타임 OS | `I공동구매수요모집OS`, `공동구매수요모집OS` |
| 상태전이 Port·Mongo 원장 | `I공동구매수요모집Os상태전이Port`, `Mongo공동구매자동집단화저장소` |
| 마감·Aging worker | `공동구매수요모집OsWorker` |
| OS 배치 등록계획·카탈로그 | `공동구매수요모집Os배치등록계획`, `I공동구매수요모집Os배치Catalog` |
| 가격 수집·게시 handoff | `AgriculturalFisheriesCommunityPipelineRunner` |
| 수요 UseCase | `I공동구매자동집단화UseCase`, `공동구매자동집단화UseCase` |
| API | `공동구매자동집단화Controller` |
| 관리자 운영 API | `공동구매수요모집OsAdminController` |
| 기능 플래그 | `GroupPurchaseDemandWorkflow` |
| 공개·사용자 계약 | `공동구매자동집단요약응답`, `공동구매자동집단사용자응답`, `공동구매자동집단배치미리보기응답` |

## 하지 않는 일

- 사용자의 명시적 참여 없는 자동 가입
- 주문·결제·매매 계약·수입 신고 확정
- 공급자·수입자·관세사·운송사·창고 자동 선정
- 실제 운송 의뢰, 자동 배차, 운임 수취, 보관과 정산
- 보호되거나 비관련 속성에 따른 배제·순위·가격 차별
- 엔진 결과만으로 영속 상태를 최종 확정하는 처리

## 관리자 API

모든 route는 `서버관리자전용` 정책을 요구한다. 배치 카탈로그 조회는 기능 플래그가 꺼진 원인을 확인할 수 있어야 하므로 읽기 전용 bootstrap route로 열어 두고, 상태 변경과 모집 원장 조회 route는 `GroupPurchaseDemandWorkflow` 기능 플래그도 요구한다.

| Method | Route | 역할 |
| --- | --- | --- |
| `GET` | `/api/v1/admin/orderer/group-purchase-demand-os/batch-workloads` | 등록된 내부·공유 배치의 활성 상태, 일정, 선행 작업, 출처와 경계 조회 |
| `GET` | `/api/v1/admin/orderer/group-purchase-demand-os/groups/{autoGroupId}/operating-status` | 저장된 큐·정책·점검·인계 상태 조회 |
| `POST` | `/api/v1/admin/orderer/group-purchase-demand-os/groups/{autoGroupId}/reconcile` | 특정 집단 수동 재조율 |
| `POST` | `/api/v1/admin/orderer/group-purchase-demand-os/groups/{autoGroupId}/handoff-approval` | `Idempotency-Key`와 관리자 식별자로 사람 승인 기록 |
| `POST` | `/api/v1/admin/orderer/group-purchase-demand-os/deadline-scan` | 마감·Aging 점검을 제한 건수로 즉시 실행 |
