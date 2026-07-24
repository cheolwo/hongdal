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
    private IReadOnlyList<OfficialFoodIngredientDemandLine> _ingredientLines = [];
    private string _deliveryCountryCode = "KR";
    private string _deliveryAreaCode = string.Empty;
    private string _receiptMethodCode = OfficialFoodIngredientReceiptMethodCodes.SharedPickup;
    private string _transactionTypeCode = 공동구매거래유형코드.B2C;
    private string _priceBasisCode = 공동구매가격표시기준코드.부가세포함;
    private string _purchasingOrganizationReference = string.Empty;
    private string _purchasingOrganizationName = string.Empty;
    private bool _taxInvoiceRequired;
    private bool _공동주문후보참여동의;
    private string _temperatureCode = "상온";
    private decimal _desiredQuantity = 1m;
    private string _quantityUnit = "kg";
    private bool _actionBusy;
    private string? _actionError;
    private string? _actionNotice;
    private 공동구매자동집단배치미리보기응답? _placementPreview;
    private 공동구매자동집단사용자응답? _registeredGroup;
    private IReadOnlyList<공동구매자동집단배치미리보기응답> _placementPreviews = [];
    private IReadOnlyList<공동구매자동집단사용자응답> _registeredGroups = [];
    private readonly Dictionary<string, string> _registeredDemandSourceKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _withdrawOperationNonces = new(StringComparer.Ordinal);
    private string _previewFingerprint = string.Empty;
    private string _saveOperationNonce = Guid.NewGuid().ToString("N");

    public CommunityGroupPurchaseIngredientSeed? Seed
    {
        get => _seed;
        private set => SetProperty(ref _seed, value);
    }

    public IReadOnlyList<OfficialFoodIngredientDemandLine> IngredientLines
    {
        get => _ingredientLines;
        private set
        {
            if (SetProperty(ref _ingredientLines, value))
            {
                OnPropertyChanged(nameof(HasSeed));
            }
        }
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

    public string TransactionTypeCode
    {
        get => _transactionTypeCode;
        set
        {
            var normalized = 공동구매거래유형코드.정규화(value);
            if (!SetProperty(ref _transactionTypeCode, normalized))
            {
                return;
            }

            if (normalized == 공동구매거래유형코드.B2B)
            {
                _priceBasisCode = 공동구매가격표시기준코드.부가세별도;
                _taxInvoiceRequired = true;
            }
            else
            {
                _priceBasisCode = 공동구매가격표시기준코드.부가세포함;
                _purchasingOrganizationReference = string.Empty;
                _purchasingOrganizationName = string.Empty;
                _taxInvoiceRequired = false;
            }

            OnPropertyChanged(nameof(IsBusinessPurchase));
            OnPropertyChanged(nameof(PriceBasisCode));
            OnPropertyChanged(nameof(PurchasingOrganizationReference));
            OnPropertyChanged(nameof(PurchasingOrganizationName));
            OnPropertyChanged(nameof(TaxInvoiceRequired));
            DraftChanged();
        }
    }

    public bool IsBusinessPurchase => TransactionTypeCode == 공동구매거래유형코드.B2B;

    public string PriceBasisCode
    {
        get => _priceBasisCode;
        set
        {
            var normalized = 공동구매가격표시기준코드.정규화(value, TransactionTypeCode);
            if (SetProperty(ref _priceBasisCode, normalized))
            {
                DraftChanged();
            }
        }
    }

    public string PurchasingOrganizationReference
    {
        get => _purchasingOrganizationReference;
        set
        {
            var normalized = NormalizeText(value, 160);
            if (SetProperty(ref _purchasingOrganizationReference, normalized))
            {
                DraftChanged();
            }
        }
    }

    public string PurchasingOrganizationName
    {
        get => _purchasingOrganizationName;
        set
        {
            var normalized = NormalizeText(value, 160);
            if (SetProperty(ref _purchasingOrganizationName, normalized))
            {
                DraftChanged();
            }
        }
    }

    public bool TaxInvoiceRequired
    {
        get => _taxInvoiceRequired;
        set
        {
            var normalized = IsBusinessPurchase && value;
            if (SetProperty(ref _taxInvoiceRequired, normalized))
            {
                DraftChanged();
            }
        }
    }

    public bool 공동주문후보참여동의
    {
        get => _공동주문후보참여동의;
        set
        {
            if (SetProperty(ref _공동주문후보참여동의, value))
            {
                OnPropertyChanged(nameof(CanRegister));
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
                if (IngredientLines.Count > 0)
                {
                    IngredientLines[0].TemperatureCode = normalized;
                }
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
                if (IngredientLines.Count > 0)
                {
                    IngredientLines[0].DesiredQuantity = value;
                }
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
                if (IngredientLines.Count > 0)
                {
                    IngredientLines[0].QuantityUnit = normalized;
                }
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

    public IReadOnlyList<공동구매자동집단배치미리보기응답> PlacementPreviews
    {
        get => _placementPreviews;
        private set => SetProperty(ref _placementPreviews, value);
    }

    public IReadOnlyList<공동구매자동집단사용자응답> RegisteredGroups
    {
        get => _registeredGroups;
        private set => SetProperty(ref _registeredGroups, value);
    }

    public bool IsAuthenticated => currentUserContext.현재사용자.인증됨;
    public bool HasSeed => IngredientLines.Count > 0;
    public bool CanPreview => HasSeed && IsAuthenticated && !ActionBusy;
    public bool CanRegister
        => CanPreview
           && 공동주문후보참여동의
           && PlacementPreviews.Count == IngredientLines.Count
           && string.Equals(_previewFingerprint, DraftFingerprint(), StringComparison.Ordinal);
    public bool CanWithdraw => !ActionBusy && _registeredDemandSourceKeys.Count > 0;
    public bool HasActiveDemand => _registeredDemandSourceKeys.Count > 0;

    public void ApplySeed(CommunityGroupPurchaseIngredientSeed? seed)
    {
        ApplySeeds(seed is null ? [] : [seed]);
    }

    public void ApplySeeds(IEnumerable<CommunityGroupPurchaseIngredientSeed>? seeds)
    {
        var normalizedSeeds = (seeds ?? [])
            .Where(seed => seed is not null)
            .GroupBy(seed => seed.SuggestedProductKey, StringComparer.Ordinal)
            .Select(group => group.Last())
            .Take(CommunityGroupPurchaseIngredientSeed.MaxBundleItems)
            .ToArray();
        var currentFingerprint = string.Join("||", IngredientLines.Select(line => line.Seed.Fingerprint));
        var nextFingerprint = string.Join("||", normalizedSeeds.Select(seed => seed.Fingerprint));
        if (string.Equals(currentFingerprint, nextFingerprint, StringComparison.Ordinal))
        {
            return;
        }

        IngredientLines = normalizedSeeds
            .Select(seed => new OfficialFoodIngredientDemandLine(seed))
            .ToArray();
        Seed = IngredientLines.FirstOrDefault()?.Seed;
        _quantityUnit = IngredientLines.FirstOrDefault()?.QuantityUnit ?? "kg";
        _desiredQuantity = 1m;
        _deliveryCountryCode = "KR";
        _deliveryAreaCode = string.Empty;
        _receiptMethodCode = OfficialFoodIngredientReceiptMethodCodes.SharedPickup;
        _transactionTypeCode = 공동구매거래유형코드.B2C;
        _priceBasisCode = 공동구매가격표시기준코드.부가세포함;
        _purchasingOrganizationReference = string.Empty;
        _purchasingOrganizationName = string.Empty;
        _taxInvoiceRequired = false;
        _공동주문후보참여동의 = false;
        _temperatureCode = "상온";
        OnPropertyChanged(nameof(QuantityUnit));
        OnPropertyChanged(nameof(DesiredQuantity));
        OnPropertyChanged(nameof(DeliveryCountryCode));
        OnPropertyChanged(nameof(DeliveryAreaCode));
        OnPropertyChanged(nameof(ReceiptMethodCode));
        OnPropertyChanged(nameof(TransactionTypeCode));
        OnPropertyChanged(nameof(IsBusinessPurchase));
        OnPropertyChanged(nameof(PriceBasisCode));
        OnPropertyChanged(nameof(PurchasingOrganizationReference));
        OnPropertyChanged(nameof(PurchasingOrganizationName));
        OnPropertyChanged(nameof(TaxInvoiceRequired));
        OnPropertyChanged(nameof(공동주문후보참여동의));
        OnPropertyChanged(nameof(TemperatureCode));
        OnPropertyChanged(nameof(IngredientLines));
        OnPropertyChanged(nameof(HasSeed));
        ResetWorkflow(clearRegistration: true);
    }

    public void UpdateLineQuantity(OfficialFoodIngredientDemandLine line, decimal value)
    {
        ArgumentNullException.ThrowIfNull(line);
        line.DesiredQuantity = value;
        SynchronizeLegacyLine(line);
        DraftChanged();
    }

    public void UpdateLineUnit(OfficialFoodIngredientDemandLine line, string? value)
    {
        ArgumentNullException.ThrowIfNull(line);
        line.QuantityUnit = NormalizeText(value, 20);
        SynchronizeLegacyLine(line);
        DraftChanged();
    }

    public void UpdateLineTemperature(OfficialFoodIngredientDemandLine line, string? value)
    {
        ArgumentNullException.ThrowIfNull(line);
        line.TemperatureCode = value is "냉장" or "냉동" ? value : "상온";
        SynchronizeLegacyLine(line);
        DraftChanged();
    }

    public async Task<bool> PreviewAsync(CancellationToken cancellationToken = default)
    {
        if (!TryCreateDemands(out var demands, out var error))
        {
            return Fail(error);
        }

        return await RunActionAsync(
            async token =>
            {
                var previews = await Task.WhenAll(demands.Select(async demand =>
                    await demandService.수요배치미리보기Async(demand, token)
                    ?? throw new InvalidOperationException($"'{demand.상품명}' 집단화 미리보기 응답이 비어 있습니다.")));
                PlacementPreviews = previews;
                PlacementPreview = previews.FirstOrDefault();
                _previewFingerprint = DraftFingerprint();
                OnPropertyChanged(nameof(CanRegister));
                ActionNotice = $"재료 {previews.Length}개의 미리보기만 만들었습니다. 수요·결제·주문·수입·운송은 아직 생성되지 않았습니다.";
            },
            cancellationToken);
    }

    public async Task<bool> RegisterAsync(CancellationToken cancellationToken = default)
    {
        if (!공동주문후보참여동의)
        {
            return Fail("내 개별주문을 공동주문 할인 후보로 함께 보는 데 동의해 주세요.");
        }

        if (!CanRegister)
        {
            return Fail("현재 입력으로 공동주문 할인 후보를 먼저 확인해 주세요.");
        }

        if (!TryCreateDemands(out var demands, out var error))
        {
            return Fail(error);
        }

        var nextSourceKeys = demands
            .Select(demand => demand.수요출처키)
            .ToHashSet(StringComparer.Ordinal);
        if (HasActiveDemand
            && !_registeredDemandSourceKeys.Values.ToHashSet(StringComparer.Ordinal).SetEquals(nextSourceKeys))
        {
            return Fail("현재 묶음의 수요를 먼저 철회한 뒤 재료·수령 권역·온도 구성을 바꿔 주세요.");
        }

        return await RunActionAsync(
            async token =>
            {
                var groups = new List<공동구매자동집단사용자응답>(demands.Count);
                foreach (var demand in demands)
                {
                    var group = await demandService.비구속수요저장Async(demand, token)
                        ?? throw new InvalidOperationException($"'{demand.상품명}' 비구속 수요 저장 응답이 비어 있습니다.");
                    groups.Add(group);
                    _registeredDemandSourceKeys[demand.상품키] = demand.수요출처키;
                    if (!_withdrawOperationNonces.ContainsKey(demand.수요출처키))
                    {
                        _withdrawOperationNonces[demand.수요출처키] = Guid.NewGuid().ToString("N");
                    }
                }

                RegisteredGroups = groups;
                RegisteredGroup = groups.FirstOrDefault();
                OnPropertyChanged(nameof(HasActiveDemand));
                OnPropertyChanged(nameof(CanWithdraw));
                ActionNotice = $"재료 {groups.Count}개의 개별주문 의향 원장을 먼저 저장하고, 동의한 범위에서만 공동주문 할인 후보에 연결했습니다. 결제·계약·수입 신고·운송·창고 작업은 실행하지 않았습니다.";
            },
            cancellationToken);
    }

    public async Task<bool> WithdrawAsync(CancellationToken cancellationToken = default)
    {
        if (!CanWithdraw)
        {
            return Fail("이 화면에서 철회할 활성 수요가 없습니다.");
        }

        var registeredSourceKeys = _registeredDemandSourceKeys.Values
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return await RunActionAsync(
            async token =>
            {
                foreach (var demandSourceKey in registeredSourceKeys)
                {
                    var nonce = _withdrawOperationNonces.GetValueOrDefault(demandSourceKey)
                                ?? Guid.NewGuid().ToString("N");
                    var response = await demandService.비구속수요철회Async(
                            demandSourceKey,
                            OperationKey("demand-withdraw", nonce, demandSourceKey),
                            "음식·재료 탐색 다중 재료 수요 화면에서 철회",
                            token)
                        ?? throw new InvalidOperationException("비구속 수요 철회 응답이 비어 있습니다.");
                    if (!response.철회완료)
                    {
                        throw new InvalidOperationException(response.안내);
                    }
                }

                _registeredDemandSourceKeys.Clear();
                _withdrawOperationNonces.Clear();
                RegisteredGroups = [];
                RegisteredGroup = null;
                PlacementPreviews = [];
                PlacementPreview = null;
                _previewFingerprint = string.Empty;
                RotateSaveOperation();
                OnPropertyChanged(nameof(HasActiveDemand));
                OnPropertyChanged(nameof(CanWithdraw));
                ActionNotice = "개별 원함 원장을 철회하고 공개 모집 집계에서도 제외했습니다.";
            },
            cancellationToken);
    }

    private bool TryCreateDemands(
        out IReadOnlyList<공동구매자동수요등록Command> demands,
        out string error)
    {
        demands = [];
        if (IngredientLines.Count == 0)
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

        var invalidLine = IngredientLines.FirstOrDefault(line =>
            line.DesiredQuantity <= 0 || string.IsNullOrWhiteSpace(line.QuantityUnit));
        if (invalidLine is not null)
        {
            error = $"'{invalidLine.Seed.IngredientName}'에 0보다 큰 희망 수량과 수량 단위를 입력해 주세요.";
            return false;
        }

        var country = NormalizeCountryCode(DeliveryCountryCode);
        if (country.Length != 2)
        {
            error = "수령 국가를 선택해 주세요.";
            return false;
        }

        if (IsBusinessPurchase
            && string.IsNullOrWhiteSpace(PurchasingOrganizationReference)
            && string.IsNullOrWhiteSpace(PurchasingOrganizationName))
        {
            error = "B2B 공동구매에는 구매 조직 이름 또는 기존 조직 참조를 입력해 주세요.";
            return false;
        }

        var deliveryScopeKey = $"delivery:{country.ToLowerInvariant()}:{area}:{ReceiptMethodCode}";
        var deliveryScopeName = $"{country} {DeliveryAreaCode.Trim()} · {OfficialFoodIngredientReceiptMethodCodes.Label(ReceiptMethodCode)}";
        var fingerprint = DraftFingerprint();
        demands = IngredientLines.Select(line =>
        {
            var seed = line.Seed;
            var sourceKey = DemandSourceKey(
                user.UserId!,
                seed.SuggestedProductKey,
                deliveryScopeKey,
                line.TemperatureCode,
                ReceiptMethodCode,
                TransactionTypeCode);
            return new 공동구매자동수요등록Command
            {
                요청멱등키 = OperationKey(
                    "demand-save",
                    _saveOperationNonce,
                    $"{fingerprint}|{seed.SuggestedProductKey}"),
                수요출처키 = sourceKey,
                상품키 = seed.SuggestedProductKey,
                상품명 = seed.IngredientName,
                온도코드 = line.TemperatureCode,
                물류방식 = 공동구매자동수요물류방식코드.후속검토,
                거래유형 = TransactionTypeCode,
                가격표시기준 = PriceBasisCode,
                구매조직참조키 = IsBusinessPurchase ? PurchasingOrganizationReference : string.Empty,
                구매조직표시명 = IsBusinessPurchase ? PurchasingOrganizationName : string.Empty,
                세금계산서필요 = IsBusinessPurchase && TaxInvoiceRequired,
                주문자키 = user.UserId!,
                주문자표시명 = user.UserName ?? "공동구매 참여자",
                배송권키 = deliveryScopeKey,
                배송권명 = deliveryScopeName,
                희망수량 = line.DesiredQuantity,
                수량단위 = line.QuantityUnit,
                수요유형 = 공동구매자동수요유형코드.관심표시,
                결제상태 = 공동구매자동결제상태코드.미결제,
                메모 = BuildEvidenceMemo(seed),
                목표참여자수 = IsBusinessPurchase ? 1 : TargetParticipantCount,
                목표수량 = IsBusinessPurchase ? 30m : null
            };
        }).ToArray();
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
        PlacementPreviews = [];
        PlacementPreview = null;
        _previewFingerprint = string.Empty;
        ActionError = null;
        ActionNotice = null;
        RotateSaveOperation();
        OnPropertyChanged(nameof(CanRegister));
    }

    private void ResetWorkflow(bool clearRegistration)
    {
        PlacementPreviews = [];
        PlacementPreview = null;
        _previewFingerprint = string.Empty;
        ActionError = null;
        ActionNotice = null;
        RotateSaveOperation();
        if (clearRegistration)
        {
            _registeredDemandSourceKeys.Clear();
            _withdrawOperationNonces.Clear();
            RegisteredGroups = [];
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
        => string.Join("||",
            DeliveryCountryCode,
            NormalizeAreaCode(DeliveryAreaCode),
            ReceiptMethodCode,
            TransactionTypeCode,
            PriceBasisCode,
            PurchasingOrganizationReference,
            PurchasingOrganizationName,
            TaxInvoiceRequired,
            string.Join('|', IngredientLines.Select(line => string.Join('~',
                line.Seed.Fingerprint,
                line.TemperatureCode,
                line.DesiredQuantity.ToString(CultureInfo.InvariantCulture),
                line.QuantityUnit))));

    private void SynchronizeLegacyLine(OfficialFoodIngredientDemandLine line)
    {
        if (IngredientLines.Count == 0 || !ReferenceEquals(IngredientLines[0], line))
        {
            return;
        }

        _temperatureCode = line.TemperatureCode;
        _desiredQuantity = line.DesiredQuantity;
        _quantityUnit = line.QuantityUnit;
        OnPropertyChanged(nameof(TemperatureCode));
        OnPropertyChanged(nameof(DesiredQuantity));
        OnPropertyChanged(nameof(QuantityUnit));
    }

    private string BuildEvidenceMemo(CommunityGroupPurchaseIngredientSeed seed)
        => NormalizeText(string.Join(" · ", new[]
        {
            "공식 음식·재료 탐색에서 시작한 비구속 수요",
            $"재료 {seed.IngredientName} ({seed.IngredientKey})",
            string.IsNullOrWhiteSpace(seed.FoodName) ? null : $"음식 {seed.FoodName}",
            string.IsNullOrWhiteSpace(seed.RecipeTitle) ? null : $"레시피 {seed.RecipeTitle}",
            seed.SourcingModeLabel,
            공동구매거래유형코드.표시명(TransactionTypeCode),
            "음식 문화 국가를 상품 원산지나 출발국으로 자동 사용하지 않음"
        }.Where(value => !string.IsNullOrWhiteSpace(value))), 900);

    private static string DemandSourceKey(
        string userId,
        string productKey,
        string deliveryScopeKey,
        string temperatureCode,
        string receiptMethodCode,
        string transactionTypeCode)
        => $"food-interest:{Hash(string.Join('|', userId, productKey, deliveryScopeKey, temperatureCode, receiptMethodCode, transactionTypeCode))[..32]}";

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

public sealed class OfficialFoodIngredientDemandLine
{
    public OfficialFoodIngredientDemandLine(CommunityGroupPurchaseIngredientSeed seed)
    {
        ArgumentNullException.ThrowIfNull(seed);
        Seed = seed;
        QuantityUnit = string.IsNullOrWhiteSpace(seed.PurchaseUnit) ? "kg" : seed.PurchaseUnit;
    }

    public CommunityGroupPurchaseIngredientSeed Seed { get; }
    public string TemperatureCode { get; set; } = "상온";
    public decimal DesiredQuantity { get; set; } = 1m;
    public string QuantityUnit { get; set; }
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
