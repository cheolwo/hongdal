# 팩별 AreaSet 상향 조립 계획

## 목적

Nature·Farm·Town·City/Hub를 하나의 AreaSet에 합치지 않고 각각 독립된 지역 세계 후보로 성장시킨다.

```text
H1 장소
  ↓
H2 블록
  ↓
H3 경관
  ↓
H4 AreaSet 후보
  ↓ 사람의 세계 의도와 현실 근거 승인
실제 AreaSet
```

팩은 주축 표현 자산을 뜻한다. 팩 이름·Prefab·GUID는 실제 공간 위치와 AreaSet 권위를 만들지 않는다.

## 우선순위

| 순서 | AreaSet 후보 | 주축 표현 | 현재 상태 |
| --- | --- | --- | --- |
| P1 | Nature 생활·탐험권 | Nature | H2→H3→H4 설계 계보 준비 |
| P2 | Farm 생산·생존권 | Farm | Nature 이후 조립 대기 |
| P3 | Town 생활·시장권 | Town | Farm 이후 조립 대기 |
| P4 | City/Hub 물류권 | City | Town 이후 조립 대기 |

기계 기준은 [`area-set-composition-priorities.v1.json`](../../eng/world-seedbeds/synty-bottom-up-inventory/area-set-composition-priorities.v1.json)이다.

## P1 Nature 상향 계보

```text
위협 감시·흔적 추적·긴급 후퇴 H1
  ↓
자연 위협 추적·대피 H2

복원 작업·안전 회복 H1
  ↓
자연 복원·안전 회복 H2

두 H2
  ↓
자연 생활·위협·회복 H3

H3 + 자연 탐색길·대피망 H3
  ↓
Nature 생활·탐험 H4 AreaSet 후보
```

현재 결과는 위치 독립 설계 후보다. 실제 지역 좌표·공공데이터·LandscapeGraph·GraphRelation·Unity Scene을 갖지 않는다.

## AreaSet 사이 관계

플레이어 이동은 Nature를 중심으로 한다.

```text
Nature ↔ Farm
Nature ↔ Town
Nature ↔ City/Hub
```

화물 흐름은 Nature 생활권과 분리한다.

```text
Farm → City/Hub → Town
```

이 관계는 양쪽 AreaSet의 승인된 외부 연결점이 준비된 뒤에만 실제 GraphRelation으로 승격한다.
