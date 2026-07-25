# USDA 미국 국내 농가가격 비교

## 결과

- 통합 커뮤니티 앱의 공공데이터 정보 영역에 `/information/usda-us-price-comparison` 직접 Route를 추가했다.
- USDA NASS `PRICE RECEIVED` 관측값을 미국 사용자가 익숙한 `oz`, `lb`, 대표 `1개` 기준으로 환산한다.
- 연도·기준기간, 주·전국 집계, 품종·분류, USDA 원문 단위와 선택 단위 환산값을 가로 스크롤 표에서 함께 확인한다.
- 환산 가능한 관측값끼리 최저값 대비 금액 차이와 비율을 표시한다.
- 대표 개수 가격은 사용자가 지정한 개당 oz 중량을 사용하며 추정값임을 명시한다.

## 단위 환산

- `$ / CWT`와 `$ / 100 LB`는 100파운드 기준으로 해석한다.
- `$ / LB`, `$ / POUND`는 파운드 가격으로 사용한다.
- `$ / TON`은 미국 short ton의 2,000파운드 기준으로 환산한다.
- bushel은 품목마다 표준 중량이 다르므로 임의 환산하지 않고 USDA 원문 단위를 유지한다.
- dozen이나 알 수 없는 원문 단위도 중량 가격으로 추정하지 않는다.

## 데이터 경계

- 현재 자동 연동은 USDA NASS Quick Stats의 생산자 `농가 수취가격` 통계다.
- 이 값은 도매가격이나 소비자가 매장에서 지불하는 일상 소매가격이 아니다.
- USDA AMS는 도매·출하지·소매 광고가격을 별도로 제공하므로 화면에서 공식 자료로 연결하되 NASS 값과 같은 단계로 합치지 않는다.
- 서로 다른 연도, 지역, 품종, 등급, 조사 빈도의 차이를 확정 유통마진으로 해석하지 않는다.
- 화면에는 원문 단위, 지역·집계 수준, 기준기간, 통화와 제한을 함께 표시한다.

공식 근거:

- [USDA NASS Quick Stats](https://quickstats.nass.usda.gov/)
- [USDA AMS Market News](https://www.ams.usda.gov/market-news)
- [USDA AMS Grocery Store Feature Reports](https://www.ams.usda.gov/market-news/grocerystore)
- [USDA AMS Specialty Crops Retail Report API 안내](https://mymarketnews.ams.usda.gov/viewReport/3324)

## Figma·MAUI 호환

- MAUI 공통 화면은 기존 KAMIS 비교 화면과 같은 정보 순서인 품목 조회 → 환산 단위 → 관측값 표 → 데이터 경계를 사용한다.
- 미국 화면은 한국식 `g·kg` 대신 `oz·lb`, 원화 대신 달러, 유통단계 대신 NASS의 농가 수취가격 관측 범위를 사용한다.
- Figma 연결 도구가 일시적으로 응답하지 않아 이번 검증에서는 MAUI·공통 UI build와 컴포넌트 test로 간접 확인했다. 연결 복구 후 같은 변경 맥락에서 실제 Figma node와 PNG를 추가한다.

## 확인

- USDA `$ / CWT`의 lb·oz·대표 개수 환산 테스트
- bushel의 품목별 중량 차이로 인한 원문 단위 유지 테스트
- 신규 공개 읽기 전용 Route의 page capability 테스트
- `Ssalddel.Ui.Common`과 `SsalddelApp` Windows target build
