using Hongdal.Contracts.Shipper.Request;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using HongdalApp.Models.Shipper;
using HongdalApp.Services;

namespace HongdalApp.ViewModels.Shipper;

public sealed class 화주운송의뢰기능ViewModel : 조립ViewModelBase
{
    public 화주운송의뢰기능ViewModel(
        IShipperOperationsService operations,
        IShipperBulkRequestService bulkRequests)
    {
        의뢰목록조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<ShipperRequestItem>>(operations.GetRequestsAsync));
        의뢰상세조회 = 하위ViewModel등록(
            new Api작업ViewModel<string, ShipperRequestItem?>(operations.GetRequestAsync));
        공개화물조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<공개화물요약응답>>(operations.GetPublicCargoAsync));
        차량종류조회 = 하위ViewModel등록(
            new Api작업ViewModel<IReadOnlyList<string>>(operations.GetVehicleTypesAsync));
        운임예상 = 하위ViewModel등록(
            new Api작업ViewModel<화주운임예상조건, decimal>(
                (condition, cancellationToken) => operations.EstimateFareAsync(
                    condition.차량종류,
                    condition.거리Km,
                    cancellationToken)));
        의뢰등록 = 하위ViewModel등록(
            new Api작업ViewModel<ShipperRequestItem, Api작업완료>(async (request, cancellationToken) =>
            {
                await operations.AddRequestAsync(request, cancellationToken);
                return Api작업완료.값;
            }));
        일괄미리보기 = 하위ViewModel등록(
            new Api작업ViewModel<화주일괄파일조건, 화주운송의뢰일괄미리보기응답?>(
                (condition, cancellationToken) => bulkRequests.미리보기Async(
                    condition.스트림,
                    condition.파일명,
                    cancellationToken)));
        일괄등록 = 하위ViewModel등록(
            new Api작업ViewModel<화주운송의뢰일괄확정등록요청, 화주운송의뢰일괄등록결과응답?>(
                bulkRequests.등록Async));
    }

    public Api작업ViewModel<IReadOnlyList<ShipperRequestItem>> 의뢰목록조회 { get; }
    public Api작업ViewModel<string, ShipperRequestItem?> 의뢰상세조회 { get; }
    public Api작업ViewModel<IReadOnlyList<공개화물요약응답>> 공개화물조회 { get; }
    public Api작업ViewModel<IReadOnlyList<string>> 차량종류조회 { get; }
    public Api작업ViewModel<화주운임예상조건, decimal> 운임예상 { get; }
    public Api작업ViewModel<ShipperRequestItem, Api작업완료> 의뢰등록 { get; }
    public Api작업ViewModel<화주일괄파일조건, 화주운송의뢰일괄미리보기응답?> 일괄미리보기 { get; }
    public Api작업ViewModel<화주운송의뢰일괄확정등록요청, 화주운송의뢰일괄등록결과응답?> 일괄등록 { get; }
}

public sealed record 화주운임예상조건(string 차량종류, decimal 거리Km);

/// <remarks>스트림의 수명은 이 요청을 만든 페이지가 관리합니다.</remarks>
public sealed record 화주일괄파일조건(Stream 스트림, string 파일명);
