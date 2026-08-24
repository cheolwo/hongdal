# 현재 완료·미완료·보류 원장

> 기준일: 2026-08-24
> 이 원장은 최신 판단 snapshot이다. 시험 숫자와 runtime 근거는 실행 시점이 다르므로 서로 합산해 “전체 완료”로 표현하지 않는다.

## 완료 또는 현재 기준으로 확정

| 항목 | 확인된 근거 | 한계 |
| --- | --- | --- |
| E/G 정의 | E1~E9·G1~G4 문서와 기계 대장 | 정의가 실제 단계 달성을 뜻하지 않음 |
| E9 우선 수직 작업 체계 | E9→E1 계획·E1→E9 검증 프로토콜, 템플릿, 검사기 | 목표 E9 선언이 실제 E9 증거를 뜻하지 않음 |
| WI 계약·구현 재고 | 48 WI, 구현 E3 | E4 실행 문맥·E5 세계 발현은 WI별 별도 판정 |
| Simulation core 자동 시험 | 이전 작업 기록의 708/708 통과 | 이번 문서 작업에서 재실행하지 않음 |
| Save/Replay core | Command log, schema, hash 경계와 시험 | Nature↔Farm 전체 E7 replay 미완료 |
| 공간 조립 호환 출력 | Nature, Farm, Hub, Town AreaSet Available | 단독으로 WI E5·E6·E7을 증명하지 않음 |
| canonical Scene 정책 | `SimulationWorldShell`, 단일 Build Settings 진입점 | 이번에 저장 Scene hierarchy 미재확인 |
| 최소 HUD | 이전 실제 Play Mode·Game View 근거 | 전체 플레이 폐루프 근거가 아님 |

## 부분 완료

| 항목 | 현재 위치 | 닫아야 할 조건 |
| --- | --- | --- |
| Farm 공간·WI | Farm 공간 자료와 AreaSet 조립 출력, FARM-04~06 E4 부분 결속·E5 부분 발현 | E4 전체 문맥·E5 결정적 폐루프·Farm 내부 반환·E7 |
| Nature 공간·WI | AreaSet 조립 출력, H 설계 선택, NATURE-01~11 E4 부분 결속 | NATURE-05~11 E5 발현·E6 정제·E7 왕복 |
| Nature 생존 E9 작업 명세 | 현재 증거 E3, Solo 공통 Core·WI 문맥·배치 영향 분해 | E4 전체 문맥부터 E9 변화 기준선까지 차단 상태 |
| Farm↔Nature 기준 플레이 | 기획·공간 추적 지도 | 같은 Session 이동·행동·귀환·Save/Replay |
| 카드 서랍·입력·카메라 | Unity 조립 코드와 일부 시험 | 실제 서버 상태와 Game View 전체 흐름 |
| Hub 공간 | 공간 조립 호환 출력, WI-001/002 공간 모판 | E4 문맥·E5 세계 발현과 독립 입고·보관·복구·출고 폐루프 |
| Town 시장 | WI 자동 시험 E3, 공간 조립 호환 출력 | E4 문맥·E5 세계 발현과 독립 시장 폐루프 |

## 미완료

- 첫 실제 E7 Nature 생존 생활거점 독립 폐루프
- Farm 포장 뒤 독립 내부 보관·다음 생산 반환 계약
- Nature WI 05~11의 허용 발생원·실행 문맥과 결정적 E5 세계 발현
- Unity `Server` 모드의 Preview→Confirm→Tick→재조회 전체 실행 증거
- 같은 Session의 Save/재실행/Replay Hash와 Game View·Console 묶음
- 지속 NPC 정체성·욕구·기억·자율 선택을 포함한 E8 폐루프
- 실제 변화 제안·영향 분석·Migration·하위 회귀·재승격을 포함한 E9 사례

## 보류 또는 독립 준비 후 통합

- Farm→Hub 실제 HTTP 통합
- Hub→Town 통합
- Farm→Hub→Town 대표 수직 슬라이스
- City 독립 영역 활성화와 다른 영역 연결
- 계절·경제·신규 AreaSet 같은 첫 E9 변화 단위
- 운영 DB 효과·실제 공급자 호출·배포

보류는 불필요하다는 뜻이 아니다. 각 영역의 독립 내부 폐루프와 권위 계약이 준비되기 전 대표 경로로 삼지 않는다는 뜻이다.

## 증거를 섞지 않는 표기

```text
코드 존재
≠ 자동 시험 통과
≠ 공간 조립 호환 출력
≠ E4 실행 문맥 결속
≠ E5 WI 세계 발현
≠ E6 현실 근거
≠ 실제 서버 실행
≠ Play Mode / Game View
≠ 운영 DB 효과
≠ commit / push / deploy
```

## 현재 최우선 실행 묶음

1. Nature 도끼·벌목·오두막 WI의 허용 발생원·주체·대상·자원·시간과 Required 공간 문맥을 결속해 E4를 닫는다.
2. 결정적 Local Runtime Fixture에서 Preview/Confirm→권위 전이→Task/Effect→결과→후속 선택을 닫고 공간 조립 증거를 함께 검사해 E5를 판정한다.
3. `SimulationWorldShell`에서 서버 없는 Solo와 Hosted의 같은 생활거점 흐름을 플레이한다.
4. Save/Replay와 Game View·Console을 함께 남겨 Nature 독립 E7을 판정한다.
5. Farm 내부 반환을 독립적으로 닫고, 양쪽이 준비된 뒤 Nature↔Farm 통합을 별도 선택한다.
