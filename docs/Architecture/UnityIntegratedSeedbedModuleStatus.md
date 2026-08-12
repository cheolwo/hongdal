# Unity 통합 모판 대응 모듈 구현 현황

## 1. 문서 목적

이 문서는 2026-08-12 현재 서버와 별도 Unity 프로젝트에 구현된 **모판 대응 모듈**을 사용 가능 수준별로 정리한 상태 사본이다. 여기서 모판은 완성된 장면을 전시하는 곳이 아니라, 다른 장면에 선택적으로 배치할 건물·시설·가구·차량·화물·인물 배치 객체를 하나씩 확인하는 독립 미리보기 공간이다.

상세 고유 식별자와 데이터 연결은 [배치 객체 대장](UnityIntegratedSeedbedExhibitionObjectInventory.md), 승격 규칙은 [배치 객체 리팩토링 계획](UnityIntegratedSeedbedExhibitionObjectRefactoringPlan.md), 전시 업무 흐름과 서버 업무의 관계는 [통합 모판·전시관 제안](UnityIntegratedSeedbedExhibitionProposal.md)을 기준으로 한다.

## 2. 한눈에 보는 현재 상태

| 항목 | 현재 수량 | 판정 |
| --- | ---: | --- |
| 서버 구성 대장에 등록된 업무 흐름 | `EXH-0~5` | 전시 흐름·공개 범위 계약 구현 |
| 서버 구성 대장에 등록된 배치 객체 | 15개 | 고유 식별자·Pack·데이터 연결·승격 조건 등록 |
| Unity 프로젝트용 Prefab | 15개 | 배치 객체마다 독립 Root와 연결 지점 보유 |
| Unity 시각 자산 대장 항목 | 15개 | `obj-7a.r1` 대장에 등록 |
| 독립 모판에서 미리보기 가능한 배치 객체 | 15개 | O5 모판 실행 검증 완료 이상 |
| `SimulationWorldShell`에 배치된 객체 | 7개 | O6 실제 World 배치 검증 기록 보유 |
| 아직 대상 Scene에 배치하지 않은 객체 | 8개 | O5 모판 실행 검증 완료 유지 |
| 음식배달 추가 배치 객체 후보 | 배치 객체 3개·표지 객체 1개 | O0 후보, 아직 프로젝트용 Prefab 미등록 |

현재 상태를 짧게 표현하면 **“배치 객체 15개를 모판에서 골라 볼 수 있고, 그중 7개는 실제 통합 플레이 Scene에도 모듈식으로 심어졌다”**이다. O5는 모판 실행 검증 완료, O6은 특정 Scene·구역·배치 기준점·데이터 연결까지 확인한 실제 World 배치 검증 완료를 뜻한다.

## 3. 모판 대응 구조

```text
서버 구성 대장
    ↓ 고유 식별자, 승격 조건, 공개 범위, 데이터 연결
Unity용 데이터 변환
    ↓
현재 상태 사본
    ↓
시각 자산 대장
    ↓
프로젝트용 Prefab과 연결 지점·바닥 점유 영역·외곽 범위
    ↓
통합 배치 객체 모판에서 하나씩 선택·회전·검사
    ↓ 명시적인 O6 실제 World 배치 검증 기록이 있을 때만
SimulationWorldShell의 Farm·Logistics·Market·Town 구역에 배치
```

표현용 Prefab은 서버 상태를 소유하지 않는다. 배치 객체는 허가된 상태 사본을 보여줄 뿐이며, 주문·참여·배차·입고·재고·수령 같은 확정은 서버 Command와 기준 원장 재조회 경계에 남는다.

## 4. 공통 기반 모듈

| 모듈 | 구현 위치·역할 | 현재 상태 |
| --- | --- | --- |
| 공통 구성 대장 계약 | `Stories`, `SeedbedObjects`, `ScenePlacements`, 출처·검증 근거·공개 범위 | 구현·서버/Unity 왕복 검증 |
| 서버 조회 결과 생성기 | 업무 흐름과 배치 객체의 승격 조건을 판정하고 O6 배치 검증 기록 생성 | 배치 객체 15개·배치 7개 |
| Unity용 변환기 | 서버 DTO를 Unity가 읽는 현재 상태 사본으로 변환 | 서버 계약 일치 검증 |
| 모판 배치 객체 공통 Root | `SeedbedObjectRoot`가 고유 식별자와 시각 자산·배치 규격을 Prefab Root에 보존 | 프로젝트용 Prefab 15개 적용 |
| 시각 자산 대장 | 고유 식별자에서 프로젝트용 Prefab, 연결 지점, 바닥 점유 영역, 외곽 범위 조회 | 15개 항목 |
| 통합 배치 객체 모판 | 중앙 미리보기 구역에서 배치 객체 하나만 선택·회전하고 설명 표시 | 2열 15개 선택 UI·Game View 검증 |
| Scene 배치 검증 | Scene·구역·배치 규격 개정 번호·배치 기준점·데이터 연결 검증 | 7개 배치 |
| 검증기와 테스트 | 중복 식별자, 누락 연결 지점, 외곽 범위, 잘못된 데이터 연결, 권한 누출 차단 | 서버·Unity 변환기·EditMode 적용 |

### Unity 클라이언트 책임 분리 현황

2026-08-12의 `UNITY-CLIENT-REFACTOR-1`에서는 통합 전시관 Presenter가 예행 연습용 상태까지 직접 만들던 책임을 분리했다. 예행 연습 상태 생성은 Runtime의 `통합전시관FixtureApiModelFactory`가 담당하고, Presenter는 전달받은 `통합전시관Snapshot`을 선택·표시하는 역할만 맡는다. 따라서 현재 화면은 기존 예행 연습 상태로 그대로 실행하면서도, 다음 단계에서 시뮬레이션 서버의 관점별 조회 결과를 Unity용 변환기로 바꿔 같은 Presenter에 주입할 수 있다.

운영 서버와 게임 세계·예행 연습 서버의 UnityWebRequest API Client도 각각 자신이 사용하는 서버 주소만 검증하도록 분리했다. 한 서버의 주소가 잘못됐다는 이유로 다른 서버용 Client 생성까지 실패하지 않는다. 전체 실행 설정을 구성할 때는 기존 `UnityClientRuntimeOptions.Validate()`가 두 주소, 상세 페이지 주소, 실행 모드와 예행 연습 데이터 허용 정책을 함께 검증하므로 전체 구성 안전성은 유지된다.

아직 구현하지 않은 부분은 시뮬레이션 서버에서 통합 전시관 상태 사본을 실제 HTTP로 조회하는 저장소, 로딩·재시도·취소 처리, 서버 개정 번호가 뒤바뀌었을 때 기존 화면을 보존하는 처리다. 이번 리팩토링은 이를 연결할 주입 경계까지만 준비했으며 실운영 API 호출이나 업무 확정 기능은 추가하지 않았다.

`UNITY-CLIENT-SIM-LOAD-1`에서는 위 경계의 다음 단계를 구현했다. Unity의 `통합전시관SimulationSessionRepository`가 기존 `GET /api/simulation/v1/sessions/{sessionStableId}`에서 세션 고유 식별자, 시나리오, 세션·World 개정 번호, WorldTick, 게임 날짜와 실행 모드를 읽는다. 응답이 Simulation이 아니거나 실운영 상태를 포함하면 거부한다. 모판의 정적 구성과 서버 세션 상태는 하나의 기준 원장처럼 합치지 않고 `통합전시관ServerBoundSnapshot` 안에서 표시 상태 사본과 세션 연결 상태로 분리한다.

조회 조율 계층은 취소된 응답을 화면에 적용하지 않고, 이미 채택한 세션 개정 번호보다 낮은 응답을 거부한다. 새로고침이 실패하면 마지막으로 성공한 상태 사본과 개정 번호를 유지하고 `RefreshError` 상태를 남기므로 사용자가 명시적으로 다시 조회할 수 있다. Presenter는 이 서버 연결 상태 봉투를 주입받을 수 있으며 세션 개정 번호를 별도로 보존한다.

아직 저장된 통합 전시관 Scene에는 이 조율 계층을 만드는 Composition Root가 연결되지 않았다. 따라서 이번 단계는 저장소·조회 조율·Presenter 주입 계약까지의 코드 검증이며, 실제 Play Mode HTTP 호출 증거는 아니다. 다음 단계는 `UnityClientRuntimeSettings`, 세션 선택과 생명주기 `CancellationToken`을 Composition Root에 연결하고, 서버가 꺼진 상태와 실행 중인 상태를 각각 Game View와 Console로 검증하는 것이다.

## 5. O6 실제 World 배치 검증 완료 모듈 7개

이 표의 배치 객체는 모판 미리보기와 별도로 `SimulationWorldShell` 배치까지 검증됐다.

| 모듈 | 고유 식별자 | 배치 구역 | 현재 데이터 연결 의미 |
| --- | --- | --- | --- |
| 감자 수확 상자 | `seedbed-object:farm.potato-harvest-box.a` | Farm | HarvestLot·HarvestCargo 읽기 표현 |
| 농장 출하 Pallet Crate | `seedbed-object:farm.pallet-crate.a` | Farm | 출하 대기 Harvest Cargo 표현 |
| Hub 입고 Gate | `seedbed-object:town.hub-inbound-gate.a` | Logistics | 화물 이동·Hub 입고·창고 인계 경계 |
| 화물 배송 차량 | `seedbed-object:town.delivery-truck.a` | Logistics | 화물 이동·운송 작업 표현 |
| 공용 화물 Pallet | `seedbed-object:shared.cargo-pallet.a` | Logistics | Cargo·입고·창고 인계 표현 |
| 도심마트 Shop | `seedbed-object:city.urban-market-building.a` | Market | 공개상품·개인정보 제거 수요 신호 표현 |
| 집단수요 Cart Table | `seedbed-object:town.grouping-cart-table.a` | Town | 개인정보를 제거한 집단화 미리보기·공개 집계 표현 |

도심마트 Shop에는 공개 상품 데이터(`MartPublicProduct`)만 연결되어 있고 후방 마트 재고(`MarketInventory`)는 연결하지 않았다. 농장 crate와 물류 차량도 실제 상차·배차·운송을 확정하지 않는다.

## 6. O5 모판 실행 검증 완료 모듈 8개

다음 배치 객체는 프로젝트용 Prefab, 시각 자산 대장, 필수 연결 지점, 바닥 점유 영역, 외곽 범위와 독립 Game View를 확인했지만 아직 특정 대상 Scene의 배치 검증 기록이 없다.

| 모듈 | 고유 식별자 | 준비된 데이터 연결 | 남은 작업 |
| --- | --- | --- | --- |
| 음식 픽업 인계 상자 | `seedbed-object:shared.food-pickup-handoff-box.a` | RestaurantPreparation·DriverAssignment·FoodPickupHandoff | 음식배달 Scene과 별도 수령 경계 배치 |
| 농장 온실 | `seedbed-object:farm.greenhouse.a` | CultivationEnvironment·FarmEnvironmentalGrowthTurn | Farm 환경 anchor 선정 |
| 감자 밭고랑 | `seedbed-object:farm.potato-row.a` | FarmSoilTile·SoilObservation | 단일 토양 tile placement 검증 |
| 감자 재배체 | `seedbed-object:farm.potato-plant-visual.a` | CanonicalProductCultivation·FarmEnvironmentalGrowthTurn | 생육 Visual variant 배치 |
| 밭 관수 스프링클러 | `seedbed-object:farm.irrigation-sprinkler.a` | FarmEnvironmentalGrowthTurn·AgriculturalWeatherObservation | 관측과 시뮬레이션 혼합 방지 배치 |
| 주민 관점 Visual | `seedbed-object:town.resident-visual.a` | IndividualIntent·OwnerAuthorizedPerspective | 실제 사람 식별 정보·개인 의향 비소유 상태로 Town 배치 |
| 운영자 전용 재고 Shelf | `seedbed-object:city.operator-inventory-shelf.a` | MarketInventory·ShelfTask | 운영자 권한 전용 구역 배치 |
| 마트 운영자 Visual | `seedbed-object:city.market-operator-visual.a` | MarketInventory·ShelfTask·MarketOperatorPerspective | 시각 표현과 권한 소유 분리 배치 |

운영자 전용 재고 Shelf와 마트 운영자 Visual을 주문 상품의 실제 적재 위치, 피킹 Tote와 포장 작업대로 연결하는 다음 설계는 [마트 주문 피킹·포장 World 설계](UnityMarketOrderPickingPackingWorldDesign.md)를 따른다.

다음 우선순위는 배치 수를 늘리는 작업이 아니라 [15개 배치 객체 심층 연구](UnitySeedbedObjectDeepStudyPriority.md)다. 첫 대상인 감자 재배체는 [재배 규모·생산 능력·품목 속성](UnityPotatoPlantScaleYieldProductAttributeStudy.md)의 세 갈래로 나누고, 대표 단위부터 순서대로 판정한다. 감자 이후 객체의 같은 질문 비교는 [배치 객체 수평 연구 대장](UnitySeedbedObjectHorizontalStudyMatrix.md)을 사용한다. 기존 `OBJ-7D 주민 관점 Visual` 배치는 연구 결과가 쌓일 때까지 보류한다.

## 7. 아직 배치 객체 모듈이 아닌 요소

| 요소 | 현재 분류 | 처리 원칙 |
| --- | --- | --- |
| 전시관 바닥·구역 Ground·MainPath | 배경 | 전시관 기본 틀이며 업무 Scene 이식 대상 아님 |
| 자료관 시청·모판 마을집 | 배경 후보 | 건물이 데이터나 거주 상태를 소유하지 않음 |
| 모판 연구대 | 미리보기 가구 | 업무 데이터 연결이 없으므로 대상 Scene 이식 금지 |
| 관측 구체·상태 표시등·권한 중심 표시 | 표지 객체 후보 | 출처·상태·권한 설명용으로 배치 객체와 분리 |
| 화물·주문자·음식배달 확인 지점 | 업무 흐름 표지 객체 후보 | 기준 상태를 소유하지 않는 선택용 겹침 표현 |

## 8. 아직 O0 후보인 음식배달 모듈

EXH-5 업무 흐름과 음식배달 상태 계보는 구현되어 있지만, 다음 시각 요소는 아직 15개 시각 자산 대장에 포함되지 않았다.

| 후보 | 예정 분류 | 주요 경계 |
| --- | --- | --- |
| 음식점 조리·픽업 건물 | 배치 객체 | 조리 준비 상태와 건물 시각 표현 분리 |
| Pizza 픽업대기 표지 | 표지 객체 | 조리·픽업대기 읽기 표현만 허용 |
| 음식배달 기사 차량 | 배치 객체 | 화물운송 차량 상태 규칙 재사용 금지 |
| 확정 음식배달 기사 | 배치 객체 | 기사 후보와 확정 기사 공개 범위 분리 |

기존 음식 픽업 인계 상자는 O5지만, 위 후보를 함께 O0~O5로 등록하고 음식점 준비·기사 배정·픽업·전달·주문자 수령을 서로 다른 상태로 유지하는 `OBJ-8A`가 필요하다.

## 9. 업무 흐름별 대응 수준

| 단계 | 전시 업무 흐름 | 현재 모판 대응 수준 |
| --- | --- | --- |
| EXH-0 | 기존 전시 후보 현황 대장 | 완료: 코드·테스트·실행 상태·실운영 연결을 분리 판정 |
| EXH-1 | 공통 구성 대장 | 완료: 업무 흐름·배치 객체·배치 계약 구현 |
| EXH-2 | 로비·자료관·Farm | 전시 업무 흐름 완료, Farm 관련 배치 객체 6개 중 O6 2개·O5 4개 |
| EXH-3 | 화물·Hub·창고 | 업무 흐름 완료, 관련 물류 배치 객체 4개 O6 실제 World 배치 검증 완료 |
| EXH-4 | 주문자 집단·도심마트 | 업무 흐름 완료, Shop·Cart 2개 O6·주민/Shelf/운영자 3개 O5 |
| EXH-5 | 음식배달 | 업무 흐름 완료, 인계 상자 1개 O5·나머지 후보 O0 |
| EXH-6 | 배치 객체 모판·배치 승격실 | 진행 중: 15개 모두 O5 이상, 그중 7개 O6 실제 World 배치 검증 완료 |
| EXH-7 | 권한이 확인된 실운영 인계 | 미완료: 로그인 초기화·실시간 관점별 조회 결과·Command 후 기준 원장 재조회 필요 |
| EXH-8 | 전통시장·수출입·정산·다품목 확장 | 미착수 |

## 10. 현재 사용자가 할 수 있는 것과 할 수 없는 것

현재 `통합Object모판`에서는 배치 객체 15개를 하나씩 선택해 외형, 고유 식별자, 시각 자산 키, 배치 규격, 바닥 점유 영역, 외곽 범위와 연결 지점을 확인할 수 있다. `SimulationWorldShell`에서는 O6 실제 World 배치 검증을 마친 일곱 객체를 Farm·Logistics·Market·Town 문맥에서 볼 수 있다.

반면 모판 자체에서 실제 주문, 공동구매 참여, 배차, 재고 변경, 음식 수령을 확정할 수는 없다. 또한 O5 모판 실행 검증을 마친 객체를 사용자가 임의로 모든 Scene에 배치하는 실행 중 건축 UI도 아직 없다. 현재는 개발자가 대상 Scene의 의미·권한·데이터 연결을 검토한 뒤 배치 검증 기록과 함께 한 객체씩 승격한다.

## 11. 다음 연구·구현 순서

1. `OBJ-STUDY-1`: 감자 재배체에 공통 10개 질문을 모두 적용
2. `OBJ-STUDY-2`: 감자 밭고랑을 재배체와 비교해 책임 중복 판정
3. 생산 묶음의 관수 스프링클러·온실 연구
4. 이미 O6인 수확 상자·출하 Pallet의 존재 이유와 권위 경계 역검증
5. 수평 연구 대장에 따라 물류→수요·마트→음식 픽업 인계 상자 순으로 같은 질문을 비교하며 심층 연구
6. 연구 결과를 근거로 `OBJ-7D` 또는 생산 객체의 O6 실제 World 배치 검증 재개

배치 객체 수를 늘리는 것보다 기존 O5 모판 실행 검증 완료 객체를 실제 사용 Scene에 하나씩 안전하게 승격하는 것을 우선한다. 새 배치는 항상 고유 식별자, Scene·구역, 배치 규격 개정 번호, 배치 기준점, 데이터 연결과 대표 Game View를 함께 남긴다.

## 12. 현재 검증 기준선

- Unity 배치 객체 모판: 프로젝트용 Prefab 15개와 2열 선택 UI, EditMode 5/5, Game View 확인
- Unity Scene 배치 검증: 7/7
- 기존 `SimulationWorldShell` 회귀: 10/10
- `UNITY-CLIENT-REFACTOR-1` 집중 검증: 통합 전시관 책임 분리 7/7, 두 서버 Client 독립 설정과 World 초기화 7/7
- `UNITY-CLIENT-SIM-LOAD-1` 집중 검증: 세션 조회·Simulation 경계·개정 번호 역행·취소·새로고침 보존 5/5
- Unity 전체 EditMode 회귀: 269개 중 268개 통과. 실패 1개는 연구 Scene 파일 수를 27개로 고정한 기존 테스트와 실제 29개 Scene의 불일치이며, 이번 Client 리팩토링 경로의 실패는 아니다
- 서버 조회 결과 생성기 집중 테스트: 24/24
- Unity용 변환기 집중 테스트: 25/25
- Unity 스크립트 재컴파일: 오류 0건
- Scene·Prefab·카메라·UI 변경과 Game View 변화: 없음
- 운영 API·시뮬레이션 API·외부 provider 호출과 배포: 수행하지 않음
