using System;
using System.Linq;
using Ssalddel.Interior.Domain;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Hub H1부터 H4 준비도까지의 결정적 공간 조립 규칙을 검증한다.",
    Boundary = "규칙 자동 시험은 실제 Scene 배치·통행 또는 E5 증거가 아니다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3결정성검증)]
public sealed class SimulationSpatialCompositionTests
{
    [Fact]
    public void HubH1Evidence_UsesCanonicalInteriorPlacementPlanHash()
    {
        var plan = new DeterministicInteriorLayoutEngine().Generate(
            HubWarehouseInteriorGrammar.CreateRequest());

        Assert.Equal(
            PyeongchangHubSpatialCompositionFixture.PlacementPlanHashSha256,
            plan.InteriorPlacementPlanHashSha256);
        Assert.All(
            PyeongchangHubSpatialCompositionFixture.CreateH1Evidence(),
            evidence => Assert.Equal(plan.InteriorPlacementPlanHashSha256,
                evidence.PlacementPlanHashSha256));
    }

    [Fact]
    public void HubH1Evidence_FormsH2AndLeavesH3AndH4PartiallyReady()
    {
        var result = new SimulationSpatialCompositionEngine().Evaluate(
            PyeongchangHubSpatialCompositionFixture.CreateRequest(1, 1, true));

        var h2 = Required(result,
            PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2);
        Assert.Equal(SimulationSpatialCompositionCodes.Formed, h2.StateCode);
        Assert.Contains(result.Instances, value => value.DefinitionStableId ==
            PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2);

        var h3 = Required(result,
            PyeongchangHubSpatialCompositionCodes.JinbuHubH3);
        Assert.Equal(SimulationSpatialCompositionCodes.Blocked, h3.StateCode);
        Assert.Equal(new[]
        {
            PyeongchangHubSpatialCompositionCodes.OutboundVehicleH2,
        }, h3.MissingChildDefinitionStableIds);
        Assert.Contains("MissingRequiredChild:"
                        + PyeongchangHubSpatialCompositionCodes.OutboundVehicleH2,
            h3.BlockReasonCodes);

        var h4 = Required(result,
            PyeongchangHubSpatialCompositionCodes.AreaSetStableId);
        Assert.Equal(SimulationSpatialCompositionCodes.PartiallyReady,
            h4.StateCode);
        Assert.Equal(SimulationSpatialCompositionCodes.ReadinessOnly,
            h4.AuthorityCode);
        Assert.Equal(64, result.GraphHashSha256.Length);
    }

    [Fact]
    public void InputOrder_DoesNotChangeCompositionGraphHash()
    {
        var left = PyeongchangHubSpatialCompositionFixture.CreateRequest(
            1, 1, true);
        var right = PyeongchangHubSpatialCompositionFixture.CreateRequest(
            1, 1, true);
        right.ChildEvidence = right.ChildEvidence.Reverse().ToArray();
        right.RuleCatalog.Rules = right.RuleCatalog.Rules.Reverse().ToArray();
        right.RuleCatalog.CatalogHashSha256 =
            SimulationSpatialCompositionEngine.ComputeRuleCatalogHash(
                right.RuleCatalog);

        var engine = new SimulationSpatialCompositionEngine();
        Assert.Equal(engine.Evaluate(left).GraphHashSha256,
            engine.Evaluate(right).GraphHashSha256);
    }

    [Fact]
    public void LostH1Evidence_DegradesPreviouslyFormedH2()
    {
        var engine = new SimulationSpatialCompositionEngine();
        var formed = engine.Evaluate(
            PyeongchangHubSpatialCompositionFixture.CreateRequest(1, 1, true));
        var request = PyeongchangHubSpatialCompositionFixture.CreateRequest(
            2, 2, true, formed,
            PyeongchangHubSpatialCompositionFixture.CreateH1Evidence(
                outboundOperational: false));

        var degraded = engine.Evaluate(request);
        var h2 = Required(degraded,
            PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2);
        Assert.Equal(SimulationSpatialCompositionCodes.Degraded, h2.StateCode);
        Assert.Contains("ChildNotOperational:"
                        + PyeongchangHubSpatialCompositionCodes.OutboundStagingH1,
            h2.BlockReasonCodes);
        Assert.Contains(degraded.Instances, value => value.StateCode ==
            SimulationSpatialCompositionCodes.Degraded);
    }

    [Fact]
    public void MissingPlacementValidation_BlocksH2()
    {
        var request = PyeongchangHubSpatialCompositionFixture.CreateRequest(
            0, 0, false, evidence:
            PyeongchangHubSpatialCompositionFixture.CreateH1Evidence(
                placementValidated: false));

        var result = new SimulationSpatialCompositionEngine().Evaluate(request);

        Assert.Equal(SimulationSpatialCompositionCodes.Blocked,
            Required(result,
                PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2)
                .StateCode);
    }

    [Fact]
    public void StaleCatalogHash_IsRejected()
    {
        var request = PyeongchangHubSpatialCompositionFixture.CreateRequest(
            0, 0, false);
        request.RuleCatalog.CatalogHashSha256 = new string('0', 64);

        var error = Assert.Throws<SimulationContractException>(() =>
            new SimulationSpatialCompositionEngine().Evaluate(request));

        Assert.Equal("SimulationSpatialCompositionCatalogHashMismatch",
            error.ErrorCode);
    }

    [Fact]
    public void CyclicRuleCatalog_IsRejected()
    {
        var request = PyeongchangHubSpatialCompositionFixture.CreateRequest(
            0, 0, false);
        request.RuleCatalog.Rules[0].RequiredChildDefinitionStableIds =
            new[] { PyeongchangHubSpatialCompositionCodes.JinbuHubH3 };
        request.RuleCatalog.Rules[1].RequiredChildDefinitionStableIds =
            new[] { PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2 };
        request.RuleCatalog.CatalogHashSha256 =
            SimulationSpatialCompositionEngine.ComputeRuleCatalogHash(
                request.RuleCatalog);

        var error = Assert.Throws<SimulationContractException>(() =>
            new SimulationSpatialCompositionEngine().Evaluate(request));

        Assert.Equal("SimulationSpatialCompositionCycleDetected",
            error.ErrorCode);
    }

    private static SpatialCompositionAssessment Required(
        SimulationSpatialCompositionStateSnapshot state, string definitionId)
        => state.Assessments.Single(value =>
            value.TargetDefinitionStableId == definitionId);
}
