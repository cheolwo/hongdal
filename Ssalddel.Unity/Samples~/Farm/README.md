# Farm Primitive Vertical Slice

생산자 소유권으로 필터된 operational 농장 projection을 `FarmTileView`, `CropView`,
`SensorView`에 stable ID로 적용하는 primitive sample이다.

- 공개 작물 기준 ID/출처와 실제 재배 생육 상태를 분리한다.
- 센서 원시값은 Unity에서 재판정하지 않고 서버의 상태·규칙 revision·근거 card ID를 표현한다.
- 생산자 NPC는 canonical 농장작업의 semantic waypoint를 NavMeshAgent에 적용하며 도착으로 서버 작업을 완료하지 않는다.
- Simulation fixture는 `SourceTypeCode=SimulatedFixture`로 명시하며 운영 API 실패를 대신하지 않는다.
- 위치, 주소, 연락처와 소유자 사용자 ID는 Unity 계약에 포함하지 않는다.
