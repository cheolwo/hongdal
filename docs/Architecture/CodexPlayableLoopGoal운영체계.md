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

권위 입력은 [`codex-playable-loop-goals.json`](../../eng/execution-ledgers/codex-playable-loop-goals.json)이다. 검사기는 Goal WIP 1, WI WIP 1, PlayableUnit 전용 Goal, 목표 E7, 대기열 순서와 기존 PlayableLoop·WI 원장 참조를 검증한다. 사람이 읽는 현재 `/goal` 입력과 상태 보고는 [`codex-playable-loop-goals.md`](../AI/generated/codex-playable-loop-goals.md)로 자동 생성한다.

현재 우선순위는 모든 Core를 먼저 닫고 Extension을 뒤에 여는 방식이다.

```text
Nature Core 5개
→ Farm Core 2개
→ Hub Core 2개
→ Town Core 1개
→ City Core 1개
→ Nature Extension 3개
→ Farm Extension 1개
→ Town Extension 1개
```

이 순서는 영역 간 업무 의존이 아니다. Farm 결과를 Hub 시작 조건으로 사용하지 않으며 각 영역은 독립 Fixture·Save/Replay·공간 증거를 먼저 가진다.

## Goal 생명주기

1. 현재 `PlayableLoop`의 플레이어 약속과 목표 E를 Goal로 고정한다.
2. E7→E1 영향 검토에서 가장 낮은 미완료 의존성 WI 하나만 활성화한다.
3. WI를 필요한 증거 단계까지 올린 뒤 E1→E7 방향으로 다시 검증한다.
4. 새 영향이 나오면 같은 Goal과 작업 명세의 하향 검토를 다시 연다.
5. 플레이어 약속이 바뀌거나 독립 폐루프를 선택할 때만 Goal을 교체한다.

Goal은 `activeMaturityTrackCode`로 현재 논리 또는 표현 궤적을 표시한다. 표현 실패가 권위 상태 누락에서 시작됐으면 같은 Goal을 유지한 채 논리 궤적으로 돌아가며, 통합 E는 [논리·시각 이중 순환 기준](플레이폐루프논리시각이중순환체계.md)에 따라 두 궤적 중 낮은 단계다.

E8~E10은 Goal의 다음 수직 목표가 아니다. 각 `PlayableUnit`이 E7을 통과하면 자기 E8 안정성 캠페인으로 인계한다. 같은 영역의 E8 Core 둘 이상이 준비됐을 때만 E9 조화·사람 승인 캠페인을 열고, 승인 후보만 E10 제한 운영으로 보낸다. 상세 기준은 [E1~E7 수직 폐루프와 E8~E10 수평 증거 체계](E1-E7수직폐루프와E8-E10수평증거체계.md)를 따른다.

## E7 종료와 E7 이후 인계

- `E7 PlayClosed`: 실제 입력, canonical `SimulationWorldShell` Play Mode, Game View, 성공·실패·회복·귀환, 결정적 Save/Restore/Replay가 유효하다.
- `NpcRoutine`이 필수인 폐루프도 해당 PlayableUnit의 수직 목표는 E7이다. NPC 생활 연속성은 관련 E9 `AreaHarmonySet`의 필수 조화 모듈로 다시 검증한다.
- Scene·Synty 배치·문서·EditMode 성공만으로 E7을 선언하지 않는다.
- E7 완료 뒤 Goal을 E8로 올리지 않는다. 해당 Goal을 완료하고 `post-e7-evidence-campaigns.json`의 개별 E8 안정성 캠페인을 연다.

## 현재 Goal

현재 활성 Goal은 `playable-loop:nature-shelter-foundation.v1`, 활성 WI는 보편 장착 계약의 `WI-ACTOR-02`, 활성 궤적은 `Presentation`이다. `playable-loop:nature-twilight-return.v1`은 완료 기록으로 유지한다. 다음은 Nature의 나머지 Core를 닫고 Farm·Hub·Town·City Core까지 순서대로 E7을 통과시킨 뒤 Extension으로 이동한다. 각 단위는 E7 직후 자기 E8 안정성 캠페인을 열 수 있지만, E9 영역 조화는 같은 영역의 E8 Core가 둘 이상일 때만 시작한다.

진행 보고는 항상 다음 순서를 사용한다.

```text
현재 WI / 현재 E단계 / 이번에 추가된 증거 /
남은 차단 항목 / 다음 최저 미완료 의존성
```

새 권위 부여, 외부 Provider 호출, 운영 쓰기, 범위가 다른 폐루프 추가 또는 기존 플레이어 약속 변경이 필요하면 구현을 멈추고 사용자 결정을 요청한다.
