using Hongdal.Application.Admin.Inbound;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Orderer;
using MediatR;
using 홍달.도메인.공통;

namespace Hongdal.Services.Orderer;

public interface I공동구매커머스이행계획UseCase
{
    Task<공동구매처리결과<IReadOnlyList<공동구매커머스이행계획Dto>>> 목록조회Async(
        공동구매커머스이행계획조회조건 조건,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매커머스이행계획Dto>> 단건조회Async(
        string planId,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<IReadOnlyList<공동구매커머스이행계획Dto>>> 공동구매별목록조회Async(
        string groupPurchaseId,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매커머스이행계획Dto>> 저장Async(
        공동구매커머스이행계획저장요청 request,
        string 사용자Id,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매플랫폼국내운송초안결과>> 플랫폼국내운송초안생성Async(
        string planId,
        공동구매플랫폼국내운송초안요청 request,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매국내운송배차대기생성결과>> 플랫폼국내운송배차대기생성Async(
        string planId,
        공동구매플랫폼국내운송초안요청 request,
        CancellationToken cancellationToken = default);
}

[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[HongdalUseCase("공동구매 커머스 이행 계획", Summary = "공동수입 물품을 국내 운송, 3PL 입고, 판매채널 출고 후보로 연결합니다.")]
[HongdalUseCaseActor(HongdalActor.OrdererGroupLeader)]
[HongdalUseCaseActor(HongdalActor.Orderer, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseActor(HongdalActor.Driver, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseActor(HongdalActor.WarehouseManager, HongdalUseCaseActorRole.Supporting)]
public sealed class 공동구매커머스이행계획UseCase : I공동구매커머스이행계획UseCase
{
    private readonly I공동구매커머스이행계획저장소 _store;
    private readonly ISender _sender;

    public 공동구매커머스이행계획UseCase(
        I공동구매커머스이행계획저장소 store,
        ISender sender)
    {
        _store = store;
        _sender = sender;
    }

    public async Task<공동구매처리결과<IReadOnlyList<공동구매커머스이행계획Dto>>> 목록조회Async(
        공동구매커머스이행계획조회조건 조건,
        CancellationToken cancellationToken = default)
    {
        var items = await _store.ListAsync(조건, cancellationToken);
        return 공동구매처리결과<IReadOnlyList<공동구매커머스이행계획Dto>>.성공결과(items);
    }

    public async Task<공동구매처리결과<공동구매커머스이행계획Dto>> 단건조회Async(
        string planId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _store.GetAsync(planId, cancellationToken);
            return item is null
                ? 공동구매처리결과<공동구매커머스이행계획Dto>.찾을수없음("공동주문 커머스 풀필먼트 플랜을 찾을 수 없습니다.")
                : 공동구매처리결과<공동구매커머스이행계획Dto>.성공결과(item);
        }
        catch (InvalidOperationException ex)
        {
            return 공동구매처리결과<공동구매커머스이행계획Dto>.잘못된요청(ex.Message);
        }
    }

    public async Task<공동구매처리결과<IReadOnlyList<공동구매커머스이행계획Dto>>> 공동구매별목록조회Async(
        string groupPurchaseId,
        CancellationToken cancellationToken = default)
    {
        var items = await _store.ListAsync(new 공동구매커머스이행계획조회조건
        {
            공동구매Id = groupPurchaseId
        }, cancellationToken);

        return 공동구매처리결과<IReadOnlyList<공동구매커머스이행계획Dto>>.성공결과(items);
    }

    public async Task<공동구매처리결과<공동구매커머스이행계획Dto>> 저장Async(
        공동구매커머스이행계획저장요청 request,
        string 사용자Id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await _store.UpsertAsync(request, 사용자Id, cancellationToken);
            return 공동구매처리결과<공동구매커머스이행계획Dto>.성공결과(item);
        }
        catch (InvalidOperationException ex)
        {
            return 공동구매처리결과<공동구매커머스이행계획Dto>.잘못된요청(ex.Message);
        }
    }

    public async Task<공동구매처리결과<공동구매플랫폼국내운송초안결과>> 플랫폼국내운송초안생성Async(
        string planId,
        공동구매플랫폼국내운송초안요청 request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _store.GetAsync(planId, cancellationToken);
            if (plan is null)
            {
                return 공동구매처리결과<공동구매플랫폼국내운송초안결과>.찾을수없음("Group purchase commerce fulfillment plan was not found.");
            }

            var result = 공동구매플랫폼국내운송계획기.계획(plan, request);
            return 공동구매처리결과<공동구매플랫폼국내운송초안결과>.성공결과(result);
        }
        catch (InvalidOperationException ex)
        {
            return 공동구매처리결과<공동구매플랫폼국내운송초안결과>.잘못된요청(ex.Message);
        }
    }

    public async Task<공동구매처리결과<공동구매국내운송배차대기생성결과>> 플랫폼국내운송배차대기생성Async(
        string planId,
        공동구매플랫폼국내운송초안요청 request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _store.GetAsync(planId, cancellationToken);
            if (plan is null)
            {
                return 공동구매처리결과<공동구매국내운송배차대기생성결과>.찾을수없음("Group purchase commerce fulfillment plan was not found.");
            }

            var result = 공동구매플랫폼국내운송계획기.계획(plan, request);
            if (!result.ReadyForDispatchQueue)
            {
                return 공동구매처리결과<공동구매국내운송배차대기생성결과>.잘못된요청(
                    "Platform domestic transport draft requires confirmation before dispatch queue creation.",
                    new 공동구매국내운송배차대기생성결과 { 운송초안 = result });
            }

            var draft = result.DispatchQueueDraft;
            var entity = await _sender.Send(new 배차대기생성Command(
                draft.RequestId,
                draft.PlatformShipperUserId,
                draft.DispatchBusinessTypeCode,
                draft.SourceRequestType,
                draft.SourceRequestId,
                draft.PickupRoadAddress,
                draft.PickupDetailAddress,
                draft.PickupLatitude,
                draft.PickupLongitude,
                draft.DropoffRoadAddress,
                draft.DropoffDetailAddress,
                draft.DropoffLatitude,
                draft.DropoffLongitude,
                상태값.배차대기상태.대기,
                draft.DestinationTypeCode,
                draft.DriverPerformsApartmentUnitDistribution,
                draft.ApartmentUnitDistributionModeCode,
                draft.ApartmentUnitDeliveryCount,
                draft.DistributionResponsibilityCode), cancellationToken);

            return 공동구매처리결과<공동구매국내운송배차대기생성결과>.성공결과(new 공동구매국내운송배차대기생성결과
            {
                운송초안 = result,
                배차대기Id = entity.Id,
                의뢰Id = entity.의뢰Id,
                원본의뢰유형 = entity.원본의뢰유형,
                배차업무유형 = entity.배차업무유형,
                상태 = entity.상태,
                공동구매도착지유형코드 = entity.공동구매도착지유형코드,
                공동구매기사세대배송여부 = entity.공동구매기사세대배송여부,
                공동구매세대배송건수 = entity.공동구매세대배송건수,
                공동구매분배책임코드 = entity.공동구매분배책임코드
            });
        }
        catch (InvalidOperationException ex)
        {
            return 공동구매처리결과<공동구매국내운송배차대기생성결과>.잘못된요청(ex.Message);
        }
    }
}
