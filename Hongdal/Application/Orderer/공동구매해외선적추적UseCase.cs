using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Http;

namespace Hongdal.Application.Orderer;

public interface I공동구매해외선적추적UseCase
{
    Task<Result<IReadOnlyList<공동구매해외선적추적Dto>>> 목록Async(
        공동구매해외선적추적조회조건 request,
        CancellationToken cancellationToken);

    Task<Result<공동구매해외선적공개Dto>> 공개조회Async(string documentManagementNumber, CancellationToken cancellationToken);
    Task<Result<공동구매해외선적추적Dto>> 관리자조회Async(string documentManagementNumber, CancellationToken cancellationToken);
    Result<IReadOnlyList<수입물류참조항목>> 수입물류참조검색(수입물류참조조회요청 request);
    Result<수입물류정규화시뮬레이션결과> 수입물류정규화시뮬레이션(수입물류정규화시뮬레이션요청 request);

    Task<Result<수입물류정규화시뮬레이션결과>> 원장기반정규화시뮬레이션Async(
        string documentManagementNumber,
        string? customsOfficeCode,
        string? customsOfficeName,
        string? bondedAreaCode,
        string? bondedAreaName,
        CancellationToken cancellationToken);

    Task<Result<공동구매해외선적추적Dto>> 저장Async(공동구매해외선적추적저장요청 request, string actorUserId, CancellationToken cancellationToken);
    Task<Result<공동구매해외선적추적Dto>> 이벤트추가Async(string documentManagementNumber, 공동구매해외선적추적이벤트추가요청 request, string actorUserId, CancellationToken cancellationToken);
    Task<Result<공동구매해외선적통관동기화결과>> 통관동기화Async(공동구매해외선적통관동기화요청 request, string actorUserId, CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[HongdalUseCase("공동구매 해외 선적 추적", Summary = "문서관리번호와 BL 기반으로 공동구매 수입 화물 위치, 통관, 보세구역 정규화 정보를 조회하고 원장을 관리합니다.")]
[HongdalUseCaseActor(HongdalActor.Orderer)]
[HongdalUseCaseActor(HongdalActor.OverseasSellerOrForwarder, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Include,
    "공공데이터조회UseCase",
    Condition = "항구, 공항, 보세구역, 관할 세관 참조 데이터를 정규화하는 경우",
    Summary = "해외 선적 추적은 수입 물류 위치와 공공 참조 데이터 조회를 포함합니다.")]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "HS코드운영UseCase",
    Condition = "HS 코드 위험 태그, 업무 분류, 관세사 보정 판단이 필요한 경우",
    Summary = "선적·통관 추적을 HS 코드 운영과 관세사 보정 흐름으로 확장합니다.")]
public sealed class 공동구매해외선적추적UseCase : I공동구매해외선적추적UseCase
{
    private readonly I공동구매해외선적추적저장소 _store;
    private readonly I공동구매수입물류정규화Service _normalizationService;
    private readonly I공동구매해외선적통관동기화Service _customsSyncService;

    public 공동구매해외선적추적UseCase(
        I공동구매해외선적추적저장소 store,
        I공동구매수입물류정규화Service normalizationService,
        I공동구매해외선적통관동기화Service customsSyncService)
    {
        _store = store;
        _normalizationService = normalizationService;
        _customsSyncService = customsSyncService;
    }

    public async Task<Result<IReadOnlyList<공동구매해외선적추적Dto>>> 목록Async(
        공동구매해외선적추적조회조건 request,
        CancellationToken cancellationToken)
    {
        return Result.Ok(await _store.ListAsync(request, cancellationToken));
    }

    public async Task<Result<공동구매해외선적공개Dto>> 공개조회Async(string documentManagementNumber, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.GetBy문서관리번호Async(documentManagementNumber, cancellationToken);
            return item is null
                ? NotFound<공동구매해외선적공개Dto>("문서관리번호에 해당하는 공동주문 해외 선적 정보를 찾을 수 없습니다.")
                : Result.Ok(공동구매해외선적추적Projection.ToPublicDto(item));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest<공동구매해외선적공개Dto>(ex.Message);
        }
    }

    public async Task<Result<공동구매해외선적추적Dto>> 관리자조회Async(string documentManagementNumber, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.GetBy문서관리번호Async(documentManagementNumber, cancellationToken);
            return item is null
                ? NotFound<공동구매해외선적추적Dto>("공동주문 해외 선적 추적 원장을 찾을 수 없습니다.")
                : Result.Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest<공동구매해외선적추적Dto>(ex.Message);
        }
    }

    public Result<IReadOnlyList<수입물류참조항목>> 수입물류참조검색(수입물류참조조회요청 request)
    {
        return Result.Ok(_normalizationService.SearchReferences(request));
    }

    public Result<수입물류정규화시뮬레이션결과> 수입물류정규화시뮬레이션(수입물류정규화시뮬레이션요청 request)
    {
        return Result.Ok(_normalizationService.Simulate(request));
    }

    public async Task<Result<수입물류정규화시뮬레이션결과>> 원장기반정규화시뮬레이션Async(
        string documentManagementNumber,
        string? customsOfficeCode,
        string? customsOfficeName,
        string? bondedAreaCode,
        string? bondedAreaName,
        CancellationToken cancellationToken)
    {
        공동구매해외선적추적Dto? item;
        try
        {
            item = await _store.GetBy문서관리번호Async(documentManagementNumber, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest<수입물류정규화시뮬레이션결과>(ex.Message);
        }

        if (item is null)
        {
            return NotFound<수입물류정규화시뮬레이션결과>("Document management number was not found in the group purchase overseas shipment ledger.");
        }

        var currentLocation = item.이벤트목록.LastOrDefault()?.위치요약 ?? item.현재위치요약;
        var result = _normalizationService.Simulate(new 수입물류정규화시뮬레이션요청
        {
            문서관리번호 = item.문서관리번호,
            운송문서유형 = item.운송문서유형,
            운송문서번호 = item.운송문서번호,
            운송수단 = item.운송수단,
            출발국가코드 = item.출발국가코드,
            출발항코드 = item.출발항코드,
            도착항코드 = item.도착항코드,
            도착항만공항명 = item.현재위치요약,
            세관코드 = customsOfficeCode ?? string.Empty,
            세관명 = customsOfficeName ?? string.Empty,
            보세구역코드 = bondedAreaCode ?? string.Empty,
            보세구역명 = bondedAreaName ?? string.Empty,
            현재위치요약 = currentLocation,
            통관단계명 = item.현재상태코드
        });

        return Result.Ok(result);
    }

    public async Task<Result<공동구매해외선적추적Dto>> 저장Async(
        공동구매해외선적추적저장요청 request,
        string actorUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Result.Ok(await _store.UpsertAsync(request, actorUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest<공동구매해외선적추적Dto>(ex.Message);
        }
    }

    public async Task<Result<공동구매해외선적추적Dto>> 이벤트추가Async(
        string documentManagementNumber,
        공동구매해외선적추적이벤트추가요청 request,
        string actorUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.AppendEventAsync(documentManagementNumber, request, actorUserId, cancellationToken);
            return item is null
                ? NotFound<공동구매해외선적추적Dto>("공동주문 해외 선적 추적 원장을 찾을 수 없습니다.")
                : Result.Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest<공동구매해외선적추적Dto>(ex.Message);
        }
    }

    public async Task<Result<공동구매해외선적통관동기화결과>> 통관동기화Async(
        공동구매해외선적통관동기화요청 request,
        string actorUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _customsSyncService.SyncAsync(request, actorUserId, cancellationToken);
            return result.선적 is null && !result.동기화됨
                ? NotFound<공동구매해외선적통관동기화결과>(result.메시지)
                : Result.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest<공동구매해외선적통관동기화결과>(ex.Message);
        }
    }

    private static Result<T> NotFound<T>(string message)
    {
        return Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));
    }

    private static Result<T> BadRequest<T>(string message)
    {
        return Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status400BadRequest));
    }

}
