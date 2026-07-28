using Ssalddel.Contracts.Common.WarehouseBilling;

namespace Ssalddel.Tests.Contracts.Common.WarehouseBilling;

public sealed class 물류대행계약계획기Tests
{
    [Fact]
    public void Plan_AssignsAnyAuthenticatedRequesterAsContractSpecificShipper()
    {
        var preview = 물류대행계약계획기.Plan(
            "community-user-1",
            "동네 공동주문 운영자",
            Request(),
            new DateTimeOffset(2026, 7, 28, 1, 2, 3, TimeSpan.Zero));

        var shipper = Assert.Single(
            preview.ContractDraft.Parties,
            party => party.RoleCode == 물류대행계약당사자역할코드.화주);
        Assert.Equal("community-user-1", shipper.PartyId);
        Assert.Equal(
            물류대행계약당사자자격출처코드.계약별지정,
            shipper.QualificationSourceCode);
        Assert.Equal(
            물류대행계약당사자유형코드.공동행동집단,
            shipper.PartyTypeCode);
        Assert.False(preview.ContractDraft.IsBinding);
        Assert.False(preview.ContractDraft.CanActivate);
        Assert.Contains("전자서명", preview.ContractDraft.ActivationRequirement);
    }

    [Fact]
    public void Plan_SnapshotsRateVersionAndEvidenceBasis()
    {
        var request = Request();
        var first = 물류대행계약계획기.Plan(
            "seller-1",
            "판매자",
            request,
            new DateTimeOffset(2026, 7, 28, 1, 2, 3, TimeSpan.Zero));
        var second = 물류대행계약계획기.Plan(
            "seller-1",
            "판매자",
            request,
            new DateTimeOffset(2026, 7, 29, 1, 2, 3, TimeSpan.Zero));

        Assert.Equal(
            first.ContractDraft.RateSnapshot.RateVersion,
            second.ContractDraft.RateSnapshot.RateVersion);
        Assert.StartsWith("DRAFT-", first.ContractDraft.RateSnapshot.RateVersion);
        Assert.All(
            first.EstimatedBilling.Lines,
            line => Assert.False(string.IsNullOrWhiteSpace(line.EvidenceTypeCode)));
        Assert.Contains(
            first.ContractDraft.ServiceScopes,
            scope => scope.ServiceStageCode == 물류대행서비스단계코드.검수);
        Assert.Contains(
            first.ContractDraft.ServiceScopes,
            scope => scope.ServiceStageCode == 물류대행서비스단계코드.보관);
    }

    [Fact]
    public void Plan_DoesNotTrustARequesterUserIdFromBody()
    {
        Assert.DoesNotContain(
            typeof(물류대행비용미리보기요청).GetProperties(),
            property => property.Name.Contains("UserId", StringComparison.OrdinalIgnoreCase));
    }

    private static 물류대행비용미리보기요청 Request()
    {
        var rates = WarehouseBillingRateCatalog.CreateDefaultRates()
            .Where(rate => rate.ChargeCode is
                WarehouseBillingChargeCode.InboundInspection or
                WarehouseBillingChargeCode.StorageDaily)
            .ToArray();

        return new 물류대행비용미리보기요청
        {
            LogisticsProviderId = "3pl-1",
            LogisticsProviderDisplayName = "서울 물류센터",
            RequesterPartyTypeCode = 물류대행계약당사자유형코드.공동행동집단,
            ServicePeriodStart = new DateOnly(2026, 7, 1),
            ServicePeriodEnd = new DateOnly(2026, 7, 3),
            Rates = rates,
            Usages =
            [
                new(WarehouseBillingChargeCode.InboundInspection, 10m),
                new(
                    WarehouseBillingChargeCode.StorageDaily,
                    2m,
                    new DateOnly(2026, 7, 1),
                    new DateOnly(2026, 7, 3))
            ]
        };
    }
}
