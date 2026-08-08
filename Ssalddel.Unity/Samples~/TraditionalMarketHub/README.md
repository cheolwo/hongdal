# Traditional Market Hub Primitive Vertical Slice

전통시장 건물과 공개 물류거점을 하나의 Unity Zone으로 표현하는 importable sample이다.

## 범위

- `Simulated전통시장물류거점조회UseCase`의 명시적 simulation fixture
- 공개 가능한 `Pilot`, `Active` 상태만 표현
- 검증된 위치 정밀도, 출처, 기준시각과 revision 표시
- 시장 건물, 물류거점, 입고·픽업 Dock과 상세 panel
- SceneController의 중복 초기화 방지
- 현재 dirty Scene 보호와 저장 후 wiring 재검증

관리자용 거점 상태 변경, 공공데이터 동기화, 정확한 사유지 위치와 운영 메모는 이 sample의 범위가 아니다.

## Editor 생성

Unity package sample을 import한 뒤 메뉴에서 다음을 실행한다.

```text
Ssalddel/Samples/Create Traditional Market Hub Primitive Scene
```

실제 API 연결은 `전통시장물류거점LifetimeScope`의 `I전통시장물류거점조회UseCase` 등록을 Repository·Mapper 기반 구현으로 교체한다. View와 Controller는 server DTO를 참조하지 않는다.
