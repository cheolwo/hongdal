# Nature 자연 방향광과 표면 명암 검증

## 변경

- `lighting.pyeongchang.shared-day.v2`와 `directional-lighting.natural.r1`을 추가해 시간대 태양 방향·직사광·그림자·환경광을 하나의 표현 프로필로 관리한다.
- 실제 표면 명암은 URP Lit의 월드 법선과 표면→광원 방향 내적에 맡기고, 모든 Renderer의 기본 색을 시간대마다 일괄 덮어쓰던 경로를 제거했다.
- LH 지면과 Nature 나무·통나무·오두막·작업대에 방향광 대상 View를 붙이고 Mesh 법선·Lit Shader·그림자 투사·수신 정책을 E6 검증 기록으로 남긴다.
- 상세 셀은 지면만으로 통과할 수 없고 Nature 핵심물이 검증되어야 `WorldPresentation.Ready`를 남긴다.
- Synty 원본 Prefab·Material·Shader Graph와 Simulation 권위·저장·Replay hash는 변경하지 않는다.

## 검증 수준

- Unity 자산 DB와 생성 C# 프로젝트 반영 확인
- `Ssalddel.Unity.Presentation.csproj`: 오류 `0`
- `Ssalddel.Unity.Tests.EditMode.csproj`: 오류 `0`
- 방향광 집중 시험 `7`개: 시험 어셈블리 컴파일 확인, 실제 Test Runner 미실행
- 표현 검증 대장: `15`모듈·`4`프로필·`16`PlayableUnit 통과
- 엔진 상호작용 대장: `8`구성요소·`3`프로필 통과
- 실제 Play Mode·Game View: 미검증. 열린 `SimulationWorldShell`의 기존 미저장 변경을 보호하기 위해 Test Runner 저장 확인에서 취소했다.
- 현재 화면 위험: 큰 전경 지형이 카메라를 가리고 `Access version should be odd when acquiring lock` 오류가 남아 있다.

실제 방향광이 적용된 Nature 핵심물의 밝은 면·그늘 면을 판독하지 못했으므로 대표 PNG는 장기 보존하지 않았다. 코드·시험 어셈블리 검증 완료 / Play Mode·Game View 미검증이다.
