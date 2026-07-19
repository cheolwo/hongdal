using System.Security.Cryptography;
using System.Text;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Domain.HumanResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 살뜰.Data;
using 살뜰.Services.Options;

namespace Ssalddel.Services.HumanResources;

public sealed class WorkRelationshipSnapshotService : IWorkRelationshipSnapshotService
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<WorkRelationshipSnapshotOptions> _options;

    public WorkRelationshipSnapshotService(
        SsalddelContext db,
        ICurrentUserAccessor currentUserAccessor,
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<WorkRelationshipSnapshotOptions> options)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    public async Task RecordAsync(WorkRelationshipSnapshotRecordRequest request, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }

        var actorUserId = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(request.WorkDomain)
            || string.IsNullOrWhiteSpace(request.WorkProcess)
            || string.IsNullOrWhiteSpace(request.ActionCode)
            || string.IsNullOrWhiteSpace(request.RelatedEntityType)
            || string.IsNullOrWhiteSpace(request.RelatedEntityId))
        {
            return;
        }

        var context = _httpContextAccessor.HttpContext;
        var now = DateTime.UtcNow;
        var actorRoleCode = _currentUserAccessor.Role ?? string.Empty;

        _db.WorkRelationshipSnapshots.Add(new WorkRelationshipSnapshotRecord
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            ActorAnonymousLabel = CreateAnonymousLabel(actorUserId),
            ActorRoleCode = actorRoleCode,
            ActorRoleName = actorRoleCode,
            WorkDomain = request.WorkDomain.Trim(),
            WorkProcess = request.WorkProcess.Trim(),
            ActionCode = request.ActionCode.Trim(),
            ActionLabel = TrimOrEmpty(request.ActionLabel),
            RelatedEntityType = request.RelatedEntityType.Trim(),
            RelatedEntityId = request.RelatedEntityId.Trim(),
            RelatedDisplayLabel = TrimOrEmpty(request.RelatedDisplayLabel),
            CounterpartyUserId = NormalizeNullable(request.CounterpartyUserId),
            CounterpartyAnonymousLabel = string.IsNullOrWhiteSpace(request.CounterpartyUserId)
                ? null
                : CreateAnonymousLabel(request.CounterpartyUserId),
            CounterpartyRoleCode = NormalizeNullable(request.CounterpartyRoleCode),
            PrivacyLevel = NormalizePrivacyLevel(request.PrivacyLevel),
            Memo = TrimOrEmpty(request.Memo),
            AppKey = context?.Request.Headers["X-App-Key"].ToString() ?? string.Empty,
            TraceId = context?.TraceIdentifier ?? string.Empty,
            ClientIpSnapshot = context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            OccurredAtUtc = now,
            CreatedAtUtc = now
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<WorkRelationshipSnapshotListResponse> GetMineAsync(int take, CancellationToken cancellationToken)
    {
        var actorUserId = _currentUserAccessor.UserId;
        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            return new WorkRelationshipSnapshotListResponse();
        }

        var safeTake = Math.Clamp(take, 1, 100);
        var items = await _db.WorkRelationshipSnapshots
            .AsNoTracking()
            .Where(x => x.ActorUserId == actorUserId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(safeTake)
            .Select(x => new WorkRelationshipSnapshotResponse
            {
                Id = x.Id,
                ActorAnonymousLabel = x.ActorAnonymousLabel,
                ActorRoleCode = x.ActorRoleCode,
                ActorRoleName = x.ActorRoleName,
                WorkDomain = x.WorkDomain,
                WorkProcess = x.WorkProcess,
                ActionCode = x.ActionCode,
                ActionLabel = x.ActionLabel,
                RelatedEntityType = x.RelatedEntityType,
                RelatedEntityId = x.RelatedEntityId,
                RelatedDisplayLabel = x.RelatedDisplayLabel,
                CounterpartyAnonymousLabel = x.CounterpartyAnonymousLabel,
                CounterpartyRoleCode = x.CounterpartyRoleCode,
                PrivacyLevel = x.PrivacyLevel,
                Memo = x.Memo,
                OccurredAtUtc = x.OccurredAtUtc
            })
            .ToArrayAsync(cancellationToken);

        return new WorkRelationshipSnapshotListResponse { Items = items };
    }

    private static string NormalizePrivacyLevel(string? value)
        => value?.Trim() switch
        {
            WorkRelationshipPrivacyLevels.PrivateInternal => WorkRelationshipPrivacyLevels.PrivateInternal,
            WorkRelationshipPrivacyLevels.ConnectionRequestEligible => WorkRelationshipPrivacyLevels.ConnectionRequestEligible,
            _ => WorkRelationshipPrivacyLevels.ActorVisibleAnonymized
        };

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string TrimOrEmpty(string? value)
        => value?.Trim() ?? string.Empty;

    private static string CreateAnonymousLabel(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value.Trim()));
        var suffix = Convert.ToHexString(bytes).ToLowerInvariant()[..8];
        return $"user-{suffix}";
    }
}
