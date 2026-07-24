using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class UnitedStatesKoreanFoodGroupBuyStorefrontViewModel(
    IOfficialFoodIngredientDiscoveryClient foodClient,
    I공동구매실행Service groupPurchaseService,
    ISsalddel현재사용자Context currentUserContext) : PageViewModelBase
{
    private const string KoreaCountryCode = "KR";
    private const string LogisticsMode = 공동구매자동수요물류방식코드.후속검토;
    private const int TargetParticipantCount = 5;
    private const decimal TargetQuantity = 30m;
    private string _dishSearchText = string.Empty;
    private IReadOnlyList<OfficialFoodRecipeDishDto> _dishes = [];
    private OfficialFoodDishDetailDto? _selectedDish;
    private string _selectedIngredientKey = string.Empty;
    private OfficialFoodIngredientHsMappingResponse? _hsMapping;
    private long? _selectedHsMappingId;
    private string _usZipCode = string.Empty;
    private decimal _desiredQuantity = 1m;
    private string _temperatureCode = "상온";
    private bool _actionBusy;
    private string? _actionError;
    private string? _actionNotice;
    private 공동구매자동집단배치미리보기응답? _placementPreview;
    private 공동구매자동집단응답? _registeredGroup;
    private string _registeredDemandSourceKey = string.Empty;

    public string DishSearchText
    {
        get => _dishSearchText;
        set => SetProperty(ref _dishSearchText, value);
    }

    public IReadOnlyList<OfficialFoodRecipeDishDto> Dishes
    {
        get => _dishes;
        private set => SetProperty(ref _dishes, value);
    }

    public OfficialFoodDishDetailDto? SelectedDish
    {
        get => _selectedDish;
        private set
        {
            if (SetProperty(ref _selectedDish, value))
            {
                ClearIngredientWorkflow();
            }
        }
    }

    public string SelectedIngredientKey
    {
        get => _selectedIngredientKey;
        private set
        {
            if (SetProperty(ref _selectedIngredientKey, value))
            {
                ClearHsAndGrouping();
                OnPropertyChanged(nameof(SelectedIngredient));
            }
        }
    }

    public OfficialFoodRecipeIngredientDto? SelectedIngredient
        => SelectedDish?.Ingredients.FirstOrDefault(ingredient =>
            string.Equals(ingredient.IngredientKey, SelectedIngredientKey, StringComparison.Ordinal));

    public OfficialFoodIngredientHsMappingResponse? HsMapping
    {
        get => _hsMapping;
        private set => SetProperty(ref _hsMapping, value);
    }

    public long? SelectedHsMappingId
    {
        get => _selectedHsMappingId;
        private set
        {
            if (SetProperty(ref _selectedHsMappingId, value))
            {
                ClearGrouping();
                OnPropertyChanged(nameof(SelectedHsCandidate));
            }
        }
    }

    public OfficialFoodIngredientHsCandidateDto? SelectedHsCandidate
        => HsMapping?.Candidates.FirstOrDefault(candidate => candidate.MappingId == SelectedHsMappingId);

    public string UsZipCode
    {
        get => _usZipCode;
        set
        {
            var normalized = new string((value ?? string.Empty).Where(char.IsDigit).Take(5).ToArray());
            if (SetProperty(ref _usZipCode, normalized))
            {
                ClearGrouping();
            }
        }
    }

    public decimal DesiredQuantity
    {
        get => _desiredQuantity;
        set
        {
            if (SetProperty(ref _desiredQuantity, value))
            {
                ClearGrouping();
            }
        }
    }

    public string TemperatureCode
    {
        get => _temperatureCode;
        set
        {
            var normalized = value is "냉장" or "냉동" ? value : "상온";
            if (SetProperty(ref _temperatureCode, normalized))
            {
                ClearGrouping();
            }
        }
    }

    public bool ActionBusy
    {
        get => _actionBusy;
        private set
        {
            if (SetProperty(ref _actionBusy, value))
            {
                OnPropertyChanged(nameof(처리중));
            }
        }
    }

    public string? ActionError
    {
        get => _actionError;
        private set => SetProperty(ref _actionError, value);
    }

    public string? ActionNotice
    {
        get => _actionNotice;
        private set => SetProperty(ref _actionNotice, value);
    }

    public 공동구매자동집단배치미리보기응답? PlacementPreview
    {
        get => _placementPreview;
        private set => SetProperty(ref _placementPreview, value);
    }

    public 공동구매자동집단응답? RegisteredGroup
    {
        get => _registeredGroup;
        private set => SetProperty(ref _registeredGroup, value);
    }

    public bool IsAuthenticated => currentUserContext.현재사용자.인증됨;
    public bool CanWithdrawDemand => !string.IsNullOrWhiteSpace(_registeredDemandSourceKey);

    protected override bool 하위ViewModel처리중 => ActionBusy;

    public async Task<bool> SearchDishesAsync(CancellationToken cancellationToken = default)
        => await RunActionAsync(
            async token =>
            {
                var previousDishKey = SelectedDish?.Dish.DishKey;
                Dishes = await foodClient.SearchDishesAsync(CreateDishQuery(), token);
                var dishKey = Dishes.Any(dish =>
                        string.Equals(dish.DishKey, previousDishKey, StringComparison.Ordinal))
                    ? previousDishKey
                    : Dishes.FirstOrDefault()?.DishKey;
                await SelectDishCoreAsync(dishKey, token);
            },
            cancellationToken);

    public async Task<bool> SelectDishAsync(
        string? dishKey,
        CancellationToken cancellationToken = default)
        => await RunActionAsync(
            token => SelectDishCoreAsync(dishKey, token),
            cancellationToken);

    public void SelectIngredient(string? ingredientKey)
    {
        if (SelectedDish?.Ingredients.Any(ingredient =>
                string.Equals(ingredient.IngredientKey, ingredientKey, StringComparison.Ordinal)) == true)
        {
            SelectedIngredientKey = ingredientKey!;
        }
    }

    public async Task<bool> LoadHsReferencesAsync(CancellationToken cancellationToken = default)
    {
        var ingredient = SelectedIngredient;
        if (ingredient is null)
        {
            return FailAction("Choose an ingredient before loading tariff references.");
        }

        return await RunActionAsync(
            async token =>
            {
                HsMapping = await foodClient.GetHsCodesAsync(
                    new OfficialFoodIngredientHsQuery
                    {
                        IngredientKey = ingredient.IngredientKey,
                        IngredientName = ingredient.CanonicalName
                    },
                    token);
                SelectedHsMappingId = null;
                ActionNotice = HsMapping.Candidates.Count == 0
                    ? "No active tariff reference is indexed for this ingredient yet."
                    : "Choose a reference only after checking product form, processing, origin, and destination use.";
            },
            cancellationToken);
    }

    public void SelectHsReference(long mappingId)
    {
        if (HsMapping?.Candidates.Any(candidate => candidate.MappingId == mappingId) == true)
        {
            SelectedHsMappingId = mappingId;
            ActionNotice = "This code is stored as a planning reference, not as a customs classification decision.";
        }
    }

    public async Task<bool> PreviewPlacementAsync(CancellationToken cancellationToken = default)
    {
        if (!TryCreateDemand(out var demand, out var error))
        {
            return FailAction(error);
        }

        return await RunActionAsync(
            async token =>
            {
                PlacementPreview = await groupPurchaseService.자동배치미리보기Async(demand, token)
                    ?? throw new InvalidOperationException("The grouping preview response was empty.");
                ActionNotice = "Preview only: no demand, payment, order, or import commitment was created.";
            },
            cancellationToken);
    }

    public async Task<bool> JoinDemandPoolAsync(CancellationToken cancellationToken = default)
    {
        if (!TryCreateDemand(out var demand, out var error))
        {
            return FailAction(error);
        }

        if (CanWithdrawDemand
            && !string.Equals(_registeredDemandSourceKey, demand.수요출처키, StringComparison.Ordinal))
        {
            return FailAction("Withdraw your current interest before joining a different ingredient or ZIP pool.");
        }

        return await RunActionAsync(
            async token =>
            {
                RegisteredGroup = await groupPurchaseService.자동수요등록Async(demand, token)
                    ?? throw new InvalidOperationException("The demand registration response was empty.");
                _registeredDemandSourceKey = demand.수요출처키;
                OnPropertyChanged(nameof(CanWithdrawDemand));
                ActionNotice = "Your nonbinding interest is grouped by ingredient, U.S. ZIP, storage temperature, and logistics mode. No payment was taken.";
            },
            cancellationToken);
    }

    public async Task<bool> WithdrawDemandAsync(CancellationToken cancellationToken = default)
    {
        if (!CanWithdrawDemand)
        {
            return FailAction("There is no active interest to withdraw from this page.");
        }

        var demandSourceKey = _registeredDemandSourceKey;
        return await RunActionAsync(
            async token =>
            {
                var response = await groupPurchaseService.자동수요철회Async(
                        demandSourceKey,
                        "Withdrawn by the U.S. storefront user",
                        token)
                    ?? throw new InvalidOperationException("The demand withdrawal response was empty.");
                if (!response.철회완료)
                {
                    throw new InvalidOperationException(response.안내);
                }

                _registeredDemandSourceKey = string.Empty;
                RegisteredGroup = null;
                PlacementPreview = null;
                OnPropertyChanged(nameof(CanWithdrawDemand));
                ActionNotice = "Your nonbinding interest was withdrawn. No payment, order, import, or shipment action was created.";
            },
            cancellationToken);
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        Dishes = await foodClient.SearchDishesAsync(CreateDishQuery(), cancellationToken);
        await SelectDishCoreAsync(Dishes.FirstOrDefault()?.DishKey, cancellationToken);
    }

    private OfficialFoodDishDiscoveryQuery CreateDishQuery()
        => new()
        {
            CountryCode = KoreaCountryCode,
            SearchText = DishSearchText,
            Take = 24
        };

    private async Task SelectDishCoreAsync(string? dishKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dishKey))
        {
            SelectedDish = null;
            return;
        }

        SelectedDish = await foodClient.GetDishAsync(dishKey, cancellationToken);
        SelectedIngredientKey = SelectedDish?.Ingredients.FirstOrDefault()?.IngredientKey
            ?? string.Empty;
    }

    private bool TryCreateDemand(
        out 공동구매자동수요등록Command demand,
        out string error)
    {
        demand = new 공동구매자동수요등록Command();
        var currentUser = currentUserContext.현재사용자;
        if (!currentUser.인증됨)
        {
            error = "Sign in before previewing or joining a demand pool.";
            return false;
        }

        var ingredient = SelectedIngredient;
        if (ingredient is null)
        {
            error = "Choose an ingredient first.";
            return false;
        }

        if (SelectedHsCandidate is null)
        {
            error = "Choose an HS or HTS planning reference first.";
            return false;
        }

        if (UsZipCode.Length != 5)
        {
            error = "Enter a five-digit U.S. ZIP code.";
            return false;
        }

        if (DesiredQuantity <= 0)
        {
            error = "Enter a quantity greater than zero.";
            return false;
        }

        var userId = currentUser.UserId!;
        demand = new 공동구매자동수요등록Command
        {
            수요출처키 = CreateDemandSourceKey(userId, ingredient.IngredientKey, UsZipCode),
            상품키 = $"official-ingredient:{ingredient.IngredientKey}",
            상품명 = ingredient.CanonicalName,
            HS코드 = SelectedHsCandidate.HsCode,
            온도코드 = TemperatureCode,
            물류방식 = LogisticsMode,
            주문자키 = userId,
            주문자표시명 = currentUser.UserName ?? "U.S. group-buy member",
            배송권키 = $"us-zcta:{UsZipCode}",
            배송권명 = $"U.S. ZIP {UsZipCode}",
            희망수량 = DesiredQuantity,
            수량단위 = "kg",
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제,
            메모 = $"Nonbinding U.S. storefront interest; tariff mapping {SelectedHsCandidate.MappingId.ToString(CultureInfo.InvariantCulture)} is reference-only.",
            목표참여자수 = TargetParticipantCount,
            목표수량 = TargetQuantity
        };
        error = string.Empty;
        return true;
    }

    private async Task<bool> RunActionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        if (ActionBusy)
        {
            return false;
        }

        ActionBusy = true;
        ActionError = null;
        ActionNotice = null;
        try
        {
            await action(cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            ActionError = exception.Message;
            return false;
        }
        finally
        {
            ActionBusy = false;
        }
    }

    private bool FailAction(string error)
    {
        ActionError = error;
        ActionNotice = null;
        return false;
    }

    private void ClearIngredientWorkflow()
    {
        SelectedIngredientKey = string.Empty;
        ClearHsAndGrouping();
    }

    private void ClearHsAndGrouping()
    {
        HsMapping = null;
        SelectedHsMappingId = null;
        ClearGrouping();
    }

    private void ClearGrouping()
    {
        PlacementPreview = null;
        ActionError = null;
        ActionNotice = null;
    }

    private static string CreateDemandSourceKey(
        string userId,
        string ingredientKey,
        string zipCode)
    {
        var material = Encoding.UTF8.GetBytes($"{userId}|{ingredientKey}|{zipCode}");
        var digest = Convert.ToHexString(SHA256.HashData(material)).ToLowerInvariant();
        return $"us-food-interest:{digest[..32]}";
    }
}
