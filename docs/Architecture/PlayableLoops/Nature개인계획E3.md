# Nature 개인 계획 설정 — Logic E3

## 식별과 근거

- PlayableLoop: `playable-loop:nature-night-day2.v1`, 주제 `topic:nature-night-day2.v1`.
- revision: `nature-personal-plan.design.r1`, 상태: `Approved`.
- 승인 근거: 2026-08-30 사용자의 남은 32개 WI 순차 Logic E3 구현 목표 요청.
- WI: `WI-ACTOR-PLAN-SET`. [동결 문답 Q040~044](PlanningSessions/nature-night-day2.inquiry.r1.md)와 [32개 캠페인](신규32개WI논리E3캠페인.md)을 사용한다.
- 기존 열원 E3 및 보관·수면·Day2 증거는 보존한다. 이 묶음은 다음 계획 선택을 일반화할 계약이며 기존 WI-NATURE-15 구현·Save 의미를 즉시 변경하지 않는다.

## 플레이어 약속과 재미

플레이 중 시간·장소에 묶이지 않고 자신의 계획을 세우고 문구를 수정한다. 계획은 다른 행동을 잠그거나 자동 실행시키지 않는다. 기본 설명 용어는 `내면의 울림`이다.

## 반복 폐루프

내면 방향 감지 → 계획과 문구 선택 → Preview → Confirm → 현재 계획 및 변경 계보 확인 → 언제든 수정하거나 실제 행동 선택.

## 선택·대가·성공·실패·회복

신뢰된 초기 상태의 플레이어·계획 슬롯·허용 Objective ID와 글자 수 정책을 사용한다. 요청자는 Objective ID와 문구, ExpectedRevision, CommandId만 전달한다. 한 원장은 한 계획 슬롯의 좁은 Fixture이며 동시 계획 개수의 게임 규칙을 확정하지 않는다.

최초 설정은 한 번만 `InitialStabilityEligibility` 근거를 남긴다. 문구 변경·목표 왕복·Command 재전송은 그 근거를 재발급하지 않는다. 회복량·기여 상한·진척 연결 규칙은 미정이므로 수치 보상은 적용하지 않는다. 근거가 존재함과 실제 보상을 받음은 별도다.

알 수 없는 목표·다른 Actor·공백/초과 문구·같은 내용·낡은 개정·Command 입력 충돌은 무변경 거부한다. 유효한 새 Preview로 수정해 재시도한다. Confirm은 슬롯 내용·revision·행위 기록을 원자적으로 교체하며 자원·시각·전투·수면에는 영향을 주지 않는다.

## WI 단일 책임 후보

주 결과는 `PersonalPlanSet`이다. 계획 완료·행동 자동 추론·진척 회복·명상 성장·NatureMind Effect·수면은 이 WI에서 실행하지 않는다. E3는 계획 설정과 최초 안정 근거의 중복 방지 경계만 증명한다.

## 논리·표현 요구

- Logic E1: 신뢰 초기화·Query·Preview·Confirm·계획·일회성 근거 계약.
- Logic E2: 순수 규칙과 메모리 수명의 Application 원장.
- Logic E3: 무변경·멱등·경합·동등 문구·권한·개정·hash·사본 격리 시험.
- Presentation E1: 계획 내용·수정 결과·거부 이유와 `내면의 울림` 설명 요구. UI 구현 없음.
- 상한 Logic E3 / Presentation E1 / 통합 E1. 수치 보상 미정은 해당 분기로 남기고 다음 WI 구현을 막지 않는다.

## H 공간과 자산 요구

시간·장소 비종속 설정 계약은 이번 범위에서 H `NotApplicable`이다. 실제 계획 UI·음향·내면 연출은 후속 표현 연구가 필요하다. 공간 전담 worktree 결과는 의존하지 않는다.

## 전문 심화 연구 판정과 재결속

순수 계획 설정에는 공간·Rig·배치 연구가 `NotRequired`다. Q041~042 성장·회복 Profile과 Q043~044 내면 연출은 아직 이번 승인에 포함하지 않으며 다음 revision에서 연구·수치를 재결속해야 한다.

## 저장·권위·외부 경계

독립 메모리 수명 Core만 제공한다. Local/Remote Adapter·Save·Unity·기존 새벽 전용 구현을 바꾸지 않는다. 일회성 근거는 이 원장 수명 안에서만 보장하며 저장 이후 악용 방지까지 검증했다고 하지 않는다. Growth는 `NotApplicable:NumericRecoveryAndProgressPolicyUndefined`다.

## 제외 범위와 승인

Q340 이후, 출시용 목표 후보·문구 길이 수치, 실제 보상·계획 진척·다중 계획 슬롯 정책·E4 이상·화면·운영·commit·push는 제외한다. 고정 Fixture의 목표·글자 제한은 계약 시험용일 뿐 게임 난이도 결정이 아니다.
