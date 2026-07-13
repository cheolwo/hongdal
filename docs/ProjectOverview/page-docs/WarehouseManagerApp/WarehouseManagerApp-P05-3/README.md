# WarehouseManagerApp-P05-3 - 알뜰살뜰 마트 피킹/포장

[전체 화면 문서](../../README.md) / [WarehouseManagerApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

현재 전용 캡처 대기 상태다.

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 라우트 | `/mart/picking` |
| 내비게이션 단계 | 3단계 작업 페이지 |
| 목표 진입 문맥 | 출고·마트 주문 다이어그램의 `피킹·포장` 행동 |
| 소스 | [MartPickingPacking.razor](../../../../../WarehouseManagerApp/Components/Pages/MartPickingPacking.razor) |
| 분류 | 확장 |
| 캡처 | 대기 |

## 왜 필요한가

알뜰살뜰 마트의 출고 예정품을 주문별로 피킹하고 포장 완료와 출고 완료 요청까지 이어준다.

## 사용자와 화면 책임

주 사용자는 창고 작업자다. 페이지는 공통 `HongdalMartPickingPackingWorkflow`를 호스팅하고 마트 작업 보드로 돌아가는 경로를 제공한다. 주문 생성·결제·배송 배차는 다른 화면의 책임이다.

## 상태·보안 점검

피킹 수량, 포장 완료, 출고 요청은 인증된 창고 작업 권한으로 제한해야 한다. 작업 실패 시 이미 처리된 수량을 중복 반영하지 않도록 서버 상태를 다시 조회해야 한다.

## 다른 화면과의 관계

- 이전: [WarehouseManagerApp-P05-1 마트 작업 보드](../WarehouseManagerApp-P05-1/)
- 상위: [WarehouseManagerApp-P05 마트 홈](../WarehouseManagerApp-P05/)
- 목표 흐름: 출고 사방괘 → 주문·피킹 다이어그램 → 마트 주문 노드의 `피킹·포장` 행동 → 이 페이지
