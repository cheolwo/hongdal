using Hongdal.Contracts.Admin.Dispatch;
using 홍달.Services.Dispatch.Coordination;

namespace Hongdal.Tests.Services.Dispatch.Coordination;

public sealed class DomesticCargoDispatchAIReviewServiceTests
{
    [Fact]
    public async Task RecordDecisionAsync는_운영자_수동묶음_판정을_RAG_사례로_저장한다()
    {
        var ledger = new FakeJudgmentLedgerStore();
        var service = new DomesticCargoDispatchAIReviewService(
            new ThrowingInputFactory(),
            new ThrowingCoordinationService(),
            ledger);

        var result = await service.RecordDecisionAsync(
            new DomesticCargoDispatchAIReviewDecisionRequest
            {
                DecisionType = "수동묶음승인",
                RequestIds = ["REQ-1", "REQ-2"],
                DriverId = "driver-1",
                ManualBundle = true,
                Accepted = true,
                AdminNote = "지도상 기사 위치와 상차지 근접성이 좋아 수동 묶음으로 확정합니다."
            },
            "admin-user");

        Assert.Equal("ADMIN-CASE-1", result.CaseId);
        Assert.NotNull(ledger.LastRequest);
        Assert.Contains("수동 묶음", ledger.LastRequest!.Title);
        Assert.Contains("REQ-1, REQ-2", ledger.LastRequest.SituationSummary);
        Assert.Contains("수동묶음", ledger.LastRequest.Keywords);
        Assert.Equal("admin-user", ledger.LastCreatedBy);
    }

    private sealed class FakeJudgmentLedgerStore : I배차AI판단사례LedgerStore
    {
        public DispatchAIJudgmentCaseCreateRequest? LastRequest { get; private set; }

        public string? LastCreatedBy { get; private set; }

        public Task<DispatchAIJudgmentCaseCatalogDto> GetCatalogAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new DispatchAIJudgmentCaseCatalogDto());

        public Task<DispatchAIJudgmentCaseDto> CreateAsync(
            DispatchAIJudgmentCaseCreateRequest request,
            string? createdBy,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            LastCreatedBy = createdBy;
            return Task.FromResult(new DispatchAIJudgmentCaseDto
            {
                CaseId = "ADMIN-CASE-1",
                Title = request.Title
            });
        }

        public Task<DispatchAIJudgmentCaseDto> PromoteSuggestionAsync(
            string suggestionKey,
            DispatchAIJudgmentCasePromoteSuggestionRequest request,
            string? createdBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingInputFactory : I국내화물배차조율입력Factory
    {
        public Task<국내화물배차조율입력> 생성Async(
            국내화물배차조율입력요청 request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingCoordinationService : I국내화물배차조율Service
    {
        public 국내화물배차조율결과 조율(국내화물배차조율입력 input)
            => throw new NotSupportedException();
    }
}
