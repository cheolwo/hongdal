# Unity 턴 카드 모판 정리

## 결과

현재 경영일 턴 카드 흐름을 에셋 모판과 같은 연구·승격 구조로 정리했다. 철학·학당, 지역문화, 경영사건과 공공관측 모판을 분리하고 카드 후보가 서버 게임 덱으로 옮겨지기 전 거쳐야 할 C0~C6 Gate를 확정했다.

- C0 카드 씨앗
- C1 출처 메타데이터 확인
- C2 내용·사람 검수
- C3 효과 규칙 검증
- C4 모판 화면 검증
- C5 게시 snapshot 확정
- C6 게임 덱 이식

## 현재 표본 분류

- 바보·전차: 철학·학당 모판 C3와 Fixture C4까지 검증, 실제 승인 게시 C5는 0건
- 서울 생활문화 질문: 지역문화 모판 C1·C3와 Fixture C4까지 검증, 특정 행사형 C2·C5는 미통과
- 카드 없이 넘기기: 카드가 아닌 덱 제어

## 화면과 실행

후속 `TURN-CARD-SEEDBED-UI-1`에서 실제 게임 덱과 분리된 `턴카드모판` Scene을 구현했다. 철학·학당과 지역문화 모판을 전환하며 후보별 C0~C6 상태, Fixture/게시 구분, 출처·효과 revision, 확인·미확인 범위와 승격 차단 사유를 표시한다.

대표 PNG는 Unity 프로젝트의 다음 경로에 보존했다.

- `Assets/Documentation/Changes/2026-08-11-turn-card-seedbed-ui-1/philosophy-academy-game-view.png`
- `Assets/Documentation/Changes/2026-08-11-turn-card-seedbed-ui-1/regional-culture-game-view.png`

Scene에는 턴 마감 authority와 Preview·Confirm 버튼이 없다. 실제 Play Mode 전환 뒤 Console 오류 0건, 집중 EditMode 3/3과 전체 222/223을 확인했다. 전체 실패 1건은 기존 연구 Scene 기대 개수 27과 현재 28의 불일치다. 서버 contract와 실제 카드 publication은 변경하지 않았다.
