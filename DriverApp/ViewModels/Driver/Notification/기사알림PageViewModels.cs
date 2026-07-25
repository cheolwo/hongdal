using CommunityToolkit.Mvvm.ComponentModel;
using DriverApp.Services;
using DriverApp.ViewModels.Driver.Features;
using Ssalddel.Contracts.Driver.Notification;

namespace DriverApp.ViewModels.Driver.Notification;

public abstract partial class 기사알림PageViewModelBase : 기사PageViewModelBase
{
    private readonly IDriverNotificationApiService _notificationApi;

    protected 기사알림PageViewModelBase(
        기사알림기능ViewModel 알림기능,
        IDriverNotificationApiService notificationApi)
    {
        this.알림기능 = 하위ViewModel등록(알림기능);
        _notificationApi = notificationApi;
    }

    public 기사알림기능ViewModel 알림기능 { get; }

    [ObservableProperty]
    public partial IReadOnlyList<기사알림함항목응답> 알림목록 { get; private set; } = [];

    public int 안읽은알림수 => 알림목록.Count(item => !item.읽음);

    protected override async Task 불러오기Async(bool 새로고침, CancellationToken cancellationToken)
    {
        var response = await _notificationApi.알림함조회Async(cancellationToken)
            ?? throw new InvalidOperationException("기사 알림함 응답이 비어 있습니다.");
        알림목록 = response.Items;
        OnPropertyChanged(nameof(안읽은알림수));
    }

    public async Task<bool> 읽음처리Async(
        long notificationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationApi.읽음처리Async(notificationId, cancellationToken);
        if (result is null)
        {
            return false;
        }

        return await 새로고침Async(cancellationToken);
    }
}

public sealed class 기사알림함PageViewModel(
    기사알림기능ViewModel 알림기능,
    IDriverNotificationApiService notificationApi)
    : 기사알림PageViewModelBase(알림기능, notificationApi);

public sealed class 기사푸시설정PageViewModel(
    기사알림기능ViewModel 알림기능,
    IDriverNotificationApiService notificationApi)
    : 기사알림PageViewModelBase(알림기능, notificationApi);
