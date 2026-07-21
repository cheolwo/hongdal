using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Mart;

public partial class OrdererMartCatalogWorkspace
{
    private bool _initialized;
    private bool _reviewFormVisible;

    [Parameter]
    public long? ProductId { get; set; }

    [Parameter]
    public EventCallback<long?> ProductSelected { get; set; }

    [Parameter]
    public string? OrderRequestBasePath { get; set; }

    [Parameter]
    public string? SalesPageComposerPath { get; set; }

    [Parameter]
    public string? CommunityPostBasePath { get; set; }

    [Parameter]
    public string LoginPath { get; set; } = "/login";

    private 마트페이지접근ViewModel Access => ViewModel.접근;
    private 마트공개상품목록ViewModel List => ViewModel.목록;
    private 마트공개상품상세ViewModel Detail => ViewModel.상세;
    private 마트공개상품후기작성ViewModel Review => ViewModel.후기;

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
                PrepareReview();
            }
        }
        else if (Detail.요청ProductId.HasValue)
        {
            Detail.선택해제();
            Review.선택해제();
            _reviewFormVisible = false;
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
        PrepareReview();
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
        PrepareReview();
    }

    private async Task ClearSelectionAsync()
    {
        Detail.선택해제();
        Review.선택해제();
        _reviewFormVisible = false;
        if (ProductSelected.HasDelegate)
        {
            await ProductSelected.InvokeAsync(null);
        }
    }

    private static string ProjectionTime(DateTime value)
        => $"{value:yyyy.MM.dd HH:mm} UTC";

    private string OrderRequestHref(long productId)
        => $"{OrderRequestBasePath?.TrimEnd('/')}?productId={productId}";

    private string SalesPageHref(마트공개상품상세응답 detail)
        => new 판매페이지공개상품Seed(
                detail.Id,
                detail.상품명,
                detail.카테고리,
                detail.설명,
                detail.판매단위,
                detail.판매가,
                detail.구매근거.완료원장확인여부,
                detail.구매근거.공개후기수,
                detail.구매근거.근거기준시각Utc ?? detail.수정일시Utc)
            .ToNavigationUri(SalesPageComposerPath!);

    private string CommunityPostHref(long postId)
        => $"{CommunityPostBasePath?.TrimEnd('/')}/{postId}";

    private void PrepareReview()
    {
        if (Detail.상세 is { } detail)
        {
            Review.준비(detail.Id, detail.상품명);
            return;
        }

        Review.선택해제();
        _reviewFormVisible = false;
    }

    private void OpenReviewForm()
    {
        Review.작업상태초기화();
        _reviewFormVisible = true;
    }

    private void CloseReviewForm()
    {
        if (!Review.처리중)
        {
            _reviewFormVisible = false;
            Review.작업상태초기화();
        }
    }

    private async Task SubmitReviewAsync()
    {
        if (!await Review.작성Async() || Detail.상세 is not { } detail)
        {
            return;
        }

        _reviewFormVisible = false;
        await Detail.조회Async(detail.Id);
    }

    private static string EvidenceTime(DateTime? value)
        => value.HasValue ? $"{value.Value:yyyy.MM.dd HH:mm} UTC" : "기준 시각 없음";

    private static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string ErrorMessage(string? value)
        => string.IsNullOrWhiteSpace(value) ? "서버 응답을 확인할 수 없습니다." : value;
}
