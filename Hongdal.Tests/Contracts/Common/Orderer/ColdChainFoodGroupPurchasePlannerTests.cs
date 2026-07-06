using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Tests.Contracts.Common.Orderer;

public sealed class ColdChainFoodGroupPurchasePlannerTests
{
    [Fact]
    public void Plan_FrozenImportedFood_MarksColdChainReviewWhenDemandIsEnough()
    {
        var draft = new ColdChainFoodGroupPurchaseDraft(
            ProductName: "냉동 삼겹살",
            OrdererGroupScopeKey: "road-address-level-2:gyeonggi-suwon",
            TargetQuantityKg: 12000m,
            CurrentIntentQuantityKg: 9000m,
            TargetUnitPrice: 8500m,
            HsCode: "0203.29");

        var plan = ColdChainFoodGroupPurchasePlanner.Plan(draft);

        Assert.True(plan.IsDemandThresholdMet);
        Assert.True(plan.IsFclCandidate);
        Assert.True(plan.IsHsFoodCandidate);
        Assert.Equal(GroupPurchaseActivationPriorityCode.ColdChainFoodFocused, plan.ActivationPriority);
        Assert.Equal(GroupPurchaseCampaignStatusCode.ColdChainReview, plan.SuggestedStatus);
        Assert.Contains("HS 식품 분류 확인", plan.RequiredReviewSteps);
        Assert.Contains("수입식품 신고/검역 검토", plan.RequiredReviewSteps);
        Assert.Contains("냉장/냉동 창고와 운송 가능 여부 확인", plan.RequiredReviewSteps);
    }

    [Fact]
    public void Plan_StaysDemandCheckingWhenDemandIsLow()
    {
        var draft = new ColdChainFoodGroupPurchaseDraft(
            ProductName: "냉동 삼겹살",
            OrdererGroupScopeKey: "road-address-level-2:seoul-gangnam",
            TargetQuantityKg: 12000m,
            CurrentIntentQuantityKg: 3000m,
            TargetUnitPrice: 8500m);

        var plan = ColdChainFoodGroupPurchasePlanner.Plan(draft);

        Assert.False(plan.IsDemandThresholdMet);
        Assert.Equal(GroupPurchaseCampaignStatusCode.DemandChecking, plan.SuggestedStatus);
    }

    [Fact]
    public void Plan_DomesticAmbientBulk_CanMoveToImportDecisionWithoutColdChainReview()
    {
        var draft = new ColdChainFoodGroupPurchaseDraft(
            ProductName: "상온 보관 식재료",
            OrdererGroupScopeKey: "road-address-level-2:busan-haeundae",
            TargetQuantityKg: 2000m,
            CurrentIntentQuantityKg: 2000m,
            TargetUnitPrice: 3000m,
            CategoryCode: GroupPurchaseCategoryCode.GeneralCommerce,
            TemperatureCode: GroupPurchaseTemperatureCode.Ambient,
            LogisticsMode: GroupPurchaseLogisticsModeCode.DomesticBulk,
            RequiresImportFoodReview: false,
            RequiresMfdsManufacturerReview: false,
            RequiresColdStorage: false);

        var plan = ColdChainFoodGroupPurchasePlanner.Plan(draft);

        Assert.False(plan.IsFclCandidate);
        Assert.False(plan.IsHsFoodCandidate);
        Assert.Equal(GroupPurchaseActivationPriorityCode.Normal, plan.ActivationPriority);
        Assert.Empty(plan.RequiredReviewSteps);
        Assert.Equal(GroupPurchaseCampaignStatusCode.ImportDecision, plan.SuggestedStatus);
    }

    [Theory]
    [InlineData("0203.29")]
    [InlineData("1602.49")]
    [InlineData("2106.90")]
    public void Plan_HsFoodChapters_AreFoodFocusedCandidates(string hsCode)
    {
        var draft = new ColdChainFoodGroupPurchaseDraft(
            ProductName: "먹거리 공동구매 후보",
            OrdererGroupScopeKey: "road-address-level-2:incheon-yeonsu",
            TargetQuantityKg: 5000m,
            CurrentIntentQuantityKg: 1000m,
            TargetUnitPrice: 6000m,
            HsCode: hsCode,
            TemperatureCode: GroupPurchaseTemperatureCode.Chilled);

        var plan = ColdChainFoodGroupPurchasePlanner.Plan(draft);

        Assert.True(plan.IsHsFoodCandidate);
        Assert.Equal(GroupPurchaseActivationPriorityCode.ColdChainFoodFocused, plan.ActivationPriority);
        Assert.Contains("HS 식품 분류 확인", plan.RequiredReviewSteps);
    }

    [Fact]
    public void Plan_NonFoodHsCode_DoesNotRaiseFoodFocusedPriority()
    {
        var draft = new ColdChainFoodGroupPurchaseDraft(
            ProductName: "일반 커머스 상품",
            OrdererGroupScopeKey: "road-address-level-2:seoul-songpa",
            TargetQuantityKg: 5000m,
            CurrentIntentQuantityKg: 5000m,
            TargetUnitPrice: 6000m,
            HsCode: "8543.70",
            CategoryCode: GroupPurchaseCategoryCode.GeneralCommerce,
            TemperatureCode: GroupPurchaseTemperatureCode.Ambient,
            LogisticsMode: GroupPurchaseLogisticsModeCode.Lcl,
            RequiresImportFoodReview: false,
            RequiresMfdsManufacturerReview: false,
            RequiresColdStorage: false);

        var plan = ColdChainFoodGroupPurchasePlanner.Plan(draft);

        Assert.False(plan.IsHsFoodCandidate);
        Assert.Equal(GroupPurchaseActivationPriorityCode.Normal, plan.ActivationPriority);
    }

    [Fact]
    public void OpeningApplication_FoodProductCard_WithAgreement_CanSubmitForApproval()
    {
        var draft = new OrdererGroupOpeningApplicationDraft(
            ApplicantOrdererId: "orderer-1",
            ProductCard: CreatePorkProductCard(),
            OrdererGroupScopeKey: "road-address-level-2:gyeonggi-suwon",
            OrdererGroupScopeName: "경기도 수원시 영통구",
            DesiredQuantityKg: 20m,
            DesiredUnitPrice: 8500m,
            NonBindingAgreementAccepted: true,
            RequestMemo: "동네 사람들과 같이 주문하고 싶습니다.");

        var plan = OrdererGroupOpeningApplicationPlanner.Plan(draft);

        Assert.True(plan.CanSubmit);
        Assert.True(plan.IsFoodFocusedCandidate);
        Assert.Equal(OrdererGroupOpeningApplicationStatusCode.PendingApproval, plan.SuggestedStatus);
        Assert.Contains("주문자 집단 범위 승인", plan.RequiredAdminReviewSteps);
        Assert.Contains("HS 식품 코드 확인", plan.RequiredAdminReviewSteps);
        Assert.Contains("냉장/냉동 보관 및 운송 검토", plan.RequiredAdminReviewSteps);
    }

    [Fact]
    public void OpeningApplication_RequiresNonBindingAgreementBeforeSubmit()
    {
        var draft = new OrdererGroupOpeningApplicationDraft(
            ApplicantOrdererId: "orderer-1",
            ProductCard: CreatePorkProductCard(),
            OrdererGroupScopeKey: "road-address-level-2:gyeonggi-suwon",
            OrdererGroupScopeName: "경기도 수원시 영통구",
            DesiredQuantityKg: 20m,
            DesiredUnitPrice: 8500m,
            NonBindingAgreementAccepted: false);

        var plan = OrdererGroupOpeningApplicationPlanner.Plan(draft);

        Assert.False(plan.CanSubmit);
        Assert.Equal(OrdererGroupOpeningApplicationStatusCode.Draft, plan.SuggestedStatus);
    }

    private static HsFoodGroupPurchaseProductCard CreatePorkProductCard()
    {
        return new HsFoodGroupPurchaseProductCard(
            ProductCardId: "hs-food-0203-pork-frozen",
            ProductName: "냉동 삼겹살",
            HsCode: "0203.29",
            HsDisplayName: "돼지고기 냉동 기타",
            TemperatureCode: GroupPurchaseTemperatureCode.Frozen,
            ExpectedLogisticsMode: GroupPurchaseLogisticsModeCode.Fcl,
            SuggestedTargetQuantityKg: 12000m,
            ExpectedUnitPrice: 8500m);
    }
}
