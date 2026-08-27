# Nature 거점 성찰·다음 원정 준비

## 식별과 목적

- `loopStableId`: `playable-loop:nature-base-reflection.v1`
- 선행 폐루프: `playable-loop:nature-field-supply-return.v1`
- 완료 역할: `Extension`
- 최종 목표: 플레이어 전용 `E7 / PlayClosed`
- 실행 단위: `WI-REFLECT-01 승인 자료로 거점 성찰 확정`
- 설계 기준 상태의 단일 출처: `eng/execution-ledgers/playable-loops.json`

플레이어가 Nature 바깥 활동에서 돌아온 뒤 그냥 쉬거나, 승인된 자료를 매개로 오늘 행동을 성찰하고, 다음 활동에서 위험 Preview와 후퇴·회복·제작·위임 선택을 조금 더 잘 읽을 수 있게 하는 선택형 폐루프다. 특정 시간대나 `저녁학당`이라는 장소를 강제하지 않는다.

## 플레이어 약속

> 원정 결과를 가지고 거점에 돌아오면 그냥 쉬거나 승인 자료로 오늘의 행동을 성찰할 수 있고, 성찰한 경우 다음 원정의 선택 정보가 영구 내면 성장에 따라 달라진다.

```text
발산(陽): 탐사·채집·전투·구조
  → 거점 귀환
  → 수렴(陰): 휴식 또는 오늘 행동 성찰
  → 다음 활동에서 달라진 위험·후퇴·회복·제작·위임 선택
  → 새 발산
```

선택은 다음 셋이다.

1. `RestOnly`: 자료 없이 그냥 휴식한다. 학습 보상은 없다.
2. `ReflectOnToday`: 세션 시작 때 동결한 승인 Publication 하나를 골라 성찰한다.
3. `OpenOptionalSource`: 선택적으로 원문 링크를 연다. 시청·재생·체류 시간은 보상 근거가 아니다.

## WI와 단일 책임

`WI-REFLECT-01`은 `SimulationNative / PlayerDirect / Yin / -+`이다. 한 번의 Confirm이 소유하는 주 결과는 `InnerLearningPending` 하나이며, 다음 활동 경계에서 `InnerLearningApplied`로 전이한다.

- 하루에 보상 성찰 한 번만 허용한다.
- 한 캐릭터는 같은 `PublicationStableId + Revision`의 보상을 한 번만 받는다.
- 첫 허용 효과는 `Awareness +1 / BeginnerMind`와 `Resolve +1 / IntegratedProgress`뿐이다.
- 효과는 위험 정보, 후퇴·회복·제작·위임 선택의 설명·해금에만 쓰며 공격력, 생산량, 가격, 타로 방향·배율을 직접 바꾸지 않는다.
- 보상은 게임 안의 명시적 성찰 선택에 귀속한다. YouTube 재생 상태나 시청 시간을 입력으로 받지 않는다.

## 승인 학습자료 3계층

| 계층 | 역할 | Simulation 사용 여부 |
| --- | --- | --- |
| `youtube-learning-source-observation.v1` | 제목·채널·조회 시각·선택 근거 구간 hash·이용 한계를 보관한다. 원문 전체와 API key는 제외한다. | 직접 사용 금지 |
| `learning-interpretation-candidate.v1` | 분류·요약·성찰 질문·내면 효과 후보를 만든다. | 후보이므로 사용 금지 |
| `approved-learning-material.v1` | 사람이 승인한 stable ID·revision·hash·허용 효과를 게시한다. | 명시적 동기화 뒤 사용 |

기존 `hongik-unity-learning-card-publication.v1`과 Unity의 `저녁학당*` 형식은 삭제하지 않는다. 새 공용 Publication으로 검증·변환하는 호환 어댑터 대상으로 남긴다. 기존 `콘텐츠시청CommandHandler`의 시청 시간 포인트·프로모션 흐름은 이 폐루프와 연결하지 않는다.

Apify 또는 YouTube Adapter는 운영자 수집 경계에서만 호출한다. Simulation 세션 생성·Tick·Preview·Confirm·Unity 조회는 동결된 Simulation 파생 원장만 읽는다.

## 공간 요구

| H | 판정 | 요구 |
| --- | --- | --- |
| H1 | Required | `ReflectionInteractionAnchor`; 플레이어 접근·점유·상호작용 한 자리 |
| H2 | Reuse | 기존 오두막 또는 생활 거점 내부 |
| H3 | Reuse | `area-set:sim:pyeongchang:nature-home.v1`의 Nature home 이동·귀환 구조 |
| H4 | Reuse | 기존 Nature AreaSet |
| H5 | NotRequired | 이 독립 폐루프는 새 세계 배치를 요구하지 않는다. |

H1의 실제 Synty 배치와 Unity 입력은 E4 이후 작업이다. 새 공식 Scene을 만들지 않고 canonical `SimulationWorldShell`에서 검증한다.

## 상태와 저장 경계

세션 시작 시 다음을 불변 사본으로 고정한다.

- 승인 파생 원장 revision과 입력 hash
- 승인 Publication stable ID·revision·publication hash
- 플레이어 내면 상태
- Publication revision별 지급 원장
- 보상 성찰을 완료한 일차
- 적용 대기·적용 완료 효과 계보

현재 구현은 공용 계약, 멱등 파생 원장, 결정적 Preview·Confirm·다음 활동 적용과 상태 hash를 갖춘 `E1~E3` 뼈대다. `simulation-save.v24` 주 세션 통합은 LocalProcess·RemoteHost Adapter와 같은 작업으로 E4에서 연결한다. v23 이하 정규형과 hash를 먼저 보존해야 하므로 이번 단계에서 기존 저장 Aggregate를 억지로 변경하지 않는다.

## 폐루프 완료 기준

- E3: 승인 자료 hash·중복·변조 거부, 하루/Publication revision 제한, 원문 열기 무보상, 상태 복원 hash, 동일 입력 Local/Hosted 계산 동등성 자동 시험.
- E4: `WI-REFLECT-01`과 H1 `ReflectionInteractionAnchor`, 플레이어, 승인 Publication, 다음 활동 경계를 결속.
- E5: 실제 Nature 거점 상태에서 Preview → Confirm → 다음 활동 적용 → 다음 원정 선택 반환.
- E6: 원문·승인·한계와 보상 이유를 오해 없이 설명하고 영상 시청 보상처럼 보이지 않게 정제.
- E7: `SimulationWorldShell` 실제 입력, 저장 재진입, LocalProcess·RemoteHost revision별 hash, Game View·Console 증거로 `PlayClosed` 판정.
- E8: E7 완결 뒤 이 PlayableUnit 자체의 반복 결정성·Save 재진입·Local/Remote·실제 입력 안정성을 별도 캠페인으로 검증한다. NPC 학습 기능은 `playable-loop:nature-building-learning.v1`이 별도 PlayableUnit으로 소유하고, 두 폐루프의 생활 조화가 필요할 때 E9 AreaHarmonySet에서 만난다.

## 이번 구현의 제외 범위

- 실제 YouTube·Apify Provider 호출과 API key
- 운영 DB Migration 적용과 배포
- Unity H1·UI 구현, Play Mode, Game View
- NPC 학습과 장기 목표
- 공격력·생산량·가격·타로 수치 변경
