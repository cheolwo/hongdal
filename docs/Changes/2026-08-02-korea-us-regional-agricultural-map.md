# 한국·미국 공통 농수산물 데이터 지도

## 화면 변화

- 한국 전용 지도 화면을 `KR`과 `US`가 함께 쓰는 Web·MAUI 공통 workspace로 확장했다.
- 국가를 바꾸고 `전체 관계`, `산지 원천`, `출하·선적`, `시장 관측` 레이어를 선택할 수 있다.
- 관계 의미는 색과 도형을 함께 사용한다. 미국 Shipping Point는 원산지로 단정하지 않는다는 경계도 화면에 유지했다.
- canonical route `/information/regional-agricultural-map?country=KR|US`를 추가하고 기존 `/information/korea-agricultural-map`은 호환 route로 유지했다.
- 커뮤니티 세계지도에서 한국과 미국 모두 해당 국가가 선택된 지도 화면으로 이동한다.

현재 지도는 검증된 대표 기준점을 표시하는 SVG 개략도다. 실제 행정경계 polygon, zoom, clustering과 MapLibre 계열 renderer는 후속 범위다.

## 실제 렌더링

WebApp의 미국 route에서 국가 전환과 시장 관측 레이어 전환을 직접 확인했다. 아래 캡처는 데이터 요청 중에도 국가·레이어·개략 지도가 유지되는 상태다.

![미국 국가·관계 레이어 지도 shell](../assets/changes/2026-08-02-regional-agricultural-map/us-country-layer-shell.png)

로컬 API는 기존 MySQL migration `20260727112931_AddTransportRequestUniqueIndex`가 빈 `request_id` 중복으로 중단되어 실제 미국 marker 응답까지 연결하지 못했다. 따라서 marker 데이터가 표시된 런타임 상태는 이번 기록의 직접 확인 범위가 아니다.

## 설계 원본과의 관계

대상 Figma 파일·node가 현재 작업 문맥에 제공되지 않아 Figma에는 반영하지 않았다. 이번 route·상태·레이어 동작은 실행 코드와 테스트를 기준으로 하며, 다음 디자인 동기화 때 [한국·미국 행정구역 기반 농수산물 지도 제안](../Architecture/KoreaUnitedStatesAdministrativeRegionMapProposal.md)의 실제 GIS 후속 범위와 함께 정렬한다.

## 검증

- 관련 ViewModel·공유 UI·route·세계지도 구성 테스트 16개 통과
- 테스트 실행 과정에서 `Ssalddel.Contracts`, `Ssalddel.Ui.Common`, `Ssalddel`, `Ssalddel.WebApp`, `Ssalddel.Tests` 빌드 성공
- WebApp 미국 route 실제 렌더, 미국↔한국 전환과 시장 관측 레이어 전환 확인
- 실제 marker 응답은 위 DB migration 오류로 미검증
