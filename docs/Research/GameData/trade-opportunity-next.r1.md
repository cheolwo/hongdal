# D367 다음 묶음: 거래기회 안내 사전 검토

판본 `game-data-research.trade-opportunity.preflight.r1`, 2026-08-30. 첫 Farm 묶음의 [재사용 재고](reuse-inventory.r1.md)를 공유한다. 상태는 **코드·기획 사전 검토 완료 / 현행 가격 표본·권리 검증 미완료**다. 추가 공식 자료 다운로드·운영 DB 조회 없이 제한된 후속 묶음으로 반환한다.

## 승인 방향과 기준선

공유 경로의 `docs/Architecture/PlayableLoops/PlanningSessions/생존경제/survival-economy.inquiry.r1.md` 내용 r2, Q399 및 D367을 직접 읽었다. SHA-256 `82EE8665E8B189F1FA6BDD9D545C4A576752D47C43B5A97B498417883A5D9C93`가 전달값과 일치한다. 이는 ConfirmedDirection이며 세부 가격/국가 대응/수익 계산·새 WI 승인과 다르다.

기존 운영 수집 → 품목 대응·비교 가능성 검토 → 판본화한 게임 해석 → Simulation 거래 후보 → 간단한 기회 안내 → 선택 시 상세 근거 확인 경로를 지지한다. 첫 Farm의 필수 교역·Farm→Hub→City 경로로 만들지 않는다. 실제 주문·결제·수익 보장으로 연결하지 않는다.

## 현재 확인된 재사용과 공백

`Kamis중심UsdaAms가격비교QueryService`는 저장 관측을 AsNoTracking으로 읽으며 연도·Daily/Monthly·품목 후보·시장 단계를 구분한다. KAMIS 포장/비교단위/정규화 근거와 AMS 품종/등급/포장/크기/유기농/원산지/지역/통화/원 단위가 존재한다. 응답에는 `AllowsDirectPriceDifference`, `AllowsDirectComparison`, `ComparisonBoundaries`가 있어 게임 기회 카드에서도 이 제한을 버리면 안 된다.

`KamisAmsCoreAnalysisAndMigration.md`는 2026-07-30 집계·이관 기록이다. 여기에 적힌 행 수·매핑 수·최신일·용량은 현재 DB 수치로 인용하지 않는다. **현행 수집량·최신 기준일·유효 매핑 수·누락률은 모두 미확인**이다. 코드상 감자 KAMIS Confirmed/AMS Candidate는 재확인했지만 운영 DB의 현재 검토 이력은 읽지 않았다.

| 비교 차원 | 사용할 기존 필드/근거 | 제한 |
| --- | --- | --- |
| 시장 단계 | ProductClassCode / MarketStageCode | KAMIS 소매 조사와 AMS 소매 광고는 동일 거래가격 아님. 중도매와 터미널도 조건 검토 필요. ShippingPoint 직접 대응 없음 |
| 품목·품종·등급 | ItemCode / Commodity / Variety / Grade / ItemSize / Organic | 이름 또는 국가만으로 동급 판정 금지 |
| 포장·중량 | Unit / SourcePackageLabel / ComparisonUnit / PriceNormalizationBasis / Package / OriginalUnit | each·포대·상자를 임의 kg로 환산 금지. 과거 전부 1kg 기록을 현행 포장 사실로 간주하지 않음 |
| 통화·시간 | KRW / CurrencyCode / SurveyDate / ReportBeginDate | 환율 기준일도 별도 필요. 최신값끼리라도 관측 주가 다를 수 있음 |
| 지역·원산지 | MarketLocationName/State / Origin | 한 시장 가격을 국가 대표가나 게임 국가 가격으로 확정 금지 |
| 품질·결측 | IsPriceMissing / null 가격 / MappingStatusCode | 빈값은 가짜 현실가격으로 보충하지 않음 |

## 상세 원문 노출 범위

기술적으로도 현재 비교 DTO만으로 완전한 원문 상세를 만들 수는 없다. AMS 가격 Point에는 RecordKey/SourceKey가 있지만 SlugId·보고서 원문 URL·권리 정보가 없다. 원 저장 모델에는 SlugId/SlugName/ReportTitle과 수집 Run의 SourceUrl이 있어 개발이 연결을 검토할 수 있다. KAMIS 저장 모델의 SourceUrl도 비교 DTO의 세부 근거 표시에 연결됐다고 입증되지 않았다. 출처 URL에는 인증 쿼리가 섞이지 않는지 별도 검사해야 한다.

| 표현 후보 | 현재 판정 |
| --- | --- |
| 기관·공식 공개 페이지로 이동하는 링크 | 후보. 자료별 접근·권리 조건과 키 없는 공개 URL 직접 확인 후 사용 |
| 원 관측값·날짜·단위·지역과 게임 해석값을 분리한 상세 | 계약 설계 후보. 현행 표본·라이선스·출처 계보를 검증한 범위에서만 |
| 원문 전체·표·사진·보고서 파일 재게시/임베드 | 미승인. KAMIS/AMS 각각의 이용·재배포/제3자 권리 확인 필요 |

본 묶음에서 KAMIS/AMS 이용조건 공식 원문을 직접 확인하지 않았으며, 기획 문서의 공식 링크 열람 기록을 이번 전문 담당의 검증으로 대신하지 않는다. 공공데이터 또는 미국 정부 사이트라는 이유만으로 재배포 허용을 추정하지 않는다. 새 가격 표본 파일은 0개, 가격 API 호출 0회, DB 조회 0회다.

## 개발 검토와 다음 기획 판단

`SimulationRealityContextService`의 승인 자료 동결·출처 hash·AreaSet 검증을 재사용 후보로 삼는다. 현재 파일 기반 승인 카탈로그가 이 비교 조회 응답을 바로 소비하거나 거래기회 UI까지 연결한다는 증거는 없다. Q265의 다음 World Day 갱신 방향과 기존 세션 동결 구현 사이의 Save/Replay 갱신 책임을 개발이 검토해야 한다. 이 조사에서는 동결 시점이나 Save를 수정하지 않았다.

기획 미정은 국가↔게임 지역 대응, 첫 안내 품목/시장, 판본화 해석 규칙, 비용·위험·수익 상한, 공개 상세 수준과 정보 해금, 오래된/결측 자료 처리, 갱신 빈도다. Q258~260 및 Q265·Q399에 연결하며 실행 WI는 미결속이다. 위험 안내는 아래 r3 추가 확인에 따라 방향 확정이며, 정확한 비용 차감·예상 이익 계산은 승인 규칙이 아니다.

후속 조사는 새 대량 수집 대신 다음 작은 검토 묶음으로 제한한다: (1) 기존 운영 담당이 제공하는 비밀 없는 현행 Coverage/관측 표본의 조회시각·범위·hash, (2) 해당 KAMIS/AMS 자료의 공식 이용·상세노출 조건, (3) 감자 한 품목의 시장/단위 불일치 표. 현재 권리/비교 조건 미확인은 이 후보의 제품 적용만 보류하며 다른 승인 개발은 막지 않는다. 이 문서만으로 자동 수집이나 새 작업을 시작하지 않는다.

## 마감 시 동시 변경 확인

20:02 KST 검증 중 공유 생존경제 문답과 DECISIONS가 변경됐다. 다시 읽은 문답은 내용 r3·Q400 추가이며 SHA-256 `3CCB7B180070B60D00F126891E2E4F27F1DB24C2C041ED89415737269AB8944C`, DECISIONS는 `F1E3306CC3EB291CC8ADCDA461925AE95525861F70E10A6CB325522ADCA6FDF9`다. 최초 전달된 r2/hash가 당시 일치했다는 기록은 보존한다.

D368은 위험 안내·게임 상단 보험·직접 경로 위협 제거의 방향을 추가했고 비용/보상/실행 세부는 미정이다. 본 사전 검토에서는 위험 안내가 미승인이라는 표현만 바로잡고 보험·법률·게임 손실 규칙 조사를 자동 확장하지 않았다. 개발은 통합 시 이 후속 판본을 다시 대조해야 한다. 기준선 JSON은 최초 캡처이며 현재 파일과 동일하다고 보고하지 않는다.
