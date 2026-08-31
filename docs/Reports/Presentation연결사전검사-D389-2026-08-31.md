# D389 표현 연결 사전검사 — 코드·독립 시험 인계

## 결과와 승인

[승인 원문](../AI/Presentation-E4-E5연결사전검사-2026-08-31.md) `presentation-connection-preflight.design.r1`, SHA `AD2300CBA2DE8BB11624F42FF9996EC0D5BD150D95335D301F2771BBEF7D3399`를 전체 읽고 읽기 전용 사전검사와 첫 Farm 소비자를 구현했다. 새 대장·E축·범용 조립 실행기는 없다. **코드/독립 관측 시험과 실제 Farm 제품 연결은 다르며 실제 E5는 미연결**이다.

기준은 [Farm 명세](../../eng/execution-ledgers/work-orders/farm-crop-cycle.e7-work-order.json)의 Approved 기획·Accepted 공간 연구 r1/r2 및 [D388 후보 조사](Presentation최소모듈-D386기술통합-2026-08-31.md)다. 구현 전 같은 명세에 D389 승인·정확6파일/기존 소비 API·실행 제외 범위를 결속했다. 기존 D386 증거가 참조하는 소스/보고/시험12파일은 수정하지 않았다.

## 정확 코드와 재사용

| 경로 | 역할 |
| --- | --- |
| `Ssalddel.Unity/Runtime/PresentationContracts/표현연결Preflight.cs` | 고정 연결 항목의 E4 요구와 관측 비교, 불변 입력/결과·진단·판본 지문 |
| `Ssalddel.Unity/Runtime/Farm/Farm수확표현연결Preflight.cs` | 이미 준비된 `Farm수확상태PresentationState`의 대상/Session/상태·표현 판본/Slot/상태명 재대조 후 공통 검사 소비 |
| `Ssalddel.Unity.Tests/표현연결PreflightTests.cs` | 순수 Fixture/첫 Farm 소비·방문자 준비 입력 회귀. 각 소스의 `.meta`만 추가 |

공통 코드는 netstandard2.1/C#9 패키지 안에 있고 UnityEngine/Editor/File/Scene/Delegate 실행 의존이 없다. 내부 표현 계약은 패키지 소비를 위해 public C# 타입으로 제공하지만 공개 게임 API/Simulation 계약/Save를 바꾸지 않는다.

- 기존 `Farm수확상태PresentationPreparation.TryPrepare`와 `StableIdReconciler`/표현 판본 계산은 그대로 사용한다. 새 검사에는 이미 준비된 불변 상태만 넘기므로 검사 실패가 준비 소비자의 `Current`를 바꾸지 않는다.
- `WorldVisualCatalog.Resolve`는 기존 실제 조회 책임으로 보존한다. 새 코드는 다른 대장을 만들지 않고 그 조회의 확인값/실패/미조회 관측을 받는다. 이번에는 Editor/API 조회 어댑터를 실행하거나 새로 만들지 않았다.
- 기존 표시 lease의 생성·표시·구독 소유 범위와 취소/전환 해제 범위는 공급 관측으로 비교한다. lease를 획득/해제하거나 Renderer/Scene을 쓰지 않는다. 실제 소유 해제 성공 시험을 한 것은 아니다.
- 방문자 `방문자체류PresentationPreparationProjector`의 대상·VisualKey·후보·판본·H 기준점을 두 번째 시험 입력으로 사용했다. 방문자 Runtime 소비자·이동/Actor/Scene은 구현하지 않았다.

## 검사 계약

`표현연결Plan`의 `PreparationRevision`과 `Requirement` 목록은 기존 E4 준비를 읽어 공급한다. `표현연결관측Snapshot`은 같은 문맥 지문과 관측 목록을 받는다. 호출자가 자동 후보 선택·값 보정을 요청하는 API는 없다.

| 범위 | 비교 내용 |
| --- | --- |
| 후보·상태 | 정확 CandidatePath/VisualKey/후보 fingerprint, 대상/Session/상태 판본/표현 판본/Slot/상태명, Logic E5 확인 근거 |
| 대상 건전성 | 요구 Component별 이름/확인값, Renderer/Collider/Bounds와 요구 활성·유한/비어있지 않음 관측 |
| 배치·입력 | 부모·위치·기준점·상호작용 대상의 승인 기대값/관측값/유효성 |
| 소유·해제 | 생성/표시/구독의 소유 범위 식별값, 취소·전환 ReleaseCoverage와 유효성 |

관측 `Unobserved`는 Conditional, 근거 있는 `Missing` 또는 확인 불일치는 Blocked다. 확인·결손 주장에 근거 경로/64자리 SHA가 없으면 확인 완료로 세지 않는다. Component 이후 공간/소유 항목은 **준비 계약이 명시적으로 비적용이며 사유가 있을 때만** 생략할 수 있고 대상/상태/후보/Logic E5는 비적용으로 우회하지 못한다. 필수 요구/관측 누락·중복/범위 밖 관측도 분리해 반환한다.

건전성/배치/소유 항목은 문자열이 같다는 이유만으로 통과시키지 않고 nullable `Validity`를 요구한다. null은 미관측, false는 확인된 부적합이다. 이 값의 의미는 공급자가 확인한 요구 활성 상태·유한 외곽/pose·정확 연결·단일 작성자·해제 계획 범위다. **순수 검사는 좌표 문자열에서 새 허용오차/지지면을 계산하거나 관측 자료의 진위/현재성을 파일·Editor로 재인증하지 않는다.** 해당 공급자는 같은 좌표계·단위/판본의 근거를 제공해야 한다. 따라서 Fixture의 모든 관측 true는 실제 Scene 성공이 아니다.

각 Check는 대상 항목·기대/관측·근거·차단 이유·다음 담당·가장 이른 E를 보존한다. Session/상태 판본 문제는 Logic E1 영향 검토, Logic E5 부족은 Logic E5, 표현 연결은 Presentation E4로 안내한다. 공통 결과는 대상·후보/상태 판본·문맥/결과 지문과 불변 Check 목록을 가진다. `IsE5Completion`은 항상 false다.

준비 판본·대상·후보·상태·배치/소유 계약이 바뀌면 이전 관측 문맥 지문이 맞지 않아 `ObservationContextChanged_RecheckRequired`로 차단한다. 적용 직전 **현재 준비/관측으로 다시 Review**해야 하며 이전 Ready를 저장해 실행하는 캐시는 없다. 동일한 근거/내용은 순서·문화권과 무관하게 같은 결과가 된다. 외부 배열을 나중에 바꿔도 이미 받은 입력/결과는 바뀌지 않는다.

## 첫 Farm 소비 결과와 남은 실제 연결

| 소비 입력 | 결과 | 근거 수준 |
| --- | --- | --- |
| 실제 연결 사본/관측/완성 E4 선택 입력 없음 | Conditional, `FarmSnapshotMissing_E5Unlinked`, `FarmLogicE5EvidenceMissing`, 항목별 준비/관측 미확보 | 누락 입력 처리. 현재 Scene를 새로 관측한 결과 아님 |
| 기존 Logic E3 + null 컴포넌트 보고를 모사한 독립 입력 | Blocked, LogicE5 `ObservedMismatch`, Component `ObservedMissing` | 시험용 대상·Session과 근거. r99 Scene를 복원/주입하지 않음 |
| 같은 준비 상태·완전한 동일 판본 관측 Fixture | Ready, IsE5Completion=false | 순수 계약 시험. 실제 논리E5·자산 조회·조립 완료 아님 |

D388에서 확인한 `Growing/HarvestReady/Harvested`와 `CropGrowing/CropHarvested`, 정확 potato-s/potato-l/box-potato 계열의 차이를 새 자동 대응표로 덮지 않았다. Slot을 VisualKey로 같다고 가정하지 않는다. Farm 대상·상태가 준비 입력과 다르면 공통 검사 결과가 Ready라도 Farm 소비자가 추가 차단한다.

현재 제품 호출·Logic E5 상태 공급·정확 자산 선택·부모/지지·접근/입력·표시/해제 관측 공급자는 남아 있다. **기존9밭 null 수리·Farm 시점/표시·Scene·실제 Play는 이번 범위 밖**이다. 다음은 개발의 Presentation E4 상태/정확 후보 소비·관측 입력 결속이며, 권위·Save는 기존 Logic 영향 검토/승인 경계를 따른다. 공간/애니 실제 실행은 별도 승인으로만 진행한다.

## 관리 연결과 검증

기존18모듈을 유지하고 `presentation-binding`·`visual-source-bounds`에 코드/시험·결과 의미를 연결했다. Farm 명세의 `connectionPreflightImplementation`과 모듈 사유에 Conditional/Fixture Blocked 및 미연결 이유를 적었다. 기존 결과가 없는 명세는 미검증으로 읽고 기존 E를 유지한다. 모듈 전체 Passed/E5 승격은 하지 않았다.

초기 집중32/전체619 통과 후, 소유·해제/부모·기준점·상호작용의 유효성 미관측/부적합을 명시하는7회귀를 추가했다. 최종 회귀·Fast/Task/생성 검증 결과는 아래 마감 절에 기록한다. 중간에 생성 상태판 갱신 전 관리 검사를 실행해 `GeneratedOutputOutOfDate`가 있었으며, 기준 검사를 완화하지 않고 기존 생성기를 실행한 뒤 재검증한다.

기획 [E5 성립 조건과 현재 준비 상태](E5성립조건과현재준비상태-2026-08-31.md), SHA `80944A25450E0B6E74423CDFB66C3242D8B5315C293DDE53F21B8A9864DE4629`의 **08:04 KST 사본**을 전체 검토했다. 지식 E4·후보·실제 연결/해제·E5 경계는 이번 결과와 일치한다. 그 시점의 D389 미완료 설명을 소급 수정하지 않고 이 보고를 후속 결과로 연결한다.

r129/야간/자동화·Editor/새가공/구매/실제 캡처·Scene/Logic/Save·commit/push 재개 없음. 과거 두 r4 명세의 `ProtocolRevisionInvalid`는 이번 변경으로 해결하지 않았으며 전체 개발시스템 완료로 보고하지 않는다.

## D389 인수 경계

- 새 회귀39개를 포함한 독립 .NET 전체 **626/626**, 실패/건너뜀0: `artifacts/local/validation/presentation-connection-d389/final-all.trx`. 순수 입력 시험이며 실제 Editor 관측이 아니다.
- Scoped Fast `artifacts/local/validation/20260831-081234`: 코드 지도/E 책임 지도·두 영향 프로젝트 빌드·집중 회귀 통과. E 책임 지도768후보/765분류/제외3/미분류0이며 전체 변경을 이번 구현으로 합산하지 않는다.
- 기존 관리42/조사구조24 및 Goal r133 Write/Check 통과. 표현18모듈의 기존 생성기를 갱신해 중간 GeneratedOutputOutOfDate를 해소했다. 검사 완화 없음.
- 위는 첫 인수 경계다. 최종 재검토에서 상태는 있지만 Plan/대상 기대값이 미준비인 경우 Farm 추가 비교가 이를 Blocked로 바꾸는 누락을 발견했다. 확인된 불일치만 Farm 차단으로 추가하고 공통 미확보 Conditional을 보존하도록 D389 소비자에 보완했으며 3회귀를 추가했다. 이는 D390의 차단 기준 완화가 아니라 기존 D389 관측 의미의 정합화다.
- **최종 새 회귀42 / 전체629/629(실패·건너뜀0)**. Task `artifacts/local/validation/20260831-081646`의 Unity.slnx 빌드 경고0/오류0, 코드/E책임 지도 및 전체 시험 통과. TRX SHA `9AA171044C45833B2F7258402BDB3E0EA25D9E5A8B1EB9B6C0A629ABC4A9091F`를 XML Counters로 직접 확인했다. 초기626 결과는 이전 코드 판본의 이력으로 구분한다.
- 기획 승인5개(D386~390)·08:04 E5 보고·방법론의7hash 불변, 관련10문서 로컬링크265/265 확인. `presentation-connection-d389/closeout-verification.json`에 코드6/관리/명세hash와 검사 결과를 기록했다. D386 근거12/주요자산28도 최종 재대조 일치. 문서 scoped Fast081728 통과(빌드·시험 생략).
- D390 작업 관리 설명은 별도 후속 보고로 구분하며 실제 Editor/Scene/자산 조회·공간/게임 검증은 여전히 없다. 기존 protocol r4 두 명세의 실패는 미해결로 보존했다.
