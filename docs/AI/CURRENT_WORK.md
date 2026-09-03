# Mirror(거울) Current Work

> 2026-09-03 기준 최신 상태판이다. 완료 이력은 각 기획·Architecture·보고서와 Git 이력에서 읽고, 장기 결정은 [DECISIONS.md](DECISIONS.md)를 따른다.

## 현재 기준선

- 현행 기획은 [PLANNING.md](PLANNING.md)의 `PLAN-*` 47개가 소유한다. 일반 기획 답변은 새 `D-###`로 만들지 않고 해당 기획의 판본을 올린다.
- 기획 문답은 `지금·여기·나·너·이렇게·결과·다음 선택`을 사용한다. `이렇게`는 상황 안에서 플레이어가 고르는 적절한 행위이며 유일한 정답이나 성공 보장이 아니다.
- WI 오행은 105개 전부 분류됐다. 이는 행위·대상·상생/상극을 읽는 메타데이터이고 권위 상태·보상·E 승격을 대신하지 않는다.

## 원자 E1 조립

- [원자 E1 색인](generated/playable-loop-planning-e1-index.md)은 기획 47개, PlayableUnit 20개, 검토 원자 모듈 11개, 원자 후보를 정리한 기획 34개·후보 182개를 포함한다.
- 현재 주 폐쇄 대상은 `play-transaction:hans-farm.till-one-plot.v1`이며 기존 `WI-FARM-01`과 `playable-loop:farm-crop-cycle.v1`을 재사용한다.
- Farm 공동 준비 묶음은 `WI-FARM-01~06`을 E4까지 함께 준비하되 E5·E6·E7 증거는 WI별로 독립 판정한다.
- 운영 서버→Unity 기획은 Hub 입고 상태 사본 조회→검수→적치→권위 재조회 네 원자 후보로 정리됐다. 이는 실제 운영 Command나 Unity 배치를 활성화하지 않는다.

## Presentation E4 준비

- [E4 후보 풀](generated/playable-loop-presentation-e4-candidate-pool.md)은 기획 47개를 `Frozen 11 / Provisional 23 / NotApplicable 13`으로 구분하고 첫 묶음 WI 22개를 추적한다.
- 한스 숲 경계 농장 프로필은 배치 인스턴스 5개와 `h1-stock:farm-residential-home`을 포함한다. 재고 대응 5/5는 확인했지만 실제 후보 선정 0, Graph 통합 0, Unity·Blender 실행 0이므로 `Blocked`다.
- 공간 대장은 H1 85개, H2 39개, H3 20개, H4 6개를 생성·검증한다. H 정의와 Synty 후보는 실제 Prefab 적합성·Renderer·Collider·Bounds·입력 증거가 아니다.
- 24절기 생활 작업·복장·식생 조사는 정적 후보와 Animation/Blender 결손을 기록했다. 새 가공이나 Scene 적용을 승인하지 않는다.

## 운영 서버→Unity 선별 이관

- [운영 기능 이관 대장](generated/operational-unity-transfer-catalog.md)은 페이지 기능 241개, EF Core `DbSet` 271개, MongoDB collection 사용 지점 28개를 결정적으로 재생성한다.
- 기본 분류는 `PlayableAction 67 / ReadOnlyContext 111 / AmbientSimulation 59 / ServerOnly 4`다. H 대응은 검토 후보이며 DB 행이나 페이지를 H1로 자동 생성하지 않는다.
- 첫 기술 표본은 Hub 입고·검수·적치다. 현행 Presentation은 E1이고 다음 목표는 E4 준비이며, 실제 Prefab·World 배치·입력·같은 revision 관측 전 E5는 차단한다.

## 현재 차단과 미커밋 경계

- Graph Map 본체의 작업 사본은 r11이지만 기획 판정이 47개 중 22개뿐이고, partition·overlay 원본은 각각 r7·r5인데 본체는 r8·r6을 기대한다. 공식 Graph Map 검사는 `PlanningAssessmentCoverageCount`에서 중단된다.
- 따라서 Graph Map r11 본체·인계 대장·관련 도구와 문서는 이번 완료 커밋에 넣지 않는다. 25개 기획 영향 판정과 partition·overlay 판본을 같은 변경에서 닫거나, r11 변경을 더 작은 승인 단위로 분리해야 한다.
- Unity Editor·Play Mode·Game View·Scene 저장, 서버 실제 연결, 운영 DB 쓰기, Evidence 승격은 이번 정리에서 실행하지 않았다.

## 이번 검증된 커밋

- `a740e101` `docs(planning): consolidate canonical gameplay plans`
- `6c1c37f3` `feat(planning): assemble atomic E1 planning index`
- `d4c238db` `feat(metadata): classify inquiry depth and WI elements`
- `28ed5b97` `feat(spatial): prepare forest-edge farm placement profiles`
- `0221523c` `feat(presentation): manage E4 candidate pool`
- `e5692f25` `feat(integration): catalog operational Unity transfer`
- `fac571d5` `docs(research): record seasonal Synty presentation gaps`
- `687201d8` `fix(unity): add logging reflection metadata`
- `efb4ac14` `docs(governance): separate planning from decision history`

각 묶음은 `git diff --check`와 범위 Fast를 통과했다. 원자 E1, WI 오행, 공간·농장 배치 준비, Presentation E4, 운영 이관 전용 회귀가 통과했고 운영 이관 도구는 0경고·0오류로 빌드됐다. 원격 push는 하지 않았다.

## 다음 우선순위

1. Graph Map r11의 25개 미판정 기획과 partition·overlay 판본을 별도 Graph Map 작업으로 닫는다.
2. 한스 농장 `WI-FARM-01` 한 구획의 정확 안정 ID·접근·도구·입력·VisualKey를 동결해 E5 실행 명세로 인계한다.
3. Hub 입고·검수·적치의 H1/H2·Graph Map·배치 맵·Synty 후보를 E4에서 같은 판본으로 결속한다.
