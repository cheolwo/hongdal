# Unity WORLD-SHELL-0·SETTLEMENT-SCENE-0 구현

## 변경 요약

| 항목 | 내용 |
| --- | --- |
| 변경일 | 2026-08-10 |
| Unity project | `C:\Users\user\ssalddel` |
| Scene | `Assets/Ssalddel/Scenes/SimulationWorldShell.unity` |
| 화면 변경 | 직접 확인 |

기존 공공데이터 `WorldBootstrapScene`을 변경하지 않고 별도 `SimulationWorldShell` Scene을 추가했다. 하나의 읽기 전용 Simulation fixture snapshot을 `WorldMapRoot`와 `SettlementInteriorRoot`가 공유하며 root 전환 전후 `Year 1 · 04-12`, Tick 12, Revision 12와 session stable ID가 유지된다.

첫 정착지는 Farm·Town·Market·Storage·Logistics·Residential District와 기능 없는 Garrison·Gate placeholder를 포함한다. District는 stable ID와 semantic VisualKey를 가진 Presentation socket이며 별도 서버 Entity·manager·Simulation을 만들지 않는다.

## 시각 증거

- World Map: `C:\Users\user\ssalddel\Assets\Documentation\Changes\2026-08-10-world-shell-settlement\world-map.png`
- Settlement Interior: `C:\Users\user\ssalddel\Assets\Documentation\Changes\2026-08-10-world-shell-settlement\settlement-interior.png`
- Unity 변경 기록: `C:\Users\user\ssalddel\Assets\Documentation\Changes\2026-08-10-world-shell-settlement\README.md`

두 PNG는 Unity Pipeline이 `DioramaTopDownCameraRig`의 1600×900 Play Mode Game View를 직접 캡처한 결과다. HUD는 같은 Simulation fixture identity·Tick·Revision과 Treasury·Labor·Market Food·Reserve Food·FoodSecurityDays·Active Tasks를 표시한다.

## 검증

- Unity 재컴파일 오류 0건
- `SimulationWorldShellTests` 5/5
- `DioramaCameraTests` 4/4
- `Ssalddel.Unity.Tests.EditMode` 전체 44/44
- 최종 Play Mode World Map→Settlement Interior 전환 뒤 Console 오류 0건

## 남은 경계

- 현재 snapshot source는 `SimulationFixture`이며 Simulation 서버 HTTP adapter는 아직 연결하지 않았다.
- Pause·Speed는 `미연결` 비활성이다.
- 판로 Preview·Confirm·Task·Tick·Effect와 실제 정착지 경제 변경은 아직 없다.
- 다음 Gate는 `SETTLEMENT-ECONOMY-1`이다.

