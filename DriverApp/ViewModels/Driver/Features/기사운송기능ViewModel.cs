using DriverApp.Services;
using Hongdal.Contracts.Driver.Transport;

namespace DriverApp.ViewModels.Driver.Features;

public sealed class 기사운송기능ViewModel : 조립ViewModelBase
{
    public 기사운송기능ViewModel(IDriverTransportApiService api)
    {
        목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<기사운송요약응답>>(api.목록조회Async));
        현재조회 = 하위ViewModel등록(new Api작업ViewModel<기사운송요약응답?>(api.현재조회Async));
        상세조회 = 하위ViewModel등록(
            new Api작업ViewModel<long, 기사운송상세응답?>(api.상세조회Async));
        상차지도착 = 하위ViewModel등록(
            new Api작업ViewModel<long, 기사운송상태변경응답?>(api.상차지도착Async));
        상차완료 = 하위ViewModel등록(
            new Api작업ViewModel<기사운송상차완료조건, 기사운송상태변경응답?>(
                (condition, cancellationToken) => api.상차완료Async(
                    condition.운송Id,
                    condition.요청,
                    cancellationToken)));
        하차지도착 = 하위ViewModel등록(
            new Api작업ViewModel<long, 기사운송상태변경응답?>(api.하차지도착Async));
        하차완료 = 하위ViewModel등록(
            new Api작업ViewModel<기사운송하차완료조건, 기사운송상태변경응답?>(
                (condition, cancellationToken) => api.하차완료Async(
                    condition.운송Id,
                    condition.요청,
                    cancellationToken)));
        예외신고 = 하위ViewModel등록(
            new Api작업ViewModel<기사운송예외신고조건, 기사운송요약응답?>(
                (condition, cancellationToken) => api.예외신고Async(
                    condition.운송Id,
                    condition.요청,
                    cancellationToken)));
        문제신고 = 하위ViewModel등록(
            new Api작업ViewModel<기사운송예외신고조건, 기사운송요약응답?>(
                (condition, cancellationToken) => api.문제신고Async(
                    condition.운송Id,
                    condition.요청,
                    cancellationToken)));
    }

    public Api작업ViewModel<IReadOnlyList<기사운송요약응답>> 목록조회 { get; }
    public Api작업ViewModel<기사운송요약응답?> 현재조회 { get; }
    public Api작업ViewModel<long, 기사운송상세응답?> 상세조회 { get; }
    public Api작업ViewModel<long, 기사운송상태변경응답?> 상차지도착 { get; }
    public Api작업ViewModel<기사운송상차완료조건, 기사운송상태변경응답?> 상차완료 { get; }
    public Api작업ViewModel<long, 기사운송상태변경응답?> 하차지도착 { get; }
    public Api작업ViewModel<기사운송하차완료조건, 기사운송상태변경응답?> 하차완료 { get; }
    public Api작업ViewModel<기사운송예외신고조건, 기사운송요약응답?> 문제신고 { get; }
    public Api작업ViewModel<기사운송예외신고조건, 기사운송요약응답?> 예외신고 { get; }
}

public sealed record 기사운송상차완료조건(long 운송Id, 기사운송상차완료요청 요청);
public sealed record 기사운송하차완료조건(long 운송Id, 기사운송하차완료요청 요청);
public sealed record 기사운송예외신고조건(long 운송Id, 기사운송문제신고요청 요청);
