# 농장 관수·급수 관리 블록

@spatial-knowledge h2-candidate:farm-irrigation-service
@hierarchy H2
@state ExploratoryInventory
@required-h1 h1-stock:farm-production
@required-h1 h1-stock:farm-maintenance-yard
@required-h1 h1-stock:farm-weather-protection
@optional-h1 h1-stock:farm-tool-storage
@connector FieldInput
@connector WaterServiceRoute
@connector CropCareOutput

## 존재 이유

생산구획과 시설 정비 공간, 기상 보호 적치를 연결해 관수 장비 점검과 재배 관리를 수용하는 Farm 단독 블록이다.

## 설계 상태

- 재고 상태: `ExploratoryInventory`
- 공간 계층: `H2`
- 실제 지역 권위: 없음

## 미해결

- 기준 크기·배치 방향과 연결구 조합은 설계 검토에서 확정한다.

이 문서는 상향식 공간 설계 지식이며 실제 좌표·AreaSet·LandscapeGraph·Unity 자산 권위를 만들지 않는다.
