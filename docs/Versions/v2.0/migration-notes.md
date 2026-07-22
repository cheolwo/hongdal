# Ssalddel 2.0 Migration Notes

## 2026-07-22 · 운송 후순위 재분류

- 기존 `1.0` 국내 화물·용달 범위를 `2.0`으로 이동했습니다.
- 화주 운송 의뢰, Driver API, 배차·운행·POD·정산과 관련 관리자 API 메타데이터를 `2.0`으로 이동했습니다.
- 기사 Command 기능 그룹도 `2.0`으로 이동했습니다.
- 기존 `CargoYongdalV1` 설정은 호환 별칭으로 유지하고 신규 설정은 `DomesticTransportWorkflow`를 사용합니다.
- HTTP `/api/v1/...` route와 저장된 운송 원장 ID는 변경하지 않습니다.
- 이번 재분류 자체는 DB 스키마 마이그레이션을 요구하지 않습니다.
