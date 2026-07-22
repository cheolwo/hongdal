using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed class OfficialFoodIngredientDemandViewModel(
    I비구속공동구매수요Service demandService,
    ISsalddel현재사용자Context currentUserContext) : ObservableObject
{
    private const int TargetParticipantCount = 5;
    private CommunityGroupPurchaseIngredientSeed? _seed;
    private string _deliveryCountryCode = "KR";
    private string _deliveryAreaCode = string.Empty;
    private string _receiptMethodCode = OfficialFoodIngredientReceiptMethodCodes.SharedPickup;
    private string _temperatureCode = "상온";
    private decimal _desiredQuantity = 1m;
    private string _quantityUnit = "kg";
    private bool _actionBusy;
    private string? _actionError;
    private string? _actionNotice;
    private 공동구매자동집단배치미리보기응답? _placementPreview;
    private 공동구매자동집단사용자응답? _registeredGroup;
    private string _previewFingerprint = string.Empty;
    private string _registeredDemandSourceKey = string.Empty;
    private string _saveOperationNonce = Guid.NewGuid().ToString("N");
    private string _withdrawOperationNonce = string.Empty;

    public CommunityGroupPurchaseIngredientSeed? Seed
    {
        get => _seed;
        private set => SetProperty(ref _seed, value);
    }

    public string DeliveryCountryCode
    {
        get => _deliveryCountryCode;
        set
        {
            var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
            if (SetProperty(ref _deliveryCountryCode, normalized))
            {
                DraftChanged();
            }
        }
    }

    public string DeliveryAreaCode
    {
        get => _deliveryAreaCode;
        set
        {
            var normalized = NormalizeText(value, 24);
            if (SetProperty(ref _deliveryAreaCode, normalized))
            {
                DraftChanged();
            }
        }
    }

    public string ReceiptMethodCode
    {
        get => _receiptMethodCode;
        set
        {
            var normalized = OfficialFoodIngredientReceiptMethodCodes.Normalize(value);
            if (SetProperty(ref _receiptMethodCode, normalized))
            {
                DraftChanged();
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
                DraftChanged();
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
                DraftChanged();
            }
        }
    }

    public string QuantityUnit
    {
        get => _quantityUnit;
        set
        {
            var normalized = NormalizeText(value, 20);
            if (SetProperty(ref _quantityUnit, normalized))
            {
                DraftChanged();
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
                OnPropertyChanged(nameof(CanPreview));
                OnPropertyChanged(nameof(CanRegister));
                OnPropertyChanged(nameof(CanWithdraw));
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
        private set
        {
            if (SetProperty(ref _placementPreview, value))
            {
                OnPropertyChanged(nameof(CanRegister));
            }
        }
    }

    public 공동구매자동집단사용자응답? RegisteredGroup
    {
        get => _registeredGroup;
        private set => SetProperty(ref _registeredGroup, value);
    }

    public bool IsAuthenticated => currentUserContext.현재사용자.인증됨;
    public bool HasSeed => Seed is not null;
    public bool CanPreview => HasSeed && IsAuthenticated && !ActionBusy;
    public bool CanRegister
        => CanPreview
           && PlacementPreview is not null
           && string.Equals(_previewFingerprint, DraftFingerprint(), StringComparison.Ordinal);
    public bool CanWithdraw => !ActionBusy && !string.IsNullOrWhiteSpace(_registeredDemandSourceKey);
    public bool HasActiveDemand => !string.IsNullOrWhiteSpace(_registeredDemandSourceKey);

    public void ApplySeed(CommunityGroupPurchaseIngredientSeed? seed)
    {
        if (string.Equals(Seed?.Fingerprint, seed?.Fingerprint, StringComparison.Ordinal))
        {
            return;
        }

        Seed = seed;
        _quantityUnit = seed?.PurchaseUnit ?? "kg";
        _desiredQuantity = 1m;
        _deliveryCountryCode = "KR";
        _deliveryAreaCode = string.Empty;
        _receiptMethodCode = OfficialFoodIngredientReceiptMethodCodes.SharedPickup;
        _temperatureCode = "상온";
        OnPropertyChanged(nameof(QuantityUnit));
        OnPropertyChanged(nameof(DesiredQuantity));
        OnPropertyChanged(nameof(DeliveryCountryCode));
        OnPropertyChanged(nameof(DeliveryAreaCode));
        OnPropertyChanged(nameof(ReceiptMethodCode));
        OnPropertyChanged(nameof(TemperatureCode));
        OnPropertyChanged(nameof(HasSeed));
        ResetWorkflow(clearRegistration: true);
    }

    public async Task<bool> PreviewAsync(CancellationToken cancellationToken = default)
    {
        if (!TryCreateDemand(out var demand, out var error))
        {
            return Fail(error);
        }

        return await RunActionAsync(
            async token =>
            {
                PlacementPreview = await demandService.수요배치미리보기Async(demand, token)
                    ?? throw new InvalidOperationException("집단화 미리보기 응답이 비어 있습니다.");
                _previewFingerprint = DraftFingerprint();
                OnPropertyChanged(nameof(CanRegister));
                ActionNotice = "미리보기만 만들었습니다. 수요·결제·주문·수입·운송은 아직 생성되지 않았습니다.";
            },
            cancellationToken);
    }

    public async Task<bool> RegisterAsync(CancellationToken cancellationToken = default)
    {
        if (!CanRegister)
        {
            return Fail("현재 입력으로 집단화 미리보기를 먼저 확인해 주세요.");
        }

        if (!TryCreateDemand(out var demand, out var error))
        {
            return Fail(error);
        }

        if (HasActiveDemand
            && !string.Equals(_registeredDemandSourceKey, demand.수요출처키, StringComparison.Ordinal))
        {
            return Fail("현재 수요를 먼저 철회한 뒤 다른 재료·수령 권역의 수요를 등록해 주세요.");
        }

        return await RunActionAsync(
            async token =>
            {
                RegisteredGroup = await demandService.비구속수요저장Async(demand, token)
                    ?? throw new InvalidOperationException("비구속 수요 저장 응답이 비어 있습니다.");
                _registeredDemandSourceKey = demand.수요출처키;
                _withdrawOperationNonce = Guid.NewGuid().ToString("N");
                OnPropertyChanged(nameof(HasActiveDemand));
                OnPropertyChanged(nameof(CanWithdraw));
                ActionNotice = "비구속 수요를 저장했습니다. 결제·주문·계약·수입 신고·운송·창고 작업은 실행하지 않았습니다.";
            },
            cancellationToken);
    }

    public async Task<bool> WithdrawAsync(CancellationToken cancellationToken = default)
    {
        if (!CanWithdraw)
        {
            return Fail("이 화면에서 철회할 활성 수요가 없습니다.");
        }

        var demandSourceKey = _registeredDemandSourceKey;
        var idempotencyKey = OperationKey("demand-withdraw", _withdrawOperationNonce, demandSourceKey);
        return await RunActionAsync(
            async token =>
            {
                var response = await demandService.비구속수요철회Async(
                        demandSourceKey,
                        idempotencyKey,
                        "음식·재료 탐색 수요 화면에서 철회",
                        token)
                    ?? throw new InvalidOperationException("비구속 수요 철회 응답이 비어 있습니다.");
                if (!response.철회완료)
                {
                    throw new InvalidOperationException(response.안내);
                }

                _registeredDemandSourceKey = string.Empty;
                _withdrawOperationNonce = string.Empty;
                RegisteredGroup = null;
                PlacementPreview = null;
                _previewFingerprint = string.Empty;
                RotateSaveOperation();
                OnPropertyChanged(nameof(HasActiveDemand));
                OnPropertyChanged(nameof(CanWithdraw));
                ActionNotice = "비구속 수요를 철회했습니다. 공개 모집 집계에서도 제외됩니다.";
            },
            cancellationToken);
    }

    private bool TryCreateDemand(
        out 공동구매자동수요등록Command demand,
        out string error)
    {
        demand = new 공동구매자동수요등록Command();
        var seed = Seed;
        if (seed is null)
        {
            error = "음식·재료 탐색 화면에서 재료를 먼저 선택해 주세요.";
            return false;
        }

        var user = currentUserContext.현재사용자;
        if (!user.인증됨)
        {
            error = "비구속 수요의 저장·변경·철회에는 로그인이 필요합니다.";
            return false;
        }

        var area = NormalizeAreaCode(DeliveryAreaCode);
        if (area.Length < 2)
        {
            error = "상세 주소가 아닌 우편번호 또는 생활권 코드를 입력해 주세요.";
            return false;
        }

        if (DesiredQuantity <= 0 || string.IsNullOrWhiteSpace(QuantityUnit))
        {
            error = "0보다 큰 희망 수량과 수량 단위를 입력해 주세요.";
            return false;
        }

        var country = NormalizeCountryCode(DeliveryCountryCode);
        if (country.Length != 2)
        {
            error = "수령 국가를 선택해 주세요.";
            return false;
        }

        var deliveryScopeKey = $"delivery:{country.ToLowerInvariant()}:{area}:{ReceiptMethodCode}";
        var deliveryScopeName = $"{country} {DeliveryAreaCode.Trim()} · {OfficialFoodIngredientReceiptMethodCodes.Label(ReceiptMethodCode)}";
        var sourceKey = DemandSourceKey(
            user.UserId!,
            seed.SuggestedProductKey,
            deliveryScopeKey,
            TemperatureCode,
            ReceiptMethodCode);
        var fingerprint = DraftFingerprint();

        demand = new 공동구매자동수요등록Command
        {
            요청멱등키 = OperationKey("demand-save", _saveOperationNonce, fingerprint),
            수요출처키 = sourceKey,
            상품키 = seed.SuggestedProductKey,
            상품명 = seed.IngredientName,
            온도코드 = TemperatureCode,
            물류방식 = ReceiptMethodCode,
            주문자키 = user.UserId!,
            주문자표시명 = user.UserName ?? "공동구매 참여자",
            배송권키 = deliveryScopeKey,
            배송권명 = deliveryScopeName,
            희망수량 = DesiredQuantity,
            수량단위 = QuantityUnit,
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제,
            메모 = BuildEvidenceMemo(seed),
            목표참여자수 = TargetParticipantCount
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

    private bool Fail(string message)
    {
        ActionError = message;
        ActionNotice = null;
        return false;
    }

    private void DraftChanged()
    {
        PlacementPreview = null;
        _previewFingerprint = string.Empty;
        ActionError = null;
        ActionNotice = null;
        RotateSaveOperation();
        OnPropertyChanged(nameof(CanRegister));
    }

    private void ResetWorkflow(bool clearRegistration)
    {
        PlacementPreview = null;
        _previewFingerprint = string.Empty;
        ActionError = null;
        ActionNotice = null;
        RotateSaveOperation();
        if (clearRegistration)
        {
            _registeredDemandSourceKey = string.Empty;
            _withdrawOperationNonce = string.Empty;
            RegisteredGroup = null;
        }

        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(CanPreview));
        OnPropertyChanged(nameof(CanRegister));
        OnPropertyChanged(nameof(CanWithdraw));
        OnPropertyChanged(nameof(HasActiveDemand));
    }

    private void RotateSaveOperation()
        => _saveOperationNonce = Guid.NewGuid().ToString("N");

    private string DraftFingerprint()
        => string.Join('|',
            Seed?.Fingerprint,
            DeliveryCountryCode,
            NormalizeAreaCode(DeliveryAreaCode),
            ReceiptMethodCode,
            TemperatureCode,
            DesiredQuantity.ToString(CultureInfo.InvariantCulture),
            QuantityUnit);

    private static string BuildEvidenceMemo(CommunityGroupPurchaseIngredientSeed seed)
        => NormalizeText(string.Join(" · ", new[]
        {
            "공식 음식·재료 탐색에서 시작한 비구속 수요",
            $"재료 {seed.IngredientName} ({seed.IngredientKey})",
            string.IsNullOrWhiteSpace(seed.FoodName) ? null : $"음식 {seed.FoodName}",
            string.IsNullOrWhiteSpace(seed.RecipeTitle) ? null : $"레시피 {seed.RecipeTitle}",
            seed.SourcingModeLabel,
            "음식 문화 국가를 상품 원산지나 출발국으로 자동 사용하지 않음"
        }.Where(value => !string.IsNullOrWhiteSpace(value))), 900);

    private static string DemandSourceKey(
        string userId,
        string productKey,
        string deliveryScopeKey,
        string temperatureCode,
        string receiptMethodCode)
        => $"food-interest:{Hash(string.Join('|', userId, productKey, deliveryScopeKey, temperatureCode, receiptMethodCode))[..32]}";

    private static string OperationKey(string prefix, string nonce, string material)
        => $"{prefix}:{Hash(string.Join('|', nonce, material))[..32]}";

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string NormalizeAreaCode(string? value)
        => new((value ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Where(character => char.IsLetterOrDigit(character) || character == '-')
            .Take(16)
            .ToArray());

    private static string NormalizeCountryCode(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalized.Length == 2 && normalized.All(character => character is >= 'A' and <= 'Z')
            ? normalized
            : string.Empty;
    }

    private static string NormalizeText(string? value, int maxLength)
    {
        var normalized = string.Join(' ', (value ?? string.Empty)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}

public static class OfficialFoodIngredientReceiptMethodCodes
{
    public const string SharedPickup = "shared-pickup";
    public const string LocalHub = "local-hub";
    public const string Parcel = "parcel";

    public static string Normalize(string? value)
        => value is LocalHub or Parcel ? value : SharedPickup;

    public static string Label(string? value)
        => Normalize(value) switch
        {
            LocalHub => "생활권 거점 수령",
            Parcel => "택배 수령",
            _ => "공동 수령"
        };
}
