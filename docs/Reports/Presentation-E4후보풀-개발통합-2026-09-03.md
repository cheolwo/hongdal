# Presentation E4 후보 풀 개발 통합 결과

## 결과

- 기획 스레드에서 개발 스레드로 전달된 `PLAN-PRESENTATION-E4-POOL-001` 작업을 실제 수신해 원문 SHA-256과 `PLANNING.md` 기준선을 검사하고 구현·시험·생성 결과까지 처리했다. 메시지 수신뿐 아니라 작업 반환 경로까지 사용할 수 있는 상태다.
- `PLANNING.md` 표의 46개 기획을 정확히 한 번씩 분류했다: `FrozenCandidate` 10개, `ProvisionalRequirement` 23개, `NotApplicable` 13개다.
- 첫 수평 묶음은 Nature 4, Farm 4, Town 7, Hub 3, City 4로 총 22 WI다. 현재 판정은 Ready 0, Conditional 0, Blocked 22다.
- 이 결과는 후보 준비 상태이며 Unity 배치, Presentation E5, Evidence 승격, 실제 Game View 성공을 뜻하지 않는다.

## 구현

- 원본 대장: `eng/execution-ledgers/playable-loop-presentation-e4-candidate-pool.json`
- 관리기: `eng/execution-ledgers/manage-playable-loop-presentation-e4-candidate-pool.ps1`
- 기계 조회 결과: `docs/AI/generated/playable-loop-presentation-e4-candidate-pool.json`
- 사람용 상태판: `docs/AI/generated/playable-loop-presentation-e4-candidate-pool.md`
- 회귀 시험: `eng/tests/playable-loop-presentation-e4-candidate-pool.ps1`

관리기는 `Write`, `Check`, `Query`를 제공한다. 기획 문서의 현재 판본과 SHA-256, 46개 분류 완전성, WI·폐루프 대응, H 정의의 현행 catalog 포함 여부와 stable ID, Graph Map 식별자·판본, 배치 준비, VisualKey 중복, 자산 GUID·파일·meta SHA-256을 검사한다. `Ready`인 공간 항목은 동결 배치 맵과 검증 결과를 요구하고, Actor 항목은 Rig·Avatar·Clip·중단·귀환 계약을 추가로 요구한다.

기존 `presentationE4Preparation` 12개와 그중 Required VisualKey 22개, Synty 폐루프 모듈 4개는 읽어 검증하고 복제하지 않았다. Required 준비의 식별은 작업 명세·폐루프·WI·VisualKey를 함께 보존하며, 같은 WI에 존재할 수 있는 별도 NotApplicable 범위를 덮어쓰지 않는다.

## 첫 묶음 판정

- Nature, Town, Hub, City는 현재 폐루프의 승인 기획 관문과 E7 v2 작업 명세가 없으므로 E1에서 Blocked다. 요구만 기록했으며 VisualKey나 Prefab을 임의 동결하지 않았다.
- Farm은 승인 기획·기존 작업 명세와 네 개의 파일 기반 후보가 있지만 Blocked다. 현 Graph Map 판본 불일치, 배치 프로필의 `placementMapRef=null` 및 `validationResultCode=Blocked`, 정확 소비 연결·InteractionAnchor·Bounds·통행·가시성 Unity 근거가 남아 있다.
- Farm 후보의 Prefab 경로, GUID, 파일/meta SHA-256은 `C:/Users/user/ssalddel`에서 현재 파일과 대조했다. 이 정적 일치는 Prefab 적합성이나 실제 배치 성공이 아니다.

## 검증

- 후보 풀 전용 시험: 13사례 통과. 현재 자료 Write/Check/Query, 결정적 재생성, 상태·영역 조회, 오래된 기획 hash, WI 집합 불일치, catalog 밖 H, VisualKey 중복, 잘못된 GUID, 미승인 후보 동결, 배치 제약 없는 Ready, Actor 계약 없는 Ready를 포함한다.
- 기존 표현 모듈 회귀: 공통 8·조건 10·프로필 9, 연결 계약 42사례 통과.
- 기존 Synty 폐루프 모듈 회귀: Nature 모듈 4개 통과.
- 기존 Synty 조사 회귀: 24사례 통과. 자산 실검사와 Editor는 이 시험 범위가 아니다.
- `game-development-work-order-docs.ps1`은 기존 방법론 문구 `자산 후보 조사는 E4의 준비 책임`을 찾지 못해 실패했다. 이번 후보 풀 파일을 되돌리거나 검사를 완화하지 않았고, 공유 방법론 문서의 동시 변경과 분리된 기준선 문제로 남겼다.

Unity Editor, Scene, Game View, Play Mode, Blender, Save, 공개 API, Simulation 규칙은 변경하거나 실행하지 않았다. commit과 push도 수행하지 않았다.

## 남은 인계

- 기획 소유: Nature·Town·Hub·City의 승인 판본과 선택·대가·실패·회복을 확정하고 현재 폐루프 planning gate에 결속한다.
- 개발 소유: 승인 후 각 폐루프의 E7 v2 작업 명세와 `presentationE4Preparation`을 기존 계약으로 추가한다.
- 공간·자산 소유: Farm의 동결 배치 맵·지지·통행·가시성·정확 소비 연결을 검증하고, 나머지 영역은 승인된 요구 안에서만 후보를 조사한다.
- 현 `PLANNING.md` 표는 46행이지만 설명 문구 한 곳은 아직 45개라고 적혀 있다. 분류기는 실제 표 46행을 기준으로 하며 기획 소유 문구를 자동 수정하지 않았다.
