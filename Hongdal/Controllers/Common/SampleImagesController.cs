using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Images;

namespace Hongdal.Controllers.Common;

[ApiController]
[Authorize(Policy = "운영사용자전용")]
[Route("api/v1/sample-images")]
public sealed class SampleImagesController : ControllerBase
{
    private readonly I샘플이미지생성Service _sampleImageGenerationService;

    public SampleImagesController(I샘플이미지생성Service sampleImageGenerationService)
    {
        _sampleImageGenerationService = sampleImageGenerationService;
    }

    [HttpGet]
    public async Task<IActionResult> 작업목록(
        [FromQuery] string? 대상타입,
        [FromQuery] string? 이미지용도,
        [FromQuery] string? 상태,
        [FromQuery] bool? 샘플데이터여부,
        [FromQuery] string? 대상식별자,
        [FromQuery] int 최대건수 = 50,
        CancellationToken cancellationToken = default)
    {
        var items = await _sampleImageGenerationService.작업목록조회Async(new 샘플이미지작업조회조건
        {
            대상타입 = 대상타입,
            이미지용도 = 이미지용도,
            상태 = 상태,
            샘플데이터여부 = 샘플데이터여부,
            대상식별자 = 대상식별자,
            최대건수 = 최대건수
        }, cancellationToken);

        return Ok(new 샘플이미지작업목록응답
        {
            Items = items.Select(ToSummary).ToArray()
        });
    }

    [HttpPost("generate-missing")]
    public async Task<IActionResult> 누락이미지생성([FromBody] 누락샘플이미지생성요청 request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("request is required");
        }

        if (string.IsNullOrWhiteSpace(request.대상타입))
        {
            return BadRequest("targetType is required");
        }

        if (string.IsNullOrWhiteSpace(request.이미지용도))
        {
            return BadRequest("imageUsage is required");
        }

        var jobs = await _sampleImageGenerationService.누락샘플이미지생성Async(
            request.대상타입,
            request.이미지용도,
            request.최대건수 <= 0 ? 10 : request.최대건수,
            request.실패재시도포함여부,
            cancellationToken);

        return Ok(new 누락샘플이미지생성응답
        {
            생성건수 = jobs.Count,
            작업 = jobs.Select(ToSummary).ToArray()
        });
    }

    [HttpPost("{jobId:long}/retry")]
    public async Task<IActionResult> 작업재시도(long jobId, CancellationToken cancellationToken)
    {
        try
        {
            var job = await _sampleImageGenerationService.작업재시도Async(jobId, cancellationToken);
            return Ok(ToSummary(job));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "샘플 이미지 작업 재시도에 실패했습니다.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    private static 샘플이미지작업요약 ToSummary(홍달.도메인.공통.생성이미지작업 x)
    {
        return new 샘플이미지작업요약
        {
            작업Id = x.Id,
            작업코드 = x.작업코드,
            대상타입 = x.대상타입,
            대상식별자 = x.대상식별자,
            이미지용도 = x.이미지용도,
            상태 = x.상태,
            샘플데이터여부 = x.샘플데이터여부,
            저장Url = x.저장Url,
            실패사유 = x.실패사유,
            생성시각 = x.생성시각,
            완료시각 = x.완료시각
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
