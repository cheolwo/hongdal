namespace Hongdal.Services.Community;

public sealed class CommunityGroupPurchaseDemandHandoffRequest
{
    public Guid VoteId { get; set; }
    public long? SourcePostId { get; set; }
    public string? CommunityLedgerId { get; set; }
    public string VoterHash { get; set; } = string.Empty;
    public string VoterDisplayName { get; set; } = string.Empty;
    public string OptionId { get; set; } = string.Empty;
    public string ProductKey { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string HsCode { get; set; } = string.Empty;
    public string TemperatureCode { get; set; } = "상온";
    public string LogisticsMode { get; set; } = "LCL";
    public string DeliveryScopeKey { get; set; } = string.Empty;
    public string DeliveryScopeName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public string QuantityUnit { get; set; } = "개";
    public int MinimumParticipantCount { get; set; }
    public int MinimumTotalQuantity { get; set; }
}

public interface ICommunityGroupPurchaseDemandHandoff
{
    Task<string> SyncAsync(
        CommunityGroupPurchaseDemandHandoffRequest request,
        CancellationToken cancellationToken);
}

internal sealed class NoOpCommunityGroupPurchaseDemandHandoff : ICommunityGroupPurchaseDemandHandoff
{
    public Task<string> SyncAsync(
        CommunityGroupPurchaseDemandHandoffRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(string.Empty);
    }
}
