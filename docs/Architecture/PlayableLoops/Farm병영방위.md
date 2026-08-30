# Farm 병영 방위

## 식별과 근거

- 주제 고유 식별자: `topic:farm-barracks-defense.v1`
- PlayableLoop 고유 식별자: `playable-loop:farm-barracks-defense.v1`
- 기획 revision: `farm-barracks-defense.design.r6`
- 원천 문답: [Farm 병영·방위 문답](PlanningSessions/Farm병영방위/farm-barracks-defense.inquiry.r1.md)
- 반영 문답: Q-223~Q-239, D1~D5 전부 `Confirmed`
- 승인 상태: `Approved`
- 승인 근거: 사용자가 승인 대기 WI 하나에 고정하지 말고 다른 WI도 목표로 삼아 개발을 계속하도록 명시했으며, 기존 개발 인계는 이 주제의 첫 묶음을 Logic E3 우선 대상으로 지정했다.

## 플레이어 약속과 재미

플레이어는 Farm 외곽 경비 초소에 농민을 분대로 배치하고, 위협이 접근하면 자동 출동시키며, 직접 참전 여부와 생산 손실을 비교한다. 방어 뒤에는 전리품·부상자·생존 인력을 초소와 Farm 생활로 돌려보낸다.

생산 인력을 경비에 투입하는 기회비용과 직접 참전 여부를 비교하는 데 재미의 중심을 둔다. 자동 방위가 Farm을 지키지만 생산력이 공짜로 유지되지는 않는다.

## 반복 폐루프

`위협 감지 → 분대 준비 확인 → 자동 소집·출동 → 플레이어 선택 참전 → 방어 결과 → 초소 귀환 → 치료·휴식·생산 재합류`

수직 구현은 폐루프 전체를 한 WI로 만들지 않고 `분대 배정 → 분대 보급 → 자동 소집`의 단일 책임 WI를 순차적으로 닫는다.

소집 WI의 E4 후보 동결 뒤 두 번째 구현은 `WI-SQUAD-ASSIGN` 하나만 활성화한다. 플레이어가 초소 관리 문맥에서 빈 배치 슬롯 하나와 미배정 분대 하나를 선택하고 Preview 뒤 Confirm한다. 이 WI는 분대 편성·훈련·영웅 조작·출동을 수행하지 않는다.

### 두 번째 WI: 분대 배정

- 시작 상태: 등록된 Farm 경비 초소와 빈 슬롯, 미배정 분대가 있다.
- Preview: Expected revision, 초소·슬롯·분대 존재, 슬롯 점유와 기존 분대 배정을 검사하며 상태를 바꾸지 않는다.
- Confirm: 슬롯과 분대의 1:1 배정을 같은 revision에서 만들고 `WI-SQUAD-ASSIGN` 행위 기록을 한 번 남긴다.
- 멱등성: 같은 Command와 같은 초소·슬롯·분대는 결과를 재사용하고 payload가 달라지면 거부한다.
- 주체: `PlayerDriven`, Host Player 발의, Simulation Core 권위, NPC 분대 대상이다.
- 제외: 분대원 구성 변경, 식량·장비 소비, 자동 출동, 영웅 직접 조작, 실제 Formation UI.

소집과 분대 배정의 기존 증거를 보존한 뒤 세 번째 구현은 `WI-SQUAD-SUPPLY` 하나만 활성화한다. 초기 권위 상태가 가진 분대별 식량 필요량과 장비 내구도 복구 필요량을 Preview로 읽고, Confirm에서 두 자원을 동시에 소비해 해당 분대를 보급 완료 상태로 만든다. 숫자는 Fixture 입력이며 이번 기획이 게임 밸런스 값을 고정하지 않는다.

### 세 번째 WI: 분대 보급

- 시작 상태: 등록된 분대가 아직 보급 완료가 아니고, 권위 상태에 식량 재고·장비 내구도 복구 능력·분대별 필요량이 있다.
- Preview: Expected revision, 분대 존재, 기보급 여부, 식량과 내구도 복구 능력 충족 여부를 검사하며 상태를 바꾸지 않는다.
- Confirm: 초기 상태가 정한 필요량만큼 식량과 내구도 복구 능력을 원자적으로 소비하고 `WI-SQUAD-SUPPLY` 행위 기록을 한 번 남긴다.
- 멱등성: 같은 Command와 같은 분대는 결과를 재사용하고 같은 Command의 다른 분대는 거부한다.
- 주체: `PlayerDriven`, Host Player 발의, Simulation Core 권위, NPC 분대 대상이다.
- 제외: 플레이어가 보급 수량을 임의 입력하는 기능, 개별 장비 슬롯·아이템 수리, 훈련, 출동, 전투, 자동 재보급.

기존 세 WI의 증거를 보존한 뒤 네 번째 구현은 `WI-FARM-DEFENSE-RESOLVE` 하나만 활성화한다. 이 WI는 전투 권위가 이미 확정한 성공 결과 묶음을 읽고 Farm의 위협·안전 기간·생산/회복 보정·전리품에 한 번만 발현한다. 승패나 수치를 다시 계산하지 않는다.

### 네 번째 WI: 방어 성공 결과 발현

- 시작 상태: 전투 권위가 조우·수행 분대·성공 여부·위협 감소·안전 종료 Tick·생산/회복 보정·전리품을 판본화된 결과 묶음으로 확정했다.
- Preview: Expected revision, 조우 존재, 미발현 여부, 성공 결과 여부를 검사하고 확정 결과 수치를 읽기만 한다.
- Confirm: 성공 결과 묶음을 위협 감소, 안전 기간 확장, 생산/회복 보정, 전리품 재고에 원자적으로 발현하고 `WI-FARM-DEFENSE-RESOLVE` 행위 기록을 한 번 남긴다.
- 멱등성: 같은 Command와 같은 조우는 결과를 재사용하고 같은 Command의 다른 조우는 거부한다.
- 주체: `WorldDerived`, 전투 결과 규칙 발의, NPC 분대 결과, Simulation Core 권위다.
- 제외: 전투 승패·피해·보상 수치 계산, 실패 결과의 별도 처리, 부상·치료·초소 귀환·생산 재합류, 실제 UI·전리품 Prefab.

기존 네 WI의 증거를 보존한 뒤 다섯 번째 구현은 `WI-FARM-DEFENSE-RETURN` 하나만 활성화한다. 방어 결과가 확정된 분대를 지정 초소로 귀환 완료시키고, 부상자는 치료 대기열에, 생존 작업자는 생산 재합류 후보 대기열에 인계한다. 치료·휴식·생산 재합류 자체는 별도 WI가 소유한다.

### 다섯 번째 WI: 초소 귀환 인계

- 시작 상태: 조우 결과가 확정됐고 귀환 대상 분대·초소·치료 필요 Actor·생산 재합류 후보 Actor가 권위 상태에 있다.
- Preview: Expected revision, 귀환 정의 존재, 결과 확정, 기귀환 여부를 검사하며 후속 인계 건수를 읽기만 한다.
- Confirm: 분대를 초소 귀환 완료로 바꾸고 치료·생산 재합류 후보를 서로 겹치지 않는 후속 대기열에 한 번 인계한다.
- 멱등성: 같은 Command와 같은 귀환은 결과를 재사용하고 같은 Command의 다른 귀환은 거부한다.
- 주체: `WorldDerived`, 방어 귀환 규칙 발의, NPC 분대 수행, Simulation Core 권위다.
- 제외: 전리품 재지급, 치료·휴식 실행, 생산 기여 재개, 이동 애니메이션·경로, 실제 초소 배치·UI.

## 선택·대가·성공·실패·회복

- 선택: 분대 편성·보급·직접 참전 여부는 후속 WI에서 선택한다. 첫 WI는 승인된 준비 분대의 자동 소집만 담당한다.
- 대가: 출동 분대에 배정된 작업자의 Farm 생산 기여가 중단된다.
- 성공: 접근 위협과 준비 분대가 같은 revision에서 결속되고 출동 행위 기록이 남는다.
- 실패: revision 불일치, 알 수 없는 분대·위협, 준비되지 않은 분대, 기출동 분대는 상태 변경 없이 거부한다.
- 회복: 전투 결과와 초소 귀환·치료·생산 재합류는 `WI-FARM-DEFENSE-RESOLVE`, `WI-FARM-DEFENSE-RETURN`에서 닫는다.

## WI 단일 책임 후보

`WI-FARM-DEFENSE-MOBILIZE`는 준비된 방위 분대 하나를 접근 중인 위협 하나에 출동시키고, 배정된 작업자의 Farm 생산 기여를 중단한다.

- Preview: WorldRevision·분대 존재·준비 상태·접근 위협·기출동 여부를 검사하고 상태를 바꾸지 않는다.
- Confirm: 분대 상태를 `Stationed → Mobilized`로 바꾸고 배정 작업자를 생산 기여 중단 목록에 넣으며 같은 revision의 행위 기록을 남긴다.
- 멱등성: 같은 Command와 같은 분대·위협은 결과를 재사용한다. 같은 Command의 다른 분대·위협은 거부한다.
- 주체: `WorldDerived` 경보 판정이 발의하고 NPC 분대가 수행한다.
- 플레이어 영웅 참전, 식량·장비 소비, 교전, 사상자, 전리품, 안전 기간, 귀환은 별도 WI다.

## 논리·표현 요구

### Logic E1~E3

- E1: 분대·접근 위협·출동 상태·생산 기여 중단·WorldRevision 계약을 정의한다.
- E2: 순수 Domain Aggregate와 Application 저장소·Service가 Query·Preview·Confirm을 같은 규칙으로 실행한다.
- E3: Preview 무변경, Confirm 원자성, Command 멱등, revision·분대·준비·위협 거부 경계와 결정적 hash를 집중 시험한다.

### Presentation E1~E3

- E1: 플레이어가 분대의 대기/출동과 생산 기회비용을 읽어야 한다.
- E2: 권위 상태 사본에서 분대 카드를 읽기 전용으로 만든다.
- E3: 카드 StableId와 정렬, SourceWorldRevision, 배정 인원 수, 생산 중단 표시를 결정적으로 검증한다.
- 카드에서 Confirm을 실행하거나 Unity가 출동 여부를 계산하지 않는다.

## H 공간과 자산 요구

E4는 [Farm 병영 방위 E4 표현 연구](Farm병영방위-E4표현연구.r1.md)의 `Accepted` 기준선으로 외곽 경비 초소 H2 후보, 감시·집결 H1, Synty 조합, InteractionAnchor와 상태별 VisualKey를 동결한다. 실제 좌표·Prefab·Renderer·Collider·Rig는 E5 승인 전에는 결속하지 않는다.

## 저장·권위·외부 경계

- 새 Save 판본, RemoteHost API, WorldTick 자동 호출은 이번 묶음에서 열지 않는다.
- 운영 서버·외부 Provider·실제 Farm 생산량 계산을 호출하지 않는다.
- 생산 손실은 첫 WI에서 배정 작업자의 `생산 기여 중단` 상태만 남기며 수확량 수치를 직접 계산하지 않는다.

## 제외 범위와 승인

- 제외: 실제 전투, 영웅 조작, 사상자, 치료, 실패 결과 처리, 귀환, H 배치, Synty, Unity Scene, Play Mode, Game View.
- `WI-FARM-DEFENSE-MOBILIZE`: Logic E4 / Presentation E4 / 통합 E4, E5 승인 대기.
- `WI-SQUAD-ASSIGN`: Logic E3 / Presentation E3 / 통합 E3. 초소 슬롯 카드의 StableId·정렬·점유 상태까지 자동 검증하며 실제 UI는 제외한다.
- `WI-SQUAD-SUPPLY`: Logic E3 / Presentation E3 / 통합 E3. 식량·내구도 복구 능력의 동시 소비와 분대별 준비 카드까지 자동 검증하며 실제 보급 UI는 제외한다.
- `WI-FARM-DEFENSE-RESOLVE`: Logic E3 / Presentation E3 / 통합 E3. 전투 권위가 확정한 위협 감소·안전 기간·생산/회복 보정·전리품의 원자적 발현과 결과 카드까지 자동 검증하며 실제 전투·UI는 제외한다.
- `WI-FARM-DEFENSE-RETURN`: Logic E3 / Presentation E3 / 통합 E3. 초소 귀환과 치료·생산 재합류 후속 대기열 인계 및 결정적 귀환 카드를 자동 검증하며 실제 치료·재합류·이동 표현은 제외한다.
- 다음 후보: `WI-FARM-DEFENSE-RETURN`의 E4 판독 순간·InteractionAnchor·VisualKey를 승인하거나, 치료·휴식과 생산 재합류를 각각 별도 WI로 기획 승인한다.
