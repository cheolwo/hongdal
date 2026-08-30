# Farm 경작·성장·수확 세계 발현 E5

## 식별과 근거

- 주제 고유 식별자: `topic:farm-crop-cycle.v1`
- PlayableLoop 고유 식별자: `playable-loop:farm-crop-cycle.v1`
- 기획 revision: `farm-crop-cycle.design.r1`
- 승인 상태: `Approved`
- 승인 근거: 2026-08-30 사용자가 「기존 WI를 실제 세계에 연결하는 E5 개발 계획」에 `Implement the proposed plan.`으로 구현을 승인했다. 준비된 Nature WI와 Farm을 병행하고, 실제 Scene 배치·실행 확인과 핵심 상태 저장을 포함하는 선택을 적용한다.
- 원천 문답: [건물·공간·배치 문답](PlanningSessions/건물공간배치/building-spatial-placement.inquiry.r1.md)의 Q-078~088, Q-103~106, Q-297~339. 전체 문답의 자유형 개간·관수·H3 확장을 이번 범위에 모두 포함하지 않는다.
- 필수 연구: [Farm 공간·배치 연구 r1](Farm경작세계발현E5-공간배치연구.r1.md), `study:farm-crop-cycle:spatial-placement.r1`, `Accepted`.
- 선행 조사: [전문 산출물 개발 통합 검토](../../Reports/전문산출물-개발통합검토-2026-08-30.md).
- 현재 근거: Loop 원장 r35는 Logic E3 / Presentation E1 / 통합 E1이다. WI 원장 r43의 기존 공간 `integration E6` 기록은 이 폐루프의 실제 Scene·입력 완료가 아니며 삭제하거나 현재 통합 E로 전용하지 않는다.

이 문서 승인은 구현 범위와 검증 기준의 승인이다. 외부 Farm H2 후보의 `ApprovedReference` 전환, 자산 적합성, Scene 배치 결과, 사람 시각 승인 또는 E 승격은 아직 없다. 개발 담당이 이 문서와 연구의 실제 SHA-256을 작업 명세·작업 목록에 결속한 뒤 해당 범위를 시작한다.

## 플레이어 약속과 재미

플레이어는 강변 Farm에서 밭과 작업마당을 알아보고 자유롭게 접근해, 기존 감자 밭 하나를 밭갈이·파종·생육 관리·수확한다. 자신이 확정한 작업 때문에 흙과 작물 표현이 바뀌고 추적 가능한 수확 Lot이 생기는 것을 본 뒤, 수확 결과와 다음에 가능한 행동을 확인한다. 게임을 저장하고 다시 들어와도 진행과 수확 결과가 유지된다.

Farm은 독립 업무 영역이다. Nature 왕복, Hub 화물 인계, Town 판매, NPC 고용이나 운영 서버 연결을 첫 실행의 전제로 삼지 않는다. 강변 H2의 동선은 접근 가능성을 위한 골격이지 플레이어에게 강제하는 이동 순서가 아니다.

## 반복 폐루프

`FarmProductionReady → 밭 접근 → WI-FARM-01 밭갈이 → WI-FARM-02 파종 → WI-FARM-03 생육 관리 → WI-FARM-04 수확 → HarvestLotCreated → FarmHarvestChoiceAvailable → 결과 확인·다음 행동 선택/재시도`

각 행위는 `Preview → 명시적 Confirm → 예약·Task → 기존 WorldTick 완료 → Effect·행위 기록 → 같은 revision 재조회 → 표현 갱신`으로 이어진다. 이 판본의 `성장`은 현재 `CropCare` 완료가 `Growing`을 `HarvestReady`로 바꾸는 기존 규칙이다. 시간 경과나 실제 날씨만으로 성장하는 새 규칙을 도입하지 않는다.

첫 검증은 시작부터 수확까지 한 주기와 그 결과의 반환을 닫고, 기존 규칙이 허용하는 같은 밭의 다음 파종을 실제 실행해 두 번째 주기 회귀까지 확인한다. `Harvested` 재배 단위가 있는 밭을 Preview는 허용하지만 Confirm 완료가 고정 ID 중복으로 실패하는 불일치는 새 기능이 아닌 기존 계약 결함으로 교정한다. 단순히 다음 생산 버튼을 표시하거나 Session을 초기화하는 것으로 반환 완료를 대신하지 않는다. WI-FARM-05~06 실행은 이번 완료 범위가 아니다.

## 선택·대가·성공·실패·회복

- 선택: 접근할 밭과 지금 실행할 유효 작업을 고른다. Preview를 보고 Confirm하거나 작업하지 않고 돌아간다. 현재 우클릭·방향키·지원 시점 전환을 유지하며 새 카메라 체계를 만들지 않는다.
- 대가: 기존 Farm 규칙의 행위자 체력·예약·시간, 파종 종자와 생육 관리 물을 사용한다. 기존 Preview가 산출한 값을 UI에 표시하며 새 밸런스 수치를 추가하지 않는다.
- 성공: 같은 Session에서 토양·재배 단위·수확 Lot의 실제 상태가 전이하고, 원인 WI·Command·Task/Effect·행위 기록·WorldRevision이 이어진다. 수확량은 기존 Fixture 규칙으로 계산한다.
- 실패: 잘못된 대상·상태, 행위자 사용 중, 체력·종자·물 부족, 공간 능력·예약 충돌, ExpectedRevision 불일치는 상태 변경 없이 거부한다. 공간 부적합은 지형을 몰래 평탄화하거나 Scenario 공간으로 대체해 성공시키지 않는다.
- 회복: 최신 상태 재조회 뒤 현재 유효 작업을 다시 Preview한다. 기존 취소 경로는 미사용 예약을 기존 규칙으로 반환한다. 거부·재시도 때문에 이미 완료된 작업이나 수확 Lot을 지우지 않는다.
- 반환: `FarmHarvestChoiceAvailable`에서 수확 결과를 읽고 기존 규칙이 허용하는 다음 파종을 선택·실행할 수 있어야 한다. 수확물을 자동 집하·포장·출하하지 않는다. 이전 수확 Lot·작업 계보를 보존하면서 같은 밭의 다음 재배를 시작하고, 고정 재배 ID 충돌과 재수확 Lot 충돌을 기존 Preview/Confirm 계약에 맞춰 교정한다. 시험용 상태 초기화나 이전 Lot 삭제로 우회하지 않는다.

## WI 단일 책임 후보

기존 WI 고유 식별자를 그대로 사용하며 새 게임 행위를 만들지 않는다.

| WI | 기존 ActionCode | 대상·전이 | E5 연결 요구 |
| --- | --- | --- | --- |
| `WI-FARM-01` | `Tilling` | 토양 `Untilled → Tilled` | 밭갈이 공간·행위자 예약, 실제 Task 완료와 토양 표현 |
| `WI-FARM-02` | `Sowing` | 경작 토양 → 재배 단위 `Growing` | 종자 예약·소비, 토양과 재배 단위의 고유 식별자 연결 |
| `WI-FARM-03` | `CropCare` | 재배 단위 `Growing → HarvestReady` | 물 예약·소비, 생육 관리 Task와 결과 표현 |
| `WI-FARM-04` | `Harvesting` | 재배 단위 `Harvested`, Lot `HarvestedAtField` | 결정적 수확량·Lot 계보, 현장 결과와 다음 선택 |

2026-08-30 읽은 `SimulationFarmSurvivalService.ConfirmWork`는 WI-FARM-04~06만 공통 실행 Pipeline으로 연결하고 01~03은 Aggregate를 직접 호출한다. 그 context의 `AuthorityLocationCode`도 `RemoteHost` 고정이다. 01~03의 기존 실행 의미를 같은 Pipeline·행위 기록·완료 인계에 연결하고, 실제 Host의 권위 위치를 전달한다. Task 시작 기록을 작업 완료나 세계 결과로 오인하지 않는다.

새 실행 endpoint를 네 개 만들지 않고 기존 Farm 작업 Preview/Confirm 및 `ISimulationFarmWorldInteractionRuntime` 경계를 재사용한다. 개별 Command의 멱등성을 작업 시작과 완료 모두에서 유지한다.

## 논리·표현 요구

### Logic E1~E5

- E1~E3: 기존 토양·종자·물·작업·수확 계약과 시험을 보존한다. 승인 범위에 필요한 연결 차이만 시험으로 보강한다.
- E4: 기존 감자 Fixture에서 Farm 행위자 하나·토양 하나·공간 능력·예약·시간·생산 규칙을 결속한다. 공급선 시험에 들어 있는 Hub·Market 시설과 이미 익은 별도 재배 단위는 독립 Farm의 필수 시작 상태로 복제하지 않는다.
- E5: 실제 Session의 네 WI를 순서대로 실행하고 `ActionRecordAppend`, 완료 결과와 다음 파종 실행을 같은 WorldRevision 계보에서 증명한다. 같은 밭 두 번째 주기는 기존 비용·시간·상태 전이를 유지하고 이전 수확 기록을 보존하는 회귀로 확인한다. 플레이어 분야 기여는 기존 농업생산 분류를 사용하며 `PlayerProgressionApply` 또는 정당한 `NotApplicable` 근거를 남긴다.
- LocalProcess와 RemoteHost는 동일 Core를 사용한다. Host별 생산·취소·시간 규칙을 복제하지 않는다.

### Presentation E1~E5

- E4: 연구 r1의 밭·작물·수확물·작업마당 후보를 `presentationE4Preparation`에 결속한다. 자산 경로·GUID·판본·fingerprint, 실제 배치 측정 조건과 fallback 한계를 기록한다.
- 기존 `farm.crop.prepare/plant/grow/harvest` 표현 Slot과 자산 계열을 재사용하되 `CropGrowing` 같은 표현용 이름을 실제 권위 상태와 명시적으로 변환한다. 표현 문자열을 새 Simulation 상태로 취급하지 않는다.
- E5: 동결한 후보를 실제 Prefab 또는 이 연구가 허용한 대체 표현에 연결하고, 활성 Renderer·Collider·Bounds·지지면·InteractionAnchor·통행을 확인한다. Logic E5의 행위 기록 또는 같은 revision 상태 사본을 읽은 증거가 필요하다.
- 작물 소형/대형 모델은 상태를 읽기 위한 표현 후보다. 모델 개수·Scale·Animator 완료로 작물 수량·성장·수확을 결정하지 않는다.
- Scene 실행에서 네 행위를 자동 수행한 뒤 대표 Game View PNG와 Console 결과를 남긴다. 실제 입구·밭·마당·출구 접근도 검증하되 이 결과만으로 E6·E7을 자동 승격하지 않는다.

## H 공간과 자산 요구

공간 단일 기준은 [농장 생산 공간 모판](../../../eng/world-seedbeds/wi-spatial-seedbeds/definitions/farm-production.v1.json)과 [공간·배치 연구 r1](Farm경작세계발현E5-공간배치연구.r1.md)이다.

- H1: 네 WI가 사용하는 `production-plot`과 접근·작업 능력. 별도 수원 자산은 이번에 실제 급수 생산원이 아니다.
- H2: 외부 진입 → Barn 작업마당 → 내부 작업로 → 외부 출구, 밭 접근점·단일 주 하천 보존 영역·자연 여백을 갖는 강변 실용 Farm 검토 인스턴스 하나.
- H3 이상: 이번에는 새 반복 배치나 성장 규칙을 만들지 않는다. 기존 AreaSet 안에 H2 소유와 H1 연결을 명시한다.
- 기존 A/B/C CompositionKey는 모판의 읽기 호환·허용 후보일 뿐 자동 승인된 세 가지 실물 배치가 아니다.
- net10 후보를 그대로 Unity에 넣지 않고 현행 netstandard2.1/C#9 소비 형식으로 순수 변환한다. 실제 Cell·원점·H·CompositionKey·pivot·Scale·입출력 hash를 기록한다.

## 전문 심화 연구 판정과 재결속

| 분야 | 필요성 | 연구 문서·식별자 | 상태 | 승인 기준선 |
| --- | --- | --- | --- | --- |
| 건물 | `NotRequired` | 공간·배치 연구 r1의 Barn 후보 참조 | `NotRequired` | 기존 Barn 외형과 작업마당만 사용한다. 외피 설계·문 개폐·건설·실내 생활은 추가하지 않는다. |
| 공간 | `Required` | `study:farm-crop-cycle:spatial-placement.r1` | `Accepted` | H1 접근·H2 동선·하천/자연 여백 보존, 실제 셀 좌표와 능력 매핑 기준 |
| 배치 | `Required` | `study:farm-crop-cycle:spatial-placement.r1` | `Accepted` | 호환 소스·순수 변환·정규형 hash·실제 Prefab 측정·같은 revision 표현 기준 |
| 애니메이션 | `Required` | `study:farm-crop-cycle:spatial-placement.r1` | `Accepted` | 기존 Actor 이동 유지, 작업·중단·완료 상태 피드백을 필수로 하고 신규 접촉 Clip은 보류하는 최소 표현 기준 |

`Accepted`는 배치 의도·재사용·기술 변환·검증법을 승인한다. 후보의 실제 크기·문·색상·Rig·충돌·World 배치가 통과했다는 의미가 아니다. 측정 산출물과 소스 fingerprint를 작업 명세에 동결하고 부적합한 대상의 표현 E5를 차단한다.

## 저장·권위·외부 경계

- 현재 Farm 상태와 `FarmWorkConfirm` 명령 기록을 기존 Save/Replay에 연결한다. 작업 중·수확 직후·복원 직후의 예약, 토양, 재배 단위, Lot, 행위 기록과 중복 명령 처리를 확인한다.
- 표시용 세계 정의는 생성 당시 지도·패턴·자산 결속 판본과 배치 계획 hash를 유지한다. 로드할 때 최신 자산 후보로 조용히 재선택하지 않는다.
- 저장 계약 변경이 필요하면 개발 통합 담당이 다음 판본 하나를 예약한다. 과거 v1~v29 및 실행 시점의 현행 판본 의미·hash를 유지하고 지원하지 않는 판본을 최신 판본으로 위장하지 않는다.
- 지도구성 → LH 지면·셀 준비 → Sky 상태 적용 → 실외/실내 배치 → World 표현 조립 경계를 재사용한다. LH 활성화는 WI나 자원·작물 상태를 생성하지 않는다.
- 기존 기후 Fixture는 게임 문맥이다. 기상청·농사로 신규 호출, 실제 수원량·수로 흐름, 공공 생산량·운영 DB 효과는 범위 밖이다.

## 제외 범위와 승인

- 이번 전달 상한: Logic E5 / Presentation E5 / 통합 E5. 기존 Goal의 최종 E7 목표와 E8 이후 별도 검증 구조는 유지한다.
- 제외: 새 WI, 자유형 밭 지정·평탄화·암석 제거, 관수 시설, 현실 재식거리 신규 적용, 자연 성장 재설계, NPC 고용·병영·전투, Hub/Town/City 필수 연결, 새 공식 Scene, 원본 Synty 변경, 새 팩 구매, 고급 접촉 애니메이션·오디오 제작, H3 이상 패턴 확장.
- 기존 후보의 합성 시험 값은 게임 치수로 승인하지 않는다. 기술 변환 통과와 실제 물리 배치 통과, 사람의 패턴 승인, E 승격은 별도 기록한다.
- 개발 담당이 공통 원장·생성물·Scene·통합 기록을 소유한다. 공간·애니메이션 담당의 독립 산출물은 담당 경로·기준 hash·소비 시험을 통해 개별 통합하며 모든 담당 종료를 기다리지 않는다.
- 연구 기준선 또는 플레이어 선택·대가를 바꿔야 하면 같은 Goal에서 가장 이른 관련 E를 다시 열고 기획 판본을 갱신한다. 이 문서 작성만으로 현재 E를 변경하지 않는다.
