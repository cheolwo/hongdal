# 대관령 Farm WI 공간 폐루프 계획

> 단계 해석: 이 문서의 Graph Node·Edge·연결점 판정은 D-232 이후 공간 WI의 `SpatialAssembly` 증거다. `eng/world-seedbeds/wi-spatial-seedbeds/`의 위치 독립 정의와 함께 E4 실행 문맥·E5 세계 발현 판정에 입력되지만 공간 조립만으로 E4·E5를 완료하지 않는다.

이 문서는 대관령 Farm의 세계 상호작용을 현재 경관 Graph에 연결하기 위한 P1 승인 계획이다. 공간 역할은 장소의 의미를, 공간 능력은 그 장소에서 가능한 활동을 나타낸다.

## 현재 기준 사실

- 경관 Graph: `landscape-graph:sim:pyeongchang:daegwallyeong-farm.v1`
- 개정: `1`
- Graph SHA-256: `9c3b09c7fc59bd98a4a0102f6e08a1538e050aa2fbc3d2d0fc0becdb459ecc84`
- 현재 조립 결과: Node 5, Edge 3, 배치 5, 외부 연결점 0, 미해결 3
- `agriculture-region` 생산구획과 `scenario-farm-road` 농로는 존재한다.
- 작업마당, 상차영역, Farm Gate는 현재 Graph에 존재하지 않는다.
- AreaSet이 기대하는 동쪽 Farm Gate 연결점은 자료가 없는 `701:1145` 타일에 속하므로 생성되지 않았다.

## 공간 폐루프 판정

| WI | 공간 역할 | 현재 판정 | 이유 |
| --- | --- | --- | --- |
| WI-FARM-01 밭갈이 | 생산구획 | 폐루프 가능 | 후속 파종과 같은 생산구획을 사용한다. |
| WI-FARM-02 파종 | 생산구획 | 폐루프 가능 | 선행·후속이 같은 생산구획에 있다. |
| WI-FARM-03 재배 관리 | 생산구획 | 폐루프 가능 | 후속 수확과 같은 생산구획을 사용한다. |
| WI-FARM-04 수확 | 생산구획 | 미해결 | 수확 Lot을 넘길 작업마당이 없다. |
| WI-FARM-05 집하 | 작업마당 | 미해결 | `farm-work-yard` Node가 없다. |
| WI-FARM-06 포장 | 작업마당 | 미해결 | `farm-work-yard` Node가 없다. |
| WI-LOG-01 상차 | 상차영역 | 미해결 | `farm-loading-bay` Node가 없다. |
| WI-LOG-02 Farm 출발 | Farm Gate | P2 대기 | Gate Node와 Farm–Hub 쪽 연결점이 모두 필요하다. |
| WI-WORLD-04 시설 수리 | 작업마당 | 미해결 | 작업마당이 정비 활동을 자연스럽게 수용하는지 후속 검토한다. |

## 용량 해석

`WorkArea = 1 slot`은 한 사람이 설 수 있는 물리 면적이 아니다. 해당 WI가 동시에 한 건만 공간을 점유할 수 있다는 Simulation 작업 용량이며 근거는 `ReviewedDesign`이다. 실제 사용 가능 면적이 확보되면 별도 용량 산정 규칙 개정으로 다시 계산한다.

## 신규 공간 배치 원칙

신규 Node의 절대좌표를 임의로 작성하지 않는다. 작업마당은 생산구획과 상차영역 사이, 상차영역은 농로와 인접, Farm Gate는 Farm–Hub 회랑 방향이라는 관계만 승인한다. 공간 파생 자료로 후보를 결정할 수 없으면 계속 미해결로 남긴다.
