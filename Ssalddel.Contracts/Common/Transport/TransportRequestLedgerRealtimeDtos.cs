namespace Ssalddel.Contracts.Common.Transport;

public static class TransportRequestLedgerRealtime
{
    public const string HubPath = "/hubs/transport-ledger";
    public const string ChangedMethod = "TransportRequestLedgerChanged";
}

public sealed record TransportRequestLedgerChangedResponse(
    string RequestId,
    string? RequestStatus,
    string? PaymentStatus,
    string? DispatchStatus,
    string? SettlementStatus,
    string? TransportStatus,
    DateTimeOffset ChangedAtUtc,
    string Source,
    string? EventType = null);
