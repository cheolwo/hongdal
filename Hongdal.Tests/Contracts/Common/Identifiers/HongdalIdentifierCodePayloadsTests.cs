using Hongdal.Contracts.Common.Identifiers;

namespace Hongdal.Tests.Contracts.Common.Identifiers;

public sealed class HongdalIdentifierCodePayloadsTests
{
    [Fact]
    public void CreateBuildsOrderPayloadWithStablePrefix()
    {
        var payload = HongdalIdentifierCodePayloads.Create(HongdalIdentifierKindCode.Order, " ORD-1 ");

        Assert.Equal(HongdalIdentifierKindCode.Order, payload.Kind);
        Assert.Equal("ORD-1", payload.Value);
        Assert.Equal("ORD:ORD-1", payload.RawCode);
        Assert.Equal("주문 번호", payload.DisplayName);
    }

    [Theory]
    [InlineData("LED:20260710-0007", HongdalIdentifierKindCode.Ledger, "20260710-0007")]
    [InlineData("OUT:20260710-0012", HongdalIdentifierKindCode.OutboundPlan, "20260710-0012")]
    [InlineData("INB:20260710-0104", HongdalIdentifierKindCode.InboundRequest, "20260710-0104")]
    [InlineData("LOC:A-01-02", HongdalIdentifierKindCode.StorageLocation, "A-01-02")]
    [InlineData("HD:TRQ:HD-WEB-001", HongdalIdentifierKindCode.TransportRequest, "HD-WEB-001")]
    public void ParseResolvesKnownPrefixes(string rawCode, string expectedKind, string expectedValue)
    {
        var payload = HongdalIdentifierCodePayloads.Parse(rawCode);

        Assert.Equal(expectedKind, payload.Kind);
        Assert.Equal(expectedValue, payload.Value);
        Assert.Equal(rawCode, payload.RawCode);
    }
}
