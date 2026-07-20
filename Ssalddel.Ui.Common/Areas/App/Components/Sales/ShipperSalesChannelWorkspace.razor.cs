using Microsoft.AspNetCore.Components;
using MudBlazor;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Sales;

public partial class ShipperSalesChannelWorkspace
{
    private bool _initialized;
    private bool _showPreparation;

    [Parameter]
    public long? AccountId { get; set; }

    [Parameter]
    public EventCallback<long> AccountSelected { get; set; }

    private 판매채널페이지접근ViewModel Access => ViewModel.접근;
    private 판매채널계정목록PageViewModel List => ViewModel.목록;
    private 판매채널계정상세PageViewModel Detail => ViewModel.상세;
    private 판매채널계정연결준비ViewModel Preparation => ViewModel.연결준비;

    protected override async Task OnInitializedAsync()
    {
        await CheckAccessAndLoadAsync();
        _initialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_initialized || !Access.사용가능)
        {
            return;
        }

        if (AccountId is long accountId)
        {
            if (Detail.요청AccountId != accountId)
            {
                await Detail.조회Async(accountId);
            }
        }
        else if (Detail.요청AccountId.HasValue)
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

        var listTask = List.초기화됨
            ? Task.FromResult(true)
            : List.조회Async();
        var detailTask = AccountId is long accountId
            ? Detail.조회Async(accountId)
            : Task.FromResult(true);
        await Task.WhenAll(listTask, detailTask);
    }

    private Task RetryAccessAsync() => CheckAccessAndLoadAsync();

    private Task ReloadListAsync() => List.조회Async();

    private void TogglePreparation()
    {
        _showPreparation = !_showPreparation;
        if (!_showPreparation)
        {
            Preparation.결과초기화();
        }
    }

    private void OpenPreparation()
    {
        _showPreparation = true;
        Preparation.결과초기화();
    }

    private void ClearFilters()
    {
        List.검색어 = string.Empty;
        List.채널종류 = null;
    }

    private async Task CreatePreparationAsync()
    {
        if (!await Preparation.등록Async() || Preparation.등록된계정 is null)
        {
            return;
        }

        var accountId = Preparation.등록된계정.Id;
        await List.조회Async();
        await SelectAccountAsync(accountId);
    }

    private async Task SelectAccountAsync(long accountId, bool updateAddress = true)
    {
        if (updateAddress && AccountSelected.HasDelegate)
        {
            await AccountSelected.InvokeAsync(accountId);
        }

        await Detail.조회Async(accountId);
    }

    private static string ChannelName(string? channelCode)
        => 판매채널연결옵션Catalog.찾기(channelCode)?.Name
           ?? (string.IsNullOrWhiteSpace(channelCode) ? "채널 확인 필요" : channelCode.Trim());

    private static Color StatusColor(string? status)
        => status?.Trim().ToLowerInvariant() switch
        {
            "연결" or "connected" or "active" => Color.Success,
            "오류" or "error" or "failed" => Color.Error,
            "준비" or "pending" or "ready" => Color.Warning,
            _ => Color.Default
        };

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
