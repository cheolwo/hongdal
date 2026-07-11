using Hongdal.Contracts.Admin.Dispatch;
using Hongdal.Contracts.Food;
using Hongdal.Services.Food;
using 홍달.Services.Dispatch.Coordination;
using 홍달.Services.Dispatch.Recommendation;
using 홍달.Services.Storage.Local;

namespace Hongdal.Tests.Services.Dispatch.Coordination;

public sealed class FoodDeliveryDispatchAIReviewServiceTests
{
    [Fact]
    public async Task GetWorkspaceAsync는_중랑구_주요배달권과_인접배달권_판단_샘플을_제공한다()
    {
        var service = new FoodDeliveryDispatchAIReviewService(
            new EmptyFoodOrderStore(),
            new EchoFoodBundleService(),
            new EmptyDeliveryScopeStore(),
            new EmptyDriverLocationStore(),
            new FakeJudgmentLedgerStore());

        var workspace = await service.GetWorkspaceAsync();

        Assert.Equal("jungnang-scope-sample", workspace.Source);
        Assert.Equal("중랑구", workspace.PrimaryDeliveryScopeName);
        Assert.Contains("동대문구", workspace.AdjacentDeliveryScopeNames);
        Assert.Contains("광진구", workspace.AdjacentDeliveryScopeNames);
        Assert.Contains("노원구", workspace.AdjacentDeliveryScopeNames);
        Assert.Contains("구리시", workspace.AdjacentDeliveryScopeNames);
        Assert.Contains(workspace.Orders, x => x.DropoffScopeName == "구리시" && x.DropoffScopeRole == "인접 배달권");

        var bundle = Assert.Single(workspace.Bundles);
        Assert.Contains("1단계 AI", bundle.BundleDecisionSummary);
        Assert.Contains("2단계 AI", bundle.DriverAssignmentDecisionSummary);
    }

    [Fact]
    public async Task RecordDecisionAsync는_음식배달_운영자_판정을_화물과_구분된_RAG_사례로_저장한다()
    {
        var ledger = new FakeJudgmentLedgerStore();
        var service = new FoodDeliveryDispatchAIReviewService(
            new ThrowingFoodOrderStore(),
            new ThrowingFoodBundleService(),
            new ThrowingDeliveryScopeStore(),
            new EmptyDriverLocationStore(),
            ledger);

        var result = await service.RecordDecisionAsync(
            new FoodDeliveryDispatchAIReviewDecisionRequest
            {
                DecisionType = "수동묶음승인",
                OrderNos = ["FOOD-1", "FOOD-2"],
                DriverId = "f-driver-1",
                ManualBundle = true,
                Accepted = true,
                AdminNote = "조리 완료 시각과 고객 전달 권역이 맞아 음식배달 수동 묶음으로 확정합니다."
            },
            "admin-user");

        Assert.Equal("ADMIN-FOOD-CASE-1", result.CaseId);
        Assert.NotNull(ledger.LastRequest);
        Assert.Equal("음식 배달 OS", ledger.LastRequest!.RelatedOS);
        Assert.Equal("admin-food-delivery-ai-review", ledger.LastRequest.Source);
        Assert.Contains("음식배달OS", ledger.LastRequest.Keywords);
        Assert.Contains("음식점주문", ledger.LastRequest.Keywords);
        Assert.Contains("수동묶음", ledger.LastRequest.Keywords);
        Assert.Contains("FOOD-1, FOOD-2", ledger.LastRequest.SituationSummary);
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
                CaseId = "ADMIN-FOOD-CASE-1",
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

    private sealed class ThrowingFoodOrderStore : IHongdalFoodOrderStore
    {
        public 음식주문목록응답 GetOrders()
            => throw new NotSupportedException();

        public 음식주문응답? GetOrder(string orderNo)
            => throw new NotSupportedException();

        public 음식주문응답 AddOrder(음식주문등록요청 request)
            => throw new NotSupportedException();

        public 음식주문응답? 음식점수락(string orderNo, 음식점주문수락요청 request)
            => throw new NotSupportedException();

        public 음식주문응답? 배차대기반영(string orderNo, long dispatchWaitId, DateTime dispatchRequestedAtUtc)
            => throw new NotSupportedException();
    }

    private sealed class EmptyFoodOrderStore : IHongdalFoodOrderStore
    {
        public 음식주문목록응답 GetOrders()
            => new();

        public 음식주문응답? GetOrder(string orderNo)
            => null;

        public 음식주문응답 AddOrder(음식주문등록요청 request)
            => throw new NotSupportedException();

        public 음식주문응답? 음식점수락(string orderNo, 음식점주문수락요청 request)
            => throw new NotSupportedException();

        public 음식주문응답? 배차대기반영(string orderNo, long dispatchWaitId, DateTime dispatchRequestedAtUtc)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingFoodBundleService : I음식멀티배차조합Service
    {
        public IReadOnlyList<멀티배차조합후보> 조합생성(멀티배차조합요청 request)
            => throw new NotSupportedException();
    }

    private sealed class EchoFoodBundleService : I음식멀티배차조합Service
    {
        public IReadOnlyList<멀티배차조합후보> 조합생성(멀티배차조합요청 request)
        {
            var jobs = request.작업목록.Take(2).ToArray();
            return
            [
                new 멀티배차조합후보(
                    string.Join("+", jobs.Select(x => x.의뢰Id)),
                    "멀티배차",
                    jobs,
                    jobs.Select(x => x.의뢰Id).ToArray(),
                    0m,
                    2.4m,
                    4.8m,
                    91m,
                    ["중랑구기준", "인접배달권"],
                    [],
                    true,
                    [])
            ];
        }
    }

    private sealed class ThrowingDeliveryScopeStore : I배달권실행공간Store
    {
        public Task Upsert기사Async(
            string 배달권키,
            string 기사Id,
            IReadOnlyList<string> 인접배달권Keys,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Remove기사Async(string 기사Id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Upsert운송의뢰Async(
            string 배달권키,
            string 의뢰Id,
            IReadOnlyList<string> 인접배달권Keys,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Remove운송의뢰Async(string 의뢰Id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<배달권실행공간Snapshot?> GetAsync(string 배달권키, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<배달권실행공간Snapshot>> SnapshotAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class EmptyDeliveryScopeStore : I배달권실행공간Store
    {
        public Task Upsert기사Async(
            string 배달권키,
            string 기사Id,
            IReadOnlyList<string> 인접배달권Keys,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Remove기사Async(string 기사Id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Upsert운송의뢰Async(
            string 배달권키,
            string 의뢰Id,
            IReadOnlyList<string> 인접배달권Keys,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Remove운송의뢰Async(string 의뢰Id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<배달권실행공간Snapshot?> GetAsync(string 배달권키, CancellationToken cancellationToken = default)
            => Task.FromResult<배달권실행공간Snapshot?>(null);

        public Task<IReadOnlyList<배달권실행공간Snapshot>> SnapshotAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<배달권실행공간Snapshot>>([]);
    }

    private sealed class EmptyDriverLocationStore : IDriverLocationStore
    {
        public void Upsert(DriverLocationSnapshot snapshot)
        {
        }

        public bool TryGetLatest(string driverId, out DriverLocationSnapshot snapshot)
        {
            snapshot = null!;
            return false;
        }
    }
}
