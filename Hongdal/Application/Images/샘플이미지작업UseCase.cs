using FluentResults;
using Hongdal.ApiMetadata;
using 홍달.Services.Images;

namespace Hongdal.Application.Images;

public interface I샘플이미지작업UseCase
{
    Task<Result<샘플이미지작업목록응답>> 작업목록Async(
        샘플이미지작업조회조건 request,
        CancellationToken cancellationToken);

    Task<Result<누락샘플이미지생성응답>> 누락이미지생성Async(
        누락샘플이미지생성요청? request,
        CancellationToken cancellationToken);

    Task<Result<샘플이미지작업요약>> 작업재시도Async(long jobId, CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.SalesChannelFulfillment)]
[HongdalUseCase("샘플 이미지 작업", Summary = "운영자가 샘플 데이터의 누락 이미지를 생성하고 실패한 이미지 작업을 재시도합니다.")]
[HongdalUseCaseActor(HongdalActor.PlatformOperator)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Include,
    "KieAi콜백UseCase",
    Condition = "외부 이미지 생성 작업의 완료 콜백을 받아 후처리하는 경우",
    Summary = "샘플 이미지 작업은 외부 Kie AI 콜백 후처리를 포함합니다.")]
public sealed class 샘플이미지작업UseCase : I샘플이미지작업UseCase
{
    private readonly I샘플이미지생성Service _sampleImageGenerationService;

    public 샘플이미지작업UseCase(I샘플이미지생성Service sampleImageGenerationService)
    {
        _sampleImageGenerationService = sampleImageGenerationService;
    }

    public async Task<Result<샘플이미지작업목록응답>> 작업목록Async(
        샘플이미지작업조회조건 request,
        CancellationToken cancellationToken)
    {
        var items = await _sampleImageGenerationService.작업목록조회Async(request, cancellationToken);
        return Result.Ok(new 샘플이미지작업목록응답
        {
            Items = items.Select(ToSummary).ToArray()
        });
    }

    public async Task<Result<누락샘플이미지생성응답>> 누락이미지생성Async(
        누락샘플이미지생성요청? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result.Fail<누락샘플이미지생성응답>("request is required");
        }

        if (string.IsNullOrWhiteSpace(request.대상타입))
        {
            return Result.Fail<누락샘플이미지생성응답>("targetType is required");
        }

        if (string.IsNullOrWhiteSpace(request.이미지용도))
        {
            return Result.Fail<누락샘플이미지생성응답>("imageUsage is required");
        }

        var jobs = await _sampleImageGenerationService.누락샘플이미지생성Async(
            request.대상타입,
            request.이미지용도,
            request.최대건수 <= 0 ? 10 : request.최대건수,
            request.실패재시도포함여부,
            cancellationToken);

        return Result.Ok(new 누락샘플이미지생성응답
        {
            생성건수 = jobs.Count,
            작업 = jobs.Select(ToSummary).ToArray()
        });
    }

    public async Task<Result<샘플이미지작업요약>> 작업재시도Async(long jobId, CancellationToken cancellationToken)
    {
        try
        {
            var job = await _sampleImageGenerationService.작업재시도Async(jobId, cancellationToken);
            return Result.Ok(ToSummary(job));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail<샘플이미지작업요약>(["샘플 이미지 작업 재시도에 실패했습니다.", ex.Message]);
        }
    }

    private static 샘플이미지작업요약 ToSummary(홍달.도메인.공통.생성이미지작업 item)
    {
        return new 샘플이미지작업요약
        {
            작업Id = item.Id,
            작업코드 = item.작업코드,
            대상타입 = item.대상타입,
            대상식별자 = item.대상식별자,
            이미지용도 = item.이미지용도,
            상태 = item.상태,
            샘플데이터여부 = item.샘플데이터여부,
            저장Url = item.저장Url,
            실패사유 = item.실패사유,
            생성시각 = item.생성시각,
            완료시각 = item.완료시각
        };
    }
}

public sealed class 샘플이미지작업목록응답
{
    public IReadOnlyList<샘플이미지작업요약> Items { get; set; } = [];
}

public sealed class 누락샘플이미지생성요청
{
    public string 대상타입 { get; set; } = string.Empty;
    public string 이미지용도 { get; set; } = string.Empty;
    public int 최대건수 { get; set; } = 10;
    public bool 실패재시도포함여부 { get; set; }
}

public sealed class 누락샘플이미지생성응답
{
    public int 생성건수 { get; set; }
    public IReadOnlyList<샘플이미지작업요약> 작업 { get; set; } = [];
}

public sealed class 샘플이미지작업요약
{
    public long 작업Id { get; set; }
    public string 작업코드 { get; set; } = string.Empty;
    public string 대상타입 { get; set; } = string.Empty;
    public string 대상식별자 { get; set; } = string.Empty;
    public string 이미지용도 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public bool 샘플데이터여부 { get; set; }
    public string? 저장Url { get; set; }
    public string? 실패사유 { get; set; }
    public DateTime 생성시각 { get; set; }
    public DateTime? 완료시각 { get; set; }
}
