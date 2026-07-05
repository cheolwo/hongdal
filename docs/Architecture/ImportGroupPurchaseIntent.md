# Import Group Purchase Intent

수입 공동구매 의향 기능은 화주가 수입을 확정하기 전에 주문자 수요를 확인하는 중간 단계다. 커뮤니티 글처럼 가볍게 시작하되, 수량과 가격 의향은 구조화해서 나중에 수입, 통관, 입고, 판매 흐름으로 연결한다.

## 목적

- 주문자는 수입 예정 상품에 "나도 구매하겠다"는 의향을 남긴다.
- 화주는 누적 의향 수량과 희망가를 보고 수입 여부, 수입 수량, FCL/LCL 선택, 판매가를 판단한다.
- 구매 의향은 결제나 구매 확정이 아니다. 실제 결제와 개인정보 확인은 수입 진행 확정 뒤 별도 단계에서 처리한다.
- 플랫폼은 커뮤니티의 자발성을 유지하면서도 법무/통관/물류 기능으로 넘어가는 지점을 구조화한다.

## 현재 UI 골격

- App: `OrdererApp`
- Route: `/group-purchase`
- Entry points:
  - 주문자 홈 QuickAction
  - 주문자 사이드 메뉴
- 현재 구현은 샘플 데이터 기반 화면이다. 서버 저장, 결제, 알림, 커뮤니티 게시글 연결은 후속 단계다.

## 화면 구조

- 왼쪽: 진행 중인 수요 확인 캠페인 목록
- 오른쪽 상단: 선택된 상품의 예상 판매가, 수요 달성률, 수입 판단
- 접이식 상세:
  - 상품/수입 정보
  - 물류/통관 정보
  - 가격/혜택 정보
  - 내 구매 의향
- 구매 의향 입력:
  - 희망 수량
  - 희망 단가
  - 수령 지역
  - 화주에게 남길 메모
  - 비확정 의향 동의

## 서버 모델 초안

### ImportDemandCampaign

- `Id`
- `ShipperId`
- `CommunityPostId`
- `ProductName`
- `Summary`
- `TargetQuantity`
- `CurrentIntentQuantity`
- `TargetUnitPrice`
- `LogisticsMode`
- `HsReviewStatus`
- `CustomsMemo`
- `ExpectedArrivalWindow`
- `Status`
- `CreatedAt`
- `ClosedAt`

### PurchaseIntent

- `Id`
- `CampaignId`
- `OrdererId`
- `DesiredQuantity`
- `DesiredUnitPrice`
- `DeliveryRegion`
- `Memo`
- `IsBinding`
- `CreatedAt`
- `CancelledAt`

`IsBinding`은 첫 골격에서는 항상 `false`로 둔다. 결제 예약금, 우선구매권, 실제 주문 전환을 도입할 때만 별도 정책과 약관을 붙여 `true` 성격의 흐름을 만든다.

## 상태 흐름

1. `Draft`: 화주가 FCL/LCL 판단 또는 커뮤니티 글 초안을 만든다.
2. `DemandChecking`: 주문자들이 구매 의향을 남긴다.
3. `ImportDecision`: 화주가 수량, 가격, 통관 리스크를 보고 진행 여부를 판단한다.
4. `ImportInProgress`: 수입, 통관, 창고 입고가 진행된다.
5. `SaleOpened`: 구매 의향자에게 실제 주문 또는 결제 링크를 안내한다.
6. `Cancelled`: 수요 부족, 통관 리스크, 가격 문제로 중단한다.

## 연결 지점

- `ShipperApp` FCL/LCL 판단 화면에서 수요 확인 캠페인을 생성한다.
- `PlatformCommunityHome`은 캠페인형 게시글을 노출할 수 있다.
- `OrdererApp`은 주문자가 의향을 남기는 전용 화면을 제공한다.
- HS 코드/관세사 검토는 캠페인의 통관 신뢰도를 보완한다.
- 창고 입고가 확정되면 홈달마트, 일반 상품 주문, 택배/도심 배송으로 전환한다.

## 다음 작업

- 캠페인과 구매 의향 저장 API를 만든다.
- 커뮤니티 게시글 유형에 `ImportDemandCampaign` 연결 키를 둔다.
- 주문자별 의향 수정/취소 화면을 추가한다.
- 화주 앱에서 누적 의향, 희망가 분포, 지역별 수요를 보는 대시보드를 만든다.
- 진행 확정 후 실제 주문, 결제, 입고 알림으로 전환하는 Command 흐름을 설계한다.
