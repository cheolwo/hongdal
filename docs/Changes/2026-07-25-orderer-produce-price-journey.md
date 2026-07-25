# 주문자 과일·채소 가격 탐색 흐름

## 결과

- 기존 공개 정보 Route `/information/produce-price-comparison`과 `/information/apple-price-comparison`은 그대로 유지했다.
- 주문자 앱에 `/group-purchase/produce-prices` Route와 `02.02A` 화면 책임을 추가했다.
- 주문자 홈, 서랍 메뉴, 기존 데스크톱 NavMenu에서 가격 탐색 화면으로 이동할 수 있다.
- 가격 탐색 화면은 `가격 탐색 → 재료 선택 → 내 원함 남기기 → 함께 주문` 순서를 보여 준다.
- 비교 뒤에는 기존 재료 후보와 비구속 개별주문 의향 Route로 이어지며, 가격 정보만으로 주문·결제·수입을 확정하지 않는다.

## Figma·MAUI 호환

- 기존 Figma `02.02~02.14` 책임 코드는 변경하지 않고 중간 판단 화면을 `02.02A`로 추가해 기존 화면 식별자와 Route 호환성을 보존했다.
- 공개 정보 화면의 공통 비교 컴포넌트를 재사용하되, 주문자 문맥에서는 뒤로 가기를 `주문자 홈`으로 바꿀 수 있도록 매개변수화했다.
- Figma 연결 도구가 현재 세션에 노출되지 않아 실제 Figma node 수정은 보류했다.
- Windows OrdererApp 실행 창은 열렸으나 WebView가 흰 화면에 머물러 실제 렌더 PNG를 얻지 못했다. 이번 기록은 테스트와 소비 앱 빌드에 의한 간접 확인이며, Figma 연결과 WebView 로딩이 복구되면 같은 화면 코드로 실제 node와 PNG를 보완한다.

## 확인

- `OrdererMobileFigma02PresentationTests` 21개 통과
- `OrdererApp` Windows target build 경고·오류 없음
- 기존 공개 가격 비교 Route 보존과 주문자 재료·원함·공동후보 연결을 소스 조립 테스트로 확인
