# Unity SimulationWorldShell 전략 카메라

## 목적

`SimulationWorldShell`의 사용자가 Unity Editor의 Scene View 편집 카메라가 아니라 Play Mode와 빌드된 게임의 Game View에서 같은 입력 방식으로 World를 탐색하게 한다.

## 첫 단계 구현

```text
PlayerCameraRig
 ├─ CameraPivot
 │   └─ Main Camera
 └─ 전략카메라Controller
```

| 입력 | 동작 |
| --- | --- |
| `WASD` | 현재 Y축 방향을 기준으로 지면 이동 |
| Mouse Wheel | 최소·최대 거리 안에서 확대·축소 |
| `Q/E` | Y축 연속 회전 |
| Right Mouse Drag | Y축과 Pitch 자유 회전 |

이동은 `Time.unscaledDeltaTime`을 사용해 프레임률과 Simulation 일시정지에 종속되지 않는다. 현재 이동 중심 범위는 X -65~65, Z -50~50이고 Zoom 거리는 12~110이며 Inspector와 Scene 생성기에서 조정할 수 있다.

## 상태와 권위 경계

전략 카메라에는 자유 탐색과 배치 객체 초점 상태를 구분하는 Presentation 상태가 있다. 첫 단계에서는 자유 탐색 입력만 실제로 연결했다.

- 카메라는 서버 상태, `WorldTick`, 상태 버전과 업무 완료를 변경하지 않는다.
- 카메라 이동·회전·Zoom은 서버 API를 호출하지 않는다.
- Unity 애니메이션이나 카메라 도착은 업무 완료 근거가 아니다.
- UI EventSystem은 `InputSystemUIInputModule`을 사용하며 `StandaloneInputModule`을 두지 않는다.
- 우클릭 회전은 UI 위에서 시작하지 않는다.

## 다음 단계

1. 왼쪽 클릭 Raycast로 배치 객체 선택
2. UI 위 클릭일 때 World 선택 차단
3. 선택된 배치 객체의 초점 연결 지점으로 전환
4. `ESC`로 초점을 해제하고 자유 탐색으로 복귀
5. 타로 카드와 정보 패널은 선택 결과를 읽되 카메라가 업무 명령을 만들지 않도록 연결

## 검증

- 카메라 상태·경계·프레임 분할 독립성 EditMode 6/6 통과
- 저장된 `SimulationWorldShell` 계층·Input System·기존 World 회귀 11/11 통과
- PlayMode에서 Input System의 WASD·E·Mouse Wheel·Right Mouse Drag 입력 1/1 통과
- 입력 전후 `WorldTick`과 상태 버전 12 유지
- 1600×900 Game View 증거 저장

Play Mode 실행 중 로컬 예행연습 서버가 꺼져 있으면 기존 턴 마감 초기화가 연결 오류를 기록한다. 이는 카메라 입력 실패가 아니며 서버 연결 검증과 별도로 다룬다.
