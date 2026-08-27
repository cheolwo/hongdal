using System;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Unity.Data;
using Xunit;

namespace Ssalddel.Unity.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Unity 영역 건물 읽기 모델의 승인 자료 필터와 fail-closed 경계를 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3Unity소비자회귀,
    WorkOrderIds = new[] { "E9-WO-NATURE-AREA-BUILDING-PROGRESSION" },
    Boundary = "엔진 독립 소비자 시험은 저장 Scene·Play Mode·Game View 증거가 아니다.")]
public sealed class AreaBuildingProgressionPresentationTests
{
    [Fact]
    public void 투영은_잠금이유와승인가르침만보이고_권위상태를거부한다()
    {
        var source = new 영역건물발전ApiModel
        {
            CatalogRevision = "area-building-tech-tree.r1",
            CatalogHashSha256 = new string('A', 64),
            AreaCode = "Nature",
            AreaSetStableId = "area-set:nature",
            SimulationOnly = true,
            Nodes = new[]
            {
                new 건물발전NodeApiModel
                {
                    BlueprintStableId = "blueprint:nature-learning-lodge.v1",
                    KoreanName = "자연 배움터",
                    StageCode = "Specialization",
                    StateCode = "Locked",
                    BlockingReasonCodes = new[] { "BlueprintLocked" },
                    RequiredWorkSeconds = 60,
                    CompletedWorkSeconds = 30,
                },
            },
            ApprovedTeachingMaterials = new[]
            {
                new 승인가르침자료ApiModel
                {
                    TeachingMaterialStableId = "teaching:approved",
                    AdminApproved = true,
                    KoreanTitle = "승인 자료",
                },
                new 승인가르침자료ApiModel
                {
                    TeachingMaterialStableId = "teaching:pending",
                    AdminApproved = false,
                    KoreanTitle = "검토 대기",
                },
            },
        };

        var result = 영역건물발전PresentationProjection.Map(source);

        Assert.Single(result.Nodes);
        Assert.Equal(.5f, result.Nodes[0].WorkProgress01);
        Assert.Equal("BlueprintLocked", result.Nodes[0].BlockingReasonCodes[0]);
        Assert.Single(result.TeachingMaterials);
        Assert.Equal("teaching:approved", result.TeachingMaterials[0].StableId);

        source.IsOperationalState = true;
        Assert.Throws<InvalidOperationException>(() =>
            영역건물발전PresentationProjection.Map(source));
    }
}
