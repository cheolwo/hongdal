# Feature Flag Policy

기능 플래그는 구현된 코드와 실제 노출·실행을 분리합니다. 제품 버전이 앞선다는 이유만으로 외부 효과를 자동 허용하지 않습니다.

## 기준 키

| 키 | 로드맵 버전 | 기본 운영값 | 범위 |
| --- | --- | --- | --- |
| `CommunityTrustWorkflow` | `0.0` | `true` | 커뮤니티, 참여 동의, 공동 원장과 신뢰 기록 |
| `GroupPurchaseDemandWorkflow` | `1.0` | `false` | 비구속 수요, 주문자 집단화와 공동구매 모집 원장 |
| `GroupPurchasePracticeWorkflow` | `1.0` | `false` | 저장 없는 가상 이웃 공동구매 연습과 실제 수요 전환 안내 |
| `CustomsAndTradeDataWorkflow` | `1.5` | `false` | 공급·가격·HS·HTS·통관 참고 자료와 무역 준비 |
| `DomesticTransportWorkflow` | `2.0` | `false` | 화주 운송 의뢰, 기사 인계, 배차·증빙·정산 준비 |
| `WarehouseFulfillmentWorkflow` | `2.5` | `false` | 입고, 재고, 피킹, 포장과 출고 |
| `SalesChannelFulfillmentWorkflow` | `2.5` | `false` | 판매채널 연결, 주문 출고 후보와 판매 이행 |
| `HrParticipationWorkflow` | `2.5` | `false` | 이행 역할 지원·검토와 운영 주체 준비 |
| `FoodDeliveryWorkflow` | `3.0` | `false` | 음식점 주문, 조리, 픽업과 배송 |
| `SsalddelMartWorkflow` | `3.5` | `false` | 마트 재고, 피킹, 포장과 도심 즉시배송 |

`0.5` 제품 단계는 catalog와 API·page metadata에는 등록됐지만 독립 Feature flag는 아직 release gate입니다. 현재 개별 원함 저장 API는 호환상 `GroupPurchaseDemandWorkflow` 안에 있으므로 이 플래그를 켜면 0.5와 1.0 코드가 함께 열릴 수 있습니다. 0.5 독립 배포 전에는 `IndividualOrderWorkflow` 경계를 추가하고 개별 원장 Command를 1.0 집단화 Command와 분리해야 합니다.

현재 기본 개발·배포 환경은 `CommunityTrustWorkflow=true`만 열고 `GroupPurchasePracticeWorkflow`, `GroupPurchaseDemandWorkflow`, `CustomsAndTradeDataWorkflow`를 모두 `false`로 유지합니다. 0.5·1.0·1.5 검증은 해당 version slice와 명시적 test profile에서만 기능을 켭니다.

## 운영 예시

```json
{
  "VersionFeatureFlags": {
    "CommunityTrustWorkflow": true,
    "GroupPurchasePracticeWorkflow": false,
    "GroupPurchaseDemandWorkflow": false,
    "GroupPurchaseImportWorkflow": false,
    "CustomsAndTradeDataWorkflow": false,
    "DomesticTransportWorkflow": false,
    "WarehouseFulfillmentWorkflow": false,
    "SalesChannelFulfillmentWorkflow": false,
    "HrParticipationWorkflow": false,
    "FoodDeliveryWorkflow": false,
    "SsalddelMartWorkflow": false
  }
}
```

## 활성화 순서

1. `CommunityTrustWorkflow`로 공개 탐색, 동의와 원장 기반을 유지합니다.
2. 0.5 전용 경계가 완성되기 전에는 `GroupPurchaseDemandWorkflow`를 제한된 Simulation profile에서만 사용해 개별 원장과 집단화의 분리 여부를 함께 검증합니다.
3. 0.5 개별 원장을 독립 활성화한 뒤 공동 참여에 동의한 원장에만 1.0 집단화를 적용합니다.
4. `CustomsAndTradeDataWorkflow`로 공급자·견적·HS·HTS와 수입 준비 자료를 연결합니다.
5. 운송 책임과 허가 경계가 준비된 환경에서만 `DomesticTransportWorkflow`를 검증합니다.
6. 실제 입고·재고·판매 이행이 필요할 때 `WarehouseFulfillmentWorkflow`와 `SalesChannelFulfillmentWorkflow`를 검증합니다.

각 단계는 독립적으로 끌 수 있어야 합니다. `CustomsAndTradeDataWorkflow`는 단독으로 켜도 활성화되지 않고 `CommunityTrustWorkflow`와 `GroupPurchaseDemandWorkflow`가 함께 켜져야 합니다. 국내 생산자 공동구매는 통관 기능 없이 `1.0`만으로 검증할 수 있고, 기존 화주의 운송 테스트 자산은 별도 제한 환경에서 보존할 수 있습니다.

## 서버 차단

`RequireVersionFeatureAttribute`는 비활성 기능 API에 `404 Not Found`와 `FeatureDisabled` 오류를 반환합니다. UI 숨김만으로 서버 실행을 허용하지 않습니다.

`SsalddelApiVersionAttribute`의 제품 버전은 `/api/v1/...` HTTP 계약 버전과 다릅니다. 제품 로드맵 버전은 다음처럼 기록합니다.

```csharp
[SsalddelApiVersion(
    SsalddelProductVersion.V1_0,
    FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
public sealed class 공동구매자동집단화Controller : ControllerBase
{
}
```

## 실행 경계

기능 플래그가 켜져 있어도 `SsalddelExecution:Mode=Simulation`이면 실제 결제, 계약 체결, 신고, 자동 배차와 외부 정산을 실행하지 않습니다. 운영 전환은 기능 플래그와 별도의 배포·법무·운영 결정입니다.

## 호환 키

다음 키는 기존 로컬 설정을 읽기 위한 별칭입니다. 새 문서와 신규 설정에는 기준 키만 사용합니다.

| 기존 키 | 기준 키 | 이전 버전명 |
| --- | --- | --- |
| `GroupPurchaseImportWorkflow` | `GroupPurchaseDemandWorkflow` | 공동구매·공동수입 통합 키 |
| `OrdererGroupOrderV25` | `GroupPurchaseDemandWorkflow` | 공동구매 `2.5` |
| `ApartmentGroupOrderV25` | `GroupPurchaseDemandWorkflow` | 공동주택 공동구매 `2.5` |
| `CustomsHsV20` | `CustomsAndTradeDataWorkflow` | 통관·HS `2.0` |
| `CargoYongdalV1` | `DomesticTransportWorkflow` | 화물·용달 `1.0` |
| `WarehouseV15` | `WarehouseFulfillmentWorkflow` | 창고 `1.5` |
| `FoodDeliveryV30` | `FoodDeliveryWorkflow` | 음식 배달 `3.0` |
| `SsalddelMartV35` | `SsalddelMartWorkflow` | 마트 `3.5` |

호환 키 이름은 즉시 삭제하지 않습니다. 설정 마이그레이션 기간에는 기준 키와 별칭 중 하나가 켜져 있으면 같은 워크플로우로 해석합니다.
