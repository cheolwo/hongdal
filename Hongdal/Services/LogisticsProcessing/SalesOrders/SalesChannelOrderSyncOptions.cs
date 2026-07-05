namespace Hongdal.Services.LogisticsProcessing.SalesOrders;

public sealed class SalesChannelOrderSyncOptions
{
    public const string SectionName = "SalesChannelOrderSync";

    public bool Enabled { get; set; } = true;

    public int DomesticSyncIntervalSeconds { get; set; } = 300;

    public int OverseasSyncIntervalSeconds { get; set; } = 600;

    public int LookbackMinutes { get; set; } = 120;

    public int BatchSize { get; set; } = 100;
}
