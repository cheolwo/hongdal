using System.Reflection;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Controllers.Admin.Content07;
using Ssalddel.Services.Content;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.External.KieAi;

namespace Ssalddel.Tests.Architecture;

public sealed class SsalddelCodeMetadataTests
{
    [Fact]
    public void CommunityAuthoringImage_MetadataBuildsSearchableVerticalSlice()
    {
        var metadata = ReadFeatureMetadata();

        Assert.Contains(metadata, item => item.ComponentType == typeof(CommunityAuthoringImagePromptPlanRequest));
        Assert.Contains(metadata, item => item.ComponentType == typeof(CommunityAuthoringImageGeneratorViewModel));
        Assert.Contains(metadata, item => item.ComponentType == typeof(ICommunityAuthoringImageClient));
        Assert.Contains(metadata, item => item.ComponentType == typeof(커뮤니티작성이미지Controller));
        Assert.Contains(metadata, item => item.ComponentType == typeof(CommunityAuthoringImagePromptPlanner));
        Assert.Contains(metadata, item => item.ComponentType == typeof(CommunityAuthoringImageService));
        Assert.Contains(metadata, item => item.ComponentType == typeof(KieAiImageGenerationClient));
        Assert.Contains(metadata, item => item.ComponentType.Name == "CommunityAuthoringImageContextSegmenter");
        Assert.Contains(metadata, item => item.ComponentType.Name == "CommunityAuthoringImagePromptFactory");

        Assert.Equal(
            metadata.OrderBy(item => item.FlowOrder).ThenBy(item => item.ComponentType.FullName, StringComparer.Ordinal),
            metadata);
    }

    [Fact]
    public void CommunityAuthoringImage_MetadataMakesPureAndPaidBoundariesExplicit()
    {
        var metadata = ReadFeatureMetadata();
        var planner = Assert.Single(metadata, item => item.ComponentType == typeof(CommunityAuthoringImagePromptPlanner));
        var imageService = Assert.Single(metadata, item => item.ComponentType == typeof(CommunityAuthoringImageService));
        var providerClient = Assert.Single(metadata, item => item.ComponentType == typeof(KieAiImageGenerationClient));

        Assert.Equal(SsalddelCodeEffect.None, planner.Effects);
        Assert.True(imageService.Effects.HasFlag(SsalddelCodeEffect.PersistentWrite));
        Assert.True(imageService.Effects.HasFlag(SsalddelCodeEffect.MayIncurExternalCost));
        Assert.True(providerClient.Effects.HasFlag(SsalddelCodeEffect.ThirdPartyApiCall));
        Assert.True(providerClient.Effects.HasFlag(SsalddelCodeEffect.MayIncurExternalCost));
        Assert.All(metadata, item => Assert.False(string.IsNullOrWhiteSpace(item.Boundary)));
    }

    [Fact]
    public void 커뮤니티작성이미지Controller는_Route와커뮤니티Metadata를유지한다()
    {
        var controller = typeof(커뮤니티작성이미지Controller);
        var route = controller.GetCustomAttribute<RouteAttribute>();
        var authorize = controller.GetCustomAttribute<AuthorizeAttribute>();
        var workflow = controller.GetCustomAttribute<SsalddelApiWorkflowAttribute>();
        var growthTrack = controller.GetCustomAttribute<SsalddelApiGrowthTrackAttribute>();

        Assert.Equal("api/v1/admin/content/information/authoring/images", route?.Template);
        Assert.Equal("서버관리자전용", authorize?.Policy);
        Assert.Equal(SsalddelWorkflow.CommunityTrust, workflow?.Workflow);
        Assert.Equal(SsalddelApiGrowthTrack.Community, growthTrack?.Track);

        Assert.Equal(
            "prompt-plan",
            controller.GetMethod(nameof(커뮤니티작성이미지Controller.생성계획수립))
                ?.GetCustomAttribute<HttpPostAttribute>()
                ?.Template);
        Assert.Equal(
            "{jobCode}",
            controller.GetMethod(nameof(커뮤니티작성이미지Controller.조회))
                ?.GetCustomAttribute<HttpGetAttribute>()
                ?.Template);
        Assert.Equal(
            "{jobCode}/post-attachments/{postId:long}",
            controller.GetMethod(nameof(커뮤니티작성이미지Controller.게시글첨부))
                ?.GetCustomAttribute<HttpPostAttribute>()
                ?.Template);
    }

    private static IReadOnlyList<SsalddelCodeMetadataDescriptor> ReadFeatureMetadata()
        => SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.CommunityAuthoringImage,
            typeof(CommunityAuthoringImagePromptPlanRequest).Assembly,
            typeof(CommunityAuthoringImageGeneratorViewModel).Assembly,
            typeof(커뮤니티작성이미지Controller).Assembly);
}
