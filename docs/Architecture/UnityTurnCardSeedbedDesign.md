# Unity 턴 카드 모판 설계

- 기준일: 2026-08-11
- 구현 상태: `TURN-CARD-SEEDBED-0` 분류·승격 Gate와 `TURN-CARD-SEEDBED-UI-1` 실제 Unity 모판 화면 완료
- 관련 구현: `TURN-0`, `TURN-CARD-UI-1A`, `TURN-CARD-UI-1B`, `CULTURE-CARD-0`, `TURN-CARD-HTTP-1`

## 1. 목적

턴 카드는 아이디어가 생겼다는 이유만으로 다음 날 경영 효과를 갖지 않는다. 에셋을 모판에서 연구한 뒤 Scene으로 옮겨 심듯이, 철학·학당·문화·경영사건 카드도 별도 **턴 카드 모판**에서 출처, 내용, 적용 기간, 효과 규칙과 화면 표현을 검증한 뒤 서버 canonical catalog로 승격한다.

```text
카드 씨앗
  ↓
분야별 턴 카드 모판
  ├─ 원문·출처 후보
  ├─ 사람 검수 상태
  ├─ 지역·기간·대상 범위
  ├─ Simulation 효과 규칙
  ├─ 알 수 있는 것과 없는 것
  └─ 게시·게임 이식 Gate
        ↓
서버 게시 catalog
  ↓
턴 context·Preview·Confirm
  ↓
canonical session 재조회
  ↓
다음 경영일 효과
```

모판은 플레이어에게 바로 배포되는 카드 덱이 아니다. 후보와 검증 상태를 안전하게 비교하는 연구 공간이며, 모판에 있다는 사실만으로 게시 승인이나 게임 효과 활성화를 뜻하지 않는다.

## 2. 모판 구분

| 모판 | 현재 표본 | 검증할 것 | 금지하는 추론 |
| --- | --- | --- | --- |
| 철학·학당 모판 | 바보·전차 | 영상·자막 구간, 사람 승인, 일반 철학 의미와 학당 해석의 출처 분리, effect revision | 타로 일반 의미나 LLM 해석만으로 경영 수치 생성 |
| 지역문화 모판 | 서울 생활문화 질문 | 지역, 유효 기간, 공식 원천, 주민 경험 질문, 행사별 원문과 사람 검수 | 기관 관계정보만으로 특정 행사·지역 대표성 주장 |
| 경영사건 모판 | 향후 가격·재고·노동·날씨 사건 | canonical 원장 입력, 확률·seed, 비용·위험, 적용 대상과 기간 | 화면 문구나 에셋 외형으로 운영 상태 변경 |
| 공공관측 모판 | 향후 계절·기상·시장 관측 카드 | source revision, 관측 시각·단위·지역·결측, Simulation 해석 규칙 | 관측값을 실제 작업 명령이나 확정 권고로 변환 |

모판마다 검수 방식은 다르지만, 서버 효과 규칙과 게시 snapshot 없이는 활성 게임 카드가 될 수 없다는 Gate는 같다.

## 3. 카드 승격 단계

| 단계 | 한국어 상태 | 통과 조건 |
| --- | --- | --- |
| C0 | 카드 씨앗 | 카드가 묻는 질문, 분야, 의도와 금지 효과를 기록 |
| C1 | 출처 메타데이터 확인 | source stable ID·URL·revision·지역·기간·이용 조건을 기록 |
| C2 | 내용·사람 검수 | 실제 원문·자막·행사 자료를 대조하고 승인자·승인 시각·revision을 보존 |
| C3 | 효과 규칙 검증 | 서버가 허용 효과·수치·대상·기간을 versioned rule로 계산하고 Preview 무변경·Confirm 후 다음 턴 적용을 테스트 |
| C4 | 모판 화면 검증 | 후보·미확인·차단·승격 가능 상태와 알 수 없는 것을 한국어 Game View에서 확인 |
| C5 | 게시 snapshot 확정 | immutable publication hash와 asset·원문·검수·effect lineage를 서버 catalog에 게시 |
| C6 | 게임 덱 이식 | 서버 턴 context에 노출하고 Confirm 뒤 canonical session 재조회로 다음 경영일 효과를 적용 |

`C0~C4`는 연구·Fixture 단계일 수 있다. 실제 게시 카드는 `C5`를 통과해야 하며, 플레이 가능한 덱 편입은 `C6`에서 별도로 검증한다. 개발용 Fixture는 화면과 계약을 검증할 수 있지만 승인 게시물을 대신하지 않는다.

## 4. 현재 카드의 위치

| 카드 | 모판 | 현재 단계 | 현재 가능한 것 | 아직 불가능한 것 |
| --- | --- | --- | --- | --- |
| 바보 `BeginnerMind` | 철학·학당 | C3·Fixture C4 검증, C5 미통과 | Preview의 미확인 질문 보강과 다음 턴 Fixture 효과 검증 | 실제 승인 학당 카드 게시, 운영 추천 |
| 전차 `IntegratedProgress` | 철학·학당 | C3·Fixture C4 검증, C5 미통과 | 물류 milestone 보강과 다음 턴 Fixture 효과 검증 | 실제 승인 학당 카드 게시, 수량·속도 자동 변경 |
| 서울 생활문화 질문 | 지역문화 | C1·C3·Fixture C4 검증, 행사형 C2·C5 미통과 | 특정 사실을 주장하지 않는 질문과 `LocalContextAwareness` Simulation 효과 | 특정 서울 행사·계절 문화의 사실 게시와 대표성 주장 |
| 카드 없이 넘기기 | 덱 제어 | 카드 아님 | 아무 카드 효과 없이 턴 마감 | 모판·게시 단계 적용 대상 아님 |

현재 WorldShell에서 세 표본을 선택할 수 있는 것은 `Simulation Fixture 덱`을 검증하기 위한 것이다. 실제 승인 카드 catalog가 준비됐다는 뜻으로 표시하지 않는다.

## 5. 권위와 데이터 경계

- 모판 Catalog는 후보·출처·검수·효과 규칙의 진행 상태를 읽는 연구 projection이다.
- Unity는 카드 수치와 효과를 만들지 않으며 서버 effect rule revision을 표시한다.
- LLM은 질문이나 설명 초안을 제안할 수 있지만 승인, 출처 판정, 수치 효과와 게시 권위를 갖지 않는다.
- 실제 승인 catalog가 비어 있으면 빈 덱이 정상이며 Fixture로 조용히 채우지 않는다.
- 카드 Preview는 session을 바꾸지 않고 Confirm 뒤에도 canonical session 재조회 결과만 적용한다.
- 문화·공공관측 결측을 일반 상식이나 다른 지역 자료로 보완하지 않는다.

## 6. 다음 세로 구현

`TURN-CARD-SEEDBED-UI-1`은 실제 게임 덱과 분리된 한국어 모판 화면으로 구현했다.

1. 철학·학당 모판과 지역문화 모판을 직접 선택한다.
2. 각 카드에 C0~C6 상태, Fixture/게시 구분, source·effect revision과 차단 사유를 표시한다.
3. 모판에서는 턴 Confirm을 제공하지 않고 `게임 덱 승격 후보`만 표시한다.
4. 실제 게시 catalog가 0건이면 그대로 0건으로 보여 준다.
5. Game View와 집중 테스트에서 모판 후보가 canonical session을 변경하지 않음을 증명한다.

실제 `턴카드모판` Scene은 철학·학당 후보 2장과 지역문화 후보 1장을 전환하며 C0~C6 상태와 차단 사유를 표시한다. Scene에는 `턴마감Presenter`, Preview·Confirm 버튼과 session authority가 없고 연구 revision 0만 유지한다.
