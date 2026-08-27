# Nature 논리·시각 이중 순환과 r5 통나무 표현

## 결과

Nature 폐루프의 논리 실행과 실제 화면 식별을 별도 성숙도 궤적으로 관리하도록 변경했다. 도끼·벌목·오두막과 황혼 위협은 논리·표현 E7로 닫았고, 보관·수면·Day2와 작업대는 논리 E7·표현 E6로 남겼다.

| 변경 축 | 결과 |
| --- | --- |
| Simulation | r5 벌목 뒤 `DroppedTimber` 생성, `WI-NATURE-18` 별도 획득, 용량 차단·멱등 처리 |
| 저장·재생 | `simulation-save.v24`에 지면 통나무와 hash 계보 포함, r1~r4 호환 유지 |
| 공간 | 전용 H2가 없을 때 Farm 원점이 아니라 canonical H의 `NatureHome` 기준점 사용 |
| Synty | Garden Shed 오두막, 통나무, Skeleton, Table Saw를 시각 자료 묶음으로 결속 |
| UI | 큰 진단 상태판을 기본 접힘으로 바꾸고 현재 문맥과 핵심 입력만 유지, F8로 진단 확장 |
| 검증 | 서버 45/45, Unity EditMode 14/14, PlayMode 비저장 5/5와 저장·복원 집중 1/1 |

## 대표 화면

- `C:/Users/user/ssalddel/Assets/Documentation/Changes/2026-08-26-nature-dual-loop-visual/nature-wi18-dropped-timber-game-view.png`
  - Synty 통나무 세 묶음, 실제 왼쪽 버튼 획득 전 상태
  - SHA-256 `777432e44a61a61418ffdd4697084b7c0b874de2e7bbb3cf64def26a2c38e6be`
- `C:/Users/user/ssalddel/Assets/Documentation/Changes/2026-08-26-nature-dual-loop-visual/nature-synty-cabin-compact-hud-game-view.png`
  - Nature H 영역의 Synty Garden Shed와 접힌 핵심 HUD
  - SHA-256 `85773cbdc0a16b21ccd0cd0eda0499afb7b66d8851b259db2496d10c40bb553a`

## 증거 판정

| 폐루프 | 논리 | 표현 | 통합 |
| --- | --- | --- | --- |
| Nature 도끼·벌목·오두막 기초 | E7 | E7 | E7 PlayClosed |
| Nature 황혼 위협 대응·귀환 | E7 | E7 | E7 PlayClosed |
| Nature 보관·수면·Day2 반환 | E7 | E6 | E6 Open |
| Nature 작업대 기반 | E7 | E6 | E6 Open |

보관·수면·새벽 화면은 지형·카메라 가림과 상태별 공간 변화 부족 때문에 E7로 올리지 않았다. 작업대는 Table Saw 자체는 보이지만 건설 중과 운영 중 작업 구역의 차이가 부족하다.

## 제한

- 사람 수동 조작에 의한 최종 미감·청음 수용은 수행하지 않았다.
- Unity 시험 도구 실행 중 Job lock 진단이 반복돼 전체 Console 무오류 증거는 사용하지 않는다. 재컴파일 오류는 0개다.
- 실제 Provider·운영 DB·RemoteHost Unity 실행·새 공식 Scene은 범위 밖이다.
- 원본 Synty Prefab은 변경하지 않았다.
