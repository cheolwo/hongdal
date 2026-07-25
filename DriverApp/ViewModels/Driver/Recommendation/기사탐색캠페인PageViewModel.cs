using CommunityToolkit.Mvvm.ComponentModel;
using DriverApp.ViewModels.Driver.Features;
using Ssalddel.Contracts.Common.Exploration;

namespace DriverApp.ViewModels.Driver.Recommendation;

/// <summary>
/// 보낸 탐색 문의함의 목록 선택과 상세·추천 대상 재조회를 조정합니다.
/// 데이터는 기사 탐색 캠페인 API에서만 읽으며 암묵적인 샘플 fallback을 사용하지 않습니다.
/// </summary>
public sealed partial class 기사탐색캠페인PageViewModel : 기사PageViewModelBase
{
    public 기사탐색캠페인PageViewModel(기사탐색캠페인기능ViewModel 탐색기능)
    {
        this.탐색기능 = 하위ViewModel등록(탐색기능);
    }

    public 기사탐색캠페인기능ViewModel 탐색기능 { get; }

    [ObservableProperty]
    public partial IReadOnlyList<탐색캠페인목록항목응답> 캠페인목록 { get; private set; } = [];

    [ObservableProperty]
    public partial 탐색캠페인목록항목응답? 선택캠페인 { get; private set; }

    [ObservableProperty]
    public partial 탐색캠페인상세응답? 선택상세 { get; private set; }

    [ObservableProperty]
    public partial IReadOnlyList<탐색캠페인추천대상응답> 추천대상목록 { get; private set; } = [];

    public bool 선택불러오는중
        => 탐색기능.상세조회.처리중 || 탐색기능.추천대상조회.처리중;

    protected override async Task 불러오기Async(bool 새로고침, CancellationToken cancellationToken)
    {
        await 탐색기능.목록조회.실행Async(cancellationToken);
        오류확인(탐색기능.목록조회, "탐색 캠페인 목록을 불러오지 못했습니다.");

        캠페인목록 = 탐색기능.목록조회.결과 ?? [];
        var next = 선택캠페인 is null
            ? 캠페인목록.FirstOrDefault()
            : 캠페인목록.FirstOrDefault(item => item.Id == 선택캠페인.Id)
              ?? 캠페인목록.FirstOrDefault();

        await 선택Async(next, cancellationToken);
    }

    public async Task 선택Async(
        탐색캠페인목록항목응답? campaign,
        CancellationToken cancellationToken = default)
    {
        선택캠페인 = campaign;
        선택상세 = null;
        추천대상목록 = [];
        if (campaign is null)
        {
            return;
        }

        await Task.WhenAll(
            탐색기능.상세조회.실행Async(campaign.Id, cancellationToken),
            탐색기능.추천대상조회.실행Async(campaign.Id, cancellationToken));

        오류확인(탐색기능.상세조회, "탐색 캠페인 상세를 불러오지 못했습니다.");
        오류확인(탐색기능.추천대상조회, "탐색 캠페인 추천 대상을 불러오지 못했습니다.");
        선택상세 = 탐색기능.상세조회.결과;
        추천대상목록 = 탐색기능.추천대상조회.결과 ?? [];
        OnPropertyChanged(nameof(선택불러오는중));
    }

    protected override bool 하위ViewModel처리중
        => 탐색기능.목록조회.처리중 || 선택불러오는중;

    private static void 오류확인<T>(Api작업ViewModel<T> operation, string fallback)
    {
        if (operation.오류발생)
        {
            throw new InvalidOperationException(operation.오류메시지 ?? fallback);
        }
    }

    private static void 오류확인<TInput, T>(
        Api작업ViewModel<TInput, T> operation,
        string fallback)
    {
        if (operation.오류발생)
        {
            throw new InvalidOperationException(operation.오류메시지 ?? fallback);
        }
    }
}
