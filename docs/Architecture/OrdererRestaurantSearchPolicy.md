# Orderer Restaurant Search Policy

주문자 앱의 음식점 조회 반경은 운영자가 조정할 수 있는 플랫폼 정책으로 분리한다.

## 현재 구현

- 서버 공개 조회 API: `GET /api/v1/orderer/restaurant-search-policy`
- 서버 관리자 API:
  - `GET /api/v1/admin/orderer/restaurant-search-policy`
  - `PUT /api/v1/admin/orderer/restaurant-search-policy`
  - `POST /api/v1/admin/orderer/restaurant-search-policy/reset`
- Admin 화면: `/restaurant-search-policy`
- OrdererApp 정책 소비: `IRestaurantSearchPolicyService`가 서버 공개 API를 우선 조회하고, 실패하면 기본 7km 정책으로 fallback
- 기본 정책:
  - 기본 반경: 7km
  - 허용 범위: 1km ~ 10km
  - 빠른 선택: 3km, 5km, 7km, 10km
  - 배달료 주의 반경: 10km

## 설계 의도

- 음식 주문은 너무 먼 음식점까지 노출하면 배달료 부담이 커질 수 있으므로 조회 반경을 플랫폼 정책으로 둔다.
- OrdererApp에는 정책 소비 서비스 경계를 유지하고, 서버/Admin은 운영자가 값을 조정하는 관리 경계를 담당한다.
- 현재 저장소는 인메모리 골격이다. 실제 운영 전에는 DB 엔티티와 감사 로그를 붙여야 한다.

## 다음 작업

- 정책 변경 이력을 DB에 저장하고 관리자 감사 로그에 남긴다.
- 음식점 조회 API가 반경 정책의 `MaxRadiusKm`를 초과한 요청을 보정하도록 연결한다.
