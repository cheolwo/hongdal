using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Community;

public partial class CommunityVoteService
{
    private static GroupPurchaseParticipation ValidateGroupPurchaseParticipation(
        CommunityVoteRecord vote,
        CommunityVoteCastRequest request,
        string voterHash)
    {
        var settings = vote.GroupPurchase;
        if (settings is null)
        {
            return new GroupPurchaseParticipation(
                1,
                string.Empty,
                null,
                공동구매거래유형코드.B2C,
                공동구매가격표시기준코드.부가세포함,
                null,
                null,
                false);
        }

        if (request.RequestedQuantity is < 1 or > 10_000)
        {
            throw new InvalidOperationException("희망 수량은 1개 이상 10,000개 이하여야 합니다.");
        }

        if (!string.IsNullOrWhiteSpace(request.TransactionTypeCode)
            && !공동구매거래유형코드.지원여부(request.TransactionTypeCode))
        {
            throw new InvalidOperationException("공동구매 거래유형은 B2C 또는 B2B여야 합니다.");
        }

        var transactionTypeCode = 공동구매거래유형코드.정규화(request.TransactionTypeCode);
        var allowedTransactionTypeCodes = NormalizeAllowedTransactionTypeCodes(
            settings.AllowedTransactionTypeCodes);
        if (!allowedTransactionTypeCodes.Contains(transactionTypeCode, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("이 공동구매에서 허용하지 않는 거래유형입니다.");
        }

        if (!string.IsNullOrWhiteSpace(request.PriceBasisCode)
            && !공동구매가격표시기준코드.지원여부(request.PriceBasisCode))
        {
            throw new InvalidOperationException("가격 표시 기준은 부가세 포함 또는 부가세 별도여야 합니다.");
        }

        var priceBasisCode = 공동구매가격표시기준코드.정규화(
            request.PriceBasisCode,
            transactionTypeCode);
        var purchasingOrganizationReference = NormalizeOptional(request.PurchasingOrganizationReference);
        var purchasingOrganizationName = NormalizeOptional(request.PurchasingOrganizationName);
        if (transactionTypeCode == 공동구매거래유형코드.B2B
            && purchasingOrganizationReference is null
            && purchasingOrganizationName is null)
        {
            throw new InvalidOperationException("B2B 구매 의향에는 구매 조직 이름 또는 조직 참조키가 필요합니다.");
        }

        if (transactionTypeCode == 공동구매거래유형코드.B2C
            && (purchasingOrganizationReference is not null
                || purchasingOrganizationName is not null
                || request.TaxInvoiceRequired
                || string.Equals(
                    request.PriceBasisCode?.Trim(),
                    공동구매가격표시기준코드.부가세별도,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("B2C 구매 의향에는 사업자 정보, 세금계산서 또는 부가세 별도 기준을 사용할 수 없습니다.");
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

        return new GroupPurchaseParticipation(
            request.RequestedQuantity,
            methodCode,
            pickupPoint?.PickupPointId,
            transactionTypeCode,
            priceBasisCode,
            purchasingOrganizationReference,
            purchasingOrganizationName,
            request.TaxInvoiceRequired);
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
            TransactionTypeCode = participation.TransactionTypeCode,
            PriceBasisCode = participation.PriceBasisCode,
            PurchasingOrganizationReference = participation.PurchasingOrganizationReference,
            PurchasingOrganizationName = participation.PurchasingOrganizationName,
            TaxInvoiceRequired = participation.TaxInvoiceRequired,
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
        var transactionSegments = BuildTransactionSegments(
            vote.Votes,
            settings.MinimumParticipantCount,
            settings.MinimumTotalQuantity);
        var hasExplicitTradeRoute = !string.IsNullOrWhiteSpace(settings.TradeRouteCode);
        var tradeRouteDecision = hasExplicitTradeRoute
            ? CommunityGroupPurchaseTradeRoutePolicy.Evaluate(
                new CommunityGroupPurchaseTradeRouteInput(
                    settings.SellerCountryCode,
                    settings.ShipFromCountryCode,
                    settings.DeliveryCountryCode,
                    settings.CustomsClearanceStatusCode,
                    settings.OperatingMarketCountryCode))
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
            OperatingMarketCountryCode = CommunityGroupPurchaseTradeRoutePolicy
                .NormalizeOperatingMarketCountryCode(settings.OperatingMarketCountryCode),
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
            AllowedTransactionTypeCodes = NormalizeAllowedTransactionTypeCodes(
                settings.AllowedTransactionTypeCodes),
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
            IsMinimumReached = transactionSegments.Any(segment => segment.IsMinimumReached),
            TransactionSegments = transactionSegments,
            PickupPoints = settings.PickupPoints.Select(point =>
            {
                var assignedVotes = vote.Votes
                    .Where(x => string.Equals(x.PickupPointId, point.PickupPointId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                var requestedQuantity = assignedVotes.Sum(x => x.RequestedQuantity);
                var minimumParticipantCount = point.MinimumParticipantCount ?? settings.MinimumParticipantCount;
                var minimumTotalQuantity = point.MinimumTotalQuantity ?? settings.MinimumTotalQuantity;
                var pickupTransactionSegments = BuildTransactionSegments(
                    assignedVotes,
                    minimumParticipantCount,
                    minimumTotalQuantity);
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
                    IsMinimumReached = pickupTransactionSegments.Any(segment => segment.IsMinimumReached),
                    IsCapacityReached = point.CapacityQuantity is int capacityQuantity
                        && requestedQuantity >= capacityQuantity,
                    TransactionSegments = pickupTransactionSegments
                };
            }).ToArray()
        };
    }

    private static IReadOnlyList<string> NormalizeAllowedTransactionTypeCodes(
        IReadOnlyList<string>? transactionTypeCodes)
    {
        var normalized = (transactionTypeCodes ?? [])
            .Where(공동구매거래유형코드.지원여부)
            .Select(공동구매거래유형코드.정규화)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return normalized.Length == 0
            ? [공동구매거래유형코드.B2C]
            : normalized;
    }

    private static IReadOnlyList<CommunityGroupPurchaseTransactionSegmentResponse> BuildTransactionSegments(
        IEnumerable<CommunityVoteCastRecord> votes,
        int minimumParticipantCount,
        int minimumTotalQuantity)
    {
        return votes
            .GroupBy(voteCast =>
            {
                var transactionTypeCode = 공동구매거래유형코드.정규화(voteCast.TransactionTypeCode);
                return (
                    TransactionTypeCode: transactionTypeCode,
                    PriceBasisCode: 공동구매가격표시기준코드.정규화(
                        voteCast.PriceBasisCode,
                        transactionTypeCode));
            })
            .Select(group =>
            {
                var buyerCount = group.Key.TransactionTypeCode == 공동구매거래유형코드.B2B
                    ? group.Select(BusinessBuyerKey).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                    : group.Select(voteCast => voteCast.VoterHash).Distinct(StringComparer.Ordinal).Count();
                var requestedQuantity = group.Sum(voteCast => voteCast.RequestedQuantity);
                return new CommunityGroupPurchaseTransactionSegmentResponse
                {
                    TransactionTypeCode = group.Key.TransactionTypeCode,
                    PriceBasisCode = group.Key.PriceBasisCode,
                    BuyerCount = buyerCount,
                    RequestedQuantity = requestedQuantity,
                    IsMinimumReached = buyerCount >= minimumParticipantCount
                        && requestedQuantity >= minimumTotalQuantity
                };
            })
            .OrderBy(segment => segment.TransactionTypeCode == 공동구매거래유형코드.B2C ? 0 : 1)
            .ThenBy(segment => segment.PriceBasisCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static string BusinessBuyerKey(CommunityVoteCastRecord voteCast)
    {
        if (!string.IsNullOrWhiteSpace(voteCast.PurchasingOrganizationReference))
        {
            return $"reference:{voteCast.PurchasingOrganizationReference.Trim()}";
        }

        if (!string.IsNullOrWhiteSpace(voteCast.PurchasingOrganizationName))
        {
            return $"name:{voteCast.PurchasingOrganizationName.Trim()}";
        }

        return $"legacy-voter:{voteCast.VoterHash}";
    }
}
