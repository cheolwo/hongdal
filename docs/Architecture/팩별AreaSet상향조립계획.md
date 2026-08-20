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
  ↓ 사람의 공간 설계 승인
승인된 위치 독립 H4 설계
  ↓ E5 지역 배치
권위 AreaSet 인스턴스
  ↓ 선택한 경우에만 E6
현실 결속 프로필의 공공데이터 계보 연결
```

팩은 주축 표현 자산을 뜻한다. 팩 이름·Prefab·GUID는 실제 공간 위치와 AreaSet 권위를 만들지 않는다.

DEM·도로·건물·블록 경계는 H1~H4와 AreaSet의 공통 필수 입력이 아니다. 현실 정합이 필요한 세계만 E6 정책과 프로필을 선택하고, 자료 부재는 그 프로필의 준비도로만 남긴다.

## 우선순위

| 순서 | AreaSet 후보 | 주축 표현 | 현재 상태 |
| --- | --- | --- | --- |
| P1 | Nature 생활·탐험권 | Nature | H2→H3→H4 설계 계보 준비 |
| P2 | Farm 생산·생존권 | Farm | H2→H3→H4 설계 계보 준비 |
| P3 | Town 생활·시장권 | Town | H2→H3→H4 설계 계보 준비 |
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

현재 결과는 위치 독립 설계 후보다. H 설계 승인에는 실제 지역 좌표·공공데이터를 요구하지 않는다. 실제 LandscapeGraph·GraphRelation·Unity Scene 배치는 E5, 공공데이터 계보 연결은 E6에서 수행한다.

## P2 Farm 상향 계보

```text
노출 점검·사건 격리·기상 보호 H1
  ↓
농장 사건 점검·격리 H2

사건 격리·손실 회복·복원 물자 H1
  ↓
농장 손실 회복·복원 인계 H2

두 H2
  ↓
농장 사건 격리·회복 H3

H3 + 고지대 생산 H3 + 생산·후처리 H3
  ↓
Farm 생산·생존 H4 AreaSet 후보
```

복원 물자 출력은 Nature AreaSet의 복원 입력 후보와 의미상 대응하지만, 양쪽 실제 연결점이 승인되기 전에는 GraphRelation이 아니다.

## P3 Town 상향 계보

```text
오염 점검·격리·정화 폐기 인계 H1
  ↓
생활권 오염 점검·정화 H2

회수 안내·근린 서비스·자연권 구호 H1
  ↓
생활권 회수 안내·자연권 구호 H2

두 H2
  ↓
생활권 오염 통제·구호 H3

H3 + 저층 생활·시장 H3 + 반품·회수 순환 H3
  ↓
Town 생활·시장 H4 AreaSet 후보
```

구호 물자 출력은 Nature AreaSet의 회복·복원 입력 후보와 의미상 대응한다. 실제 Town·Nature 양쪽 연결점과 GraphRelation이 승인되기 전에는 플레이어 이동이나 E5 공간 폐루프로 간주하지 않는다.

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
