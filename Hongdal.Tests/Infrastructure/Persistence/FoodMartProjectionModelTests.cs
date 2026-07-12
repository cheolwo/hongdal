using Hongdal.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using 홍달.Data;
using 홍달.Infrastructure.Security;
using 홍달.도메인.마트;
using 홍달.도메인.음식;
using 홍달.도메인.창고;

namespace Hongdal.Tests.Infrastructure.Persistence;

public sealed class FoodMartProjectionModelTests
{
    [Fact]
    public void Model_contains_food_order_and_mart_order_projections()
    {
        using var context = CreateContext();

        var foodOrder = context.Model.FindEntityType(typeof(음식주문));
        var martOrder = context.Model.FindEntityType(typeof(마트주문));
        var martOrderItem = context.Model.FindEntityType(typeof(마트주문상품));
        var pickingPackingTask = context.Model.FindEntityType(typeof(피킹포장작업));
        var ledgerStateEvent = context.Model.FindEntityType(typeof(커뮤니티원장상태이벤트));

        Assert.NotNull(foodOrder);
        Assert.NotNull(martOrder);
        Assert.NotNull(martOrderItem);
        Assert.NotNull(pickingPackingTask);
        Assert.NotNull(ledgerStateEvent);
        Assert.Equal("음식주문", foodOrder!.GetTableName());
        Assert.Equal("마트주문", martOrder!.GetTableName());
        Assert.Equal("피킹포장작업", pickingPackingTask!.GetTableName());
        Assert.Equal("community_ledger_state_events", ledgerStateEvent!.GetTableName());
        Assert.Contains(foodOrder.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(["주문번호"]));
        Assert.Contains(martOrder.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(["주문참조번호"]));
        Assert.Contains(martOrderItem!.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(["마트주문Id", "출고예정Id"]));
        Assert.Contains(pickingPackingTask.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(["작업Key"]));
        Assert.Contains(ledgerStateEvent.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(["EventId"]));
    }

    private static HongdalContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HongdalContext>()
            .UseMySql(
                "Server=localhost;Database=hongdal_model_test;User=root;Password=test;",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;

        return new HongdalContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
