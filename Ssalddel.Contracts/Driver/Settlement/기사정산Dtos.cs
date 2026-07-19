namespace Ssalddel.Contracts.Driver.Settlement;

public sealed class 기사정산응답
{
    public string DriverId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int DispatchCount { get; set; }
    public decimal UsageFee { get; set; }
    public decimal MonthlyFeeCap { get; set; }
    public decimal RemainingUntilCap { get; set; }
    public bool IsPaid { get; set; }
}

public sealed class 기사정산월요약응답
{
    public string DriverId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int DispatchCount { get; set; }
    public decimal UsageFee { get; set; }
    public decimal MonthlyFeeCap { get; set; }
    public decimal RemainingUntilCap { get; set; }
    public bool IsPaid { get; set; }
}