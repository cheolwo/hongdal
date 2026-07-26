using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공급자관심구독ServiceTests
{
    [Fact]
    public async Task 개인_관심구독은_결제없이_사용자_소유_초안으로_저장된다()
    {
        var service = CreateService();

        var draft = await service.초안생성Async(
            "buyer-1",
            CreateRequest());

        Assert.Equal(SupplierMembershipStatusCodes.InterestFollowing, draft.StatusCode);
        Assert.False(draft.PaymentRequired);
        Assert.False(draft.MembershipActivated);
        Assert.False(draft.SupplierContactDetailsDisclosed);
        Assert.Equal(["사과", "배"], draft.InterestedProductTags);
        Assert.NotNull(await service.초안조회Async("buyer-1", draft.DraftId));
        Assert.Null(await service.초안조회Async("buyer-2", draft.DraftId));
    }

    [Fact]
    public async Task 배송권_집단은_현재_구성원_동의와_배송권Key를_요구한다()
    {
        var service = CreateService();
        var request = CreateRequest();
        request.AudienceTypeCode =
            SupplierRelationshipAudienceTypeCodes.DeliveryScopeGroup;
        request.DeliveryScopeKey = null;

        var missingScope = await Assert.ThrowsAsync<ArgumentException>(
            () => service.초안생성Async("buyer-1", request));
        Assert.Contains("배송권 Key", missingScope.Message);

        request.DeliveryScopeKey = "kr-admin2:서울특별시-양천구";
        request.CurrentMemberConsentConfirmed = false;
        var missingConsent = await Assert.ThrowsAsync<ArgumentException>(
            () => service.초안생성Async("buyer-1", request));
        Assert.Contains("현재 사용자의", missingConsent.Message);
    }

    [Fact]
    public async Task 해외제조업체도_같은_무료_관심구독_상태를_사용한다()
    {
        var service = CreateService();
        var request = CreateRequest();
        request.SupplierPartyTypeCode =
            SupplierRelationshipPartyTypeCodes.OverseasFoodManufacturer;
        request.SupplierKey = "us-manufacturer-1";

        var draft = await service.초안생성Async("buyer-1", request);

        Assert.Equal(
            SupplierRelationshipPartyTypeCodes.OverseasFoodManufacturer,
            draft.SupplierPartyTypeCode);
        Assert.False(draft.PaymentRequired);
    }

    private static 공급자관심구독Service CreateService()
        => new(
            new InMemory공급자관심구독DraftStore(),
            new FixedTimeProvider());

    private static SupplierInterestSubscriptionDraftRequest CreateRequest()
        => new()
        {
            SupplierKey = "farm-apple-01",
            SupplierDisplayName = "충주 사과 농업경영체",
            SupplierPartyTypeCode =
                SupplierRelationshipPartyTypeCodes.DomesticAgriculturalBusiness,
            AudienceTypeCode =
                SupplierRelationshipAudienceTypeCodes.IndividualOrderer,
            InterestedProductTags = ["사과", "배", "사과", " "],
            ReceiveSupplierUpdates = true,
            CurrentMemberConsentConfirmed = true,
            TermsVersion = "2026-07"
        };

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(2026, 7, 26, 1, 0, 0, TimeSpan.Zero);
    }
}
