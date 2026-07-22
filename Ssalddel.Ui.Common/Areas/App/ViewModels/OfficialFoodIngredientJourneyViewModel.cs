using Ssalddel.Contracts.Common.Content;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class OfficialFoodIngredientJourneyViewModel(
    IOfficialFoodIngredientDiscoveryClient client) : PageViewModelBase
{
    private string _searchText = string.Empty;
    private string _dishSearchText = string.Empty;
    private string _selectedCountryCode = string.Empty;
    private IReadOnlyList<OfficialFoodIngredientDto> _ingredients = [];
    private IReadOnlyList<OfficialFoodRecipeDishDto> _dishes = [];
    private OfficialFoodDishDetailDto? _selectedDish;
    private string _selectedDishIngredientKey = string.Empty;
    private bool _dishDetailBusy;
    private string? _dishErrorMessage;
    private OfficialFoodIngredientCompanyResearchResponse? _companyResearch;
    private bool _companyResearchBusy;
    private string? _companyResearchErrorMessage;
    private OfficialFoodIngredientHsMappingResponse? _hsMapping;
    private bool _hsMappingBusy;
    private string? _hsMappingErrorMessage;

    public static IReadOnlyList<OfficialFoodCountryOption> CountryOptions { get; } =
    [
        new(string.Empty, "전체"),
        new("KR", "한국"),
        new("JP", "일본"),
        new("GB", "영국"),
        new("US", "미국"),
        new("CA", "캐나다"),
        new("FR", "프랑스")
    ];

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public IReadOnlyList<OfficialFoodIngredientDto> Ingredients
    {
        get => _ingredients;
        private set
        {
            if (SetProperty(ref _ingredients, value))
            {
                OnPropertyChanged(nameof(HasIngredients));
            }
        }
    }

    public bool HasIngredients => Ingredients.Count > 0;

    public string DishSearchText
    {
        get => _dishSearchText;
        set => SetProperty(ref _dishSearchText, value);
    }

    public string SelectedCountryCode
    {
        get => _selectedCountryCode;
        private set => SetProperty(ref _selectedCountryCode, value);
    }

    public IReadOnlyList<OfficialFoodRecipeDishDto> Dishes
    {
        get => _dishes;
        private set
        {
            if (SetProperty(ref _dishes, value))
            {
                OnPropertyChanged(nameof(HasDishes));
            }
        }
    }

    public OfficialFoodDishDetailDto? SelectedDish
    {
        get => _selectedDish;
        private set
        {
            if (SetProperty(ref _selectedDish, value))
            {
                ClearCompanyResearch();
                ClearHsMapping();
                OnPropertyChanged(nameof(SelectedDishIngredient));
            }
        }
    }

    public string SelectedDishIngredientKey
    {
        get => _selectedDishIngredientKey;
        private set
        {
            if (SetProperty(ref _selectedDishIngredientKey, value))
            {
                ClearCompanyResearch();
                ClearHsMapping();
                OnPropertyChanged(nameof(SelectedDishIngredient));
            }
        }
    }

    public OfficialFoodRecipeIngredientDto? SelectedDishIngredient
        => SelectedDish?.Ingredients.FirstOrDefault(ingredient =>
            string.Equals(
                ingredient.IngredientKey,
                SelectedDishIngredientKey,
                StringComparison.Ordinal));

    public bool HasDishes => Dishes.Count > 0;

    public bool DishDetailBusy
    {
        get => _dishDetailBusy;
        private set => SetProperty(ref _dishDetailBusy, value);
    }

    public string? DishErrorMessage
    {
        get => _dishErrorMessage;
        private set => SetProperty(ref _dishErrorMessage, value);
    }

    public OfficialFoodIngredientCompanyResearchResponse? CompanyResearch
    {
        get => _companyResearch;
        private set => SetProperty(ref _companyResearch, value);
    }

    public bool CompanyResearchBusy
    {
        get => _companyResearchBusy;
        private set => SetProperty(ref _companyResearchBusy, value);
    }

    public string? CompanyResearchErrorMessage
    {
        get => _companyResearchErrorMessage;
        private set => SetProperty(ref _companyResearchErrorMessage, value);
    }

    public OfficialFoodIngredientHsMappingResponse? HsMapping
    {
        get => _hsMapping;
        private set => SetProperty(ref _hsMapping, value);
    }

    public bool HsMappingBusy
    {
        get => _hsMappingBusy;
        private set => SetProperty(ref _hsMappingBusy, value);
    }

    public string? HsMappingErrorMessage
    {
        get => _hsMappingErrorMessage;
        private set => SetProperty(ref _hsMappingErrorMessage, value);
    }

    public Task<bool> SearchAsync(CancellationToken cancellationToken = default)
        => 새로고침Async(cancellationToken);

    public async Task<bool> ClearSearchAsync(CancellationToken cancellationToken = default)
    {
        SearchText = string.Empty;
        return await 새로고침Async(cancellationToken);
    }

    public async Task<bool> SelectCountryAsync(
        string? countryCode,
        CancellationToken cancellationToken = default)
    {
        SelectedCountryCode = NormalizeCountryCode(countryCode);
        return await SearchDishesAsync(cancellationToken);
    }

    public async Task<bool> SearchDishesAsync(CancellationToken cancellationToken = default)
    {
        if (DishDetailBusy)
        {
            return false;
        }

        DishDetailBusy = true;
        DishErrorMessage = null;
        try
        {
            var previousDishKey = SelectedDish?.Dish.DishKey;
            Dishes = await client.SearchDishesAsync(CreateDishQuery(), cancellationToken);
            await SelectDishCoreAsync(PreferredDishKey(previousDishKey), cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            Dishes = [];
            SelectedDish = null;
            SelectedDishIngredientKey = string.Empty;
            DishErrorMessage = exception.Message;
            return false;
        }
        finally
        {
            DishDetailBusy = false;
        }
    }

    public async Task<bool> SelectDishAsync(
        string? dishKey,
        CancellationToken cancellationToken = default)
    {
        if (DishDetailBusy)
        {
            return false;
        }

        DishDetailBusy = true;
        DishErrorMessage = null;
        try
        {
            return await SelectDishCoreAsync(dishKey, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            DishErrorMessage = exception.Message;
            return false;
        }
        finally
        {
            DishDetailBusy = false;
        }
    }

    public void SelectDishIngredient(string? ingredientKey)
    {
        if (SelectedDish?.Ingredients.Any(ingredient =>
                string.Equals(ingredient.IngredientKey, ingredientKey, StringComparison.Ordinal)) == true)
        {
            SelectedDishIngredientKey = ingredientKey!;
        }
    }

    public async Task<bool> ResearchSelectedIngredientCompaniesAsync(
        CancellationToken cancellationToken = default)
    {
        var ingredient = SelectedDishIngredient;
        if (ingredient is null || CompanyResearchBusy)
        {
            return false;
        }

        var ingredientKey = ingredient.IngredientKey;
        CompanyResearchBusy = true;
        CompanyResearchErrorMessage = null;
        try
        {
            var result = await client.SearchCompaniesAsync(
                new OfficialFoodIngredientCompanyQuery
                {
                    IngredientKey = ingredient.IngredientKey,
                    IngredientName = ingredient.CanonicalName,
                    Take = 16
                },
                cancellationToken);
            if (!string.Equals(
                    SelectedDishIngredientKey,
                    ingredientKey,
                    StringComparison.Ordinal))
            {
                return false;
            }

            CompanyResearch = result;
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            CompanyResearch = null;
            CompanyResearchErrorMessage = exception.Message;
            return false;
        }
        finally
        {
            CompanyResearchBusy = false;
        }
    }

    public async Task<bool> LoadSelectedIngredientHsCodesAsync(
        CancellationToken cancellationToken = default)
    {
        var ingredient = SelectedDishIngredient;
        if (ingredient is null || HsMappingBusy)
        {
            return false;
        }

        var ingredientKey = ingredient.IngredientKey;
        HsMappingBusy = true;
        HsMappingErrorMessage = null;
        try
        {
            var result = await client.GetHsCodesAsync(
                new OfficialFoodIngredientHsQuery
                {
                    IngredientKey = ingredient.IngredientKey,
                    IngredientName = ingredient.CanonicalName
                },
                cancellationToken);
            if (!string.Equals(
                    SelectedDishIngredientKey,
                    ingredientKey,
                    StringComparison.Ordinal))
            {
                return false;
            }

            HsMapping = result;
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            HsMapping = null;
            HsMappingErrorMessage = exception.Message;
            return false;
        }
        finally
        {
            HsMappingBusy = false;
        }
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        var ingredientTask = client.SearchAsync(
            new OfficialFoodIngredientQuery
            {
                SearchText = SearchText,
                Take = 12
            },
            cancellationToken);
        var dishTask = client.SearchDishesAsync(CreateDishQuery(), cancellationToken);
        await Task.WhenAll(ingredientTask, dishTask);

        var previousDishKey = SelectedDish?.Dish.DishKey;
        Ingredients = await ingredientTask;
        Dishes = await dishTask;
        await SelectDishCoreAsync(PreferredDishKey(previousDishKey), cancellationToken);
    }

    private OfficialFoodDishDiscoveryQuery CreateDishQuery()
        => new()
        {
            CountryCode = SelectedCountryCode,
            SearchText = DishSearchText,
            Take = 24
        };

    private async Task<bool> SelectDishCoreAsync(
        string? dishKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dishKey))
        {
            SelectedDish = null;
            SelectedDishIngredientKey = string.Empty;
            return false;
        }

        var detail = await client.GetDishAsync(dishKey, cancellationToken);
        SelectedDish = detail;
        SelectedDishIngredientKey = detail?.Ingredients.FirstOrDefault()?.IngredientKey
            ?? string.Empty;
        return detail is not null;
    }

    private string? PreferredDishKey(string? previousDishKey)
        => Dishes.Any(dish => string.Equals(dish.DishKey, previousDishKey, StringComparison.Ordinal))
            ? previousDishKey
            : Dishes.FirstOrDefault()?.DishKey;

    private static string NormalizeCountryCode(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
        return CountryOptions.Any(option => option.CountryCode == normalized)
            ? normalized
            : string.Empty;
    }

    private void ClearCompanyResearch()
    {
        CompanyResearch = null;
        CompanyResearchErrorMessage = null;
    }

    private void ClearHsMapping()
    {
        HsMapping = null;
        HsMappingErrorMessage = null;
    }
}

public sealed record OfficialFoodCountryOption(string CountryCode, string DisplayName);
