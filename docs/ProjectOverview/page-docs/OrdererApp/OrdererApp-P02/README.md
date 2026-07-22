# OrdererApp-P02 - 공동구매 개요

[전체 화면 문서](../../README.md) / [OrdererApp 화면 목록](../README.md) / [앱 전체 카탈로그](../../../app-page-catalog.md)

## 화면 캡처

기존 캡처는 상품·수요·원가·선적이 한 화면에 묶인 이전 구조입니다. route SRP 리팩터링 이후 desktop/390px 캡처를 다시 남겨야 합니다.

## 기본 정보

| 항목 | 내용 |
| --- | --- |
| 앱 | OrdererApp |
| 페이지 ID / 제목 | OrdererApp-P02 - 공동구매 개요 |
| 라우트 | `/group-purchase` |
| 소스 파일 | [OrdererApp/Components/Pages/GroupPurchaseHome.razor](../../../../../OrdererApp/Components/Pages/GroupPurchaseHome.razor) |
| 분류 | 1.0 |
| 주 책임 | 공동구매 하위 화면 진입과 1.0/1.5 경계 안내 |
| 캡처 상태 | route SRP 재캡처 필요 |

## 책임 경계

이 개요 화면은 데이터를 저장하거나 원가·선적을 조회하지 않습니다. 사용자가 하려는 한 가지 일을 선택해 다음 route로 이동시키는 색인만 담당합니다.

| 화면 | route | 단일 책임 |
| --- | --- | --- |
| 재료 자동집단화 | `/group-purchase/products` | 재료 카드를 훑고 한 번 클릭해 기존 집단 합류 또는 새 집단 시작 |
| 상품 근거 상세 | `/group-purchase/products/{ProductId}` | 한 상품의 HS·보관·모집 근거 읽기 |
| 상세 조건 수요 등록 | `/group-purchase/demands/new/{ProductId}` | 배송권이 없거나 수량·수령 조건을 직접 조정할 때 사용하는 보조 Action |
| 수입 원가 참고 | `/group-purchase/import-review/{ProductId}` | 1.5 Simulation 수입 참고값 조회 |
| 선적 조회 | `/group-purchase/shipments` | 문서관리번호 한 건의 공개 선적 정보 조회 |

공용 route 계약은 [GroupPurchasePageRoutes.cs](../../../../../Ssalddel.Contracts/Common/Orderer/GroupPurchasePageRoutes.cs), responsive 화면 frame은 [GroupPurchaseScreenFrame.razor](../../../../../Ssalddel.Ui.Common/Areas/App/Components/Orderer/GroupPurchaseScreenFrame.razor)에 둡니다. 따라서 MAUI Blazor와 Web wrapper가 같은 화면 구분과 모바일 레이아웃 규칙을 재사용할 수 있습니다.

기본 참여 경로는 `/group-purchase/products`의 카드 버튼입니다. 로그인 토큰의 온보딩 배송권과 카드 조건으로 배치 미리보기와 비구속 저장을 한 번의 클릭 안에서 연속 실행합니다. 결과와 철회도 같은 카드 안에 남깁니다.

## 보안과 운영 경계

- 1.0 화면은 비구속 수요와 공개 모집까지만 다룹니다.
- 카드 클릭은 명시적 참여 동의이지만 결제나 주문 확정 동의는 아닙니다.
- 수요 저장은 결제·계약·수입 신고·재고·배차·운송 요청을 만들지 않습니다.
- 수입 원가와 선적 화면은 1.5 준비 자산으로 분리하고 운영 효과를 만들지 않습니다.
- 상세주소는 자동집단 원장에 저장하지 않고 사용자가 확인한 모집권 키만 사용합니다.
- 사용자의 원문 ID는 노출하지 않고 사용자·재료·배송권을 해시한 수요 출처 키를 사용합니다.
