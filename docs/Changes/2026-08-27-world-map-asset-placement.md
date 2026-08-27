# LH 지도와 Nature 세계자산배치 분리

## 변경

- LH는 셀·지면·Mesh·Collider와 준비 상태만 소유한다.
- 공통 `SimulationNatureWorldCellAssemblyEngine`이 Nature 권위 상태에서 실외·실내 조립 자료를 만든다.
- Unity의 `World공간표현조립Coordinator`가 Sky 적용 뒤 실외·실내 표현 객체를 배치 고유 식별자로 조정한다.
- Nature 좌표 원점을 Controller GameObject가 아니라 기존 `visualRoot`에 맞췄다.

## 검증 수준

- Simulation 집중 시험: `21/21` 통과
- Unity Bootstrap·EditMode 생성 프로젝트: 오류 `0`
- 실제 Play Mode: `SimulationWorldShell → Nature 1인칭 → 도끼 획득`과 HUD 전이 확인
- 화면 판정: 미통과. 도끼 획득 뒤 다음 나무 형상을 밤 Game View에서 식별하기 어렵다.
- Console 판정: 미통과. `Access version should be odd when acquiring lock` 오류가 반복된다.
- Unity Test Runner: Pipeline 단절로 결과 미회수

최종 대표 PNG는 통과 증거가 아니므로 장기 보존하지 않았다. 현재 증거는 E5이며 표현 E6·E7은 차단 상태다.
