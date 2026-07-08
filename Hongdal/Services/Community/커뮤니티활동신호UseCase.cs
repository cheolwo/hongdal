using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;

namespace Hongdal.Services.Community;

public interface I커뮤니티활동신호UseCase
{
    Task<Result<CommunityActivitySignalListResponse>> 조회Async(
        CommunityActivitySignalQuery? query,
        CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalUseCase("커뮤니티 활동 신호 조회", Summary = "업무 로그에서 개인정보 보호 범위 안에 공개 가능한 활동 신호를 조회합니다.")]
[HongdalUseCaseActor(HongdalActor.CommunityMember)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Include,
    "사용자행위로그조회UseCase",
    Condition = "업무 행위 로그를 개인정보 보호 범위 안에서 커뮤니티 신호로 투영하는 경우",
    Summary = "커뮤니티 활동 신호 조회는 원천 행위 로그 조회와 필터링을 포함합니다.")]
public sealed class 커뮤니티활동신호UseCase : I커뮤니티활동신호UseCase
{
    private readonly ICommunityActivitySignalService _signalService;

    public 커뮤니티활동신호UseCase(ICommunityActivitySignalService signalService)
    {
        _signalService = signalService;
    }

    public async Task<Result<CommunityActivitySignalListResponse>> 조회Async(
        CommunityActivitySignalQuery? query,
        CancellationToken cancellationToken)
    {
        var request = query ?? new CommunityActivitySignalQuery();
        return Result.Ok(await _signalService.GetSignalsAsync(request, cancellationToken));
    }
}
