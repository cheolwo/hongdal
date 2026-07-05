using System.Text.Json;
using Hongdal.Application.HumanResources;
using Hongdal.Contracts.Common.Hr;
using Microsoft.EntityFrameworkCore;
using MediatR;
using 홍달.Data;

namespace Hongdal.Application.Immigration;

public sealed class VisaSupportAdministrativeAgentNotificationHandler : INotificationHandler<VisaSupportRequestedEvent>
{
    private readonly HongdalContext _db;
    private readonly IHrRoleAssignmentStore _roleAssignmentStore;
    private readonly ILogger<VisaSupportAdministrativeAgentNotificationHandler> _logger;

    public VisaSupportAdministrativeAgentNotificationHandler(
        HongdalContext db,
        IHrRoleAssignmentStore roleAssignmentStore,
        ILogger<VisaSupportAdministrativeAgentNotificationHandler> logger)
    {
        _db = db;
        _roleAssignmentStore = roleAssignmentStore;
        _logger = logger;
    }

    public async Task Handle(VisaSupportRequestedEvent notification, CancellationToken cancellationToken)
    {
        var targets = await ResolveAdministrativeAgentsAsync(cancellationToken);
        if (targets.Count == 0)
        {
            _logger.LogInformation(
                "비자 행정지원 알림 대상 행정사가 없습니다. RequestId={RequestId}, Country={Country}",
                notification.RequestId,
                notification.ForeignPartnerCountry);
            return;
        }

        var outboxType = ResolveCommandNotificationOutboxType();
        if (outboxType is null)
        {
            _logger.LogWarning("Command 알림 Outbox 엔티티를 찾지 못해 비자 행정지원 알림을 적재하지 못했습니다. RequestId={RequestId}", notification.RequestId);
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var target in targets)
        {
            var payload = JsonSerializer.Serialize(new
            {
                TargetUserId = target.UserId,
                notification.RequestId,
                notification.RequesterUserId,
                notification.ForeignPartnerName,
                notification.ForeignPartnerCountry,
                notification.ForeignPartnerCompanyName,
                notification.ImporterUserId,
                notification.RelatedOrderReference,
                notification.DesiredVisaType,
                notification.SupportMemo,
                notification.RequestedAtUtc,
                NotificationType = "VisaAdministrativeSupportRequested"
            });

            var entity = Activator.CreateInstance(outboxType)
                ?? throw new InvalidOperationException("Command 알림 Outbox 인스턴스를 생성할 수 없습니다.");

            SetProperty(entity, "CommandName", nameof(VisaSupportRequestCommand));
            SetProperty(entity, "EventName", nameof(VisaSupportRequestedEvent));
            SetProperty(entity, "FeatureName", "ImmigrationVisaSupport");
            SetProperty(entity, "Target", "AdministrativeAgent");
            SetProperty(entity, "PayloadJson", payload);
            SetProperty(entity, "Status", "Pending");
            SetProperty(entity, "TraceId", notification.TraceId);
            SetProperty(entity, "CreatedAt", now);
            SetProperty(entity, "UpdatedAt", now);

            _db.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "비자 행정지원 알림 의도 적재 완료: RequestId={RequestId}, 대상수={TargetCount}",
            notification.RequestId,
            targets.Count);
    }

    private async Task<IReadOnlyList<HrRoleAssignment>> ResolveAdministrativeAgentsAsync(CancellationToken cancellationToken)
    {
        var scoped = await _roleAssignmentStore.ListAsync(
            userId: null,
            scopeType: HrScopeTypes.Immigration,
            scopeId: HrScopeIds.Global,
            cancellationToken);

        var platform = await _roleAssignmentStore.ListAsync(
            userId: null,
            scopeType: HrScopeTypes.Platform,
            scopeId: HrScopeIds.Global,
            cancellationToken);

        return scoped.Concat(platform)
            .Where(x => string.Equals(x.RoleCode, HrDetailedRoleCodes.ImmigrationVisaAgent, StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.UserId, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToArray();
    }

    private Type? ResolveCommandNotificationOutboxType()
    {
        return _db.Model.GetEntityTypes()
            .Select(x => x.ClrType)
            .FirstOrDefault(type =>
                type.GetProperty("CommandName") is not null
                && type.GetProperty("EventName") is not null
                && type.GetProperty("FeatureName") is not null
                && type.GetProperty("Target") is not null
                && type.GetProperty("PayloadJson") is not null
                && type.GetProperty("Status") is not null
                && type.GetProperty("TraceId") is not null);
    }

    private static void SetProperty(object entity, string propertyName, object value)
    {
        var property = entity.GetType().GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Command 알림 Outbox에 {propertyName} 속성이 없습니다.");

        property.SetValue(entity, value);
    }
}
