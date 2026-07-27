using Ssalddel.Contracts.Common.Inventory;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace SellerApp.ViewModels;

public sealed class SellerInventoryPageViewModel(
    IWarehouseWorkspaceService warehouseService)
{
    public IReadOnlyList<재고항목응답> Items { get; private set; } = [];
    public bool IsBusy { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            Items = (await warehouseService.GetInventoryAsync(cancellationToken))?.Items ?? [];
        }
        catch (Exception exception)
        {
            Items = [];
            ErrorMessage = SellerSalesPageError.Resolve(
                exception,
                "재고 원장을 불러오지 못했습니다.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class SellerProductsPageViewModel(I상품등록Service productService)
{
    public IReadOnlyList<판매상품항목응답> Products { get; private set; } = [];
    public bool IsBusy { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            Products = await productService.상품목록조회Async(cancellationToken);
        }
        catch (Exception exception)
        {
            Products = [];
            ErrorMessage = SellerSalesPageError.Resolve(
                exception,
                "판매상품 원장을 불러오지 못했습니다.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class SellerProductCreatePageViewModel(
    IWarehouseWorkspaceService warehouseService,
    I상품등록Service productService)
{
    public 재고항목응답? Inventory { get; private set; }
    public 판매상품항목응답? CreatedProduct { get; private set; }
    public string ProductName { get; set; } = string.Empty;
    public string SalesSku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsBusy { get; private set; }
    public string? StatusMessage { get; private set; }
    public bool HasError { get; private set; }

    public async Task LoadAsync(long? inventoryItemId, CancellationToken cancellationToken = default)
    {
        Inventory = null;
        CreatedProduct = null;
        StatusMessage = null;
        HasError = false;
        if (inventoryItemId is not > 0)
        {
            SetError("판매상품으로 등록할 재고를 먼저 선택해 주세요.");
            return;
        }

        IsBusy = true;
        try
        {
            var response = await warehouseService.GetInventoryAsync(cancellationToken);
            Inventory = response?.Items.FirstOrDefault(
                item => item.입고상품Id == inventoryItemId.Value);
            if (Inventory is null)
            {
                SetError("현재 판매자 계정에서 이 재고를 찾을 수 없습니다.");
                return;
            }

            ProductName = Inventory.상품명;
            SalesSku = Inventory.SKU;
            Price = 0m;
        }
        catch (Exception exception)
        {
            SetError(SellerSalesPageError.Resolve(exception, "재고 원장을 불러오지 못했습니다."));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> CreateAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy || Inventory is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(ProductName)
            || string.IsNullOrWhiteSpace(SalesSku)
            || Price < 0)
        {
            SetError("상품명, 판매 SKU와 0원 이상의 판매가를 확인해 주세요.");
            return false;
        }

        IsBusy = true;
        HasError = false;
        StatusMessage = "판매상품 원장에 저장하는 중입니다.";
        try
        {
            CreatedProduct = await productService.상품생성Async(
                new 판매상품저장요청
                {
                    입고상품Id = Inventory.입고상품Id,
                    대표상품명 = ProductName.Trim(),
                    판매SKU = SalesSku.Trim(),
                    판매가 = Price,
                    샘플데이터여부 = false
                },
                cancellationToken);
            if (CreatedProduct is null)
            {
                SetError("판매상품 저장 결과를 확인하지 못했습니다.");
                return false;
            }

            StatusMessage = $"판매상품 #{CreatedProduct.Id}을 저장했습니다.";
            return true;
        }
        catch (Exception exception)
        {
            SetError(SellerSalesPageError.Resolve(exception, "판매상품을 저장하지 못했습니다."));
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetError(string message)
    {
        HasError = true;
        StatusMessage = message;
    }
}

public sealed class SellerListingsPageViewModel(
    I채널출품Service listingService,
    I상품등록Service productService,
    I판매채널계정읽기Service accountService)
{
    public IReadOnlyList<채널출품항목응답> Listings { get; private set; } = [];
    public IReadOnlyDictionary<long, 판매상품항목응답> Products { get; private set; }
        = new Dictionary<long, 판매상품항목응답>();
    public IReadOnlyDictionary<long, 판매채널계정항목응답> Accounts { get; private set; }
        = new Dictionary<long, 판매채널계정항목응답>();
    public bool IsBusy { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var listingsTask = listingService.출품목록조회Async(cancellationToken);
            var productsTask = productService.상품목록조회Async(cancellationToken);
            var accountsTask = accountService.계정목록조회Async(cancellationToken);
            await Task.WhenAll(listingsTask, productsTask, accountsTask);

            Listings = await listingsTask;
            Products = (await productsTask).ToDictionary(item => item.Id);
            Accounts = (await accountsTask).ToDictionary(item => item.Id);
        }
        catch (Exception exception)
        {
            Listings = [];
            Products = new Dictionary<long, 판매상품항목응답>();
            Accounts = new Dictionary<long, 판매채널계정항목응답>();
            ErrorMessage = SellerSalesPageError.Resolve(
                exception,
                "채널 출품 원장을 불러오지 못했습니다.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public string ProductName(long productId)
        => Products.TryGetValue(productId, out var product)
            ? product.대표상품명
            : $"판매상품 #{productId}";

    public string AccountName(long accountId)
        => Accounts.TryGetValue(accountId, out var account)
            ? $"{account.채널종류} · {account.상점명}"
            : $"채널계정 #{accountId}";
}

public sealed class SellerListingCreatePageViewModel(
    I채널출품Service listingService,
    I상품등록Service productService,
    I판매채널계정읽기Service accountService)
{
    public IReadOnlyList<판매상품항목응답> Products { get; private set; } = [];
    public IReadOnlyList<판매채널계정항목응답> Accounts { get; private set; } = [];
    public long? SelectedProductId { get; set; }
    public long? SelectedAccountId { get; set; }
    public 채널출품항목응답? CreatedListing { get; private set; }
    public bool IsBusy { get; private set; }
    public string? StatusMessage { get; private set; }
    public bool HasError { get; private set; }

    public async Task LoadAsync(long? productId, CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        StatusMessage = null;
        HasError = false;
        CreatedListing = null;
        try
        {
            var productsTask = productService.상품목록조회Async(cancellationToken);
            var accountsTask = accountService.계정목록조회Async(cancellationToken);
            await Task.WhenAll(productsTask, accountsTask);
            Products = await productsTask;
            Accounts = await accountsTask;
            SelectedProductId = productId is > 0
                && Products.Any(item => item.Id == productId.Value)
                    ? productId
                    : null;
            SelectedAccountId = null;
        }
        catch (Exception exception)
        {
            Products = [];
            Accounts = [];
            SetError(SellerSalesPageError.Resolve(
                exception,
                "출품에 필요한 판매상품과 채널 계정을 불러오지 못했습니다."));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> CreateAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return false;
        }

        if (SelectedProductId is not > 0 || SelectedAccountId is not > 0)
        {
            SetError("판매상품과 판매채널 계정을 각각 선택해 주세요.");
            return false;
        }

        IsBusy = true;
        HasError = false;
        StatusMessage = "내부 채널 출품 원장을 저장하는 중입니다.";
        try
        {
            CreatedListing = await listingService.출품생성Async(
                new 채널출품저장요청
                {
                    판매상품Id = SelectedProductId.Value,
                    판매채널계정Id = SelectedAccountId.Value
                },
                cancellationToken);
            if (CreatedListing is null)
            {
                SetError("채널 출품 저장 결과를 확인하지 못했습니다.");
                return false;
            }

            StatusMessage =
                $"채널 출품 #{CreatedListing.Id}을 준비 원장에 저장했습니다. 외부 채널 발행은 실행하지 않았습니다.";
            return true;
        }
        catch (Exception exception)
        {
            SetError(SellerSalesPageError.Resolve(
                exception,
                "채널 출품 준비 원장을 저장하지 못했습니다."));
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetError(string message)
    {
        HasError = true;
        StatusMessage = message;
    }
}

internal static class SellerSalesPageError
{
    public static string Resolve(Exception exception, string fallback)
        => exception switch
        {
            SsalddelApiException { StatusCode: 401 } =>
                "로그인 세션이 만료되었습니다. 다시 로그인해 주세요.",
            SsalddelApiException { StatusCode: 403 } =>
                "판매자 또는 화주 역할과 원장 소유 범위를 확인해 주세요.",
            SsalddelApiException { StatusCode: 404 } =>
                "현재 판매자 계정에서 요청한 원장을 찾을 수 없습니다.",
            _ => fallback
        };
}
