# Sky Engine 세계 대기 표현 계층

## 목적

`Sky Engine`은 세계 공통 하늘 상태를 시간대·구름·강수·바람·번개·음향으로 표현한다. `LH Engine`이 지면 셀과 H 공간을 조립하는 것과 달리 Sky Engine은 카메라 전역 표현 계층이며 H1~H5나 배치 통제 단계가 아니다.

```text
Simulation 세계 대기 상태
  profile + rule revision + Nature 권위 시계 + seed
                    ↓ 상태 사본
Unity Sky Engine Projector
  기존 월드 시간대 + 날씨 + 차폐 상태
                    ↓
조명 / 환경광 / 안개 / Synty 구름 / 비 / 번개 / 빗소리·천둥
```

Unity의 `Transform`, `ParticleSystem`, `AudioSource`, 카메라와 Renderer는 `WorldTick`, 날씨 코드, 전이 진행률 또는 번개 순번을 변경하지 않는다.

## 첫 프로필

- 고유 식별자: `world-atmosphere:nature-night-day2.fixture.r1`
- 규칙: `world-atmosphere.r1`
- 범위: `World`
- 시계 원천: `NatureCycleClock`
- 대상 폐루프: `playable-loop:nature-night-day2.v1`
- 첫 순서: `Clear → Cloudy → Rain → Thunderstorm → Rain → Dawn Clear`
- 순환 길이: Nature 권위 시계와 같은 1,200초

각 구간의 구름량·강수량·바람 세기와 번개 순번은 `scenarioSeed`, `cycleIndex`, `elapsedSecondsInCycle`에서 결정적으로 파생한다. 실제 기상 관측, Unity frame time과 카메라 위치는 권위 입력이 아니다. 첫 판본은 게임 규칙 효과를 추가하지 않고 가시성과 감각 피드백만 제공한다.

## Unity 결속

- canonical `SimulationWorldShell`의 조립 순서는 `LH 지면·셀 준비 → Sky 상태 적용 → 실외배치 → 실내배치 → 셀 표현 완료`다. 기존 독립 지연 설치는 LH가 없는 호환 Scene에서만 사용한다.
- 기존 `월드시간대Presenter`는 외부 출력이 없을 때 기존 단독 동작을 보존한다.
- `SkyEnginePresenter`가 연결되면 기존 시간대 모델에 날씨의 명암·환경광·안개를 합성한다. 개별 Renderer의 기본 색은 일괄 변경하지 않는다.
- 구름과 비는 원본 Prefab을 수정하지 않고 canonical `SimulationWorldShell/SkyEngineVisualRoot` 아래 배치한다.
- 첫 자산은 Polygon Farm의 `SM_Generic_Cloud_01~05`와 `FX_Rain`이다.
- 비와 구름은 카메라·플레이어를 따라가는 표현일 뿐 세계 객체 배치 기록이나 H 능력을 만들지 않는다.
- 실외배치엔진은 대기 상태를 건조·젖음·적설·바람 표현 Variant로만 읽는다. 배치 Stable ID·위치·Spawn과 게임 규칙은 바꾸지 않는다.
- 실내공간조립엔진은 날씨를 가구 선택과 배치 hash에 사용하지 않으며, Sky는 조명·창문 노출·강수 차폐에만 관여한다.
- 오두막 내부 상태 또는 `SkyExposureVolume` 안에서는 강수 Particle과 빗소리를 숨긴다. 권위 강수 상태 자체는 유지한다.
- 번개와 천둥은 권위 `LightningSequenceIndex`가 바뀔 때 한 번만 재생한다.
- 별도 공식 Scene, 외부 날씨 Provider, 외부 Sky 플러그인을 만들거나 호출하지 않는다.

## Nature 자연 방향광

Nature의 나무·통나무·오두막·작업대와 LH 지면은 `lighting.pyeongchang.shared-day.v2` 프로필과 `directional-lighting.natural.r1` 규칙을 사용한다. 실제 표면 명암은 URP Lit가 월드 법선 `N`과 표면에서 광원으로 향하는 방향 `L`을 읽어 `saturate(dot(N, L))`로 계산한다. Unity `Directional Light`의 광선 진행 방향은 `Transform.forward`이므로 `L`은 그 반대 방향을 쓴다.

```text
Simulation 시간대 상태 사본
  SunPitch + SunYaw + 조도 + 그림자 강도
                    ↓
월드시간대Presenter
  Directional Light 회전·세기 + 시간대별 환경광
                    ↓
URP Lit
  saturate(dot(WorldNormal, SurfaceToLightDirection))
                    ↓
밝은 면 / 그늘 면 / 투사 그림자
```

- 낮의 환경광 보정은 `1.0`, 황혼·새벽은 `1.1`, 밤은 `1.2`다. 이 값은 어두운 면의 형태 판독을 돕는 표현 보정이며 게임 규칙이나 은신·시야 수치를 만들지 않는다.
- Synty 원본 Prefab·Material·Shader Graph는 수정하지 않는다. Runtime Wrapper의 Renderer 그림자 정책과 기존 URP Lit 입력만 사용한다.
- 기존 `자연경관ShadowPolicyView`가 있는 대상은 그 정책을 우선한다. 새 방향광 대상 View가 중복으로 그림자 설정을 덮어쓰지 않는다.
- 동적 LH 상세 셀은 바닥만으로 통과하지 않는다. 나무·통나무·오두막·작업대 가운데 실제 핵심 대상의 Mesh 법선, Lit Shader와 그림자 투사·수신 정책이 확인되어야 E6 방향광 검증을 통과한다.
- 알 수 없는 조명 프로필은 화면 안전성을 위해 기본 프로필로 투영하지만 `DirectionalLightingProfileUnknown`을 남겨 표현 승격을 차단한다.
- 방향광 검증 실패는 `WorldTick`, `AuthorityRevision`, 셀 통행 준비와 Save/Replay hash를 변경하지 않는다. 해당 셀은 실행을 유지하되 `WorldPresentation.Ready` 표현 증거만 만들지 않는다.

## 저장과 호환

대기 프로필이 있는 세션은 `simulation-save.v25`를 사용하고 생성 입력, 현재 상태와 Replay hash에 대기 계약을 포함한다. 대기 프로필이 없는 기존 세션은 비활성 상태 사본을 읽고 v24 이하 schema와 canonical hash 의미를 유지한다.

Solo `LocalProcess`와 Hosted `RemoteHost`는 같은 Simulation Core 규칙을 사용한다. Unity는 양쪽의 같은 상태 사본을 소비한다.

## 표현 검증

`Atmosphere` 기능은 공통 표현 관문에 다음 조건 모듈을 더한다.

- E4 `atmosphere-authority-binding`: 프로필·규칙 revision·권위 시계·세계 범위 결속
- E6 `weather-camera-audio-exposure`: 조명 합성, 구름·강수·번개, 음향과 실내 차폐
- E6 `directional-light-surface-readability`: 시간대 광원 방향, Mesh 법선, Lit Shader, 그림자 정책과 Game View 그늘 면 판독
- E7 공통 관문: canonical Scene 실제 입력, Game View 상태 차이, 결과·귀환과 Console

자동 시험과 Scene 배선은 실제 Game View와 청음을 대신하지 않는다. 첫 E7 증거는 적어도 맑음, 비, 뇌우, 오두막 내부 차폐, 새벽 맑음의 중요한 상태를 구분해 남긴다.

## 후속 범위

계절·지역별 날씨 확률, 젖음·시야·이동 비용 같은 게임 효과, 현실 기상 관측, 다중 지역 미기후와 외부 Sky 플러그인은 별도 작업 명세에서 권위·저장·실패 복구를 다시 검토한 뒤 추가한다.
