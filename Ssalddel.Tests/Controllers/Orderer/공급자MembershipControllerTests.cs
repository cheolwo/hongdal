using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ssalddel.Tests.Controllers.Orderer;

public sealed class 공급자MembershipControllerTests
{
    [Fact]
    public void 혜택미리보기는_route의_공급자Key를_사용하고_실행효과를_허용하지_않는다()
    {
        var controller = new 공급자MembershipController(
            new 공급자Membership혜택계산Service(),
            CreateInterestService());
        var request = CreateRequest();
        request.SupplierKey = "body-supplier";

        var action = controller.혜택미리보기("route-supplier", request);

        var ok = Assert.IsType<OkObjectResult>(action);
        var response = Assert.IsType<SupplierMembershipBenefitPreviewResponse>(ok.Value);
        Assert.Equal("route-supplier", response.SupplierKey);
        Assert.False(response.MembershipChargeExecutionAllowed);
        Assert.False(response.OrderExecutionAllowed);
    }

    [Fact]
    public void 혜택미리보기는_잘못된_금액을_BadRequest로_반환한다()
    {
        var controller = new 공급자MembershipController(
            new 공급자Membership혜택계산Service(),
            CreateInterestService());
        var request = CreateRequest();
        request.MonthlyFeeAmount = -1m;

        var action = controller.혜택미리보기("farm-1", request);

        Assert.IsType<BadRequestObjectResult>(action);
    }

    [Fact]
    public async Task 관심구독초안은_현재_사용자_소유로_생성하고_과금을_요구하지_않는다()
    {
        var controller = WithUser(new 공급자MembershipController(
            new 공급자Membership혜택계산Service(),
            CreateInterestService()));

        var action = await controller.관심구독초안생성(
            "farm-1",
            new SupplierInterestSubscriptionDraftRequest
            {
                SupplierDisplayName = "충주 사과 농업경영체",
                SupplierPartyTypeCode =
                    SupplierRelationshipPartyTypeCodes.DomesticAgriculturalBusiness,
                AudienceTypeCode =
                    SupplierRelationshipAudienceTypeCodes.IndividualOrderer,
                InterestedProductTags = ["사과"],
                ReceiveSupplierUpdates = true,
                CurrentMemberConsentConfirmed = true,
                TermsVersion = "2026-07"
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(action);
        var response = Assert.IsType<SupplierInterestSubscriptionDraftResponse>(
            created.Value);
        Assert.Equal("buyer-1", response.OwnerUserId);
        Assert.Equal("farm-1", response.SupplierKey);
        Assert.False(response.PaymentRequired);
        Assert.False(response.MembershipActivated);
        Assert.False(response.SupplierContactDetailsDisclosed);
    }

    private static SupplierMembershipBenefitPreviewRequest CreateRequest()
        => new()
        {
            SupplierDisplayName = "충주 사과 농업경영체",
            SupplierPartyTypeCode =
                SupplierRelationshipPartyTypeCodes.DomesticAgriculturalBusiness,
            AudienceTypeCode =
                SupplierRelationshipAudienceTypeCodes.IndividualOrderer,
            MembershipStatusCode = SupplierMembershipStatusCodes.Active,
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

    private static 공급자관심구독Service CreateInterestService()
        => new(
            new InMemory공급자관심구독DraftStore(),
            new FixedTimeProvider());

    private static 공급자MembershipController WithUser(
        공급자MembershipController controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "buyer-1")
                ], "test"))
            }
        };
        return controller;
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(2026, 7, 26, 1, 0, 0, TimeSpan.Zero);
    }
}
