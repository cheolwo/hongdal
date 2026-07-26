namespace Ssalddel.Tests.Architecture;

public sealed class ConventionalArchitectureNamingTests
{
    [Fact]
    public void 공동구매수요모집_기능키도_ProcessManager를_사용한다()
        => Assert.Equal(
            "group-purchase-demand-process-manager",
            Ssalddel.Contracts.Common.Metadata.SsalddelCodeFeatureKeys
                .GroupPurchaseDemandProcessManager);

    [Fact]
    public void 공동구매수요모집은_ProcessManager와_ProcessStore로_표현한다()
    {
        var root = FindRepositoryRoot();
        var processManagerPath = Path.Combine(
            root,
            "Ssalddel",
            "Services",
            "Orderer",
            "공동구매수요모집ProcessManager.cs");
        var source = File.ReadAllText(processManagerPath);

        Assert.Contains("interface I공동구매수요모집ProcessManager", source);
        Assert.Contains("class 공동구매수요모집ProcessManager", source);
        Assert.Contains("interface I공동구매수요모집ProcessStore", source);
        Assert.Contains("class 공동구매수요모집DeadlineScanBackgroundService", source);
        Assert.DoesNotContain("interface I공동구매수요모집OS", source);
        Assert.DoesNotContain("class 공동구매수요모집OS", source);
        Assert.DoesNotContain("공동구매수요모집OsWorker", source);
    }

    [Fact]
    public void 공동수입준비도_ProcessManager와_BackgroundService로_표현한다()
    {
        var root = FindRepositoryRoot();
        var sourcePath = Path.Combine(
            root,
            "Ssalddel",
            "Services",
            "Orderer",
            "공동수입준비ProcessManager.cs");
        var source = File.ReadAllText(sourcePath);

        Assert.Contains("interface I공동수입준비ProcessManager", source);
        Assert.Contains("class 공동수입준비ProcessManager", source);
        Assert.Contains("class 공동수입준비정기점검BackgroundService", source);
        Assert.DoesNotContain("interface I공동수입준비OS", source);
        Assert.DoesNotContain("class 공동수입준비OS", source);
        Assert.DoesNotContain("class 공동수입준비OsWorker", source);
    }

    [Fact]
    public void 기존_API와_설정키는_호환계약으로_유지한다()
    {
        var root = FindRepositoryRoot();
        var controller = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel",
            "Controllers",
            "Admin",
            "Orderer",
            "공동구매수요모집ProcessManagerAdminController.cs"));
        var options = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel",
            "Services",
            "Options",
            "GroupPurchaseDemandProcessManagerOptions.cs"));
        var importController = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel",
            "Controllers",
            "Admin",
            "Orderer",
            "공동수입준비원장AdminController.cs"));
        var importOptions = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel",
            "Services",
            "Options",
            "GroupImportReadinessOsOptions.cs"));

        Assert.Contains(
            "[Route(\"api/v1/admin/orderer/group-purchase-demand-os\")]",
            controller);
        Assert.Contains("SectionName = \"GroupPurchaseDemandOS\"", options);
        Assert.Contains(
            "[Route(\"api/v1/admin/orderer/group-purchase-demand-os/groups/{autoGroupId}/trade-readiness\")]",
            importController);
        Assert.Contains("SectionName = \"GroupImportReadinessOS\"", importOptions);
    }

    [Fact]
    public void 업무실행책임모델은_HIOPS와_OS를_호환용어로만_유지한다()
    {
        var root = FindRepositoryRoot();
        var currentModel = File.ReadAllText(Path.Combine(
            root,
            "docs",
            "Architecture",
            "BusinessWorkflowResponsibilityModel.md"));
        var compatibilityDocument = File.ReadAllText(Path.Combine(
            root,
            "docs",
            "Architecture",
            "HIOPSLayerModel.md"));
        var rootInstructions = File.ReadAllText(Path.Combine(root, "AGENTS.md"));

        Assert.Contains(
            "새 코드에는 HIOPS나 OS를 기술 역할명으로 추가하지 않는다.",
            currentModel);
        Assert.Contains("ProcessManager", currentModel);
        Assert.Contains("WorkflowCoordinator", currentModel);
        Assert.Contains(
            "현재 기준은 [업무 실행 책임 모델]",
            compatibilityDocument);
        Assert.Contains(
            "docs/Architecture/BusinessWorkflowResponsibilityModel.md",
            rootInstructions);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Ssalddel repository root를 찾지 못했습니다.");
    }
}
