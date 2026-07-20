using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Mart;

public partial class OrdererMartCatalogWorkspace
{
    private bool _initialized;

    [Parameter]
    public long? ProductId { get; set; }

    [Parameter]
    public EventCallback<long?> ProductSelected { get; set; }

    [Parameter]
    public string? OrderRequestBasePath { get; set; }

    private 마트페이지접근ViewModel Access => ViewModel.접근;
    private 마트공개상품목록ViewModel List => ViewModel.목록;
    private 마트공개상품상세ViewModel Detail => ViewModel.상세;

    private string EmptyMessage => List.검색조건사용중
        ? "검색어나 판매 가능 조건을 바꿔 다시 조회해 주세요."
        : "운영자가 공개 판매 상품 투영을 등록하면 이 목록에 표시됩니다.";

    protected override async Task OnInitializedAsync()
    {
        await InitializeAsync();
        _initialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_initialized || !Access.사용가능)
        {
            return;
        }

        if (ProductId is long productId)
        {
            if (Detail.요청ProductId != productId)
            {
                await Detail.조회Async(productId);
            }
        }
        else if (Detail.요청ProductId.HasValue)
        {
            Detail.선택해제();
        }
    }

    private async Task InitializeAsync()
    {
        if (!await Access.확인Async() || !Access.사용가능)
        {
            return;
        }

        var listTask = List.초기화됨 ? Task.FromResult(true) : List.조회Async();
        var detailTask = ProductId is long productId
            ? Detail.조회Async(productId)
            : Task.FromResult(true);
        await Task.WhenAll(listTask, detailTask);
    }

    private Task SearchAsync() => List.조회Async();

    private Task ReloadListAsync() => List.새로고침Async();

    private Task ChangePageAsync(int page) => List.페이지조회Async(page);

    private Task HandleSearchKeyAsync(KeyboardEventArgs args)
        => string.Equals(args.Key, "Enter", StringComparison.Ordinal) ? SearchAsync() : Task.CompletedTask;

    private async Task SelectProductAsync(long productId, bool updateAddress = true)
    {
        if (updateAddress && ProductSelected.HasDelegate)
        {
            await ProductSelected.InvokeAsync(productId);
        }

        await Detail.조회Async(productId);
    }

    private async Task ClearSelectionAsync()
    {
        Detail.선택해제();
        if (ProductSelected.HasDelegate)
        {
            await ProductSelected.InvokeAsync(null);
        }
    }

    private static string ProjectionTime(DateTime value)
        => $"{value:yyyy.MM.dd HH:mm} UTC";

    private string OrderRequestHref(long productId)
        => $"{OrderRequestBasePath?.TrimEnd('/')}?productId={productId}";

    private static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string ErrorMessage(string? value)
        => string.IsNullOrWhiteSpace(value) ? "서버 응답을 확인할 수 없습니다." : value;
}
