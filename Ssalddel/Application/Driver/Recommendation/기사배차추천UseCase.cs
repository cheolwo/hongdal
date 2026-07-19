using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Driver.Recommendation;
using Ssalddel.Hubs;
using MediatR;
using 살뜰.Services;
using 살뜰.Services.Dispatch.Queue;
using 살뜰.Services.Dispatch.Request;

namespace Ssalddel.Application.Driver.Recommendation;

public interface I기사배차추천UseCase
{
    Task<IReadOnlyList<DispatchRecommendationDto>> 추천조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DispatchRecommendationDto>> 비운행중추천조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DispatchRecommendationDto>> 운행중추천조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DispatchRecommendationDto>> 위치기반추천검색Async(
        string 기사Id,
        decimal latitude,
        decimal longitude,
        decimal radiusKm,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DispatchRecommendationDto>> 전국콜조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DispatchRecommendationDto>> 공개배차조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default);

    Task<기사배차추천요약응답> 추천요약조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default);

    Task<기사운송의뢰상세응답> 운송의뢰상세조회Async(
        string 기사Id,
        string 의뢰Id,
        CancellationToken cancellationToken = default);
}

[SsalddelApiWorkflow(SsalddelWorkflow.DomesticTransport)]
[SsalddelUseCase("기사 배차 추천 조회", Summary = "기사에게 일반 화물, 공동주문 운송, 공개 배차, 전국콜 후보를 추천하고 상세를 조회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Driver)]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator, SsalddelUseCaseActorRole.Supporting)]
[SsalddelUseCaseRelation(
    SsalddelUseCaseRelationKind.Include,
    "용달기사프로필UseCase",
    Condition = "기사 추천 후보를 산정하기 전",
    Summary = "기사 추천은 기사 프로필, 차량, 운행 가능 상태를 전제로 합니다.")]
public sealed class 기사배차추천UseCase : I기사배차추천UseCase
{
    private readonly I배차추천Service _배차추천Service;
    private readonly INationalDispatchRequestService _전국콜Service;
    private readonly I공개배차Service _공개배차Service;
    private readonly ISender _sender;

    public 기사배차추천UseCase(
        I배차추천Service 배차추천Service,
        INationalDispatchRequestService 전국콜Service,
        I공개배차Service 공개배차Service,
        ISender sender)
    {
        _배차추천Service = 배차추천Service;
        _전국콜Service = 전국콜Service;
        _공개배차Service = 공개배차Service;
        _sender = sender;
    }

    public async Task<IReadOnlyList<DispatchRecommendationDto>> 추천조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default)
        => await _배차추천Service.GetRecommendationsAsync(기사Id);

    public async Task<IReadOnlyList<DispatchRecommendationDto>> 비운행중추천조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default)
        => await _배차추천Service.GetIdleRecommendationsAsync(기사Id);

    public async Task<IReadOnlyList<DispatchRecommendationDto>> 운행중추천조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default)
        => await _배차추천Service.GetDrivingRecommendationsAsync(기사Id);

    public async Task<IReadOnlyList<DispatchRecommendationDto>> 위치기반추천검색Async(
        string 기사Id,
        decimal latitude,
        decimal longitude,
        decimal radiusKm,
        CancellationToken cancellationToken = default)
    {
        var criteria = new 배차추천검색조건(latitude, longitude, radiusKm);
        return await _배차추천Service.GetRecommendationsAsync(기사Id, criteria);
    }

    public async Task<IReadOnlyList<DispatchRecommendationDto>> 전국콜조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default)
        => await _전국콜Service.GetNationwideRequestsAsync(기사Id, cancellationToken);

    public async Task<IReadOnlyList<DispatchRecommendationDto>> 공개배차조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default)
        => await _공개배차Service.GetPublicDispatchesAsync(기사Id, cancellationToken);

    public async Task<기사배차추천요약응답> 추천요약조회Async(
        string 기사Id,
        CancellationToken cancellationToken = default)
    {
        var all = await _배차추천Service.GetRecommendationsAsync(기사Id);
        var idle = await _배차추천Service.GetIdleRecommendationsAsync(기사Id);
        var driving = await _배차추천Service.GetDrivingRecommendationsAsync(기사Id);
        var national = await _전국콜Service.GetNationwideRequestsAsync(기사Id, cancellationToken);

        return new 기사배차추천요약응답
        {
            전체추천수 = all.Count,
            적합추천수 = all.Count(x => x.차량적합여부),
            운행중추천수 = driving.Count,
            비운행중추천수 = idle.Count,
            전국콜수 = national.Count
        };
    }

    public async Task<기사운송의뢰상세응답> 운송의뢰상세조회Async(
        string 기사Id,
        string 의뢰Id,
        CancellationToken cancellationToken = default)
        => await _sender.Send(new 운송의뢰상세조회Query(기사Id, 의뢰Id), cancellationToken);
}
