# Farm 경관 LS01 공간·배치 연구

## 식별·상태·범위

- 연구 식별자: `study:farm-crop-cycle:landscape-ls01.r1`
- 연구 판본: `farm-crop-cycle.landscape-ls01-study.r1`, 2026-08-30.
- 상태: **Accepted — 비교·보존·거부 기준 및 다음 12자산 읽기 실측 절차에 한정**. D362의 경관 방향 아래 연구 기준선을 수용한 것이며, 자산 선정·구체 후보의 승인이나 구현 결과의 통과 판정이 아니다.
- 검토자: **개발** (`01a02198-8b2a-7491-ac93-366b30ff474c`), 2026-08-30. 연구/보고서 전체와 인계4파일 hash, 소비21/21·Prefab/meta34/34·재질7/7 직접 대조 후 위 제한 범위를 수용했다.
- 수용한 Draft SHA256: `742B21720396C09DD2B9712B37EC0F0D24476F8F3801698AD8D3D1AD107F50E4`. 본 수정은 이 Draft의 기술 공백과 검증 조건을 보존한 수용 상태 기록이다.
- 대상: `playable-loop:farm-crop-cycle.v1`, 기존 `WI-FARM-01~04`; 작업 `work:farm-crop-cycle:landscape-ls01-study`, Presentation E4 연구 준비.
- 소유: 월드·공간·배치. 개발이 검토·명세 재결속 후 기획에 반환한다. 원장·CURRENT_WORK는 이 연구의 쓰기 대상이 아니다.
- 원문: [LS01 문답](PlanningSessions/건물공간배치/landscape-composition.inquiry.r1.md), 파일명 r1/내용 r2. Q387~396 전체 읽기와 SHA256 `ABF57E4907FAC5A152EA08CFC5AAEAF24C172C0731809B0B6AAF7785131731EC` 일치 확인.
- 부모 기획: [Farm 경작 세계 발현](Farm경작세계발현E5.md), `FF9D86E660486E77DCEA1D6E6D8A69669C7C7D8A885966D4651486861D477FFC`.
- 부모 공간: [확대 연구 r2](Farm경작세계발현E5-공간배치연구.r2.md), `EC9AD2D5333102B22477A8063A6938C5CE15817A09D53900CC98C5EC8A78ACE5`. 별도 LS01 연구이며 이를 대체하지 않는다.
- 소비 명세: [farm-crop-cycle.e7-work-order.json](../../../eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json), 착수 SHA256 `F2EC6743AD1EDB0D2034E1F4ECEC3981075732C3C34EF0BA986FEC343301971C`. 착수 시 Required LS01은 Draft/빈 승인 hash였으며, 본 Accepted 문서의 최종 hash 재결속은 개발 담당의 후속 작업이다.

기존 Farm Logic E3/Presentation E1/통합 E1 및 E5 전달 상한을 보존한다. 이번 수용은 수관 fade·국소 빛·Sky·상태 공급의 지원 완료, 코드·조립·실제 통행·시각 승인·E 승격 근거가 아니다. 명세/hash 재결속 및 별도 Editor 슬롯 인계 전 12자산 실측을 시작하지 않는다. 현재 애니메이션 담당이 통합 컴파일·EditMode 시험을 점유 중이므로 공간은 Editor 대기 상태다. 새 후보·공통 코드·Scene 쓰기·공급자 변경은 이번 수용에 포함되지 않으며 이후 별도 배분이 필요하다.

## 이번에 확인한 사실과 아직 확인하지 않은 것

파일 조사만 수행했다. Editor 상태 조회·점유·자산 import·Play·Preview·캡처·새 조립은 하지 않았다. 애니메이션 담당의 D359 코드/컴파일 슬롯과 분리했다. 새 후보 이미지도 없다.

- 공유 Hongdal HEAD는 `1bc87323d8a70c9a09acdd0e0c32691b7186412c`; 착수 dirty108항목이다. 별도 detached worktree의 구형 기준이나 미커밋 내용이 자동 반영됐다고 가정하지 않았다.
- Prefab17개와 meta17개, 재질7개, 소비 소스21개의 읽기 기준을 수집했다. 목록·GUID·파일 hash·직렬화 Renderer/재질 참조는 [자산 조사 JSON](C:/Users/user/ssalddel/artifacts/local/validation/farm-landscape-ls01/disk-assets.json), [소비 기준 JSON](C:/Users/user/ssalddel/artifacts/local/validation/farm-landscape-ls01/consumed-baseline.json)에 있다.
- 기존 실측9개 Prefab/meta18개는 현재 디스크 hash와 모두 일치했다. 기존 실측 재사용이지 이번 Editor 재측정이 아니다. 새 나무·관목·돌·길·램프의 실제 활성 Renderer/Collider/Bounds와 색은 아직 미측정이다.
- D320 후보의 순수 배치 시험과 실제 canonical 반입은 다르다. 명세의 `e5Execution.statusCode=E5Pending`, Scene/GameView 증거 목록은 비어 있다. 기존 Nature 진단 화면이나 자산 단독 Preview를 LS01/Farm 성공으로 사용하지 않는다.

## Q387~392 보유 자산과 비교 의도

아래 경로는 Unity의 `Assets/Synty/` 기준이다. R은 YAML MeshRenderer 개수, MC는 MeshCollider 개수이며 실시간 활성/유효 형상 판정이 아니다. `MC0`이 모든 종류의 Collider 부재를 뜻하지 않는다. 공급자 원본을 고치지 않고 의미·소유·판본은 외부 배치 정의에 둔다.

| 문답·역할 | 실제 보유 후보 | 디스크 확인 / 크기 근거 | 배치 연구 의도·한계 |
| --- | --- | --- | --- |
| Q387·390 붉은 랜드마크 | `PolygonFarm/Prefabs/Buildings/SM_Bld_Barn_01.prefab`, 대체 `02` | 각각 R7/MC1, Farm 재질. 기존 Bounds XYZ Barn01=13.494843/9.105388/14.157824, Barn02=13.494843/9.105388/7.435112 | native Scale1 유지. Barn01 주 비교 안에서는 교체하지 않으며 대체02는 별도 비교 묶음. 색 판독은 같은 빛의 화면에서 확인 |
| Q387·389 큰/성긴 나무 | `PolygonFarm/Prefabs/Generic/SM_Generic_Tree_01.prefab` | R1/MC1, 크기 미측정 | Farm 계열 우선 탐색. 수관 전용 Renderer 없음. 밀도·위치는 실측 뒤 선정 |
| Q387·389 큰 나무 대조 | `PolygonFarm/Prefabs/Environments/SM_Env_Tree_Large_01.prefab` | R2/MC10; 다른 자식 이름 `SM_Env_Tree_Large_Swing_01` | 두 Renderer를 줄기/수관 분리로 오인하지 않는다. 그네 소품이 경관 의도에 불필요할 수 있어 우선순위 낮음 |
| Q389·393 대체 나무 조사 | `PolygonNature/Prefabs/Trees/SM_Tree_Birch_01.prefab` | R2/MC0, LOD 자식 및 줄기/잎 재질 참조3개 | 재질 슬롯 분리는 단서일 뿐 독립 수관 제어·Collider 적합성 미검증 |
| Q389 관목 | `PolygonNatureBiomes/PNB_Alpine_Mountain/Prefabs/SM_Env_Bush_01.prefab` | R4/MC0, Branches/Leaves LOD0/LOD1 자식 | 분리 구조 조사 후보. Farm와 색·규모 조화/LOD복원/반투명 지원 미검증. 별도 식생 능력 없음 |
| Q388·389·391 낮은 풀 | `PolygonFarm/Prefabs/Generic/SM_Generic_Grass_Patch_01.prefab`, 대체 `02` | 각각 R1/MC0. 01 기존 Bounds=.287026/.620244/.309168 | 이름이 Grass여도 높이 약 .62의 기존 실측: 급수점·발밑 판독을 가릴 수 있어 자동 채택하지 않음. 02 크기 미측정 |
| Q388·391 작은 돌 | `PolygonFarm/Prefabs/Generic/SM_Generic_Small_Rocks_01.prefab` | R1/MC0, 크기 미측정 | 군집과 여백을 번갈아 배치. 시각적으로 막힌 길을 Collider0 때문에 통과 판정하지 않음 |
| Q391 중앙 흙길/테두리 | `PolygonFarm/Prefabs/Environments/SM_Env_Road_Dirt_Straight_01.prefab`, `SM_Env_Road_GrassEdge_01.prefab` | 각각 R1/MC1, 크기 미측정 | 장식으로 통행 폭을 좁히거나 두 번째 지지/충돌 표면을 겹쳐 만들지 않음 |
| Q392 마당 눌린 흙 | `PolygonFarm/Prefabs/Environments/SM_Env_Dirt_01.prefab`, `PolygonFarm/Prefabs/Generic/SM_Generic_Ground_Dirt_01.prefab` | 각각 R1/MC1, 크기 미측정 | 토질 상태가 아니라 표면 표현 후보. 지형 자동 평탄화·숨김 금지 |
| Q392 경작 흔적 | `PolygonFarm/Prefabs/Environments/SM_Env_Dirt_Rows_01.prefab` | R1/MC1. 기존 Bounds=4.990549/.198393/5.068394 | 표시 외곽 약25.29㎡와 생산100㎡/H1 WorkArea 분리. 표면만으로 Tilled/성장 상태를 확정하지 않음 |
| Q388 급수 접근 기준점 | `PolygonFarm/Prefabs/Props/SM_Prop_Well_01.prefab` | R2/MC1. 기존 Bounds=1.601893/1.675407/1.414733 | 기존 물 접근 표현이지 실제 급수 생산원이라는 승인 아님. 하천 접근점과 혼동하지 않음 |
| Q396 소품 묶음 | `PolygonFarm/Prefabs/Props/SM_Prop_Crate_01.prefab` | R1/MC1, 크기 미측정 | 확정 상태 공급 후에만 장식 후보로 사용. 가상 재고/성과 생성 금지 |
| Q394 조명 외형 | `PolygonTown/Prefabs/Props/SM_Prop_Lamp_01.prefab` | R1/MC1/**Light0**, 크기 미측정 | 램프 모양은 보유했으나 조명 기능 증거가 아님. 형태 선정·소유 Light 연결은 별도 |

기존 실측은 Unity unit, identity wrapper/native Scale1에서 얻은 모델 외곽이다. 미측정 값은 0이나 추정 치수로 채우지 않는다. 이전 실측 전체 원본 hash `99F8B557D4DC8C2E6044D7D7D688B043CBF502F86C5370418F44D6EE09BE9CC8`, 재사용 묶음은 `6431605203D5477ED8C48AB375B3A19EDA1429BFCD1424D656E66A622003C8D5`다.

Farm 계열은 `PolygonFarm/Materials/PolygonFarm_01_A.mat` GUID `fdf18c58b725a2f469593ece1786d307`을 공유한다. Generic_Basic ShaderGraph 및 선택 재질은 `_Surface=0`, `_ZWrite=1`, AlphaClip 사용 상태다. 나무·관목에는 Trees/Foliage/LOD ShaderGraph와 다른 재질이 연결된다. 저장 속성/키워드에 alpha가 있다는 사실은 연속 반투명이나 현재 파이프라인 지원 증거가 아니다. 공급자 재질 색상·텍스처·shader를 수정하지 않는다.

## 같은 조건의 비교 기준선

기존 기준 A는 [01-Barn01.plan.json](../../../Ssalddel.Simulation.Tests/Fixtures/FarmH2ExpandedR2/01-Barn01.plan.json), `farm-riverside-h2.measured-expansion.r2 / UnapprovedCandidate`다. 파일 SHA `027CAABCF45A1BA013833476B741093857325004BE519764FB84FECDEB5E1578`, ResultHash `2d9a5c43075d6f96075fca0d67ac60ef5d8d74595d0a55e1506d1023bde7a720`는 서로 다른 의미다. InputHash `006370857707996d5b54d69bb3744d2154de0bba901893594b4213fe31b79bd4`; SurfaceHash `369b0d239a84b9751ea606bb6d6ec8690929dd3532346b7d2d46ca8238a33838`.

이 파일은 Flat 합성 지형/Seed `farm-flat-297`/SourceWorldRevision0의 후보이며 실제 Farm 상태 사본이 아니다. B는 Accepted 이후 만들 **LS01 경관 변화분 후보**다. B의 새 revision/입력·출력 hash는 미생성이며 기존 A를 덮어쓰지 않는다. 새 revision을 현재 Adapter가 받을 것이라고 가정하지 않는다. 공간 소품 변화분을 별도 읽기 형식으로 결합할지 현행 정규형으로 확장할지는 개발이 소유 경로·호환 시험과 함께 결정한다.

| 고정할 항목 | A/B 비교에서의 규칙 |
| --- | --- |
| 지형·하천·H소유 | 동일 높이 입력/hash·Cell→World 변환·보호마스크·실내제외. 평지 A/B부터, noise/단차도 각각 같은 표본끼리 비교. 급경사·보호 침범은 계속 거부 |
| Barn·생산·접근 | Barn01 Scale1/TRS/pivot와 D320 예약 외곽·기존 핵심 경로 보존. 생산100㎡/수확 규칙·수원·입출구를 바꾸지 않음. 새 장식 간격 때문에 부지 재확장이 필요하면 연구로 반환 |
| 카메라·표시 | 같은 지원 플레이 시점, 위치·회전·투영·FOV 또는 ortho size·해상도·품질/노출·후처리. 자유 카메라를 고정 연출로 바꾸지 않음. 캡처용 관찰 좌표는 재현 기록일 뿐 플레이 강제 경로가 아님 |
| 시간·Sky | 같은 권위 상태 사본의 revision/WorldTick/대기 프로필/주기시각/강수·구름·바람을 고정한 한 쌍. 낮/밤/비는 각 별도 쌍. 테스트 상태는 TestFixture라고 표시하고 실제 공급으로 위장하지 않음 |
| 자산·상태 | 동일 Prefab/meta/mesh/material fingerprint 및 원본 크기. 밭·Lot·WI 상태는 같은 Session/Target/Revision. 변화분의 StableId·VisualKey·자산계열·Seed·revision·hash를 구분 |

기존 entry(0,-40) → yard(0,-12) → work-path(0,12) → exit(0,40), 밭 접근(9,12), 우물 접근(-8,12), Barn 문앞(-4.5,-12)와 뒤편 외부 통로를 비교 관찰의 출발점으로 삼는다. 이는 후보 로컬 X/Z이며 실제 카메라/Player 월드 좌표가 아니다. 강변 첫 접근 관찰점은 하천 마스크/안전지지면과 위 변환을 대조한 뒤 선정해야 한다. 골격을 따라야만 이동 가능한 방식으로 만들지 않는다.

정지 비교는 원거리 지붕 노출, 중거리 마당/밭, 급수점, 길 가장자리, 뒤편 접근을 동일 구도로 남긴다. 전체/세부 화면마다 A/B·후보hash·지형hash·실제/시험 상태·Play여부를 표시한다. Game View 실제 입력 영상/연속 캡처는 별도 검증이며 정지 비교의 시각 승인으로 대체하지 않는다.

## Q393~396 지원 현황·누락·소유 제안

### Q393 수관 가림/복원 — 부분 기반만 있음

`Assets/Ssalddel/Presentation/World/DioramaForegroundOcclusionController.cs`는 카메라→focus SphereCast 결과의 `DioramaOcclusionView`를 찾는다. `SetOccluded`는 등록 Renderer 전체를 `enabled=!occluded`로 전환하고 복귀/OnDisable에서 다시 켠다. Collider를 끄지는 않지만 **옅게 표시가 아니며** 원래 비활성 Renderer 상태 보존도 별도 확인이 필요하다. 현재 카메라의 플레이어와 실제 작업 대상을 각각 보호하는 계약이라고 볼 수 없다.

GenericTree는 수관 독립 제어가 없고 LargeTree의 추가 Renderer는 Swing이다. Birch는 줄기/잎 material-slot 단서, Alpine Bush는 Leaves/Branches LOD 자식 단서가 있다. 메시 분할이나 재가공 없이 제어 가능한 범위인지 실측해야 한다. 현재 연구에서 성공/지원완료로 표시하지 않는다.

후속 소유 제안: 공간이 잎/줄기·LOD·재질 원본과 판독 시험을 제공하고, 개발이 가림 Controller/시점·대상 연결의 수정 담당을 지정한다. 필요하면 전용 wrapper/임시 재질의 가역 제어를 별도 승인받는다. 모든 나무를 숨기거나 Collider 해제로 우회하지 않는다. 분리 불가하면 시야 통로 확보는 Q387~392의 독립 대안일 뿐 Q393 완료가 아니다.

### Q394 국소 빛 — 전역 시간대 기반 있음, Farm 국소 연결 미확인

`월드시간대Presenter.cs`의 directional/ambient/fog/camera/surface 적용은 존재한다. 선택 Lamp는 Light0이므로 입구·Barn 문앞·현재 작업점 조명 지원을 증명하지 않는다. 실제 Farm 국소 Light의 설치·대상 연동과 UI 대비는 이번 파일 조사에서 확인하지 못했다.

개발이 시간대 출력과 작업 대상 공급자를 맡고 공간이 실제 조명 영역·그림자·통행 및 안내 판독을 검증하도록 경로를 나눠야 한다. 주변을 전부 밝히는 방식이나 새 화재·전력·안전 능력으로 연결하지 않는다. 범위·intensity·대비 숫자는 화면 측정 전 미정이며 게임 규칙으로 확정하지 않는다.

### Q395 Sky — 실제 코드 경로 있음, LS01 근거리 판독 미검증

`SkyEngineAutoBootstrap.cs` → `SkyEnginePresenter.cs` → `월드시간대Presenter.cs` 경로가 있다. Sky는 `Nature생존Controller.State.Atmosphere`를 Clone하여 구름/강수/시간대·안개·음향에 투영하고 WorldTick을 직접 진행하지 않는다. 비 길이/화면 점유 제한도 코드에 있다. 따라서 Sky 자체를 미지원이라고 쓰지 않는다.

하지만 이 공급과 Farm의 실제 같은 Session/Revision 결합, 선택 대상/발밑 경계의 비·안개 판독, 새로운 로컬 장식과의 조화는 미검증이다. 전역 Fog를 새 LS01 공급자가 덮어쓰지 않도록 기존 출력 소유자를 유지한다. 공간은 고정 Sky 사본별 비교, 개발은 상태 계보·출력 소유·근거리 제한 변경 경로를 책임진다. 현재 Editor의 연결 여부를 읽은 것은 아니다.

### Q396 생활 흔적 — 일부 권위 투영 있음, 누적 이용량 공급은 없음이 아니라 미확인

`오늘작업CanonicalStateData`에는 WorldRevision/WorldTick/WorkOrders/HarvestLots/PackageLots가 있다. `오늘작업계획Presenter.ApplyCanonicalState`는 WorkOrders가 가리키는 대상의 색과 HarvestLots 존재에 따른 기존 Lot 표시를 갱신한다. 누적 이용 횟수나 길 마모량 필드는 이 사본에 없다. 코드의 Lot 전체 존재 판정을 밭별 수량/재고 생성 근거로 재사용하지 않는다.

`오늘작업계획SceneCompositionRoot`는 LocalRuntime이 설치되면 `DailyWorkPlanLocalSimulationCapabilityWaiting`으로 서버 초기화를 중단한다. 다른 Local 연결이 있을 가능성을 배제하지 않되 이번 읽기에서 LS01 공급 완료를 확인하지 못했다. Core에는 `ISimulationFarmWorldInteractionRuntime` Preview/Confirm 포트가 있으나 포트 존재는 현재 Farm 화면 공급 증거가 아니다.

첫 표현은 개발이 공급한 같은 Session·Target·Revision의 확정 상태가 있을 때만 검토한다. 미공급/오래된 상태에서는 변화 없음과 준비 안 됨을 명시하고 물품을 무작위로 늘리지 않는다. 신규 이용·마모 규칙, 재고/수확량 추정, Model 수로 권위 재계산은 금지한다. 상태 공급/Mapper 소유는 개발, 공간은 장식 변화분·원상복구·판독만 담당하는 제안이다.

## 수용 기준과 거부 조건 — Accepted 검증 기준, 결과 통과 아님

1. Q387: 강변 안전 접근의 원거리 지붕→중거리 작업마당·밭 노출을 A/B 같은 관찰점에서 확인한다. 자유 후방 접근에서도 핵심 대상과 발밑이 읽혀야 한다. 강제 이동·카메라 전환이 필요하면 거부한다.
2. Q388: 급수 접근/상호작용 예약 영역을 비운다. 낮은 군집과 열린 구간이 교대하고 일정 폭 장식 띠가 주 하천 전체를 덮지 않아야 한다. 물/보호 마스크 침범은 실패다.
3. Q389: 실측 높이·외곽에 근거한 큰 나무→성긴 나무·관목→풀→밭의 전환과 통행/시야 여백을 판독한다. 이름만으로 높이 단계를 정하지 않는다.
4. Q390: 동일 광원·노출에서 Barn이 핵심 색 강조로 읽혀야 한다. 소품 원색이 경쟁하면 개수/위치/후보를 검토하되 공급자 재질을 몰래 재색칠하지 않는다. 사람 화면 검토를 기록한다.
5. Q391~392: 중앙 길/테두리, 경작 흔적/마당/초지가 구분되고 기존 경로폭·Collider/지지면을 침범하지 않아야 한다. 겹침·부유·매몰·급단차·z-fighting을 실패로 남긴다. 표시 면적을 생산100㎡로 계산하지 않는다.
6. 수치 제안은 `조정 가능한 시험값`으로 표시한다. 기존 r2의 통로3m/Player 여유 약.60m는 해당 동결 후보의 입력이지 모든 Farm의 새 규칙이 아니다. 면적·생산·하천·H소유 변경이 필요한 해법은 이 연구 범위 밖이다.
7. Q393: 실제 가림 수관만 옅어지고 시야 해소·시점전환·대상변경·비활성/해제 때 원재질/속성/LOD가 복원돼야 한다. trunk/Collider·위험 판독·권위 상태는 동일, 다른 인스턴스/공유재질 오염은 실패다. 현재 코드는 통과 증거 없음.
8. Q394~395: 같은 후보의 낮/밤/비에서 입구·문앞·현재 작업 안내·발밑이 판독되고 바깥의 명암은 유지돼야 한다. 그림만 밝게 보정하거나 Sky 상태를 위조하지 않는다. 실제 성능/정렬·그림자 품질은 미측정으로 시작한다.
9. Q396: 확정 상태→선택된 장식 변화가 같은 revision으로 추적돼야 한다. 중복/오래된 사본·재진입·LH해제/재활성에서 권위 수량 불변/재추첨 없음. 상태 공급 부재는 Blocked이며 시험값으로 가리지 않는다.
10. 같은 입력/순서변형에서 배치 정규형 hash·Partition→LH 수명주기가 보존돼야 한다. 기존 Adapter의 지지면·경사·높이편차·겹침·통로·입출 연결·소유/손상hash 거부를 재사용한다. 합성 표본과 실제 연속 이동은 별도 검증한다.

## 실제 실행 전 차단과 후속 슬롯 요청안

현재 Player 소스는 `UsesStreamingTraversalCoverage`가 true이면 `ClampToTraversalBounds`를 건너뛴다. 과거 X±30.5/Z±22.5 대 후보 Z±40 차이는 보존하되 현재에도 같은 제한이라고 단정하지 않는다. 실행 전 실제 movementGate/coverage, Local↔World 변환, 안전지지·활성 셀을 읽어 대조해야 한다. 자동 clamp 확장이나 후보 축소로 해결하지 않는다.

다음 읽기 슬롯은 **아직 요청 승인/점유하지 않았다**. 제안은 다음과 같다.

- 구체 대상: 위17개 중 미측정 나무3·관목1·Grass02·돌·길2·마당2·Crate·Lamp 총12개. 기존 Barn/Rows/Grass01/Well 5개는 fingerprint 대조만 우선. 필요한 경우 실제 Collider 유형/mesh 유효성도 함께 읽는다.
- 목적: native TRS/pivot, Renderer별 local Bounds, Mesh/submesh/material-slot/LOD 연결, Collider shape·유효성, Light 유무와 실제 외형 PNG. 수관 alpha 성능 시험이나 새 동작 코드 작성은 별도 허용 전 제외.
- 시작 조건: 개발/애니의 슬롯 해제 및 승인 후 ready/stopped/compilefalse, open/saved Scene·dirty·선택·Preview·Console cursor와 원본/의존 hash 기록. 이번 문서가 점유 허가는 아니다.
- 절차: 읽기 자산에 스크립트가 있으면 중지/검토. 소유 Preview만 생성하고 native 인스턴스 측정·렌더 후 finally 정리. 기존 Preview/Scene/선택/재질 원본을 변경하지 않는다. 전체 씬 저장·reload·reimport·Play 없음. modal/다른 점유/예상외 dirty 발생 시 조작 중단 후 반환.
- 종료: 전후 Scene·dirty·선택·Preview·원본 hash·Console 대조, 이미지와 측정/미검증을 `artifacts/local/validation/farm-landscape-ls01/`에 반환. 과거 정상 상태를 현재 상태 보장으로 쓰지 않는다.

수관·Sky·국소조명 코드는 현재 쓰기 예약이 없다. 후보 수용 뒤에만 개발이 기존 공통 소스의 담당을 배정하고, 필요 전용 공간 경로를 따로 예약한다. 연구→자산 실측→비중첩 비교 후보→기획 화면 검토→canonical Game View/실제 상태·입력·Console/저장재진입 순으로 증거를 분리한다. D320 실제 반입 미완료를 LS01 정지 비교로 닫지 않는다.

## 무효화·반환

연구/명세·후보·지형·Player/카메라·자산/재질/LOD·Sky/상태 공급 판본 변경은 관련 비교 기준을 무효화한다. 같은 비교 묶음 중 하나만 새 판본으로 바꾸지 않는다. 승인 전 기존 경관 선택을 재질문하지 않고 기술 공백과 필요한 소유 경로를 개발에 반환한다. 상세 파일/검증 기록은 [전문 인계](../../Reports/Farm경관-LS01-전문인계-2026-08-30.md)를 따른다.
