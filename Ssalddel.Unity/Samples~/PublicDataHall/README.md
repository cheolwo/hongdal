# Public Data Hall Primitive Vertical Slice

`GET api/v1/community/world-map/observations?dataset=day-work`의 공개 관측을 정보관 marker로 투영한다.

- stable ID 기준 marker 생성·갱신·제거
- source, evidence 시각, 위치 정밀도, freshness와 boundary 보존
- 최초 실패는 빈 정보관, 갱신 실패는 마지막 성공 marker 유지
- Simulation과 Operational API를 LifetimeScope에서 명시적으로 선택

위도·경도는 정보관 내부의 단순 세계판 좌표로만 투영한다. `LocationPrecisionCode`보다 정밀한 실제 위치라고 해석하거나 표시하지 않는다.
