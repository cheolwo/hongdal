# Presentation 최소 모듈 — Farm 상태 사본 소비

## 결과와 범위

D386의 **공통 판본 검사 보완과 첫 Farm 준비 소비자**를 구현했다. 기존 공통 비교기를 실제 소비하며, 새 프레임워크나 빈 구현을 추가하지 않았다. 코드·독립 시험 결과이며 **실제 Farm UI/Preview/Confirm·Scene·수확·E5 연결은 미완료**다. 야간 작업·r129·다른 전문 작업은 재개하지 않았다.

- 승인: [Presentation 단계별 최소 모듈](../AI/Presentation단계별최소모듈-2026-08-31.md), `presentation-minimum-modules.design.r1`, SHA256 `602306949E897AFBD59B066A9547A76D611A8CD7001476B7564DC294FB1CEF53`.
- 기존 기획: [Farm 경작 세계 발현](../Architecture/PlayableLoops/Farm경작세계발현E5.md).
- 재사용 연구: [Accepted 공간·배치 연구 r1](../Architecture/PlayableLoops/Farm경작세계발현E5-공간배치연구.r1.md), SHA256 `EADA84A0CA49E2D0BE986866420030612249CC622398F06C7BE83A72B57711B0`. 정적 상태 문구·결과 표시의 최소 대체 허용만 소비했다. 자산 적합성이나 E5 승격으로 확대하지 않았다.

## 실제 재사용 조사와 선택

| 기존 코드 | 확인한 책임 / 이번 선택 |
| --- | --- |
| `Ssalddel.Unity/Runtime/PresentationContracts/Reconciliation/StableIdReconciliation.cs` | 낮은 데이터 판본 거부, 중복 식별자 거부, 동일 표현의 기존 인스턴스 보존. **기존 항목과 비교할 때만 표현 판본을 검사하여 최초 추가는 빈 판본을 통과시키는 누락**을 재현하고 보완했다. |
| `Ssalddel.Unity/Runtime/WorldProjection/SimulationLhAssetPlanPresentationReconciler.cs` | 위 공통 비교기의 실제 기존 소비자. 수정하지 않고 집중 회귀 5개로 호환을 확인했다. |
| `Ssalddel.Unity/Runtime/Data/WorldDataFlowRevisionModels.cs` | `DataRevisionSet`, `WorldDataFlowRevisionCalculator.CalculateInterpretation/CalculatePresentation`을 그대로 소비한다. 새 해시 프레임워크를 만들지 않는다. |
| `SimulationFarmSurvivalStateSnapshot`, `Simulation재배단위Snapshot`, `Simulation수확LotSnapshot` | 기존 읽기 계약을 그대로 사용한다. `TileStableId`는 기존 파종 코드가 `SoilTileStableId`로 공급하는 관계를 확인했다. `CausedByTaskStableId`는 기존 원인 식별자를 그대로 읽으며 완료 행위 기록 전체를 새로 인증하지 않는다. |
| Unity `WorldVisualCatalog.Resolve` | Prefab·UnityEngine 기반 자산 조회 책임이다. 이번 순수 상태 준비에는 호출하지 않으며 새 대장을 만들지 않는다. |
| Unity `공용AnimationAdapter.TryAcquireExternalPose/ReleaseExternalPose`, `Farm표시범위Lease` | Actor 포즈/정적 계층의 실제 소유 수명 책임이다. 이번에는 Actor·GameObject·렌더를 생성하지 않아 적용하지 않는다. 해당 수명 성공을 이번 시험으로 주장하지 않는다. |

위 Unity 파일은 `C:/Users/user/ssalddel/Assets/Ssalddel/Presentation/World/`에서 파일로만 확인했다. 제품 배선은 변경하지 않았다.

## 변경 파일과 소비 API

정확 구현 루트는 `C:/Users/user/source/repos/Hongdal/`이다. 기존 프로젝트의 자동 소스 포함을 사용하며 csproj/asmdef는 수정하지 않았다.

- [공통 비교기](../../Ssalddel.Unity/Runtime/PresentationContracts/Reconciliation/StableIdReconciliation.cs): 기존 본문에 9줄 추가. 입력 인덱싱 중 표현 판본을 검사하므로 최초 추가·삭제 대상도 누락을 거부하고 첫 실패에서 중단한다. 판본 대신 기존 동등성 함수만 쓰는 소비자에는 새 필드를 요구하지 않는다.
- [Farm 준비 소비자](../../Ssalddel.Unity/Runtime/Farm/Farm수확상태PresentationPreparation.cs), 같은 경로의 `.meta` 신규.
- [Farm 독립 시험](../../Ssalddel.Unity.Tests/Farm수확상태PresentationPreparationTests.cs), 같은 경로의 `.meta` 신규.
- [최초 판본 회귀](../../Ssalddel.Unity.Tests/PresentationRevisionFirstApplyTests.cs), 같은 경로의 `.meta` 신규.
- 이 보고서. 그 외 공통 원장·CURRENT_WORK·generated·승인 원문 쓰기 없음.

```csharp
var preparation = new Farm수확상태PresentationPreparation(
    sessionStableId, ruleRevision, soilTileStableId, cultivationUnitStableId);
bool prepared = preparation.TryPrepare(authoritativeSnapshot, out var state, out var diagnostic);
```

입력은 호출자가 확보한 기존 상태 사본이며 여기서 Session/권위 상태를 생성하지 않는다. 명시한 Session·규칙·토양·재배 단위를 바꾸어 재사용하지 않고 다른 문맥은 새 준비 소비자로 분리한다. 호출은 순차 사용을 전제로 하며 다중 스레드 갱신은 이번 계약에 포함하지 않는다.

- `Growing/HarvestReady/Harvested`를 기존 `farm.crop.grow/harvest` **표현 슬롯**과 한국어 상태 문구로 변환한다. 슬롯을 검증된 Prefab/VisualKey로 위장하지 않는다.
- 결과는 읽기 전용 값이다. 수확 전 `Quantity=null`, 수확 뒤에는 같은 재배 단위의 Lot 식별자·판본·수량·단위·상태·원인 식별자만 복사한다. 예측 생산량·고정 300kg·단위 변환을 만들지 않는다.
- 없는 상태 사본은 `FarmSnapshotMissing_E5Unlinked`; 다른 Session/규칙, 음수 판본, 누락/중복 대상, 잘못된 관계, 알 수 없는 재배 상태, 수확 후 Lot 누락 등은 명시 사유와 `false`를 반환한다. 정상 판본 0은 허용하며 기본값 0만으로 판본이 실제 공급됐다고 인증하지 않는다.
- 같은 판본·같은 표시 내용은 `Unchanged`이며 같은 객체를 반환한다. 같은 판본의 다른 표시 내용은 충돌, 낮은 판본은 공통 비교기의 `LowerDataRevision`으로 거부한다. 높은 판본은 내용이 같아도 조회 판본을 보존한다.
- 실패 시 `out state=null`; `Current`는 마지막 성공 자료를 보존한다. 이를 최신 연결 상태로 표시하면 안 된다. 올바른 같은 판본으로 다시 준비할 수 있다. 원본 사본은 변경하지 않는다.
- 표시 판본 해시는 **소비한 필드**에 대한 계보다. 전체 Simulation 상태 해시·서명·신뢰/권한 검증이 아니다. 같은 재배의 여러 Lot, 다른 생산 주기 자동 선택, 작업 기록 전체 검증은 지원하지 않는다.
- 모든 준비 결과는 `PresentationOnly=true`, `CanConfirmAuthority=false`, `SceneBindingStatus=E5Unlinked`다. 이는 의도적인 준비 상한이며 실제 권위 포트 연결을 성공으로 돌려주는 stub이 아니다.

## 검증

Editor 없이 기존 `netstandard2.1/C#9` Unity 패키지를 빌드하고 `net10.0` xUnit 소비 프로젝트로 검증했다. `--no-restore`를 사용했고 Assets 안 bin/obj를 생성하는 명령은 사용하지 않았다.

| 실행 | 실제 결과 |
| --- | --- |
| 수정 전 공통 회귀 | 6개 중 5개 실패/1개 통과. 최초 null·빈값·공백 판본, 기존 삭제 대상 누락, 첫 실패 중단을 재현. 의도한 실패 자료 보존. |
| 수정 후 집중 | **50/50 통과**: 공통 신규6 + Farm 신규36 + 기존 StableId3 + LH5. |
| 기존 Unity 패키지 전체 | **587/587 통과**, 실패/건너뜀0. 실제 Scene/Unity Editor 시험이 아니라 독립 .NET 회귀다. |
| 표준 Fast `20260831-072116` | `diff --check`와 Simulation Unity 코드 지도 검사 통과. **E 책임 코드 지도 불일치(exit2)로 중단**, 이후 표준 단계는 미실행. 생성물 소유권을 지켜 `--write`로 고치지 않았다. |

원결과: `C:/Users/user/source/repos/Hongdal/artifacts/local/validation/presentation-minimum-farm-d386/`의 `common-before.trx`, `focused-after.trx`, `unity-package-regression.trx`. 표준 실패 로그: `artifacts/local/validation/20260831-072116/evidence-map-check.log`. 메타데이터 추가 후 최종 소스 재빌드·전체 재검증 결과는 아래 마감 기록에 별도로 적는다.

시험은 정상/중복/오래된/동일 판본 충돌, 사본·컬렉션·대상·관계·Lot 누락/중복, operational 경계, 수량·단위·원인 계보, 다른 재배 Lot 오인 방지, 실패 후 회복, 원본/직전 준비 보존, 문화권과 배열 순서를 포함한다. Fixture 값은 독립 메모리 입력일 뿐 실제 Session에 주입하지 않았다.

## 남은 실제 연결과 인계

1. 기획 소유의 공통 모듈 연결·근거 판본과 생성 E 책임 코드 지도 갱신/검증이 남는다. 표준 Task 전체 성공을 선언하지 않는다. 생성물 불일치를 반복해서 고치거나 권한 밖 쓰기를 하지 않았다.
2. Farm UI가 같은 Session의 실제 조회 결과와 선택한 토양/재배 식별자를 공급하는 배선은 없다. Preview/명시 Confirm/권위 결과 재조회가 공급되지 않아 이번 산출물은 **E5 미연결**이다. 직접 상태 setter·새 Session·테스트 데이터를 이용한 실제 UI 우회는 하지 않았다.
3. 현재 Scene의 밭 누락 컴포넌트, 표시·카메라·접근·지지·Renderer/Collider/Bounds, 실제 입력·Save/재진입은 이번 범위 밖이며 해결하지 않았다. 새 자산 조회·조립/해제·Actor·애니메이션·렌더·캡처 시험도 없다.
4. Logic/Domain/Application/공개 게임 API/Save 규칙, Scene/Packages/자산 가공, 공통 원장과 승인 hash는 변경하지 않았다. Editor/Play/입력/캡처·다른 전문 재개·commit/push/자동화 재개 없음. Editor 현재 상태나 자동 import 여부를 새로 계측한 것은 아니다.

## 최종 소스 결속

메타데이터 추가 후 `dotnet test Ssalddel.Unity.Tests/Ssalddel.Unity.Tests.csproj --no-restore`로 최종 소스를 다시 빌드해 **587/587, 실패/건너뜀0**을 확인했다. `final-package-regression.trx` SHA256은 `861F4740B28EC8A466D7A8D29BE25732D702C1475696238FC5697D23C531EB0F`다. 이 전체 결과는 위 집중50을 포함하며 별도 시험 수로 합산하지 않는다. 최종 문서 링크7/7 존재, diff 공백 오류0을 확인했다. 생성 지도 불일치와 표준 Fast/Task 미완료는 그대로 남긴다.

| 최종 파일 | SHA256 |
| --- | --- |
| StableIdReconciliation.cs | `07CC6D1D3EB65A29E0DBA3C75E3CD9E6A3F1F278020AD4A0AC50ACA620CC67BF` |
| Farm수확상태PresentationPreparation.cs | `E5587D94334B188B488CA7ED4AF6A759154176112B4F4EB09B82A03104DB0175` |
| Farm수확상태PresentationPreparationTests.cs | `22F3324B1DB216AEB807B4DDC25A6CF96FF014AEB21C8D095B35F2127F7BFCDE` |
| PresentationRevisionFirstApplyTests.cs | `C7D7DCFC215CE17F87A0F862773C572CEF50A2BE79E80A2EC019021A603D926A` |
