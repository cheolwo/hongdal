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
        Assert.Contains(metadata, item => item.ComponentType == typeof(CommunityAuthoringImagesController));
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
    public void CommunityAuthoringImagesController_PreservesRoutesAndCommunityMetadata()
    {
        var controller = typeof(CommunityAuthoringImagesController);
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
            controller.GetMethod(nameof(CommunityAuthoringImagesController.Plan))
                ?.GetCustomAttribute<HttpPostAttribute>()
                ?.Template);
        Assert.Equal(
            "{jobCode}",
            controller.GetMethod(nameof(CommunityAuthoringImagesController.Get))
                ?.GetCustomAttribute<HttpGetAttribute>()
                ?.Template);
        Assert.Equal(
            "{jobCode}/post-attachments/{postId:long}",
            controller.GetMethod(nameof(CommunityAuthoringImagesController.Attach))
                ?.GetCustomAttribute<HttpPostAttribute>()
                ?.Template);
    }

    private static IReadOnlyList<SsalddelCodeMetadataDescriptor> ReadFeatureMetadata()
        => SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.CommunityAuthoringImage,
            typeof(CommunityAuthoringImagePromptPlanRequest).Assembly,
            typeof(CommunityAuthoringImageGeneratorViewModel).Assembly,
            typeof(CommunityAuthoringImagesController).Assembly);
}
