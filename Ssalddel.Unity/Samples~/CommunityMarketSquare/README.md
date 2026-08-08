# Community Market Square primitive

공개 게시판, 공개 게시글 요약, 비식별 활동 신호, 권한이 적용된 원장 요약을 하나의 Unity Zone으로 투영하는 코드 샘플이다.

- 기본 모드는 `SIMULATED`이며 실제 운영 데이터처럼 표시하지 않는다.
- Operational 모드는 공개 `GET api/v1/community/world/zones/community-market-square`만 읽는다.
- 작성자 식별자, 연락처, 댓글 본문, 원장 담당자와 실행 행동은 Unity 공개 계약에 포함하지 않는다.
- 갱신 실패 시 마지막 성공 Snapshot과 기존 GameObject를 유지한다.
- `Ssalddel/Samples/Create Community Market Square Primitive`에서 primitive 씬을 생성할 수 있다.
