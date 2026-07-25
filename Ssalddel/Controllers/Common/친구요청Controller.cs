using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Application.Connections.Commands;
using Ssalddel.Application.Connections.Queries;
using Ssalddel.Contracts.Common.Hr;
using 살뜰.도메인.사용자;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Common;

[SsalddelApiIntroducedIn(SsalddelProductVersion.V0_0)]
[SsalddelApiCapability(SsalddelCapability.RelationshipFormation)]
[SsalddelApiAudience(SsalddelActor.CommunityMember)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[SsalddelApiOperation(SsalddelOperation.Request)]
[SsalddelApiOperation(SsalddelOperation.Decide)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[ApiController]
[Authorize]
[Route("api/v1/connections")]
public sealed class 친구요청Controller : CommunityControllerBase
{
    private readonly ISender _sender;

    public 친구요청Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("requests")]
    public async Task<IActionResult> 요청생성([FromBody] 친구요청생성요청 request, CancellationToken cancellationToken)
    {
        var command = new 친구요청작성Command(
            request.요청자참여자Id,
            request.요청자역할,
            request.대상자참여자Id,
            request.대상자역할,
            request.감사메시지Id,
            request.주문Id,
            request.통관절차Id,
            request.요청목적,
            request.요청메시지);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("requests/from-work-relationship/{snapshotId:guid}")]
    public async Task<IActionResult> 업무관계에서친구요청생성(
        Guid snapshotId,
        [FromBody] WorkRelationshipConnectionRequestCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new 업무관계친구요청작성Command(
                snapshotId,
                request.Purpose,
                request.Message),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("requests/{connectionRequestId:long}/respond")]
    public async Task<IActionResult> 요청응답(long connectionRequestId, [FromBody] 친구요청응답요청 request, CancellationToken cancellationToken)
    {
        var command = new 친구요청응답Command(
            connectionRequestId,
            request.수락,
            request.거절사유,
            request.공개동의 is null
                ? null
                : new 연락처공개동의입력
                {
                    동의자참여자Id = request.공개동의.동의자참여자Id,
                    프로필공개 = request.공개동의.프로필공개,
                    업체명공개 = request.공개동의.업체명공개,
                    이메일공개 = request.공개동의.이메일공개,
                    전화번호공개 = request.공개동의.전화번호공개,
                    카카오채널공개 = request.공개동의.카카오채널공개,
                    판매채널공개 = request.공개동의.판매채널공개,
                    제공목적 = request.공개동의.제공목적
                });

        var result = await _sender.Send(command, cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpGet("requests/sent")]
    public async Task<IActionResult> 내요청함([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new 내친구요청함조회Query(page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("requests/received")]
    public async Task<IActionResult> 수신요청함([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new 내친구수신함조회Query(page, pageSize), cancellationToken);
        return Ok(result);
    }

}

public sealed class 친구요청생성요청
{
    public string 요청자참여자Id { get; set; } = string.Empty;
    public 살뜰역할유형 요청자역할 { get; set; }
    public string 대상자참여자Id { get; set; } = string.Empty;
    public 살뜰역할유형 대상자역할 { get; set; }
    public long? 감사메시지Id { get; set; }
    public long? 주문Id { get; set; }
    public long? 통관절차Id { get; set; }
    public string 요청목적 { get; set; } = string.Empty;
    public string 요청메시지 { get; set; } = string.Empty;
}

public sealed class 친구요청응답요청
{
    public bool 수락 { get; set; }
    public string? 거절사유 { get; set; }
    public 연락처공개동의요청? 공개동의 { get; set; }
}

public sealed class 연락처공개동의요청
{
    public string 동의자참여자Id { get; set; } = string.Empty;
    public bool 프로필공개 { get; set; }
    public bool 업체명공개 { get; set; }
    public bool 이메일공개 { get; set; }
    public bool 전화번호공개 { get; set; }
    public bool 카카오채널공개 { get; set; }
    public bool 판매채널공개 { get; set; }
    public string 제공목적 { get; set; } = string.Empty;
}
