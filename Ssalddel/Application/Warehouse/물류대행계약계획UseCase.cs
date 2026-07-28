using FluentResults;
using Microsoft.AspNetCore.Http;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.WarehouseBilling;

namespace Ssalddel.Application.Warehouse;

public interface I물류대행계약계획UseCase
{
    Task<Result<물류대행비용미리보기응답>> 비용미리보기Async(
        물류대행비용미리보기요청 request,
        string requesterUserId,
        string requesterDisplayName,
        CancellationToken cancellationToken = default);
}

[SsalddelApiWorkflow(SsalddelWorkflow.WarehouseFulfillment)]
[SsalddelUseCase(
    "물류대행 계약 계획",
    Summary = "계약별 화주 당사자, 서비스 범위, 요율표 버전과 예상 물류비를 검토용 초안으로 계산합니다.")]
[SsalddelUseCaseActor(SsalddelActor.CommunityMember)]
[SsalddelUseCaseActor(SsalddelActor.Orderer, SsalddelUseCaseActorRole.Supporting)]
[SsalddelUseCaseActor(SsalddelActor.OrdererGroupLeader, SsalddelUseCaseActorRole.Supporting)]
[SsalddelUseCaseActor(SsalddelActor.Seller, SsalddelUseCaseActorRole.Supporting)]
[SsalddelUseCaseActor(SsalddelActor.Shipper, SsalddelUseCaseActorRole.Supporting)]
[SsalddelUseCaseActor(SsalddelActor.WarehouseManager, SsalddelUseCaseActorRole.Supporting)]
public sealed class 물류대행계약계획UseCase : I물류대행계약계획UseCase
{
    public Task<Result<물류대행비용미리보기응답>> 비용미리보기Async(
        물류대행비용미리보기요청 request,
        string requesterUserId,
        string requesterDisplayName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var preview = 물류대행계약계획기.Plan(
                requesterUserId,
                requesterDisplayName,
                request,
                DateTimeOffset.UtcNow);
            return Task.FromResult(Result.Ok(preview));
        }
        catch (ArgumentException exception)
        {
            return Task.FromResult(Result.Fail<물류대행비용미리보기응답>(
                new Error(exception.Message)
                    .WithMetadata("StatusCode", StatusCodes.Status400BadRequest)));
        }
    }
}
