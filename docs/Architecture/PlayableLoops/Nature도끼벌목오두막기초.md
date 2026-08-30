# Nature 도끼·벌목·오두막 기초 — 기존 폐루프 기획 연결 복원

## 식별과 근거

- 주제: `topic:nature-shelter-foundation.v1`.
- PlayableUnit: `playable-loop:nature-shelter-foundation.v1`.
- 판본: `nature-shelter-foundation.design.r1`.
- 상태: `Approved` (D-360, 2026-08-30). 기존 플레이어 약속의 문서 연결 복원과 D359 한 동작의 준비 범위 승인이다. 새로 전체 Nature를 승인하거나 미완료 연구를 Accepted로 만드는 문서가 아니다.
- 근거: [현행 폐루프 원장](../../../eng/execution-ledgers/playable-loops.json)의 동일 ID, [Nature 생존 생활 거점](../Nature생존생활거점세로조각.md), [벌목 동작 승인](Nature벌목동작-Blender제작승인.md) `nature-woodcutting-animation.design.r1`, SHA256 `A4A7E942D025372EC4063E0C8152A6ABB31633E92D22DAFD336A40D4B1DF6642`.
- 원장 관찰: planningGate는 NotStarted/문서 공란이지만 기존 Logic E7·Presentation E7·PlayClosed 증거가 있다. 이 결손을 새 주제 1:1 문서로 복원하며 기존 증거 ID·hash·검증 범위를 보존한다. 그 E7은 새 Blender 동작의 증거가 아니다.

## 플레이어 약속과 재미

도끼를 얻고 나무를 베어 오두막을 완성한 뒤 다시 안전한 선택 상태로 돌아온다. 기존 약속을 유지하며 이번 개선은 손·도끼·나무 접촉과 준비/타격/복귀가 자연스럽게 읽히도록 하는 것이다. 필수 농장·상점·멀티플레이 이동이나 새 전투 기술을 요구하지 않는다.

## 반복 폐루프

안전 빈터에서 도구 선택→도끼 획득→명시적 장착→나무 접근/작업 시작→입력을 유지하며 벌목 진행→완료 후 통나무 획득→기존 부지 선정/건설→오두막 출입→다음 활동 선택. 진행 중 취소하면 기존 계약대로 작업을 해제하고 다시 선택한다. 스윙 한 번은 벌목 Task 전체 완료와 동일하지 않다.

## 선택·대가·성공·실패·회복

- 선택: 현재 작업 가능한 나무·도구·부지와 계속 작업/취소를 선택한다.
- 대가: 현행 장착 능력·접근·작업 시간·건설 재료와 점유 규칙을 유지한다. 애니메이션 때문에 소모나 보상을 추가하지 않는다.
- 성공: 벌목 Task 완료에 따른 기존 나무/통나무 변화, CabinOperational 및 NatureSafeChoiceAvailable로의 반환이다.
- 실패/중단: 미장착·대상 소진·접근 불가·작업 충돌은 현행 Preview/Confirm 거부를 유지한다. WI-NATURE-12 및 기존 취소 조건에서 미완료 작업을 성공으로 처리하지 않는다.
- 회복/귀환: 작업 점유 해제 후 현재 실제 상태에 맞춰 이동/기본 자세 및 다음 선택으로 돌아온다. 새 체력 회복·취소 보상·피격 규칙을 여기서 만들지 않는다.

## WI 단일 책임 후보

현행 원장 ID를 재사용하며 새 ID는 만들지 않는다.

| 기존 WI | 책임 |
| --- | --- |
| WI-ACTOR-01 / WI-NATURE-05 | 물품 획득 / 도끼 획득 특화. 부모·특화를 이중 실행하지 않는다. |
| WI-ACTOR-02 | 장착 상태 변경 |
| WI-NATURE-06 | 장착 도구로 나무 벌목 Task 시작. 이번 동작 개선 대상 |
| WI-NATURE-18 | 벌목 통나무 줍기 |
| WI-NATURE-07 / WI-NATURE-08 | 부지 선정 / 건설 작업 시작 |
| WI-NATURE-09 / WI-NATURE-10 | 오두막 안/밖으로 이동 |
| WI-NATURE-12 | 진행 중 작업 취소 |

## 논리·표현 요구

논리는 기존 4초 벌목 Task·입력 유지·완료·취소·행위 기록을 보존한다. 표현은 D359의 준비→접촉→복귀·반복·중단 기준을 따른다. Animation Event로 권위 시간을 진행하거나 재고/보상을 생성하지 않는다. 단일 뼈 작성자·root motion·기존 fallback 충돌을 전문 연구와 개발 검토로 해소한다. 기존 E7과 새 동작의 미검증 상태를 분리하고 통합 E를 두 궤적 중 낮은 실제 증거로 판정한다.

## H 공간과 자산 요구

기존 `area-set:sim:pyeongchang:nature-home.v1` 및 ToolAccessible·HarvestWorkArea·ShelterBuildingSite·RecoveryWorkArea를 유지한다. canonical SimulationWorldShell의 기존 Actor·장착 도끼·나무 InteractionAnchor를 우선 사용하고 새 지형/건물 배치를 만들지 않는다. 원본 GUID/hash·실제 Rig·Clip·접촉 위치는 전문 담당이 읽기 조사 후 동결한다. 구매 팩 이름만으로 호환이나 실제 재생을 증명하지 않는다.

## 전문 심화 연구 판정과 재결속

| 분야 | 이번 변경의 판정 | 사유/준비 상태 |
| --- | --- | --- |
| 건물 | NotRequired | 기존 오두막 형태·규칙 변경 없음 |
| 공간 | NotRequired | 기존 부지·통행·시점 유지. 기존 공간 증거 보존 |
| 배치 | NotRequired | 새 배치 없음. 기존 나무 접촉 위치를 애니 연구에서 확인 |
| 애니메이션 | Required | `study:nature-woodcutting:animation.r1`, 개발 명세가 지정한 Nature벌목동작-애니메이션적용연구.md 작성/검토 중. 아직 Accepted가 아니며 hash 미결속 |

이 문서는 주제의 약속과 D359 연구·준비 작업 등록을 복원한다. Required 연구가 미완료인 동안 실제 자산 가공/Runtime 연결은 그 연구 관문을 유지하고 읽기 조사·연구 준비만 진행한다. 애니 담당의 실제 기준선과 D359 수용 기준을 개발이 검토해 Accepted/hash로 명세에 결속한 뒤 실행한다. 기획 파일 형식 때문에 같은 플레이 선택을 다시 질문하거나 연구 상태를 허위로 올리지 않는다. 연구 기준선과 실제 제작 범위가 달라지면 해당 범위만 다시 연다.

## 저장·권위·외부 경계

Solo LocalProcess / Hosted RemoteHost는 같은 Simulation Core를 사용한다. 기존 Save 판본·읽기/hash 호환·Session 개정·행위 기록은 변경하지 않는다. 기존 저장을 불러오거나 같은 상태를 다시 조회해 스윙 완료/보상을 중복 실행하지 않는다. 제작 원본 .blend와 Unity 전달 FBX는 별도 관리하고 공급사 원본·기존 dirty Scene을 보존한다. 실제 외부 구매·생성형 AI 자산 업로드는 하지 않는다.

## 제외 범위와 승인

이번 새 실행 범위는 D359의 기존 캐릭터 도끼 스윙 하나와 그 연결뿐이다. 신규 캐릭터 디자인·새 전투/생존 규칙·보편 재생 엔진 교체·다른 WI 애니메이션 전체 제작은 포함하지 않는다. 별도 D361 자산 대조는 조사·후보 우선순위 작업이며 이 승인에 소급 포함하지 않는다.

기존 E7 기록은 과거 검증 후보의 증거로 보존한다. 이번 전달 E5 상한과 promotion=false를 유지하며 제작물 미완료를 기존 E7 완료로 포장하지 않는다. 개발 담당이 planningGate·sourcePlanningDocumentRefs·Goal/작업 명세를 동일 판본/hash로 결속한다. 실제 입력·Game View 캡처는 공간 담당이 수행하고 개발 검토 후 기획에 반환한다. 원래 D359 원문/hash는 변경하지 않는다.
