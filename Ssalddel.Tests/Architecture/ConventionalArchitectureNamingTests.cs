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

        Assert.Contains(
            "[Route(\"api/v1/admin/orderer/group-purchase-demand-os\")]",
            controller);
        Assert.Contains("SectionName = \"GroupPurchaseDemandOS\"", options);
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
