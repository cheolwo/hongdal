namespace Ssalddel.Application.CommandProcessing;

public static class Command기능명
{
    public const string AuditLog = "AuditLog";
    public const string WorkRelationshipSnapshot = "WorkRelationshipSnapshot";
    public const string Sms = "Sms";
    public const string Sns = "Sns";
    public const string Push = "Push";

    public static readonly string[] All = [AuditLog, WorkRelationshipSnapshot, Sms, Sns, Push];

    public static string 표시명(string featureName)
    {
        return featureName switch
        {
            AuditLog => "감사 로그",
            WorkRelationshipSnapshot => "인연 스냅샷",
            Sms => "SMS",
            Sns => "SNS",
            Push => "푸시 알림",
            _ => featureName
        };
    }
}
