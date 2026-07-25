using System.Reflection;
using Ssalddel.ApiMetadata;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Controllers.Admin.Content07;
using Ssalddel.Controllers.Common;
using Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;
using Ssalddel.Infrastructure.BackgroundJobs.Community;
using Ssalddel.Services.Community;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Architecture;

public sealed class SsalddelCommunityV0ModuleMetadataTests
{
    [Fact]
    public void Reader_FindsEveryCommunityV0ModuleGroup()
    {
        var modules = ReadModules();
        var moduleKeys = modules
            .Select(module => module.ModuleKey)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(SsalddelCommunityV0ModuleKeys.All.Count, moduleKeys.Count);
        Assert.All(SsalddelCommunityV0ModuleKeys.All, moduleKey => Assert.Contains(moduleKey, moduleKeys));
        Assert.Contains(modules, module => module.ComponentName.EndsWith("CommunityPlatformUiModule", StringComparison.Ordinal));
        Assert.Contains(modules, module => module.ComponentName.EndsWith("CommunityWritingUiModule", StringComparison.Ordinal));
        Assert.Contains(modules, module => module.Component == typeof(PlatformCommunityHomePageViewModel));
        Assert.Contains(modules, module => module.Component == typeof(CommunityPostComposerViewModel));
        Assert.Contains(modules, module => module.Component == typeof(커뮤니티게시글UseCase));
        Assert.Contains(modules, module => module.Component == typeof(커뮤니티투표UseCase));
        Assert.Contains(modules, module => module.Component == typeof(Mongo커뮤니티원장저장소));
        Assert.Contains(modules, module => module.Component == typeof(Mongo커뮤니티원장투영작업저장소));
        Assert.Contains(modules, module => module.Component == typeof(CommunityBoardWritePolicy));
        Assert.Contains(
            modules,
            module => module.Component == typeof(CommunityEditorialBatchRunner));
        Assert.Contains(
            modules,
            module => module.Component == typeof(AgriculturalFisheriesCommunityPipelineRunner));
        Assert.Contains(
            modules,
            module => module.Component == typeof(OfficialFoodIngredientCompanyBatchRunner));
        Assert.Contains(modules, module => module.Component == typeof(커뮤니티정보수집Controller));
        Assert.Contains(modules, module => module.Component == typeof(공식음식조리법ArchiveController));
        Assert.Contains(modules, module => module.Component == typeof(CommunityContentApplicationModule));
        Assert.Contains(modules, module => module.Component == typeof(CommunityParticipationApplicationModule));
        Assert.Contains(modules, module => module.Component == typeof(CommunityLedgerApplicationModule));
    }

    [Fact]
    public void CommunityApplicationModule_DependsOnlyOnSharedContracts()
    {
        var moduleAssembly = typeof(CommunityContentApplicationModule).Assembly;
        var references = moduleAssembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("Ssalddel.Contracts", references);
        Assert.DoesNotContain("Ssalddel", references);
        Assert.DoesNotContain("Ssalddel.Ui.Common", references);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", references);
        Assert.DoesNotContain("MongoDB.Driver", references);
        Assert.Equal(moduleAssembly, typeof(CommunityDriverAvailabilityService).Assembly);
        Assert.Equal(moduleAssembly, typeof(커뮤니티원장Dto).Assembly);
        Assert.Equal(moduleAssembly, typeof(주문원장구성정책).Assembly);
    }

    [Fact]
    public void CommunityV0Modules_HaveStableVersionFlagStageAndBoundary()
    {
        var modules = ReadModules();

        Assert.NotEmpty(modules);
        Assert.All(modules, module =>
        {
            Assert.Equal(SsalddelProductVersionCodes.V0_0, module.ProductVersion);
            Assert.Equal(SsalddelCommunityV0Metadata.FeatureFlag, module.FeatureFlag);
            Assert.Equal(SsalddelCommunityV0Metadata.WorkflowKey, module.WorkflowKey);
            Assert.True(module.DefaultEnabled);
            Assert.Contains(module.ModuleKey, SsalddelCommunityV0ModuleKeys.All);
            Assert.Contains(module.ReleaseStage, SsalddelCommunityV0ReleaseStages.All);
            Assert.False(string.IsNullOrWhiteSpace(module.Responsibility));
            Assert.False(string.IsNullOrWhiteSpace(module.Boundary));
        });
    }

    [Fact]
    public void CommunityV0ApiModules_AlsoUseV0ApiVersionMetadata()
    {
        var apiModules = ReadModules()
            .Where(module => module.Kind == SsalddelModuleKind.Api)
            .ToArray();

        Assert.NotEmpty(apiModules);
        Assert.All(apiModules, module =>
        {
            var controllerType = Assert.IsAssignableFrom<Type>(module.Component);
            var apiVersion = controllerType.GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);
            Assert.NotNull(apiVersion);
            Assert.Equal(SsalddelProductVersion.V0_0, apiVersion.Version);
        });
    }

    private static IReadOnlyList<SsalddelModuleDescriptor> ReadModules()
        => SsalddelModuleMetadataReader.ReadVersion(
            SsalddelProductVersionCodes.V0_0,
            typeof(PlatformCommunityHomePageViewModel).Assembly,
            typeof(커뮤니티게시글UseCase).Assembly,
            typeof(CommunityContentApplicationModule).Assembly);
}
