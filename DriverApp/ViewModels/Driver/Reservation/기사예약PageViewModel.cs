using CommunityToolkit.Mvvm.ComponentModel;
using DriverApp.ViewModels.Driver.Features;
using Ssalddel.Contracts.Driver.Reservation;

namespace DriverApp.ViewModels.Driver.Reservation;

public sealed partial class 기사예약PageViewModel : 기사PageViewModelBase
{
    public 기사예약PageViewModel(기사예약기능ViewModel 예약기능)
    {
        this.예약기능 = 하위ViewModel등록(예약기능);
    }

    public 기사예약기능ViewModel 예약기능 { get; }

    [ObservableProperty]
    public partial IReadOnlyList<기사예약목록응답> 예약목록 { get; private set; } = [];

    protected override async Task 불러오기Async(bool 새로고침, CancellationToken cancellationToken)
    {
        await 예약기능.목록조회.실행Async(cancellationToken);
        if (예약기능.목록조회.오류발생)
        {
            throw new InvalidOperationException(예약기능.목록조회.오류메시지 ?? "예약 목록을 불러오지 못했습니다.");
        }

        예약목록 = 예약기능.목록조회.결과 ?? [];
    }

    protected override bool 하위ViewModel처리중 => 예약기능.목록조회.처리중;
}
