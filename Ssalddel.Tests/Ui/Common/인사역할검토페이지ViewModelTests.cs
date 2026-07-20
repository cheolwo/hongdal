using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 인사역할검토페이지ViewModelTests
{
    [Fact]
    public async Task 목록은_검색상태참여자범위와페이지를서버요청에전달한다()
    {
        var service = new FakeRoleReviewService
        {
            ListResponse = new HrRoleReviewListResponse
            {
                TotalCount = 17,
                Page = 2,
                PageSize = 15,
                Items = [new HrRoleReviewSummaryResponse { ReviewId = Guid.NewGuid() }]
            }
        };
        var viewModel = new 인사역할검토목록ViewModel(service)
        {
            검색어 = "입고",
            원장유형 = HrRoleReviewSourceCodes.RoleAssignment,
            상태코드 = HrRoleReviewStatusCodes.Assigned,
            참여자유형 = HrParticipantCategoryCodes.InternalProjectOperator,
            범위유형 = HrScopeTypes.Warehouse
        };

        Assert.True(await viewModel.조회Async());
        Assert.True(await viewModel.페이지조회Async(2));

        Assert.Equal("입고", service.LastRequest!.Search);
        Assert.Equal(HrRoleReviewSourceCodes.RoleAssignment, service.LastRequest.SourceCode);
        Assert.Equal(HrRoleReviewStatusCodes.Assigned, service.LastRequest.StatusCode);
        Assert.Equal(HrParticipantCategoryCodes.InternalProjectOperator, service.LastRequest.ParticipantCategory);
        Assert.Equal(HrScopeTypes.Warehouse, service.LastRequest.ScopeType);
        Assert.Equal(2, service.LastRequest.Page);
        Assert.Equal(17, viewModel.전체건수);
        Assert.Equal(2, viewModel.총페이지수);
    }

    [Fact]
    public async Task 정확한상세가없어도첫배정이나다른사용자로대체하지않는다()
    {
        var reviewId = Guid.NewGuid();
        var viewModel = new 인사역할검토상세ViewModel(
            new FakeRoleReviewService { DetailResponse = null });

        var succeeded = await viewModel.조회Async(reviewId);

        Assert.True(succeeded);
        Assert.Equal(reviewId, viewModel.요청ReviewId);
        Assert.True(viewModel.찾을수없음);
        Assert.Null(viewModel.상세);
    }

    [Fact]
    public async Task PageViewModel은_목록후명시한ReviewId상세만조회한다()
    {
        var reviewId = Guid.NewGuid();
        var service = new FakeRoleReviewService
        {
            DetailResponse = new HrRoleReviewDetailResponse { ReviewId = reviewId }
        };
        var page = new 인사역할검토PageViewModel(
            new 인사역할검토목록ViewModel(service),
            new 인사역할검토상세ViewModel(service));

        var succeeded = await page.초기화Async(reviewId);

        Assert.True(succeeded);
        Assert.Equal(reviewId, service.LastReviewId);
        Assert.Equal(reviewId, page.상세.상세?.ReviewId);
    }

    private sealed class FakeRoleReviewService : I인사역할검토읽기Service
    {
        public HrRoleReviewListResponse ListResponse { get; init; } = new();
        public HrRoleReviewDetailResponse? DetailResponse { get; init; }
        public HrRoleReviewListRequest? LastRequest { get; private set; }
        public Guid? LastReviewId { get; private set; }

        public Task<HrRoleReviewListResponse> 목록Async(
            HrRoleReviewListRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(ListResponse);
        }

        public Task<HrRoleReviewDetailResponse?> 상세Async(
            Guid reviewId,
            CancellationToken cancellationToken = default)
        {
            LastReviewId = reviewId;
            return Task.FromResult(DetailResponse);
        }
    }
}
