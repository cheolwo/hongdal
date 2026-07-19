using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.Tests.Contracts.Common.ContractManagement;

public sealed class 수입식품공동주문계약검토계획기Tests
{
    [Fact]
    public void Plan_CompleteFoodContract_IsReadyToSign()
    {
        var plan = 수입식품공동주문계약검토계획기.계획(Create초안());

        Assert.True(plan.IsFoodHS코드);
        Assert.True(plan.CanProceedToReview);
        Assert.Equal(수입식품공동주문계약상태코드.ReadyToSign, plan.제안상태);
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
        var draft = Create초안(
            parties: CreateRequiredParties()
                .Where(x => x.RoleCode != 수입식품공동주문계약역할코드.SupplierOrShipper)
                .ToArray()) with
        {
            HasMilestonePaymentClause = false
        };

        var plan = 수입식품공동주문계약검토계획기.계획(draft);

        Assert.True(plan.CanProceedToReview);
        Assert.Equal(수입식품공동주문계약상태코드.PendingReview, plan.제안상태);
        Assert.Contains("공급자 또는 화주 당사자", plan.MissingItems);
        Assert.Contains("상차/하차/분배 확인 마일스톤 지급 조항", plan.MissingItems);
    }

    [Fact]
    public void Plan_NonFoodHS코드_IsBlocked()
    {
        var draft = Create초안(productCard: Create상품카드("8543.70"));

        var plan = 수입식품공동주문계약검토계획기.계획(draft);

        Assert.False(plan.IsFoodHS코드);
        Assert.False(plan.CanProceedToReview);
        Assert.Equal(수입식품공동주문계약상태코드.Blocked, plan.제안상태);
        Assert.Contains("HS 식품 코드 확인", plan.MissingItems);
    }

    [Fact]
    public void Plan_RequiresNonBindingDemandNotice()
    {
        var draft = Create초안() with
        {
            HasNonBindingDemandNotice = false
        };

        var plan = 수입식품공동주문계약검토계획기.계획(draft);

        Assert.False(plan.CanProceedToReview);
        Assert.Equal(수입식품공동주문계약상태코드.Blocked, plan.제안상태);
        Assert.Contains("비구속 수요 확인 고지 조항", plan.MissingItems);
    }

    [Fact]
    public void Plan_MissingPrivacyAndContractReadiness_StaysPendingReview()
    {
        var draft = Create초안() with
        {
            ProtectionProfile = new 수입식품공동주문계약보호프로필(
                HasPurposeAndLegalBasis: true,
                HasConsentOrNotice: true,
                HasRoleBasedAccessControl: true,
                HasAuditLog: true,
                HasSecureDevelopmentReview: true)
        };

        var plan = 수입식품공동주문계약검토계획기.계획(draft);

        Assert.True(plan.CanProceedToReview);
        Assert.Equal(수입식품공동주문계약상태코드.PendingReview, plan.제안상태);
        Assert.False(plan.PrivacyAndContractReadiness.IsReadyForInternalReview);
        Assert.Contains("P-03", plan.PrivacyAndContractReadiness.MissingRequiredCodes);
        Assert.Contains("S-02", plan.PrivacyAndContractReadiness.MissingRequiredCodes);
    }

    [Fact]
    public void Plan_WithCompletedElectronicSignatures_IsSigned()
    {
        var now = new DateTimeOffset(2026, 7, 6, 10, 0, 0, TimeSpan.Zero);
        var parties = CreateRequiredParties();
        var bundle = ContractElectronicSignaturePlanner.CreateBundleFromParties(
            "IFGP-2026-0001",
            "sha256:contract-document",
            parties,
            now,
            now.AddDays(7));
        foreach (var party in parties.Where(x => x.IsRequiredSigner))
        {
            bundle = ContractElectronicSignaturePlanner.AddEvidence(
                bundle,
                new ContractSignatureEvidence(
                    party.PartyId,
                    party.DisplayName,
                    ContractSignatureMethodCode.PlatformClickSign,
                    "sha256:contract-document",
                    "sha256:consent-text",
                    $"sha256:evidence-{party.PartyId}",
                    now.AddMinutes(1),
                    $"sha256:ip-{party.PartyId}"));
        }

        var draft = Create초안(parties: parties) with
        {
            SignatureBundle = bundle
        };

        var plan = 수입식품공동주문계약검토계획기.계획(draft, now.AddMinutes(1));

        Assert.Equal(수입식품공동주문계약상태코드.Signed, plan.제안상태);
        Assert.NotNull(plan.SignaturePlan);
        Assert.True(plan.SignaturePlan.IsFullySigned);
    }

    private static 수입식품공동주문계약초안 Create초안(
        HS먹거리공동구매상품카드? productCard = null,
        IReadOnlyList<수입식품공동주문계약당사자>? parties = null)
        => new(
            ContractNumber: "IFGP-2026-0001",
            GroupPurchaseId: "gp-food-1",
            상품카드: productCard ?? Create상품카드("0203.29"),
            주문자집단배송권키: "road-address-level-2:gyeonggi-suwon",
            주문자집단배송권명: "경기도 수원시 영통구",
            TargetQuantityKg: 12000m,
            EstimatedUnitPrice: 8500m,
            Parties: parties ?? CreateRequiredParties(),
            PaymentPolicy: new 공동구매결제단계정책(),
            HasNonBindingDemandNotice: true,
            HasImportFoodReviewClause: true,
            HasColdChainHandlingClause: true,
            HasMilestonePaymentClause: true,
            HasDistributionConfirmationClause: true,
            HasRefundAndCancellationClause: true,
            ProtectionProfile: 수입식품공동주문계약보호프로필.AllReviewed());

    private static IReadOnlyList<수입식품공동주문계약당사자> CreateRequiredParties()
        =>
        [
            new("orderer-1", "개설 신청 주문자", 수입식품공동주문계약역할코드.ApplicantOrderer),
            new("shipper-1", "공급 화주", 수입식품공동주문계약역할코드.SupplierOrShipper),
            new("platform", "살뜰 운영자", 수입식품공동주문계약역할코드.PlatformOperator)
        ];

    private static HS먹거리공동구매상품카드 Create상품카드(string hsCode)
        => new(
            상품카드Id: "hs-food-0203-pork-frozen",
            상품명: "냉동 삼겹살",
            HS코드: hsCode,
            HS표시명: "돼지고기 냉동 기타",
            온도코드: 공동구매온도코드.냉동,
            예상물류방식: 공동구매물류방식코드.FCL,
            SuggestedTargetQuantityKg: 12000m,
            ExpectedUnitPrice: 8500m);
}
