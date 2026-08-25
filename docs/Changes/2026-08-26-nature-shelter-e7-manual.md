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
- 별도 외부 `RemoteHost`와 `LocalProcess`의 저장·Replay hash 동등성은 기존 현재 증거를 유지한다.

## 남은 경계

절차형 도끼 획득·타격·취소·나무 낙하 효과음 4개는 Listener와 `AudioSource`에 결속됐지만, Editor의 FMOD 출력 장치 초기화 오류 60 때문에 실제 청음은 하지 못했다. 승인 Nature Ambient·BGM은 E7 필수 조건이 아닌 선택 채널로 대기한다. 운영 Provider 호출과 운영 DB 쓰기는 발생하지 않았다.

따라서 `WI-NATURE-05`와 `playable-loop:nature-shelter-foundation.v1`은 수동 Game View·저장 복원까지 확보했지만 실제 청음 전까지 `E7 Partial`과 WIP 1을 유지한다.
