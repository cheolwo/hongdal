using Hongdal.Contracts.Common.AgriculturalFisheries;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.AgriculturalFisheries.ImportReadiness;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using 홍달.Data;

namespace Hongdal.Services.Community;

public interface ICommunityPostOpportunityService
{
    Task<CommunityPostOpportunityListResponse?> GetAsync(
        long postId,
        string? displayLanguageCode,
        CancellationToken cancellationToken = default);

    Task<StartCommunityPostParticipationResponse> StartParticipationAsync(
        long postId,
        StartCommunityPostParticipationRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default);

    Task<PromoteCommunityPostParticipationResponse> PromoteParticipationAsync(
        long postId,
        PromoteCommunityPostParticipationRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default);

    Task<JoinCommunityPostProfessionalResponse> JoinProfessionalAsync(
        long postId,
        JoinCommunityPostProfessionalRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default);

    Task<JoinCommunityPostPartyRoleResponse> JoinPartyRoleAsync(
        long postId,
        JoinCommunityPostPartyRoleRequest request,
        string actorUserId,
        string actorDisplayName,
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
    string? LinkedLedgerId,
    bool IsReportBoardPost = false);

public enum CommunityPostLedgerLinkResult
{
    Linked,
    AlreadyLinked,
    NotFound,
    NotOwner,
    ConflictingLedger
}

public enum CommunityPostMomentumUpdateResult
{
    Updated,
    NotFound,
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

    Task<CommunityPostMomentumUpdateResult> SetMomentumPromotionAsync(
        long postId,
        string ledgerId,
        string momentumCode,
        string momentumMessage,
        int roleParticipantCount,
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
                post.커뮤니티원장Id,
                post.IsReportBoardPost))
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
                           && !post.IsReportBoardPost
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

    public async Task<CommunityPostMomentumUpdateResult> SetMomentumPromotionAsync(
        long postId,
        string ledgerId,
        string momentumCode,
        string momentumMessage,
        int roleParticipantCount,
        CancellationToken cancellationToken = default)
    {
        var normalizedLedgerId = ledgerId.Trim();
        var normalizedCode = momentumCode.Trim();
        var normalizedMessage = momentumMessage.Trim();
        var participantCount = Math.Max(0, roleParticipantCount);
        var now = DateTime.UtcNow;
        var updated = await _db.PlatformCommunityPosts
            .Where(post => post.Id == postId
                           && !post.IsDeleted
                           && !post.IsReportBoardPost
                           && post.커뮤니티원장Id == normalizedLedgerId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(post => post.IsCommunityMomentumPromoted, true)
                    .SetProperty(post => post.CommunityMomentumCode, normalizedCode)
                    .SetProperty(post => post.CommunityMomentumMessage, normalizedMessage)
                    .SetProperty(post => post.CommunityMomentumRoleParticipantCount, participantCount)
                    .SetProperty(post => post.CommunityMomentumUpdatedAtUtc, now)
                    .SetProperty(post => post.UpdatedAtUtc, now),
                cancellationToken);
        if (updated == 1)
        {
            return CommunityPostMomentumUpdateResult.Updated;
        }

        var current = await GetAsync(postId, cancellationToken);
        return current is null
            ? CommunityPostMomentumUpdateResult.NotFound
            : CommunityPostMomentumUpdateResult.ConflictingLedger;
    }
}

public sealed class CommunityPostOpportunityService : ICommunityPostOpportunityService
{
    private readonly ICommunityPostOpportunityStore _postStore;
    private readonly ICommunityPostOpportunityAnalyzer _analyzer;
    private readonly IMeatImportReadinessService _readinessService;
    private readonly ICommunityVoteService? _voteService;
    private readonly I커뮤니티원장저장소? _ledgerStore;
    private readonly ICommunityProfessionalEligibilityService? _professionalEligibilityService;
    private readonly ICommunityPostProfessionalParticipationService? _professionalParticipationService;

    public CommunityPostOpportunityService(
        ICommunityPostOpportunityStore postStore,
        ICommunityPostOpportunityAnalyzer analyzer,
        IMeatImportReadinessService readinessService,
        ICommunityVoteService? voteService = null,
        I커뮤니티원장저장소? ledgerStore = null,
        ICommunityProfessionalEligibilityService? professionalEligibilityService = null,
        ICommunityPostProfessionalParticipationService? professionalParticipationService = null)
    {
        _postStore = postStore;
        _analyzer = analyzer;
        _readinessService = readinessService;
        _voteService = voteService;
        _ledgerStore = ledgerStore;
        _professionalEligibilityService = professionalEligibilityService;
        _professionalParticipationService = professionalParticipationService;
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
        var items = !source.IsReportBoardPost && (analysis.SuggestMeatImportReadiness || isActive)
            ? new[] { BuildOpportunity(source, analysis, language) }
            : [];
        var participationVote = await FindParticipationVoteAsync(postId, cancellationToken);
        커뮤니티원장Dto? provisionalLedger = null;
        if (_ledgerStore is not null && !string.IsNullOrWhiteSpace(source.LinkedLedgerId))
        {
            provisionalLedger = await _ledgerStore.원장조회Async(source.LinkedLedgerId, cancellationToken);
        }

        return new CommunityPostOpportunityListResponse
        {
            PostId = source.PostId,
            DisplayLanguageCode = language,
            ExperiencePolicy = new CommunitySharedExperiencePolicyResponse(),
            Participation = BuildParticipationEntry(source, language, participationVote, provisionalLedger),
            Items = items
        };
    }

    public async Task<StartCommunityPostParticipationResponse> StartParticipationAsync(
        long postId,
        StartCommunityPostParticipationRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireActor(actorUserId);
        if (!request.ConfirmExplicitStart || !request.ConfirmNonBindingParticipation)
        {
            throw new InvalidOperationException(
                "게시글에서 참여 관심 모집을 명시적으로 시작하고, 이 단계가 비구속적이라는 점을 모두 확인해야 합니다.");
        }

        var source = await _postStore.GetAsync(postId, cancellationToken)
                     ?? throw new KeyNotFoundException("커뮤니티 게시글을 찾을 수 없습니다.");
        EnsureCollectiveActionAllowed(source);
        var language = CommunityDisplayLanguageCodes.Normalize(request.DisplayLanguageCode);
        var voteService = _voteService
                          ?? throw new InvalidOperationException("커뮤니티 참여 관심 투표 서비스가 구성되지 않았습니다.");
        var existingVote = await FindParticipationVoteAsync(postId, cancellationToken);
        if (existingVote?.Status == CommunityVoteStatusCodes.Open)
        {
            return BuildParticipationStartResponse(source, language, existingVote, reused: true);
        }

        if (!string.IsNullOrWhiteSpace(existingVote?.CommunityLedgerId))
        {
            return BuildParticipationStartResponse(source, language, existingVote, reused: true);
        }

        var roleOptions = BuildRoleDefinitions(language);
        var interestVote = await voteService.CreateAsync(
            new CommunityVoteCreateRequest
            {
                AppKey = source.AppKey,
                CommunityScope = source.AppKey,
                Title = BuildParticipationTitle(source, request.Title, language),
                Description = string.Equals(language, CommunityDisplayLanguageCodes.English, StringComparison.OrdinalIgnoreCase)
                    ? "Choose any roles you may be interested in. This is non-binding and does not create an order, contract, dispatch, brokerage, or ledger."
                    : "관심 있는 역할을 가볍게 선택합니다. 이 선택만으로 주문·계약·배차·주선·원장이 만들어지지 않습니다.",
                VoteKind = CommunityVoteKindCodes.CollectiveActionInterest,
                SourcePostId = postId,
                StructuredOptions = roleOptions.Select(role => new CommunityVoteOptionCreateRequest
                {
                    Text = role.Label,
                    ProductKey = RoleProductKey(role.RoleCode)
                }).ToArray(),
                AllowMultipleSelection = true,
                ResolutionDocumentEnabled = false,
                SignatureRequired = false,
                ClosesAtUtc = request.ClosesAtUtc,
                CreatedByDisplayName = string.IsNullOrWhiteSpace(actorDisplayName)
                    ? "참여자"
                    : actorDisplayName.Trim()
            },
            cancellationToken);

        return BuildParticipationStartResponse(source, language, interestVote, reused: false);
    }

    public async Task<PromoteCommunityPostParticipationResponse> PromoteParticipationAsync(
        long postId,
        PromoteCommunityPostParticipationRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = RequireActor(actorUserId);
        if (!request.ConfirmProvisionalLedger
            || !request.ConfirmNonBindingEvidence
            || !request.ConfirmParticipantNotifications)
        {
            throw new InvalidOperationException(
                "가원장 생성, 비구속적 관심 증빙, 참여자 알림을 모두 명시적으로 확인해야 합니다.");
        }

        if (request.InterestVoteId == Guid.Empty)
        {
            throw new InvalidOperationException("승격할 참여 관심 투표가 필요합니다.");
        }

        if (!CommunityCollectiveIntentTypeCodes.IsSupported(request.CollectiveIntentTypeCode))
        {
            throw new InvalidOperationException("공동구매, 공동수입 또는 공동수출 검토 의도만 가원장으로 기록할 수 있습니다.");
        }

        var intentTypeCode = CommunityCollectiveIntentTypeCodes.All.First(code =>
            string.Equals(code, request.CollectiveIntentTypeCode.Trim(), StringComparison.OrdinalIgnoreCase));
        var tradeDirectionCode = NormalizeTradeDirectionCode(intentTypeCode, request.TradeDirectionCode);
        var originCountryCode = NormalizeCountryCode(request.OriginCountryCode, "출발국가");
        var destinationCountryCode = NormalizeCountryCode(request.DestinationCountryCode, "도착국가");
        var transportModeCodes = NormalizeTransportModeCodes(request.TransportModeCodes);

        var source = await _postStore.GetAsync(postId, cancellationToken)
                     ?? throw new KeyNotFoundException("커뮤니티 게시글을 찾을 수 없습니다.");
        EnsureCollectiveActionAllowed(source);
        if (string.IsNullOrWhiteSpace(source.AuthorUserId)
            || !string.Equals(source.AuthorUserId, actor, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("게시글 작성자만 모인 관심을 가원장으로 승격할 수 있습니다.");
        }

        var voteService = _voteService
                          ?? throw new InvalidOperationException("커뮤니티 참여 관심 투표 서비스가 구성되지 않았습니다.");
        var ledgerStore = _ledgerStore
                          ?? throw new InvalidOperationException("커뮤니티 원장 저장소가 구성되지 않았습니다.");
        var snapshot = await voteService.GetInterestPromotionSnapshotAsync(
                           request.InterestVoteId,
                           postId,
                           cancellationToken)
                       ?? throw new KeyNotFoundException("참여 관심 투표를 찾을 수 없습니다.");
        if (snapshot.ParticipantCount < CommunityPostProvisionalLedgerPolicy.MinimumParticipantCount)
        {
            throw new InvalidOperationException(
                $"가원장은 서로 다른 관심 참여자 {CommunityPostProvisionalLedgerPolicy.MinimumParticipantCount}명 이상이 모인 뒤 만들 수 있습니다.");
        }

        IReadOnlyList<string> authorProfessionalRoles = _professionalEligibilityService is null
            ? []
            : await _professionalEligibilityService.GetVerifiedRoleCodesAsync(actor, cancellationToken);
        var plannedSpecialistRoles = CommunityPostPartyRoleCodes
            .ForPlan(tradeDirectionCode, transportModeCodes)
            .Where(CommunityPostPartyRoleCodes.IsSpecialist)
            .ToArray();
        authorProfessionalRoles = authorProfessionalRoles
            .Where(role => plannedSpecialistRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .ToArray();
        var ledgerId = CommunityPostProvisionalLedgerIds.FromInterestVote(postId, request.InterestVoteId);
        if (source.LinkedLedgerId is not null
            && !string.Equals(source.LinkedLedgerId, ledgerId, StringComparison.OrdinalIgnoreCase))
        {
            throw new CommunityPostOpportunityConflictException("게시글에 이미 다른 원장이 연결되어 있습니다.");
        }

        var linkResult = await _postStore.LinkLedgerAsync(postId, actor, ledgerId, cancellationToken);
        if (linkResult is CommunityPostLedgerLinkResult.NotFound)
        {
            throw new KeyNotFoundException("커뮤니티 게시글을 찾을 수 없습니다.");
        }

        if (linkResult is CommunityPostLedgerLinkResult.NotOwner)
        {
            throw new UnauthorizedAccessException("게시글 작성자만 가원장을 연결할 수 있습니다.");
        }

        if (linkResult is CommunityPostLedgerLinkResult.ConflictingLedger)
        {
            throw new CommunityPostOpportunityConflictException("동시에 다른 원장이 게시글에 연결되었습니다. 게시글을 다시 확인해 주세요.");
        }

        var ledger = await ledgerStore.원장조회Async(ledgerId, cancellationToken);
        var reused = ledger is not null;
        if (ledger is null)
        {
            try
            {
                ledger = await ledgerStore.원장저장Async(
                    BuildProvisionalLedgerRequest(
                        source,
                        snapshot,
                        ledgerId,
                        intentTypeCode,
                        tradeDirectionCode,
                        originCountryCode,
                        destinationCountryCode,
                        transportModeCodes,
                        actor,
                        actorDisplayName,
                        authorProfessionalRoles),
                    actor,
                    cancellationToken);
            }
            catch (InvalidOperationException)
            {
                ledger = await ledgerStore.원장조회Async(ledgerId, cancellationToken);
                if (ledger is null)
                {
                    throw;
                }

                reused = true;
            }
        }

        var storedTradeDirectionCode = CommunityPostProfessionalParticipationProjection.ReadTradeDirectionCode(ledger);
        var storedOriginCountryCode = ledger.확장속성.GetValueOrDefault(
            CommunityPostProvisionalLedgerPolicy.OriginCountryAttributeKey,
            string.Empty);
        var storedDestinationCountryCode = ledger.확장속성.GetValueOrDefault(
            CommunityPostProvisionalLedgerPolicy.DestinationCountryAttributeKey,
            string.Empty);
        var storedTransportModeCodes = CommunityPostProfessionalParticipationProjection.ReadTransportModeCodes(ledger);
        if (!string.Equals(storedTradeDirectionCode, tradeDirectionCode, StringComparison.OrdinalIgnoreCase)
            || !RouteValueMatches(originCountryCode, storedOriginCountryCode)
            || !RouteValueMatches(destinationCountryCode, storedDestinationCountryCode)
            || transportModeCodes.Count > 0
            && !transportModeCodes.Order(StringComparer.OrdinalIgnoreCase).SequenceEqual(
                storedTransportModeCodes.Order(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase))
        {
            throw new CommunityPostOpportunityConflictException(
                "이미 기록된 가원장의 거래 방향·국가·운송수단과 요청이 다릅니다.");
        }

        tradeDirectionCode = storedTradeDirectionCode;
        originCountryCode = storedOriginCountryCode;
        destinationCountryCode = storedDestinationCountryCode;
        transportModeCodes = storedTransportModeCodes;

        var attachedSnapshot = await voteService.AttachProvisionalLedgerAsync(
                                   request.InterestVoteId,
                                   postId,
                                   ledgerId,
                                   CommunityPostProvisionalLedgerPolicy.MinimumParticipantCount,
                                   actorDisplayName,
                                   cancellationToken)
                               ?? throw new KeyNotFoundException("참여 관심 투표를 찾을 수 없습니다.");
        var linkedSource = source with { LinkedLedgerId = ledgerId };
        var language = CommunityDisplayLanguageCodes.Normalize(request.DisplayLanguageCode);
        var vote = await voteService.GetAsync(request.InterestVoteId, cancellationToken);
        await SetInitialMomentumPromotionAsync(
            postId,
            ledger,
            authorProfessionalRoles,
            cancellationToken);

        return new PromoteCommunityPostParticipationResponse
        {
            PostId = postId,
            DisplayLanguageCode = language,
            ReusedExistingProvisionalLedger = reused,
            CollectiveIntentTypeCode = intentTypeCode,
            TradeDirectionCode = tradeDirectionCode,
            OriginCountryCode = originCountryCode,
            DestinationCountryCode = destinationCountryCode,
            TransportModeCodes = transportModeCodes,
            ProvisionalLedger = new CommunityPostProvisionalLedgerResponse
            {
                LedgerId = ledger.원장Id,
                Revision = ledger.Revision,
                LedgerTemplateKey = ledger.원장템플릿Key,
                State = ledger.상태,
                CurrentStageCode = ledger.현재단계Key ?? string.Empty,
                ParticipantCount = attachedSnapshot.ParticipantCount,
                EvidenceSnapshotHash = attachedSnapshot.EvidenceSnapshotHash,
                NonBinding = true,
                ParticipantNotificationsRequested = true,
                TradeDirectionCode = tradeDirectionCode,
                OriginCountryCode = originCountryCode,
                DestinationCountryCode = destinationCountryCode,
                TransportModeCodes = transportModeCodes
            },
            Participation = BuildParticipationEntry(linkedSource, language, vote, ledger)
        };
    }

    public Task<JoinCommunityPostProfessionalResponse> JoinProfessionalAsync(
        long postId,
        JoinCommunityPostProfessionalRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
        => (_professionalParticipationService
            ?? throw new InvalidOperationException("커뮤니티 전문가 참여 서비스가 구성되지 않았습니다."))
            .JoinAsync(
                postId,
                request,
                actorUserId,
                actorDisplayName,
                cancellationToken);

    public Task<JoinCommunityPostPartyRoleResponse> JoinPartyRoleAsync(
        long postId,
        JoinCommunityPostPartyRoleRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
        => (_professionalParticipationService
            ?? throw new InvalidOperationException("커뮤니티 거래 역할 참여 서비스가 구성되지 않았습니다."))
            .JoinPartyRoleAsync(
                postId,
                request,
                actorUserId,
                actorDisplayName,
                cancellationToken);

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
        EnsureCollectiveActionAllowed(source);
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

    private async Task SetInitialMomentumPromotionAsync(
        long postId,
        커뮤니티원장Dto ledger,
        IReadOnlyList<string> authorProfessionalRoles,
        CancellationToken cancellationToken)
    {
        var assignments = CommunityPostProfessionalParticipationProjection.ReadAssignments(ledger);
        var professionalCount = assignments
            .Select(assignment => assignment.UserId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (professionalCount == 0 && authorProfessionalRoles.Count > 0)
        {
            professionalCount = 1;
        }

        var momentumCode = CommunityPostProfessionalParticipationProjection.ResolveMomentumCode(
            ledger,
            assignments);
        var updateResult = await _postStore.SetMomentumPromotionAsync(
            postId,
            ledger.원장Id,
            momentumCode,
            CommunityPostProfessionalParticipationProjection.ReadinessMessage(
                ledger,
                CommunityDisplayLanguageCodes.Korean),
            professionalCount,
            cancellationToken);
        if (updateResult == CommunityPostMomentumUpdateResult.NotFound)
        {
            throw new KeyNotFoundException("커뮤니티 게시글을 찾을 수 없습니다.");
        }

        if (updateResult == CommunityPostMomentumUpdateResult.ConflictingLedger)
        {
            throw new CommunityPostOpportunityConflictException("게시글의 가원장 연결이 변경되었습니다.");
        }
    }

    private async Task<CommunityVoteResponse?> FindParticipationVoteAsync(
        long postId,
        CancellationToken cancellationToken)
    {
        if (_voteService is null)
        {
            return null;
        }

        var votes = await _voteService.ListBySourcePostAsync(postId, cancellationToken);
        var interestVotes = votes.Items
            .Where(vote => vote.VoteKind == CommunityVoteKindCodes.CollectiveActionInterest)
            .ToArray();
        return interestVotes.FirstOrDefault(vote => vote.Status == CommunityVoteStatusCodes.Open)
               ?? interestVotes.FirstOrDefault();
    }

    private static StartCommunityPostParticipationResponse BuildParticipationStartResponse(
        CommunityPostOpportunitySource source,
        string language,
        CommunityVoteResponse vote,
        bool reused)
        => new()
        {
            PostId = source.PostId,
            DisplayLanguageCode = language,
            ReusedExistingInterestVote = reused,
            Participation = BuildParticipationEntry(source, language, vote),
            InterestVote = vote
        };

    private static CommunityPostParticipationEntryResponse BuildParticipationEntry(
        CommunityPostOpportunitySource source,
        string language,
        CommunityVoteResponse? vote,
        커뮤니티원장Dto? provisionalLedger = null)
    {
        var english = string.Equals(language, CommunityDisplayLanguageCodes.English, StringComparison.OrdinalIgnoreCase);
        if (source.IsReportBoardPost)
        {
            return new CommunityPostParticipationEntryResponse
            {
                StateCode = CommunityPostParticipationStateCodes.Closed,
                Title = english ? "Collective action is unavailable" : "공동 행동을 시작할 수 없습니다",
                Summary = english
                    ? "Report and dispute posts remain separate from transaction formation."
                    : "신고·분쟁 게시글은 거래 참여와 가원장 구성에서 분리됩니다.",
                CanStart = false,
                CanJoin = false,
                NonBinding = true,
                RoleOptions = []
            };
        }

        var open = vote?.Status == CommunityVoteStatusCodes.Open;
        var provisionalLedgerId = vote?.CommunityLedgerId;
        var promoted = !string.IsNullOrWhiteSpace(provisionalLedgerId);
        return new CommunityPostParticipationEntryResponse
        {
            StateCode = promoted
                ? CommunityPostParticipationStateCodes.ProvisionalLedgerCreated
                : vote is null
                ? CommunityPostParticipationStateCodes.Available
                : open
                    ? CommunityPostParticipationStateCodes.Gathering
                    : CommunityPostParticipationStateCodes.Closed,
            Title = english ? "Maybe we can do this together" : "같이 해볼까요?",
            Summary = english
                ? "Express interest as a buyer, supplier, logistics professional, or observer without committing to a transaction."
                : "구매자·공급자·물류 전문가·관심 참여자 중 가능한 역할을 부담 없이 표시합니다.",
            CanStart = !promoted && !open,
            CanJoin = !promoted && open,
            AutoStartsWorkflow = false,
            NonBinding = true,
            RequiresExplicitStart = true,
            RequiresExplicitPromotionToPlanning = true,
            CanPromoteToProvisionalLedger = !promoted
                                            && open
                                            && vote!.TotalVoteCount >= CommunityPostProvisionalLedgerPolicy.MinimumParticipantCount
                                            && source.LinkedLedgerId is null,
            InterestVoteId = vote?.Id,
            ProvisionalLedgerId = provisionalLedgerId,
            ParticipantCount = vote?.TotalVoteCount ?? 0,
            StartEndpoint = $"/api/v1/community/posts/{source.PostId}/opportunities/participation/start",
            JoinEndpoint = vote is null ? string.Empty : $"/api/v1/community/votes/{vote.Id:D}/votes",
            ProvisionalLedgerEndpoint = vote is null
                ? string.Empty
                : $"/api/v1/community/posts/{source.PostId}/opportunities/participation/provisional-ledger",
            PlanningSourceReferenceId = vote?.Id.ToString("D") ?? string.Empty,
            RoleOptions = BuildRoleDefinitions(language).Select(role =>
            {
                var option = vote?.Options.FirstOrDefault(candidate => string.Equals(
                    candidate.ProductKey,
                    RoleProductKey(role.RoleCode),
                    StringComparison.OrdinalIgnoreCase));
                return new CommunityPostParticipationRoleOptionResponse
                {
                    RoleCode = role.RoleCode,
                    OptionId = option?.OptionId ?? string.Empty,
                    Label = role.Label,
                    Summary = role.Summary,
                    InterestCount = option?.VoteCount ?? 0
                };
            }).ToArray(),
            ProfessionalParticipation = CommunityPostProfessionalParticipationProjection.BuildResponse(
                provisionalLedger,
                source.PostId,
                language),
            PartyFormation = CommunityPostProfessionalParticipationProjection.BuildPartyFormationResponse(
                provisionalLedger,
                language)
        };
    }

    private static 커뮤니티원장저장요청 BuildProvisionalLedgerRequest(
        CommunityPostOpportunitySource source,
        CommunityInterestVotePromotionSnapshot snapshot,
        string ledgerId,
        string intentTypeCode,
        string tradeDirectionCode,
        string originCountryCode,
        string destinationCountryCode,
        IReadOnlyList<string> transportModeCodes,
        string actorUserId,
        string actorDisplayName,
        IReadOnlyList<string> authorProfessionalRoles)
    {
        var normalizedActorDisplayName = string.IsNullOrWhiteSpace(actorDisplayName)
            ? "게시글 작성자"
            : actorDisplayName.Trim();
        var participants = snapshot.Participants
            .Select(participant => new 커뮤니티원장참여자Dto
            {
                UserId = participant.UserId,
                DisplayName = participant.DisplayName,
                RoleLabel = participant.RoleCodes.Count == 0
                    ? "관심 참여자"
                    : string.Join(", ", participant.RoleCodes),
                ParticipationState = "비구속 관심표시"
            })
            .ToList();
        if (!participants.Any(participant => string.Equals(
                participant.UserId,
                actorUserId,
                StringComparison.OrdinalIgnoreCase)))
        {
            participants.Add(new 커뮤니티원장참여자Dto
            {
                UserId = actorUserId,
                DisplayName = normalizedActorDisplayName,
                RoleLabel = CommunityPostParticipationRoleCodes.Facilitator,
                ParticipationState = "가원장 발의"
            });
        }

        var professionalAssignments = authorProfessionalRoles
            .Select(roleCode => new CommunityPartyRoleAssignment
            {
                UserId = actorUserId,
                DisplayName = normalizedActorDisplayName,
                RoleCode = roleCode,
                SourceCode = CommunityPartyRoleAssignmentSourceCodes.Author,
                VerificationScopeCode = CommunityPartyRoleConfirmationScopeCodes.PlatformProfileOnly
            })
            .ToArray();
        if (professionalAssignments.Length > 0)
        {
            var actorIndex = participants.FindIndex(participant => string.Equals(
                participant.UserId,
                actorUserId,
                StringComparison.OrdinalIgnoreCase));
            var existing = participants[actorIndex];
            var professionalLabels = professionalAssignments
                .Select(assignment => CommunityPostProfessionalParticipationProjection.RoleLabel(
                    assignment.RoleCode,
                    false));
            participants[actorIndex] = new 커뮤니티원장참여자Dto
            {
                UserId = existing.UserId,
                DisplayName = existing.DisplayName,
                RoleLabel = $"{existing.RoleLabel} | 플랫폼 역할 확인 · {string.Join(", ", professionalLabels)}",
                ParticipationState = "가원장 발의·역할 참여"
            };
        }

        var roleCountsJson = JsonSerializer.Serialize(snapshot.RoleCounts);
        var requiredProfessionalRoles = CommunityPostPartyRoleCodes
            .ForPlan(tradeDirectionCode, transportModeCodes)
            .Where(CommunityPostPartyRoleCodes.IsSpecialist)
            .ToArray();
        var professionalAssignmentsJson = CommunityPostProfessionalParticipationProjection.SerializeAssignments(
            professionalAssignments);
        var initialMomentumCode = professionalAssignments.Length == 0
            ? CommunityPostMomentumCodes.SeekingParty
            : CommunityPostMomentumCodes.PartyForming;
        var title = $"[가원장] {source.Title}";
        if (title.Length > 180)
        {
            title = title[..180];
        }

        return new 커뮤니티원장저장요청
        {
            원장Id = ledgerId,
            기대Revision = 0,
            커뮤니티Id = source.AppKey,
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupPurchase,
            제목 = title,
            원함 = intentTypeCode switch
            {
                CommunityCollectiveIntentTypeCodes.GroupImportCandidate =>
                    "공동수입 가능성과 당사자·통관·운송 조건을 비구속적으로 함께 검토합니다.",
                CommunityCollectiveIntentTypeCodes.GroupExportCandidate =>
                    "공동수출 가능성과 당사자·통관·운송 조건을 비구속적으로 함께 검토합니다.",
                _ => "공동구매 가능성과 조건을 비구속적으로 함께 검토합니다."
            },
            상태 = 커뮤니티원장상태.초안,
            현재단계Key = CommunityGroupPurchaseLedgerStageCodes.Proposal,
            대상OsCode = CommunityLedgerOperatingSystemCodes.CommunityTrust,
            대상OsName = "커뮤니티 신뢰 OS",
            생성자UserId = actorUserId,
            생성자표시명 = normalizedActorDisplayName,
            참여자목록 = participants,
            블록목록 =
            [
                new 커뮤니티원장블록Dto
                {
                    BlockId = "non-binding-interest-evidence",
                    BlockType = CommunityLedgerBlockTypes.Generic,
                    Title = "비구속적 관심 모임 증빙",
                    State = "기록됨",
                    Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["SourcePostId"] = source.PostId.ToString(),
                        ["InterestVoteId"] = snapshot.VoteId.ToString("D"),
                        ["ParticipantCount"] = snapshot.ParticipantCount.ToString(),
                        ["RoleCountsJson"] = roleCountsJson,
                        ["EvidenceSnapshotHash"] = snapshot.EvidenceSnapshotHash,
                        ["LegalEffectNotice"] = "이 가원장은 관심이 모였다는 사실만 기록하며 주문, 계약, 결제, 배차 또는 운송 주선을 확정하지 않습니다."
                    }
                },
                CommunityPostProfessionalParticipationProjection.BuildProfessionalBlock(
                    requiredProfessionalRoles,
                    professionalAssignments)
            ],
            외부참조 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["SourceCommunityPostId"] = source.PostId.ToString(),
                ["SourceInterestVoteId"] = snapshot.VoteId.ToString("D")
            },
            확장속성 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey] = CommunityPostProvisionalLedgerPolicy.LedgerMaturityCode,
                [CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey] = CommunityPostProvisionalLedgerPolicy.NonBindingEffectCode,
                [CommunityPostProvisionalLedgerPolicy.CollectiveIntentTypeAttributeKey] = intentTypeCode,
                [CommunityPostProvisionalLedgerPolicy.TradeDirectionAttributeKey] = tradeDirectionCode,
                [CommunityPostProvisionalLedgerPolicy.OriginCountryAttributeKey] = originCountryCode,
                [CommunityPostProvisionalLedgerPolicy.DestinationCountryAttributeKey] = destinationCountryCode,
                [CommunityPostProvisionalLedgerPolicy.TransportModesAttributeKey] = JsonSerializer.Serialize(transportModeCodes),
                [CommunityPostProvisionalLedgerPolicy.EvidenceSnapshotHashAttributeKey] = snapshot.EvidenceSnapshotHash,
                [CommunityPostProvisionalLedgerPolicy.ParticipantNotificationsAttributeKey] = bool.TrueString,
                [CommunityPostProvisionalLedgerPolicy.RequiredProfessionalRolesAttributeKey] = JsonSerializer.Serialize(requiredProfessionalRoles),
                [CommunityPostProvisionalLedgerPolicy.ConfirmedPartyRoleAssignmentsAttributeKey] = professionalAssignmentsJson,
                [CommunityPostProvisionalLedgerPolicy.ConfirmedPartyRoleParticipantCountAttributeKey] = professionalAssignments.Length == 0 ? "0" : "1",
                [CommunityPostProvisionalLedgerPolicy.AuthorProfessionalRolesAttributeKey] = JsonSerializer.Serialize(authorProfessionalRoles),
                [CommunityPostProvisionalLedgerPolicy.CommunityMomentumCodeAttributeKey] = initialMomentumCode,
                [CommunityPostProvisionalLedgerPolicy.CommunityPromotionRequestedAttributeKey] = bool.TrueString,
                ["InterestParticipantCount"] = snapshot.ParticipantCount.ToString(),
                ["InterestRoleCountsJson"] = roleCountsJson
            }
        };
    }

    private static string NormalizeTradeDirectionCode(string intentTypeCode, string? requestedCode)
    {
        var expectedCode = CommunityTradeDirectionCodes.ExpectedForIntent(intentTypeCode);
        if (string.IsNullOrWhiteSpace(requestedCode))
        {
            return expectedCode;
        }

        if (!CommunityTradeDirectionCodes.IsSupported(requestedCode))
        {
            throw new InvalidOperationException("지원하지 않는 거래 방향입니다.");
        }

        var normalized = CommunityTradeDirectionCodes.All.First(code => string.Equals(
            code,
            requestedCode.Trim(),
            StringComparison.OrdinalIgnoreCase));
        if (!string.Equals(normalized, expectedCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("가원장 의도와 거래 방향이 일치하지 않습니다.");
        }

        return normalized;
    }

    private static string NormalizeCountryCode(string? value, string fieldLabel)
    {
        var normalized = CommunityGroupPurchaseTradeRoutePolicy.NormalizeCountryCode(value);
        if (!string.IsNullOrWhiteSpace(normalized)
            && !CommunityGroupPurchaseTradeRoutePolicy.IsValidCountryCode(normalized))
        {
            throw new InvalidOperationException($"{fieldLabel} 코드는 ISO 알파-2 두 자리로 입력해 주세요.");
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeTransportModeCodes(IEnumerable<string>? values)
    {
        var requested = (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
        var normalized = CommunityTransportModeCodes.NormalizeMany(requested);
        if (normalized.Count != requested
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count())
        {
            throw new InvalidOperationException("지원하지 않는 운송수단이 포함되어 있습니다.");
        }

        return normalized;
    }

    private static bool RouteValueMatches(string requestedValue, string storedValue)
        => string.IsNullOrWhiteSpace(requestedValue)
           || string.Equals(requestedValue, storedValue, StringComparison.OrdinalIgnoreCase);

    private static string BuildParticipationTitle(
        CommunityPostOpportunitySource source,
        string? requestedTitle,
        string language)
    {
        var title = string.IsNullOrWhiteSpace(requestedTitle)
            ? string.Equals(language, CommunityDisplayLanguageCodes.English, StringComparison.OrdinalIgnoreCase)
                ? $"Join the conversation: {source.Title}"
                : $"함께 해보기: {source.Title}"
            : requestedTitle.Trim();
        return title.Length <= 180 ? title : title[..180];
    }

    private static IReadOnlyList<ParticipationRoleDefinition> BuildRoleDefinitions(string language)
    {
        var english = string.Equals(language, CommunityDisplayLanguageCodes.English, StringComparison.OrdinalIgnoreCase);
        return english
            ?
            [
                new(CommunityPostParticipationRoleCodes.Buyer, "Interested buyer", "I may join the purchase or import."),
                new(CommunityPostParticipationRoleCodes.Supplier, "Potential supplier", "I may be able to supply the product."),
                new(CommunityPostParticipationRoleCodes.FreightBroker, "Broker or forwarder interest", "I may join subject to separate authority and license verification."),
                new(CommunityPostParticipationRoleCodes.Carrier, "Carrier", "I may provide transportation."),
                new(CommunityPostParticipationRoleCodes.CustomsBroker, "Customs professional interest", "I may help review customs questions before separate credential verification and engagement."),
                new(CommunityPostParticipationRoleCodes.WarehouseOperator, "Warehouse operator", "I may provide storage or handling."),
                new(CommunityPostParticipationRoleCodes.Facilitator, "Conversation facilitator", "I may help participants organize the discussion."),
                new(CommunityPostParticipationRoleCodes.FollowOnly, "Follow this", "I only want to follow the conversation for now.")
            ]
            :
            [
                new(CommunityPostParticipationRoleCodes.Buyer, "구매에 관심 있어요", "공동구매나 공동수입에 참여할 수 있어요."),
                new(CommunityPostParticipationRoleCodes.Supplier, "공급할 수 있어요", "상품 공급 가능성을 함께 검토할 수 있어요."),
                new(CommunityPostParticipationRoleCodes.FreightBroker, "운송 주선 검토로 도울 수 있어요", "관할 면허·등록을 별도로 확인한 뒤 가능한 범위에서 참여할 수 있어요."),
                new(CommunityPostParticipationRoleCodes.Carrier, "운송할 수 있어요", "운송 업무 제공 가능성을 검토할 수 있어요."),
                new(CommunityPostParticipationRoleCodes.CustomsBroker, "통관 검토로 도울 수 있어요", "자격과 수임을 별도로 확인하기 전 관세·통관 쟁점 검토에 관심을 표시해요."),
                new(CommunityPostParticipationRoleCodes.WarehouseOperator, "보관·하역으로 도울 수 있어요", "창고 보관이나 현장 작업을 제공할 수 있어요."),
                new(CommunityPostParticipationRoleCodes.Facilitator, "대화를 정리할 수 있어요", "참여자들의 의견과 다음 단계를 정리할 수 있어요."),
                new(CommunityPostParticipationRoleCodes.FollowOnly, "일단 지켜볼게요", "아직 약속하지 않고 대화만 이어서 볼게요.")
            ];
    }

    private static string RoleProductKey(string roleCode)
        => $"community-role:{roleCode}";

    private static string RequireActor(string? actorUserId)
        => string.IsNullOrWhiteSpace(actorUserId)
            ? throw new UnauthorizedAccessException("로그인 사용자 식별자를 확인할 수 없습니다.")
            : actorUserId.Trim();

    private static void EnsureCollectiveActionAllowed(CommunityPostOpportunitySource source)
    {
        if (source.IsReportBoardPost)
        {
            throw new InvalidOperationException("신고·분쟁 게시글에서는 관심 모집, 가원장 또는 거래 역할 참여를 시작할 수 없습니다.");
        }
    }

    private sealed record ParticipationRoleDefinition(
        string RoleCode,
        string Label,
        string Summary);
}

public sealed class CommunityPostOpportunityConflictException : Exception
{
    public CommunityPostOpportunityConflictException(string message)
        : base(message)
    {
    }
}

public static class CommunityPostProvisionalLedgerIds
{
    public static string FromInterestVote(long postId, Guid voteId)
    {
        if (postId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(postId));
        }

        if (voteId == Guid.Empty)
        {
            throw new ArgumentException("관심 투표 ID가 필요합니다.", nameof(voteId));
        }

        return $"community-post-{postId}-interest-{voteId:N}";
    }
}
