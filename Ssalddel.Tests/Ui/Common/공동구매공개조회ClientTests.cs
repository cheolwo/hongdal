using System.Net;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 공동구매공개조회ClientTests
{
    [Fact]
    public async Task 목록조회는_인증client없이_0점0공개투표API를사용한다()
    {
        var domestic = Campaign(1, CommunityVoteKindCodes.GroupPurchaseDemand);
        var groupImport = Campaign(2, CommunityVoteKindCodes.GroupPurchaseDemand);
        groupImport.GroupPurchase!.TradeRouteCode = CommunityGroupPurchaseTradeRouteCodes.InboundGroupImportCandidate;
        var general = Campaign(3, CommunityVoteKindCodes.General);
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CommunityVoteListResponse
            {
                Items = [domestic, groupImport, general]
            })
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost/")
        };
        var client = new PlatformCommunityService(httpClient, null!);

        var result = await client.GetPublicGroupPurchaseVotesAsync("서울 동부", "0201");

        Assert.Equal([domestic.Id, groupImport.Id], result.Items.Select(item => item.Id));
        Assert.Equal(
            "api/v1/community/votes?appKey=OrdererApp&communityScope=%EC%84%9C%EC%9A%B8%20%EB%8F%99%EB%B6%80&hsCode=0201",
            handler.RequestPath);
    }

    [Fact]
    public async Task 상세조회404는_오류대신_대상없음을반환한다()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost/")
        };
        var client = new PlatformCommunityService(httpClient, null!);
        var campaignId = Guid.NewGuid();

        var result = await client.GetPublicGroupPurchaseVoteAsync(campaignId);

        Assert.Null(result);
        Assert.Equal($"api/v1/community/votes/{campaignId:D}", handler.RequestPath);
    }

    [Fact]
    public async Task 상세조회가_일반투표이면_공동구매상세로노출하지않는다()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Campaign(4, CommunityVoteKindCodes.General))
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost/")
        };
        var client = new PlatformCommunityService(httpClient, null!);

        var result = await client.GetPublicGroupPurchaseVoteAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    private static CommunityVoteResponse Campaign(int seed, string voteKind)
        => new()
        {
            Id = Guid.Parse($"00000000-0000-0000-0000-{seed:D12}"),
            VoteKind = voteKind,
            Title = $"공동구매 {seed}",
            GroupPurchase = new CommunityGroupPurchaseVoteResponse
            {
                TradeRouteCode = CommunityGroupPurchaseTradeRouteCodes.Domestic
            }
        };

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public string RequestPath { get; private set; } = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestPath = request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty;
            return Task.FromResult(responseFactory(request));
        }
    }
}
