using Ssalddel.Contracts.Common.Identifiers;

namespace Ssalddel.Tests.Contracts.Common.Identifiers;

public sealed class SsalddelIdentifierCodePayloadsTests
{
    [Fact]
    public void CreateBuildsOrderPayloadWithStablePrefix()
    {
        var payload = SsalddelIdentifierCodePayloads.Create(SsalddelIdentifierKindCode.Order, " ORD-1 ");

        Assert.Equal(SsalddelIdentifierKindCode.Order, payload.Kind);
        Assert.Equal("ORD-1", payload.Value);
        Assert.Equal("ORD:ORD-1", payload.RawCode);
        Assert.Equal("주문 번호", payload.DisplayName);
    }

    [Theory]
    [InlineData("LED:20260710-0007", SsalddelIdentifierKindCode.Ledger, "20260710-0007")]
    [InlineData("OUT:20260710-0012", SsalddelIdentifierKindCode.OutboundPlan, "20260710-0012")]
    [InlineData("INB:20260710-0104", SsalddelIdentifierKindCode.InboundRequest, "20260710-0104")]
    [InlineData("LOC:A-01-02", SsalddelIdentifierKindCode.StorageLocation, "A-01-02")]
    [InlineData("HD:TRQ:HD-WEB-001", SsalddelIdentifierKindCode.TransportRequest, "HD-WEB-001")]
    public void ParseResolvesKnownPrefixes(string rawCode, string expectedKind, string expectedValue)
    {
        var payload = SsalddelIdentifierCodePayloads.Parse(rawCode);

        Assert.Equal(expectedKind, payload.Kind);
        Assert.Equal(expectedValue, payload.Value);
        Assert.Equal(rawCode, payload.RawCode);
    }
}
