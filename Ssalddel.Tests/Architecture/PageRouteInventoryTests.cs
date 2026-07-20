using System.Text.RegularExpressions;

namespace Ssalddel.Tests.Architecture;

public sealed partial class PageRouteInventoryTests
{
    private static readonly string[] UiProjectDirectories =
    [
        "DriverApp",
        "FDriverApp",
        "HumanResourcesManagerApp",
        "OrdererApp",
        "RestaurantDeskApp",
        "Ssalddel.WebApp",
        "SsalddelAdmin",
        "SsalddelAdminApp",
        "SsalddelApp",
        "WarehouseManagerApp"
    ];

    [Fact]
    public void 전체_UI_라우트는_앱과_라우트_조합으로_유일하게_식별된다()
    {
        var routes = ReadRoutes();

        Assert.True(routes.Select(item => item.SourcePath).Distinct(StringComparer.OrdinalIgnoreCase).Count() >= 233);
        Assert.True(routes.Count >= 260);
        Assert.All(UiProjectDirectories, appCode =>
            Assert.Contains(routes, route => route.AppCode == appCode));
        Assert.All(routes, route => Assert.StartsWith("/", route.RouteTemplate));

        var duplicateOwners = routes
            .GroupBy(
                route => $"{route.AppCode}|{route.RouteTemplate}",
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        Assert.Empty(duplicateOwners);
    }

    private static IReadOnlyList<PageRouteInventoryItem> ReadRoutes()
    {
        var repositoryRoot = FindRepositoryRoot();
        var routes = new List<PageRouteInventoryItem>();

        foreach (var appCode in UiProjectDirectories)
        {
            var projectDirectory = Path.Combine(repositoryRoot, appCode);
            foreach (var sourcePath in Directory.EnumerateFiles(
                         projectDirectory,
                         "*.razor",
                         SearchOption.AllDirectories))
            {
                if (IsGeneratedPath(sourcePath))
                {
                    continue;
                }

                var source = File.ReadAllText(sourcePath);
                foreach (Match match in PageDirectiveRegex().Matches(source))
                {
                    routes.Add(new PageRouteInventoryItem(
                        appCode,
                        match.Groups["route"].Value,
                        Path.GetRelativePath(repositoryRoot, sourcePath)));
                }
            }
        }

        return routes;
    }

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

    private static bool IsGeneratedPath(string path)
        => path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
           || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex("^\\s*@page\\s+\"(?<route>[^\"]+)\"", RegexOptions.Multiline)]
    private static partial Regex PageDirectiveRegex();

    private sealed record PageRouteInventoryItem(
        string AppCode,
        string RouteTemplate,
        string SourcePath);
}
