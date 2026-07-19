using System.Text.Json;
using FluentResults;
using Ssalddel.ApiMetadata;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;
using 살뜰.Services.Images;

namespace Ssalddel.Application.Images;

public interface IKieAi콜백UseCase
{
    Task<Result<KieAi콜백처리결과>> 처리Async(JsonElement payload, CancellationToken cancellationToken);
}

public sealed record KieAi콜백처리결과(bool Accepted, bool Processed);

[SsalddelApiWorkflow(SsalddelWorkflow.SalesChannelFulfillment)]
[SsalddelUseCase("Kie AI 이미지 콜백", Summary = "외부 이미지 생성 콜백을 받아 샘플 이미지 작업 결과를 후처리합니다.")]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator)]
[SsalddelUseCaseRelation(
    SsalddelUseCaseRelationKind.Extend,
    "샘플이미지작업UseCase",
    Condition = "외부 이미지 생성 작업이 완료되어 저장 URL과 실패 사유를 갱신하는 경우",
    Summary = "Kie AI 콜백을 샘플 이미지 작업 상태 갱신 흐름으로 확장합니다.")]
public sealed class KieAi콜백UseCase : IKieAi콜백UseCase
{
    private readonly SsalddelContext _db;
    private readonly I샘플이미지생성Service _sampleImageGenerationService;

    public KieAi콜백UseCase(SsalddelContext db, I샘플이미지생성Service sampleImageGenerationService)
    {
        _db = db;
        _sampleImageGenerationService = sampleImageGenerationService;
    }

    public async Task<Result<KieAi콜백처리결과>> 처리Async(JsonElement payload, CancellationToken cancellationToken)
    {
        var rawJson = payload.GetRawText();
        var taskId = ResolveTaskId(payload);
        if (string.IsNullOrWhiteSpace(taskId))
        {
            return Result.Ok(new KieAi콜백처리결과(Accepted: true, Processed: false));
        }

        var job = await _db.생성이미지작업.FirstOrDefaultAsync(x => x.외부TaskId == taskId, cancellationToken);
        if (job is null)
        {
            return Result.Ok(new KieAi콜백처리결과(Accepted: true, Processed: false));
        }

        var processed = await _sampleImageGenerationService.작업후처리Async(job.Id, rawJson, cancellationToken);
        return Result.Ok(new KieAi콜백처리결과(Accepted: true, processed));
    }

    private static string? ResolveTaskId(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (payload.TryGetProperty("taskId", out var taskIdElement))
        {
            return taskIdElement.GetString();
        }

        if (payload.TryGetProperty("data", out var dataElement)
            && dataElement.ValueKind == JsonValueKind.Object
            && dataElement.TryGetProperty("taskId", out var nestedTaskIdElement))
        {
            return nestedTaskIdElement.GetString();
        }

        return null;
    }
}
