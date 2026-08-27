using Ssalddel.Interior.Contracts;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "LH가 생성된 실내 handle만 소비하고 재배치하지 않는 계약을 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀,
    WorkOrderIds = new[] { "E9-WO-TOWN-HOUSE-INTERIOR-LAYOUT" },
    Boundary = "계약 회귀는 actual H1 결속 또는 플레이 완료 증거가 아니다.")]
public sealed class SimulationLhInteriorPlanHandleServiceTests
{
    [Fact]
    public void Bind_MapsLhWindowRoleWithoutLettingLhGenerateInterior()
    {
        var handle = new InteriorPlanHandle
        {
            BuildingPlacementStableId = "town-building:01",
            H1StableId = "H1-TOWN-HOUSE-01",
            InteriorDefinitionRevision = "house.r1",
            ReferenceCatalogRevision = "catalog.r1",
            ReferenceCatalogHashSha256 = new string('b', 64),
            InteriorPlacementPlanHashSha256 = new string('a', 64),
        };
        var preview = new SimulationLhCellPreviewResponse
        {
            Cells =
            [
                Cell("cell:detail", SimulationLhWorldCodes.Detail, handle),
                Cell("cell:active", SimulationLhWorldCodes.Active, handle),
                Cell("cell:prefetch", SimulationLhWorldCodes.Prefetch, handle),
            ],
        };

        var result = new SimulationLhInteriorPlanHandleService().Bind(
            preview,
            new PresentationWorldDefinition { InteriorPlanHandles = [handle] });

        Assert.Equal(InteriorLayoutCodes.ObjectFocus, result.Single(x => x.CellKey == "cell:detail").FocusLevelCode);
        Assert.Equal(InteriorLayoutCodes.ZoneFocus, result.Single(x => x.CellKey == "cell:active").FocusLevelCode);
        Assert.Equal(InteriorLayoutCodes.OverviewFocus, result.Single(x => x.CellKey == "cell:prefetch").FocusLevelCode);
        Assert.All(result, item =>
        {
            Assert.False(item.LhDeterminesPlacement);
            Assert.Single(item.PlanHandles);
        });
    }

    private static SimulationLhCellPlanResponse Cell(
        string cellKey,
        string roleCode,
        InteriorPlanHandle handle)
        => new()
        {
            CellKey = cellKey,
            WindowRoleCode = roleCode,
            Placements =
            [
                new SimulationLhPlacementResponse
                {
                    GeneratedStableId = handle.BuildingPlacementStableId,
                    H1StableId = handle.H1StableId,
                },
            ],
        };
}
