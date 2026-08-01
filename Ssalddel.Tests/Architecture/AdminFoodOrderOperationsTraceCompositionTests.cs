namespace Ssalddel.Tests.Architecture;

public sealed class AdminFoodOrderOperationsTraceCompositionTests
{
    [Fact]
    public void 관리자화면은_주문번호상관관계와복구정보를표시하고민감자료를제외한다()
    {
        var root = FindRepositoryRoot();
        var page = File.ReadAllText(Path.Combine(
            root,
            "SsalddelAdmin",
            "Components",
            "Pages",
            "FoodOrderOperationsTrace.razor"));

        Assert.Contains("@page \"/food/order-trace\"", page);
        Assert.Contains("업무 체크포인트", page);
        Assert.Contains("Outbox 상태", page);
        Assert.Contains("복구 안내", page);
        Assert.Contains("수령지 주소·연락처·Outbox payload는 이 화면에 표시하지 않습니다.", page);
        Assert.DoesNotContain("DataJson", page);
        Assert.DoesNotContain("PayloadJson", page);
    }

    [Fact]
    public void 관리자V1탐색에_음식주문추적화면을포함한다()
    {
        var root = FindRepositoryRoot();
        var navigation = File.ReadAllText(Path.Combine(
            root,
            "SsalddelAdmin",
            "Services",
            "AdminV1NavigationPolicy.cs"));

        Assert.Contains("\"/admin/food-delivery/order-trace\"", navigation);
    }

    [Fact]
    public void 음식배달워크플로우에_관리자운영추적화면을등록한다()
    {
        var root = FindRepositoryRoot();
        var metadata = File.ReadAllText(Path.Combine(
            root,
            "Ssalddel",
            "ApiMetadata",
            "SsalddelApiVersionAttribute.cs"));

        Assert.Contains("\"음식 주문 운영 추적\"", metadata);
        Assert.Contains("\"/food/order-trace\"", metadata);
        Assert.Contains("\"운영자가 음식 주문번호로 배차·추천·운송·Outbox 상관관계", metadata);
    }

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "Ssalddel.slnx")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new DirectoryNotFoundException("저장소 루트를 찾을 수 없습니다.");
    }
}
