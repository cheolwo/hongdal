using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "작업 참여 공통 정책의 Solo 가능·협력 권장·도움 권한·호혜 보수 판정을 자동시험으로 검증한다.",
    Boundary = "실제 Farm WI, NPC 이동, Save 또는 Unity Play Mode 증거를 대신하지 않는다.")]
public sealed class SimulationWorkParticipationPolicyTests
{
    [Fact]
    public void 작업참여Policy는_기존작업체계를재사용하는_비실행공통뼈대다()
    {
        var catalog = Simulation작업참여PolicyCatalog.Create();

        Assert.Equal(Simulation작업참여PolicyCodes.MetadataOnly,
            catalog.ExecutionModeCode);
        Assert.False(catalog.IsExecutable);
        Assert.False(catalog.OwnsPreviewConfirmTaskEffect);
        Assert.Contains(nameof(SimulationCoopContributionSnapshot),
            catalog.ReusedSystemRefs);
        Assert.Contains(nameof(SimulationFarmWorkPreviewSnapshot),
            catalog.ReusedSystemRefs);
        Assert.Contains(nameof(SimulationNpcTaskAssignmentSnapshot),
            catalog.ReusedSystemRefs);
        Assert.Equal(64, catalog.CatalogHashSha256.Length);
        Assert.Equal(catalog.CatalogHashSha256,
            Simulation작업참여PolicyCatalog.Create().CatalogHashSha256);
    }

    [Fact]
    public void 기본농지는_플레이어혼자시도할수있다()
    {
        var result = Simulation작업참여PolicyCatalog.AssessWorkload(
            new Simulation작업부담평가Request());

        Assert.Equal(Simulation작업참여PolicyCodes.SoloFriendly,
            result.WorkloadCode);
        Assert.True(result.CanAttemptSolo);
        Assert.False(result.CollaborationRecommended);
        Assert.True(result.ProgressPreservedOnPause);
        Assert.Empty(result.ActiveBurdenCodes);
    }

    [Fact]
    public void 면적이나현장난도가커져도_혼자시도는가능하고_협력을권한다()
    {
        var result = Simulation작업참여PolicyCatalog.AssessWorkload(
            new Simulation작업부담평가Request
            {
                IsLargeArea = true,
                DifficultyCodes = new[]
                {
                    Simulation작업참여PolicyCodes.DistantWaterSource,
                    Simulation작업참여PolicyCodes.SteepSlope,
                    Simulation작업참여PolicyCodes.SteepSlope,
                },
            });

        Assert.Equal(
            Simulation작업참여PolicyCodes.CollaborationHelpful,
            result.WorkloadCode);
        Assert.True(result.CanAttemptSolo);
        Assert.True(result.CollaborationRecommended);
        Assert.Equal(4, result.ActiveBurdenCodes.Length);
        Assert.Contains(Simulation작업참여PolicyCodes.TimeBurden,
            result.ActiveBurdenCodes);
        Assert.Empty(result.BlockReasonCodes);
    }

    [Fact]
    public void 현재도구로물리적으로불가능한작업만_별도차단한다()
    {
        var result = Simulation작업참여PolicyCatalog.AssessWorkload(
            new Simulation작업부담평가Request
            {
                CurrentToolCanPerform = false,
                DifficultyCodes = new[]
                {
                    Simulation작업참여PolicyCodes.EmbeddedRock,
                },
            });

        Assert.Equal(Simulation작업참여PolicyCodes.PhysicallyBlocked,
            result.WorkloadCode);
        Assert.False(result.CanAttemptSolo);
        Assert.Contains(
            Simulation작업참여PolicyCodes.CurrentToolCannotPerform,
            result.BlockReasonCodes);
    }

    [Fact]
    public void 가벼운도움은_기본자동허용이지만_플레이어가끌수있다()
    {
        var rule = Simulation작업참여PolicyCatalog.ResolveAssistance(
            Simulation작업참여PolicyCodes.WeedClearing);

        Assert.Equal(Simulation작업참여PolicyCodes.LightAssistance,
            rule.AssistanceClassCode);
        Assert.Equal(Simulation작업참여PolicyCodes.DefaultAutoAllowed,
            rule.DefaultPermissionCode);
        Assert.True(rule.PlayerMayDisableAutoHelp);
        Assert.True(rule.RequiresAuthorityCommandRecord);
        Assert.False(rule.MayMutatePlayerPlanOrOwnedWorldState);
    }

    [Theory]
    [InlineData(Simulation작업참여PolicyCodes.ResourceConsumption)]
    [InlineData(Simulation작업참여PolicyCodes.TerrainMutation)]
    [InlineData(Simulation작업참여PolicyCodes.ConstructionOrDemolition)]
    public void 중요한세계변경도움은_명시적확인을요구한다(
        string actionCode)
    {
        var rule = Simulation작업참여PolicyCatalog.ResolveAssistance(
            actionCode);

        Assert.Equal(
            Simulation작업참여PolicyCodes.ExplicitConfirmRequired,
            rule.DefaultPermissionCode);
        Assert.True(rule.MayMutatePlayerPlanOrOwnedWorldState);
    }

    [Fact]
    public void 가벼운도움은_호혜원장이고_전문노동은_사전보수합의다()
    {
        var light = Simulation작업참여PolicyCatalog.ResolveCompensation(
            Simulation작업참여PolicyCodes.LightAssistance);
        var professional =
            Simulation작업참여PolicyCatalog.ResolveCompensation(
                Simulation작업참여PolicyCodes.ProfessionalWork);

        Assert.Equal(
            Simulation작업참여PolicyCodes.ReciprocityContributionLedger,
            light.SettlementCode);
        Assert.False(light.CompensationAgreementRequiredBeforeWork);
        Assert.True(light.ContributionRecordRequired);
        Assert.Equal(
            Simulation작업참여PolicyCodes.PreAgreedCompensation,
            professional.SettlementCode);
        Assert.True(professional.CompensationAgreementRequiredBeforeWork);
    }

    [Fact]
    public void 알수없는도움과난도는_조용히추정하지않는다()
    {
        Assert.Throws<SimulationContractException>(() =>
            Simulation작업참여PolicyCatalog.ResolveAssistance(
                "UnknownHelp"));
        Assert.Throws<SimulationContractException>(() =>
            Simulation작업참여PolicyCatalog.AssessWorkload(
                new Simulation작업부담평가Request
                {
                    DifficultyCodes = new[] { "UnknownDifficulty" },
                }));
    }
}
