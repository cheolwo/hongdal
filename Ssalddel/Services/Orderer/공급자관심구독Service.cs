using System.Collections.Concurrent;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

public interface I공급자관심구독DraftStore
{
    Task<SupplierInterestSubscriptionDraftResponse> 저장Async(
        SupplierInterestSubscriptionDraftResponse draft,
        CancellationToken cancellationToken = default);

    Task<SupplierInterestSubscriptionDraftResponse?> 조회Async(
        Guid draftId,
        CancellationToken cancellationToken = default);
}

public sealed class InMemory공급자관심구독DraftStore : I공급자관심구독DraftStore
{
    private readonly ConcurrentDictionary<
        Guid,
        SupplierInterestSubscriptionDraftResponse> _drafts = new();

    public Task<SupplierInterestSubscriptionDraftResponse> 저장Async(
        SupplierInterestSubscriptionDraftResponse draft,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _drafts[draft.DraftId] = draft;
        return Task.FromResult(draft);
    }

    public Task<SupplierInterestSubscriptionDraftResponse?> 조회Async(
        Guid draftId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _drafts.TryGetValue(draftId, out var draft);
        return Task.FromResult(draft);
    }
}

public interface I공급자관심구독Service
{
    Task<SupplierInterestSubscriptionDraftResponse> 초안생성Async(
        string ownerUserId,
        SupplierInterestSubscriptionDraftRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierInterestSubscriptionDraftResponse?> 초안조회Async(
        string ownerUserId,
        Guid draftId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 과금 없는 관심 구독 의향만 저장합니다.
/// 공급자 연락처 공개, 유료 멤버십 활성화와 주문 실행은 별도 동의와 상태 전이가 필요합니다.
/// </summary>
public sealed class 공급자관심구독Service : I공급자관심구독Service
{
    private readonly I공급자관심구독DraftStore _store;
    private readonly TimeProvider _timeProvider;

    public 공급자관심구독Service(
        I공급자관심구독DraftStore store,
        TimeProvider timeProvider)
    {
        _store = store;
        _timeProvider = timeProvider;
    }

    public Task<SupplierInterestSubscriptionDraftResponse> 초안생성Async(
        string ownerUserId,
        SupplierInterestSubscriptionDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserId);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SupplierKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SupplierDisplayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TermsVersion);

        ValidatePartyType(request.SupplierPartyTypeCode);
        ValidateAudience(request);

        var draft = new SupplierInterestSubscriptionDraftResponse
        {
            DraftId = Guid.NewGuid(),
            OwnerUserId = ownerUserId.Trim(),
            SupplierKey = request.SupplierKey.Trim(),
            SupplierDisplayName = request.SupplierDisplayName.Trim(),
            SupplierPartyTypeCode = request.SupplierPartyTypeCode,
            AudienceTypeCode = request.AudienceTypeCode,
            DeliveryScopeKey = NullIfWhiteSpace(request.DeliveryScopeKey),
            InterestedProductTags = request.InterestedProductTags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(20)
                .ToArray(),
            ReceiveSupplierUpdates = request.ReceiveSupplierUpdates,
            StatusCode = SupplierMembershipStatusCodes.InterestFollowing,
            TermsVersion = request.TermsVersion.Trim(),
            PaymentRequired = false,
            SupplierContactDetailsDisclosed = false,
            MembershipActivated = false,
            CreatedAtUtc = _timeProvider.GetUtcNow(),
            GuidanceMessage =
                "무료 관심 구독 초안입니다. 유료 멤버십, 자동 갱신, 연락처 공개와 상품 주문은 " +
                "각각 별도 동의가 필요합니다."
        };

        return _store.저장Async(draft, cancellationToken);
    }

    public async Task<SupplierInterestSubscriptionDraftResponse?> 초안조회Async(
        string ownerUserId,
        Guid draftId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerUserId);
        if (draftId == Guid.Empty)
        {
            return null;
        }

        var draft = await _store.조회Async(draftId, cancellationToken);
        return draft is not null &&
               string.Equals(
                   draft.OwnerUserId,
                   ownerUserId.Trim(),
                   StringComparison.Ordinal)
            ? draft
            : null;
    }

    private static void ValidatePartyType(string partyTypeCode)
    {
        if (partyTypeCode is not
            SupplierRelationshipPartyTypeCodes.DomesticAgriculturalBusiness and not
            SupplierRelationshipPartyTypeCodes.OverseasFoodManufacturer)
        {
            throw new ArgumentException(
                "지원하지 않는 공급자 유형입니다.",
                nameof(partyTypeCode));
        }
    }

    private static void ValidateAudience(SupplierInterestSubscriptionDraftRequest request)
    {
        if (request.AudienceTypeCode is not
            SupplierRelationshipAudienceTypeCodes.IndividualOrderer and not
            SupplierRelationshipAudienceTypeCodes.DeliveryScopeGroup)
        {
            throw new ArgumentException(
                "지원하지 않는 구독 대상 유형입니다.",
                nameof(request.AudienceTypeCode));
        }

        if (!request.CurrentMemberConsentConfirmed)
        {
            throw new ArgumentException(
                "현재 사용자의 관심 구독 동의가 필요합니다.",
                nameof(request.CurrentMemberConsentConfirmed));
        }

        if (request.AudienceTypeCode ==
                SupplierRelationshipAudienceTypeCodes.DeliveryScopeGroup &&
            string.IsNullOrWhiteSpace(request.DeliveryScopeKey))
        {
            throw new ArgumentException(
                "배송권 집단 관심 구독에는 배송권 Key가 필요합니다.",
                nameof(request.DeliveryScopeKey));
        }
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
