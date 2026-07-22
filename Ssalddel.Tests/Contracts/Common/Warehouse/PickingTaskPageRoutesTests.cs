using Ssalddel.Contracts.Common.Warehouse;

namespace Ssalddel.Tests.Contracts.Common.Warehouse;

public sealed class PickingTaskPageRoutesTests
{
    [Fact]
    public void StableKeyRoutes_RequireBoundedTaskKeyAndEscapePathSegment()
    {
        Assert.Equal("/work/picking-batch/PICK%2071%2FBLUE", PickingTaskPageRoutes.DetailFor(" PICK 71/BLUE "));
        Assert.Equal("/work/picking-batch/PICK-71/execute", PickingTaskPageRoutes.ExecuteFor("PICK-71"));
        Assert.Throws<ArgumentException>(() => PickingTaskPageRoutes.DetailFor(" "));
        Assert.Throws<ArgumentException>(() => PickingTaskPageRoutes.ExecuteFor(new string('A', 121)));
    }

    [Fact]
    public void Context_RoundTripsListStateAcrossDetailAndExecute()
    {
        var context = new PickingTaskNavigationContext
        {
            From = "/diagram?node=picking-71",
            Search = "감자 SKU",
            Status = 피킹작업조회상태코드.진행중,
            Page = 2
        };

        var detail = context.PathFor(PickingTaskScreenKind.Detail, "PICK-71");
        var parsed = PickingTaskNavigationContext.Parse(detail);

        Assert.StartsWith("/work/picking-batch/PICK-71?", detail, StringComparison.Ordinal);
        Assert.Equal(context.From, parsed.From);
        Assert.Equal(context.Search, parsed.Search);
        Assert.Equal(context.Status, parsed.Status);
        Assert.Equal(2, parsed.Page);
        Assert.StartsWith("/work/picking-batch/PICK-71/execute?", parsed.PathFor(PickingTaskScreenKind.Execute, "PICK-71"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("https://evil.example/path")]
    [InlineData("//evil.example/path")]
    [InlineData("/safe\\redirect")]
    [InlineData("/%2f%2fevil.example")]
    public void Context_DropsUnsafeReturnPath(string from)
    {
        var parsed = PickingTaskNavigationContext.Parse(
            $"/work/picking-batch?from={Uri.EscapeDataString(from)}");

        Assert.Null(parsed.From);
        Assert.Equal("/work-board", parsed.ResolveReturnPath("/work-board"));
    }

    [Fact]
    public void Context_NormalizesInvalidFilterValues()
    {
        var parsed = PickingTaskNavigationContext.Parse(
            "/work/picking-batch?status=unknown&page=-3&search=%E0%A4%A");

        Assert.Equal(피킹작업조회상태코드.대기, parsed.Status);
        Assert.Equal(0, parsed.Page);
        Assert.NotNull(parsed.Search);
        Assert.Equal(PickingTaskPageRoutes.Root, new PickingTaskNavigationContext().PathFor(PickingTaskScreenKind.List));
    }
}
