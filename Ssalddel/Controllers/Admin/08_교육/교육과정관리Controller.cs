using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Education;
using Ssalddel.Services.Education;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Admin.Education08;

[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/education")]
public sealed class 교육과정관리Controller : ControllerBase
{
    private readonly I교육과정정의Service _정의Service;
    private readonly I교육과정참여Service _참여Service;

    public 교육과정관리Controller(
        I교육과정정의Service 정의Service,
        I교육과정참여Service 참여Service)
    {
        _정의Service = 정의Service;
        _참여Service = 참여Service;
    }

    [HttpGet("courses")]
    public async Task<IActionResult> 과정목록조회(CancellationToken cancellationToken)
        => Ok(await _정의Service.목록조회Async(false, cancellationToken));

    [HttpGet("courses/{과정코드}")]
    public async Task<IActionResult> 과정상세조회(string 과정코드, CancellationToken cancellationToken)
        => Ok(await _정의Service.상세조회Async(과정코드, false, cancellationToken));

    [HttpPut("courses/{과정코드}")]
    public async Task<IActionResult> 과정저장(
        string 과정코드,
        [FromBody] 교육과정관리요청 요청,
        CancellationToken cancellationToken)
        => Ok(await _정의Service.저장Async(과정코드, 요청, cancellationToken));

    [HttpPost("presets/hongik-academy-shinsa-online")]
    public async Task<IActionResult> 홍익학당온라인신사과정초안등록(CancellationToken cancellationToken)
        => Ok(await _정의Service.홍익학당온라인신사과정초안등록Async(cancellationToken));

    [HttpGet("applications")]
    public async Task<IActionResult> 신청목록조회(
        [FromQuery] string? 과정코드,
        [FromQuery] string? 상태,
        [FromQuery] int 개수 = 100,
        CancellationToken cancellationToken = default)
        => Ok(await _참여Service.신청목록조회Async(과정코드, 상태, 개수, cancellationToken));
}
