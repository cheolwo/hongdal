using Ssalddel.Contracts.Common.WarehouseBilling;

namespace Ssalddel.Tests.Contracts.Common.WarehouseBilling;

public sealed class WarehouseBillingPlannerTests
{
    [Fact]
    public void Plan_CalculatesWorkChargesAndTax()
    {
        var draft = WarehouseBillingPlanner.Plan(
            "agent-1",
            "customer-1",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31),
            [
                new(WarehouseBillingChargeCode.InboundInspection, 10, ReferenceId: "INB-1"),
                new(WarehouseBillingChargeCode.BarcodeLabeling, 8, ReferenceId: "INB-1"),
                new(WarehouseBillingChargeCode.BundleHandling, 2, ReferenceId: "BND-1")
            ],
            WarehouseBillingRateCatalog.CreateDefaultRates());

        Assert.Equal(4960m, draft.SubtotalAmount);
        Assert.Equal(496m, draft.TaxAmount);
        Assert.Equal(5456m, draft.TotalAmount);
    }

    [Fact]
    public void Plan_CalculatesStorageByOverlappedDays()
    {
        var draft = WarehouseBillingPlanner.Plan(
            "agent-1",
            "customer-1",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 7),
            [
                new(
                    WarehouseBillingChargeCode.StorageDaily,
                    2.5m,
                    StartedOn: new DateOnly(2026, 6, 30),
                    EndedOn: new DateOnly(2026, 7, 3),
                    ReferenceId: "SKU-1")
            ],
            WarehouseBillingRateCatalog.CreateDefaultRates(),
            taxRate: 0m);

        var line = Assert.Single(draft.Lines);
        Assert.Equal(7.5m, line.Quantity);
        Assert.Equal(600m, line.Amount);
        Assert.Equal(600m, draft.TotalAmount);
    }

    [Fact]
    public void Plan_IgnoresStorageOutsideBillingPeriod()
    {
        var draft = WarehouseBillingPlanner.Plan(
            "agent-1",
            "customer-1",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 7),
            [
                new(
                    WarehouseBillingChargeCode.StorageDaily,
                    1m,
                    StartedOn: new DateOnly(2026, 6, 1),
                    EndedOn: new DateOnly(2026, 6, 30))
            ],
            WarehouseBillingRateCatalog.CreateDefaultRates());

        Assert.Empty(draft.Lines);
        Assert.Equal(0m, draft.TotalAmount);
    }

    [Fact]
    public void Plan_RejectsMissingRate()
    {
        Assert.Throws<InvalidOperationException>(() => WarehouseBillingPlanner.Plan(
            "agent-1",
            "customer-1",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31),
            [new("custom-work", 1)],
            WarehouseBillingRateCatalog.CreateDefaultRates()));
    }
}
