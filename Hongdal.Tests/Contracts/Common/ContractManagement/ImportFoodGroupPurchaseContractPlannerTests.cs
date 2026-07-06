using Hongdal.Contracts.Common.ContractManagement;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Tests.Contracts.Common.ContractManagement;

public sealed class ImportFoodGroupPurchaseContractPlannerTests
{
    [Fact]
    public void Plan_CompleteFoodContract_IsReadyToSign()
    {
        var plan = ImportFoodGroupPurchaseContractPlanner.Plan(CreateDraft());

        Assert.True(plan.IsFoodHsCode);
        Assert.True(plan.CanProceedToReview);
        Assert.Equal(ImportFoodGroupPurchaseContractStatusCode.ReadyToSign, plan.SuggestedStatus);
        Assert.Empty(plan.MissingItems);
        Assert.True(plan.PrivacyAndContractReadiness.IsReadyForInternalReview);
        Assert.NotNull(plan.PrivacyAndContractReadiness.FieldProtectionPlan);
        Assert.Contains(plan.PrivacyAndContractReadiness.FieldProtectionPlan.Rules, x =>
            x.FieldKey == PersonalDataFieldKey.DetailedAddress);
        Assert.Contains("마일스톤 지급 조건", plan.RequiredClauses);
        Assert.Contains("냉장/냉동 보관 및 운송 조건", plan.RequiredClauses);
    }

    [Fact]
    public void Plan_MissingSupplierAndPaymentClause_StaysPendingReview()
    {
        var draft = CreateDraft(
            parties: CreateRequiredParties()
                .Where(x => x.RoleCode != ImportFoodGroupPurchaseContractRoleCode.SupplierOrShipper)
                .ToArray()) with
        {
            HasMilestonePaymentClause = false
        };

        var plan = ImportFoodGroupPurchaseContractPlanner.Plan(draft);

        Assert.True(plan.CanProceedToReview);
        Assert.Equal(ImportFoodGroupPurchaseContractStatusCode.PendingReview, plan.SuggestedStatus);
        Assert.Contains("공급자 또는 화주 당사자", plan.MissingItems);
        Assert.Contains("상차/하차/분배 확인 마일스톤 지급 조항", plan.MissingItems);
    }

    [Fact]
    public void Plan_NonFoodHsCode_IsBlocked()
    {
        var draft = CreateDraft(productCard: CreateProductCard("8543.70"));

        var plan = ImportFoodGroupPurchaseContractPlanner.Plan(draft);

        Assert.False(plan.IsFoodHsCode);
        Assert.False(plan.CanProceedToReview);
        Assert.Equal(ImportFoodGroupPurchaseContractStatusCode.Blocked, plan.SuggestedStatus);
        Assert.Contains("HS 식품 코드 확인", plan.MissingItems);
    }

    [Fact]
    public void Plan_RequiresNonBindingDemandNotice()
    {
        var draft = CreateDraft() with
        {
            HasNonBindingDemandNotice = false
        };

        var plan = ImportFoodGroupPurchaseContractPlanner.Plan(draft);

        Assert.False(plan.CanProceedToReview);
        Assert.Equal(ImportFoodGroupPurchaseContractStatusCode.Blocked, plan.SuggestedStatus);
        Assert.Contains("비구속 수요 확인 고지 조항", plan.MissingItems);
    }

    [Fact]
    public void Plan_MissingPrivacyAndContractReadiness_StaysPendingReview()
    {
        var draft = CreateDraft() with
        {
            ProtectionProfile = new ImportFoodGroupPurchaseContractProtectionProfile(
                HasPurposeAndLegalBasis: true,
                HasConsentOrNotice: true,
                HasRoleBasedAccessControl: true,
                HasAuditLog: true,
                HasSecureDevelopmentReview: true)
        };

        var plan = ImportFoodGroupPurchaseContractPlanner.Plan(draft);

        Assert.True(plan.CanProceedToReview);
        Assert.Equal(ImportFoodGroupPurchaseContractStatusCode.PendingReview, plan.SuggestedStatus);
        Assert.False(plan.PrivacyAndContractReadiness.IsReadyForInternalReview);
        Assert.Contains("P-03", plan.PrivacyAndContractReadiness.MissingRequiredCodes);
        Assert.Contains("S-02", plan.PrivacyAndContractReadiness.MissingRequiredCodes);
    }

    private static ImportFoodGroupPurchaseContractDraft CreateDraft(
        HsFoodGroupPurchaseProductCard? productCard = null,
        IReadOnlyList<ImportFoodGroupPurchaseContractParty>? parties = null)
        => new(
            ContractNumber: "IFGP-2026-0001",
            GroupPurchaseId: "gp-food-1",
            ProductCard: productCard ?? CreateProductCard("0203.29"),
            OrdererGroupScopeKey: "road-address-level-2:gyeonggi-suwon",
            OrdererGroupScopeName: "경기도 수원시 영통구",
            TargetQuantityKg: 12000m,
            EstimatedUnitPrice: 8500m,
            Parties: parties ?? CreateRequiredParties(),
            PaymentPolicy: new GroupPurchasePaymentMilestonePolicy(),
            HasNonBindingDemandNotice: true,
            HasImportFoodReviewClause: true,
            HasColdChainHandlingClause: true,
            HasMilestonePaymentClause: true,
            HasDistributionConfirmationClause: true,
            HasRefundAndCancellationClause: true,
            ProtectionProfile: ImportFoodGroupPurchaseContractProtectionProfile.AllReviewed());

    private static IReadOnlyList<ImportFoodGroupPurchaseContractParty> CreateRequiredParties()
        =>
        [
            new("orderer-1", "개설 신청 주문자", ImportFoodGroupPurchaseContractRoleCode.ApplicantOrderer),
            new("shipper-1", "공급 화주", ImportFoodGroupPurchaseContractRoleCode.SupplierOrShipper),
            new("platform", "홍달 운영자", ImportFoodGroupPurchaseContractRoleCode.PlatformOperator)
        ];

    private static HsFoodGroupPurchaseProductCard CreateProductCard(string hsCode)
        => new(
            ProductCardId: "hs-food-0203-pork-frozen",
            ProductName: "냉동 삼겹살",
            HsCode: hsCode,
            HsDisplayName: "돼지고기 냉동 기타",
            TemperatureCode: GroupPurchaseTemperatureCode.Frozen,
            ExpectedLogisticsMode: GroupPurchaseLogisticsModeCode.Fcl,
            SuggestedTargetQuantityKg: 12000m,
            ExpectedUnitPrice: 8500m);
}
