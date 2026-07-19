using Ssalddel.Application.Abstractions;
using 살뜰.Services.Options;

namespace Ssalddel.Application.CommandProcessing;

public static class Command후처리규칙
{
    public static bool IsCommandRequest(object request)
    {
        return request is ICommand
               || request.GetType().GetInterfaces().Any(IsGenericCommandInterface);
    }

    public static bool HasEnabled후처리Feature(CommandProcessingRule rule, bool canHandleWorkRelationshipSnapshot)
    {
        return IsAuditLogEnabled(rule)
               || Is알림FeatureEnabled(rule)
               || (canHandleWorkRelationshipSnapshot && IsWorkRelationshipSnapshotEnabled(rule));
    }

    public static bool IsAuditLogEnabled(CommandProcessingRule rule)
    {
        return rule.AuditLogEnabled.GetValueOrDefault();
    }

    public static bool IsWorkRelationshipSnapshotEnabled(CommandProcessingRule rule)
    {
        return rule.WorkRelationshipSnapshotEnabled.GetValueOrDefault();
    }

    public static bool Is알림FeatureEnabled(CommandProcessingRule rule)
    {
        return rule.SmsEnabled.GetValueOrDefault()
               || rule.SnsEnabled.GetValueOrDefault()
               || rule.PushEnabled.GetValueOrDefault();
    }

    private static bool IsGenericCommandInterface(Type type)
    {
        return type.IsGenericType
               && type.GetGenericTypeDefinition() == typeof(ICommand<>);
    }
}
