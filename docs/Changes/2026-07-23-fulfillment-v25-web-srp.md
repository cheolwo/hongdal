# 2.5 Web 창고·판매 이행 단일책임 정리

## 결과

통합 Web의 `2.5` 입고·창고·판매 이행 화면을 서버 원장과 사용자 목표 기준으로 다시 닫았다. 기존에 한 재고 화면이 재고 조회, 판매상품 생성, 채널 출품까지 연속 실행하던 구조를 없애고 각 저장을 독립 Route에서 명시적으로 실행하도록 분리했다.

| Route | 단일 책임 | 실행 경계 |
| --- | --- | --- |
| `/shipper/warehouse/inventory` | 사용자 소유 재고 조회와 다음 목표 선택 | ReadOnly |
| `/shipper/sales/products` | 판매상품 원장 조회 | ReadOnly |
| `/shipper/sales/products/new?inventoryItemId={id}` | 정확한 재고 ID 한 건으로 판매상품 생성 | PlatformPersistence |
| `/shipper/sales/listings` | 내부 채널 출품 준비 원장 조회 | ReadOnly |
| `/shipper/sales/listings/new?productId={id}` | 판매상품과 채널계정을 직접 선택해 준비 원장 생성 | PlatformPersistence |
| `/warehouse/work-board` | 실제 목적별 작업 화면 진입 | ReadOnly |

판매상품 생성은 채널 출품을 자동 실행하지 않는다. 출품 준비 생성도 외부 채널 인증, 상품 발행과 주문 동기화를 실행하지 않는다.

## 서버 원장 경계

- Web 입고·창고 서비스의 고정 창고·입고·재고 샘플을 제거했다.
- `ISsalddelJsonApiClient`를 통해 `api/v1/warehouse-operations`의 인증·권한·기능 플래그 경계를 그대로 사용한다.
- 판매상품과 출품 화면은 공용 `I상품등록Service`, `I채널출품Service`, `I판매채널계정읽기Service`를 사용한다.
- API 실패나 빈 응답을 샘플 데이터로 바꾸지 않는다.
- 창고 작업 보드에서 실제 작업처럼 보이던 고정 업무 번호와 상태를 제거하고 서버 원장 화면 링크만 남겼다.

## 2.5 배포 경계

`compose.fulfillment-v25.override.yaml`은 창고·판매·HR 2.5 기능을 켜되 `SsalddelExecution__Mode=Simulation`을 유지한다. 외부 판매채널 주문 동기화와 업무 관계 snapshot 효과는 별도로 비활성화한다.

입고·창고·판매·창고 관리자 capability는 제품 버전 `2.5`로 통일하고, 조회 Route와 내부 저장 Route를 구분했다.

## 화면 확인

간접 확인만 수행했다. 이번 작업에서는 브라우저 시각 검증을 요청받지 않아 실제 PNG를 새로 만들지 않았으며, Blazor 컴파일과 Route·ViewModel 조립 테스트로 화면 구조를 확인했다. 실제 배포 전 로그인 계정의 창고 역할과 사용자 소유 원장을 준비한 뒤 desktop·mobile 시각 검증이 남아 있다.

## 검증

- `Ssalddel.WebApp` build 경고 0개·오류 0개
- 2.5 ViewModel·Route·capability·통합 카탈로그 targeted test 189개 통과
- 변경 범위 Fast·Task validation 통과
- Release 기준 `0.0`·`1.0`·`1.5` build와 전체 test 3,109개 통과
- Azure base compose와 2.5 override 병합 config 통과
