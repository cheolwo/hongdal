using Hongdal.Contracts.Common.AgriculturalFisheries;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.AgriculturalFisheries.ImportReadiness;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Services.Community;

public interface ICommunityPostOpportunityService
{
    Task<CommunityPostOpportunityListResponse?> GetAsync(
        long postId,
        string? displayLanguageCode,
        CancellationToken cancellationToken = default);

    Task<StartCommunityMeatImportReadinessResponse> StartMeatImportReadinessAsync(
        long postId,
        StartCommunityMeatImportReadinessRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default);
}

public interface ICommunityPostOpportunityAnalyzer
{
    CommunityPostOpportunityAnalysis Analyze(string? title, string? body);
}

public sealed record CommunityPostOpportunityAnalysis(
    bool SuggestMeatImportReadiness,
    IReadOnlyList<string> MatchedSignals);

public sealed class CommunityPostOpportunityAnalyzer : ICommunityPostOpportunityAnalyzer
{
    private static readonly string[] MeatSignals =
    [
        "소고기", "쇠고기", "돼지고기", "육류", "축산물", "beef", "pork", "meat"
    ];

    private static readonly string[] CrossBorderSignals =
    [
        "수입", "수출", "해외 작업장", "해외작업장", "검역", "통관",
        "import", "export", "foreign establishment", "quarantine", "customs"
    ];

    public CommunityPostOpportunityAnalysis Analyze(string? title, string? body)
    {
        var text = $"{title}\n{body}";
        var meatMatches = MeatSignals.Where(signal => text.Contains(signal, StringComparison.OrdinalIgnoreCase));
        var crossBorderMatches = CrossBorderSignals.Where(signal => text.Contains(signal, StringComparison.OrdinalIgnoreCase));
        var matched = meatMatches
            .Concat(crossBorderMatches)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new(
            meatMatches.Any() && crossBorderMatches.Any(),
            matched);
    }
}

public sealed record CommunityPostOpportunitySource(
    long PostId,
    string AppKey,
    string Title,
    string Body,
    string? AuthorUserId,
    string? LinkedLedgerId);

public enum CommunityPostLedgerLinkResult
{
    Linked,
    AlreadyLinked,
    NotFound,
    NotOwner,
    ConflictingLedger
}

public interface ICommunityPostOpportunityStore
{
    Task<CommunityPostOpportunitySource?> GetAsync(long postId, CancellationToken cancellationToken = default);

    Task<CommunityPostLedgerLinkResult> LinkLedgerAsync(
        long postId,
        string actorUserId,
        string ledgerId,
        CancellationToken cancellationToken = default);
}

public sealed class EfCommunityPostOpportunityStore : ICommunityPostOpportunityStore
{
    private readonly HongdalContext _db;

    public EfCommunityPostOpportunityStore(HongdalContext db)
    {
        _db = db;
    }

    public Task<CommunityPostOpportunitySource?> GetAsync(
        long postId,
        CancellationToken cancellationToken = default)
        => _db.PlatformCommunityPosts
            .AsNoTracking()
            .Where(post => post.Id == postId && !post.IsDeleted)
            .Select(post => new CommunityPostOpportunitySource(
                post.Id,
                post.AppKey,
                post.Title,
                post.Body,
                post.AuthorUserId,
                post.커뮤니티원장Id))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<CommunityPostLedgerLinkResult> LinkLedgerAsync(
        long postId,
        string actorUserId,
        string ledgerId,
        CancellationToken cancellationToken = default)
    {
        var actor = actorUserId.Trim();
        var updated = await _db.PlatformCommunityPosts
            .Where(post => post.Id == postId
                           && !post.IsDeleted
                           && post.AuthorUserId == actor
                           && post.커뮤니티원장Id == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(post => post.커뮤니티원장Id, ledgerId)
                    .SetProperty(post => post.UpdatedAtUtc, DateTime.UtcNow),
                cancellationToken);
        if (updated == 1)
        {
            return CommunityPostLedgerLinkResult.Linked;
        }

        var current = await GetAsync(postId, cancellationToken);
        if (current is null)
        {
            return CommunityPostLedgerLinkResult.NotFound;
        }

        if (!string.Equals(current.AuthorUserId, actor, StringComparison.OrdinalIgnoreCase))
        {
            return CommunityPostLedgerLinkResult.NotOwner;
        }

        return string.Equals(current.LinkedLedgerId, ledgerId, StringComparison.OrdinalIgnoreCase)
            ? CommunityPostLedgerLinkResult.AlreadyLinked
            : CommunityPostLedgerLinkResult.ConflictingLedger;
    }
}

public sealed class CommunityPostOpportunityService : ICommunityPostOpportunityService
{
    private readonly ICommunityPostOpportunityStore _postStore;
    private readonly ICommunityPostOpportunityAnalyzer _analyzer;
    private readonly IMeatImportReadinessService _readinessService;

    public CommunityPostOpportunityService(
        ICommunityPostOpportunityStore postStore,
        ICommunityPostOpportunityAnalyzer analyzer,
        IMeatImportReadinessService readinessService)
    {
        _postStore = postStore;
        _analyzer = analyzer;
        _readinessService = readinessService;
    }

    public async Task<CommunityPostOpportunityListResponse?> GetAsync(
        long postId,
        string? displayLanguageCode,
        CancellationToken cancellationToken = default)
    {
        var source = await _postStore.GetAsync(postId, cancellationToken);
        if (source is null)
        {
            return null;
        }

        var language = CommunityDisplayLanguageCodes.Normalize(displayLanguageCode);
        var analysis = _analyzer.Analyze(source.Title, source.Body);
        var expectedLedgerId = MeatImportReadinessCaseIds.FromCommunityPost(postId);
        var isActive = string.Equals(source.LinkedLedgerId, expectedLedgerId, StringComparison.OrdinalIgnoreCase);
        var items = analysis.SuggestMeatImportReadiness || isActive
            ? new[] { BuildOpportunity(source, analysis, language) }
            : [];

        return new CommunityPostOpportunityListResponse
        {
            PostId = source.PostId,
            DisplayLanguageCode = language,
            ExperiencePolicy = new CommunitySharedExperiencePolicyResponse(),
            Items = items
        };
    }

    public async Task<StartCommunityMeatImportReadinessResponse> StartMeatImportReadinessAsync(
        long postId,
        StartCommunityMeatImportReadinessRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Case);
        var actor = RequireActor(actorUserId);
        if (!request.ConfirmExplicitStart || !request.ConfirmInformationOnly)
        {
            throw new InvalidOperationException("자동 전환은 하지 않습니다. 시작 의사와 정보 제공 전용 경계를 모두 명시적으로 확인해야 합니다.");
        }

        var source = await _postStore.GetAsync(postId, cancellationToken)
                     ?? throw new KeyNotFoundException("커뮤니티 게시글을 찾을 수 없습니다.");
        if (string.IsNullOrWhiteSpace(source.AuthorUserId)
            || !string.Equals(source.AuthorUserId, actor, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("게시글 작성자만 이 대화에서 준비도 원장을 시작할 수 있습니다.");
        }

        var analysis = _analyzer.Analyze(source.Title, source.Body);
        if (!analysis.SuggestMeatImportReadiness)
        {
            throw new InvalidOperationException("게시글에서 육류와 국경 간 거래 신호가 함께 확인되지 않아 이 정보 협업을 제안할 수 없습니다.");
        }

        var expectedLedgerId = MeatImportReadinessCaseIds.FromCommunityPost(postId);
        if (source.LinkedLedgerId is not null
            && !string.Equals(source.LinkedLedgerId, expectedLedgerId, StringComparison.OrdinalIgnoreCase))
        {
            throw new CommunityPostOpportunityConflictException("게시글에 이미 다른 원장이 연결되어 있습니다.");
        }

        request.Case.CommunityId = string.IsNullOrWhiteSpace(request.Case.CommunityId)
            ? source.AppKey
            : request.Case.CommunityId;
        var readinessCase = await _readinessService.CreateCaseFromCommunityPostAsync(
            postId,
            request.Case,
            actor,
            actorDisplayName,
            cancellationToken);
        var linkResult = await _postStore.LinkLedgerAsync(postId, actor, readinessCase.CaseId, cancellationToken);
        if (linkResult is CommunityPostLedgerLinkResult.NotFound)
        {
            throw new KeyNotFoundException("커뮤니티 게시글을 찾을 수 없습니다.");
        }

        if (linkResult is CommunityPostLedgerLinkResult.NotOwner)
        {
            throw new UnauthorizedAccessException("게시글 작성자만 준비도 원장을 연결할 수 있습니다.");
        }

        if (linkResult is CommunityPostLedgerLinkResult.ConflictingLedger)
        {
            throw new CommunityPostOpportunityConflictException("동시에 다른 원장이 게시글에 연결되었습니다. 게시글을 다시 확인해 주세요.");
        }

        var linkedSource = source with { LinkedLedgerId = readinessCase.CaseId };
        var language = CommunityDisplayLanguageCodes.Normalize(request.DisplayLanguageCode);
        return new StartCommunityMeatImportReadinessResponse
        {
            PostId = postId,
            DisplayLanguageCode = language,
            LinkedToCommunityPost = true,
            Opportunity = BuildOpportunity(linkedSource, analysis, language),
            Case = readinessCase
        };
    }

    private static CommunityPostOpportunityResponse BuildOpportunity(
        CommunityPostOpportunitySource source,
        CommunityPostOpportunityAnalysis analysis,
        string language)
    {
        var expectedLedgerId = MeatImportReadinessCaseIds.FromCommunityPost(source.PostId);
        var active = string.Equals(source.LinkedLedgerId, expectedLedgerId, StringComparison.OrdinalIgnoreCase);
        var blocked = source.LinkedLedgerId is not null && !active;
        var english = string.Equals(language, CommunityDisplayLanguageCodes.English, StringComparison.OrdinalIgnoreCase);

        return new CommunityPostOpportunityResponse
        {
            Code = CommunityPostOpportunityCodes.MeatImportReadiness,
            StateCode = active
                ? CommunityPostOpportunityStateCodes.Active
                : blocked
                    ? CommunityPostOpportunityStateCodes.BlockedByAnotherLedger
                    : CommunityPostOpportunityStateCodes.Suggested,
            Title = english ? "Review meat import readiness" : "육류 수입 준비 정보 확인",
            Summary = english
                ? "Review the same information-only checklist together before either party begins an import transaction."
                : "어느 한쪽이 수입 업무를 실행하기 전에 국내외 당사자가 같은 정보 제공용 절차표를 함께 확인합니다.",
            WhySuggested = english
                ? "The post contains both meat-product and cross-border trade signals. Nothing starts automatically."
                : "게시글에서 육류 제품과 국경 간 거래 신호가 함께 확인되었습니다. 어떤 업무도 자동으로 시작하지 않습니다.",
            LedgerTemplateKey = CommunityLedgerTemplateKeys.MeatImportReadiness,
            CanStart = !active && !blocked,
            AutoStartsWorkflow = false,
            RequiresExplicitConsent = true,
            InformationOnly = true,
            IsBrokerageEnabled = false,
            PreviewEndpoint = $"/api/v1/agricultural-fisheries/import-readiness/diagram?displayLanguage={language}",
            StartEndpoint = $"/api/v1/community/posts/{source.PostId}/opportunities/meat-import-readiness/start",
            MatchedSignals = analysis.MatchedSignals,
            MissingInformationPrompts = english
                ? ["Which beef or pork product is involved?", "What is the origin country and HS code?", "Who will participate on the Korean and overseas sides?"]
                : ["소고기·돼지고기 중 어떤 제품인가요?", "원산지 국가와 HS 코드는 무엇인가요?", "한국 측과 해외 측에서 누가 함께 확인하나요?"]
        };
    }

    private static string RequireActor(string? actorUserId)
        => string.IsNullOrWhiteSpace(actorUserId)
            ? throw new UnauthorizedAccessException("로그인 사용자 식별자를 확인할 수 없습니다.")
            : actorUserId.Trim();
}

public sealed class CommunityPostOpportunityConflictException : Exception
{
    public CommunityPostOpportunityConflictException(string message)
        : base(message)
    {
    }
}
