# 농업 시뮬레이션 scenario v1 데이터 사전

이 문서는 `SchemaVersion=1.0.0`의 필드 의미와 권위를 정의한다. JSON은 Unity API model 또는 test fixture가 읽고, runtime에서는 `농업ScenarioPackage`로 변환·검증한다.

## 공통 규칙

| 항목 | 기준 |
| --- | --- |
| 식별자 | locale과 표시명을 포함하지 않는 colon 구분 stable ID |
| 금액 | `long` KRW 최소 단위, 계산 중간값은 `decimal` |
| 질량 | `decimal kg`, 원 단위 별도 보존 |
| 비율 | `decimal`, 0 이상 1 이하 |
| 현실 시각 | offset을 포함한 ISO 8601 |
| 게임 시각 | scenario 내부의 정수 `GameDay` |
| 결측 | `0`이나 빈 문자열로 대체하지 않고 validator에서 거부 |
| 변경 | 기존 version 덮어쓰기 금지, manifest version과 hash 갱신 |

## `ScenarioManifest`

| 필드 | 형식 | 책임 |
| --- | --- | --- |
| `ScenarioKey` | stable ID | 시나리오의 영구 식별자 |
| `ScenarioVersion` | semantic version | 시나리오 내용 version |
| `SchemaVersion` | semantic version | JSON·model 구조 version |
| `RuleSetKey` | stable ID | 계산 규칙 집합 식별자 |
| `RuleSetVersion` | semantic version | balance·계산 규칙 version |
| `DefaultRandomSeed` | int | 결정적 난수의 기본 seed |
| `ExpectedDataHash` | SHA-256 hex | manifest hash 자신을 제외한 package 내용 봉인 |
| `Mode` | `SIMULATED` | 운영 효과가 없음을 표시 |

## 기준정보와 규칙

| model | 주요 필드 | 권위·제한 |
| --- | --- | --- |
| `작물Definition` | crop/variety key, 성장량, 수확 기준, 수확량, 온도·수분 범위 | Unity rule package 소유, 공공 관측값 아님 |
| `성장단계Definition` | stage key, 최소 성장점 | 0부터 시작하며 수확 기준을 넘지 않음 |
| `토양Definition` | soil key, 초기 수분, 보수율, 일 손실률 | 첫 시나리오의 게임 규칙 |
| `비용항목Definition` | cost key, trigger code, KRW 금액 | 생산비 계산 규칙, 실제 견적 아님 |
| `판매방식Rule` | 방식 key, 가격·판매량 factor, 추가 비용, 노동·위험 code | 일반·공동판매 교육 비교 규칙 |
| `ExternalCodeMapping` | 외부 source/code, game key, mapping version·품질·근거 | 외부 code와 game 작물의 동일성을 직접 가정하지 않음 |

## 현실 관측과 fixture

| model | 주요 필드 | 권위·제한 |
| --- | --- | --- |
| `일별날씨Snapshot` | game day, 평균기온, 강수량, 각각의 evidence | 실제 관측 또는 명시적 fixture, 게임 날짜와 현실 날짜 분리 |
| `시장가격관측Snapshot` | observation/crop key, KRW/kg 값, evidence | 시장 단계 관측이며 실제 판매가격·견적 아님 |
| `데이터근거Envelope` | source·dataset·시각·지역·시장 단계·원 값·정규화값·품질·제한·hash | 외부 관측과 파생 계산의 추적 근거 |

## 실행 상태와 기록

| model | 주요 필드 | 변경 책임 |
| --- | --- | --- |
| `농업SimulationCommand` | command ID/code, game day, amount | 사용자 행동 또는 결정적 tick 입력 |
| `농업SimulationEvent` | sequence, event code, game day, amount/unit, 설명 code | engine이 append |
| `농업SimulationState` | 날짜, 수분, 성장, 단계, 수확량, 생산비, 판매 비교 | engine만 변경 |
| `판매비교Result` | 예상 판매량·단가·매출·비용·수익·노동·위험 | 관측가격과 rule에서 파생, 실제 거래 사실 아님 |
| `파생값Lineage` | evidence ID, rule version/hash, seed, 계산 시각, 설명 | 결과를 입력과 규칙까지 역추적 |

## v1 필수 검증

- stable ID와 외부 mapping은 유효하고 중복되지 않는다.
- 날씨는 1일부터 연속되며 기온·강수 값이 각 evidence의 정규화값과 일치한다.
- 관측 시각은 수집 시각보다 늦지 않다.
- `Fixture` 품질은 `Fixture` source type으로, 실제 검증 관측은 `PublicObservation`으로 표시한다.
- 가격은 `KRW/kg`, 기온은 `degC`, 강수량은 `mm`만 첫 version에서 허용한다.
- 일반판매와 공동판매 rule이 모두 존재한다.
- package hash가 manifest와 다르면 `Invalid`로 차단한다.
