# 플레이어 감각 표현축과 Nature 벌목 표현

## 변경

- `placement-control-hierarchy.v4`에 Graphics·Camera·Animation·Lighting·Audio·UI 교차 표현축을 추가했다.
- 배치 기준점 계약 `placement-presentation-bindings.v1`과 WI 상태별 계약 `wi-presentation-plan.v1`을 분리했다.
- Nature 도끼·나무에 Camera Focus, Work, Tool Socket, 3D Audio와 FX 기준점을 Runtime에서 결속했다.
- 도끼 획득, 벌목 타격, 취소와 나무 낙하를 권위 상태 사본에서 Animation·Audio·FX Intent로 투영했다.
- 승인 음원 파일이 없어 도끼·나무 효과음은 절차형 fallback을 사용하고, Ambient·BGM은 선택 채널 미결 상태로 기록했다.
- canonical Scene에 Listener가 없으면 Player 위치에 프로젝트 소유 Audio Listener를 한 번만 만들고, 상태 공급자가 비활성화되면 Presenter도 권위 사본 읽기를 멈춘다.

## 권위 경계

- Animation Event·Audio·FX는 Simulation Command, WorldTick, 작업 완료나 재고 변경을 호출하지 않는다.
- 표현 계획과 기준점 hash는 Simulation save/replay canonical hash에 포함하지 않는다.
- Synty 원본 Prefab·Material은 수정하지 않는다.

## 검증 수준

- .NET 계약 집중 시험 통과
- Unity EditMode 상태 투영과 배치 계층 v4 집중 시험 통과
- canonical `SimulationWorldShell` 자동 PlayMode에서 도끼 획득→타격→취소→완료 표현 Adapter 통과
- 기존 도끼 실제 Input System 회귀 시험 통과
- 자동 PlayMode 종료 뒤 Script Runtime 예외와 Audio Listener 누락 경고 없음
- 연결된 Editor가 없어 수동 Game View·실제 청음·최종 PNG는 미검증
