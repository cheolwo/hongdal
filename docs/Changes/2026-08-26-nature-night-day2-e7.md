# Nature 보관·수면·Day2 반환 E7

## 결과

`playable-loop:nature-night-day2.v1`을 `E7 PlayClosed`로 닫았다. 플레이어는 완성된 오두막에서 자원을 보관하고 밤에 잠든 뒤 새벽에 작업대 확장 계획을 선택해 `Day2Ready`와 다음 안전 선택 상태로 돌아온다.

기존 Logic E7 증거는 유지하고, 새 논리·표현 이중 순환 방법론에서 다시 열린 Presentation E6 피드백을 실제 공간 변화와 Game View로 닫았다. 통합 E는 두 트랙의 낮은 값인 E7이다.

## 실제 화면·입력 증거

- `WI-NATURE-13`: 실제 `G` 입력으로 통나무 2개를 소지 `2→0`, 오두막 보관 `0→2`로 옮겼다.
- `WI-NATURE-14`: 실제 `T` 입력으로 수면을 시작하고 권위 시간 진행 뒤 정확히 1110초 `DawnReached`, `Sleeping=false`를 확인했다.
- `WI-NATURE-15`: 실제 숫자 `1` 입력으로 `Workbench` 계획과 `Day2Ready=true`를 확정했다.
- `WI-NATURE-13`: 오두막 옆 Synty 목재 더미와 `2/20` 보관 수량을 함께 표시한다.
- `WI-NATURE-14`: 야간 차광 아래 침상·캠프파이어 자리를 표시하고 새벽에는 이를 숨긴 뒤 계획 자료·계획판으로 교대한다.
- `WI-NATURE-15`: 작업대 계획 확정 뒤 계획판에 건설 위치 선택을 안내하고 실제 건설 시작 뒤 계획판을 숨긴다.
- 집중 실제 입력 PlayMode `3/3`과 시각 자산 대장 EditMode `1/1`이 통과했다. 전체 Nature E7 PlayMode는 `4/6`이며 기존 이동 제한·황혼 상태 문구 2건은 별도 회귀로 남았다.
- 대표 Game View는 Unity 작업 폴더 `artifacts/local/validation/nature-night-day2/`의 보관·수면·새벽·Day2 선택 PNG 네 장으로 남겼다.

## 권위·저장 동등성

- 같은 명령열을 Solo `LocalProcess`와 Hosted `RemoteHost`에 적용한 생활 거점 동등성 회귀 `4/4`가 통과했다.
- Container 보관, 수면·새벽, Workbench 계획, 최종 revision, `simulation-save.v23` Replay hash와 Local slot 복원이 일치했다.
- Unity는 Simulation 상태 사본을 표시할 뿐 시간 배율, 자원 이동, 계획 비용 또는 건설 Effect를 결정하지 않는다.

## 공간·관리 원장

- 오두막 H1에 `ShelterStorage`, `ShelterSleep`, `DawnPlanChoice`와 대응 입력 기준점을 승인했다.
- Actual E5 생성 결과는 `WI=40/6/7/7`, WI-H 상태는 `EstablishedH1=15`, `EstablishedH3=13`이다.
- `WI-NATURE-13~15`는 각각 E7이며 폐루프는 `Validated / PlayClosed`, Goal은 `Completed`, WIP는 Goal·WI 모두 0이다.

## 제외

Day2 작업대 건설 `WI-CON-01`의 표현 E7, 실제 음향 청취, 승인 Ambient·BGM, 운영 Provider와 운영 DB는 이번 폐루프에 포함하지 않았다. Unity 시험 도구의 Job lock Assert 반복 때문에 Console 무오류 승격 근거도 이번 증거에는 포함하지 않았다.
