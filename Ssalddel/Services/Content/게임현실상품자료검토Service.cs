using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Services.Content;

/// <summary>
/// 서버가 읽은 상태와 인증 주체로 다음 사본만 준비한다. DI/API/Store 미등록.
/// 후속 저장자는 ExpectedRevision과 이력을 원자적으로 저장해야 한다.
/// </summary>
[SsalddelCodeMetadata(게임현실상품자료Codes.FeatureKey, SsalddelCodeLayer.Application,
    "운영 권한·개정·자료 완전성을 검사하여 상품자료 검토의 다음 상태 후보를 준비한다.",
    StepKey = "curation-prepare", DependsOnStepKeys = new[] { "curation-state" }, FlowOrder = 20,
    ExecutionStage = SsalddelCodeExecutionStage.Preview, Effects = SsalddelCodeEffect.None,
    Boundary = "서버 인증 정책을 재사용한다. Provider·DB·게시·통지·Simulation 실행 포트는 없으며 영속 원자성은 후속이다.")]
public sealed class 게임현실상품자료검토Service(IAuthorizationService authorization, TimeProvider clock)
{
    public const string AuthorizationPolicy = "서버관리자전용";

    public async Task<게임현실상품자료Result> 준비Async(
        게임현실상품자료State? current,
        게임현실상품자료Request request,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(principal);
        cancellationToken.ThrowIfCancellationRequested();
        // 인증 await 동안 호출자가 목록을 바꿔도 검사와 감사 hash는 같은 사본을 본다.
        if (request.초안?.부족조건 is not null)
            request = request with { 초안 = Copy(request.초안) };
        if (current is not null) current = Copy(current);
        var reviewer = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (principal.Identity?.IsAuthenticated != true || !Present(reviewer)
            || !(await authorization.AuthorizeAsync(principal, null, AuthorizationPolicy)).Succeeded)
            return Deny("Unauthorized");
        cancellationToken.ThrowIfCancellationRequested();

        if (!Present(request.StableId) || !Present(request.IdempotencyKey)
            || !Present(request.검토메모) || request.ExpectedRevision < 0
            || !Enum.IsDefined(request.Action))
            return Deny("InvalidRequest");
        if (current is not null && current.StableId != request.StableId)
            return Deny("IdentityMismatch");
        // UI의 승인 값으로 인증하지 않는다. 요청/중복 키는 서버 인증 주체에도 결속한다.
        var requestHash = Convert.ToHexString(SHA256.HashData(
            JsonSerializer.SerializeToUtf8Bytes(new { ReviewerId = reviewer, Request = request })));
        var prior = current?.History.FirstOrDefault(h => h.IdempotencyKey == request.IdempotencyKey);
        if (prior is not null)
            return prior.RequestHash == requestHash
                ? new(true, true, "Duplicate", Copy(current!))
                : Deny("IdempotencyConflict");
        if (request.ExpectedRevision != (current?.Revision ?? 0))
            return Deny("RevisionConflict");
        if (request.Action is not (게임현실상품자료Action.CreateDraft or 게임현실상품자료Action.ReviseDraft)
            && request.초안 is not null)
            return Deny("UnexpectedDraft");

        var now = clock.GetUtcNow();
        게임현실상품자료State next;
        switch (request.Action)
        {
            case 게임현실상품자료Action.CreateDraft:
            case 게임현실상품자료Action.ReviseDraft:
                if ((request.Action == 게임현실상품자료Action.CreateDraft) != (current is null))
                    return Deny("InvalidTransition");
                if (!ValidDraft(request.초안)) return Deny("InvalidDraft");
                if (current is not null && current.초안.게임상품.StableId != request.초안!.게임상품.StableId)
                    return Deny("GameProductMismatch");
                next = new(request.StableId, 0, Copy(request.초안!), 게임현실상품자료Codes.Draft,
                    false, current?.History ?? []);
                break;
            case 게임현실상품자료Action.SubmitReview:
                if (current?.검토상태 != 게임현실상품자료Codes.Draft) return Deny("InvalidTransition");
                if (current.초안.현실후보 is null || !DisplayComplete(current.초안))
                    return Deny("ReviewDraftIncomplete");
                next = current with { 검토상태 = CommunityInformationReviewStates.PendingReview };
                break;
            case 게임현실상품자료Action.ApproveMapping:
                if (current?.검토상태 != CommunityInformationReviewStates.PendingReview || current.대응승인됨)
                    return Deny("InvalidTransition");
                if (current.초안.현실후보 is null
                    || current.초안.대응종류 is not (게임현실상품자료Codes.Similar or 게임현실상품자료Codes.Identical)
                    || !Present(current.초안.대응근거))
                    return Deny("MappingUnconfirmed");
                next = current with { 대응승인됨 = true };
                break;
            case 게임현실상품자료Action.Approve:
                if (current?.검토상태 != CommunityInformationReviewStates.PendingReview)
                    return Deny("InvalidTransition");
                var issue = ApprovalIssue(current, now);
                if (issue is not null) return Deny(issue);
                next = current with { 검토상태 = CommunityInformationReviewStates.Approved };
                break;
            case 게임현실상품자료Action.Exclude:
                if (current is null || current.검토상태 == CommunityInformationReviewStates.Excluded)
                    return Deny("InvalidTransition");
                next = current with { 검토상태 = CommunityInformationReviewStates.Excluded };
                break;
            default:
                return Deny("InvalidRequest");
        }

        var revision = checked((current?.Revision ?? 0) + 1);
        var history = next.History.Append(new 게임현실상품자료이력Dto(request.IdempotencyKey,
            requestHash, reviewer!, request.Action, request.검토메모, now, revision)).ToArray();
        return new(true, false, "PreparedNotPersisted", Copy(next with { Revision = revision, History = history }));
    }

    private static string? ApprovalIssue(게임현실상품자료State state, DateTimeOffset now)
    {
        var draft = state.초안;
        var candidate = draft.현실후보;
        if (!state.대응승인됨) return "MappingUnconfirmed";
        if (candidate is null || candidate.자료종류 is not (게임현실상품자료Codes.Fixture or 게임현실상품자료Codes.Observed))
            return "ObservationMissing";
        if (!Present(candidate.출처) || !Https(candidate.상품Url)
            || !Present(candidate.플랫폼) || !Present(candidate.판매자) || !Present(candidate.원천상품Id)
            || candidate.관측시각 is null || candidate.관측시각 > now)
            return "SourceIncomplete";
        if (candidate.이용조건검토상태 != CommunityInformationReviewStates.Approved
            || !Present(candidate.이용조건근거))
            return "UsageUnreviewed";
        if (candidate.가격?.현재가격 is null or <= 0 || candidate.가격.배송비 is null or < 0
            || !Currency(candidate.가격.통화코드) || candidate.수량 is null or <= 0
            || candidate.최소주문수량 is null or <= 0 || !Present(candidate.단위)
            || !Present(candidate.규격) || !Present(candidate.배송조건))
            return "PriceOrTermsIncomplete";
        if (draft.비교상태 != 게임현실상품자료Codes.Comparable || !Present(draft.비교근거)
            || draft.부족조건.Count > 0)
            return "NotComparable";
        return DisplayComplete(draft) ? null : "ReviewDraftIncomplete";
    }

    private static bool ValidDraft(게임현실상품자료초안Dto? draft)
        => draft is not null && draft.게임상품 is not null
           && Present(draft.게임상품.StableId) && Present(draft.게임상품.이름)
           && draft.부족조건 is not null && draft.부족조건.All(Present)
           && draft.대응종류 is 게임현실상품자료Codes.Unconfirmed or 게임현실상품자료Codes.Similar or 게임현실상품자료Codes.Identical
           && (draft.현실후보 is null || (Present(draft.현실후보.StableId)
               && draft.현실후보.자료종류 is 게임현실상품자료Codes.Uncollected or 게임현실상품자료Codes.Fixture or 게임현실상품자료Codes.Observed));

    private static bool DisplayComplete(게임현실상품자료초안Dto draft)
        => Present(draft.제목) && Present(draft.요약) && Present(draft.출처표시) && Present(draft.한계);
    private static bool Present(string? value) => !string.IsNullOrWhiteSpace(value);
    private static bool Currency(string? value)
        => value is { Length: 3 } && value.All(c => c is >= 'A' and <= 'Z');
    private static bool Https(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
           && string.IsNullOrEmpty(uri.UserInfo);
    private static 게임현실상품자료Result Deny(string reason) => new(false, false, reason, null);
    private static 게임현실상품자료초안Dto Copy(게임현실상품자료초안Dto draft)
        => draft with { 부족조건 = Array.AsReadOnly(draft.부족조건.ToArray()) };
    private static 게임현실상품자료State Copy(게임현실상품자료State state)
        => state with { 초안 = Copy(state.초안), History = Array.AsReadOnly(state.History.ToArray()) };
}
