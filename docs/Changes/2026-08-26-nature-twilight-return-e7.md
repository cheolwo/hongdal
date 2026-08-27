# Nature 황혼 이중 전투 참여 E7

## 결과

`WI-NATURE-11 황혼 위협 대응 방식 확정`을 canonical `SimulationWorldShell`에서 E7로 닫았다.

- 3인칭 관찰 운영: 실제 `O` 관찰과 `F` 전투 입력 뒤 `ObserverOperation`을 잠그고 Simulation 권위 전투를 완료했다.
- 1인칭 직접 개입: 실제 `O`, `F`, 좌클릭 전투 행동으로 `DirectAction`을 잠그고 Simulation 권위 결과까지 완료했다.
- 안전 후퇴: 실제 `O`, `R` 입력으로 황혼 조우를 `Retreat`로 해결하고 권위 결과 코드를 보존한 채 거점 선택으로 돌아왔다.
- 두 경로 모두 Nature 조우를 `Resolved`로 만들고 결과 코드를 보존한 채 거점의 다음 선택 상태로 돌아왔다.
- 전투 생성이 일시 실패한 `CombatActive` 상태에서는 `F`로 인계만 재시도하며 Nature 선택 Effect를 중복 적용하지 않는다.

## 권위와 결정성

`WorldLocal` Nature 전투는 별도 `DerivedBattlefield` 문맥을 섞지 않는다. 이에 따라 Solo `LocalProcess`와 Hosted `RemoteHost`가 같은 현장 전투 context를 사용한다.

ObserverOperation과 DirectAction 각각에 동일 명령열을 적용한 자동 시험 `2/2`에서 다음이 일치했다.

- 전투 결과 코드
- 최종 World revision
- Battle Replay hash
- `simulation-save.v23` Replay hash
- Battle Store를 포함한 실제 슬롯 복원 결과

Nature 집중 회귀는 `40/40`이 통과했다. 관련 전투 회귀 `22`건 중 `21`건이 통과했고, 남은 `SimulationBattleSupportSourceUnavailable` 1건은 이번 변경 전부터 존재한 전투 증원 Fixture 문제다.

## 화면·Runtime 증거

- 관찰 운영 실제 입력 Play Mode: `1/1`, 33.82초
- 직접 개입 실제 입력 Play Mode: `1/1`, 9.26초
- 안전 후퇴 실제 입력 Play Mode: `1/1`, 3.52초
- 시험 직후 Unity Console 오류: `0`
- 관찰 Game View: `C:/Users/user/ssalddel/artifacts/local/validation/nature-twilight/nature-twilight-wi01-observed-game-view.png`
- 직접 결과 Game View: `C:/Users/user/ssalddel/artifacts/local/validation/nature-twilight/nature-twilight-wi11-direct-result-game-view.png`
- 후퇴 결과 Game View: `C:/Users/user/ssalddel/artifacts/local/validation/nature-twilight/nature-twilight-wi11-retreat-result-game-view.png`
- Hosted 동등성: `artifacts/local/validation/nature-twilight/nature-twilight-wi11-host-parity-current.trx`

화면과 애니메이션은 참여 방식과 결과를 표현할 뿐 피해·보상·귀환 상태를 계산하지 않는다. 운영 Provider 호출과 운영 DB 쓰기는 수행하지 않았다.

## 다음 작업

E1 재검토에서 WI-NATURE-11의 `Retreat`는 황혼 조우 해결, WI-NATURE-02의 `EmergencyRetreat`는 지역 사건 압력을 유지한 파티·경로 예약 이동으로 판정했다. WI-NATURE-02~04는 Stable ID와 E3 증거를 유지한 별도 `playable-loop:nature-regional-threat-recovery.v1` Extension으로 분리했다.

장기 Goal `playable-loop:nature-twilight-return.v1 → E7 PlayClosed`는 완료했다. WIP 1은 다음 Core Goal `playable-loop:nature-night-day2.v1`의 `WI-NATURE-13 획득 자원 거점 보관` E5로 이동한다.
