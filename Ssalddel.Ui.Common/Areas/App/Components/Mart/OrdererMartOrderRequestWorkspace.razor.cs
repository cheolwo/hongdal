using Microsoft.AspNetCore.Components;
using MudBlazor;
using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.Models.Auth;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Mart;

public partial class OrdererMartOrderRequestWorkspace
{
    private bool _initialized;

    [Parameter]
    public long? ProductId { get; set; }

    [Parameter]
    public Guid? RequestId { get; set; }

    [Parameter]
    public string CatalogHref { get; set; } = "/food/mart";

    [Parameter]
    public EventCallback<Guid?> RequestSelected { get; set; }

    private 마트페이지접근ViewModel Access => ViewModel.접근;
    private 주문자앱인증ViewModel Authentication => ViewModel.인증;
    private 마트공개상품상세ViewModel Product => ViewModel.상품;
    private 마트주문작성ViewModel Writer => ViewModel.작성;
    private 마트주문요청상세ViewModel RequestDetail => ViewModel.요청상세;

    private Severity AuthenticationSeverity
        => Authentication.오류발생 ? Severity.Error : Severity.Info;

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

        var tasks = new List<Task>();
        if (ProductId is long productId && Product.요청ProductId != productId)
        {
            Writer.새요청준비();
            tasks.Add(Product.조회Async(productId));
        }
        else if (ProductId is null && Product.요청ProductId.HasValue)
        {
            Product.선택해제();
        }

        if (Authentication.로그인됨 && RequestId is Guid requestId && RequestDetail.요청Id != requestId)
        {
            tasks.Add(RequestDetail.조회Async(requestId));
        }
        else if (RequestId is null && RequestDetail.요청Id.HasValue)
        {
            RequestDetail.선택해제();
        }

        await Task.WhenAll(tasks);
    }

    private async Task InitializeAsync()
    {
        if (!await Access.확인Async() || !Access.사용가능)
        {
            return;
        }

        var productTask = ProductId is long productId
            ? Product.조회Async(productId)
            : Task.FromResult(true);
        var authenticationTask = Authentication.초기화됨
            ? Task.FromResult(true)
            : Authentication.복원Async();
        await Task.WhenAll(productTask, authenticationTask);

        if (Authentication.로그인됨 && RequestId is Guid requestId)
        {
            await RequestDetail.조회Async(requestId);
        }
    }

    private async Task LoginAsync(공통로그인요청 request)
    {
        if (await Authentication.로그인Async(request.UserNameOrEmail, request.Password)
            && RequestId is Guid requestId)
        {
            await RequestDetail.조회Async(requestId);
        }
    }

    private async Task LogoutAsync()
    {
        await Authentication.로그아웃Async();
        RequestDetail.선택해제();
        Writer.새요청준비();
    }

    private async Task SubmitAsync()
    {
        if (Product.상세 is not { 판매가능여부: true } product
            || !await Writer.등록Async(product.Id)
            || Writer.등록응답 is not { } response)
        {
            return;
        }

        if (RequestSelected.HasDelegate)
        {
            await RequestSelected.InvokeAsync(response.주문요청Id);
        }

        await RequestDetail.조회Async(response.주문요청Id);
    }

    private decimal EstimatedTotal(마트공개상품상세응답 product)
        => product.판매가 * Math.Clamp(Writer.수량, 0, 100);

    private static string ShortId(Guid value)
        => value.ToString("N")[..12].ToUpperInvariant();

    private static string FormatDate(DateTime value)
        => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime().ToString("yyyy.MM.dd HH:mm");

    private static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string ErrorMessage(string? value)
        => string.IsNullOrWhiteSpace(value) ? "서버 응답을 확인할 수 없습니다." : value;
}
