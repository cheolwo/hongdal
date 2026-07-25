using CommunityToolkit.Mvvm.ComponentModel;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace SsalddelApp.ViewModels.Shipper;

public sealed partial class OrderFulfillmentServerInboxPageViewModel(
    I판매채널주문읽기Service readService,
    I판매채널주문동기화Client syncClient) : 화주PageViewModelBase
{
    [ObservableProperty]
    public partial IReadOnlyList<판매채널주문요약응답> 주문목록 { get; private set; } = [];

    [ObservableProperty]
    public partial 판매채널주문동기화응답? 최근동기화결과 { get; private set; }

    [ObservableProperty]
    public partial bool 동기화중 { get; private set; }

    [ObservableProperty]
    public partial string? 동기화오류 { get; private set; }

    public bool 주문없음 => 초기화됨 && 주문목록.Count == 0;
    protected override bool 하위ViewModel처리중 => 동기화중;

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
        => await 원장조회Async(cancellationToken);

    private async Task 원장조회Async(CancellationToken cancellationToken)
    {
        var response = await readService.목록조회Async(
            new 판매채널주문목록조회요청
            {
                Page = 0,
                PageSize = 50
            },
            cancellationToken);

        주문목록 = response.Items;
        OnPropertyChanged(nameof(주문없음));
    }

    public async Task<bool> 동기화Async(
        string syncScope,
        CancellationToken cancellationToken = default)
    {
        if (동기화중)
        {
            return false;
        }

        동기화중 = true;
        동기화오류 = null;
        try
        {
            최근동기화결과 = await syncClient.동기화Async(
                new 판매채널주문동기화요청 { SyncScope = syncScope },
                cancellationToken)
                ?? throw new InvalidOperationException("판매채널 주문 동기화 응답이 비어 있습니다.");

            await 원장조회Async(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            동기화오류 = ex.Message;
            return false;
        }
        finally
        {
            동기화중 = false;
        }
    }
}
