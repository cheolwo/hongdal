# Nature 자원·LandUse·건설 문답

## 식별

- 문답 고유 식별자: `inquiry:nature-resource-construction.r1`
- 이관 질문: `Q-036~Q-039`, `Q-051~Q-060`
- 상세 원문·조사 계보: [동결 통합 아카이브](../nature-night-day2.inquiry.r1.md)
- 상태: `Refining`

## 이 문서가 소유하는 질문

- 나무·환경 자원의 WorldTick 재생과 실시간 표현
- LandUse에 따른 Spawn 제외와 Preview·Overlay
- 폐야영지의 발견 연기·잔불·선택형 소형 거점 성장
- 청사진, 재료 단계 투입, 시공 기여, WorldTick 건설 단계
- 보편 건설 정책과 청사진·재료·시공·수리·해체 하위 WI
- 영역별 Synty Construction VisualProfile
- 중단·재개와 건설 HUD

## 현재 확정 기준

- 자연 토지는 재성장하지만 평탄화·건설·도로 LandUse 셀은 자원 Spawn에서 제외한다.
- 폐야영지는 연기 기둥과 `Smoldering` 화로로 발견하며 선택 가능한 소형 거점으로 성장한다.
- 청사진을 먼저 확정하고 재료를 여러 번 투입한다.
- 건설은 `청사진 → 재료 더미 → 기초·골조 → 외형 → 사용 가능`으로 표현한다.
- 플레이어·NPC의 실시간 도구 작업은 기여량을 만들고 WorldTick이 권위 단계를 전이한다.
- 중단된 프로젝트는 재료와 진척을 보존하고 작업자가 돌아올 때 재개한다.
- 상위 건설 활동은 공통 정책이고 실제 권위 변화는 하위 단일 책임 WI가 담당한다.
- 권위 단계는 공통이지만 Nature·Farm·Town·City는 서로 다른 Synty VisualProfile을 사용한다.

## 자산 조사 근거

- `PolygonConstruction` 584 Prefab에서 Plans·Clipboard·Survey, Cone·Barrier, Plank·Brick·Concrete·Rebar, Wood Frame·House·Scaffold, 손도구·발전기·믹서·크레인·잔해 표현 후보를 확인했다.
- Prefab은 권위 상태를 확정하지 않고 건설 단계의 표현 후보로만 사용한다.

## 다음 질문 후보

- 청사진 확정 자체의 비용과 동시 활성 프로젝트 상한
- 손상·수리·해체의 자원 반환 규칙
- 건설 단계별 실제 Animation과 Audio Profile
