namespace 홍달.Services.Audit;

public sealed class 사용자행위로그기록
{
    public string AppKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string MetadataJson { get; set; } = string.Empty;
}
