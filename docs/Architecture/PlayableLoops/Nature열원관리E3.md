# Nature 열원 관리 — 문답 기반 Logic E3 구현

## 식별과 근거

- PlayableLoop: `playable-loop:nature-night-day2.v1`
- 주제: `topic:nature-night-day2.v1`
- 기획 revision: `nature-heat-source.design.r1`
- 상태: `Approved` — 2026-08-30 사용자의 신규 33개 문답 기반 E3 구현 요청에 따른 제한 승인.
- 이번 활성 WI: `WI-HEAT-SOURCE-STATE-CHANGE`
- 근거: [동결 문답 Q032~034](PlanningSessions/nature-night-day2.inquiry.r1.md), [자원·건설 Q053](PlanningSessions/Nature자원건설/nature-resource-construction.inquiry.r1.md).
- 승인 범위는 열원 상태 변경의 Logic E1~E3다. 기존 수면·보관·Day2 증거와 Farm 귀환의 E4 승인 대기는 보존한다.

## 플레이어 약속과 재미

플레이어가 쉬기 전에 불을 직접 준비하고, 남은 연료를 확인해 보충하거나 끌 수 있다. 수면 선택이 몰래 연료를 구매·투입하지 않는다.

## 반복 폐루프

꺼진 열원 또는 남은 불씨 확인 → 점화·보충·소화 선택 → 자원·상태 Preview → Confirm → 같은 revision의 열원·연료·행위 기록 조회 → 휴식 준비 또는 다음 관리 선택.

## 선택·대가·성공·실패·회복

- `Ignite`: 꺼짐 또는 불씨 상태를 타는 상태로 바꾼다. 마른 연료 또는 벌목 목재를 명시적으로 투입한다.
- `AddFuel`: 타는 열원에 연료를 투입한다. 꺼진 열원을 보충 요청만으로 점화하지 않는다.
- `Extinguish`: 타는 상태 또는 불씨를 끈다. 연료를 돌려주거나 소비한 시간을 되돌리지 않는다.
- 비용은 권위 정책에 지정한 연료 단위와 열량이며 클라이언트가 열량·상한을 지정할 수 없다. 열량은 정수 단위다.
- 중복 Command는 같은 결과를 반환하고 재소비하지 않는다. 다른 payload로 Command ID를 재사용하면 거부한다.
- 권한·접근·연료·용량·예상 revision·작업 코드 오류는 무변경으로 거부한다. 새 Preview와 유효한 선택으로 재시도한다.
- 수치 튜닝은 승인된 고정 Fixture 정책으로만 시험한다. 출시용 연소 시간·날씨·전파·수면 회복·실패 확률을 만들지 않는다.

## WI 단일 책임 후보

열원 상태 변경 하나에 세 Operation을 둔다. 주 결과는 `HeatSourceStateChanged`이며 자원 차감과 같은 revision의 ActionRecord는 원자적 부수 기록이다. Task 시간 경과·자연 연소·수면·소비·소각 피해·명상 성장은 별도 책임이다.

## 논리·표현 요구

- Logic E1: 입력·출력·상태·권한·불변 정책 계약.
- Logic E2: 순수 Aggregate와 Query·Preview·Confirm 서비스. 상태 사본은 외부 수정으로 내부가 바뀌지 않는다.
- Logic E3: 결정성·멱등·경합·거부·오버플로·행위 계보·정책 사본 시험.
- Presentation E1: 현재 상태·잔량·비용·거부 사유를 읽어야 한다는 요구만 정의한다. 화면·카드 구현은 제외한다.
- E4~E7: 공유 Local/Remote Adapter, 공간·입력·Save·연소/수면 결속·실행 화면은 후속 승인에서 검증한다.

## H 공간과 자산 요구

향후 H1 열원 앵커와 연료 접근 지점이 필요하다. 이번 E3에서는 신뢰된 초기 상태의 단일 플레이어·단일 열원 접근 가능 여부로 경계를 시험하며 좌표나 Prefab을 만들지 않는다. NPC와 다중 열원 인계는 등록 가능 범위지만 이번 증거에 포함하지 않는다.

## 전문 심화 연구 판정과 재결속

건물·공간·배치·애니메이션은 이번 순수 Logic E3 범위에서 `NotRequired`다. 실제 열원·접촉·지면·불꽃·소리·Rig를 연결하는 E4 이전에는 필요한 연구를 다시 판정한다. 연구 없이 표현을 승격하지 않는다.

## 저장·권위·외부 경계

같은 Core를 LocalProcess와 RemoteHost에서 소비할 수 있게 구현하되 이번에는 메모리 수명의 독립 원장만 제공한다. Save revision·세션 Adapter·HTTP API·Provider·운영 DB는 변경하지 않는다. 정책과 초기 재고는 신뢰된 구성 경계에서 한 번 복사한다. 클라이언트 요청은 Actor·열원·Operation·연료 ID·단위 수량·ExpectedRevision·Command ID만 갖는다.

## 제외 범위와 승인

신규 33개 전체 구현 완료를 뜻하지 않는다. 이 문서의 승인 상한은 Logic E3 / Presentation E1 / 통합 E1이다. Q340~345, 자동 보상, 실제 연소 Tick, 열 확산·날씨 효과, 수면 자동 연료 소비, Unity·Scene·Save·RemoteHost는 제외한다.
