# Nature 생활거점 E7 수동 완주와 저장 복원

## 결과

canonical `SimulationWorldShell`에서 실제 플레이어 입력으로 다음 경로를 완주했다.

`Nature 탐험 → 도끼 획득 → 첫 벌목 취소·자원 무변경 → 재시도 → 나무 3그루 벌목 → 오두막 도면 배치·건설 → 입실·퇴실 → HUD 저장 → Play Mode 재진입 복원`

재진입 뒤 `Day 2`, `도끼 보유`, 통나무 소지 `0`, 그루터기 3개와 `오두막 완성`이 같은 안전한 다음 선택 상태로 복원됐다. 실제 Play Console 오류는 `0`개였다.

## 화면 증거

- 확인 수준: 직접 확인
- Unity 증거: `C:/Users/user/ssalddel/Assets/Documentation/Changes/2026-08-26-nature-e7-entry/nature-e7-restored-closed-loop-game-view.png`
- 화면에는 복원된 `Day 2`, 도끼·재료·오두막 상태와 다음 선택 HUD가 함께 보인다.

오두막 접근은 남은 나무보다 생활 거점을 우선한다. 플레이어가 도면 위치에 이미 너무 가까우면 카메라가 외벽 안으로 들어가지 않도록 안전 반경까지 물러난 뒤 접근 완료로 판정한다.

## 자동 검증

- 실제 Input System 전체 폐루프 PlayMode `1/1` — 27.49초
- 권위 상태 사본 기반 감각 표현 PlayMode `1/1` — 2.92초
- 하단 지속 모드 UI 입력 경계 EditMode `1/1`
- 현재 Simulation revision의 Nature·Local Runtime 집중 회귀 `44/44`
- 취소·재수확·오두막·저장·복원 전체 `LocalProcess`·`RemoteHost` 동등성 `1/1`
- 두 실행 위치의 최종 revision `15`, `simulation-save.v23`, 저장 revision·Replay hash 일치
- Local slot 복원과 RemoteHost `replay-verifications`가 같은 완료 상태를 복원

새 동등성 시험은 E3 결정성 책임에 등록했다. 범위 지정 Fast는 `git diff --check`와 Simulation–Unity 코드 지도까지 통과했지만, 병행 작업의 기존 `SimulationSpatialCompositionSessionBindingTests` 한 건이 E 책임 미분류라 전역 strict 지도 관문에서 중단됐다. 이 범위 밖 시험을 이번 Goal에 맞춰 임의 분류하지 않았다.

## 남은 경계

절차형 도끼 획득·타격·취소·나무 낙하 효과음 4개는 Listener와 `AudioSource`에 결속됐다. Windows 사운드 드라이버와 Audio 서비스는 정상이지만 현재 연결된 재생 종단점은 `0개`라 실제 청음은 하지 못했다. 실제 청음과 승인 Nature Ambient·BGM은 E7 필수 조건이 아닌 선택형 감각 수용 증거로 대기한다. 운영 Provider 호출과 운영 DB 쓰기는 발생하지 않았다.

공식 E7 관문의 실제 입력·저장 Scene·Play Mode·Game View·Console·성공/취소/회복/귀환·결정적 Save/Restore/Replay·LocalProcess/RemoteHost 동등성을 모두 충족했으므로 `WI-NATURE-05`는 `E7Closed`, `playable-loop:nature-shelter-foundation.v1`은 `E7 PlayClosed`로 닫는다.
