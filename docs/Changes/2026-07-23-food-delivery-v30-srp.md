# 3.0 음식 주문·배달 페이지 단일책임 정리

## 결과

3.0 음식 주문·음식점 수락·음식 배달 기사 흐름을 앱과 Route 책임 기준으로 다시 확인했다. 주문자 앱의 음식점·메뉴 탐색과 본인 주문 내역은 이미 공용 Screen 안에서 조회 책임이 분리되어 있어 유지했다. 한 화면에서 실시간 수신, 주문 수락, 전표 출력, 운영 통계와 샘플 주문 생성을 함께 처리하던 음식점 앱을 아래처럼 나눴다.

| 앱·Route | 단일 책임 | 실행 경계 |
| --- | --- | --- |
| Orderer `/food` | 일반 음식점 3.0과 마트 3.5 진입 경계 안내 | ReadOnly |
| Orderer `/food/restaurants` | 공개 음식점과 메뉴 조회 | ReadOnly |
| Orderer `/orders/food` | 로그인 주문자 소유 음식 주문 조회 | ReadOnly |
| Restaurant Desk `/` | 음식점 업무 진입 | ReadOnly |
| Restaurant Desk `/orders` | 설정된 음식점의 실시간 주문 알림 조회 | ReadOnly |
| Restaurant Desk `/orders/{orderNo}` | 정확한 주문 한 건 수락과 성공 응답 기반 전표 출력 | PlatformPersistence |
| Food Delivery Driver `/` | 배차·묶음·경로·정산 업무 진입 | ReadOnly |
| Food Delivery Driver `/food-delivery/open/{focus}` | 로그인 기사의 네이티브 지도 업무 연결 | PlatformPersistence |

## 운영 데이터 경계

- 음식점 앱 시작 시 고정 샘플 주문을 수신함에 넣지 않는다.
- API 장애·빈 응답·상세 조회 실패를 샘플 주문으로 바꾸지 않는다.
- 주문 수락 화면은 `POST api/v1/food-orders/{orderNo}/restaurant-acceptance`의 성공 응답으로만 전표를 만든다.
- 다른 음식점 ID로 온 실시간 알림은 현재 음식점 수신함에 저장하지 않는다.
- 수신함은 조회와 상세 이동만 담당하며 서버 상태 변경 버튼을 포함하지 않는다.
- 배달기사 반복 카드의 Command source를 기사 업무 ViewModel로 명시해 다중 플랫폼 XAML compiled binding을 안정화했다.

## 3.0 배포 경계

기본 `appsettings.json`의 `FoodDeliveryWorkflow=false`를 유지한다. `compose.food-delivery-v30.override.yaml`을 명시적으로 합칠 때만 3.0 API를 켜며 `SsalddelExecution__Mode=Simulation`을 유지하고 3.5 `SsalddelMartWorkflow`는 끈다. 따라서 이번 변경은 실제 배포를 수행하지 않고, 분리된 3.0 미리보기 범위를 검증할 수 있는 배포 구성까지만 제공한다.

Restaurant Desk의 운영 계정 인증과 사용자-음식점 소유권 claim 연결은 아직 완료되지 않았다. 이 때문에 음식점 주문 수락 화면은 운영 배포 대상이 아니라 Simulation 점검 대상으로 분류한다. 이 권한 경계를 구현하고 서버에서 검증하기 전에는 `Operational`로 전환하면 안 된다.

## 화면 확인

간접 확인만 수행했다. 브라우저 시각 검증을 요청받지 않아 실제 PNG는 추가하지 않았으며, MAUI Blazor 컴파일과 Route·Capability 조립 테스트로 화면 구조를 확인한다. 실제 기기에서는 음식점 주문 알림 수신, 상세 이동, 전표 출력 장치 연동과 배달기사 GPS 권한을 추가 확인해야 한다.

## 검증

- Restaurant Desk, Contracts, Tests project Fast build 경고 0개·오류 0개
- 3.0 Route·Capability·샘플 fallback·기사 compiled binding 경계 targeted test 84개 통과
- 변경 범위 Task validation과 `Ssalddel.v1.5.slnx` Release build 통과
- FDriverApp Android·iOS·MacCatalyst·Windows Release build 경고 0개·오류 0개
- Release 기준 `0.0`·`1.0`·`1.5` build와 전체 test 3,120개 통과
- Azure base compose와 3.0 override 병합 결과 `Simulation`, `FoodDeliveryWorkflow=true`, `SsalddelMartWorkflow=false` 확인
