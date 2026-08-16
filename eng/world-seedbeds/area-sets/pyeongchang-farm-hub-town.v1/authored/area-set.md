# 평창 Farm–Hub–Town 지역 세계

@areaset area-set:sim:pyeongchang:farm-hub-town.v1

## 포함 영역

@area area:sim:pyeongchang:daegwallyeong-farm
@area area:sim:pyeongchang:jinbu-hub
@area area:sim:pyeongchang:pyeongchang-town

## 경관 구조

@landscape-graph landscape-graph:sim:pyeongchang:daegwallyeong-farm.v1
@landscape-graph landscape-graph:sim:pyeongchang:farm-hub-corridor.v1
@landscape-graph landscape-graph:sim:pyeongchang:jinbu-hub.v1
@landscape-graph landscape-graph:sim:pyeongchang:hub-town-corridor.v1
@landscape-graph landscape-graph:sim:pyeongchang:pyeongchang-town.v1

대관령의 생산 공간에서 진부의 물류 거점을 거쳐 평창읍 생활권으로 이어지는 첫 지역 유통 Simulation 공간이다. Area는 의미 영역이고 LandscapeGraph는 조립·갱신 가능한 공간 구조이므로 둘을 1:1로 고정하지 않는다.

공식 도로 공간자료가 연결되기 전 두 회랑은 `ScenarioRoute`다. 문서나 경관 표현만으로 실제 도로·시설·운송 완료를 주장하지 않는다.
