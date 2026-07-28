namespace Ssalddel.Tests.Architecture;

public sealed class LegacyFoodApiRuntimeConvergenceTests
{
    [Fact]
    public void 배포대상은_별도FoodApi를포함하지않고_주변음식점을메인Db에서조회한다()
    {
        var releaseSolution = ReadRoot("Ssalddel.v3.5.slnx");
        var registration = Read(
            "Ssalddel",
            "Extensions/ServiceCollectionExtensions.HttpClients.cs");
        var directory = Read(
            "Ssalddel",
            "Services/Community/CommunityNearbyRestaurantDirectory.cs");
        var options = Read(
            "Ssalddel",
            "Services/Options/CommunityContextDiscoveryOptions.cs");

        Assert.DoesNotContain("Ssalddel.FoodApi", releaseSolution);
        Assert.Contains(
            "AddScoped<ICommunityNearbyRestaurantDirectory, MainServerCommunityNearbyRestaurantDirectory>()",
            registration);
        Assert.Contains("db.음식점공개프로필", directory);
        Assert.Contains("db.음식점리뷰", directory);
        Assert.DoesNotContain("HttpCommunityNearbyRestaurantDirectory", registration);
        Assert.DoesNotContain("FoodApiBaseUrl", options);
    }

    [Fact]
    public void 음식기능카탈로그는_실제메인서버경로만노출한다()
    {
        var catalog = Read(
            "Ssalddel.Ui.Common",
            "Areas/App/ViewModels/Controller기능카탈로그.cs");

        Assert.Contains("api/v1/food-orders", catalog);
        Assert.Contains("api/v1/food-orders/dispatch/address-form", catalog);
        Assert.Contains("api/v1/orderer/restaurants", catalog);
        Assert.DoesNotContain("api/v1/food-delivery-tickets", catalog);
        Assert.DoesNotContain("api/v1/food-delivery-settlements", catalog);
        Assert.DoesNotContain("api/v1/food-delivery-pricing", catalog);
        Assert.DoesNotContain("\"api/v1/restaurants\"", catalog);
    }

    private static string Read(string project, string relativePath)
        => File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            project,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string ReadRoot(string relativePath)
        => File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));

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
