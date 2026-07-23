using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>
/// 여러 재료를 한 번에 확인하되 재료별 개별 원함과 멱등 요청을 각각 저장합니다.
/// 서로 다른 B2B/B2C·가격표시 기준을 한 저장 묶음에 섞지 않습니다.
/// </summary>
public sealed class GroupPurchaseWishBatchViewModel(
    I비구속공동구매수요Service demandService,
    ISsalddel현재사용자Context currentUserContext)
{
    private const int TargetParticipantCount = 5;
    private readonly List<GroupPurchaseWishDraftItem> _items = [];
    private string? _ownerUserId;

    public IReadOnlyList<GroupPurchaseWishDraftItem> Items => _items;
    public 현재사용자Snapshot CurrentUser => currentUserContext.현재사용자;
    public 주문자집단배송권Snapshot? DeliveryScope => CurrentUser.주문자집단배송권;

    public string TransactionType { get; set; } = 공동구매거래유형코드.B2C;
    public string PriceBasis { get; set; } = 공동구매가격표시기준코드.부가세포함;
    public string PurchasingOrganizationReference { get; set; } = string.Empty;
    public string PurchasingOrganizationName { get; set; } = string.Empty;
    public bool TaxInvoiceRequired { get; set; }
    public bool NonBindingAgreementAccepted { get; set; }
    public bool IsBusy { get; private set; }
    public string? StatusMessage { get; private set; }
    public bool HasError { get; private set; }

    public int SelectedCount => _items.Count(item => item.Selected);
    public int SavedCount => _items.Count(item => item.Saved);

    public bool IsBusinessPurchase
        => 공동구매거래유형코드.정규화(TransactionType) == 공동구매거래유형코드.B2B;

    public void Initialize(IReadOnlyList<HS먹거리공동구매상품카드> products)
    {
        ArgumentNullException.ThrowIfNull(products);
        if (_items.Count > 0)
        {
            return;
        }

        _items.AddRange(products.Select(product => new GroupPurchaseWishDraftItem(
            product,
            product.온도코드 == 공동구매온도코드.냉동 ? 20m : 5m)));
    }

    public void ApplyTransactionType(string? transactionType)
    {
        TransactionType = 공동구매거래유형코드.정규화(transactionType);
        if (!IsBusinessPurchase)
        {
            PriceBasis = 공동구매가격표시기준코드.부가세포함;
            PurchasingOrganizationReference = string.Empty;
            PurchasingOrganizationName = string.Empty;
            TaxInvoiceRequired = false;
        }
    }

    public void PrepareForCurrentUser()
    {
        var currentOwner = CurrentUser.UserId;
        if (string.Equals(_ownerUserId, currentOwner, StringComparison.Ordinal))
        {
            return;
        }

        _ownerUserId = currentOwner;
        foreach (var item in _items)
        {
            item.Reset();
        }

        TransactionType = 공동구매거래유형코드.B2C;
        PriceBasis = 공동구매가격표시기준코드.부가세포함;
        PurchasingOrganizationReference = string.Empty;
        PurchasingOrganizationName = string.Empty;
        TaxInvoiceRequired = false;
        NonBindingAgreementAccepted = false;
        StatusMessage = null;
        HasError = false;
    }

    public void SelectAll(bool selected)
    {
        foreach (var item in _items.Where(item => !item.Saved))
        {
            item.Selected = selected;
        }
    }

    public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return false;
        }

        var validation = Validate();
        if (validation is not null)
        {
            return Fail(validation);
        }

        IsBusy = true;
        HasError = false;
        StatusMessage = null;
        var selected = _items.Where(item => item.Selected && !item.Saved).ToArray();
        var successCount = 0;
        try
        {
            foreach (var item in selected)
            {
                item.IsBusy = true;
                item.ErrorMessage = null;
                try
                {
                    var command = BuildCommand(item);
                    item.PlacementPreview = await demandService.수요배치미리보기Async(
                        command,
                        cancellationToken);
                    item.SavedGroup = await demandService.비구속수요저장Async(
                        command,
                        cancellationToken);
                    if (item.PlacementPreview is null || item.SavedGroup is null)
                    {
                        throw new InvalidOperationException("자동집단 미리보기 또는 개별 원함 저장 결과를 확인하지 못했습니다.");
                    }

                    item.DemandSourceKey = command.수요출처키;
                    item.WishLedgerId = item.SavedGroup.수요목록.FirstOrDefault()?.개별원함원장Id
                                        ?? string.Empty;
                    item.Saved = true;
                    item.Selected = false;
                    item.Notice = item.PlacementPreview.배치유형 == 공동구매자동집단배치유형코드.기존집단
                        ? "개별 원함을 저장하고 기존 집단에 연결했습니다."
                        : "개별 원함을 저장하고 새 집단을 시작했습니다.";
                    successCount++;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    item.ErrorMessage = exception.Message;
                }
                finally
                {
                    item.IsBusy = false;
                }
            }

            var failedCount = selected.Length - successCount;
            HasError = failedCount > 0;
            StatusMessage = failedCount == 0
                ? $"{successCount:N0}개 재료의 개별 원함을 저장했습니다. 결제·주문·수입·운송은 실행되지 않았습니다."
                : $"{successCount:N0}개는 저장했고 {failedCount:N0}개는 저장하지 못했습니다. 실패한 재료만 다시 시도할 수 있습니다.";
            return failedCount == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string? Validate()
    {
        if (!CurrentUser.인증됨)
        {
            return "여러 재료의 개별 원함을 저장하려면 먼저 로그인해 주세요.";
        }

        if (DeliveryScope is null || string.IsNullOrWhiteSpace(DeliveryScope.ScopeKey))
        {
            return "가입 또는 온보딩에서 주문자 배송권을 먼저 설정해 주세요.";
        }

        if (!_items.Any(item => item.Selected && !item.Saved))
        {
            return "원하는 재료를 하나 이상 선택해 주세요.";
        }

        if (_items.Any(item => item.Selected && !item.Saved && item.Quantity <= 0))
        {
            return "선택한 모든 재료의 희망 수량은 0보다 커야 합니다.";
        }

        if (!NonBindingAgreementAccepted)
        {
            return "비구속 원함이며 결제나 주문 확정이 아니라는 안내를 확인해 주세요.";
        }

        if (IsBusinessPurchase
            && string.IsNullOrWhiteSpace(PurchasingOrganizationReference)
            && string.IsNullOrWhiteSpace(PurchasingOrganizationName))
        {
            return "B2B 원함에는 구매 조직 참조 또는 표시명을 입력해 주세요.";
        }

        return null;
    }

    private bool Fail(string message)
    {
        HasError = true;
        StatusMessage = message;
        return false;
    }

    private 공동구매자동수요등록Command BuildCommand(GroupPurchaseWishDraftItem item)
    {
        var user = CurrentUser;
        var scope = DeliveryScope!;
        var transactionType = 공동구매거래유형코드.정규화(TransactionType);
        var priceBasis = 공동구매가격표시기준코드.정규화(PriceBasis, transactionType);
        var purchasingOrganizationIdentity = !string.IsNullOrWhiteSpace(PurchasingOrganizationReference)
            ? PurchasingOrganizationReference.Trim()
            : PurchasingOrganizationName.Trim();
        var demandSourceKey = $"orderer-wish:{Hash(string.Join(
            '|',
            user.UserId,
            item.Product.상품카드Id,
            scope.ScopeKey,
            item.Product.온도코드,
            transactionType,
            priceBasis,
            purchasingOrganizationIdentity))[..32]}";

        return new 공동구매자동수요등록Command
        {
            요청멱등키 = $"wish-save:{Hash(string.Join(
                '|',
                item.OperationNonce,
                demandSourceKey,
                item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)))[..32]}",
            수요출처키 = demandSourceKey,
            상품키 = item.Product.상품카드Id,
            상품명 = item.Product.상품명,
            HS코드 = item.Product.HS코드,
            온도코드 = item.Product.온도코드,
            물류방식 = 공동구매자동수요물류방식코드.후속검토,
            거래유형 = transactionType,
            가격표시기준 = priceBasis,
            구매조직참조키 = IsBusinessPurchase ? PurchasingOrganizationReference.Trim() : string.Empty,
            구매조직표시명 = IsBusinessPurchase ? PurchasingOrganizationName.Trim() : string.Empty,
            세금계산서필요 = IsBusinessPurchase && TaxInvoiceRequired,
            주문자키 = user.UserId!,
            주문자표시명 = user.UserName ?? "공동구매 참여자",
            배송권키 = scope.ScopeKey,
            배송권명 = scope.DisplayName,
            희망수량 = item.Quantity,
            수량단위 = "kg",
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제,
            메모 = "주문자 앱에서 여러 재료를 각각 선택해 저장한 비구속 개별 원함",
            목표참여자수 = TargetParticipantCount,
            목표수량 = item.Product.SuggestedTargetQuantityKg
        };
    }

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

public sealed class GroupPurchaseWishDraftItem(
    HS먹거리공동구매상품카드 product,
    decimal quantity)
{
    private readonly decimal _defaultQuantity = quantity;
    public HS먹거리공동구매상품카드 Product { get; } = product;
    public decimal Quantity { get; set; } = quantity;
    public bool Selected { get; set; }
    public bool Saved { get; internal set; }
    public bool IsBusy { get; internal set; }
    public string? ErrorMessage { get; internal set; }
    public string? Notice { get; internal set; }
    public string DemandSourceKey { get; internal set; } = string.Empty;
    public string WishLedgerId { get; internal set; } = string.Empty;
    public 공동구매자동집단배치미리보기응답? PlacementPreview { get; internal set; }
    public 공동구매자동집단사용자응답? SavedGroup { get; internal set; }
    internal string OperationNonce { get; } = Guid.NewGuid().ToString("N");

    internal void Reset()
    {
        Quantity = _defaultQuantity;
        Selected = false;
        Saved = false;
        IsBusy = false;
        ErrorMessage = null;
        Notice = null;
        DemandSourceKey = string.Empty;
        WishLedgerId = string.Empty;
        PlacementPreview = null;
        SavedGroup = null;
    }
}
