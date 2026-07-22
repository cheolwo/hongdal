# 공동구매 수요·모집 OS 분리

## 결정

1.0 비구속 수요·주문자 집단화·모집 조율을 `공동구매 수요·모집 OS`로 독립시켰습니다. 기존 `공동주문 수입 OS`는 사람이 확인한 모집 결과를 받은 뒤의 공급·무역 준비와 후속 인계만 담당합니다.

## 코드 반영

- 영속 canonical ID `GroupPurchaseDemandOS`를 추가했습니다.
- `SsalddelOperatingSystem.GroupPurchaseDemand`와 `SsalddelWorkflow.GroupPurchaseDemand`를 추가했습니다.
- Batching, EDF, Aging 모집 정책과 주문자 집단화 엔진 연결을 새 OS에 배치했습니다.
- 자동집단화 UseCase·Controller, 수요 투표와 음식 발견 진입점을 새 워크플로우에 연결했습니다.
- 버전 워크플로우 응답과 Page Capability가 `GroupPurchaseDemandWorkflow`를 사용하도록 정렬했습니다.
- 기존 공동주문 수입 OS에서 집단화 엔진과 모집 정책을 제거했습니다.
- 수요 등록·철회를 실제 OS 조율 경로로 연결하고 트리거·정책·현재 큐·다음 점검 시각을 Mongo 원장에 영속합니다.
- `GroupPurchaseDemandWorkflow`가 켜진 동안 모집 마감과 장기 정체를 처리하는 background worker를 추가했습니다.
- 서버 관리자가 운영 상태 조회, 수동 재조율과 멱등 인계 승인을 수행하는 API를 추가했습니다.
- 집단화 엔진과 Batching·EDF·Aging 정책의 런타임 상태를 `Active`로 노출합니다.
- KAMIS 일별·월별, USDA NASS 월별 가격 수집, 검증된 가격 브리프 게시와 공식 재료 기업 근거 수집을 1.0 지원 배치 카탈로그에 등록했습니다.
- 수집 성공 직후 가격 게시가 활성일 때 같은 원천의 독립 Quartz 게시 일정을 등록하지 않아 중복 실행을 막습니다.
- 관리자 API에서 각 작업의 등록·OS 활성 상태, 실행 방식, cron·시간대, 선행 작업, 출처, 필요 설정과 실행 경계를 조회할 수 있게 했습니다.

## 경계

- 목표 충족은 주문·결제·계약 확정이 아니라 사람의 검토 대기입니다.
- `Confirmed` 호환 상태는 1.0에서 모집 결과의 인계 승인으로만 해석합니다.
- 공급자·수입자·관세사·운송사·창고는 자동 선정하지 않습니다.
- OS는 상태전이 Port를 호출하고 Mongo 저장소가 낙관적 동시성 검증 뒤 실제 상태를 기록합니다. 엔진은 판단만 반환합니다.
- 인계 승인은 `ApprovedAwaitingGroupPurchaseImport` 요청까지만 만들며 1.5 원장과 외부 실행은 자동 생성하지 않습니다.
- 공공 API 호출과 자동 게시 설정은 계속 기본 비활성이며, API key나 source 자격 증명을 카탈로그 응답에 노출하지 않습니다.

## 화면

화면 없음. 기존 UI와 CSS는 변경하지 않고 서버 OS 조율, 관리자 API, background worker, 원장 필드와 문서만 변경했습니다.

## 검증

- API 메타데이터·OS ID·Page Capability 테스트 162개 통과
- 집단화 엔진·UseCase·Controller 회귀 테스트 21개 통과
- 최종 코드 기준 런타임 OS·UseCase·메타데이터 관련 테스트 68개 통과
- 전체 `Ssalddel.Tests` 2,798개 통과(`MSBuildEnableWorkloadResolver=false` 비영속 옵션 사용)
- `Ssalddel` 서버 project build 성공(경고·오류 0)
- `git diff --check` 통과
- OS 배치 등록계획·기존 가격 수집/게시 파이프라인 단위 테스트 20개 통과
