namespace Ssalddel.Contracts.Admin.Settlement;

public sealed class 기사지급승인요청
{
    public long TransportId { get; set; }

    public string ConfirmedRequestId { get; set; } = string.Empty;

    public decimal ConfirmedExpectedPayoutAmount { get; set; }

    public string CurrencyCode { get; set; } = "KRW";

    public string IdempotencyKey { get; set; } = string.Empty;

    public string ApprovalReason { get; set; } = string.Empty;
}

public sealed class 기사지급승인응답
{
    public long PayoutRequestId { get; set; }

    public long TransportId { get; set; }

    public string TransportNumber { get; set; } = string.Empty;

    public string RequestId { get; set; } = string.Empty;

    public string DriverId { get; set; } = string.Empty;

    public decimal ExpectedPayoutAmount { get; set; }

    public string CurrencyCode { get; set; } = "KRW";

    public string StatusCode { get; set; } = string.Empty;

    public string ExecutionModeCode { get; set; } = string.Empty;

    public string ApprovedBy { get; set; } = string.Empty;

    public DateTime ApprovedAtUtc { get; set; }

    public DateTime? SimulationVerifiedAtUtc { get; set; }

    public string OutboxStatusCode { get; set; } = string.Empty;

    public int OutboxAttemptCount { get; set; }

    public DateTime? NextAttemptAtUtc { get; set; }

    public string LastResultCode { get; set; } = string.Empty;

    public string LastResultMessage { get; set; } = string.Empty;

    public bool IsIdempotentReplay { get; set; }

    public bool IsActualTransferCompleted { get; set; }
}
