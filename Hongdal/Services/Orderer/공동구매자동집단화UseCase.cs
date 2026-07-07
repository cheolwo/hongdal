using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Services.Orderer;

public interface I공동구매자동집단화UseCase
{
    Task<공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>> 목록조회Async(
        공동구매자동집단조회조건 조건,
        CancellationToken cancellationToken = default);

    Task<공동구매처리결과<공동구매자동집단응답>> 수요등록Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default);
}

[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[HongdalUseCase("공동구매 자동 집단화", Summary = "주문자의 구매 의사를 배송권 기준으로 모아 공동구매 집단 후보를 형성합니다.")]
[HongdalUseCaseActor(HongdalActor.Orderer)]
[HongdalUseCaseActor(HongdalActor.OrdererGroupLeader, HongdalUseCaseActorRole.Supporting)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
public sealed class 공동구매자동집단화UseCase : I공동구매자동집단화UseCase
{
    private readonly I공동구매자동집단화저장소 _저장소;

    public 공동구매자동집단화UseCase(I공동구매자동집단화저장소 저장소)
    {
        _저장소 = 저장소;
    }

    public async Task<공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>> 목록조회Async(
        공동구매자동집단조회조건 조건,
        CancellationToken cancellationToken = default)
    {
        var items = await _저장소.집단목록조회Async(조건, cancellationToken);
        return 공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>.성공결과(items);
    }

    public async Task<공동구매처리결과<공동구매자동집단응답>> 수요등록Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _저장소.수요등록Async(command, cancellationToken);
            return 공동구매처리결과<공동구매자동집단응답>.성공결과(group);
        }
        catch (InvalidOperationException ex)
        {
            return 공동구매처리결과<공동구매자동집단응답>.잘못된요청(ex.Message);
        }
    }
}
