using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Tests.Contracts.Common.Orderer;

public sealed class GroupPurchasePaymentMilestonePlannerTests
{
    [Fact]
    public void Plan_PickupCompleted_RequestsFirstPaymentOnly()
    {
        var plan = GroupPurchasePaymentMilestonePlanner.Plan(new GroupPurchasePaymentMilestoneDraft(
            GroupPurchaseId: "gp-1",
            OrdererId: "orderer-1",
            TotalAmount: 100000m,
            IsPickupCompleted: true,
            IsDropoffCompleted: false,
            DistributionConfirmationRate: 0m));

        Assert.Equal(40000m, plan.RequestableAmount);
        Assert.Contains(plan.Lines, x =>
            x.MilestoneCode == GroupPurchasePaymentMilestoneCode.PickupFirstPayment &&
            x.Status == GroupPurchasePaymentMilestoneStatusCode.Requestable);
        Assert.Contains(plan.Lines, x =>
            x.MilestoneCode == GroupPurchasePaymentMilestoneCode.DropoffSecondPayment &&
            x.Status == GroupPurchasePaymentMilestoneStatusCode.Blocked);
    }

    [Fact]
    public void Plan_DropoffCompletedAfterFirstPayment_RequestsSecondPayment()
    {
        var plan = GroupPurchasePaymentMilestonePlanner.Plan(new GroupPurchasePaymentMilestoneDraft(
            GroupPurchaseId: "gp-1",
            OrdererId: "orderer-1",
            TotalAmount: 100000m,
            IsPickupCompleted: true,
            IsDropoffCompleted: true,
            DistributionConfirmationRate: 0.3m,
            PaidMilestoneCodes: new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                GroupPurchasePaymentMilestoneCode.PickupFirstPayment
            }));

        Assert.Equal(40000m, plan.PaidAmount);
        Assert.Equal(40000m, plan.RequestableAmount);
        Assert.True(plan.IsFinalPaymentBlocked);
    }

    [Fact]
    public void Plan_DistributionConfirmed_RequestsFinalPayment()
    {
        var plan = GroupPurchasePaymentMilestonePlanner.Plan(new GroupPurchasePaymentMilestoneDraft(
            GroupPurchaseId: "gp-1",
            OrdererId: "orderer-1",
            TotalAmount: 100000m,
            IsPickupCompleted: true,
            IsDropoffCompleted: true,
            DistributionConfirmationRate: 0.82m,
            PaidMilestoneCodes: new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                GroupPurchasePaymentMilestoneCode.PickupFirstPayment,
                GroupPurchasePaymentMilestoneCode.DropoffSecondPayment
            }));

        Assert.Equal(80000m, plan.PaidAmount);
        Assert.Equal(20000m, plan.RequestableAmount);
        Assert.False(plan.IsFinalPaymentBlocked);
        Assert.Contains(plan.Lines, x =>
            x.MilestoneCode == GroupPurchasePaymentMilestoneCode.DistributionConfirmationFinalPayment &&
            x.Status == GroupPurchasePaymentMilestoneStatusCode.Requestable);
    }

    [Fact]
    public void Plan_RejectsRatesThatDoNotSumToOne()
    {
        var draft = new GroupPurchasePaymentMilestoneDraft(
            GroupPurchaseId: "gp-1",
            OrdererId: "orderer-1",
            TotalAmount: 100000m,
            IsPickupCompleted: true,
            IsDropoffCompleted: true,
            DistributionConfirmationRate: 1m,
            Policy: new GroupPurchasePaymentMilestonePolicy(
                PickupFirstPaymentRate: 0.5m,
                DropoffSecondPaymentRate: 0.4m,
                DistributionFinalPaymentRate: 0.2m));

        Assert.Throws<ArgumentException>(() => GroupPurchasePaymentMilestonePlanner.Plan(draft));
    }
}
