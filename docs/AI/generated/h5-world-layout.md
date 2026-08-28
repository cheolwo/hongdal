# H5 세계 배치

H4 AreaSet과 H3 회랑을 H5의 `ScenarioLocalMeters`에 배치한 실제 E5 공간 조립 결과다.

- H5: `world-layout:sim:pyeongchang:nature-farm-hub-town.v1`
- H4 AreaSet 인스턴스: `4`
- 고정 영역 앵커: `5` (조립 `4` / 예약 `1`)
- 물리 회랑: `3`
- 예약 회랑: `1`
- 현실 결속: `Optional / NotApplied`

E6가 없어도 이 H5는 ScenarioRelative 권위 세계다. E6는 H5 이하 상대 X/Z 배치를 바꾸지 않는다.
DEM·도로는 전역 필수 자료가 아니며 선택한 현실 결속 프로필의 준비도에만 참여한다.
예약 City 앵커는 위치와 특징만 고정하며 Actual E5 Graph·통행·활성화를 의미하지 않는다.
