using System.Text.Json;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Operations;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.사용자;

namespace Hongdal.Services.Community;

public interface ICommunityProfessionalEligibilityService
{
    Task<IReadOnlyList<string>> GetVerifiedRoleCodesAsync(
        string userId,
        CancellationToken cancellationToken = default);
}

public sealed class CommunityProfessionalEligibilityService : ICommunityProfessionalEligibilityService
{
    private static readonly string[] RoadFreightBrokerIdentityRoles = ["화물운송주선업자", "FreightBroker", "RoadFreightBroker"];
    private static readonly string[] OceanFreightForwarderIdentityRoles = ["해상운송주선업자", "OceanFreightForwarder"];
    private static readonly string[] AirFreightForwarderIdentityRoles = ["항공화물주선업자", "AirFreightForwarder"];
    private static readonly string[] MultimodalCoordinatorIdentityRoles = ["국제물류주선업자", "복합운송주선업자", "MultimodalCoordinator"];
    private static readonly string[] RoadCarrierIdentityRoles = [역할명.기사, 역할명.용달기사, 역할명.배달기사, "Carrier", "RoadCarrier"];
    private static readonly string[] OceanCarrierIdentityRoles = ["해상운송사", "OceanCarrier"];
    private static readonly string[] AirCarrierIdentityRoles = ["항공운송사", "AirCarrier"];
    private static readonly string[] RailCarrierIdentityRoles = ["철도운송사", "RailCarrier"];
    private static readonly string[] WarehouseIdentityRoles = [역할명.창고관리자, "WarehouseOperator"];
    private static readonly string[] CustomsControlledFacilityIdentityRoles =
        [역할명.보세창고운영자, 역할명.FTZ운영자, "CustomsBondedWarehouseOperator", "ForeignTradeZoneOperator", "CustomsControlledFacilityOperator"];
    private static readonly string[] InBondCarrierIdentityRoles =
        [역할명.보세운송사, "InBondCarrier"];
    private static readonly string[] FulfillmentIdentityRoles =
        [역할명.창고관리자, 역할명.풀필먼트운영자, "WarehouseOperator", "FulfillmentOperator", "ThirdPartyLogisticsProvider"];
    private static readonly string[] ParticipantAddressDeliveryIdentityRoles =
        [역할명.배달기사, 역할명.택배운송사, "ParcelCarrier", "LastMileCarrier"];

    private readonly HongdalContext _db;

    public CommunityProfessionalEligibilityService(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<string>> GetVerifiedRoleCodesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return [];
        }

        var normalizedUserId = userId.Trim();
        var identityRoles = await (
                from userRole in _db.UserRoles.AsNoTracking()
                join role in _db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where userRole.UserId == normalizedUserId && role.Name != null
                select role.Name!)
            .ToListAsync(cancellationToken);
        var participant = await _db.홍달참여자
            .AsNoTracking()
            .Where(item => item.Id == normalizedUserId)
            .Select(item => new { item.활성화여부 })
            .SingleOrDefaultAsync(cancellationToken);
        var participantRoles = participant?.활성화여부 == true
            ? await _db.홍달참여자역할
                .AsNoTracking()
                .Where(role => role.참여자Id == normalizedUserId && role.활성화여부)
                .Select(role => role.역할유형)
                .ToListAsync(cancellationToken)
            : [];

        var verifiedRoles = new List<string>();
        if (participantRoles.Contains(홍달역할유형.기사)
            || HasAnyRole(identityRoles, RoadCarrierIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.RoadCarrier);
        }

        if (HasAnyRole(identityRoles, OceanCarrierIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.OceanCarrier);
        }

        if (HasAnyRole(identityRoles, AirCarrierIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.AirCarrier);
        }

        if (HasAnyRole(identityRoles, RailCarrierIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.RailCarrier);
        }

        if (participantRoles.Contains(홍달역할유형.창고관리자)
            || HasAnyRole(identityRoles, WarehouseIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.WarehouseOperator);
        }

        if (HasAnyRole(identityRoles, CustomsControlledFacilityIdentityRoles))
        {
            verifiedRoles.Add(
                CommunityPostPartyRoleCodes.CustomsControlledFacilityOperator);
        }

        if (HasAnyRole(identityRoles, InBondCarrierIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.InBondCarrier);
        }

        if (participantRoles.Contains(홍달역할유형.창고관리자)
            || HasAnyRole(identityRoles, FulfillmentIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.DomesticFulfillmentOperator);
        }

        if (HasAnyRole(identityRoles, ParticipantAddressDeliveryIdentityRoles))
        {
            verifiedRoles.Add(
                CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider);
        }

        if (HasAnyRole(identityRoles, RoadFreightBrokerIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.RoadFreightBroker);
        }

        if (HasAnyRole(identityRoles, OceanFreightForwarderIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.OceanFreightForwarder);
        }

        if (HasAnyRole(identityRoles, AirFreightForwarderIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.AirFreightForwarder);
        }

        if (HasAnyRole(identityRoles, MultimodalCoordinatorIdentityRoles))
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.MultimodalCoordinator);
        }

        var hasCustomsBrokerRole = participantRoles.Contains(홍달역할유형.관세사)
                                   || HasAnyRole(identityRoles, [역할명.관세사, "CustomsBroker"]);
        var customsProfile = hasCustomsBrokerRole
            ? await _db.관세사프로필
                .AsNoTracking()
                .Where(profile => profile.참여자Id == normalizedUserId
                                  && profile.관리자승인여부
                                  && profile.수임가능여부)
                .Select(profile => new { profile.수입전문여부, profile.수출전문여부 })
                .SingleOrDefaultAsync(cancellationToken)
            : null;
        if (customsProfile?.수입전문여부 == true)
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.ImportCustomsBroker);
        }

        if (customsProfile?.수출전문여부 == true)
        {
            verifiedRoles.Add(CommunityPostPartyRoleCodes.ExportCustomsBroker);
        }

        return verifiedRoles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool HasAnyRole(IEnumerable<string> actualRoles, IEnumerable<string> expectedRoles)
        => actualRoles.Any(actual => expectedRoles.Contains(actual, StringComparer.OrdinalIgnoreCase));
}

public interface ICommunityPostProfessionalParticipationService
{
    Task<JoinCommunityPostProfessionalResponse> JoinAsync(
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
}

public sealed class CommunityPostProfessionalParticipationService : ICommunityPostProfessionalParticipationService
{
    private readonly ICommunityPostOpportunityStore _postStore;
    private readonly I커뮤니티원장저장소 _ledgerStore;
    private readonly ICommunityProfessionalEligibilityService _eligibilityService;

    public CommunityPostProfessionalParticipationService(
        ICommunityPostOpportunityStore postStore,
        I커뮤니티원장저장소 ledgerStore,
        ICommunityProfessionalEligibilityService eligibilityService)
    {
        _postStore = postStore;
        _ledgerStore = ledgerStore;
        _eligibilityService = eligibilityService;
    }

    public async Task<JoinCommunityPostProfessionalResponse> JoinAsync(
        long postId,
        JoinCommunityPostProfessionalRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = RequireActor(actorUserId);
        if (!request.ConfirmProfessionalCapacity
            || !request.ConfirmVoluntaryNonBindingParticipation
            || !request.ConfirmParticipantNotification)
        {
            throw new InvalidOperationException(
                "전문 역할 확인, 자발적 비구속 참여, 기존 참여자 알림을 모두 명시적으로 확인해야 합니다.");
        }

        var roleCode = NormalizeProfessionalRoleCode(request.ProfessionalRoleCode);
        var verifiedRoles = await _eligibilityService.GetVerifiedRoleCodesAsync(actor, cancellationToken);
        if (!verifiedRoles.Contains(roleCode, StringComparer.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "현재 계정의 플랫폼 프로필에서 해당 역할을 확인할 수 없습니다. 관할 면허·등록이 필요한 역할은 외부 자격 확인도 별도로 완료해야 합니다.");
        }

        var result = await JoinRoleCoreAsync(
            postId,
            request.ProvisionalLedgerId,
            roleCode,
            actor,
            actorDisplayName,
            CommunityPartyRoleConfirmationScopeCodes.PlatformProfileOnly,
            cancellationToken);
        return BuildProfessionalResponse(
            postId,
            request.DisplayLanguageCode,
            result.Ledger,
            roleCode,
            result.Reused);
    }

    public async Task<JoinCommunityPostPartyRoleResponse> JoinPartyRoleAsync(
        long postId,
        JoinCommunityPostPartyRoleRequest request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actor = RequireActor(actorUserId);
        if (!request.ConfirmRoleCapacity
            || !request.ConfirmVoluntaryNonBindingParticipation
            || !request.ConfirmParticipantNotification)
        {
            throw new InvalidOperationException(
                "거래 당사자 역할, 자발적 비구속 참여, 기존 참여자 알림을 모두 명시적으로 확인해야 합니다.");
        }

        var roleCode = NormalizeCommercialPartyRoleCode(request.PartyRoleCode);
        var result = await JoinRoleCoreAsync(
            postId,
            request.ProvisionalLedgerId,
            roleCode,
            actor,
            actorDisplayName,
            CommunityPartyRoleConfirmationScopeCodes.ExplicitSelfAcceptance,
            cancellationToken);
        return BuildPartyRoleResponse(
            postId,
            request.DisplayLanguageCode,
            result.Ledger,
            roleCode,
            result.Reused);
    }

    private async Task<RoleJoinResult> JoinRoleCoreAsync(
        long postId,
        string? provisionalLedgerId,
        string roleCode,
        string actorUserId,
        string actorDisplayName,
        string verificationScopeCode,
        CancellationToken cancellationToken)
    {
        var source = await _postStore.GetAsync(postId, cancellationToken)
                     ?? throw new KeyNotFoundException("커뮤니티 게시글을 찾을 수 없습니다.");
        if (source.IsReportBoardPost)
        {
            throw new InvalidOperationException("신고·분쟁 게시글에서는 거래 역할에 참여할 수 없습니다.");
        }

        if (string.IsNullOrWhiteSpace(source.LinkedLedgerId)
            || !string.Equals(source.LinkedLedgerId, provisionalLedgerId?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new CommunityPostOpportunityConflictException("게시글에 연결된 가원장이 요청과 일치하지 않습니다.");
        }

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var ledger = await _ledgerStore.원장조회Async(source.LinkedLedgerId, cancellationToken)
                         ?? throw new KeyNotFoundException("연결된 가원장을 찾을 수 없습니다.");
            CommunityPostProfessionalParticipationProjection.EnsureProvisionalLedger(ledger);
            var availableRoles = CommunityPostPartyRoleCodes.ForPlan(
                CommunityPostProfessionalParticipationProjection.ReadTradeDirectionCode(ledger),
                CommunityPostProfessionalParticipationProjection.ReadTransportModeCodes(ledger),
                ledger.확장속성.GetValueOrDefault(
                    CommunityPostProvisionalLedgerPolicy.DestinationCountryAttributeKey));
            if (!availableRoles.Contains(roleCode, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("이 가원장의 거래 방향과 운송수단에서 모집 중인 역할이 아닙니다.");
            }

            var assignments = CommunityPostProfessionalParticipationProjection.ReadAssignments(ledger).ToList();
            var existingAssignment = assignments.FirstOrDefault(assignment =>
                string.Equals(assignment.UserId, actorUserId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(assignment.RoleCode, roleCode, StringComparison.OrdinalIgnoreCase));
            if (existingAssignment is not null)
            {
                await PromotePostMomentumAsync(postId, ledger, assignments, cancellationToken);
                return new RoleJoinResult(ledger, Reused: true);
            }

            assignments.Add(new CommunityPartyRoleAssignment
            {
                UserId = actorUserId,
                DisplayName = NormalizeDisplayName(actorDisplayName),
                RoleCode = roleCode,
                SourceCode = CommunityPartyRoleAssignmentSourceCodes.Joined,
                VerificationScopeCode = verificationScopeCode
            });
            var nextRevision = ledger.Revision + 1;
            var updatedRequest = BuildLedgerUpdateRequest(
                ledger,
                assignments,
                actorUserId,
                NormalizeDisplayName(actorDisplayName),
                roleCode,
                nextRevision);

            try
            {
                var saved = await _ledgerStore.원장저장Async(updatedRequest, actorUserId, cancellationToken);
                await PromotePostMomentumAsync(postId, saved, assignments, cancellationToken);
                return new RoleJoinResult(saved, Reused: false);
            }
            catch (InvalidOperationException) when (attempt < 4)
            {
                // The ledger store uses optimistic revisions. Reload and retry a concurrent role join.
            }
        }

        throw new InvalidOperationException("다른 참여자가 가원장을 먼저 변경했습니다. 최신 상태를 확인한 뒤 다시 시도해 주세요.");
    }

    private async Task PromotePostMomentumAsync(
        long postId,
        커뮤니티원장Dto ledger,
        IReadOnlyList<CommunityPartyRoleAssignment> assignments,
        CancellationToken cancellationToken)
    {
        var count = assignments
            .Select(assignment => assignment.UserId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var result = await _postStore.SetMomentumPromotionAsync(
            postId,
            ledger.원장Id,
            CommunityPostProfessionalParticipationProjection.ResolveMomentumCode(ledger, assignments),
            CommunityPostProfessionalParticipationProjection.ReadinessMessage(ledger, CommunityDisplayLanguageCodes.Korean),
            count,
            cancellationToken);
        if (result == CommunityPostMomentumUpdateResult.NotFound)
        {
            throw new KeyNotFoundException("커뮤니티 게시글을 찾을 수 없습니다.");
        }

        if (result == CommunityPostMomentumUpdateResult.ConflictingLedger)
        {
            throw new CommunityPostOpportunityConflictException("게시글의 가원장 연결이 변경되었습니다.");
        }
    }

    private static 커뮤니티원장저장요청 BuildLedgerUpdateRequest(
        커뮤니티원장Dto ledger,
        IReadOnlyList<CommunityPartyRoleAssignment> assignments,
        string actorUserId,
        string actorDisplayName,
        string roleCode,
        long nextRevision)
    {
        var attributes = new Dictionary<string, string>(ledger.확장속성, StringComparer.OrdinalIgnoreCase)
        {
            [CommunityPostProvisionalLedgerPolicy.ConfirmedPartyRoleAssignmentsAttributeKey] =
                CommunityPostProfessionalParticipationProjection.SerializeAssignments(assignments),
            [CommunityPostProvisionalLedgerPolicy.ConfirmedPartyRoleParticipantCountAttributeKey] = assignments
                .Select(assignment => assignment.UserId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
                .ToString(),
            [CommunityPostProvisionalLedgerPolicy.CommunityMomentumCodeAttributeKey] =
                CommunityPostProfessionalParticipationProjection.ResolveMomentumCode(ledger, assignments),
            [CommunityPostProvisionalLedgerPolicy.CommunityPromotionRequestedAttributeKey] = bool.TrueString,
            [CommunityPostProvisionalLedgerPolicy.LastPartyRoleJoinedUserIdAttributeKey] = actorUserId,
            [CommunityPostProvisionalLedgerPolicy.LastPartyRoleJoinedDisplayNameAttributeKey] = actorDisplayName,
            [CommunityPostProvisionalLedgerPolicy.LastPartyRoleJoinedRoleCodeAttributeKey] = roleCode,
            [CommunityPostProvisionalLedgerPolicy.LastPartyRoleJoinRevisionAttributeKey] = nextRevision.ToString()
        };
        var participants = MergeRoleParticipant(
            ledger.참여자목록,
            assignments,
            actorUserId,
            actorDisplayName);
        var blocks = ledger.블록목록
            .Select(block => string.Equals(
                    block.BlockId,
                    CommunityPostProvisionalLedgerPolicy.ProfessionalParticipationBlockId,
                    StringComparison.OrdinalIgnoreCase)
                ? CommunityPostProfessionalParticipationProjection.BuildProfessionalBlock(
                    CommunityPostProfessionalParticipationProjection.ResolveRequiredRoles(ledger),
                    assignments)
                : block)
            .ToList();
        if (blocks.All(block => !string.Equals(
                block.BlockId,
                CommunityPostProvisionalLedgerPolicy.ProfessionalParticipationBlockId,
                StringComparison.OrdinalIgnoreCase)))
        {
            blocks.Add(CommunityPostProfessionalParticipationProjection.BuildProfessionalBlock(
                CommunityPostProfessionalParticipationProjection.ResolveRequiredRoles(ledger),
                assignments));
        }

        return new 커뮤니티원장저장요청
        {
            원장Id = ledger.원장Id,
            기대Revision = ledger.Revision,
            커뮤니티Id = ledger.커뮤니티Id,
            원장템플릿Key = ledger.원장템플릿Key,
            제목 = ledger.제목,
            원함 = ledger.원함,
            상태 = ledger.상태,
            현재단계Key = ledger.현재단계Key,
            대상OsCode = ledger.대상OsCode,
            대상OsName = ledger.대상OsName,
            생성자UserId = ledger.생성자UserId,
            생성자표시명 = ledger.생성자표시명,
            블록목록 = blocks,
            참여자목록 = participants,
            포함원장목록 = ledger.포함원장목록,
            다이어그램스냅샷 = ledger.다이어그램스냅샷,
            외부참조 = ledger.외부참조,
            확장속성 = attributes
        };
    }

    private static IReadOnlyList<커뮤니티원장참여자Dto> MergeRoleParticipant(
        IReadOnlyList<커뮤니티원장참여자Dto> participants,
        IReadOnlyList<CommunityPartyRoleAssignment> assignments,
        string actorUserId,
        string actorDisplayName)
    {
        var result = participants.ToList();
        var actorIndex = result.FindIndex(participant => string.Equals(
            participant.UserId,
            actorUserId,
            StringComparison.OrdinalIgnoreCase));
        var actorAssignments = assignments
            .Where(assignment => string.Equals(assignment.UserId, actorUserId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var labelSegments = new List<string>();
        var explicitlyAcceptedLabels = actorAssignments
            .Where(assignment => string.Equals(
                assignment.VerificationScopeCode,
                CommunityPartyRoleConfirmationScopeCodes.ExplicitSelfAcceptance,
                StringComparison.OrdinalIgnoreCase))
            .Select(assignment => CommunityPostProfessionalParticipationProjection.RoleLabel(assignment.RoleCode, false))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (explicitlyAcceptedLabels.Length > 0)
        {
            labelSegments.Add($"명시적 역할 수락 · {string.Join(", ", explicitlyAcceptedLabels)}");
        }

        var platformConfirmedLabels = actorAssignments
            .Where(assignment => !string.Equals(
                assignment.VerificationScopeCode,
                CommunityPartyRoleConfirmationScopeCodes.ExplicitSelfAcceptance,
                StringComparison.OrdinalIgnoreCase))
            .Select(assignment => CommunityPostProfessionalParticipationProjection.RoleLabel(assignment.RoleCode, false))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (platformConfirmedLabels.Length > 0)
        {
            labelSegments.Add($"플랫폼 역할 확인 · {string.Join(", ", platformConfirmedLabels)}");
        }

        var roleParticipationLabel = string.Join(" | ", labelSegments);
        if (actorIndex < 0)
        {
            result.Add(new 커뮤니티원장참여자Dto
            {
                UserId = actorUserId,
                DisplayName = actorDisplayName,
                RoleLabel = roleParticipationLabel,
                ParticipationState = "가원장 역할 참여"
            });
            return result;
        }

        var existing = result[actorIndex];
        var baseRoleLabel = string.Join(
            " | ",
            existing.RoleLabel
                .Split(" | ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(segment => !segment.StartsWith("플랫폼 역할 확인", StringComparison.Ordinal)
                                  && !segment.StartsWith("명시적 역할 수락", StringComparison.Ordinal)
                                  && !segment.StartsWith("검증 전문가", StringComparison.Ordinal)));
        result[actorIndex] = new 커뮤니티원장참여자Dto
        {
            UserId = existing.UserId,
            DisplayName = string.IsNullOrWhiteSpace(existing.DisplayName) ? actorDisplayName : existing.DisplayName,
            RoleLabel = string.IsNullOrWhiteSpace(baseRoleLabel)
                ? roleParticipationLabel
                : $"{baseRoleLabel} | {roleParticipationLabel}",
            ParticipationState = "가원장 역할 참여"
        };
        return result;
    }

    private static JoinCommunityPostProfessionalResponse BuildProfessionalResponse(
        long postId,
        string? displayLanguageCode,
        커뮤니티원장Dto ledger,
        string roleCode,
        bool reused)
    {
        var language = CommunityDisplayLanguageCodes.Normalize(displayLanguageCode);
        return new JoinCommunityPostProfessionalResponse
        {
            PostId = postId,
            DisplayLanguageCode = language,
            ReusedExistingParticipation = reused,
            JoinedProfessionalRoleCode = roleCode,
            ProvisionalLedger = BuildLedgerResponse(ledger),
            Participation = BuildParticipationResponse(postId, ledger, language)
        };
    }

    private static JoinCommunityPostPartyRoleResponse BuildPartyRoleResponse(
        long postId,
        string? displayLanguageCode,
        커뮤니티원장Dto ledger,
        string roleCode,
        bool reused)
    {
        var language = CommunityDisplayLanguageCodes.Normalize(displayLanguageCode);
        return new JoinCommunityPostPartyRoleResponse
        {
            PostId = postId,
            DisplayLanguageCode = language,
            ReusedExistingParticipation = reused,
            JoinedPartyRoleCode = roleCode,
            ProvisionalLedger = BuildLedgerResponse(ledger),
            Participation = BuildParticipationResponse(postId, ledger, language)
        };
    }

    private static CommunityPostProvisionalLedgerResponse BuildLedgerResponse(커뮤니티원장Dto ledger)
        => new()
        {
            LedgerId = ledger.원장Id,
            Revision = ledger.Revision,
            LedgerTemplateKey = ledger.원장템플릿Key,
            State = ledger.상태,
            CurrentStageCode = ledger.현재단계Key ?? string.Empty,
            ParticipantCount = ledger.참여자목록.Count,
            EvidenceSnapshotHash = ledger.확장속성.GetValueOrDefault(
                CommunityPostProvisionalLedgerPolicy.EvidenceSnapshotHashAttributeKey,
                string.Empty),
            NonBinding = true,
            ParticipantNotificationsRequested = true,
            TradeDirectionCode = CommunityPostProfessionalParticipationProjection.ReadTradeDirectionCode(ledger),
            OriginCountryCode = ledger.확장속성.GetValueOrDefault(
                CommunityPostProvisionalLedgerPolicy.OriginCountryAttributeKey,
                string.Empty),
            DestinationCountryCode = ledger.확장속성.GetValueOrDefault(
                CommunityPostProvisionalLedgerPolicy.DestinationCountryAttributeKey,
                string.Empty),
            TransportModeCodes = CommunityPostProfessionalParticipationProjection.ReadTransportModeCodes(ledger)
        };

    private static CommunityPostParticipationEntryResponse BuildParticipationResponse(
        long postId,
        커뮤니티원장Dto ledger,
        string language)
        => new()
        {
            StateCode = CommunityPostParticipationStateCodes.ProvisionalLedgerCreated,
            Title = language == CommunityDisplayLanguageCodes.English ? "Maybe we can do this together" : "같이 해볼까요?",
            Summary = language == CommunityDisplayLanguageCodes.English
                ? "Transaction parties and platform-confirmed role participants may voluntarily join the non-binding provisional ledger."
                : "거래 당사자와 플랫폼에서 역할이 확인된 참여자가 비구속적 가원장에 자발적으로 참여할 수 있습니다.",
            CanStart = false,
            CanJoin = false,
            NonBinding = true,
            ProvisionalLedgerId = ledger.원장Id,
            ParticipantCount = ledger.참여자목록.Count,
            ProfessionalParticipation = CommunityPostProfessionalParticipationProjection.BuildResponse(
                ledger,
                postId,
                language),
            PartyFormation = CommunityPostProfessionalParticipationProjection.BuildPartyFormationResponse(
                ledger,
                language)
        };

    private static string NormalizeProfessionalRoleCode(string? value)
    {
        if (!CommunityPostPartyRoleCodes.IsSpecialist(value))
        {
            throw new InvalidOperationException("지원하지 않는 전문 참여 역할입니다.");
        }

        return CommunityPostPartyRoleCodes.SpecialistRoles.First(role => string.Equals(
            role,
            value!.Trim(),
            StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeCommercialPartyRoleCode(string? value)
    {
        if (!CommunityPostPartyRoleCodes.IsCommercialParty(value))
        {
            throw new InvalidOperationException("지원하지 않는 거래 당사자 역할입니다.");
        }

        return CommunityPostPartyRoleCodes.CommercialPartyRoles.First(role => string.Equals(
            role,
            value!.Trim(),
            StringComparison.OrdinalIgnoreCase));
    }

    private static string RequireActor(string? actorUserId)
        => string.IsNullOrWhiteSpace(actorUserId)
            ? throw new UnauthorizedAccessException("거래 참여팀 역할을 수락하려면 로그인이 필요합니다.")
            : actorUserId.Trim();

    private static string NormalizeDisplayName(string? displayName)
    {
        var normalized = string.IsNullOrWhiteSpace(displayName) ? "역할 참여자" : displayName.Trim();
        return normalized.Length <= 100 ? normalized : normalized[..100];
    }

    private sealed record RoleJoinResult(커뮤니티원장Dto Ledger, bool Reused);
}

internal static class CommunityPostProfessionalParticipationProjection
{
    public static void EnsureProvisionalLedger(커뮤니티원장Dto ledger)
    {
        if (!string.Equals(
                ledger.원장템플릿Key,
                CommunityLedgerTemplateKeys.GroupPurchase,
                StringComparison.OrdinalIgnoreCase)
            || !ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey,
                out var maturityCode)
            || !string.Equals(
                maturityCode,
                CommunityPostProvisionalLedgerPolicy.LedgerMaturityCode,
                StringComparison.OrdinalIgnoreCase)
            || !ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.BindingEffectAttributeKey,
                out var bindingCode)
            || !string.Equals(
                bindingCode,
                CommunityPostProvisionalLedgerPolicy.NonBindingEffectCode,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("전문가 참여는 비구속적 가원장 단계에서만 가능합니다.");
        }
    }

    public static IReadOnlyList<string> ResolveRequiredRoles(커뮤니티원장Dto ledger)
    {
        var plannedRoles = CommunityPostPartyRoleCodes
            .ForPlan(
                ReadTradeDirectionCode(ledger),
                ReadTransportModeCodes(ledger),
                ledger.확장속성.GetValueOrDefault(
                    CommunityPostProvisionalLedgerPolicy.DestinationCountryAttributeKey))
            .Where(CommunityPostPartyRoleCodes.IsSpecialist)
            .ToArray();
        if (ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.RequiredProfessionalRolesAttributeKey,
                out var serialized))
        {
            try
            {
                var stored = JsonSerializer.Deserialize<string[]>(serialized);
                if (stored is { Length: > 0 })
                {
                    return stored
                        .Where(CommunityPostPartyRoleCodes.IsSpecialist)
                        .Select(role => CommunityPostPartyRoleCodes.SpecialistRoles.First(candidate => string.Equals(
                            candidate,
                            role,
                            StringComparison.OrdinalIgnoreCase)))
                        .Concat(plannedRoles)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                }
            }
            catch (JsonException)
            {
                // Fall back to the intent policy for ledgers written before this metadata existed.
            }
        }

        return plannedRoles;
    }

    public static string ReadTradeDirectionCode(커뮤니티원장Dto ledger)
    {
        if (ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.TradeDirectionAttributeKey,
                out var stored)
            && CommunityTradeDirectionCodes.IsSupported(stored))
        {
            return CommunityTradeDirectionCodes.All.First(code => string.Equals(
                code,
                stored,
                StringComparison.OrdinalIgnoreCase));
        }

        ledger.확장속성.TryGetValue(
            CommunityPostProvisionalLedgerPolicy.CollectiveIntentTypeAttributeKey,
            out var intentTypeCode);
        return CommunityTradeDirectionCodes.ExpectedForIntent(intentTypeCode ?? string.Empty);
    }

    public static IReadOnlyList<string> ReadTransportModeCodes(커뮤니티원장Dto ledger)
    {
        if (!ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.TransportModesAttributeKey,
                out var serialized)
            || string.IsNullOrWhiteSpace(serialized))
        {
            return [];
        }

        try
        {
            return CommunityTransportModeCodes.NormalizeMany(
                JsonSerializer.Deserialize<string[]>(serialized));
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static IReadOnlyDictionary<string, int> ReadInterestRoleCounts(커뮤니티원장Dto ledger)
    {
        if (!ledger.확장속성.TryGetValue("InterestRoleCountsJson", out var serialized)
            || string.IsNullOrWhiteSpace(serialized))
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var stored = JsonSerializer.Deserialize<Dictionary<string, int>>(serialized);
            return stored is null
                ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, int>(stored, StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public static IReadOnlyList<CommunityPartyRoleAssignment> ReadAssignments(커뮤니티원장Dto ledger)
    {
        if (!ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.ConfirmedPartyRoleAssignmentsAttributeKey,
                out var serialized)
            || string.IsNullOrWhiteSpace(serialized))
        {
            return [];
        }

        try
        {
            return (JsonSerializer.Deserialize<CommunityPartyRoleAssignment[]>(serialized) ?? [])
                .Where(assignment => !string.IsNullOrWhiteSpace(assignment.UserId)
                                     && CommunityPostPartyRoleCodes.IsSupported(assignment.RoleCode))
                .GroupBy(
                    assignment => $"{assignment.UserId.Trim()}:{assignment.RoleCode.Trim()}",
                    StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static string SerializeAssignments(IReadOnlyList<CommunityPartyRoleAssignment> assignments)
        => JsonSerializer.Serialize(assignments);

    public static 커뮤니티원장블록Dto BuildProfessionalBlock(
        IReadOnlyList<string> requiredRoles,
        IReadOnlyList<CommunityPartyRoleAssignment> assignments)
    {
        var joinedRoleCounts = assignments
            .GroupBy(assignment => assignment.RoleCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        return new 커뮤니티원장블록Dto
        {
            BlockId = CommunityPostProvisionalLedgerPolicy.ProfessionalParticipationBlockId,
            BlockType = CommunityLedgerBlockTypes.Generic,
            Title = "거래 참여팀 역할 구성",
            State = assignments.Count == 0 ? "역할 참여 요청" : "참여팀 구성중",
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["RequiredProfessionalRolesJson"] = JsonSerializer.Serialize(requiredRoles),
                ["ConfirmedPartyRoleCountsJson"] = JsonSerializer.Serialize(joinedRoleCounts),
                ["ConfirmedPartyRoleParticipantCount"] = assignments
                    .Select(assignment => assignment.UserId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count()
                    .ToString(),
                ["PlatformConfirmedProfessionalParticipantCount"] = assignments
                    .Where(assignment => CommunityPostPartyRoleCodes.IsSpecialist(assignment.RoleCode))
                    .Select(assignment => assignment.UserId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count()
                    .ToString(),
                ["VerificationScopeCode"] = "RoleDependent",
                ["ParticipationNotice"] = "역할 참여는 자발적이고 비구속적이며 주문, 계약, 업무 배정 또는 운송 주선을 확정하지 않습니다. 플랫폼 역할 확인은 관할기관 면허·등록 확인을 대신하지 않습니다."
            }
        };
    }

    public static CommunityPostProfessionalParticipationResponse BuildResponse(
        커뮤니티원장Dto? ledger,
        long postId,
        string language)
    {
        if (ledger is null)
        {
            return new CommunityPostProfessionalParticipationResponse();
        }

        try
        {
            EnsureProvisionalLedger(ledger);
        }
        catch (InvalidOperationException)
        {
            return new CommunityPostProfessionalParticipationResponse();
        }

        var assignments = ReadAssignments(ledger);
        var professionalAssignments = assignments
            .Where(assignment => CommunityPostPartyRoleCodes.IsSpecialist(assignment.RoleCode))
            .ToArray();
        var requiredRoles = ResolveRequiredRoles(ledger);
        var momentumCode = ResolveMomentumCode(ledger, assignments);

        return new CommunityPostProfessionalParticipationResponse
        {
            IsAvailable = true,
            PlatformPromotionActive = true,
            MomentumCode = momentumCode,
            MomentumMessage = MomentumMessage(momentumCode, language),
            PlatformConfirmedRoleParticipantCount = professionalAssignments
                .Select(assignment => assignment.UserId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            JoinEndpoint = $"/api/v1/community/posts/{postId}/opportunities/participation/professionals",
            RoleOpenings = requiredRoles.Select(roleCode => new CommunityPostProfessionalRoleOpeningResponse
            {
                RoleCode = roleCode,
                Label = RoleLabel(roleCode, language == CommunityDisplayLanguageCodes.English),
                Summary = RoleSummary(roleCode, language == CommunityDisplayLanguageCodes.English),
                VerificationRequirementCode = VerificationRequirementCode(roleCode),
                ExternalCredentialVerificationRequired = RequiresExternalCredential(roleCode),
                ExternalCredentialVerified = false,
                PlatformConfirmedParticipantCount = professionalAssignments.Count(assignment => string.Equals(
                    assignment.RoleCode,
                    roleCode,
                    StringComparison.OrdinalIgnoreCase)),
                CandidateDirectoryEndpoint = CandidateDirectoryEndpoint(roleCode),
                CandidateDirectoryIsResearchOnly = !string.IsNullOrWhiteSpace(
                    CandidateDirectoryEndpoint(roleCode)),
                RequiresSeparateAuthorityAndContractVerification = true
            }).ToArray()
        };
    }

    public static CommunityPostPartyFormationResponse BuildPartyFormationResponse(
        커뮤니티원장Dto? ledger,
        string language)
        => ledger is null
            ? new CommunityPostPartyFormationResponse()
            : BuildPartyFormationResponse(ledger, language, ReadAssignments(ledger));

    public static string ResolveMomentumCode(
        커뮤니티원장Dto ledger,
        IReadOnlyList<CommunityPartyRoleAssignment> assignments)
    {
        var formation = BuildPartyFormationResponse(
            ledger,
            CommunityDisplayLanguageCodes.Korean,
            assignments);
        if (formation.IsReadyForRealLedgerReview)
        {
            return CommunityPostMomentumCodes.ReadyForRealLedgerReview;
        }

        return assignments.Count > 0
            ? CommunityPostMomentumCodes.PartyForming
            : CommunityPostMomentumCodes.SeekingParty;
    }

    public static string ReadinessMessage(커뮤니티원장Dto ledger, string language)
        => BuildPartyFormationResponse(ledger, language).ReadinessMessage;

    private static CommunityPostPartyFormationResponse BuildPartyFormationResponse(
        커뮤니티원장Dto ledger,
        string language,
        IReadOnlyList<CommunityPartyRoleAssignment> assignments)
    {
        try
        {
            EnsureProvisionalLedger(ledger);
        }
        catch (InvalidOperationException)
        {
            return new CommunityPostPartyFormationResponse();
        }

        var english = language == CommunityDisplayLanguageCodes.English;
        var tradeDirectionCode = ReadTradeDirectionCode(ledger);
        var transportModeCodes = ReadTransportModeCodes(ledger);
        var originCountryCode = ledger.확장속성.GetValueOrDefault(
            CommunityPostProvisionalLedgerPolicy.OriginCountryAttributeKey,
            string.Empty);
        var destinationCountryCode = ledger.확장속성.GetValueOrDefault(
            CommunityPostProvisionalLedgerPolicy.DestinationCountryAttributeKey,
            string.Empty);
        var interestCounts = ReadInterestRoleCounts(ledger);
        var definitions = BuildPartyRoleDefinitions(
            tradeDirectionCode,
            transportModeCodes,
            destinationCountryCode);
        var slots = definitions.Select(definition =>
        {
            var interestCount = ResolveInterestCount(
                definition.RoleCode,
                interestCounts,
                transportModeCodes);
            var confirmedCount = assignments.Count(assignment => string.Equals(
                assignment.RoleCode,
                definition.RoleCode,
                StringComparison.OrdinalIgnoreCase));
            return new CommunityPostPartyRoleSlotResponse
            {
                RoleCode = definition.RoleCode,
                CategoryCode = definition.CategoryCode,
                Label = RoleLabel(definition.RoleCode, english),
                Summary = RoleSummary(definition.RoleCode, english),
                IsRequired = definition.IsRequired,
                IsRecommended = definition.IsRecommended,
                TransportModeCode = definition.TransportModeCode,
                VerificationRequirementCode = VerificationRequirementCode(definition.RoleCode),
                ExternalCredentialVerificationRequired = RequiresExternalCredential(definition.RoleCode),
                ExternalCredentialVerified = false,
                InterestCount = interestCount,
                ConfirmedParticipantCount = confirmedCount,
                StateCode = confirmedCount > 0
                    ? CommunityPartyRoleSlotStateCodes.RoleAccepted
                    : interestCount > 0
                        ? CommunityPartyRoleSlotStateCodes.InterestExpressed
                        : CommunityPartyRoleSlotStateCodes.Open,
                CandidateDirectoryEndpoint = CandidateDirectoryEndpoint(
                    definition.RoleCode),
                CandidateDirectoryIsResearchOnly = !string.IsNullOrWhiteSpace(
                    CandidateDirectoryEndpoint(definition.RoleCode)),
                RequiresSeparateAuthorityAndContractVerification =
                    RequiresExternalCredential(definition.RoleCode)
            };
        }).ToArray();
        var requiredSlots = slots.Where(slot => slot.IsRequired).ToArray();
        var representedCount = requiredSlots.Count(slot => slot.IsRepresented);
        var routeNeedsConfirmation = TradeRouteNeedsConfirmation(
            tradeDirectionCode,
            originCountryCode,
            destinationCountryCode,
            transportModeCodes);
        var ready = requiredSlots.Length > 0
                    && representedCount == requiredSlots.Length
                    && !routeNeedsConfirmation;

        return new CommunityPostPartyFormationResponse
        {
            IsAvailable = true,
            TradeDirectionCode = tradeDirectionCode,
            OriginCountryCode = originCountryCode,
            DestinationCountryCode = destinationCountryCode,
            TransportModeCodes = transportModeCodes,
            TradeRouteNeedsConfirmation = routeNeedsConfirmation,
            RequiredRoleSlotCount = requiredSlots.Length,
            RepresentedRequiredRoleSlotCount = representedCount,
            IsReadyForRealLedgerReview = ready,
            ReadinessMessage = BuildReadinessMessage(
                ready,
                routeNeedsConfirmation,
                representedCount,
                requiredSlots.Length,
                english),
            RoleSlots = slots
        };
    }

    public static string RoleLabel(string roleCode, bool english)
        => (roleCode, english) switch
        {
            (CommunityPostPartyRoleCodes.Buyer, true) => "Buyer",
            (CommunityPostPartyRoleCodes.Buyer, false) => "구매자",
            (CommunityPostPartyRoleCodes.Seller, true) => "Seller",
            (CommunityPostPartyRoleCodes.Seller, false) => "판매자",
            (CommunityPostPartyRoleCodes.Importer, true) => "Responsible importer",
            (CommunityPostPartyRoleCodes.Importer, false) => "수입 책임 당사자",
            (CommunityPostPartyRoleCodes.Exporter, true) => "Responsible exporter",
            (CommunityPostPartyRoleCodes.Exporter, false) => "수출 책임 당사자",
            (CommunityPostPartyRoleCodes.ImportCustomsBroker, true) => "Import customs professional",
            (CommunityPostPartyRoleCodes.ImportCustomsBroker, false) => "수입 통관 관세사",
            (CommunityPostPartyRoleCodes.ExportCustomsBroker, true) => "Export customs professional",
            (CommunityPostPartyRoleCodes.ExportCustomsBroker, false) => "수출 통관 관세사",
            (CommunityPostPartyRoleCodes.OceanFreightForwarder, true) => "Ocean freight forwarder",
            (CommunityPostPartyRoleCodes.OceanFreightForwarder, false) => "해상 운송 주선업자",
            (CommunityPostPartyRoleCodes.AirFreightForwarder, true) => "Air freight forwarder",
            (CommunityPostPartyRoleCodes.AirFreightForwarder, false) => "항공 화물 주선업자",
            (CommunityPostPartyRoleCodes.RoadFreightBroker, true) => "Road freight broker",
            (CommunityPostPartyRoleCodes.RoadFreightBroker, false) => "육상 화물 운송 주선업자",
            (CommunityPostPartyRoleCodes.MultimodalCoordinator, true) => "Multimodal logistics coordinator",
            (CommunityPostPartyRoleCodes.MultimodalCoordinator, false) => "복합운송 물류 주선업자",
            (CommunityPostPartyRoleCodes.OceanCarrier, true) => "Ocean carrier",
            (CommunityPostPartyRoleCodes.OceanCarrier, false) => "해상 운송사",
            (CommunityPostPartyRoleCodes.AirCarrier, true) => "Air carrier",
            (CommunityPostPartyRoleCodes.AirCarrier, false) => "항공 운송사",
            (CommunityPostPartyRoleCodes.RoadCarrier, true) => "Road carrier",
            (CommunityPostPartyRoleCodes.RoadCarrier, false) => "육상 운송사·기사",
            (CommunityPostPartyRoleCodes.RailCarrier, true) => "Rail carrier",
            (CommunityPostPartyRoleCodes.RailCarrier, false) => "철도 운송사",
            (CommunityPostPartyRoleCodes.WarehouseOperator, true) => "Warehouse operator",
            (CommunityPostPartyRoleCodes.WarehouseOperator, false) => "창고 운영자",
            (CommunityPostPartyRoleCodes.CustomsControlledFacilityOperator, true) =>
                "Customs-controlled facility operator",
            (CommunityPostPartyRoleCodes.CustomsControlledFacilityOperator, false) =>
                "보세창고·FTZ 운영자",
            (CommunityPostPartyRoleCodes.InBondCarrier, true) => "In-bond carrier",
            (CommunityPostPartyRoleCodes.InBondCarrier, false) => "통관 전 보세운송사",
            (CommunityPostPartyRoleCodes.DomesticFulfillmentOperator, true) =>
                "Domestic fulfillment operator",
            (CommunityPostPartyRoleCodes.DomesticFulfillmentOperator, false) =>
                "미국 내 풀필먼트 운영자",
            (CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider, true) =>
                "Participant-address delivery provider",
            (CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider, false) =>
                "참여자 주소 배송 사업자",
            _ => roleCode
        };

    private static string RoleSummary(string roleCode, bool english)
        => (roleCode, english) switch
        {
            (CommunityPostPartyRoleCodes.Buyer, true) => "Express purchase interest without creating an order or payment obligation.",
            (CommunityPostPartyRoleCodes.Buyer, false) => "주문·결제 의무 없이 구매 관심과 필요한 수량을 검토합니다.",
            (CommunityPostPartyRoleCodes.Seller, true) => "Review supply quantity, price range, and lead time without accepting an order.",
            (CommunityPostPartyRoleCodes.Seller, false) => "주문 수락 전 공급 수량·가격 범위·납기를 검토합니다.",
            (CommunityPostPartyRoleCodes.Importer, true) => "A transaction party must later accept the importer responsibility required by the destination jurisdiction.",
            (CommunityPostPartyRoleCodes.Importer, false) => "도착국 법령상 수입 책임을 맡을 당사자를 별도 계약 단계에서 확정합니다.",
            (CommunityPostPartyRoleCodes.Exporter, true) => "A transaction party must later accept the exporter responsibility required by the origin jurisdiction.",
            (CommunityPostPartyRoleCodes.Exporter, false) => "출발국 법령상 수출 책임을 맡을 당사자를 별도 계약 단계에서 확정합니다.",
            (CommunityPostPartyRoleCodes.ImportCustomsBroker, true) => "Review import customs questions before a separate engagement; platform profile approval is not proof of every jurisdictional license.",
            (CommunityPostPartyRoleCodes.ImportCustomsBroker, false) => "별도 수임 전 수입 통관 쟁점을 검토하며, 플랫폼 프로필 확인만으로 모든 관할 면허를 증명하지 않습니다.",
            (CommunityPostPartyRoleCodes.ExportCustomsBroker, true) => "Review export filing questions before a separate engagement; platform profile approval is not proof of every jurisdictional license.",
            (CommunityPostPartyRoleCodes.ExportCustomsBroker, false) => "별도 수임 전 수출 신고 쟁점을 검토하며, 플랫폼 프로필 확인만으로 모든 관할 면허를 증명하지 않습니다.",
            (CommunityPostPartyRoleCodes.OceanFreightForwarder, true) => "Review ocean booking and documents without the platform arranging carriage; required authority depends on jurisdiction.",
            (CommunityPostPartyRoleCodes.OceanFreightForwarder, false) => "플랫폼이 운송을 주선하지 않는 상태에서 해상 예약·서류 조건을 검토하며 관할 등록은 별도로 확인합니다.",
            (CommunityPostPartyRoleCodes.AirFreightForwarder, true) => "Review air cargo handling and booking conditions under separately verified authority.",
            (CommunityPostPartyRoleCodes.AirFreightForwarder, false) => "별도로 확인된 권한 범위에서 항공 화물 취급·예약 조건을 검토합니다.",
            (CommunityPostPartyRoleCodes.RoadFreightBroker, true) => "A separately authorized broker may offer to arrange road carriage; the platform does not select a carrier or set a dispatch.",
            (CommunityPostPartyRoleCodes.RoadFreightBroker, false) => "별도 허가·등록 주선업자가 육상 운송 조건을 제안하며 플랫폼은 운송사 선택이나 배차를 결정하지 않습니다.",
            (CommunityPostPartyRoleCodes.MultimodalCoordinator, true) => "Review handoffs across modes under the registrations required by each jurisdiction.",
            (CommunityPostPartyRoleCodes.MultimodalCoordinator, false) => "관할별 등록 범위 안에서 복수 운송수단의 인계 조건을 검토합니다.",
            (CommunityPostPartyRoleCodes.OceanCarrier, true) => "Review ocean carriage capacity without accepting a booking.",
            (CommunityPostPartyRoleCodes.OceanCarrier, false) => "예약 수락 전 해상 운송 가능 용량과 조건을 검토합니다.",
            (CommunityPostPartyRoleCodes.AirCarrier, true) => "Review air carriage capacity without accepting a booking.",
            (CommunityPostPartyRoleCodes.AirCarrier, false) => "예약 수락 전 항공 운송 가능 용량과 조건을 검토합니다.",
            (CommunityPostPartyRoleCodes.RoadCarrier, true) => "Review feasible road carriage without accepting a dispatch.",
            (CommunityPostPartyRoleCodes.RoadCarrier, false) => "배차 수락 전 육상 운송 가능 조건을 검토합니다.",
            (CommunityPostPartyRoleCodes.RailCarrier, true) => "Review rail carriage capacity without accepting a booking.",
            (CommunityPostPartyRoleCodes.RailCarrier, false) => "예약 수락 전 철도 운송 가능 용량과 조건을 검토합니다.",
            (CommunityPostPartyRoleCodes.WarehouseOperator, true) => "Review receiving, storage, and outbound feasibility without accepting a service order.",
            (CommunityPostPartyRoleCodes.WarehouseOperator, false) => "서비스 주문 수락 전 입고·보관·출고 가능 조건을 검토합니다.",
            (CommunityPostPartyRoleCodes.CustomsControlledFacilityOperator, true) =>
                "Review bonded warehouse or FTZ storage without confirming current facility authorization, availability, or a service contract.",
            (CommunityPostPartyRoleCodes.CustomsControlledFacilityOperator, false) =>
                "현재 시설 승인·가용 공간·계약을 확정하지 않은 상태에서 보세창고 또는 FTZ 보관 가능성을 검토합니다.",
            (CommunityPostPartyRoleCodes.InBondCarrier, true) =>
                "Review pre-release in-bond movement subject to ACE filing, carrier bond, route, and a separate carriage contract.",
            (CommunityPostPartyRoleCodes.InBondCarrier, false) =>
                "ACE 신고·carrier bond·이동 경로와 별도 운송계약 확인을 전제로 통관 전 보세운송 가능성을 검토합니다.",
            (CommunityPostPartyRoleCodes.DomesticFulfillmentOperator, true) =>
                "Review released-cargo receiving, break-pack, kitting, storage, and parcel tender without accepting a fulfillment order.",
            (CommunityPostPartyRoleCodes.DomesticFulfillmentOperator, false) =>
                "서비스 주문 수락 전 반출 완료 화물의 입고·소분·kitting·보관·parcel 인계 가능성을 검토합니다.",
            (CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider, true) =>
                "Review delivery from fulfillment to participant addresses without accepting shipments or confirming coverage.",
            (CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider, false) =>
                "배송 접수나 권역을 확정하지 않은 상태에서 풀필먼트 창고부터 참여자 주소까지의 배송 가능성을 검토합니다.",
            _ => string.Empty
        };

    private static string VerificationRequirementCode(string roleCode)
        => roleCode switch
        {
            CommunityPostPartyRoleCodes.Buyer
                or CommunityPostPartyRoleCodes.Seller
                or CommunityPostPartyRoleCodes.Importer
                or CommunityPostPartyRoleCodes.Exporter
                => CommunityPartyRoleVerificationRequirementCodes.ExplicitPartyAcceptance,
            CommunityPostPartyRoleCodes.ImportCustomsBroker
                or CommunityPostPartyRoleCodes.ExportCustomsBroker
                or CommunityPostPartyRoleCodes.OceanFreightForwarder
                or CommunityPostPartyRoleCodes.AirFreightForwarder
                or CommunityPostPartyRoleCodes.RoadFreightBroker
                or CommunityPostPartyRoleCodes.MultimodalCoordinator
                => CommunityPartyRoleVerificationRequirementCodes.JurisdictionLicenseOrRegistration,
            CommunityPostPartyRoleCodes.OceanCarrier
                or CommunityPostPartyRoleCodes.AirCarrier
                or CommunityPostPartyRoleCodes.RoadCarrier
                or CommunityPostPartyRoleCodes.RailCarrier
                => CommunityPartyRoleVerificationRequirementCodes.CarrierOperatingAuthority,
            CommunityPostPartyRoleCodes.CustomsControlledFacilityOperator
                => CommunityPartyRoleVerificationRequirementCodes
                    .CustomsFacilityAuthorization,
            CommunityPostPartyRoleCodes.InBondCarrier
                => CommunityPartyRoleVerificationRequirementCodes
                    .BondedCarrierOperatingAuthority,
            CommunityPostPartyRoleCodes.DomesticFulfillmentOperator
                => CommunityPartyRoleVerificationRequirementCodes
                    .FacilityCapabilityAndContract,
            CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider
                => CommunityPartyRoleVerificationRequirementCodes
                    .CarrierOperatingAuthority,
            _ => CommunityPartyRoleVerificationRequirementCodes.PlatformProfile
        };

    private static bool RequiresExternalCredential(string roleCode)
        => VerificationRequirementCode(roleCode) is
            CommunityPartyRoleVerificationRequirementCodes.JurisdictionLicenseOrRegistration
            or CommunityPartyRoleVerificationRequirementCodes.CarrierOperatingAuthority
            or CommunityPartyRoleVerificationRequirementCodes.CustomsFacilityAuthorization
            or CommunityPartyRoleVerificationRequirementCodes.BondedCarrierOperatingAuthority
            or CommunityPartyRoleVerificationRequirementCodes.FacilityCapabilityAndContract;

    private static IReadOnlyList<PartyRoleDefinition> BuildPartyRoleDefinitions(
        string tradeDirectionCode,
        IReadOnlyList<string> transportModeCodes,
        string destinationCountryCode)
    {
        var roles = CommunityPostPartyRoleCodes.ForPlan(
            tradeDirectionCode,
            transportModeCodes,
            destinationCountryCode);
        return roles
            .Select(roleCode => new PartyRoleDefinition(
                roleCode,
                CategoryCode(roleCode),
                IsRequiredRole(roleCode, tradeDirectionCode),
                IsRecommendedRole(roleCode),
                TransportModeCode(roleCode)))
            .OrderBy(definition => CategoryOrder(definition.CategoryCode))
            .ThenBy(definition => definition.RoleCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static string CategoryCode(string roleCode)
        => roleCode switch
        {
            CommunityPostPartyRoleCodes.Buyer
                or CommunityPostPartyRoleCodes.Seller
                or CommunityPostPartyRoleCodes.Importer
                or CommunityPostPartyRoleCodes.Exporter
                => CommunityPartyRoleCategoryCodes.CommercialParty,
            CommunityPostPartyRoleCodes.ImportCustomsBroker
                or CommunityPostPartyRoleCodes.ExportCustomsBroker
                => CommunityPartyRoleCategoryCodes.CustomsAndDocumentation,
            CommunityPostPartyRoleCodes.OceanFreightForwarder
                or CommunityPostPartyRoleCodes.AirFreightForwarder
                or CommunityPostPartyRoleCodes.RoadFreightBroker
                or CommunityPostPartyRoleCodes.MultimodalCoordinator
                => CommunityPartyRoleCategoryCodes.TransportationIntermediary,
            CommunityPostPartyRoleCodes.OceanCarrier
                or CommunityPostPartyRoleCodes.AirCarrier
                or CommunityPostPartyRoleCodes.RoadCarrier
                or CommunityPostPartyRoleCodes.RailCarrier
                or CommunityPostPartyRoleCodes.InBondCarrier
                or CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider
                => CommunityPartyRoleCategoryCodes.Carrier,
            _ => CommunityPartyRoleCategoryCodes.Fulfillment
        };

    private static bool IsRequiredRole(string roleCode, string tradeDirectionCode)
        => roleCode is CommunityPostPartyRoleCodes.Buyer or CommunityPostPartyRoleCodes.Seller
           || !string.Equals(
                  tradeDirectionCode,
                  CommunityTradeDirectionCodes.Domestic,
                  StringComparison.OrdinalIgnoreCase)
              && roleCode is CommunityPostPartyRoleCodes.Importer or CommunityPostPartyRoleCodes.Exporter
           || roleCode is CommunityPostPartyRoleCodes.OceanCarrier
               or CommunityPostPartyRoleCodes.AirCarrier
               or CommunityPostPartyRoleCodes.RoadCarrier
               or CommunityPostPartyRoleCodes.RailCarrier
               or CommunityPostPartyRoleCodes.MultimodalCoordinator;

    private static bool IsRecommendedRole(string roleCode)
        => roleCode is CommunityPostPartyRoleCodes.ImportCustomsBroker
            or CommunityPostPartyRoleCodes.ExportCustomsBroker
            or CommunityPostPartyRoleCodes.OceanFreightForwarder
            or CommunityPostPartyRoleCodes.AirFreightForwarder
            or CommunityPostPartyRoleCodes.RoadFreightBroker
            or CommunityPostPartyRoleCodes.WarehouseOperator
            or CommunityPostPartyRoleCodes.CustomsControlledFacilityOperator
            or CommunityPostPartyRoleCodes.InBondCarrier
            or CommunityPostPartyRoleCodes.DomesticFulfillmentOperator
            or CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider;

    private static string? TransportModeCode(string roleCode)
        => roleCode switch
        {
            CommunityPostPartyRoleCodes.OceanFreightForwarder or CommunityPostPartyRoleCodes.OceanCarrier
                => CommunityTransportModeCodes.Ocean,
            CommunityPostPartyRoleCodes.AirFreightForwarder or CommunityPostPartyRoleCodes.AirCarrier
                => CommunityTransportModeCodes.Air,
            CommunityPostPartyRoleCodes.RoadFreightBroker or CommunityPostPartyRoleCodes.RoadCarrier
                => CommunityTransportModeCodes.Road,
            CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider
                => CommunityTransportModeCodes.Road,
            CommunityPostPartyRoleCodes.RailCarrier => CommunityTransportModeCodes.Rail,
            CommunityPostPartyRoleCodes.MultimodalCoordinator => CommunityTransportModeCodes.Multimodal,
            _ => null
        };

    private static int ResolveInterestCount(
        string roleCode,
        IReadOnlyDictionary<string, int> interestCounts,
        IReadOnlyList<string> transportModeCodes)
    {
        var sourceRoleCode = roleCode switch
        {
            CommunityPostPartyRoleCodes.Buyer => CommunityPostParticipationRoleCodes.Buyer,
            CommunityPostPartyRoleCodes.Seller => CommunityPostParticipationRoleCodes.Supplier,
            CommunityPostPartyRoleCodes.ImportCustomsBroker or CommunityPostPartyRoleCodes.ExportCustomsBroker
                => CommunityPostParticipationRoleCodes.CustomsBroker,
            CommunityPostPartyRoleCodes.OceanFreightForwarder
                or CommunityPostPartyRoleCodes.AirFreightForwarder
                or CommunityPostPartyRoleCodes.RoadFreightBroker
                or CommunityPostPartyRoleCodes.MultimodalCoordinator
                => transportModeCodes.Count == 1 ? CommunityPostParticipationRoleCodes.FreightBroker : string.Empty,
            CommunityPostPartyRoleCodes.OceanCarrier
                or CommunityPostPartyRoleCodes.AirCarrier
                or CommunityPostPartyRoleCodes.RoadCarrier
                or CommunityPostPartyRoleCodes.RailCarrier
                => transportModeCodes.Count == 1 ? CommunityPostParticipationRoleCodes.Carrier : string.Empty,
            CommunityPostPartyRoleCodes.WarehouseOperator => CommunityPostParticipationRoleCodes.WarehouseOperator,
            CommunityPostPartyRoleCodes.CustomsControlledFacilityOperator
                or CommunityPostPartyRoleCodes.DomesticFulfillmentOperator
                => CommunityPostParticipationRoleCodes.WarehouseOperator,
            CommunityPostPartyRoleCodes.InBondCarrier
                or CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider
                => CommunityPostParticipationRoleCodes.Carrier,
            _ => string.Empty
        };
        return string.IsNullOrWhiteSpace(sourceRoleCode)
            ? 0
            : Math.Max(0, interestCounts.GetValueOrDefault(sourceRoleCode));
    }

    private static string CandidateDirectoryEndpoint(string roleCode)
    {
        var stageCode = roleCode switch
        {
            CommunityPostPartyRoleCodes.CustomsControlledFacilityOperator
                => BondedToDoorLogisticsStageCodes.CustomsControlledStorage,
            CommunityPostPartyRoleCodes.InBondCarrier
                => BondedToDoorLogisticsStageCodes.InBondTransportation,
            CommunityPostPartyRoleCodes.DomesticFulfillmentOperator
                => BondedToDoorLogisticsStageCodes.FulfillmentWarehouseInbound,
            CommunityPostPartyRoleCodes.ParticipantAddressDeliveryProvider
                => BondedToDoorLogisticsStageCodes
                    .ParticipantAddressFinalMileDelivery,
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(stageCode)
            ? string.Empty
            : $"/api/v1/operations/third-party-logistics/providers/bonded-to-door?stageCode={stageCode}";
    }

    private static bool TradeRouteNeedsConfirmation(
        string tradeDirectionCode,
        string originCountryCode,
        string destinationCountryCode,
        IReadOnlyList<string> transportModeCodes)
    {
        var crossBorder = !string.Equals(
            tradeDirectionCode,
            CommunityTradeDirectionCodes.Domestic,
            StringComparison.OrdinalIgnoreCase);
        if (crossBorder)
        {
            return string.IsNullOrWhiteSpace(originCountryCode)
                   || string.IsNullOrWhiteSpace(destinationCountryCode)
                   || string.Equals(originCountryCode, destinationCountryCode, StringComparison.OrdinalIgnoreCase)
                   || transportModeCodes.Count == 0;
        }

        var oneCountryMissing = string.IsNullOrWhiteSpace(originCountryCode)
                                != string.IsNullOrWhiteSpace(destinationCountryCode);
        return oneCountryMissing
               || !string.IsNullOrWhiteSpace(originCountryCode)
               && !string.Equals(originCountryCode, destinationCountryCode, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildReadinessMessage(
        bool ready,
        bool routeNeedsConfirmation,
        int representedCount,
        int requiredCount,
        bool english)
    {
        if (ready)
        {
            return english
                ? $"All {requiredCount} required role slots have explicit participants. A real-ledger review may begin, but no order, contract, assignment, or brokerage is confirmed."
                : $"필수 역할 {requiredCount}/{requiredCount}에 참여자가 명시적으로 역할을 수락했습니다. 실원장 전환 검토를 시작할 수 있지만 주문·계약·업무 배정·운송 주선은 확정되지 않았습니다.";
        }

        if (routeNeedsConfirmation)
        {
            return english
                ? $"Required roles explicitly accepted: {representedCount}/{requiredCount}. Confirm origin, destination, and transport modes before real-ledger review."
                : $"필수 역할 {representedCount}/{requiredCount}이 명시적으로 수락되었습니다. 실원장 전환 검토 전에 출발국·도착국·운송수단을 확인해야 합니다.";
        }

        return english
            ? $"Required roles explicitly accepted: {representedCount}/{requiredCount}. Open roles still need voluntary participants."
            : $"필수 역할 {representedCount}/{requiredCount}이 명시적으로 수락되었습니다. 빈 역할에는 자발적 참여가 더 필요합니다.";
    }

    private static int CategoryOrder(string categoryCode)
        => categoryCode switch
        {
            CommunityPartyRoleCategoryCodes.CommercialParty => 0,
            CommunityPartyRoleCategoryCodes.CustomsAndDocumentation => 1,
            CommunityPartyRoleCategoryCodes.TransportationIntermediary => 2,
            CommunityPartyRoleCategoryCodes.Carrier => 3,
            _ => 4
        };

    public static string MomentumMessage(string momentumCode, string language)
    {
        var english = language == CommunityDisplayLanguageCodes.English;
        return momentumCode switch
        {
            CommunityPostMomentumCodes.ReadyForRealLedgerReview => english
                ? "The required party roles were explicitly accepted and the trade route is specified. A real-ledger review may begin, but no transaction is confirmed."
                : "필수 거래 역할이 명시적으로 수락되고 경로가 구체화되어 실원장 전환 검토를 시작할 수 있습니다. 아직 거래는 확정되지 않았습니다.",
            CommunityPostMomentumCodes.PartyForming => english
                ? "A platform-confirmed role participant joined the provisional ledger. External licenses and final authority still require separate verification."
                : "플랫폼에서 역할이 확인된 참여자가 가원장에 합류했습니다. 외부 면허·등록과 최종 권한은 별도로 확인해야 합니다.",
            _ => english
                ? "Community interest formed a provisional ledger. Transaction parties and qualified specialists may join voluntarily."
                : "사용자 관심이 가원장으로 모였습니다. 거래 당사자와 자격을 갖춘 업무 참여자의 자발적 참여를 기다립니다."
        };
    }

    private sealed record PartyRoleDefinition(
        string RoleCode,
        string CategoryCode,
        bool IsRequired,
        bool IsRecommended,
        string? TransportModeCode);
}

internal sealed class CommunityPartyRoleAssignment
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string SourceCode { get; set; } = CommunityPartyRoleAssignmentSourceCodes.Joined;
    public string VerificationScopeCode { get; set; } = CommunityPartyRoleConfirmationScopeCodes.PlatformProfileOnly;
}

internal static class CommunityPartyRoleAssignmentSourceCodes
{
    public const string Author = "Author";
    public const string Joined = "Joined";
}

internal static class CommunityPartyRoleConfirmationScopeCodes
{
    public const string PlatformProfileOnly = "PlatformProfileOnly";
    public const string ExplicitSelfAcceptance = "ExplicitSelfAcceptance";
}
