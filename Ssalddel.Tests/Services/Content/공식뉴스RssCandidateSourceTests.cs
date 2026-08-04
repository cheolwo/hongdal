using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Services.Content;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Content;

public sealed class 공식뉴스RssCandidateSourceTests
{
    [Fact]
    public async Task ReadAsync_공식기사메타데이터만_검토대기후보로반환한다()
    {
        var handler = new SequenceHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                <rss version="2.0">
                  <channel>
                    <item>
                      <title>수입 식품 안전 점검 결과</title>
                      <description><![CDATA[<p>공식 설명 자료입니다.</p>]]></description>
                      <link>http://www.mafra.go.kr/article/123</link>
                      <guid>article-123</guid>
                      <pubDate>Mon, 03 Aug 2026 09:00:00 +0900</pubDate>
                      <category>식품</category>
                    </item>
                    <item>
                      <title>허용하지 않는 외부 링크</title>
                      <link>https://example.com/article/999</link>
                    </item>
                  </channel>
                </rss>
                """)
        });
        var source = CreateSource(handler, 공식뉴스RssFeedCatalog.MafraPressReleases);

        var result = await source.ReadAsync(new CommunityInformationCollectionQuery { Take = 10 });

        var item = Assert.Single(result);
        Assert.Equal(CommunityInformationSourceKeys.MafraPressReleases, item.SourceKey);
        Assert.Equal(CommunityInformationSourceTypes.OfficialNews, item.SourceType);
        Assert.Equal(CommunityInformationReviewStates.PendingReview, item.ReviewState);
        Assert.Equal("농림축산식품부", item.Provider);
        Assert.Equal("공식 설명 자료입니다.", item.Summary);
        Assert.Equal("https://www.mafra.go.kr/article/123", item.OriginalUrl);
        Assert.Equal(new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc), item.PublishedAtUtc);
        Assert.Equal(공식뉴스RssFeedCatalog.MafraPressReleases.FeedUrl, item.SourceFeedUrl);
        Assert.Contains("수입·통관", item.TopicTags);
        Assert.Contains("식품안전", item.TopicTags);
        Assert.Null(item.NumericValue);
    }

    [Fact]
    public async Task ReadAsync_ETag와LastModified로_조건부요청하고_304에서는캐시를사용한다()
    {
        var calls = 0;
        var handler = new SequenceHandler(request =>
        {
            calls++;
            if (calls == 1)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        <rss><channel><item><title>회수 대상 식품 안내</title><link>https://www.mfds.go.kr/article/1</link><guid>1</guid></item></channel></rss>
                        """)
                };
                response.Headers.ETag = new EntityTagHeaderValue("\"feed-v1\"");
                response.Content.Headers.LastModified = new DateTimeOffset(2026, 8, 3, 0, 0, 0, TimeSpan.Zero);
                return response;
            }

            Assert.Contains(request.Headers.IfNoneMatch, value => value.Tag == "\"feed-v1\"");
            Assert.Equal(
                new DateTimeOffset(2026, 8, 3, 0, 0, 0, TimeSpan.Zero),
                request.Headers.IfModifiedSince);
            return new HttpResponseMessage(HttpStatusCode.NotModified);
        });
        var cache = new 공식뉴스RssConditionalCache();
        var source = CreateSource(
            handler,
            공식뉴스RssFeedCatalog.MfdsPressReleases,
            cache);

        var first = await source.ReadAsync(new CommunityInformationCollectionQuery());
        var second = await source.ReadAsync(new CommunityInformationCollectionQuery());

        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal(first[0].CandidateKey, second[0].CandidateKey);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task ReadAsync_비활성설정에서는_외부요청을하지않는다()
    {
        var handler = new SequenceHandler(_ => throw new InvalidOperationException("호출되면 안 됩니다."));
        var source = CreateSource(
            handler,
            공식뉴스RssFeedCatalog.MafraExplanations,
            enabled: false);

        var result = await source.ReadAsync(new CommunityInformationCollectionQuery());

        Assert.Empty(result);
        Assert.Equal(0, handler.CallCount);
    }

    private static 공식뉴스RssCandidateSource CreateSource(
        HttpMessageHandler handler,
        공식뉴스RssFeedDefinition feed,
        공식뉴스RssConditionalCache? cache = null,
        bool enabled = true)
    {
        var options = Options.Create(new OfficialNewsRssOptions
        {
            Enabled = enabled,
            MaxItemsPerFeed = 50,
            MaxResponseCharacters = 100_000
        });
        var client = new 공식뉴스RssClient(
            new HttpClient(handler),
            options,
            cache ?? new 공식뉴스RssConditionalCache());
        return new 공식뉴스RssCandidateSource(
            feed,
            client,
            options,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 4, 1, 2, 3, TimeSpan.Zero)));
    }

    private sealed class SequenceHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
