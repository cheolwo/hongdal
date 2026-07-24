# 농수산물·공식 재료 근거 수집 배치

살뜰 서버는 Quartz 작업으로 KAMIS 국내 농수축산물 가격, USDA NASS 미국 생산자 가격과 식약처 재료별 국내외 기업 공식 근거를 주기적으로 수집한다. 명시적으로 허용하면 같은 작업 안에서 `수집 성공 → 검증된 초안 생성 → 커뮤니티 시스템 글 저장`까지 순서대로 실행한다. 수집과 게시 모두 안정된 원천 키·기준기간 키를 사용하므로 실패 뒤 전체 작업을 재실행해도 관측값이나 게시글이 중복되지 않는다. 각 수집 결과와 실패 내용은 해당 수집 실행 이력에 남는다.

## 기본 일정

| 작업 | 기본 실행 시각 | 수집 범위 | 게시 후속 작업 |
| --- | --- | --- | --- |
| KAMIS 일별 가격 | 매일 06:30 (한국 시간) | 전날 가격 | 최신 조사일의 일부 품목을 `KAMIS 가격 데이터` 글로 게시 |
| KAMIS 월평균 가격 | 매월 2일 07:00 (한국 시간) | 직전 완료 월까지 12개월 | 게시 없음, 통계 보관 전용 |
| USDA NASS 월별 가격 | 매월 10일 07:30 (한국 시간) | 현재 연도와 직전 1년의 전국 생산자 수취가격 | 최신 기준월의 통합 계열 일부를 `USDA 가격 데이터` 글로 게시 |
| 식약처 재료별 기업 근거 | 매주 일요일 03:00 (한국 시간) | 갱신 시점이 지난 공식 재료의 국내 제조·수입·해외제조업소 근거 | 중국 제조업소 권역과 미국 제조업소 주별 현재값을 각각 월 1회 `MFDS 수입식품 데이터` 글로 게시 |

같은 종류의 작업은 동시에 실행되지 않는다. 게시 단계가 실패해도 작업 전체가 실패 처리되어 기본 1회 즉시 재시도한다. 이때 공공데이터 수집은 upsert되고 게시글은 `sourceKey + periodKey`로 기존 글을 확인하므로 중복을 만들지 않는다. 같은 키의 기존 글이 과거 `정보·시세`에 있으면 새 글을 만들지 않고 Category만 원천별 전용 게시판으로 옮긴다. 관련 게시판에는 DB 글을 복제하지 않고 대표 안내 link만 표시한다. 서버가 꺼져 있던 동안 놓친 Cron 실행을 시작 직후 몰아서 수행하지 않으며, KAMIS 월평균 작업과 USDA 작업이 최근 기간을 다시 조회하므로 늦게 확정된 원천 자료는 다음 정기 실행에서 보완된다.

`CommunityEditorialBatch`를 별도로 켜면 KAMIS 06:50, USDA 08:00 독립 작업이 같은 기준기간 글을 다시 확인한다. 수집 직후 게시가 이미 성공했다면 새 글을 만들지 않고 종료하며, 일시적인 게시 실패가 있었을 때는 조정 작업 역할을 한다.

## 설정

배치는 기본적으로 꺼져 있다. API 인증 정보를 비밀 설정에 넣은 환경에서 다음 설정으로 켠다.

```json
{
  "AgriculturalFisheriesBatch": {
    "Enabled": true,
    "TimeZoneId": "Asia/Seoul",
    "ImmediateRetryCount": 1,
    "PublishCommunityPriceBriefs": true,
    "KamisDailyEnabled": true,
    "KamisDailyCronExpression": "0 30 6 * * ?",
    "KamisDailyDaysBehind": 1,
    "KamisMonthlyEnabled": true,
    "KamisMonthlyCronExpression": "0 0 7 2 * ?",
    "KamisMonthlyLookbackMonths": 12,
    "UsdaMonthlyEnabled": true,
    "UsdaMonthlyCronExpression": "0 30 7 10 * ?",
    "UsdaLookbackYears": 1,
    "IngredientCompanyResearchEnabled": true,
    "IngredientCompanyResearchCronExpression": "0 0 3 ? * SUN",
    "IngredientCompanyResearchRefreshAfterDays": 30,
    "PublishChinaImportedFoodRegionBriefs": true,
    "PublishUnitedStatesImportedFoodStateBriefs": true
  }
}
```

환경 변수로 켤 때는 `AgriculturalFisheriesBatch__Enabled=true`, `AgriculturalFisheriesBatch__IngredientCompanyResearchEnabled=true`를 사용하고 국가별 게시를 `AgriculturalFisheriesBatch__PublishChinaImportedFoodRegionBriefs=true`, `AgriculturalFisheriesBatch__PublishUnitedStatesImportedFoodStateBriefs=true`로 각각 허용한다. Docker Compose에서는 `SSALDDEL_INGREDIENT_COMPANY_RESEARCH_ENABLED`, `SSALDDEL_CHINA_IMPORTED_FOOD_REGION_PUBLICATION_ENABLED`, `SSALDDEL_US_IMPORTED_FOOD_STATE_PUBLICATION_ENABLED`로 주입한다. 가격 글은 기존 `AgriculturalFisheriesBatch__PublishCommunityPriceBriefs=true`로 별도 허용한다. 기본값은 수집과 게시 모두 비활성이며, KAMIS 인증키와 요청자 ID, USDA API 키 및 식약처·공공데이터포털 키는 저장소에 넣지 않고 사용자 비밀 또는 배포 환경의 비밀 저장소에서 주입한다.

Quartz Cron 식과 표준 시간대 ID가 올바르지 않으면 서버 시작 단계에서 설정 오류가 드러난다. 여러 서버 인스턴스가 동시에 실행되는 운영 환경에서는 Quartz 영속 저장소와 클러스터 잠금 구성을 추가해야 인스턴스 간 중복 호출까지 방지할 수 있다.
