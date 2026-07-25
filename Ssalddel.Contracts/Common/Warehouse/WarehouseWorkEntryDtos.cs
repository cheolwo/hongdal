namespace Ssalddel.Contracts.Common.Warehouse;

public sealed class 창고작업진입확인요청
{
    public string ProcessCode { get; set; } = string.Empty;
}

public sealed class 창고작업진입확인응답
{
    public bool IsAllowed { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
