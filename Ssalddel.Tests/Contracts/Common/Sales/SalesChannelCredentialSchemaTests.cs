using Ssalddel.Contracts.Common.Sales;

namespace Ssalddel.Tests.Contracts.Common.Sales;

public sealed class SalesChannelCredentialSchemaTests
{
    [Fact]
    public void 지원채널은_각자필수인증필드와비밀값을공유계약으로제공한다()
    {
        Assert.Equal(
            new[]
            {
                CommerceChannelKeys.SmartStore,
                CommerceChannelKeys.Coupang,
                CommerceChannelKeys.Shopify,
                CommerceChannelKeys.Amazon
            },
            판매채널인증SchemaCatalog.Items.Select(item => item.채널종류));

        Assert.All(
            판매채널인증SchemaCatalog.Items,
            schema =>
            {
                Assert.Contains(schema.Fields, field => field.필수);
                Assert.Contains(schema.Fields, field => field.비밀값);
                Assert.Equal(
                    schema.Fields.Count,
                    schema.Fields.Select(field => field.Key)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count());
            });
    }

    [Fact]
    public void 계정응답은_인증원문Dictionary를노출하지않는다()
    {
        var responseProperties = typeof(판매채널계정항목응답).GetProperties();

        Assert.DoesNotContain(
            responseProperties,
            property => property.Name == nameof(판매채널계정저장요청.인증정보));
        Assert.Contains(
            responseProperties,
            property => property.Name == nameof(판매채널계정항목응답.인증필드상태));
    }
}
