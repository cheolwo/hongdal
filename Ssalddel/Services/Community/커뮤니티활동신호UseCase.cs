using FluentResults;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Services.Community;

public interface I커뮤니티활동신호UseCase
{
    Task<Result<CommunityActivitySignalListResponse>> 조회Async(
        CommunityActivitySignalQuery? query,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelUseCase("커뮤니티 활동 신호 조회", Summary = "허용된 업무 이벤트의 비식별 집계 투영에서 공개 가능한 활동 신호를 조회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.CommunityMember)]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator, SsalddelUseCaseActorRole.Supporting)]
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
