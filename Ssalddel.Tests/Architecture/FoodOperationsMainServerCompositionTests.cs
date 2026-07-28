namespace Ssalddel.Tests.Architecture;

public sealed class FoodOperationsMainServerCompositionTests
{
    [Fact]
    public void 관리자웹의_음식운영은_메인서버를사용하고_샘플실패대체를두지않는다()
    {
        var program = Read("SsalddelAdmin", "Program.cs");
        var service = Read("SsalddelAdmin", "Services/음식운영Service.cs");
        var developmentSettings = Read("SsalddelAdmin", "appsettings.Development.json");

        Assert.Contains("GetSection(관리자ApiOptions.SectionName)", program);
        Assert.DoesNotContain("FoodApiOptions.SectionName", program);
        Assert.DoesNotContain("7264", program);
        Assert.DoesNotContain("CreateSampleReviewItems", service);
        Assert.DoesNotContain("CanUseMemoryFallback", service);
        Assert.Contains("\"UseMemory\": false", developmentSettings);
        Assert.DoesNotContain("\"FoodApi\"", developmentSettings);
    }

    private static string Read(string project, string relativePath)
        => File.ReadAllText(Path.Combine(FindRepositoryRoot(), project, relativePath));

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }
}
