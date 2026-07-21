# 도심 생활물류센터 창고 Profile 계층

## 변경 요약

- 공용 물류대행지 분류에 `UrbanLogisticsCenter`를 추가했다.
- 별도 창고 Entity를 만들지 않고 `일반 입출고 → 도심 생활물류센터 → 마트 도심/공동주택 물류` Profile 상속으로 공통 기능을 조립한다.
- 파생 Profile이 기반 Profile의 작업 페이지를 재사용하도록 resolver와 페이지 접근 판정을 정리했다.
- 커뮤니티 창고 대행 후보와 입고 요청 화면에서 도심 생활물류센터를 같은 계약으로 선택할 수 있게 했다.

## 화면 영향

간접 확인 — 창고 Profile 선택지와 지원 페이지 조립이 늘어나며 새 레이아웃은 없다. 시설 이름은 인허가나 영업 자격을 보증하지 않고 실제 운영 가능 상태는 별도 검증한다.

## 검증

- `LogisticsProxySiteTypesTests`
- `PlatformCommunityWarehouseProxyViewModelTests`
- `dotnet build WarehouseManagerApp/WarehouseManagerApp.csproj -f net10.0-windows10.0.19041.0 --no-restore`
