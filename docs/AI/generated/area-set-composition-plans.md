# 역할 슬롯 기반 AreaSet 구성 패턴

H4 세계 의도를 기존 H3·H2로 번역하는 조립 설명서의 결정적 생성 결과다. 새 H 계층이나 Runtime 상태가 아니다.

- 기준안: `4` · 변형안: `4`
- 해결된 H3 배치: `32` · 연결: `32`

| 영역 | 종류 | 구성 패턴 | H3 | 연결 | 구조 상태 |
| --- | --- | --- | ---: | ---: | --- |
| CityHub | Baseline | `CITYHUB-ASET-COMP-01` City/Hub 기준 구성 01 — 입고·보관·출고 선형 | 4 | 4 | `Closed` |
| CityHub | Variant | `CITYHUB-ASET-COMP-01-V01` City/Hub 변형 01 — 격리·비상 우회형 | 4 | 4 | `Closed` |
| Farm | Baseline | `FARM-ASET-COMP-01` Farm 기준 구성 01 — 고지대 생산·후처리형 | 4 | 4 | `Closed` |
| Farm | Variant | `FARM-ASET-COMP-01-V01` Farm 변형 01 — 계절 출하 집중형 | 4 | 4 | `Closed` |
| NatureHome | Baseline | `NATURE-ASET-COMP-01` Nature 기준 구성 01 — 숲·하천 생활핵 순환형 | 3 | 3 | `Closed` |
| NatureHome | Variant | `NATURE-ASET-COMP-01-V01` Nature 변형 01 — 위협 고조·대피 우회형 | 3 | 3 | `Closed` |
| Town | Variant | `TOWN-ASET-COMP-01-V01` Town 변형 01 — 오염 통제·구호형 | 5 | 5 | `Closed` |
| Town | Baseline | `TOWN-ASET-COMP-01` Town 기준 구성 01 — 저층 생활광장·시장형 | 5 | 5 | `Closed` |

Markdown은 의도와 선택 이유, JSON은 실행 가능한 구성 권위, Unity는 검토·표현을 담당한다. 변형안 전환은 이번 범위의 Simulation 규칙이 아니다.
