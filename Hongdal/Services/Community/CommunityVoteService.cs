using System.Security.Cryptography;
using System.Text;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.ContractManagement;

namespace Hongdal.Services.Community;

public interface ICommunityVoteService
{
    Task<CommunityVoteResponse> CreateAsync(CommunityVoteCreateRequest request, CancellationToken cancellationToken);

    Task<CommunityVoteListResponse> ListAsync(
        string? appKey,
        string? communityScope,
        string? hsCode,
        CancellationToken cancellationToken);

    Task<CommunityVoteResponse?> GetAsync(Guid voteId, CancellationToken cancellationToken);

    Task<CommunityVoteResponse?> CastVoteAsync(Guid voteId, CommunityVoteCastRequest request, CancellationToken cancellationToken);

    Task<CommunityVoteResponse?> CloseAsync(Guid voteId, CommunityVoteCloseRequest request, CancellationToken cancellationToken);

    Task<CommunityVoteResolutionDocumentResponse?> CreateResolutionDraftAsync(
        Guid voteId,
        CommunityVoteResolutionDraftRequest request,
        CancellationToken cancellationToken);

    Task<CommunityVoteResolutionDocumentResponse?> SignResolutionAsync(
        Guid voteId,
        CommunityVoteResolutionSignRequest request,
        CancellationToken cancellationToken);

    Task<CommunityVoteResolutionDocumentResponse?> MarkResolutionReadyToSignAsync(
        Guid voteId,
        CommunityVoteResolutionReadyToSignRequest request,
        CancellationToken cancellationToken);
}

internal static class CommunityVoteHsCode
{
    public static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = new string(value.Where(char.IsDigit).ToArray());
        if (normalized.Length is < 2 or > 10)
        {
            throw new InvalidOperationException("HS 코드는 구분기호를 제외한 2~10자리 숫자로 입력해야 합니다.");
        }

        return normalized;
    }

    public static bool MatchesPrefix(string? storedValue, string normalizedPrefix)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return false;
        }

        var storedCode = new string(storedValue.Where(char.IsDigit).ToArray());
        return storedCode.StartsWith(normalizedPrefix, StringComparison.Ordinal);
    }

    public static string PrefixRegex(string normalizedPrefix)
        => $"^{string.Join("[^0-9]*", normalizedPrefix.Select(character => character.ToString()))}";
}

public class CommunityVoteService : ICommunityVoteService
{
    private readonly ICommunityVoteStore _store;
    private readonly ICommunityGroupPurchaseDemandOutboxProcessor _demandOutboxProcessor;

    internal CommunityVoteService(
        ICommunityVoteStore store,
        ICommunityGroupPurchaseDemandOutboxProcessor demandOutboxProcessor)
    {
        _store = store;
        _demandOutboxProcessor = demandOutboxProcessor;
    }

    public async Task<CommunityVoteResponse> CreateAsync(CommunityVoteCreateRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("투표 제목이 필요합니다.");
        }

        if (request.Options.Count > 0 && request.StructuredOptions.Count > 0)
        {
            throw new InvalidOperationException("문자열 선택지와 구조화 선택지는 동시에 지정할 수 없습니다.");
        }

        var options = request.StructuredOptions.Count > 0
            ? request.StructuredOptions
                .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                .Select((option, index) => new CommunityVoteOptionRecord
                {
                    OptionId = $"option-{index + 1}",
                    Text = option.Text.Trim(),
                    ProductKey = NormalizeOptional(option.ProductKey) ?? string.Empty,
                    HsCode = NormalizeOptional(option.HsCode) ?? string.Empty,
                    TemperatureCode = NormalizeOptional(option.TemperatureCode) ?? string.Empty,
                    LogisticsMode = NormalizeOptional(option.LogisticsMode) ?? string.Empty,
                    QuantityUnit = NormalizeOptional(option.QuantityUnit) ?? string.Empty
                })
                .ToArray()
            : request.Options
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select((text, index) => new CommunityVoteOptionRecord
                {
                    OptionId = $"option-{index + 1}",
                    Text = text.Trim()
                })
                .ToArray();
        if (options.Length < 2)
        {
            throw new InvalidOperationException("투표 선택지는 2개 이상이어야 합니다.");
        }

        var voteKind = Normalize(request.VoteKind, CommunityVoteKindCodes.General);
        if (voteKind is not CommunityVoteKindCodes.General and not CommunityVoteKindCodes.GroupPurchaseDemand)
        {
            throw new InvalidOperationException("지원하지 않는 투표 유형입니다.");
        }

        if (request.ClosesAtUtc is not null && request.ClosesAtUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("투표 마감 시각은 현재보다 이후여야 합니다.");
        }

        var groupPurchase = CreateGroupPurchaseSettings(request, voteKind);

        var vote = new CommunityVoteRecord
        {
            Id = Guid.NewGuid(),
            AppKey = Normalize(request.AppKey, "platform"),
            CommunityScope = Normalize(request.CommunityScope, "platform"),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            VoteKind = voteKind,
            SourcePostId = request.SourcePostId,
            CommunityLedgerId = NormalizeOptional(request.CommunityLedgerId),
            AllowMultipleSelection = request.AllowMultipleSelection,
            ResolutionDocumentEnabled = request.ResolutionDocumentEnabled,
            SignatureRequired = request.SignatureRequired,
            CreatedByDisplayName = Normalize(request.CreatedByDisplayName, "익명 참여자"),
            CreatedAtUtc = DateTime.UtcNow,
            ClosesAtUtc = request.ClosesAtUtc,
            Options = options,
            GroupPurchase = groupPurchase
        };

        await _store.AddAsync(vote, cancellationToken);
        return ToResponse(vote);
    }

    public async Task<CommunityVoteListResponse> ListAsync(
        string? appKey,
        string? communityScope,
        string? hsCode,
        CancellationToken cancellationToken)
    {
        var normalizedHsCode = CommunityVoteHsCode.NormalizeOptional(hsCode);
        var items = await _store.ListAsync(appKey, communityScope, normalizedHsCode, cancellationToken);
        return new CommunityVoteListResponse
        {
            Items = items.Select(ToResponse).ToArray()
        };
    }

    public async Task<CommunityVoteResponse?> GetAsync(Guid voteId, CancellationToken cancellationToken)
    {
        var vote = await _store.GetAsync(voteId, cancellationToken);
        return vote is null ? null : ToResponse(vote);
    }

    public async Task<CommunityVoteResponse?> CastVoteAsync(Guid voteId, CommunityVoteCastRequest request, CancellationToken cancellationToken)
    {
        var vote = await _store.GetAsync(voteId, cancellationToken);
        if (vote is null)
        {
            return null;
        }

        EnsureOpen(vote);
        var selectedOptionIds = request.OptionIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (selectedOptionIds.Length == 0)
            {
                throw new InvalidOperationException("선택한 투표 항목이 없습니다.");
            }

            if (!vote.AllowMultipleSelection && selectedOptionIds.Length > 1)
            {
                throw new InvalidOperationException("이 투표는 하나의 항목만 선택할 수 있습니다.");
            }

            var validOptionIds = vote.Options.Select(x => x.OptionId).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (selectedOptionIds.Any(x => !validOptionIds.Contains(x)))
            {
                throw new InvalidOperationException("존재하지 않는 투표 항목이 포함되어 있습니다.");
            }

        var voterHash = Hash(Normalize(request.VoterKey, request.VoterDisplayName));
        var groupPurchaseParticipation = ValidateGroupPurchaseParticipation(vote, request, voterHash);
        vote.Votes.RemoveAll(x => string.Equals(x.VoterHash, voterHash, StringComparison.Ordinal));
        vote.Votes.Add(new CommunityVoteCastRecord
        {
            VoterHash = voterHash,
            VoterDisplayName = Normalize(request.VoterDisplayName, "익명 참여자"),
            OptionIds = selectedOptionIds,
            RequestedQuantity = groupPurchaseParticipation.RequestedQuantity,
            ParticipationMethodCode = groupPurchaseParticipation.ParticipationMethodCode,
            PickupPointId = groupPurchaseParticipation.PickupPointId,
            AllowNearbyPickupPointFallback = request.AllowNearbyPickupPointFallback,
            VotedAtUtc = DateTime.UtcNow
        });

        var demandOutboxId = QueueGroupPurchaseDemand(
            vote,
            request,
            selectedOptionIds[0],
            voterHash,
            groupPurchaseParticipation);
        await SaveMutationAsync(vote, cancellationToken);
        if (demandOutboxId is not null)
        {
            await _demandOutboxProcessor.ProcessAsync(vote.Id, demandOutboxId, cancellationToken);
            vote = await _store.GetAsync(vote.Id, cancellationToken) ?? vote;
        }

        return ToResponse(vote);
    }

    public async Task<CommunityVoteResponse?> CloseAsync(Guid voteId, CommunityVoteCloseRequest request, CancellationToken cancellationToken)
    {
        var vote = await _store.GetAsync(voteId, cancellationToken);
        if (vote is null)
        {
            return null;
        }

        vote.Status = CommunityVoteStatusCodes.Closed;
        vote.ClosedAtUtc = DateTime.UtcNow;
        vote.ClosedByDisplayName = Normalize(request.ClosedByDisplayName, "운영자");
        await SaveMutationAsync(vote, cancellationToken);
        return ToResponse(vote);
    }

    public async Task<CommunityVoteResolutionDocumentResponse?> CreateResolutionDraftAsync(
        Guid voteId,
        CommunityVoteResolutionDraftRequest request,
        CancellationToken cancellationToken)
    {
        var vote = await _store.GetAsync(voteId, cancellationToken);
        if (vote is null)
        {
            return null;
        }

        if (!vote.ResolutionDocumentEnabled)
        {
            throw new InvalidOperationException("이 투표는 결의문 생성을 사용하지 않습니다.");
        }

        if (vote.Status == CommunityVoteStatusCodes.Open)
        {
            throw new InvalidOperationException("투표를 마감한 뒤 결의문을 만들 수 있습니다.");
        }

        var requiredSigners = request.RequiredSigners;
        if (requiredSigners.Count == 0
            && vote.SignatureRequired
            && vote.GroupPurchase is not null)
        {
            requiredSigners = vote.Votes
                .OrderBy(x => x.VotedAtUtc)
                .Select(x => new CommunityVoteResolutionSignerRequest
                {
                    PartyId = x.VoterHash,
                    RoleCode = "GroupPurchaseParticipant",
                    SignerDisplayName = x.VoterDisplayName
                })
                .ToArray();
        }

        if (requiredSigners.Count == 0 && vote.SignatureRequired)
        {
            throw new InvalidOperationException("서명 필수 결의문은 서명 요청 대상이 필요합니다.");
        }

        if (!request.LegalReviewRequested && vote.SignatureRequired)
        {
            EnsureGroupImportContractReady(vote);
        }

        var legalEffectNotice = BuildLegalEffectNotice(vote);
        var documentText = BuildDocumentText(vote, request, legalEffectNotice);
        var documentHash = Hash(documentText);
        var documentNumber = $"COMM-VOTE-{DateTime.UtcNow:yyyyMMdd}-{vote.Id:N}"[..42];
        var signatureBundle = requiredSigners.Count == 0
            ? null
            : ContractElectronicSignaturePlanner.CreateBundle(
                documentNumber,
                documentHash,
                requiredSigners.Select(x => new ContractSignatureRequest(
                    x.PartyId,
                    x.RoleCode,
                    x.SignerDisplayName,
                    IsRequiredSigner: true,
                    DateTimeOffset.UtcNow)),
                DateTimeOffset.UtcNow);

        vote.Status = CommunityVoteStatusCodes.ResolutionDrafted;
        vote.ResolutionDocument = new CommunityVoteResolutionDocumentRecord
        {
            Id = Guid.NewGuid(),
            VoteId = vote.Id,
            DocumentNumber = documentNumber,
            DocumentTitle = Normalize(request.DocumentTitle, $"{vote.Title} 결의문"),
            ResolutionText = Normalize(request.ResolutionText, "투표 결과에 따른 커뮤니티 결의 초안입니다."),
            DocumentHash = documentHash,
            Status = request.LegalReviewRequested
                ? CommunityVoteResolutionStatusCodes.LegalReviewRequired
                : vote.SignatureRequired
                    ? CommunityVoteResolutionStatusCodes.ReadyToSign
                    : CommunityVoteResolutionStatusCodes.Draft,
            LegalEffectNotice = legalEffectNotice,
            CreatedAtUtc = DateTime.UtcNow,
            SignatureBundle = signatureBundle
        };

        await SaveMutationAsync(vote, cancellationToken);
        return ToResolutionResponse(vote.ResolutionDocument);
    }

    public async Task<CommunityVoteResolutionDocumentResponse?> SignResolutionAsync(
        Guid voteId,
        CommunityVoteResolutionSignRequest request,
        CancellationToken cancellationToken)
    {
        var vote = await _store.GetAsync(voteId, cancellationToken);
        var document = vote?.ResolutionDocument;
        if (document?.SignatureBundle is null)
        {
            return null;
        }

        if (document.Status == CommunityVoteResolutionStatusCodes.LegalReviewRequired)
        {
            throw new InvalidOperationException("법무/운영 검토가 필요한 결의문은 서명 가능 상태로 전환한 뒤 서명해야 합니다.");
        }

        EnsureGroupImportContractReady(vote!);

        var evidence = new ContractSignatureEvidence(
            request.PartyId,
            Normalize(request.SignerDisplayName, "익명 참여자"),
            Normalize(request.SignatureMethodCode, ContractSignatureMethodCode.PlatformClickSign),
            document.DocumentHash,
            Hash(request.ConsentText),
            Hash(request.SignatureEvidencePayload),
            DateTimeOffset.UtcNow,
            request.ClientIpHash);
        document.SignatureBundle = ContractElectronicSignaturePlanner.AddEvidence(document.SignatureBundle, evidence);

        var plan = ContractElectronicSignaturePlanner.Plan(document.SignatureBundle, DateTimeOffset.UtcNow);
        document.Status = plan.IsFullySigned
            ? CommunityVoteResolutionStatusCodes.Signed
            : CommunityVoteResolutionStatusCodes.PartiallySigned;

        await SaveMutationAsync(vote!, cancellationToken);
        return ToResolutionResponse(document);
    }

    public async Task<CommunityVoteResolutionDocumentResponse?> MarkResolutionReadyToSignAsync(
        Guid voteId,
        CommunityVoteResolutionReadyToSignRequest request,
        CancellationToken cancellationToken)
    {
        var vote = await _store.GetAsync(voteId, cancellationToken);
        var document = vote?.ResolutionDocument;
        if (document is null)
        {
            return null;
        }

        if (document.SignatureBundle is null)
        {
            document.Status = CommunityVoteResolutionStatusCodes.Draft;
        }
        else
        {
            EnsureGroupImportContractReady(vote!);
            document.Status = CommunityVoteResolutionStatusCodes.ReadyToSign;
        }

        document.LegalEffectNotice = $"{BuildLegalEffectNotice(vote!)} 검토자: {Normalize(request.ReviewedByDisplayName, "운영자")}.";
        await SaveMutationAsync(vote!, cancellationToken);
        return ToResolutionResponse(document);
    }

    private async Task SaveMutationAsync(CommunityVoteRecord vote, CancellationToken cancellationToken)
    {
        var expectedRevision = vote.Revision;
        vote.Revision++;
        if (!await _store.ReplaceAsync(vote, expectedRevision, cancellationToken))
        {
            throw new InvalidOperationException("다른 참여자가 투표를 먼저 변경했습니다. 최신 결과를 확인한 뒤 다시 시도해 주세요.");
        }
    }

    private static void EnsureOpen(CommunityVoteRecord vote)
    {
        if (vote.Status != CommunityVoteStatusCodes.Open || vote.ClosesAtUtc is not null && vote.ClosesAtUtc <= DateTime.UtcNow)
        {
            vote.Status = CommunityVoteStatusCodes.Closed;
            vote.ClosedAtUtc ??= DateTime.UtcNow;
            throw new InvalidOperationException("마감된 투표입니다.");
        }
    }

    private static CommunityVoteResponse ToResponse(CommunityVoteRecord vote)
    {
        var counts = vote.Votes
            .SelectMany(x => x.OptionIds)
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);
        var requestedQuantities = vote.Votes
            .SelectMany(voteCast => voteCast.OptionIds.Select(optionId => new
            {
                OptionId = optionId,
                voteCast.RequestedQuantity
            }))
            .GroupBy(x => x.OptionId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.RequestedQuantity), StringComparer.OrdinalIgnoreCase);
        var maxCount = counts.Count == 0 ? 0 : counts.Values.Max();

        return new CommunityVoteResponse
        {
            Id = vote.Id,
            AppKey = vote.AppKey,
            CommunityScope = vote.CommunityScope,
            Title = vote.Title,
            Description = vote.Description,
            VoteKind = vote.VoteKind,
            SourcePostId = vote.SourcePostId,
            CommunityLedgerId = vote.CommunityLedgerId,
            Status = vote.Status,
            AllowMultipleSelection = vote.AllowMultipleSelection,
            ResolutionDocumentEnabled = vote.ResolutionDocumentEnabled,
            SignatureRequired = vote.SignatureRequired,
            CreatedByDisplayName = vote.CreatedByDisplayName,
            CreatedAtUtc = vote.CreatedAtUtc,
            ClosesAtUtc = vote.ClosesAtUtc,
            ClosedAtUtc = vote.ClosedAtUtc,
            TotalVoteCount = vote.Votes.Count,
            Options = vote.Options.Select(x =>
            {
                counts.TryGetValue(x.OptionId, out var count);
                requestedQuantities.TryGetValue(x.OptionId, out var requestedQuantity);
                return new CommunityVoteOptionResponse
                {
                    OptionId = x.OptionId,
                    Text = x.Text,
                    ProductKey = x.ProductKey,
                    HsCode = x.HsCode,
                    TemperatureCode = x.TemperatureCode,
                    LogisticsMode = x.LogisticsMode,
                    QuantityUnit = x.QuantityUnit,
                    VoteCount = count,
                    RequestedQuantity = requestedQuantity,
                    IsWinningOption = count > 0 && count == maxCount
                };
            }).ToArray(),
            GroupPurchase = ToGroupPurchaseResponse(vote),
            ResolutionDocument = vote.ResolutionDocument is null ? null : ToResolutionResponse(vote.ResolutionDocument)
        };
    }

    private static CommunityGroupPurchaseVoteSettingsRecord? CreateGroupPurchaseSettings(
        CommunityVoteCreateRequest request,
        string voteKind)
    {
        if (voteKind == CommunityVoteKindCodes.General)
        {
            if (request.GroupPurchase is not null)
            {
                throw new InvalidOperationException("일반 투표에는 공동구매 설정을 지정할 수 없습니다.");
            }

            return null;
        }

        if (request.AllowMultipleSelection)
        {
            throw new InvalidOperationException("공동구매 수요 투표의 첫 버전은 하나의 상품 선택지만 선택할 수 있습니다.");
        }

        var settings = request.GroupPurchase
            ?? throw new InvalidOperationException("공동구매 수요 투표 설정이 필요합니다.");
        var proposerRoleCode = Normalize(
            settings.ProposerRoleCode,
            CommunityGroupPurchaseProposerRoleCodes.GroupPurchaseRepresentative);
        if (!CommunityGroupPurchaseProposerRoleCodes.IsSupported(proposerRoleCode))
        {
            throw new InvalidOperationException("공동구매 제안 주체는 생산자 또는 공동구매 대표여야 합니다.");
        }

        var tradeRouteDecision = CommunityGroupPurchaseTradeRoutePolicy.Evaluate(
            new CommunityGroupPurchaseTradeRouteInput(
                settings.SellerCountryCode,
                settings.ShipFromCountryCode,
                settings.DeliveryCountryCode,
                settings.CustomsClearanceStatusCode));
        if (tradeRouteDecision.InvalidFieldCodes.Count > 0)
        {
            throw new InvalidOperationException(
                "판매자·상품 출발·배송 국가 코드는 ISO 알파-2 두 자리이고, 통관 상태는 지원하는 코드여야 합니다.");
        }

        var sellerCountryCode = CommunityGroupPurchaseTradeRoutePolicy.NormalizeCountryCode(
            settings.SellerCountryCode);
        var shipFromCountryCode = CommunityGroupPurchaseTradeRoutePolicy.NormalizeCountryCode(
            settings.ShipFromCountryCode);
        var deliveryCountryCode = CommunityGroupPurchaseTradeRoutePolicy.NormalizeCountryCode(
            settings.DeliveryCountryCode);
        var customsClearanceStatusCode = CommunityGroupPurchaseTradeRoutePolicy
            .NormalizeCustomsClearanceStatusCode(settings.CustomsClearanceStatusCode);
        var hasExplicitTradeRouteInput = !string.IsNullOrWhiteSpace(sellerCountryCode)
            || !string.IsNullOrWhiteSpace(shipFromCountryCode)
            || !string.IsNullOrWhiteSpace(deliveryCountryCode)
            || !string.Equals(
                customsClearanceStatusCode,
                CommunityGroupPurchaseCustomsClearanceStatusCodes.Unknown,
                StringComparison.OrdinalIgnoreCase);

        var policyCode = Normalize(settings.ParticipationPolicyCode, CommunityVoteParticipationPolicyCodes.Hybrid);
        if (policyCode is not CommunityVoteParticipationPolicyCodes.CommunityOnly
            and not CommunityVoteParticipationPolicyCodes.ServiceAreaOnly
            and not CommunityVoteParticipationPolicyCodes.PickupPoint
            and not CommunityVoteParticipationPolicyCodes.Hybrid)
        {
            throw new InvalidOperationException("지원하지 않는 공동구매 참여 정책입니다.");
        }

        if (settings.MinimumParticipantCount is < 1 or > 100_000)
        {
            throw new InvalidOperationException("최소 참여 인원은 1명 이상 100,000명 이하여야 합니다.");
        }

        if (settings.MinimumTotalQuantity is < 1 or > 1_000_000)
        {
            throw new InvalidOperationException("최소 주문 수량은 1개 이상 1,000,000개 이하여야 합니다.");
        }

        if (settings.TargetUnitPriceKrwPerKg is <= 0 or > 1_000_000_000m)
        {
            throw new InvalidOperationException("공동구매 목표단가는 0원/kg 초과 10억원/kg 이하여야 합니다.");
        }

        if (settings.RadiusMeters is < 100 or > 200_000)
        {
            throw new InvalidOperationException("생활권 반경은 100m 이상 200km 이하여야 합니다.");
        }

        var serviceAreaKey = NormalizeOptional(settings.ServiceAreaKey);
        if (policyCode is CommunityVoteParticipationPolicyCodes.ServiceAreaOnly or CommunityVoteParticipationPolicyCodes.Hybrid
            && serviceAreaKey is null)
        {
            throw new InvalidOperationException("생활권 참여 정책에는 서비스 지역 키가 필요합니다.");
        }

        var pickupPoints = settings.PickupPoints
            .Select((point, index) => CreatePickupPoint(point, index))
            .ToArray();
        if (policyCode is CommunityVoteParticipationPolicyCodes.PickupPoint or CommunityVoteParticipationPolicyCodes.Hybrid
            && pickupPoints.Length == 0)
        {
            throw new InvalidOperationException("픽업 참여 정책에는 공동수령 거점이 하나 이상 필요합니다.");
        }

        if (pickupPoints.Select(x => x.PickupPointId).Distinct(StringComparer.OrdinalIgnoreCase).Count() != pickupPoints.Length)
        {
            throw new InvalidOperationException("공동수령 거점 ID는 중복될 수 없습니다.");
        }

        return new CommunityGroupPurchaseVoteSettingsRecord
        {
            ProposerRoleCode = proposerRoleCode,
            AgreementPolicyCode = CommunityGroupPurchaseAgreementPolicy.PolicyCode,
            ProposalOriginLegalEffectNotice = CommunityGroupPurchaseAgreementPolicy.FullLegalEffectNotice,
            SellerCountryCode = sellerCountryCode,
            ShipFromCountryCode = shipFromCountryCode,
            DeliveryCountryCode = deliveryCountryCode,
            CustomsClearanceStatusCode = customsClearanceStatusCode,
            TradeRouteCode = hasExplicitTradeRouteInput
                ? tradeRouteDecision.RouteCode
                : string.Empty,
            ParticipationPolicyCode = policyCode,
            HsCode = NormalizeOptional(settings.HsCode) ?? string.Empty,
            TemperatureCode = Normalize(settings.TemperatureCode, "상온"),
            LogisticsMode = Normalize(settings.LogisticsMode, "LCL"),
            QuantityUnit = Normalize(settings.QuantityUnit, "개"),
            TargetUnitPriceKrwPerKg = settings.TargetUnitPriceKrwPerKg,
            ServiceAreaKey = serviceAreaKey ?? string.Empty,
            ServiceAreaLabel = Normalize(settings.ServiceAreaLabel, serviceAreaKey ?? string.Empty),
            RadiusMeters = settings.RadiusMeters,
            MinimumParticipantCount = settings.MinimumParticipantCount,
            MinimumTotalQuantity = settings.MinimumTotalQuantity,
            PickupPoints = pickupPoints
        };
    }

    private static CommunityVotePickupPointRecord CreatePickupPoint(
        CommunityVotePickupPointRequest request,
        int index)
    {
        var name = NormalizeOptional(request.Name)
            ?? throw new InvalidOperationException("공동수령 거점 이름이 필요합니다.");
        var addressSummary = NormalizeOptional(request.AddressSummary)
            ?? throw new InvalidOperationException("공동수령 거점의 주소 요약이 필요합니다.");
        if (request.Latitude is < -90 or > 90 || request.Longitude is < -180 or > 180)
        {
            throw new InvalidOperationException("공동수령 거점 좌표 범위가 올바르지 않습니다.");
        }

        if (request.PickupStartsAtUtc is not null
            && request.PickupEndsAtUtc is not null
            && request.PickupStartsAtUtc >= request.PickupEndsAtUtc)
        {
            throw new InvalidOperationException("픽업 종료 시각은 시작 시각보다 이후여야 합니다.");
        }

        if (request.CapacityQuantity is < 1)
        {
            throw new InvalidOperationException("공동수령 거점 보관 가능 수량은 1개 이상이어야 합니다.");
        }

        if (request.MinimumParticipantCount is < 1 || request.MinimumTotalQuantity is < 1)
        {
            throw new InvalidOperationException("거점별 최소 참여 인원과 수량은 1 이상이어야 합니다.");
        }

        if (request.CapacityQuantity is not null
            && request.MinimumTotalQuantity is not null
            && request.MinimumTotalQuantity > request.CapacityQuantity)
        {
            throw new InvalidOperationException("거점별 최소 수량은 보관 가능 수량을 초과할 수 없습니다.");
        }

        if (request.PickupFee < 0)
        {
            throw new InvalidOperationException("픽업 수수료는 0 이상이어야 합니다.");
        }

        var storageTypeCode = Normalize(request.StorageTypeCode, CommunityVotePickupStorageTypeCodes.Ambient);
        if (storageTypeCode is not CommunityVotePickupStorageTypeCodes.Ambient
            and not CommunityVotePickupStorageTypeCodes.Refrigerated
            and not CommunityVotePickupStorageTypeCodes.Frozen)
        {
            throw new InvalidOperationException("지원하지 않는 거점 보관 유형입니다.");
        }

        return new CommunityVotePickupPointRecord
        {
            PickupPointId = Normalize(request.PickupPointId, $"pickup-{index + 1}"),
            Name = name,
            AddressSummary = addressSummary,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            StorageTypeCode = storageTypeCode,
            PickupStartsAtUtc = request.PickupStartsAtUtc,
            PickupEndsAtUtc = request.PickupEndsAtUtc,
            CapacityQuantity = request.CapacityQuantity,
            MinimumParticipantCount = request.MinimumParticipantCount,
            MinimumTotalQuantity = request.MinimumTotalQuantity,
            PickupFee = request.PickupFee
        };
    }

    private static GroupPurchaseParticipation ValidateGroupPurchaseParticipation(
        CommunityVoteRecord vote,
        CommunityVoteCastRequest request,
        string voterHash)
    {
        var settings = vote.GroupPurchase;
        if (settings is null)
        {
            return new GroupPurchaseParticipation(1, string.Empty, null);
        }

        if (request.RequestedQuantity is < 1 or > 10_000)
        {
            throw new InvalidOperationException("희망 수량은 1개 이상 10,000개 이하여야 합니다.");
        }

        var methodCode = NormalizeOptional(request.ParticipationMethodCode)
            ?? throw new InvalidOperationException("공동구매 참여 방법을 선택해야 합니다.");
        var methodAllowed = settings.ParticipationPolicyCode switch
        {
            CommunityVoteParticipationPolicyCodes.CommunityOnly => methodCode == CommunityVoteParticipationMethodCodes.CommunityMember,
            CommunityVoteParticipationPolicyCodes.ServiceAreaOnly => methodCode == CommunityVoteParticipationMethodCodes.ServiceArea,
            CommunityVoteParticipationPolicyCodes.PickupPoint => methodCode == CommunityVoteParticipationMethodCodes.PickupPoint,
            CommunityVoteParticipationPolicyCodes.Hybrid => methodCode is CommunityVoteParticipationMethodCodes.CommunityMember
                or CommunityVoteParticipationMethodCodes.ServiceArea
                or CommunityVoteParticipationMethodCodes.PickupPoint,
            _ => false
        };
        if (!methodAllowed)
        {
            throw new InvalidOperationException("이 공동구매에서 허용하지 않는 참여 방법입니다.");
        }

        if (methodCode == CommunityVoteParticipationMethodCodes.CommunityMember
            && !string.Equals(request.CommunityMembershipReference?.Trim(), vote.CommunityScope, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("현재 커뮤니티의 확인된 구성원 참조가 필요합니다.");
        }

        if (methodCode == CommunityVoteParticipationMethodCodes.ServiceArea
            && !string.Equals(request.ServiceAreaReference?.Trim(), settings.ServiceAreaKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("공동구매 서비스 지역과 일치하는 생활권 참조가 필요합니다.");
        }

        var pickupPointId = NormalizeOptional(request.PickupPointId);
        var pickupPoint = pickupPointId is null
            ? null
            : settings.PickupPoints.FirstOrDefault(x => string.Equals(x.PickupPointId, pickupPointId, StringComparison.OrdinalIgnoreCase));
        if (pickupPointId is not null && pickupPoint is null)
        {
            throw new InvalidOperationException("선택한 공동수령 거점을 찾을 수 없습니다.");
        }

        if (methodCode == CommunityVoteParticipationMethodCodes.PickupPoint && pickupPoint is null)
        {
            throw new InvalidOperationException("픽업 참여자는 공동수령 거점을 선택해야 합니다.");
        }

        if (pickupPoint?.CapacityQuantity is int capacityQuantity)
        {
            var assignedQuantity = vote.Votes
                .Where(x => !string.Equals(x.VoterHash, voterHash, StringComparison.Ordinal))
                .Where(x => string.Equals(x.PickupPointId, pickupPoint.PickupPointId, StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.RequestedQuantity);
            if (assignedQuantity + request.RequestedQuantity > capacityQuantity)
            {
                throw new InvalidOperationException("선택한 공동수령 거점의 보관 가능 수량을 초과합니다.");
            }
        }

        return new GroupPurchaseParticipation(request.RequestedQuantity, methodCode, pickupPoint?.PickupPointId);
    }

    private static string? QueueGroupPurchaseDemand(
        CommunityVoteRecord vote,
        CommunityVoteCastRequest request,
        string optionId,
        string voterHash,
        GroupPurchaseParticipation participation)
    {
        var settings = vote.GroupPurchase;
        if (settings is null)
        {
            return null;
        }

        var option = vote.Options.Single(x => string.Equals(x.OptionId, optionId, StringComparison.OrdinalIgnoreCase));
        var pickupPoint = participation.PickupPointId is null
            ? null
            : settings.PickupPoints.Single(x => string.Equals(
                x.PickupPointId,
                participation.PickupPointId,
                StringComparison.OrdinalIgnoreCase));
        var deliveryScopeKey = pickupPoint is not null
            ? $"pickup-point:{pickupPoint.PickupPointId}"
            : participation.ParticipationMethodCode == CommunityVoteParticipationMethodCodes.ServiceArea
                ? settings.ServiceAreaKey
                : vote.CommunityScope;
        var deliveryScopeName = pickupPoint?.Name
            ?? (participation.ParticipationMethodCode == CommunityVoteParticipationMethodCodes.ServiceArea
                ? settings.ServiceAreaLabel
                : vote.CommunityScope);

        var handoffRequest = new CommunityGroupPurchaseDemandHandoffRequest
        {
            VoteId = vote.Id,
            SourcePostId = vote.SourcePostId,
            CommunityLedgerId = vote.CommunityLedgerId,
            VoterHash = voterHash,
            VoterDisplayName = Normalize(request.VoterDisplayName, "익명 참여자"),
            OptionId = option.OptionId,
            ProductKey = string.IsNullOrWhiteSpace(option.ProductKey)
                ? $"community-vote:{vote.Id:N}:{option.OptionId}"
                : option.ProductKey,
            ProductName = option.Text,
            HsCode = string.IsNullOrWhiteSpace(option.HsCode) ? settings.HsCode : option.HsCode,
            TemperatureCode = string.IsNullOrWhiteSpace(option.TemperatureCode) ? settings.TemperatureCode : option.TemperatureCode,
            LogisticsMode = string.IsNullOrWhiteSpace(option.LogisticsMode) ? settings.LogisticsMode : option.LogisticsMode,
            DeliveryScopeKey = deliveryScopeKey,
            DeliveryScopeName = deliveryScopeName,
            RequestedQuantity = participation.RequestedQuantity,
            QuantityUnit = string.IsNullOrWhiteSpace(option.QuantityUnit) ? settings.QuantityUnit : option.QuantityUnit,
            MinimumParticipantCount = pickupPoint?.MinimumParticipantCount ?? settings.MinimumParticipantCount,
            MinimumTotalQuantity = pickupPoint?.MinimumTotalQuantity ?? settings.MinimumTotalQuantity
        };
        var outboxId = $"community-vote:{vote.Id:N}:{voterHash}";
        vote.DemandHandoffOutbox.RemoveAll(x =>
            string.Equals(x.OutboxId, outboxId, StringComparison.Ordinal));
        vote.DemandHandoffOutbox.Add(new CommunityVoteDemandHandoffOutboxRecord
        {
            OutboxId = outboxId,
            Request = handoffRequest,
            Status = CommunityVoteDemandHandoffStatusCodes.Pending,
            UpdatedAtUtc = DateTime.UtcNow
        });
        return outboxId;
    }

    private static CommunityGroupPurchaseVoteResponse? ToGroupPurchaseResponse(CommunityVoteRecord vote)
    {
        var settings = vote.GroupPurchase;
        if (settings is null)
        {
            return null;
        }

        var totalRequestedQuantity = vote.Votes.Sum(x => x.RequestedQuantity);
        var unassignedVotes = vote.Votes.Where(x => x.PickupPointId is null).ToArray();
        var hasExplicitTradeRoute = !string.IsNullOrWhiteSpace(settings.TradeRouteCode);
        var tradeRouteDecision = hasExplicitTradeRoute
            ? CommunityGroupPurchaseTradeRoutePolicy.Evaluate(
                new CommunityGroupPurchaseTradeRouteInput(
                    settings.SellerCountryCode,
                    settings.ShipFromCountryCode,
                    settings.DeliveryCountryCode,
                    settings.CustomsClearanceStatusCode))
            : null;
        return new CommunityGroupPurchaseVoteResponse
        {
            ProposerRoleCode = CommunityGroupPurchaseProposerRoleCodes.IsSupported(settings.ProposerRoleCode)
                ? settings.ProposerRoleCode
                : CommunityGroupPurchaseProposerRoleCodes.GroupPurchaseRepresentative,
            AgreementPolicyCode = string.IsNullOrWhiteSpace(settings.AgreementPolicyCode)
                ? CommunityGroupPurchaseAgreementPolicy.PolicyCode
                : settings.AgreementPolicyCode,
            ProposalOriginLegalEffectNotice = string.IsNullOrWhiteSpace(settings.ProposalOriginLegalEffectNotice)
                ? CommunityGroupPurchaseAgreementPolicy.FullLegalEffectNotice
                : settings.ProposalOriginLegalEffectNotice,
            SellerCountryCode = settings.SellerCountryCode,
            ShipFromCountryCode = settings.ShipFromCountryCode,
            DeliveryCountryCode = settings.DeliveryCountryCode,
            CustomsClearanceStatusCode = settings.CustomsClearanceStatusCode,
            TradeRouteCode = tradeRouteDecision?.RouteCode ?? string.Empty,
            IsGroupImportCandidate = tradeRouteDecision?.IsGroupImportCandidate == true,
            RequiresTradeRouteReview = tradeRouteDecision?.RequiresManualReview == true,
            RecommendedLedgerTemplateKey = tradeRouteDecision?.IsGroupImportCandidate == true
                ? CommunityLedgerTemplateKeys.GroupImport
                : string.Empty,
            TradeRouteReasonCodes = tradeRouteDecision?.ReasonCodes ?? [],
            TradeRouteMissingFieldCodes = tradeRouteDecision?.MissingFieldCodes ?? [],
            TradeRouteInvalidFieldCodes = tradeRouteDecision?.InvalidFieldCodes ?? [],
            ParticipationPolicyCode = settings.ParticipationPolicyCode,
            HsCode = settings.HsCode,
            TemperatureCode = settings.TemperatureCode,
            LogisticsMode = settings.LogisticsMode,
            QuantityUnit = settings.QuantityUnit,
            TargetUnitPriceKrwPerKg = settings.TargetUnitPriceKrwPerKg,
            ServiceAreaKey = settings.ServiceAreaKey,
            ServiceAreaLabel = settings.ServiceAreaLabel,
            RadiusMeters = settings.RadiusMeters,
            MinimumParticipantCount = settings.MinimumParticipantCount,
            MinimumTotalQuantity = settings.MinimumTotalQuantity,
            TotalRequestedQuantity = totalRequestedQuantity,
            UnassignedPickupParticipantCount = unassignedVotes.Length,
            UnassignedPickupQuantity = unassignedVotes.Sum(x => x.RequestedQuantity),
            DemandHandoffPendingCount = vote.DemandHandoffOutbox.Count(x =>
                x.Status is CommunityVoteDemandHandoffStatusCodes.Pending
                    or CommunityVoteDemandHandoffStatusCodes.Processing
                    or CommunityVoteDemandHandoffStatusCodes.RetryPending),
            DemandHandoffFailedCount = vote.DemandHandoffOutbox.Count(x =>
                x.Status is CommunityVoteDemandHandoffStatusCodes.Failed),
            IsMinimumReached = vote.Votes.Count >= settings.MinimumParticipantCount
                && totalRequestedQuantity >= settings.MinimumTotalQuantity,
            PickupPoints = settings.PickupPoints.Select(point =>
            {
                var assignedVotes = vote.Votes
                    .Where(x => string.Equals(x.PickupPointId, point.PickupPointId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                var requestedQuantity = assignedVotes.Sum(x => x.RequestedQuantity);
                var minimumParticipantCount = point.MinimumParticipantCount ?? settings.MinimumParticipantCount;
                var minimumTotalQuantity = point.MinimumTotalQuantity ?? settings.MinimumTotalQuantity;
                return new CommunityVotePickupPointResponse
                {
                    PickupPointId = point.PickupPointId,
                    Name = point.Name,
                    AddressSummary = point.AddressSummary,
                    Latitude = point.Latitude,
                    Longitude = point.Longitude,
                    StorageTypeCode = point.StorageTypeCode,
                    PickupStartsAtUtc = point.PickupStartsAtUtc,
                    PickupEndsAtUtc = point.PickupEndsAtUtc,
                    CapacityQuantity = point.CapacityQuantity,
                    MinimumParticipantCount = point.MinimumParticipantCount,
                    MinimumTotalQuantity = point.MinimumTotalQuantity,
                    PickupFee = point.PickupFee,
                    ParticipantCount = assignedVotes.Length,
                    RequestedQuantity = requestedQuantity,
                    IsMinimumReached = assignedVotes.Length >= minimumParticipantCount
                        && requestedQuantity >= minimumTotalQuantity,
                    IsCapacityReached = point.CapacityQuantity is int capacityQuantity
                        && requestedQuantity >= capacityQuantity
                };
            }).ToArray()
        };
    }

    private static CommunityVoteResolutionDocumentResponse ToResolutionResponse(CommunityVoteResolutionDocumentRecord document)
    {
        return new CommunityVoteResolutionDocumentResponse
        {
            Id = document.Id,
            VoteId = document.VoteId,
            DocumentNumber = document.DocumentNumber,
            DocumentTitle = document.DocumentTitle,
            ResolutionText = document.ResolutionText,
            DocumentHash = document.DocumentHash,
            Status = document.Status,
            LegalEffectNotice = document.LegalEffectNotice,
            CreatedAtUtc = document.CreatedAtUtc,
            SignaturePlan = document.SignatureBundle is null
                ? null
                : ContractElectronicSignaturePlanner.Plan(document.SignatureBundle, DateTimeOffset.UtcNow)
        };
    }

    private static string BuildDocumentText(
        CommunityVoteRecord vote,
        CommunityVoteResolutionDraftRequest request,
        string legalEffectNotice)
    {
        var resultLines = ToResponse(vote).Options
            .OrderByDescending(x => x.VoteCount)
            .Select(x => $"- {x.Text}: {x.VoteCount}표");
        return string.Join('\n',
            Normalize(request.DocumentTitle, $"{vote.Title} 결의문"),
            vote.Title,
            vote.Description,
            Normalize(request.ResolutionText, "투표 결과에 따른 커뮤니티 결의 초안입니다."),
            "투표 결과:",
            string.Join('\n', resultLines),
            "법적 효력 고지:",
            legalEffectNotice);
    }

    private static string BuildLegalEffectNotice(CommunityVoteRecord vote)
    {
        var settings = vote.GroupPurchase;
        if (settings is null)
        {
            return GenericLegalEffectNotice;
        }

        var proposalOriginNotice = settings.ProposalOriginLegalEffectNotice;
        var agreementNotice = string.IsNullOrWhiteSpace(proposalOriginNotice)
            ? $"{GenericLegalEffectNotice} {CommunityGroupPurchaseAgreementPolicy.FullLegalEffectNotice}"
            : $"{GenericLegalEffectNotice} {proposalOriginNotice}";

        return CommunityGroupPurchaseTradeRouteCodes.IsGroupImport(settings.TradeRouteCode)
            ? $"{agreementNotice} {CommunityGroupPurchaseTradeRoutePolicy.GroupImportCandidateNotice}"
            : agreementNotice;
    }

    private static void EnsureGroupImportContractReady(CommunityVoteRecord vote)
    {
        var settings = vote.GroupPurchase;
        if (settings is null
            || !CommunityGroupPurchaseTradeRouteCodes.IsGroupImport(settings.TradeRouteCode))
        {
            return;
        }

        var tradeRouteDecision = CommunityGroupPurchaseTradeRoutePolicy.Evaluate(
            new CommunityGroupPurchaseTradeRouteInput(
                settings.SellerCountryCode,
                settings.ShipFromCountryCode,
                settings.DeliveryCountryCode,
                settings.CustomsClearanceStatusCode));
        if (!tradeRouteDecision.IsGroupImportCandidate
            || tradeRouteDecision.RequiresManualReview)
        {
            throw new InvalidOperationException(
                "공동수입 계약 확정 전에 상품 출발국가, 국내 배송국가와 통관 상태를 다시 확인해 주세요.");
        }

        var hasValidHsCode = new[] { settings.HsCode }
            .Concat(vote.Options.Select(option => option.HsCode))
            .Any(code => CommunityVoteHsCode.NormalizeOptional(code) is not null);
        if (!hasValidHsCode)
        {
            throw new InvalidOperationException(
                "공동수입 계약을 확정하려면 검토 가능한 HS 코드가 하나 이상 필요합니다.");
        }
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private const string GenericLegalEffectNotice =
        "이 문서는 커뮤니티 투표 결과와 전자서명 증적을 정리한 플랫폼 결의문입니다. 실제 법적 효력과 제출 가능 여부는 문서 종류, 당사자 권한, 고지/동의, 상대 기관 기준, 관련 법령 검토가 필요합니다.";

    private sealed record GroupPurchaseParticipation(
        int RequestedQuantity,
        string ParticipationMethodCode,
        string? PickupPointId);
}

public sealed class InMemoryCommunityVoteService : CommunityVoteService
{
    public InMemoryCommunityVoteService(ICommunityGroupPurchaseDemandHandoff? groupPurchaseDemandHandoff = null)
        : this(
            new InMemoryCommunityVoteStore(),
            groupPurchaseDemandHandoff ?? new NoOpCommunityGroupPurchaseDemandHandoff())
    {
    }

    private InMemoryCommunityVoteService(
        InMemoryCommunityVoteStore store,
        ICommunityGroupPurchaseDemandHandoff handoff)
        : this(
            store,
            new CommunityGroupPurchaseDemandOutboxProcessor(
                store,
                handoff,
                retryBaseDelay: TimeSpan.Zero))
    {
    }

    private InMemoryCommunityVoteService(
        InMemoryCommunityVoteStore store,
        ICommunityGroupPurchaseDemandOutboxProcessor processor)
        : base(store, processor)
    {
        DemandOutboxProcessor = processor;
    }

    private ICommunityGroupPurchaseDemandOutboxProcessor DemandOutboxProcessor { get; }

    public Task<bool> ProcessPendingDemandHandoffAsync(CancellationToken cancellationToken = default) =>
        DemandOutboxProcessor.ProcessNextAsync(cancellationToken);
}
