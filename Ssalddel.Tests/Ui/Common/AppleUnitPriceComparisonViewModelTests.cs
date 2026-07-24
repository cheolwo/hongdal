using System.Globalization;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class AppleUnitPriceComparisonViewModelTests
{
    [Theory]
    [InlineData("ko-KR", 사과가격표시통화.KRW)]
    [InlineData("zh-CN", 사과가격표시통화.CNY)]
    [InlineData("zh-TW", 사과가격표시통화.CNY)]
    [InlineData("en-US", 사과가격표시통화.USD)]
    [InlineData("fr-FR", 사과가격표시통화.USD)]
    public void Constructor_SelectsViewerCurrencyFromUiCulture(
        string cultureName,
        사과가격표시통화 expected)
    {
        using var viewModel = new 사과한개가격비교ViewModel(
            CultureInfo.GetCultureInfo(cultureName));

        Assert.Equal(expected, viewModel.DisplayCurrency);
    }

    [Fact]
    public void Items_UseOneSharedWeightAndPreserveSourceMeaning()
    {
        using var viewModel = new 사과한개가격비교ViewModel(
            CultureInfo.GetCultureInfo("ko-KR"))
        {
            AppleWeightGrams = 250m
        };

        var items = viewModel.Items;

        Assert.Equal(3, items.Count);
        Assert.Equal(2_580.8m, items.Single(item => item.Observation.CountryCode == "KR").NativeApplePrice);
        Assert.Equal(1.5m, items.Single(item => item.Observation.CountryCode == "CN").NativeApplePrice);
        Assert.Equal("광고 소매가", items.Single(item => item.Observation.CountryCode == "US").Observation.MarketStage);
        Assert.Equal("산지·도매 관측값", items.Single(item => item.Observation.CountryCode == "CN").Observation.MarketStage);
    }

    [Fact]
    public void DisplayCurrency_RecalculatesAllRowsInViewerCurrency()
    {
        using var viewModel = new 사과한개가격비교ViewModel(
            CultureInfo.GetCultureInfo("ko-KR"));

        viewModel.DisplayCurrency = 사과가격표시통화.CNY;

        Assert.All(viewModel.Items, item => Assert.True(item.DisplayApplePrice > 0m));
        Assert.Equal("위안(CNY)", viewModel.DisplayCurrencyLabel);
        Assert.StartsWith("¥", viewModel.FormatDisplayPrice(viewModel.Items[0].DisplayApplePrice));
    }

    [Theory]
    [InlineData(50, 100)]
    [InlineData(250, 250)]
    [InlineData(800, 500)]
    public void AppleWeight_StaysInsideComparableRange(decimal requested, decimal expected)
    {
        using var viewModel = new 사과한개가격비교ViewModel();

        viewModel.AppleWeightGrams = requested;

        Assert.Equal(expected, viewModel.AppleWeightGrams);
    }
}
