# 사방괘 기본 목적지 Navigation 계약 정렬

## 결과

공용 사방괘 역할 전환 화면이 Web 또는 모바일 전용 URL을 직접 소유하지 않도록 정리했다. 사방괘 base, 커뮤니티 복귀, 주문·판매·창고·운송·합의 목적지는 이제 `CommunityPageRoutes`, `SalesOrderPageRoutes`, `ShipperHomePageRoutes`에서만 가져온다.

기존 운송 기본 링크 `/shipper/transport`는 `SsalddelApp`에는 있지만 Web에는 Route Page가 없었다. 공용 사방괘 목적지는 두 플랫폼에 모두 존재하는 `/shipper/request`로 변경했다. 판매 기본 링크도 플랫폼별 의미가 다른 `/shipper/sales/listings` 대신 두 플랫폼에서 같은 영속 원장을 읽는 `/shipper/sales/orders`로 변경했다. 모바일 전용 운송 작업대와 출품 Simulation 화면 자체는 삭제하지 않았다.

## 공용 목적지

| 도착 업무 | 변경 전 | 변경 후 | 효과 경계 |
| --- | --- | --- | --- |
| 주문 | `/shipper/sales/orders` | `/shipper/sales/orders` | 영속 주문 원장 읽기 |
| 판매 | `/shipper/sales/listings` | `/shipper/sales/orders` | 플랫폼별 출품 의미 충돌을 공용 기본 링크에서 제외 |
| 창고 | `/shipper/warehouse/workspace` literal | `ShipperHomePageRoutes.WarehouseWorkspace` | 기존 창고 작업대, 권한·기능 플래그 재확인 |
| 운송 | `/shipper/transport` | `/shipper/request` | Web·모바일 공용 의뢰 작성 흐름, 자동 배차 없음 |
| 합의 | `/community/group-purchase` literal | `CommunityPageRoutes.GroupPurchase` | 공동구매 합의 진입 |

역할 관점은 계속 화면 해석만 제공한다. 로그인, 원장 참여, 기능 플래그, 수정·투표·전자서명·실행 권한은 목적지 화면과 서버가 다시 확인하며 이번 변경은 Command나 운영 효과를 실행하지 않는다.

## 책임 경계

- `CommunityPageRoutes.Bagua`가 사방괘 canonical base를 소유한다.
- `BaguaRoleTransitionPageModel`은 업무 code를 공용 목적지 계약으로 변환한다.
- `BaguaRoleTransitionPageViewModel`의 커뮤니티 복귀도 `CommunityPageRoutes.Home`을 사용한다.
- Web `ShipperRoutes.WarehouseWorkspace`와 모바일의 같은 항목은 동일한 `ShipperHomePageRoutes` 상수를 참조한다.
- 조립 회귀 테스트가 판매 주문, 창고 작업대, 운송 의뢰 Route Page가 Web·모바일 양쪽에 실제 존재하는지 확인한다.

## 화면 확인

화면 없음 · 간접 확인. DOM, 문구, CSS와 배치는 바꾸지 않았고 기본 목적지 href 의미만 정렬했다. 새 PNG 대신 source composition test와 공용 route 계약 test로 양쪽 Route Page 연결을 확인했다.

## 검증

- 사방괘·커뮤니티·화주 route 선택 테스트 46개 통과
- clean-index 전체 `Ssalddel.Tests` 2,617개 통과, 실패·건너뜀 0개
- clean-index `Ssalddel.WebApp` build 경고 0개·오류 0개
- clean-index `SsalddelApp` `net10.0-windows10.0.19041.0` build 경고 0개·오류 0개
- 변경 파일 `git diff --check` 통과

## 다음 작업

`PlatformCommunityHome`의 다이어그램 원장 node 상세에 남은 업무 URL literal과 임시 화주 요청 ID를 다음 `P0-0` 수직 단위로 감사한다. stable-ID route builder와 안전한 `from` 문맥을 사용하고, Web·모바일·전문 앱이 제공하지 않는 목적지는 platform navigation adapter가 열지 않도록 한다.
