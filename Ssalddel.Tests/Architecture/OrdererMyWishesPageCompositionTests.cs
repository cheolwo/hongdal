namespace Ssalddel.Tests.Architecture;

public sealed class OrdererMyWishesPageCompositionTests
{
    [Fact]
    public void 내공동진행은_본인원함집단만사용하고_두목표중낮은진행률을표시한다()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "OrdererApp",
            "Components",
            "GroupPurchase",
            "GroupPurchaseMyGroupsScreen.razor"));

        Assert.Contains("@inject GroupPurchaseMyWishesViewModel ViewModel", source);
        Assert.Contains("@foreach (var group in ViewModel.Groups)", source);
        Assert.DoesNotContain("I공동구매실행Service", source, StringComparison.Ordinal);
        Assert.DoesNotContain("자동집단목록조회Async", source, StringComparison.Ordinal);

        Assert.Contains("group.목표참여자수 is > 0", source);
        Assert.Contains("100d * group.참여자수 / group.목표참여자수.Value", source);
        Assert.Contains("group.목표수량 is > 0", source);
        Assert.Contains("group.총희망수량 / group.목표수량.Value", source);
        Assert.Contains("Math.Clamp(progress.Min(), 0d, 100d)", source);
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
}
