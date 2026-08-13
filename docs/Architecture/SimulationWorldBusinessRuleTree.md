# Simulation World 업무 규칙 집결 트리

## 한눈에 보는 구조

이 문서는 흐름도가 아니라, `Simulation World 파생 DB` 안에서 공간·시설·업무 규칙·표현 규칙이 어디에 속하는지를 보여 주는 구조 트리다.

```text
Simulation World 파생 DB
├─ 1. 공간 사실·해석 원장
│  ├─ 공간 실행·원본 계보
│  ├─ Tile / Area / AreaSet
│  ├─ 건물·공개 사업장 파생 node와 관계
│  └─ Unity 공간 변환·Terrain·Mask·배치 기준점
├─ 2. 시설 의미 대장
│  ├─ 대관령면 Farm
│  ├─ 진부면 Logistics Hub
│  ├─ 평창읍 Mart
│  └─ 평창읍 Restaurant
├─ 3. 시설 기능 대장
│  ├─ Farm: 생산 / 수확 / 포장 / 출하
│  ├─ Hub: 입고 / 검수 / 보관 / 상차 / 하차 / 출하
│  ├─ Mart: 입고 / 보관 / 진열 / 판매 / 주문
│  └─ Restaurant: 입고 / 보관 / 소비 / 주문
├─ 4. 업무 Simulation 규칙 대장
│  ├─ 생산: 수확물 판로 배정
│  ├─ 주문: 개인 주문
│  ├─ 마트: 재고 진열
│  ├─ 창고: 용량 예약 / 입고 검수
│  ├─ 물류: 거점 간 이동
│  ├─ 화물: 배차 / 운송
│  └─ 음식점: 식재료 주문
├─ 5. 객체–업무 규칙 연결
│  └─ 시설 + 필요한 기능 + 규칙 개정 + 범위 + 우선순위
├─ 6. Scenario 규칙 묶음
│  └─ pyeongchang-farm-hub-town-v1의 규칙과 적용 순서
├─ 7. 객체 표현 규칙 원장
│  └─ 공간 규칙 + 현재 Simulation 상태
│     └─ 기본 구성 키 + 동적 표현 의도 묶음 키
└─ 8. 독립 Synty·URP 표현 파이프라인
   ├─ 의미 기반 VisualKey / ProfileKey
   ├─ Unity 구성 대장
   └─ Prefab / Material variant / Shader / Volume / HLOD
```

## 각 층의 권위

| 층 | 저장하는 것 | 저장하지 않는 것 |
| --- | --- | --- |
| 공간 사실·해석 | 공공데이터 계보와 파생 공간 관계 | 주문·재고·배차 확정 |
| 시설 의미·기능 | 공간 node가 Scenario에서 맡는 역할과 가능한 기능 | 실제 영업 사실·허가·소유 관계 |
| 업무 규칙 대장 | 규칙 식별자, 개정, Engine 키, 입출력 계약, Parameter | 규칙 실행 코드와 현재 Session 상태 |
| 객체–규칙 연결 | 어떤 시설 기능에 어떤 규칙을 적용할지 | 실제 업무 완료 결과 |
| Scenario 규칙 묶음 | AreaSet별 규칙 목록과 적용 순서 | 운영 환경 자동 활성화 |
| 객체 표현 규칙 | 공간·Simulation 상태를 표현 의미로 해석한 결과 | Prefab 경로와 업무 상태 변경 |
| Synty·URP | 마지막 시각 자산·렌더링 결합 | 서버 사실과 Simulation 권위 |

규칙 실행 코드는 `Ssalddel.Simulation.Domain/Application`에 남고, 현재 수량·주문·화물 상태는 Simulation Session 원장이 계속 소유한다. 파생 DB의 규칙 대장은 어떤 개정의 규칙을 어떤 공간 객체에 적용했는지 재현하기 위한 관계·계보 원장이다.

## 첫 대표 세로 단면

`pyeongchang-farm-hub-town-business-rules.v1`은 시설 4개, 시설 기능 18개, 업무 Simulation 규칙 10개, 객체–규칙 연결 10개, Scenario 규칙 묶음 1개와 규칙 항목 10개를 저장한다.

시설은 공공데이터에서 실제 업종이 관측되었다는 뜻이 아니라 `Scenario` 근거로 배치된 역할이다. 모든 첫 규칙은 `SimulationOnly=true`이며 운영 주문·재고·배차·운송을 만들지 않는다.

## 물리 표

- `시뮬레이션월드_업무Simulation규칙대장`
- `시뮬레이션월드_시설의미대장`
- `시뮬레이션월드_시설기능대장`
- `시뮬레이션월드_업무Simulation규칙`
- `시뮬레이션월드_업무Simulation규칙Parameter`
- `시뮬레이션월드_객체업무규칙연결`
- `시뮬레이션월드_Scenario규칙묶음`
- `시뮬레이션월드_Scenario규칙항목`

하위 표는 모두 상위 규칙 대장을 외래키로 참조한다. 같은 대장 개정은 같은 공간 실행·공간 출력 SHA-256·규칙 대장 SHA-256일 때만 재사용하며, 같은 개정에 다른 내용이 들어오면 충돌로 거부한다.
