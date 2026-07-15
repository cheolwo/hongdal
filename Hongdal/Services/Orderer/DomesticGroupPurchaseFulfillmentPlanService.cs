using System.Collections.Concurrent;
using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Services.Orderer;

public interface IDomesticGroupPurchaseFulfillmentOrderDraftStore
{
    Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse> SaveAsync(
        DomesticGroupPurchaseFulfillmentOrderDraftResponse draft,
        CancellationToken cancellationToken = default);

    Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse?> GetAsync(
        Guid draftId,
        CancellationToken cancellationToken = default);
}

public sealed class InMemoryDomesticGroupPurchaseFulfillmentOrderDraftStore
    : IDomesticGroupPurchaseFulfillmentOrderDraftStore
{
    private readonly ConcurrentDictionary<Guid, DomesticGroupPurchaseFulfillmentOrderDraftResponse> drafts = new();

    public Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse> SaveAsync(
        DomesticGroupPurchaseFulfillmentOrderDraftResponse draft,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        drafts[draft.DraftId] = draft;
        return Task.FromResult(draft);
    }

    public Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse?> GetAsync(
        Guid draftId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        drafts.TryGetValue(draftId, out var draft);
        return Task.FromResult(draft);
    }
}

public interface IDomesticGroupPurchaseFulfillmentPlanService
{
    DomesticGroupPurchaseFulfillmentPlanResponse Preview(
        DomesticGroupPurchaseFulfillmentPlanRequest request);

    Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse> CreateOrderDraftAsync(
        string createdByUserId,
        DomesticGroupPurchaseFulfillmentPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse?> GetOrderDraftAsync(
        string createdByUserId,
        Guid draftId,
        CancellationToken cancellationToken = default);
}

public sealed class DomesticGroupPurchaseFulfillmentPlanService
    : IDomesticGroupPurchaseFulfillmentPlanService
{
    private readonly IDomesticGroupPurchaseFulfillmentOrderDraftStore draftStore;

    public DomesticGroupPurchaseFulfillmentPlanService(
        IDomesticGroupPurchaseFulfillmentOrderDraftStore draftStore)
    {
        this.draftStore = draftStore;
    }

    public DomesticGroupPurchaseFulfillmentPlanResponse Preview(
        DomesticGroupPurchaseFulfillmentPlanRequest request)
        => DomesticGroupPurchaseFulfillmentPlanBuilder.Preview(request);

    public async Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse> CreateOrderDraftAsync(
        string createdByUserId,
        DomesticGroupPurchaseFulfillmentPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(createdByUserId);
        var plan = Preview(request);
        if (!plan.OrderPlacementReady)
        {
            throw new ArgumentException(string.Join(" ", plan.PlanningWarnings), nameof(request));
        }

        var draft = new DomesticGroupPurchaseFulfillmentOrderDraftResponse
        {
            DraftId = Guid.NewGuid(),
            CreatedByUserId = createdByUserId.Trim(),
            StatusCode = DomesticGroupPurchaseFulfillmentDraftStatuses.Draft,
            IsDurablyPersisted = false,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Plan = plan,
            GuidanceMessage = "발주와 원장 생성 계획을 서버 메모리 초안으로 보관했습니다. 확정 단계에서는 주문 원장을 먼저 만든 뒤 계획된 판매·창고·운송 원장을 포함 관계로 연결해야 합니다."
        };

        return await draftStore.SaveAsync(draft, cancellationToken);
    }

    public async Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse?> GetOrderDraftAsync(
        string createdByUserId,
        Guid draftId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(createdByUserId);
        if (draftId == Guid.Empty)
        {
            return null;
        }

        var draft = await draftStore.GetAsync(draftId, cancellationToken);
        return draft is not null
               && string.Equals(draft.CreatedByUserId, createdByUserId.Trim(), StringComparison.Ordinal)
            ? draft
            : null;
    }
}
