using System.Security.Claims;
using FluentResults;
using Hongdal.Application.Immigration;
using Hongdal.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[ApiController]
[Authorize]
[Route("api/v1/immigration/visa-support-requests")]
public sealed class VisaSupportController : ControllerBase
{
    private readonly ISender _sender;

    public VisaSupportController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> RequestVisaSupport([FromBody] VisaSupportRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return this.ToAuthenticationProblem("로그인 사용자 정보를 확인할 수 없습니다.");
        }

        var result = await _sender.Send(
            new VisaSupportRequestCommand(
                userId,
                request.ForeignPartnerName,
                request.ForeignPartnerCountry,
                request.ForeignPartnerCompanyName,
                request.ImporterUserId,
                request.RelatedOrderReference,
                request.DesiredVisaType,
                request.SupportMemo),
            cancellationToken);

        return ToActionResult(result);
    }

    private IActionResult ToActionResult(Result<VisaSupportRequestResult> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return this.ToProblemActionResult(result.Errors.Select(x => x.Message));
    }
}

public sealed class VisaSupportRequest
{
    public string ForeignPartnerName { get; set; } = string.Empty;
    public string ForeignPartnerCountry { get; set; } = string.Empty;
    public string? ForeignPartnerCompanyName { get; set; }
    public string? ImporterUserId { get; set; }
    public string? RelatedOrderReference { get; set; }
    public string? DesiredVisaType { get; set; }
    public string? SupportMemo { get; set; }
}
