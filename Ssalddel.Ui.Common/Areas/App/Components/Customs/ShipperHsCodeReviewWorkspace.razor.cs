using Microsoft.AspNetCore.Components;
using MudBlazor;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Customs;

public partial class ShipperHsCodeReviewWorkspace
{
    private bool _initialized;

    [Parameter]
    public long? ReviewId { get; set; }

    [Parameter]
    public EventCallback<long> ReviewSelected { get; set; }

    private 화주HS코드검토접근ViewModel Access => ViewModel.접근;
    private 화주HS코드검토목록ViewModel List => ViewModel.목록;
    private 화주HS코드검토상세ViewModel Detail => ViewModel.상세;

    private string ListErrorMessage => AccessErrorMessage(List.오류, List.오류메시지);
    private string DetailErrorMessage => AccessErrorMessage(Detail.오류, Detail.오류메시지);

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

        if (ReviewId is long reviewId)
        {
            if (Detail.요청ReviewId != reviewId)
            {
                await Detail.조회Async(reviewId);
            }
        }
        else if (Detail.요청ReviewId.HasValue)
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
        var detailTask = ReviewId is long reviewId
            ? Detail.조회Async(reviewId)
            : Task.FromResult(true);
        await Task.WhenAll(listTask, detailTask);
    }

    private Task RetryAccessAsync() => CheckAccessAndLoadAsync();

    private Task SearchAsync() => List.조회Async();

    private Task ReloadListAsync() => List.페이지조회Async(List.현재페이지);

    private Task ChangePageAsync(int page) => List.페이지조회Async(page);

    private async Task SelectReviewAsync(long reviewId, bool updateAddress = true)
    {
        if (updateAddress && ReviewSelected.HasDelegate)
        {
            await ReviewSelected.InvokeAsync(reviewId);
        }

        await Detail.조회Async(reviewId);
    }

    private static string PrimaryName(화주HS코드검토항목응답 item)
        => !string.IsNullOrWhiteSpace(item.KoreanName)
            ? item.KoreanName
            : !string.IsNullOrWhiteSpace(item.EnglishName)
                ? item.EnglishName
                : "품명 확인 필요";

    private static Color RiskColor(string? riskLevelCode)
        => riskLevelCode?.Trim().ToLowerInvariant() switch
        {
            "high" => Color.Error,
            "review" => Color.Warning,
            "low" => Color.Success,
            _ => Color.Default
        };

    private static Color AgencyColor(string? agencyTypeLabel)
        => agencyTypeLabel?.Contains("통관", StringComparison.Ordinal) == true
            ? Color.Primary
            : agencyTypeLabel?.Contains("수입", StringComparison.Ordinal) == true
                ? Color.Info
                : Color.Default;

    private static string SourceShortLabel(화주HS코드검토출처응답 source)
    {
        var standard = SourceStandardLabel(source);
        return string.IsNullOrWhiteSpace(source.Revision)
            ? standard
            : $"{standard} · {source.Revision}";
    }

    private static string SourceStandardLabel(화주HS코드검토출처응답 source)
    {
        var parts = new[] { source.StandardCode, source.CountryCode }
            .Where(value => !string.IsNullOrWhiteSpace(value));
        var label = string.Join(" · ", parts);
        return string.IsNullOrWhiteSpace(label) ? "출처 확인 필요" : label;
    }

    private static string DateLabel(DateTime? value)
        => value?.ToLocalTime().ToString("yyyy.MM.dd") ?? "—";

    private static string DateTimeLabel(DateTime? value)
        => value?.ToLocalTime().ToString("yyyy.MM.dd HH:mm") ?? "—";

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
