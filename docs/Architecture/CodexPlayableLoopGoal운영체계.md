# Codex PlayableLoop Goal 운영 체계

## 목적

짧은 이동·대기 시간에 사용자와 Codex가 개발을 이어가더라도 큰 목적을 잃지 않도록 장기 Goal과 한 번에 수행할 WI를 분리한다. Goal은 플레이어가 경험하는 폐루프 하나만 소유하며 H·E·G·WI 전체 체계를 하나의 작업으로 뭉치지 않는다.

```text
Goal = PlayableLoop 하나
WI = 현재 구현할 단일 행동 책임, WIP 1
E = 완료를 입증하는 증거 성숙도
H = 현재 폐루프가 요구하는 공간 능력
G = 다음 E로 가기 위한 관리·검토 체계
EvidencePackage = 시험·저장·Runtime·화면·Hosted 증거 묶음
```

`AreaAggregate`와 `WorldAggregate`는 Goal이 아니다. 필수 `PlayableUnit` 자식의 상태에서 파생하는 완결 이정표다.

## 운영 원장

권위 입력은 [`codex-playable-loop-goals.json`](../../eng/execution-ledgers/codex-playable-loop-goals.json)이다. 검사기는 Goal WIP 1, WI WIP 1, PlayableUnit 전용 Goal, 목표 E7/E8, 대기열 순서와 기존 PlayableLoop·WI 원장 참조를 검증한다. 사람이 읽는 현재 `/goal` 입력과 상태 보고는 [`codex-playable-loop-goals.md`](../AI/generated/codex-playable-loop-goals.md)로 자동 생성한다.

현재 플레이어 연속성 우선순위는 다음과 같다.

```text
Nature Core
→ Nature Extension
→ Farm Core
→ Farm Extension
→ Hub Core
→ Town Core
→ Town Extension
→ City Core
```

이 순서는 영역 간 업무 의존이 아니다. Farm 결과를 Hub 시작 조건으로 사용하지 않으며 각 영역은 독립 Fixture·Save/Replay·공간 증거를 먼저 가진다.

## Goal 생명주기

1. 현재 `PlayableLoop`의 플레이어 약속과 목표 E를 Goal로 고정한다.
2. E9→E1 영향 검토에서 가장 낮은 미완료 의존성 WI 하나만 활성화한다.
3. WI를 필요한 증거 단계까지 올린 뒤 E1→목표 E 방향으로 다시 검증한다.
4. 새 영향이 나오면 같은 Goal과 작업 명세의 하향 검토를 다시 연다.
5. 플레이어 약속이 바뀌거나 독립 폐루프를 선택할 때만 Goal을 교체한다.

E9는 즉시 목표가 아니다. 안정된 E8 기준선에 대한 변경의 Migration·호환성·결정성·회귀를 검증할 때만 E9 승격을 검토한다.

## E7과 E8 종료 기준

- `E7 PlayClosed`: 실제 입력, canonical `SimulationWorldShell` Play Mode, Game View, 성공·실패·회복·귀환, 결정적 Save/Restore/Replay가 유효하다.
- `E8 WorldClosed`: E7에 더해 필요한 NPC가 판단→행동→결과→다음 판단으로 돌아오는 생활세계 폐루프가 닫힌다.
- `NpcRoutine`이 필수인 폐루프는 E8을 목표로 한다. `PlayerOrNpc`가 선택형인 폐루프는 플레이어 경로만으로 E7을 목표로 둘 수 있다.
- Scene·Synty 배치·문서·EditMode 성공만으로 E7이나 E8을 선언하지 않는다.

## 현재 Goal

현재 활성 Goal은 `playable-loop:nature-shelter-foundation.v1 → E7 PlayClosed`, 활성 작업은 `WI-NATURE-05 벌목 도끼 확보`다. 자동 실제 입력 전체 폐루프와 LocalProcess·RemoteHost 동등성에 더해 사람 입력의 취소·재시도·세 나무 벌목·오두막 건설·입실·퇴실·저장·Play Mode 재진입 복원과 Game View를 현재 EvidencePackage에 기록했다. FMOD 출력 장치 초기화 오류 60 때문에 실제 청음은 아직 없으므로 E7을 `Partial`로 유지한다. 승인 Nature Ambient·BGM은 E7 필수 조건이 아닌 선택 채널로 대기한다.

진행 보고는 항상 다음 순서를 사용한다.

```text
현재 WI / 현재 E단계 / 이번에 추가된 증거 /
남은 차단 항목 / 다음 최저 미완료 의존성
```

새 권위 부여, 외부 Provider 호출, 운영 쓰기, 범위가 다른 폐루프 추가 또는 기존 플레이어 약속 변경이 필요하면 구현을 멈추고 사용자 결정을 요청한다.
