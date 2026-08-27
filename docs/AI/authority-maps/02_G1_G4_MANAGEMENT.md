# G1~G5 관리 체계 지도

이 파일은 전달 묶음 안의 빠른 진입점이다. 상세 정의의 단일 기준은 [E 성숙도와 G 관리 체계](../../Architecture/E성숙도와G관리체계.md), 기계 기준은 [`evidence-management-systems.json`](../../../eng/execution-ledgers/evidence-management-systems.json)이다.

| 체계 | 주 구간 | 관리 질문 | 대표 작업 |
| --- | --- | --- | --- |
| G1 세계 성립 관리 | E1→E6 | 이 플레이어 약속과 세계가 제대로 성립했는가? | WI, 권위 상태, H1~H5, AreaSet, Graph, Revision·Hash·Lineage, 필요한 현실 근거 |
| G2 플레이어 경험 관리 | E6→E7 | 사람이 실제 입력으로 폐루프를 완주할 수 있는가? | HUD, 입력, 카메라, 이동, Preview·Confirm, `SimulationWorldShell`, Play Mode, Game View |
| G3 개별 폐루프 안정 관리 | E7→E8 | 같은 폐루프를 반복해도 동일하고 복원 가능한가? | 결정적 반복, Save/Restore/Replay, LocalProcess·RemoteHost 동등성, 실제 입력 재진입 |
| G4 영역 조화·사람 승인 관리 | E8→E9 | 안정된 Core 폐루프들이 한 영역에서 조화롭고 사람이 승인할 만한가? | Core 인계, 공간·시간·자원·회복 조화, 조건부 NPC 연속성, 사람 평가, 후보 승인 |
| G5 제한 운영 관리 | E9→E10 | 승인 후보를 제한된 운영창에서 안전하게 관찰할 수 있는가? | 불변 build, 완주·오류·rollback 관찰, 계속 운영 승인 |

## 새 아이디어 routing

1. 세계의 의미·규칙·공간·근거를 바꾸면 G1부터 본다.
2. 플레이어 입력·UI·카메라·피드백을 바꾸면 G2다. 권위 계약이 함께 바뀌면 G1도 다시 연다.
3. E7 후보의 반복 결정성·저장 재진입·Host 동등성을 검증하면 G3다.
4. 같은 영역의 E8 Core 둘 이상을 이어 보고 사람 평가로 후보를 승인하면 G4다. NPC는 관련 묶음의 조건 모듈이지 E8 전체의 동의어가 아니다.
5. 승인된 E9 후보를 판본화된 제한 운영창에서 관찰하면 G5다.
6. G 작업 상태는 E 증거가 아니다. 최종 완료는 [E1~E10 정의](01_E1_E9_DEFINITION.md)의 해당 관문으로 판정한다.

## 변화 영향 routing

변경 영향·Migration·호환·회귀는 G4나 E9만의 책임이 아니다. 의미 있는 변경은 현재 목표 E7에서 E1까지 영향을 내려가 검토하고, 가장 낮은 미완료 의존성을 구현한 뒤 E1부터 다시 조립한다. E7을 통과한 후보만 E8 안정성 캠페인으로 인계하며, 같은 영역의 안정 Core가 둘 이상일 때만 E9 조화 후보를 연다.

과거 `.e9-work-order.json`과 E8 NPC·E9 변화 적응 문서는 `legacy-change-adaptive.r10` 호환 기록이다. 새 승격에는 [현재 수직·수평 증거 체계](../../Architecture/E1-E7수직폐루프와E8-E10수평증거체계.md)를 사용한다.
