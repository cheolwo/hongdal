# Nature 획득 자원 거점 보관 E7

## 결과

`playable-loop:nature-night-day2.v1`의 첫 작업인 `WI-NATURE-13 획득 자원 거점 보관`을 E7로 닫았다.

- 완성 오두막 H1에 `ShelterStorage`, `StorageInteractionAnchor`, `ContainerCapacity`, `Material` 능력을 승인했다.
- 실제 Nature 생활핵 Graph의 `nature-shelter` Node에 `binding:actual-e5:wi-nature-13`을 생성했다.
- 보관 가능한 수량과 오두막 용량은 Simulation Core가 계산하고 Unity는 상태 사본의 소지량·보관량·결과만 표현한다.

## 실제 입력과 화면

canonical `SimulationWorldShell`에서 도끼 획득, 나무 4개 벌목, 오두막 배치·건설·입장 뒤 실제 Input System `G` 입력으로 통나무 2개를 보관했다.

- PlayMode: `1/1`, 3.40초
- 상태: 소지 통나무 `2 → 0`, 오두막 보관 `0 → 2`
- 시험 뒤 Unity Console 오류: `0`
- Game View: `C:/Users/user/ssalddel/artifacts/local/validation/nature-night-day2/nature-night-day2-wi13-store-game-view.png`
- PNG SHA-256: `d5612e2aec6207a67cb051dabe4385277ff702b6c11773169a0bc3f3a3ab3b13`

## 저장·Hosted 동등성

`nature-survival.realtime.r2`의 같은 명령열을 Solo `LocalProcess`와 Hosted `RemoteHost` HTTP 경계에 적용했다. 최종 World revision, `simulation-save.v23` Replay hash, 오두막 Container와 통나무 2개 입고 Transfer가 일치했고 Local slot 복원도 같은 보관 상태를 되살렸다.

- 전용 동등성: `1/1`
- 결과: `artifacts/local/validation/nature-night-day2/nature-night-day2-wi13-host-parity.trx`
- TRX SHA-256: `81045c461af456421df292f85ddf56e9d4dbdecb6520eb8e8a7b63c24381e2da`

## 다음 작업

장기 Goal은 `playable-loop:nature-night-day2.v1 → E7 PlayClosed`로 유지한다. WI WIP 1은 `WI-NATURE-14 오두막에서 수면·새벽 맞기` E4로 이동하며, 다음 최저 미완료 의존성은 실제 생활핵 Graph의 수면 기준점·점유·밤 시간 문맥 결속이다.

운영 Provider 호출, 운영 DB 쓰기, 새 공식 Scene 생성은 수행하지 않았다.
