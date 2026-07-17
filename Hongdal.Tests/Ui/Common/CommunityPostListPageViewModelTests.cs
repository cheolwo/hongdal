using System.Net;
using System.Net.Http.Json;
using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace Hongdal.Tests.Ui.Common;

public sealed class CommunityPostListPageViewModelTests
{
    [Fact]
    public async Task 목록상태는_게시판_추천_검색조건을_한곳에서투영한다()
    {
        var response = new PlatformCommunityPostListResponse
        {
            Items =
            [
                new PlatformCommunityPostResponse
                {
                    Id = 1,
                    Category = "생활 원장",
                    Title = "공동 주문을 가볍게 시작합니다",
                    Body = "아파트 구성원이 함께 주문합니다.",
                    Nickname = "동네 사용자",
                    RecommendationCount = 2,
                    CreatedAtUtc = new DateTime(2026, 7, 16, 1, 0, 0, DateTimeKind.Utc)
                },
                new PlatformCommunityPostResponse
                {
                    Id = 2,
                    Category = "업무 질문",
                    Title = "수입 통관 역할을 찾습니다",
                    Body = "관세사 참여가 필요합니다.",
                    Nickname = "수입 준비자",
                    IsOperatorPinned = true,
                    RecommendationCount = 9,
                    CreatedAtUtc = new DateTime(2026, 7, 17, 1, 0, 0, DateTimeKind.Utc)
                }
            ],
            TotalCount = 2
        };
        using var httpClient = new HttpClient(new JsonResponseHandler(response))
        {
            BaseAddress = new Uri("https://localhost/")
        };
        var service = new PlatformCommunityService(httpClient, null!);
        using var viewModel = new CommunityPostListPageViewModel(service);
        viewModel.Configure("platform");

        Assert.True(await viewModel.초기화Async());
        Assert.Equal([2L, 1L], viewModel.VisibleItems.Select(item => item.Id));

        viewModel.SelectedBoard = "업무 질문";
        Assert.Equal(2, Assert.Single(viewModel.VisibleItems).Id);

        viewModel.SelectedBoard = "전체";
        viewModel.SelectedListFilter = "추천글";
        Assert.Equal(2, Assert.Single(viewModel.VisibleItems).Id);

        viewModel.SelectedListFilter = "전체글";
        viewModel.SelectedPostId = 2;
        viewModel.SearchText = "아파트";
        Assert.Null(viewModel.SelectedPostId);
        Assert.Equal(1, Assert.Single(viewModel.VisibleItems).Id);
    }

    private sealed class JsonResponseHandler(PlatformCommunityPostListResponse response)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(response)
            });
    }
}
