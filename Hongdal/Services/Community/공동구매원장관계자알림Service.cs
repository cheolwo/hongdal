using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hongdal.Contracts.Common.Community;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services.Notifications;
using 홍달.도메인.설정;

namespace Hongdal.Services.Community;

public interface I공동구매원장관계자알림Service
{
    Task<int> 변경알림적재Async(
        커뮤니티원장Dto 원장,
        string 변경유형,
        string 변경자UserId,
        string eventId,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default);
}

public sealed class 공동구매원장관계자알림Service : I공동구매원장관계자알림Service
{
    private readonly HongdalContext _db;
    private readonly ILogger<공동구매원장관계자알림Service> _logger;

    public 공동구매원장관계자알림Service(
        HongdalContext db,
        ILogger<공동구매원장관계자알림Service> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> 변경알림적재Async(
        커뮤니티원장Dto 원장,
        string 변경유형,
        string 변경자UserId,
        string eventId,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(원장);

        var recipients = 공동구매원장관계자알림Policy.ResolveRecipientUserIds(원장, 변경자UserId);
        if (recipients.Count == 0)
        {
            return 0;
        }

        var traceId = 공동구매원장관계자알림Policy.BuildTraceId(
            eventId,
            $"{원장.원장Id}:{원장.Revision}:{변경유형}");
        var alreadyQueued = await _db.Command알림Outbox
            .AsNoTracking()
            .AnyAsync(
                x => x.TraceId == traceId
                     && x.FeatureName == Command알림FeatureNames.공동구매원장변경
                     && x.Target == Command알림TargetNames.공동구매원장관계자,
                cancellationToken);
        if (alreadyQueued)
        {
            return 0;
        }

        var now = occurredAtUtc == default ? DateTime.UtcNow : occurredAtUtc.ToUniversalTime();
        foreach (var recipientUserId in recipients)
        {
            _db.Command알림Outbox.Add(new Command알림Outbox
            {
                CommandName = "공동구매원장변경",
                EventName = "커뮤니티원장변경됨Event",
                FeatureName = Command알림FeatureNames.공동구매원장변경,
                Target = Command알림TargetNames.공동구매원장관계자,
                PayloadJson = 공동구매원장관계자알림Policy.BuildPayload(
                    원장,
                    recipientUserId,
                    변경유형),
                Status = "Pending",
                TraceId = traceId,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "공동구매 원장 변경 관계자 알림 적재 완료. LedgerId={LedgerId}, Revision={Revision}, RecipientCount={RecipientCount}",
            원장.원장Id,
            원장.Revision,
            recipients.Count);
        return recipients.Count;
    }
}

public static class 공동구매원장관계자알림Policy
{
    private const string SourceCommunityPostIdKey = "SourceCommunityPostId";
    private const string SourcePostIdKey = "SourcePostId";

    public static bool ShouldQueue(string 변경유형, 커뮤니티원장Dto 원장)
        => string.Equals(
               원장.원장템플릿Key,
               CommunityLedgerTemplateKeys.GroupPurchase,
               StringComparison.OrdinalIgnoreCase)
           && (string.Equals(변경유형, "상태변경", StringComparison.Ordinal)
               || string.Equals(변경유형, "저장", StringComparison.Ordinal)
               && (원장.Revision > 1 || IsNotifiableProvisionalLedger(원장)));

    public static bool IsNotifiableProvisionalLedger(커뮤니티원장Dto 원장)
    {
        ArgumentNullException.ThrowIfNull(원장);
        return 원장.Revision == 1
               && 원장.확장속성.TryGetValue(
                   CommunityPostProvisionalLedgerPolicy.LedgerMaturityAttributeKey,
                   out var maturityCode)
               && string.Equals(
                   maturityCode,
                   CommunityPostProvisionalLedgerPolicy.LedgerMaturityCode,
                   StringComparison.OrdinalIgnoreCase)
               && 원장.확장속성.TryGetValue(
                   CommunityPostProvisionalLedgerPolicy.ParticipantNotificationsAttributeKey,
                   out var notificationsRequested)
               && bool.TryParse(notificationsRequested, out var requested)
               && requested;
    }

    public static IReadOnlyList<string> ResolveRecipientUserIds(
        커뮤니티원장Dto 원장,
        string? changedByUserId)
    {
        ArgumentNullException.ThrowIfNull(원장);

        var actor = string.IsNullOrWhiteSpace(changedByUserId)
            ? string.Empty
            : changedByUserId.Trim();
        return new[] { 원장.생성자UserId }
            .Concat(원장.참여자목록.Select(participant => participant.UserId))
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Select(userId => userId!.Trim())
            .Where(userId => !string.Equals(userId, actor, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<CommunityPostPartyRoleSlotResponse> ResolveInitialOpenRoleSlots(
        커뮤니티원장Dto 원장)
    {
        ArgumentNullException.ThrowIfNull(원장);
        if (!IsNotifiableProvisionalLedger(원장))
        {
            return [];
        }

        return CommunityPostProfessionalParticipationProjection
            .BuildPartyFormationResponse(원장, CommunityDisplayLanguageCodes.Korean)
            .RoleSlots
            .Where(slot => slot.ConfirmedParticipantCount == 0)
            .OrderByDescending(slot => slot.IsRequired)
            .ThenByDescending(slot => slot.InterestCount)
            .ThenBy(slot => slot.Label, StringComparer.Ordinal)
            .ToArray();
    }

    public static string BuildPayload(
        커뮤니티원장Dto 원장,
        string targetUserId,
        string 변경유형)
    {
        ArgumentNullException.ThrowIfNull(원장);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetUserId);

        var ledgerId = 원장.원장Id.Trim();
        var title = string.IsNullOrWhiteSpace(원장.제목) ? "공동구매 원장" : 원장.제목.Trim();
        var stateSummary = string.IsNullOrWhiteSpace(원장.현재단계Key)
            ? 원장.상태
            : $"{원장.상태} · {원장.현재단계Key.Trim()}";
        var provisionalCreated = string.Equals(변경유형, "저장", StringComparison.Ordinal)
                                 && IsNotifiableProvisionalLedger(원장);
        IReadOnlyList<CommunityPostPartyRoleSlotResponse> openRoleSlots = provisionalCreated
            ? ResolveInitialOpenRoleSlots(원장)
            : [];
        var roleInterestInvitation = openRoleSlots.Count > 0;
        var openRoleSummary = roleInterestInvitation
            ? BuildOpenRoleSummary(openRoleSlots)
            : string.Empty;
        var roleParticipantJoined = TryReadRoleParticipation(
            원장,
            out var joinedDisplayName,
            out var joinedRoleLabel,
            out var specialistRoleJoined);
        var body = provisionalCreated
            ? roleInterestInvitation
                ? $"{title}에 관심이 모여 비구속적 가원장이 만들어졌습니다. 현재 비어 있는 역할은 {openRoleSummary}입니다. 기존 참여자는 게시글에서 맡을 수 있는 역할을 확인하고 비구속적 참여 의향을 표시할 수 있습니다. 이 알림만으로 역할이 배정되거나 확정되지는 않습니다. 아직 주문·계약·배차·운송 주선은 확정되지 않았습니다."
                : $"{title}에 관심이 모여 비구속적 가원장이 만들어졌습니다. 아직 주문·계약·배차·운송 주선은 확정되지 않았습니다."
            : roleParticipantJoined
                ? specialistRoleJoined
                    ? $"{joinedDisplayName}님이 {joinedRoleLabel} 역할로 거래 참여팀에 합류했습니다. 플랫폼 역할 확인은 외부 면허·등록 확인이나 업무 수임을 대신하지 않습니다."
                    : $"{joinedDisplayName}님이 {joinedRoleLabel} 역할을 비구속적으로 수락했습니다. 주문·계약·결제 또는 최종 거래 책임은 별도 합의 전까지 확정되지 않습니다."
            : string.Equals(변경유형, "상태변경", StringComparison.Ordinal)
                ? $"{title}의 진행 상태가 {stateSummary}(으)로 변경되었습니다."
                : $"{title}의 조건 또는 참여 내용이 변경되었습니다.";

        return JsonSerializer.Serialize(new
        {
            notificationType = Command알림FeatureNames.공동구매원장변경,
            targetUserId = targetUserId.Trim(),
            ledgerId,
            ledgerRevision = 원장.Revision,
            changeType = 변경유형,
            currentState = 원장.상태,
            currentStep = 원장.현재단계Key ?? string.Empty,
            title = provisionalCreated
                ? roleInterestInvitation
                    ? "가원장에 함께 맡을 역할이 열렸습니다"
                    : "관심이 모여 가원장이 만들어졌습니다"
                : roleParticipantJoined
                    ? "거래 참여팀에 새 역할이 합류했습니다"
                    : "공동구매 원장이 변경되었습니다",
            body,
            deepLink = roleInterestInvitation
                ? BuildRoleParticipationDeepLink(원장, ledgerId)
                : BuildLedgerDeepLink(ledgerId),
            roleInterestInvitation,
            openRoleSlotCount = openRoleSlots.Count,
            openRoleSlots = openRoleSlots.Select(slot => new
            {
                roleCode = slot.RoleCode,
                label = slot.Label,
                required = slot.IsRequired,
                recommended = slot.IsRecommended,
                interestCount = slot.InterestCount,
                externalCredentialVerificationRequired = slot.ExternalCredentialVerificationRequired
            }).ToArray(),
            requiresExplicitRoleAcceptance = true,
            platformDoesNotAssignWork = true,
            channels = new[] { "Push" }
        });
    }

    private static string BuildOpenRoleSummary(
        IReadOnlyList<CommunityPostPartyRoleSlotResponse> openRoleSlots)
    {
        const int visibleRoleCount = 3;
        var labels = openRoleSlots
            .Take(visibleRoleCount)
            .Select(slot => slot.Label)
            .ToArray();
        var remainingCount = openRoleSlots.Count - labels.Length;
        return remainingCount > 0
            ? $"{string.Join(", ", labels)} 외 {remainingCount}개"
            : string.Join(", ", labels);
    }

    private static string BuildRoleParticipationDeepLink(커뮤니티원장Dto 원장, string ledgerId)
    {
        if (원장.외부참조.TryGetValue(SourceCommunityPostIdKey, out var externalSourcePostId)
            && TryBuildCommunityPostDeepLink(externalSourcePostId, out var externalDeepLink))
        {
            return externalDeepLink;
        }

        var blockSourcePostId = 원장.블록목록
            .Select(block => block.Data.GetValueOrDefault(SourcePostIdKey))
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        return TryBuildCommunityPostDeepLink(blockSourcePostId, out var blockDeepLink)
            ? blockDeepLink
            : BuildLedgerDeepLink(ledgerId);
    }

    private static bool TryBuildCommunityPostDeepLink(string? value, out string deepLink)
    {
        if (long.TryParse(value, out var postId) && postId > 0)
        {
            deepLink = $"/community/posts/{postId}";
            return true;
        }

        deepLink = string.Empty;
        return false;
    }

    private static string BuildLedgerDeepLink(string ledgerId)
        => $"/community/group-purchase?ledgerId={Uri.EscapeDataString(ledgerId)}";

    private static bool TryReadRoleParticipation(
        커뮤니티원장Dto ledger,
        out string displayName,
        out string roleLabel,
        out bool specialistRole)
    {
        displayName = "새 참여자";
        roleLabel = "업무 참여";
        specialistRole = false;
        if (!ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.LastPartyRoleJoinRevisionAttributeKey,
                out var revisionValue)
            || !long.TryParse(revisionValue, out var joinRevision)
            || joinRevision != ledger.Revision
            || !ledger.확장속성.TryGetValue(
                CommunityPostProvisionalLedgerPolicy.LastPartyRoleJoinedRoleCodeAttributeKey,
                out var roleCode)
            || !CommunityPostPartyRoleCodes.IsSupported(roleCode))
        {
            return false;
        }

        displayName = ledger.확장속성.GetValueOrDefault(
            CommunityPostProvisionalLedgerPolicy.LastPartyRoleJoinedDisplayNameAttributeKey,
            displayName);
        roleLabel = CommunityPostProfessionalParticipationProjection.RoleLabel(roleCode, false);
        specialistRole = CommunityPostPartyRoleCodes.IsSpecialist(roleCode);
        return true;
    }

    public static string BuildTraceId(string? eventId, string fallback)
    {
        var source = string.IsNullOrWhiteSpace(eventId) ? fallback.Trim() : eventId.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant();
    }
}
