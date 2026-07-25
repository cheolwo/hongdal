using System.Globalization;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class ProduceRegionalPriceComparisonViewModelTests
{
    [Theory]
    [InlineData("ko-KR", 농산물가격표시통화.KRW)]
    [InlineData("zh-CN", 농산물가격표시통화.CNY)]
    [InlineData("zh-TW", 농산물가격표시통화.CNY)]
    [InlineData("en-US", 농산물가격표시통화.USD)]
    [InlineData("fr-FR", 농산물가격표시통화.USD)]
    public void Constructor_SelectsViewerCurrencyFromUiCulture(
        string cultureName,
        농산물가격표시통화 expected)
    {
        using var viewModel = new 농산물지역가격비교ViewModel(
            CultureInfo.GetCultureInfo(cultureName));

        Assert.Equal(expected, viewModel.DisplayCurrency);
    }

    [Fact]
    public void Catalog_SeparatesFruitAndVegetableProducts()
    {
        using var viewModel = new 농산물지역가격비교ViewModel();

        Assert.Equal(["사과", "배"], viewModel.AvailableProducts.Select(product => product.Name));

        viewModel.SelectedCategory = 농산물가격분류.채소;

        Assert.Equal(["토마토", "양파"], viewModel.AvailableProducts.Select(product => product.Name));
        Assert.Equal("tomato", viewModel.SelectedProductKey);
        Assert.Equal(200m, viewModel.ComparisonWeightGrams);
    }

    [Fact]
    public void Apple_KeepsCountryAndRegionalObservations()
    {
        using var viewModel = new 농산물지역가격비교ViewModel(
            CultureInfo.GetCultureInfo("ko-KR"));

        Assert.Equal(4, viewModel.Items.Count);
        Assert.Equal(3, viewModel.AvailableCountries.Count - 1);
        Assert.Contains(viewModel.Items, item => item.Observation.RegionCode == "US-SC");
        Assert.Contains(viewModel.Items, item => item.Observation.RegionCode == "US-SW");
        Assert.Contains(viewModel.Items, item => item.Observation.CountryCode == "CN");
    }

    [Fact]
    public void ComparisonTable_IsDefaultAndShowsDifferenceFromLowestObservation()
    {
        using var viewModel = new 농산물지역가격비교ViewModel(
            CultureInfo.GetCultureInfo("ko-KR"));

        Assert.Equal(농산물가격보기방식.비교표, viewModel.ViewMode);
        Assert.Equal(0m, viewModel.Items[0].DisplayDifferenceFromLowest);
        Assert.Equal(0m, viewModel.Items[0].DifferencePercentFromLowest);
        Assert.All(
            viewModel.Items.Skip(1),
            item =>
            {
                Assert.True(item.DisplayDifferenceFromLowest > 0m);
                Assert.True(item.DifferencePercentFromLowest > 0m);
            });
    }

    [Fact]
    public void ViewMode_CanSwitchToHorizontalCards()
    {
        using var viewModel = new 농산물지역가격비교ViewModel();

        viewModel.ViewMode = 농산물가격보기방식.카드;

        Assert.Equal(농산물가격보기방식.카드, viewModel.ViewMode);
    }

    [Fact]
    public void CountryAndRegionFilters_NarrowSelectedProduct()
    {
        using var viewModel = new 농산물지역가격비교ViewModel();

        viewModel.SelectedCategory = 농산물가격분류.채소;
        viewModel.SelectedProductKey = "tomato";
        viewModel.SelectedCountryCode = "US";
        viewModel.SelectedRegionCode = "US-SW";

        var item = Assert.Single(viewModel.Items);
        Assert.Equal("Southwest U.S.", item.Observation.RegionName);
        Assert.Equal(0.99m, item.Observation.NativePrice);
    }

    [Fact]
    public void ChangingProduct_ResetsLocationFiltersAndUsesDefaultWeight()
    {
        using var viewModel = new 농산물지역가격비교ViewModel();
        viewModel.SelectedCountryCode = "US";
        viewModel.SelectedRegionCode = "US-SC";

        viewModel.SelectedProductKey = "pear";

        Assert.Equal(농산물지역가격비교ViewModel.AllFilter, viewModel.SelectedCountryCode);
        Assert.Equal(농산물지역가격비교ViewModel.AllFilter, viewModel.SelectedRegionCode);
        Assert.Equal(220m, viewModel.ComparisonWeightGrams);
        Assert.Equal(5, viewModel.Items.Count);
    }

    [Fact]
    public void DisplayCurrency_RecalculatesAllRowsInViewerCurrency()
    {
        using var viewModel = new 농산물지역가격비교ViewModel(
            CultureInfo.GetCultureInfo("ko-KR"));

        viewModel.DisplayCurrency = 농산물가격표시통화.CNY;

        Assert.All(viewModel.Items, item => Assert.True(item.DisplayWeightPrice > 0m));
        Assert.Equal("위안(CNY)", viewModel.DisplayCurrencyLabel);
        Assert.StartsWith("¥", viewModel.FormatDisplayPrice(viewModel.Items[0].DisplayWeightPrice));
    }

    [Theory]
    [InlineData(50, 100)]
    [InlineData(250, 250)]
    [InlineData(1200, 1000)]
    public void ComparisonWeight_StaysInsideComparableRange(decimal requested, decimal expected)
    {
        using var viewModel = new 농산물지역가격비교ViewModel();

        viewModel.ComparisonWeightGrams = requested;

        Assert.Equal(expected, viewModel.ComparisonWeightGrams);
    }
}
