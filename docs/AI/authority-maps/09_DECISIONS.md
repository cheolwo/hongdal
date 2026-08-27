# 확정 결정 진입점

이 파일은 전달 묶음의 고정 번호를 위한 진입점이다. 장기 결정의 단일 기준과 대체 관계는 [`docs/AI/DECISIONS.md`](../DECISIONS.md)에 기록한다. 이 파일은 결정을 복제하거나 별도 번호를 발급하지 않는다.

## 이 묶음에서 반드시 유지할 결정

- E는 증거 성숙도이고 G는 다음 E 관문을 준비·검증하는 관리 체계다.
- E1~E7의 판정 주체는 WI 또는 PlayableUnit이다. E4는 실행 문맥 결속, E5는 결정적 세계 발현이며 H·AreaSet·Graph는 필요한 WI에만 조건부 공간 증거로 들어간다.
- E8은 한 E7 PlayableUnit의 반복 안정성, E9는 같은 영역의 안정 Core 둘 이상의 조화와 사람 승인, E10은 제한 운영 관문이다.
- 핵심 개발 순서는 Nature→Farm→Hub→Town→City의 독립 Core이며, 모든 Core를 닫은 뒤 Extension을 진행한다.
- Farm→Hub→Town은 자동 기본 수직 슬라이스가 아니다.
- `SimulationWorldShell`만 canonical Play Scene으로 유지한다.
- Solo `LocalProcess`, Hosted `RemoteHost`, Unity `ReviewFixture`를 혼동하지 않는다.
- Unity는 Shared Simulation Core가 결정한 상태를 표현하며 WorldTick·Revision을 직접 변경하지 않는다.
- H 설계 재고, 실행 자원 지도, 공간 조립 호환 출력과 WI E4·E5 판정은 서로 다른 증거다.
- 권위 지도는 navigation snapshot이고 JSON 대장·계약·코드·실행 증거를 대체하지 않는다.
- 변경 영향·Migration·호환·회귀는 특정 E 단계가 아니라 전 구간 교차 책임이다.

새 결정이 위 항목을 바꾸면 기존 문장을 조용히 고치지 않고 중앙 결정 문서에 대체 관계를 기록한 뒤 이 묶음을 함께 갱신한다.
