using Ssalddel.Contracts.Common.Content;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class OfficialFoodIngredientJourneyViewModel(
    IOfficialFoodIngredientDiscoveryClient client) : PageViewModelBase
{
    private string _searchText = string.Empty;
    private IReadOnlyList<OfficialFoodIngredientDto> _ingredients = [];

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

    public Task<bool> SearchAsync(CancellationToken cancellationToken = default)
        => 새로고침Async(cancellationToken);

    public async Task<bool> ClearSearchAsync(CancellationToken cancellationToken = default)
    {
        SearchText = string.Empty;
        return await 새로고침Async(cancellationToken);
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        Ingredients = await client.SearchAsync(
            new OfficialFoodIngredientQuery
            {
                SearchText = SearchText,
                Take = 12
            },
            cancellationToken);
    }
}
