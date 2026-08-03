# 지도형 홈 운송 시뮬레이션 레이어

## 범위와 안전 경계

`/community/home`의 물류·항공·해상 레이어는 **SIMULATED · 교육용 · 비실시간** 흐름만 설명한다. 표시점은 고정 fixture 경로 위의 합성 진행률이며 실제 운행 추적, 배차 확정, 개인 위치, 항공편·선박 식별자 또는 타 조직 운영정보가 아니다. 레이어 선택은 조회 상태만 바꾸며 주문·계약·배차·알림·외부 호출을 만들지 않는다.

초기 경로 계약인 `운송시뮬레이션RouteDto`는 다음 항목을 필수로 둔다.

- stable ID, 운송 mode, 경로점과 교육 상태
- 출처 code·이름·종류
- fixture 기준 시각과 `고정 교육 예시 · 자동 갱신 없음` 신선도
- `IsSimulation`, 화면에 그대로 표시할 simulation mark와 실제 위치가 아님을 설명하는 position meaning

초기 catalog는 `sourceKindCode=simulated-fixture`와 `SIMULATED` mark가 모두 있는 세 경로만 renderer에 전달한다. 외부 source catalog 항목은 이 fixture와 연결하지 않는다.

## 기술 스택과 확장 경계

| 층 | 첫 구현 | 다음 확장 기준 |
| --- | --- | --- |
| 기본 지도 | 기존 Google Maps JavaScript API 한 인스턴스 | runtime key·origin 제한과 기존 SVG fallback 유지 |
| 소수 객체 | `google.maps.OverlayView`에 pointer event 없는 Canvas 한 장 | `onAdd`/`draw`/`onRemove` 생명주기와 지도 projection 사용. overlay pane 원점을 지도 viewport 좌상단으로 보정해 drag·zoom 중에도 경로가 잘리지 않게 한다. [Google Custom Overlays](https://developers.google.com/maps/documentation/javascript/customoverlays) |
| Google 미연결 | 같은 fixture를 기존 개략 지도 위 별도 SVG path와 `animateMotion`으로 표시 | Google 설정 실패를 실제 데이터 fallback으로 숨기지 않음 |
| 다량·향후 3D | `registerTransportSimulationRenderer(kind, factory)` registry | 객체·경로가 Canvas 예산을 넘는 별도 성능 검증 뒤 `WebGLOverlayView` 또는 deck.gl adapter를 등록한다. [Google WebGLOverlayView](https://developers.google.com/maps/documentation/javascript/webgl/webgl-overlay-view), [deck.gl GoogleMapsOverlay](https://deck.gl/docs/api-reference/google-maps/google-maps-overlay) |

낮은 zoom은 최대 3개, 중간 zoom은 6개, 높은 zoom은 설정 상한(현재 12개)까지만 viewport와 만나는 경로를 그린다. 사용자는 움직임을 끌 수 있고 `prefers-reduced-motion`이면 첫 렌더부터 정지 상태를 사용한다. 브라우저 tab이 숨겨지면 animation frame도 중단한다.

## 공식 공개 데이터 adapter 후보

검토일은 2026-08-02다. 이번 통합에서도 API 호출, 활용신청, 계정 생성, key 발급, scraping, 유료 호출을 하지 않았다. 아래 항목은 모두 `catalog-only`이며 adapter는 비활성이다.

| mode | 공식 catalog | 확인한 접근·재사용 조건 | 초기 판단 |
| --- | --- | --- | --- |
| 항공 | [국토교통부 TAGO 국내항공운항정보](https://www.data.go.kr/data/15098526/openapi.do) | 활용신청형 REST, JSON/XML, 무료, 개발계정 10,000건, 게시된 이용허락범위 제한 없음 | 출도착 일정·현황이지 항공기 좌표 추적 source가 아니다. 운영 명세와 재배포 조건을 다시 검토하기 전까지 catalog-only |
| 해상 | [해양수산부 선박운항정보](https://www.data.go.kr/data/15006353/openapi.do) | 활용신청형 XML, 무료, 개발계정 10,000건, 게시된 이용허락범위 제한 없음 | 호출부호·선명·입출항시각 등 운영 식별정보가 포함될 수 있으므로 초기 지도에는 수집·표시하지 않음 |
| 항공 | [FAA SWIM Flight Data Publication Service](https://www.faa.gov/air_traffic/technology/swim/sfdps) | FAA 문서상 외부 소비자는 NAS Enterprise Security Gateway를 통한 승인 접근 대상 | 공개 무인 API로 가정하지 않으며 access agreement·재배포 조건 확인 전 수집하지 않음 |

향후 공식 source adapter를 열더라도 원본 식별자와 순간 위치를 공개 홈으로 전달하지 않는다. 교육 시나리오는 계속 별도 fixture로 유지하고, 공개 통계가 필요하면 시간·공간 집계와 최소 공개 기준을 먼저 계약으로 고정한다.

## 현재 파일 경계

- 계약·fixture·향후 source catalog: `Ssalddel.Contracts/Common/Community/운송시뮬레이션MapDtos.cs`
- Google adapter registry·Canvas renderer: `Ssalddel.WebApp/wwwroot/js/transport-simulation-map-layer.js`
- 기존 Google 지도 연결 seam: `Ssalddel.WebApp/wwwroot/js/community-world-google-map.js`
- toggle·범례·경고·SVG fallback: `Ssalddel.WebApp/Pages/CommunityRoleHomePage.razor`

renderer 실패 시 Google 지도 자체와 고정 예시 정보 panel은 유지한다. Google runtime이 없으면 기존 지도형 fallback 위에서 같은 simulated fixture를 표시한다. 이 화면은 WebApp 전용 지도 adapter 통합이므로 이번 작업에서 공용 MAUI route나 Figma 구조는 변경하지 않았다.
