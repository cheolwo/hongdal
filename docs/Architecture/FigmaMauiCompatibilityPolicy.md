# Figma-MAUI 화면 호환성 정책

## 목적

Figma `01 Community`와 `SsalddelApp`의 .NET MAUI Blazor 화면을 서로 독립된 시안과 구현으로 방치하지 않는다. Figma는 화면 의도와 토큰의 시각 기준이고, MAUI 코드는 route·상태·접근성·실행 경계의 동작 기준이다.

## 기준과 대응

| Figma | MAUI |
| --- | --- |
| `01 Community` 화면 이름과 책임 코드 | `CommunityPageRoutes`와 Razor `@page` |
| Community 색상·간격·반경 변수 | `CommunityMobileLayout.razor.css`와 화면별 CSS 변수 |
| 반복 카드 컴포넌트와 text property | 공용 Razor component와 contract/catalog property |
| AppBar·drawer·bottom navigation | `CommunityMobileLayout` |
| 선택·빈 상태·오류·비활성 표현 | ViewModel 또는 component state |

현재 지역 문화·특산물 화면은 Figma `01A.07 · 지역 문화·특산물`, MAUI `/community/regions`, `RegionalCultureSpecialtyBrowse`로 대응한다.

## 변경 규칙

1. 화면을 추가하거나 구조를 바꿀 때 Figma node, MAUI route, 공용 component, 데이터 contract의 대응을 먼저 기록한다.
2. Figma 변수의 Web code syntax는 실제 CSS custom property 이름을 사용한다. Android와 iOS 이름도 함께 유지해 MAUI 플랫폼 확장을 막지 않는다.
3. Figma의 정적 화면을 그대로 상태 모델로 간주하지 않는다. loading, empty, error, retry, disabled, 동의·철회와 실행 경계는 MAUI 코드와 test에서 확인한다.
4. MAUI 변경으로 화면 구조나 토큰이 달라지면 같은 작업에서 Figma를 갱신한다. Figma가 먼저 바뀌면 `get_design_context`를 기준으로 기존 공용 component를 재사용해 MAUI에 반영한다.
5. 양쪽이 다를 수밖에 없으면 임의로 한쪽을 덮지 않고 변경 기록에 차이와 이유를 남긴다.

## 검증과 커밋

- Figma는 대상 frame의 metadata와 screenshot으로 글꼴, 잘림, 겹침, component property를 확인한다.
- MAUI는 Windows 대상 build와 실제 화면 render를 확인하고, 공용 UI 변경이면 최소 한 소비 client test를 함께 실행한다.
- 대표 PNG와 node ID, route, 검증 결과를 `docs/Changes/`에 남긴다.
- 커밋은 `Figma 기준·정책`과 `MAUI 구현·test`처럼 되돌릴 수 있는 맥락으로 나눈다. Figma 외부 변경은 해당 변경 기록에 node ID와 파일 링크를 남긴다.
