using Hongdal.Contracts.Common.ContractManagement;

namespace Hongdal.Tests.Contracts.Common.ContractManagement;

public sealed class ContractElectronicSignaturePlannerTests
{
    [Fact]
    public void Plan_AllRequiredPartiesSigned_IsFullySigned()
    {
        var now = new DateTimeOffset(2026, 7, 6, 10, 0, 0, TimeSpan.Zero);
        var bundle = CreateSignedBundle(now);

        var plan = ContractElectronicSignaturePlanner.Plan(bundle, now.AddMinutes(1));

        Assert.True(plan.IsFullySigned);
        Assert.Equal(ContractSignatureStatusCode.Signed, plan.StatusCode);
        Assert.Empty(plan.MissingRequiredPartyIds);
        Assert.Empty(plan.InvalidEvidencePartyIds);
        Assert.Equal(2, plan.SignedRequiredSignerCount);
    }

    [Fact]
    public void Plan_OneRequiredPartyMissing_IsPartiallySigned()
    {
        var now = new DateTimeOffset(2026, 7, 6, 10, 0, 0, TimeSpan.Zero);
        var bundle = CreateBundle(now);
        bundle = ContractElectronicSignaturePlanner.AddEvidence(
            bundle,
            CreateEvidence("orderer-1", "개설 신청 주문자", now));

        var plan = ContractElectronicSignaturePlanner.Plan(bundle, now.AddMinutes(1));

        Assert.False(plan.IsFullySigned);
        Assert.Equal(ContractSignatureStatusCode.PartiallySigned, plan.StatusCode);
        Assert.Contains("shipper-1", plan.MissingRequiredPartyIds);
    }

    [Fact]
    public void Plan_EvidenceForDifferentDocument_IsInvalidEvidence()
    {
        var now = new DateTimeOffset(2026, 7, 6, 10, 0, 0, TimeSpan.Zero);
        var bundle = ContractElectronicSignaturePlanner.AddEvidence(
            CreateBundle(now),
            CreateEvidence("orderer-1", "개설 신청 주문자", now) with
            {
                SignedDocumentHash = "sha256:different-document"
            });

        var plan = ContractElectronicSignaturePlanner.Plan(bundle, now.AddMinutes(1));

        Assert.False(plan.IsFullySigned);
        Assert.Equal(ContractSignatureStatusCode.InvalidEvidence, plan.StatusCode);
        Assert.Contains("orderer-1", plan.InvalidEvidencePartyIds);
    }

    [Fact]
    public void Plan_ExpiredBundle_IsExpiredEvenWhenEvidenceExists()
    {
        var now = new DateTimeOffset(2026, 7, 6, 10, 0, 0, TimeSpan.Zero);
        var bundle = CreateSignedBundle(now) with
        {
            ExpiresAtUtc = now.AddMinutes(5)
        };

        var plan = ContractElectronicSignaturePlanner.Plan(bundle, now.AddMinutes(10));

        Assert.False(plan.IsFullySigned);
        Assert.Equal(ContractSignatureStatusCode.Expired, plan.StatusCode);
    }

    private static ContractElectronicSignatureBundle CreateSignedBundle(DateTimeOffset now)
    {
        var bundle = CreateBundle(now);
        bundle = ContractElectronicSignaturePlanner.AddEvidence(
            bundle,
            CreateEvidence("orderer-1", "개설 신청 주문자", now));
        bundle = ContractElectronicSignaturePlanner.AddEvidence(
            bundle,
            CreateEvidence("shipper-1", "공급 화주", now.AddMinutes(1)));
        return bundle;
    }

    private static ContractElectronicSignatureBundle CreateBundle(DateTimeOffset now)
        => ContractElectronicSignaturePlanner.CreateBundle(
            "IFGP-2026-0001",
            "sha256:contract-document",
            [
                new("orderer-1", 수입식품공동주문계약역할코드.ApplicantOrderer, "개설 신청 주문자", true, now),
                new("shipper-1", 수입식품공동주문계약역할코드.SupplierOrShipper, "공급 화주", true, now)
            ],
            now,
            now.AddDays(7));

    private static ContractSignatureEvidence CreateEvidence(
        string partyId,
        string signerDisplayName,
        DateTimeOffset signedAtUtc)
        => new(
            partyId,
            signerDisplayName,
            ContractSignatureMethodCode.PlatformClickSign,
            "sha256:contract-document",
            "sha256:consent-text",
            $"sha256:evidence-{partyId}",
            signedAtUtc,
            $"sha256:ip-{partyId}");
}
