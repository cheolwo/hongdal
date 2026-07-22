using Microsoft.AspNetCore.Components;
using MudBlazor;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Sales;

public enum ShipperSalesOrderWorkspaceMode
{
    List,
    Detail,
    Combined
}

public partial class ShipperSalesOrderWorkspace
{
    private bool _initialized;
    private bool _initialListStateApplied;

    [Parameter]
    public long? OrderId { get; set; }

    [Parameter]
    public ShipperSalesOrderWorkspaceMode Mode { get; set; } = ShipperSalesOrderWorkspaceMode.List;

    [Parameter]
    public string? InitialSearch { get; set; }

    [Parameter]
    public string? InitialSyncScope { get; set; }

    [Parameter]
    public string? InitialStatus { get; set; }

    [Parameter]
    public int InitialPage { get; set; } = 1;

    private 판매채널페이지접근ViewModel Access => ViewModel.접근;
    private 판매채널주문목록PageViewModel List => ViewModel.목록;
    private 판매채널주문상세PageViewModel Detail => ViewModel.상세;

    private bool ShowList => Mode is ShipperSalesOrderWorkspaceMode.List or ShipperSalesOrderWorkspaceMode.Combined;
    private bool ShowDetail => Mode is ShipperSalesOrderWorkspaceMode.Detail or ShipperSalesOrderWorkspaceMode.Combined;
    private string WorkspaceLayoutClass => Mode == ShipperSalesOrderWorkspaceMode.Combined
        ? "sales-order-layout"
        : "sales-order-screen";

    private int CurrentPageLineCount => List.주문목록.Sum(item => item.출고라인수);
    private int CurrentPageWarehouseCount => List.주문목록.Sum(item => item.출고창고수);
    private int CurrentPageTransportCount => List.주문목록.Count(item => item.운송인계여부);

    protected override async Task OnInitializedAsync()
    {
        ApplyInitialListState();
        await CheckAccessAndLoadAsync();
        _initialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_initialized || !Access.사용가능)
        {
            return;
        }

        if (ShowDetail && OrderId is long orderId)
        {
            if (Detail.요청OrderId != orderId)
            {
                await Detail.조회Async(orderId);
            }
        }
        else if (ShowDetail && Detail.요청OrderId.HasValue)
        {
            Detail.선택해제();
        }
    }

    private async Task CheckAccessAndLoadAsync()
    {
        if (!await Access.확인Async() || !Access.사용가능)
        {
            return;
        }

        var listTask = ShowList
            ? List.초기화됨
                ? Task.FromResult(true)
                : List.페이지조회Async(Math.Max(1, InitialPage))
            : Task.FromResult(true);
        var detailTask = ShowDetail && OrderId is long orderId
            ? Detail.조회Async(orderId)
            : Task.FromResult(true);
        await Task.WhenAll(listTask, detailTask);
    }

    private Task RetryAccessAsync() => CheckAccessAndLoadAsync();

    private Task SearchAsync() => List.조회Async();

    private Task ReloadListAsync() => List.페이지조회Async(List.현재페이지);

    private Task ChangePageAsync(int page) => List.페이지조회Async(page);

    private async Task ClearFiltersAsync()
    {
        List.필터초기화();
        await List.조회Async();
    }

    private Task SelectOrderAsync(long orderId)
        => Detail.조회Async(orderId);

    private void ApplyInitialListState()
    {
        if (_initialListStateApplied || !ShowList)
        {
            return;
        }

        List.검색어 = InitialSearch ?? string.Empty;
        List.국내외구분 = InitialSyncScope;
        List.출고상태 = InitialStatus;
        _initialListStateApplied = true;
    }

    private string DetailPath(long orderId)
    {
        var listPath = new SalesOrderNavigationContext()
            .WithListState(List.검색어, List.국내외구분, List.출고상태, List.현재페이지)
            .PathFor(SalesOrderScreenKind.List);

        return new SalesOrderNavigationContext { From = listPath }
            .PathFor(SalesOrderScreenKind.Detail, orderId);
    }

    private static string ChannelName(string? channelType)
        => channelType?.Trim() switch
        {
            CommerceChannelKeys.SmartStore => "네이버 스마트스토어",
            CommerceChannelKeys.Coupang => "쿠팡 Wing",
            CommerceChannelKeys.ElevenStreet => "11번가",
            CommerceChannelKeys.Shopify => "Shopify",
            CommerceChannelKeys.Amazon => "Amazon",
            CommerceChannelKeys.Ebay => "eBay",
            CommerceChannelKeys.Walmart => "Walmart",
            CommerceChannelKeys.Etsy => "Etsy",
            CommerceChannelKeys.TikTokShop => "TikTok Shop",
            CommerceChannelKeys.Shopee => "Shopee",
            CommerceChannelKeys.Lazada => "Lazada",
            _ => string.IsNullOrWhiteSpace(channelType) ? "채널 확인 필요" : channelType.Trim()
        };

    private static string ScopeName(string? scope)
        => string.Equals(scope, CommerceChannelOrderSyncScopes.Domestic, StringComparison.OrdinalIgnoreCase)
            ? "국내 채널"
            : string.Equals(scope, CommerceChannelOrderSyncScopes.Overseas, StringComparison.OrdinalIgnoreCase)
                ? "해외 채널"
                : "구분 확인 필요";

    private static Color StatusColor(string? status)
        => status?.Trim() switch
        {
            "출고완료" => Color.Success,
            "출고준비중" => Color.Info,
            "출고취소" => Color.Error,
            "출고예정" => Color.Warning,
            _ => Color.Default
        };

    private static string WarehouseName(판매채널주문출고라인응답 line)
        => string.IsNullOrWhiteSpace(line.출고창고명)
            ? $"창고 #{line.출고창고Id}"
            : line.출고창고명;

    private static string NullableIdLabel(long? value) => value?.ToString() ?? "—";

    private static string DateTimeLabel(DateTime? value)
        => value is null || value.Value == default
            ? "—"
            : value.Value.ToLocalTime().ToString("yyyy.MM.dd HH:mm");

    private static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string AccessErrorMessage(Api작업오류? error, string? fallback)
        => error?.Http상태코드 switch
        {
            401 => "로그인 세션이 만료되었습니다. 다시 로그인해 주세요.",
            403 => "화주 또는 판매자 역할이 있는 계정으로 이용해 주세요.",
            _ => string.IsNullOrWhiteSpace(fallback) ? "서버 응답을 확인할 수 없습니다." : fallback
        };
}
