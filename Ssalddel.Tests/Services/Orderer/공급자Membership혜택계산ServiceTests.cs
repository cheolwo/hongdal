using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공급자Membership혜택계산ServiceTests
{
    private readonly 공급자Membership혜택계산Service sut = new();

    [Fact]
    public void 활성_농업경영체_Membership은_확인된_할인을_주문예상액에_반영한다()
    {
        var result = sut.미리보기(CreateRequest(
            membershipStatusCode: SupplierMembershipStatusCodes.Active));

        Assert.True(result.BenefitOfferEligible);
        Assert.True(result.BenefitApplied);
        Assert.Equal(2_000m, result.MonthlyFeeAmount);
        Assert.Equal(3_000m, result.PotentialDiscountAmount);
        Assert.Equal(3_000m, result.AppliedDiscountAmount);
        Assert.Equal(27_000m, result.EstimatedOrderAmount);
        Assert.Equal(1_000m, result.PotentialNetBenefitAfterMonthlyFee);
        Assert.Equal(1, result.EstimatedOrdersToBreakEven);
        Assert.False(result.MembershipChargeExecutionAllowed);
        Assert.False(result.OrderExecutionAllowed);
    }

    [Fact]
    public void 가입전_관심구독은_예상혜택만_보이고_주문가격에는_적용하지_않는다()
    {
        var result = sut.미리보기(CreateRequest(
            membershipStatusCode: SupplierMembershipStatusCodes.InterestFollowing));

        Assert.Equal(3_000m, result.PotentialDiscountAmount);
        Assert.Equal(0m, result.AppliedDiscountAmount);
        Assert.Equal(30_000m, result.EstimatedOrderAmount);
        Assert.True(result.RequiresMembershipActivation);
        Assert.Contains("가입 전 예상 혜택", result.BenefitReason);
    }

    [Fact]
    public void 배송권_집단_Membership은_각_구성원의_별도동의를_요구한다()
    {
        var request = CreateRequest(
            membershipStatusCode: SupplierMembershipStatusCodes.EnrollmentDraft);
        request.AudienceTypeCode =
            SupplierRelationshipAudienceTypeCodes.DeliveryScopeGroup;
        request.DeliveryScopeKey = "kr-admin2:서울특별시-양천구";

        var result = sut.미리보기(request);

        Assert.True(result.RequiresEachGroupMemberConsent);
        Assert.True(result.RequiresSeparateMembershipConsent);
        Assert.True(result.RequiresSeparateOrderConsent);
    }

    [Fact]
    public void 해외제조업체_근거가_확인되지_않으면_혜택을_적용하지_않는다()
    {
        var request = CreateRequest(
            membershipStatusCode: SupplierMembershipStatusCodes.Active);
        request.SupplierPartyTypeCode =
            SupplierRelationshipPartyTypeCodes.OverseasFoodManufacturer;
        request.SupplierEvidenceVerified = false;

        var result = sut.미리보기(request);

        Assert.False(result.BenefitOfferEligible);
        Assert.False(result.BenefitApplied);
        Assert.Equal(0m, result.PotentialDiscountAmount);
        Assert.Equal(30_000m, result.EstimatedOrderAmount);
        Assert.Contains("업체 근거", result.BenefitReason);
    }

    [Fact]
    public void 음수_월구독료는_거절한다()
    {
        var request = CreateRequest(
            membershipStatusCode: SupplierMembershipStatusCodes.Active);
        request.MonthlyFeeAmount = -1m;

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.미리보기(request));
    }

    private static SupplierMembershipBenefitPreviewRequest CreateRequest(
        string membershipStatusCode)
        => new()
        {
            SupplierKey = "farm-apple-01",
            SupplierDisplayName = "충주 사과 농업경영체",
            SupplierPartyTypeCode =
                SupplierRelationshipPartyTypeCodes.DomesticAgriculturalBusiness,
            AudienceTypeCode =
                SupplierRelationshipAudienceTypeCodes.IndividualOrderer,
            MembershipStatusCode = membershipStatusCode,
            BenefitTypeCode =
                SupplierMembershipBenefitTypeCodes.PercentageDiscount,
            MonthlyFeeAmount = 2_000m,
            CurrencyCode = "KRW",
            DiscountRatePercent = 10m,
            MaximumDiscountAmount = 5_000m,
            OrderSubtotalAmount = 30_000m,
            ProductEligible = true,
            SupplierBenefitOfferConfirmed = true,
            SupplierEvidenceVerified = true,
            TermsVersion = "2026-07"
        };
}
