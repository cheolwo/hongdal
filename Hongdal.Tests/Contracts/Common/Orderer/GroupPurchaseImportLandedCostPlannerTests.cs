using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Tests.Contracts.Common.Orderer;

public sealed class GroupPurchaseImportLandedCostPlannerTests
{
    [Fact]
    public void Plan_SeparatesCifTaxAndDomestic3plInboundCosts()
    {
        var plan = GroupPurchaseImportLandedCostPlanner.Plan(new ImportLandedCostDraft(
            QuantityKg: 100m,
            ProductPurchaseUnitPriceKrw: 5000m,
            OverseasHandlingUnitCostKrw: 500m,
            InternationalFreightInsuranceUnitCostKrw: 1000m,
            CustomsDutyRate: 0.08m,
            ImportVatRate: 0.1m,
            BondedWarehouseUnitCostKrw: 200m,
            CustomsBrokerageUnitCostKrw: 100m,
            DomesticTransportTo3plUnitCostKrw: 300m,
            ThreePlInboundUnitCostKrw: 400m,
            CustomsReviewRequired: true));

        Assert.Equal(6500m, plan.EstimatedCifUnitPriceKrw);
        Assert.Equal(7020m, plan.Stages.Single(x => x.StageCode == ImportLandedCostStageCode.CustomsDuty).AccumulatedUnitCostKrw);
        Assert.Equal(7722m, plan.EstimatedAfterTaxUnitCostKrw);
        Assert.Equal(8722m, plan.EstimatedLandedUnitCostKrw);
        Assert.Equal(872200m, plan.EstimatedLandedTotalKrw);
        Assert.Equal(ImportLandedCostStageStatusCode.NeedsReview, plan.Stages.Single(x => x.StageCode == ImportLandedCostStageCode.CustomsReview).StatusCode);
    }

    [Fact]
    public void Plan_CustomsRejected_MarksReviewBlockedAndWarns()
    {
        var plan = GroupPurchaseImportLandedCostPlanner.Plan(new ImportLandedCostDraft(
            QuantityKg: 10m,
            ProductPurchaseUnitPriceKrw: 3000m,
            CustomsRejected: true));

        Assert.Equal(ImportLandedCostStageStatusCode.Blocked, plan.Stages.Single(x => x.StageCode == ImportLandedCostStageCode.CustomsReview).StatusCode);
        Assert.NotEmpty(plan.Warnings);
    }
}
