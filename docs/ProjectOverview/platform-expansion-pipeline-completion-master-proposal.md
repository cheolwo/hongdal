# 살뜰 플랫폼 확장·파이프라인·완결 종합 제안서

- 기준일: 2026-08-04
- 범위: 커뮤니티, 공공데이터, 개별 의향, 공동구매, 무역 준비, 운송, 창고, 판매채널, 음식점 배달, 마트, 운영·관리
- 판단 기준: 버전 번호보다 사용자 가치, 업무 폐쇄 루프, 데이터 신뢰성, 운영 준비도, 외부 효과 위험을 우선
- 관련 문서: [업무 흐름 리듬](../Architecture/BusinessWorkflowRhythm.md), [업무 실행 책임 모델](../Architecture/BusinessWorkflowResponsibilityModel.md), [릴리즈 게이트](../Versions/release-gates.md)

## 1. 제안의 핵심

살뜰은 더 이상 개별 화면이나 기능을 계속 추가하는 프로젝트로만 볼 단계가 아니다. 현재 저장소에는 공개 정보 지도, 커뮤니티 참여, 개별 의향, 공동구매, 공급·무역 준비, 운송, 입고·재고·피킹·포장, 판매채널, 음식 주문·배달, 마트와 역할별 앱이 이미 넓게 존재한다.

앞으로의 핵심 과제는 이 기능들을 **하나의 업무 생명주기와 운영 가능한 파이프라인으로 묶는 것**이다.

```text
공개 근거와 생활의 필요
  → 질문·제안·참여 의향
  → 역할·비용·위험·조건 협의
  → 공동 Business Case와 원장
  → 계약·권한 Gate
  → 주문·무역·운송·창고·판매·배달 실행
  → 증빙·정산 후보·이슈 처리
  → 완료 사례와 신뢰 기록
  → 새로운 수요와 협업
```

제품 전략은 다음 세 문장으로 정리할 수 있다.

1. **확장 방향**은 기능 수가 아니라 사람들이 정보를 발견하고 함께 일을 끝내는 범위를 넓히는 것이다.
2. **파이프라인화 대상**은 데이터뿐 아니라 수요, 계약, 권한, 실행, 증빙, 정산 후보, 알림과 복구 전체다.
3. **완결 기준**은 화면 존재가 아니라 stable ID 하나로 시작부터 종료·재조회·재처리·감사까지 이어지는가이다.

## 2. 현재 자산과 가장 큰 간극

| 제품 축 | 현재 확보된 자산 | 가장 큰 간극 | 판단 |
| --- | --- | --- | --- |
| 정보·지도 | 지역문화, 관광, 가격, KOSIS, 해외제조업소, 수산, HS 가격 카드, 역할별 지도 | source별 영속 snapshot, 운영 상태, 근거에서 행동으로 가는 연결 | 기반 강함, 운영 pipeline 필요 |
| 커뮤니티 | 게시판, 댓글·투표·활동, 공개 범위, 공동 원장·다이어그램, 친구 후보 경계 | 대화에서 원장, 완료 사례까지 실제 한 바퀴의 통합 증거 | 제품 중심축으로 완결 필요 |
| 수요·공동구매 | 개별 의향, 변경·철회, 집단화, 모집·투표 관련 contract와 UI | 실제 영속 원장 간 인계, 동의 철회 전파, 재실행 멱등성 | 좁은 세로 slice로 연결 필요 |
| 공급·무역 준비 | 공급자 근거, 가격·원가, HS/HTS 후보, 통관 준비와 전문 역할 | 견적 유효성, 단위·통화 정렬, 전문 검토 인계와 책임 표시 | 정보와 실행을 분리한 확장 필요 |
| 운송 | 운송 의뢰, 배차 queue, 추천·공개배차, 기사 흐름, 증빙·알림 기반 | 운영 허가·보험·계약 Gate, 장애 복구, 실제 정산 전 책임 경계 | Simulation 우선, 운영 승격은 별도 |
| 창고·판매 | 입고 계약, 재고, 바코드, 피킹·포장, 판매채널 동기화 | 계약·권한 선행 검증의 일관성, 재고 reservation과 보상 처리 | 업무 pipeline 후보로 성숙 |
| 음식·마트 | 음식 주문·배달, 마트·도심 물류 contract와 일부 실행 흐름 | 음식점·마트·공동구매 재고와 배차 정책의 오염 방지, 영속 store | 수직 제품별 폐쇄 루프 필요 |
| 운영 기반 | Quartz, BackgroundService, Event/Outbox, feature flag, Simulation/Operational 경계 | 실행 원장 통합, lease, dead-letter, 재처리 UI, SLO와 복구 훈련 | 공통 플랫폼으로 우선 투자 |
| 다중 클라이언트 | Web과 역할별 앱, 공유 UI, route·PageViewModel 체계 | 같은 업무 상태의 Web·MAUI 재조회, deep link, 오류·복구 일관성 | 공통 workflow shell 필요 |

최근 지도와 역할 관점 변경은 중요한 통합 기반이지만, 지도 레이어의 증가 자체를 전체 제품 완성으로 보지는 않는다. 일부 기능은 build·test 또는 Simulation 증거가 있고, 일부는 catalog·proposal·in-memory 구현에 머문다. 각 capability의 상태를 분리해 관리해야 한다.

## 3. 넓게 확장할 제품 영역

### 3.1 공개 근거를 연결하는 정보 그래프

공공데이터를 각각의 카드와 지도 marker로만 제공하지 않고 다음 식별자를 연결한 **근거 그래프**로 확장한다.

- 지역, 음식, 재료, 품목, HS/HTS 후보, 시장, 기관, 업체 근거
- 출처, 원문, 기준시각, 수집시각, 단위, 통화, 공간 범위, 갱신주기
- 관련 게시글, 질문, 참여 의향, 견적 요청, 원장과 완료 사례
- 동일 개체의 별칭과 국가·기관별 코드 crosswalk
- 검증됨, 오래됨, 제한됨, 자료 없음, 검토 필요 상태

이 그래프는 공급 가능성, 재고, 계약 관계를 추정하지 않는다. 공개 사실과 개인·조직의 운영 상태는 다른 저장소와 공개 범위를 유지한다.

### 3.2 공동행동 Workspace

사용자가 지도, 게시판, 상품 카드 어디에서 시작하더라도 하나의 공동행동 Workspace로 들어가게 한다.

Workspace에는 다음이 함께 보인다.

- 이 일을 시작한 공개 근거와 문제 정의
- 참여 의향, 필요한 역할, 역할 지원과 담당자 확인
- 비용, 노동, 위험, 미정 조건과 계산 근거
- 대화, 투표, 이견, 결정과 철회 이력
- 가원장·실원장, 상태, 관계와 선택적 증빙
- 가능한 다음 행동과 현재 막힌 Gate
- 완료 조건, 사례 공개 범위와 후속 행동

앱은 역할별로 다른 작업 목록을 보여 주되 같은 Business Case와 원장을 조회한다. 주문자, 화주, 기사, 창고, 음식점과 운영자가 서로 다른 사본을 갖지 않도록 한다.

### 3.3 공급·무역 준비 Desk

공급자를 자동 추천하거나 계약시키는 기능보다, 검토 가능한 준비 자료를 만드는 Desk로 확장한다.

- 공개 업체·제조업소 근거와 직접 제출 자료를 구분
- 견적 version, 통화, Incoterms, 유효기간, 최소수량, 포장단위
- HS/HTS 후보와 성분·가공상태·용도에 따른 불확실성
- 검역·표시·관세·행정 check와 전문 검토 요청
- 예상 원가의 가정, 제외 비용과 민감도
- 검토 완료 후에도 발주·신고·결제를 별도 승인으로 분리

### 3.4 운송·창고·판매 Fulfillment Network

개별 운송·창고 기능을 한 화면에 합치는 것이 아니라, 표준 handoff contract로 연결한다.

- 구매 또는 판매 결과에서 출고 필요 생성
- 운송 인계의 상차·하차 조건과 증빙 요구
- 입고 계약 확인 후 검수·적재·재고 생성
- 판매 가능 계약만 reservation·피킹·포장으로 진행
- 국내·해외 판매채널 주문을 같은 출고 계획으로 정규화
- 실패·부분완료·취소 시 재고와 원장 보상
- 인수 완료 후 비용·운임·수수료의 정산 후보 생성

실제 운송사·창고·판매채널 adapter는 공통 port 뒤에 둔다. 초기에는 fixture·sandbox·수동 인계로 전체 흐름을 검증하고, 외부 연결은 capability별로 승격한다.

### 3.5 음식점 배달과 마트 물류의 독립 수직 제품

배차 기술을 공유하더라도 두 제품의 원장과 정책은 분리한다.

| 음식점 배달 | 마트·도심 물류 |
| --- | --- |
| 주문 접수, 조리, 픽업 준비, 기사 인수, 고객 전달 | 재고 확인, reservation, 피킹, 대체품, 포장, 기사 인수, 고객 전달 |
| 조리시간과 음식 품질이 핵심 | 재고 정확도와 피킹 품질이 핵심 |
| 음식점 취소·품절·조리 지연 정책 | 부분 품절·대체·중량 차이·냉장 분리 정책 |
| 음식 주문 원장 | 마트 주문·재고·출고 원장 |

공통 배차, 주소 공개, 알림과 결제 경계는 재사용할 수 있지만 상태 코드를 억지로 하나로 합치지 않는다.

### 3.6 파트너와 운영 생태계

향후 확장은 플랫폼이 모든 일을 직접 수행하는 방향보다, 자격과 책임이 확인된 참여자가 같은 원장에서 협업하는 방향이 적합하다.

- 생산자·판매자·수입자·운송사·창고·관세사·행정사·음식점·마트의 조직 profile
- 역할 신청, 자격·보험·계약 문서 검토와 만료 관리
- API 또는 파일 기반 partner adapter와 sandbox
- 담당자 범위, 위임, 교대와 비상 연락 정책
- 서비스 수준, 이슈, 이의·사고와 개선 이력

공개 지도 사실을 파트너 가용성이나 계약 상태로 변환하지 않고, 파트너가 제출하고 검증된 운영 자료만 별도 권한으로 사용한다.

## 4. 공통 파이프라인으로 만들 영역

### 4.1 데이터 수집·근거 발행 Pipeline

모든 사용자 입력, 공식·공공 데이터, RSS·파일·외부 API, 내부 Event와 AI 파생값에 공통으로 적용할 처리 범위·동의·보존·공개·외부 효과 기준은 [입력·데이터 수집 처리 파이프라인 정형화 제안서](../Architecture/DataInputCollectionProcessingPipelineProposal.md)를 단일 기준으로 삼는다.

```text
Source Registry
  → Acquire
  → Raw Archive
  → Normalize
  → Validate / Quarantine
  → Versioned Snapshot
  → Projection
  → Search / Map / Detail
  → Canonical Community Brief
  → Freshness / Failure Monitoring
```

공통 실행 키는 `SourceKey + PeriodKey + SchemaVersion`으로 하고, 마지막 성공본 유지, source별 실패 격리, 재처리 멱등성, 원문 hash와 공개 version을 기본으로 한다. 현재 process memory에 있는 지도 snapshot부터 영속 repository로 옮긴다.

### 4.2 의향·동의·집단화 Pipeline

```text
개별 의향 등록
  → 본인 소유 원장 저장
  → 변경·철회
  → 별도 참여 동의
  → 조건별 집단화 후보
  → 모집 원장
  → 목표·마감·역할 확인
  → 진행 또는 종료
```

- 관심, 참여, 연락처, 가원장, 실원장, 실행 동의를 각각 저장한다.
- 철회는 다음 projection과 집계에서 즉시 제외하되 과거 감사 이력은 남긴다.
- 지리적 가까움은 후보 집계에만 사용하고 자동 가입·상대 선정 근거로 쓰지 않는다.
- 집단화 engine은 후보와 사유만 반환하고 저장은 UseCase가 수행한다.

### 4.3 Business Case·원장 Pipeline

모든 수직 업무가 공유할 가장 중요한 기반이다.

```text
Case 생성
  → Section 입력
  → Policy/Gate 평가
  → 역할·담당자 확인
  → Command
  → 상태 변경
  → Event/Outbox
  → Projection
  → 같은 Case 재조회
  → 완료·취소·분쟁·보상
```

Case에는 stable ID, 시작 근거, 참여자와 역할, 공개 범위, 상태, 선행·후속 handoff, 판단 근거, 증빙, 재시도와 종료 사유를 둔다. 지도·게시글·주문·운송·입고 화면은 Case의 서로 다른 projection이 된다.

### 4.4 계약·권한 Gate Pipeline

실행 직전에 공통 Gate가 다음을 확인하게 한다.

- 계약 유형과 허용 업무
- 사용자·조직 역할과 위임 범위
- 자격·보험·약관·동의의 유효성
- 기능 플래그와 `Simulation`/`Operational`
- 지역·시간·금액·데이터 공개 제한
- 외부 효과별 추가 승인

화면 숨김만으로 차단하지 않고 API·UseCase에서 같은 Gate를 적용한다. Gate 결과에는 허용 여부뿐 아니라 막힌 이유와 해결 가능한 다음 행동을 남긴다.

### 4.5 실행 Orchestration Pipeline

각 책임을 구분해 현재 흩어진 scheduler와 worker를 정렬한다.

| 책임 | 사용 구조 | 금지 사항 |
| --- | --- | --- |
| 장기 업무 상태 | Process Manager | 화면 session에 장기 상태 보관 |
| 호출 순서·보상 | Workflow Coordinator | 외부 호출 성공을 원장 성공으로 바로 간주 |
| 시간 실행 | Quartz Scheduler | 기능별 임의 무한 loop 남발 |
| 전달 보장 | Transactional Outbox | 상태 저장과 외부 발송의 이중 성공 가정 |
| 재시도 격리 | lease·retry·dead-letter | 무한 재시도와 실패 은폐 |
| 읽기 제공 | Projection·Query | 요청 중 외부 API 직접 호출 |

모든 background workload는 공통 activation policy, 단일 실행 주체 또는 분산 lease, 실행 원장, 취소, timeout, retry budget과 dead-letter 재처리 절차를 갖는다.

### 4.6 Partner Integration Pipeline

외부 서비스마다 controller와 job을 새로 만들기보다 공통 adapter 생명주기를 둔다.

```text
Catalog-only
  → Contract 검토
  → Credential 준비
  → Sandbox probe
  → Normalization contract
  → Fixture replay
  → 제한된 read-only 연결
  → 승인된 write effect
  → 운영·중단·복구
```

각 단계는 비용, 약관, 개인정보, 재배포, rate limit, 장애와 공급자 종속성을 검토한다. secret 존재는 runtime 연결 증거가 아니며, adapter 존재는 운영 승인 증거가 아니다.

### 4.7 문서·이미지·콘텐츠 Pipeline

- 공식 근거 조사와 콘텐츠 초안 생성
- 출처·권리·개인정보·왜곡 검토
- AI 생성물 표시와 사람 승인
- 비용 발생 생성과 저장·게시 승인을 분리
- canonical 게시와 파생 link
- 수정·철회·대체 version 관리

이미지의 `confirm-billable`과 `confirm-storage-write` 같은 승인 경계를 문서, 대량 알림, 자동 게시에도 동일한 철학으로 적용한다.

### 4.8 검증·승격·복구 Pipeline

버전 전체 완료를 기다리기보다 capability마다 아래 단계를 통과시킨다.

```text
Contract Test
  → Domain/UseCase Test
  → Persistence/Migration Test
  → Adapter Fixture Test
  → Integrated Build/Test
  → Browser/Mobile Runtime
  → Simulation
  → 제한 공개
  → 운영 공개
  → 복구 훈련 후 기본 공개
```

각 capability card에는 구현, build/test, runtime, 외부 연결, commit, push, deploy 상태를 별도로 기록한다.

## 5. 완결해야 할 공통 기반

### 5.1 영속성 정리

- 핵심 업무의 `InMemory*Store`를 목록화하고 운영 대상부터 영속 store로 교체
- Mongo 원본 원장과 RDB 권한·조회 projection의 소유권 확정
- 같은 stable ID, idempotency key와 optimistic concurrency 적용
- migration, backup, restore, projection rebuild와 rollback 검증
- 개발 sample과 운영 저장 실패를 명확히 분리

### 5.2 Event·Outbox 정리

- 업무 상태와 Outbox를 같은 transaction에 기록
- consumer idempotency와 순환 발행 방지
- lease, retry, 최대 시도, dead-letter, 수동 재처리
- payload version과 호환 기간
- 운영자용 backlog·지연·실패 상태와 감사 UI

### 5.3 계약·권한·책임 정리

- 계약 레인, 인사 레인, 실행 레인의 선행조건을 모든 상태 전이에 적용
- 조직·개인·전문가 역할과 위임을 분리
- 연락처·주소·정확한 위치의 목적별 공개 동의
- 공급자, 운송사, 창고, 전문가와 플랫폼 책임 표시
- 결제·계약·신고·유상 주선·정산의 승인 주체 명시

### 5.4 공통 UI와 다중 클라이언트 정리

- Route → Page → PageViewModel → Workflow Session → Client 구조 일관화
- 역할별 앱이 같은 Case를 다른 projection으로 조회
- loading, empty, error, retry, disabled, stale, conflict 상태 공통화
- 상태 전이 성공 후 server 재조회
- Web·Android·iOS deep link와 offline 재접속
- 접근성, reduced motion, 모바일 정보 밀도 검증

### 5.5 운영 관측과 복구 정리

- source, scheduler, outbox, adapter, workflow별 health와 SLO
- correlation ID로 사용자 요청부터 Event·외부 호출까지 추적
- 비용, 호출량, rate limit, queue age, stale snapshot과 실패율
- 운영 중단 switch와 capability별 kill switch
- 장애·데이터 오염·중복 실행·partner 장애 복구 runbook
- secret rotation, 개인정보 삭제와 감사 로그 보유 정책

## 6. 완결해야 할 수직 흐름

### A. 공개 근거에서 공동행동까지

완료 상태는 사용자가 공개 근거에서 질문을 만들고, 사람들과 숙고하고, 명시적 동의로 원장을 만들고, 완료 사례를 승인해 다시 커뮤니티에 돌려보내는 것이다.

### B. 개별 의향에서 공동구매까지

완료 상태는 개인 의향의 등록·변경·철회가 집단화 결과에 정확히 반영되고, 참여 동의자만 모집 원장에 포함되며, 실패·마감·철회 뒤에도 같은 원장을 재조회하는 것이다.

### C. 공동구매에서 공급·무역 준비까지

완료 상태는 검증된 수요가 공급자 근거, 견적 version, 원가 가정, HS/HTS 후보와 전문 검토에 연결되고, 검토가 끝나도 발주·신고·결제가 자동 실행되지 않는 것이다.

### D. 구매·출고에서 운송 인계까지

완료 상태는 확인된 원장에서 운송 필요가 만들어지고, 계약·자격 Gate를 통과한 수행자에게 인계되며, 상차·하차·인수 증빙과 예외가 기록되는 것이다. 운영 준비 전에는 Simulation과 수동 인계로 닫는다.

### E. 입고에서 재고·피킹·포장·판매까지

완료 상태는 입고 계약에 따라 재고가 생성되고, 판매 가능한 재고만 예약되며, 피킹·포장·출고·판매채널 상태가 같은 원장에서 연결되는 것이다. 취소와 부분 실패의 보상까지 포함한다.

### F. 음식점 주문에서 배달 완료까지

완료 상태는 주문 접수, 조리, 픽업 준비, 기사 인수, 고객 전달과 이슈 처리의 주체·시각·증빙이 영속 저장되는 것이다.

### G. 마트 주문에서 도심 배송까지

완료 상태는 재고 확인, 대체품 동의, 중량·가격 변동, 피킹, 온도대별 포장, 기사 인수와 고객 전달이 음식점·공동구매 정책과 섞이지 않고 이어지는 것이다.

## 7. 우선순위 결정 방식

앞으로의 backlog는 버전 번호보다 아래 점수로 정렬한다.

| 기준 | 질문 | 높은 점수를 주는 경우 |
| --- | --- | --- |
| 사용자 가치 | 실제로 더 많은 사람이 일을 시작하거나 끝낼 수 있는가? | 단절된 사용자 여정을 연결 |
| 재사용성 | 여러 역할·수직 제품이 사용하는가? | Case, Gate, Outbox, snapshot 같은 공통 기반 |
| 완결성 | 미완료 상태를 재조회·복구 가능한 종료로 바꾸는가? | 영속성·멱등성·보상·감사 추가 |
| 위험 감소 | 개인정보·비용·법률·운영 위험을 줄이는가? | 명시적 동의와 외부 효과 차단 |
| 증거 가능성 | 자동 test와 runtime으로 판정할 수 있는가? | 좁은 세로 slice와 명확한 완료 조건 |
| 확장 비용 | 새 adapter와 화면마다 중복을 줄이는가? | 공통 pipeline과 표준 handoff 제공 |

낮은 우선순위는 단독 신규 화면, 실사용 경로가 없는 catalog 추가, 별도 in-memory store, 중복 scheduler와 승인 없는 외부 연결이다.

## 8. 권장 투자 순서

### 2026-08-04 구현 상태 재검토에 따른 재정렬

초안 이후 지도 observation에서 질문 초안·게시글을 만드는 stable ID 경로, 공식뉴스 RSS 검토 후보, 국가별 언론사 마커, 지도 마커 신청 메뉴가 추가됐다. 개인정보 관문은 서버 증적·철회와 신청 Command Gate까지 확장됐다. 이제 관리자 기능과 후속 실행 workflow를 앞세우지 않고 **사용자가 들어오는 지도 자체의 영속 근거, 선택 복원, 상세 재조회와 대화 전환**을 먼저 닫는다.

| 우선순위 | 지금 할 묶음 | 현재 상태 | 다음 완료 기준 |
| --- | --- | --- | --- |
| A0 | 현재 변경 통합 기준선 | 여러 기능이 같은 작업 트리에 있으나 build·runtime·commit·push 증거가 서로 다름 | capability별 route·contract·store·test·runtime 증거를 분리하고 0.0 비활성 구성 검증 |
| A1 | 공개 근거 Case 폐쇄 루프 | 완료 — 지도 observation → 질문 → 상세 근거 → 명시적 관심 → 중복·철회 가능한 서로 다른 참여자 → 가원장 생성·재조회 | 완료 증거를 A0 capability 기준선에 편입하고 회귀 검증 유지 |
| A2 | 신청 개인정보 서버 Gate | 완료 — 계정·업무·출처·버전·문안 hash·시각을 MongoDB 증적으로 저장하고 철회 및 세 지도 신청 Command Gate 연결 | 보유기간 집행·실제 제3자 제공·국외 이전은 B2 운영 Gate에서 별도 완결 |
| M1 | 지도 source snapshot | 지도 source가 서로 다른 수명주기와 일부 process memory를 사용 | 관광·온라인가격·KOSIS부터 versioned snapshot, 마지막 성공본, freshness·source version 재조회 |
| M2 | 지도 탐색 연속성 | 선택 상태는 화면 내부에 있으나 dataset 외 국가·레이어·마커 deep link가 충분하지 않음 | 공유 URL·새로고침·뒤로가기에서 같은 지도 선택과 상세 복원 |
| M3 | 지도 근거의 대화 전환 | observation 질문·게시글 경로가 구현됨 | 같은 stable ID와 snapshot version을 질문 초안·게시글 상세까지 유지하고 runtime 회귀 검증 |
| M4 | 뉴스/RSS 지도 보강 | 공식 feed parsing·allowlist와 국가별 언론사 마커는 있으나 후보가 process memory | 승인·제외·중복 상태를 영속화하고 승인된 metadata만 마커 상세·canonical 게시판에 연결 |
| S1 | 안전·권리·복구 | 화면 경계와 일부 정책은 있으나 운영 폐쇄 루프 부족 | 신고·차단·삭제·동의 철회·보유기간·backup/restore를 감사 가능한 상태로 검증 |
| S2 | 완료 사례 환류와 다중 client | 원장·다이어그램 자산은 있으나 한 Case의 공개 환류 증거 부족 | 비식별 사례 초안 → 사용자 공개 승인 → Web·Android·iOS stable deep link |
| C | 실행 수직 제품 | 운송·창고·마트 등 넓은 자산 존재 | Simulation 폐쇄 루프를 하나씩 완성하고 허가·계약 Gate 전 운영 외부 효과는 비활성 |

`A1`은 서로 다른 참여자 조건, 동일 계정 중복 방지, 관심 철회, 세 가지 가원장 동의, 멱등 생성과 같은 게시글 재조회까지 구현됐다. `A2`도 서버 증적 저장·조회·철회와 물류대행·운송대행·개별 주문 Command Gate까지 구현했다. 이 완료 기능은 회귀 검증으로 유지하되 다음 구현은 `M1` 지도 source snapshot이다. 신청·운송·관리자 기능은 지도 핵심 탐색과 근거 전환을 막는 오류가 아니라면 `C` 보조 backlog로 둔다.

| 순서 | 투자 묶음 | 첫 세로 slice | 완료 판정 |
| --- | --- | --- | --- |
| 1 | 지도 source 영속 pipeline | 관광·온라인가격·KOSIS 영속 snapshot | 재시작·부분 실패·재처리 뒤 같은 공개 version과 마지막 성공본 확인 |
| 2 | 지도 상태·상세 재조회 | 국가·레이어·마커 deep link와 observation 상세 | 공유 URL·새로고침·뒤로가기 뒤 같은 선택과 source version 표시 |
| 3 | 지도에서 대화로 | 공개 observation → 질문 초안 → 게시글 재조회 | 같은 stable ID와 근거 version이 Web/API에서 확인됨 |
| 4 | 지도 사용성 | 모바일 bottom sheet, marker 밀도, 접근성, 오류 복구 | desktop·390px 실제 렌더와 keyboard·reduced-motion 검증 |
| 5 | 뉴스/RSS 지도 보강 | 승인 기사 metadata → 언론사 마커 상세·게시판 | 자동 게시 없이 최신 승인 근거·기준 시각·원문 link 표시 |
| 6 | 공통 Case·원장 환류 | 게시글 → 가원장 → 사례 초안 | 동의·철회와 비식별 공개 승인까지 한 바퀴 재조회 |
| 7 | 실행 기반 신뢰성 | Outbox 실행 원장, lease, dead-letter와 운영 UI | 실패 주입 뒤 중복 없이 복구 가능 |
| 8 | 수직 제품·partner 운영 | 공동구매·운송·창고 중 한 폐쇄 루프 | Simulation 검증 뒤 허가·계약 Gate를 통과한 adapter만 제한 활성화 |

1~3은 다른 모든 기능의 품질과 개발 속도를 높이는 공통 투자다. 4~8은 시장과 운영 준비도에 따라 병렬로 선택할 수 있지만 공통 pipeline을 우회해서는 안 된다.

## 9. 성과 지표

### 제품 지표

- 공개 근거를 본 사용자가 질문·참여 의향으로 이어지는 비율
- 질문에서 원장 생성, 원장에서 완료까지의 전환율과 소요시간
- 철회·이견·실패가 숨겨지지 않고 해결된 비율
- 완료 사례가 새 대화나 협업으로 이어진 비율

### 데이터 지표

- source별 freshness, 성공률, quarantine율, 중복 방지율
- 출처·단위·기준시각·지역·제한 필드 완전성
- 마지막 성공 snapshot 복원 시간과 projection 재생성 시간

### 실행 지표

- Case 상태 전이 성공률, idempotent replay 성공률
- Outbox queue age, retry·dead-letter 비율, 중복 외부 효과 0건
- 주문·운송·입고·배달의 예외 해결 시간

### 안전 지표

- 동의 없는 연락처·주소·정확한 위치 공개 0건
- 기능 비활성 또는 Simulation 환경의 운영 외부 효과 0건
- 삭제·철회 요청 처리시간, 감사 로그 완전성
- 장애 복구 훈련 성공률과 데이터 손실 범위

## 10. 당장 시작할 세 가지

### 첫째, Capability 현황 원장

모든 주요 기능을 `제안 / contract / 구현 / build-test / runtime / Simulation / 제한 공개 / 운영 공개` 상태로 분류한다. 각 항목에 소유 Case, route, contract, store, Event, client, test, 운영 Gate와 최근 증거를 연결한다. 이 원장이 있어야 넓은 저장소에서 구현된 것과 보이는 것, 실제 운영 가능한 것을 혼동하지 않는다.

### 둘째, 공통 Case와 Handoff contract

한 번에 전체 도메인을 통합하지 말고 `공개 observation → 커뮤니티 질문 → 가원장` 한 slice로 stable ID, 동의, 상태, Event와 재조회를 고정한다. 이후 개별 의향, 공동구매, 무역, 운송과 창고가 같은 handoff 규칙을 채택하게 한다.

### 셋째, 영속 source·job 운영 Pipeline

관광·온라인가격·KOSIS 메모리 snapshot을 첫 대상으로 source 실행 원장, versioned snapshot, Quartz/lease, retry·quarantine, 운영 상태 API를 완성한다. 이 구조를 KAMIS, USDA, 경기 축산, 해양 파일, 판매채널과 partner adapter로 확장한다.

## 11. 최종 제안

살뜰의 다음 성장은 더 많은 메뉴를 만드는 데 있지 않다. **출처 있는 정보가 사람의 의향과 동의로 바뀌고, 여러 역할이 같은 원장에서 협업하며, 실제 실행은 계약·권한·운영 Gate를 통과하고, 실패와 완료가 다시 커뮤니티의 신뢰 자산으로 돌아오는 구조**를 만드는 데 있다.

버전은 배포와 호환성을 관리하는 도구로 유지하되, 투자 우선순위는 다음 질문으로 정한다.

> 이 작업이 하나의 실제 사용자 필요를 더 안전하게 시작하게 하고, 여러 역할이 같은 상태를 보게 하며, 실패해도 복구하고 끝까지 완료하게 하는가?

그 질문에 가장 강하게 답하는 공통 Case backbone, 영속 데이터 pipeline, 실행·복구 기반을 먼저 완성한 뒤 공급·운송·창고·음식·마트 수직 흐름을 그 위에서 선택적으로 확장하는 것이 가장 효율적이다.

## 부록

- 공공데이터와 커뮤니티 기반에 초점을 둔 좁은 실행안은 [문화교통 0.0 확장·파이프라인·완결 제안서](../Versions/v0.0/expansion-pipeline-completion-proposal.md)를 참고한다.
