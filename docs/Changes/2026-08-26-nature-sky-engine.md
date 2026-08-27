# Nature Sky Engine 세계 대기 표현

## 결과

`world-atmosphere.r1`을 Nature의 결정적 1,200초 시계에 결속하고, 맑음 → 흐림 → 비 → 뇌우 → 비 → 새벽 맑음 상태를 세계 공통 상태 사본으로 투영한다. `simulation-save.v25`는 이 상태와 Replay hash를 저장하며 대기 프로필을 사용하지 않는 v24 이하 의미와 hash를 유지한다.

Unity의 canonical `SimulationWorldShell`에는 `SkyEngineVisualRoot` 아래 Synty 구름 5개, 비 Particle 1개, 번개와 빗소리·천둥 AudioSource를 배치했다. `SkyEnginePresenter`는 기존 `월드시간대Presenter`의 조명 출력에 날씨를 합성하고, 플레이어가 `SkyExposureVolume` 안에 있을 때 강수를 차폐한다. Unity 표현은 권위 상태를 변경하지 않는다.

## 화면 검증

- 확인 수준: 간접 확인
- 저장 Scene 구조: Synty 구름 `5`, 비 Particle `1`, 번개·음향 Root 확인
- 코드·시험: Unity Editor·EditMode 어셈블리 빌드 오류 `0`
- 미검증: 실제 Test Runner, Play Mode, Game View, Console, 음향 청취

Unity Editor의 기존 Job lock `Access version should be odd when acquiring lock` 때문에 재컴파일 명령이 60초 후 시간 초과됐다. 따라서 저장 Scene 조립과 코드 빌드를 실제 하늘 화면 증거로 확대하지 않으며 WI-NATURE-14의 표현 E6과 E7을 열어 둔다.

## 자동 검증과 경계

- Nature+Atmosphere 집중 Simulation 시험 `55/55`
- Local Core·RemoteHost의 v25 대기 상태·revision·Replay hash 동등성
- 표현 검증 모듈 `14`, 프로필 `4`, PlayableUnit `16`
- `E9-WO-NATURE-SKY-ENGINE` 작업 명세 검사 통과
- 세계 대기는 H 단계가 아니며 LH Engine의 지면·셀·H 조립 책임을 변경하지 않음
- 현실 기상 Provider, 운영 DB, 플레이 효과, 새 공식 Scene은 포함하지 않음

전체 개발 관리 검사는 병행 변경된 기존 WI-13 Game View PNG의 등록 hash 불일치에서 중단됐고, Fast 검사의 E 책임 지도는 범위 밖 `SimulationSpatialCompositionSessionBindingTests` 미분류 한 건 때문에 통과하지 못했다. 기존 증거 hash와 병행 작업의 책임 분류는 임의로 고치지 않았다.
