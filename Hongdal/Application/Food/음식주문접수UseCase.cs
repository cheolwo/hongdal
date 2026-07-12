using Hongdal.ApiMetadata;
using Hongdal.Application.Food.Commands;
using Hongdal.Application.Food.Queries;
using Hongdal.Contracts.Food;
using MediatR;

namespace Hongdal.Application.Food;

public interface I음식주문접수UseCase
{
    Task<음식주문목록응답> 목록조회Async(CancellationToken cancellationToken);

    Task<음식주문응답?> 상세조회Async(string orderNo, CancellationToken cancellationToken);

    Task<음식주문응답> 등록Async(음식주문등록요청 request, CancellationToken cancellationToken);

    Task<음식주문응답?> 음식점수락Async(string orderNo, 음식점주문수락요청 request, string? 처리UserId, CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.FoodDelivery)]
[HongdalUseCase("음식 주문 접수와 음식점 알림", Summary = "주문자가 음식 주문을 등록하면 음식점 데스크에 실시간 주문 알림을 보내고, 음식점 수락 후 조리·전표 출력·배차 흐름으로 넘길 준비를 합니다.")]
[HongdalUseCaseActor(HongdalActor.Orderer)]
[HongdalUseCaseActor(HongdalActor.Restaurant)]
[HongdalUseCaseActor(HongdalActor.FoodDeliveryDriver, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "기사배차추천UseCase",
    Condition = "음식점이 주문을 수락한 뒤 배달 기사 배차 후보를 산정해야 하는 경우",
    Summary = "음식 주문 접수 결과를 음식 배달권과 기사 추천 흐름으로 확장합니다.")]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "배달기사월정산UseCase",
    Condition = "음식 배달이 완료되어 기사 이용료, 정산, 플랫폼 수익 배분으로 이어지는 경우",
    Summary = "음식 주문과 배달 완료 이력을 배달 기사 월정산 흐름으로 확장합니다.")]
public sealed class 음식주문접수UseCase(ISender sender) : I음식주문접수UseCase
{
    public Task<음식주문목록응답> 목록조회Async(CancellationToken cancellationToken)
    {
        return sender.Send(new 음식주문목록조회Query(), cancellationToken);
    }

    public Task<음식주문응답?> 상세조회Async(string orderNo, CancellationToken cancellationToken)
    {
        return sender.Send(new 음식주문상세조회Query(orderNo), cancellationToken);
    }

    public Task<음식주문응답> 등록Async(음식주문등록요청 request, CancellationToken cancellationToken)
    {
        return sender.Send(new 음식주문등록Command(request), cancellationToken);
    }

    public Task<음식주문응답?> 음식점수락Async(string orderNo, 음식점주문수락요청 request, string? 처리UserId, CancellationToken cancellationToken)
    {
        return sender.Send(new 음식점주문수락Command(orderNo, request, 처리UserId), cancellationToken);
    }
}
