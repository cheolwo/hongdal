# Simulation 규칙 기반 Runtime 렌더링 의도 Pipeline

## 한 문장 요약

Simulation은 `무슨 상태인가`만 확정하고, 렌더링 의도 Pipeline은 그 상태를 `무엇을 강조할 것인가`로 번역하며, URP 표현 대장은 현재 기기의 능력에 맞는 실제 표현 키를 선택한다.

```text
Simulation 상태 사본
→ 렌더링 의도 Projector
→ Channel별 합성
→ 공간·Synty 근거와 Capability 결합
→ Runtime World 표현 상태 사본
→ Unity Adapter
   ├─ URP MaterialPropertyBlock
   ├─ URP Volume·Renderer Feature
   ├─ Particle
   └─ Animation
```

Particle이나 Animation 완료는 Simulation 업무 완료를 의미하지 않는다. Runtime 표현 결과는 항상 `PresentationOnly=true`다.

## 세 Pipeline의 책임

| Pipeline | 질문 | 대표 산출물 |
| --- | --- | --- |
| Unity 공간 Pipeline | 어디에 놓을 수 있는가? | Tile·Area·Terrain·Mask·Route·배치 기준점 |
| Synty 경관 Job | 기본적으로 무엇으로 보일 것인가? | Farm·Town·Hub 경관, 그래픽 계획, `VisualKey` |
| Runtime 렌더링 의도 Pipeline | 현재 Simulation 상태를 어떻게 보여줄 것인가? | 강조·FX·Animation·URP 의미 지시 |

Runtime Pipeline은 앞의 두 실행본을 수정하지 않는다. 공간 실행 ID·출력 SHA-256과 Synty 시각 실행 ID·출력 SHA-256을 입력으로 참조한다.

객체별 공간 조건과 Simulation 규칙이 어떤 기본 구성·동적 의도 묶음으로 만나는지는 [공간·Simulation 규칙 객체 표현 결합 원장](SimulationWorldObjectRepresentationRuleLedger.md)이 먼저 설명한다. 이 원장의 동적 의도 묶음 키가 Runtime Projector 선택의 입력이 되며, 실제 URP 수치를 저장하지 않는다.

## 왜 URP 수치를 Simulation 규칙에 넣지 않는가

다음처럼 업무 상태와 렌더러 설정을 직접 연결하면 서버가 Unity 구현에 종속된다.

```text
나쁜 결합
Cargo = InTransit → Bloom 0.3, Color #00FFFF
```

현재 구조는 의미를 한 단계씩 번역한다.

```text
Cargo = InTransit
→ RouteFlowActive
→ urp.route-flow.emission.v1
→ Unity 구성 대장이 실제 Material·Shader 값을 적용
```

따라서 URP 프로필을 교체해도 화물 상태, WorldTick과 공간 원장은 변하지 않는다.

## 주요 계약

### `Simulation렌더링의도`

Simulation 상태를 표현 의미로 바꾼 결과다.

```text
IntentStableId
SourceStateStableId + SourceStateRevision
SessionRevision
IntentCode
ChannelCode
ScopeCode + TargetStableId
ContextStableId
Priority
LifetimeCode
EvidenceKindCode
PresentationOnly
```

### 렌더링 Channel

서로 다른 종류의 표현이 불필요하게 경쟁하지 않도록 영역을 나눈다.

| Channel | 의미 |
| --- | --- |
| `Environment` | 하늘·안개·시간대 |
| `Surface` | 지면·도로·건물 표면 |
| `Lighting` | 조명·노출·그림자 |
| `ObjectState` | 차량·화물·시설의 현재 상태 |
| `Attention` | 선택·위험·업무 강조 |
| `Fx` | 비·먼지·연기·불빛 |
| `Animation` | 차량 이동·NPC 행동 |

현재 합성기는 같은 대상과 같은 Channel 안에서 높은 우선순위를 선택한다. 우선순위가 같으면 `IntentStableId` 순으로 결정하고 억제된 의도와 이유를 남겨 재현성을 보장한다.

### 수명

| 수명 코드 | 사용 예 |
| --- | --- |
| `WhileStateMatches` | 화물이 운송 중인 동안 경로 강조 |
| `UntilRevision` | 특정 개정까지 유지하는 경고 |
| `Duration` | 몇 Tick 동안 보이는 효과 |
| `OneShot` | 출발·도착 순간 효과 |
| `UntilDeselected` | 선택 Outline |

일회성 의도는 발생 순번을 요구하며 Unity가 처리한 의도 ID를 다음 요청의 `AcknowledgedOneShotIntentStableIds`로 보내면 재조회 때 다시 표시하지 않는다.

### `Simulation렌더CapabilityProfile`

PC·모바일이라는 이름만 보지 않고 실제 지원 기능과 예산을 전달한다.

```text
SupportsForwardPlus
SupportsDepthTexture / OpaqueTexture
SupportsSsao / Decal
SupportsGpuInstancing / Particle
MaximumShadowedAdditionalLights
ParticleBudget
ShadowCasterBudget
```

Capability가 부족하면 상태를 실패시키지 않고 의미 있는 Fallback을 선택한다.

## 첫 세로 단위: 감자 화물 Farm에서 Hub로 운송

의도 생성 조건은 다음 두 상태가 동시에 참일 때다.

```text
화물운송 = 운송중
물류이동 = InTransit
CargoStableId와 LogisticsTaskStableId 일치
```

생성되는 의도와 표현은 다음과 같다.

| 의도 | 대상 | 표현 |
| --- | --- | --- |
| `CargoInTransit` | 화물 | 현재 표현 상태의 의미 근거, 직접 URP 지시 없음 |
| `VehicleMovementActive` | 차량 | `animation.vehicle.route-follow.v1` |
| `RouteFlowActive` | 경로 | Depth 지원 시 발광, 미지원 시 단순 색상 강조 |
| `DirtRoadDustCandidate` | 차량+경로 Context | 흙길 근거와 Particle 예산이 있을 때만 먼지 |

도로가 포장도로이거나 표면 근거가 없으면 먼지를 생성하지 않고 `DirtSurfaceEvidenceMissingOmitted`를 남긴다. 모바일에서 Particle이 없거나 예산이 0이면 `ParticleUnsupportedOmitted`를 남긴다.

## 결정성과 권위 경계

표현 상태 사본 hash는 다음 값으로 결정된다.

```text
Session·World 개정과 Tick
+ 공간 실행 ID·출력 hash
+ Synty 시각 실행 ID·출력 hash
+ 렌더링 의도 규칙 개정
+ URP 표현 대장 개정
+ Capability Profile 개정
+ 선택된 의도·표현 지시·억제 기록
```

같은 입력은 같은 `PresentationHashSha256`을 만든다. Pipeline은 전달받은 `경영SimulationSessionSnapshot`을 변경하지 않으며 운영 상태 사본은 입력 단계에서 거부한다.

## 현재 구현과 아직 남은 것

현재 구현됨:

- 화물운송 Runtime 렌더링 의도 Projector
- Channel 우선순위와 결정적 동률 처리
- 상태·개정·기간·일회성 수명 처리
- PC·Mobile Capability Fallback
- 공간 Route 표면 근거 검증
- 의미 기반 URP·Particle·Animation 지시
- Runtime 표현 hash와 Simulation 불변 검증

아직 구현하지 않음:

- Unity 프로젝트의 실제 URP Adapter와 Material·Volume 연결
- World→AreaSet→Area→Tile→Object 범위 상속을 위한 공간 계층 입력
- 날씨·계절·창고 혼잡 렌더링 의도 Projector
- 실제 HTTP 조회 계약과 Unity Runtime 저장소 연결
- Scene·Game View와 Frame Debugger 성능 검증

다음 세로 단위는 Unity가 `SimulationRuntimeWorldPresentationSnapshot`을 읽어 `RouteFlowActive`를 `MaterialPropertyBlock`에 적용하고, Synty Van의 기존 route follower를 `VehicleMovementActive`에 연결하는 것이다.
