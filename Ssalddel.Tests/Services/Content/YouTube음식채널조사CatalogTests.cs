using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class YouTube음식채널조사CatalogTests
{
    [Fact]
    public void 운영시작카탈로그는_검증된고유채널과분류를가진다()
    {
        var channels = YouTube음식채널조사Catalog.항목;

        Assert.True(channels.Count >= 25);
        Assert.Equal(channels.Count, channels.Select(item => item.ChannelId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(channels.Count, channels.Select(item => item.Handle).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(channels, channel =>
        {
            Assert.StartsWith("UC", channel.ChannelId, StringComparison.Ordinal);
            Assert.StartsWith("@", channel.Handle, StringComparison.Ordinal);
            Assert.StartsWith("https://www.youtube.com/@", channel.공식채널Url, StringComparison.Ordinal);
            Assert.Equal(2, channel.국가코드.Length);
            Assert.InRange(channel.구매발견점수, 0, 100);
            Assert.InRange(channel.수입발견점수, 0, 100);
            Assert.NotEmpty(channel.분류코드목록);
            Assert.All(channel.분류코드목록, category =>
                Assert.Contains(category, YouTube음식채널분류코드.전체));
            Assert.Equal(DateTimeKind.Utc, channel.조사확인일시Utc.Kind);
        });

        Assert.Contains(channels, channel => channel.국가코드 == YouTube채널수집국가코드.한국);
        Assert.Contains(channels, channel => channel.국가코드 == YouTube채널수집국가코드.미국);
    }

    [Fact]
    public void 일본지역음식조사는_독립여행채널과_영어권채널을포함한다()
    {
        var japanChannels = YouTube음식채널조사Catalog.항목
            .Where(item => item.국가코드 == "JP")
            .ToArray();

        Assert.True(japanChannels.Length >= 6);
        Assert.Contains(
            japanChannels,
            item => item.ChannelId == "UCL_ZYXcb07HM4wobK58PcEA"
                    && item.Handle == "@shiyago");
        Assert.Contains(
            japanChannels,
            item => item.ChannelId == "UCXxYVUTiVq-_RDNt5VPGGug"
                    && item.Handle == "@marutto_tabi");
        Assert.Contains(
            japanChannels,
            item => item.ChannelId == "UCYuVuLCtgrkQ6znA_ShdvXA"
                    && item.Handle == "@boutacchan");
        Assert.Contains(
            japanChannels,
            item => item.ChannelId == "UCfNVLQ0xYJyjHVEdlOyuHcQ"
                    && item.Handle == "@morrytravel");
        Assert.Contains(
            japanChannels,
            item => item.ChannelId == "UCHL9bfHTxCMi-7vfxQ-AYtg"
                    && item.기본언어코드 == "en");
        Assert.True(japanChannels.Count(item => item.분류코드목록.Contains(
            YouTube음식채널분류코드.음식여행)) >= 10);
    }

    [Theory]
    [InlineData("KR", 20)]
    [InlineData("US", 20)]
    [InlineData("JP", 18)]
    public void 한미일전수조사는_국가별분류축을고르게포함한다(
        string countryCode,
        int minimumChannelCount)
    {
        var channels = YouTube음식채널조사Catalog.항목
            .Where(item => item.국가코드 == countryCode)
            .ToArray();
        var categories = channels
            .SelectMany(item => item.분류코드목록)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(channels.Length >= minimumChannelCount);
        Assert.Contains(YouTube음식채널분류코드.상품리뷰, categories);
        Assert.Contains(YouTube음식채널분류코드.요리재료, categories);
        Assert.Contains(YouTube음식채널분류코드.음식여행, categories);
        Assert.Contains(YouTube음식채널분류코드.육류수산, categories);
        Assert.Contains(YouTube음식채널분류코드.식품산업, categories);
        Assert.Contains(
            channels,
            item => item.조사확인일시Utc == new DateTime(
                2026,
                7,
                24,
                0,
                0,
                0,
                DateTimeKind.Utc));
    }
}
