# KAMIS·USDA AMS 핵심 분석과 Azure 데이터 이관

## 변경 범위

- KAMIS 96개 품목, USDA AMS 2026년 관측, HS 후보 매핑과 포장·FCL 근거를
  시장 단계·원 단위·품종·등급·지역 차원으로 분석했다.
- 원본을 직접 비교가격으로 합치지 않고 Coverage와 비교 가능한 Series로 나누는
  주간 투영 기준을 문서화했다.
- 로컬 MySQL의 KAMIS·AMS 가격, 공개 사업체, FCL Snapshot과 HS 검토 매핑을
  Azure 상시 체험 서버 DB로 이관했다.
- AMS Archive에 시장 단계 필터, 접두 품목 검색과 최신일 복합 인덱스를 추가했다.
- 대용량 농수산물 migration에만 15분 명령 제한을 적용하고 일반 API 제한은
  그대로 유지했다.

## 검증

- KAMIS 관측 35,191건, AMS 관측 1,131,591건과 관련 10개 표의 정확한 행 수를
  로컬·Azure에서 대조했다.
- 기준일 범위, RecordKey 중복 0건, 가격·날짜 역전 0건과 알려진 가격 공란·0원
  예외가 동일함을 확인했다.
- Azure 앱 `healthy`, 최신 AgriculturalFisheries migration, Archive 인덱스와
  역방향 인덱스 실행계획을 확인했다.
- AMS 연도 최신 조회 0.36초, `Apples + TerminalWholesale` 조회 0.07초,
  KAMIS 중심 20품목 비교 0.73초를 공개 URL에서 확인했다.
- `eng/validate-changes.ps1 -Level Task`로 v3.5 build와 관련 테스트를 통과했다.

## 화면

화면 없음 — 서버 계약·조회·DB migration·분석 문서와 Azure 데이터만 변경했다.
