# Nature 자연 흔적 조사 — Logic E3 준비

## 식별과 근거

- 대상 PlayableLoop: `playable-loop:nature-regional-threat-recovery.v1`, 주제 `topic:nature-regional-threat-recovery.v1`.
- WI: `WI-NATURE-TRACE-INVESTIGATE`, 기획 revision `nature-trace-investigation.design.r1`, 상태 `ReadyForReview`.
- 요청 근거: 2026-08-30 사용자 [32개 WI Logic E3 순차 구현 캠페인](신규32개WI논리E3캠페인.md). 이 문서의 작업 목록·승인 hash 결속은 공통 원장 담당자가 수행하기 전까지 미등록이다.
- 원문: [지역 오행 몬스터 문답 Q164~166](PlanningSessions/지역오행몬스터/region-five-elements-monster.inquiry.r1.md). 현재 승인된 다른 WI와 증거를 제거하지 않는다.

## 플레이어 약속과 재미

흔적을 조사해 위협을 더 잘 이해하고 다음 준비·경로를 선택한다. 낮은 명상 숙련도도 조사할 수 있다. 같은 버튼을 반복해 미관찰 사실을 얻거나 모든 정답을 즉시 공개하지 않는다.

## 반복 폐루프

접근 가능한 흔적 발견 → Preview → 조사 Confirm → 출처 있는 관찰 근거와 허용된 정보 확인 → 추가 조사·장비 준비·후퇴 중 다음 선택. 첫 묶음은 조사 결과를 지식 원장에 남기는 단일 WI만 구현하며 실제 전투·경로 이동은 연결하지 않는다.

## 선택·대가·성공·실패·회복

신뢰된 초기 상태에 플레이어, 대상 Profile의 관찰 기준 revision, 흔적 SourceStableId·접근 가능 여부, 관찰 시각·지역·날씨·신뢰 범위 및 지식 공개 정책을 동결한다. 요청은 플레이어·흔적 ID·ExpectedRevision·CommandId만 보낸다. 외부 요청이 비율·근거 종류·신뢰도·숙련도·공개 단계를 지정할 수 없다.

처음 보는 접근 가능한 흔적을 확정하면 그 근거·WorldRevision·행위 기록을 원자적으로 추가한다. 같은 출처를 새 Command로 반복해도 새 진척을 만들지 않고, 같은 Command 재전송은 최초 결과를 재사용한다. 잘못된 주체·출처·접근·개정·입력 충돌은 무변경 거부하고 다른 유효한 흔적이나 재조회를 선택할 수 있다. 비용·명상 성장량·회복량은 미정이며 지급하지 않는다.

## WI 단일 책임 후보

주 결과는 `NatureTraceInvestigated`다. 이 WI는 흔적 관찰만 추가하며 직접 관찰·전투 경험·분석·공유 행위를 대신 수행하지 않는다. 다른 종류의 기확보 근거는 신뢰된 초기 상태로만 소비할 수 있다. 처방 전용 `SimulationPlayerKnowledge`의 Recipe 계약이나 Save를 오행 지식으로 확장하지 않는다. 공통 행위 원장만 재사용한다.

## 논리·표현 요구

- 미관찰: 정확한 구성 비율·배율은 반환하지 않는다.
- 첫 관찰: 승인된 근거의 경향 설명만 반환한다.
- 반복 조사: 출처와 넓은 신뢰 범위를 반환할 수 있지만 동일 근거를 중복 집계하지 않는다.
- 상세 정보: 현재 Profile revision에 결속된 서로 다른 근거가 정책 관문을 충족해야 한다. 단일 흔적 또는 같은 종류 반복만으로 전체 비율을 공개하는 정책은 거부한다. 전투 근거를 필수 조건으로 고정하지 않는다.
- 오래된 관찰은 지우지 않되 현재 상세 공개 근거로 승격하지 않는다. 관찰 기준 시각·revision과 불확실성을 별도 표시한다.
- 명상 수준만으로 근거·정확한 사실을 생성하지 않는다. 수치적 해석 보정은 미정으로 보류한다.
- Logic E1 계약 → E2 규칙·Application → E3 무변경·멱등·중복 출처·권한·개정·정보 누출·사본 격리·결정성 시험. Presentation은 E1 판독 요구뿐이다.

## H 공간과 자산 요구

흔적 관찰 H1·실제 접근 거리·Synty 흔적 Prefab·카메라·동작·오디오는 후속 연구 대상이다. 이번에는 신뢰된 접근 가능 상태만 소비한다. 해당 값은 Fixture이며 실제 Physics/거리 시험이 아니다. 새 Scene·공간 배치는 하지 않는다.

## 전문 심화 연구 판정과 재결속

출처 중복 방지·권위 원장·정보 공개 경계의 독립 논리 시험에는 공간 연구가 `NotRequired`다. 기획에서 미정인 진척량·공개 문턱·신뢰 범위 감소량·명상 배율은 생산 정책으로 만들지 않는다. 시험 정책은 명시적으로 제공하되 단일 흔적 상세 해금 금지 등 문답의 불변 규칙을 우회할 수 없다. 실제 수치·범위 계산과 표현은 후속 전문 연구 및 revision에서 재결속한다.

## 저장·권위·외부 경계

독립 메모리 원장의 동일 실행 수명만 증명한다. 저장 판본·Recipe API·RemoteHost·공유 지식 동의·실제 시계·Provider는 변경하지 않는다. 플레이어 성장 적용은 `NotApplicable:TraceProgressionPolicyUndefined`로 기록하며 기존 명상·회복 권위를 대체하지 않는다.

## 제외 범위와 승인

예정 상한 Logic E3 / Presentation E1 / 통합 E1. 기획 승인·작업 명세 hash·수정 소유가 공통 작업 목록에 결속되기 전에는 구현을 활성화하지 않는다. 비중첩 예정 파일은 `Simulation자연흔적조사Contracts.cs`, `Simulation자연흔적조사.cs`, `Simulation자연흔적조사Service.cs`, `Simulation자연흔적조사Tests.cs`다. Q340 이후·Save·Unity·E4 이상·commit·push 제외.
