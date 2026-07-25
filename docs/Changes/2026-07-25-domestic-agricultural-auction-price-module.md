# 국내 농산물 공영도매시장 경락가격 서버 모듈

## 변경

- 농림축산식품부 공영도매시장 경매 정산가격 typed provider와 공통 조회 contract를 추가했다.
- live 조회와 누적 archive 조회 API를 기존 농수산물 정보 API 영역에 배치했다.
- 수집 실행과 비식별 관측값을 `AgriculturalFisheriesDbContext`에 저장하고 migration을 추가했다.
- 같은 원천 거래의 반복 수집은 중복하지 않고 정정된 가격과 조건을 갱신한다.
- 기존 KAMIS 도·소매 조사 가격과 경매 정산가격을 별도 출처·시장 단계로 유지한다.
- 전일 자료를 수집하는 Quartz 작업을 기존 농수산물 배치의 전역 활성화 경계 안에 추가했다.
- 공식 HTTP 원천은 기본 차단하고 HTTPS 중계 또는 명시적인 비보안 HTTP 허용이 있을 때만 호출한다.
- 출하자·생산자·중도매인 정보와 원문 JSON은 저장하거나 API로 노출하지 않는다.

## 화면

화면 없음. 이번 변경은 서버 contract, API, 수집 archive, 배치와 검증만 포함한다.

## 검증

- 신규 provider 파싱·개인정보 비노출·HTTP 차단·입력 검증 test
- 같은 거래 반복 수집 및 가격 정정의 멱등 archive test
- 기존 KAMIS·USDA 농수산물 정보, 배치 runner, API 분류 호환 test

상세 설계는 [국내 농산물 공영도매시장 경락가격 모듈](../Architecture/DomesticAgriculturalAuctionPriceModule.md)을 따른다.
