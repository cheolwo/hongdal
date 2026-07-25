using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.도메인.공통;

namespace Ssalddel.Controllers.Common;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[Route("api/v1/community/driver-availability")]
[SsalddelApiContractName("CommunityDriverAvailabilityController")]
public sealed class 커뮤니티기사운행가능Controller : CommunityControllerBase
{
    private readonly ICommunityDriverAvailabilityService 기사운행가능Service;

    public 커뮤니티기사운행가능Controller(ICommunityDriverAvailabilityService 기사운행가능Service)
    {
        this.기사운행가능Service = 기사운행가능Service;
    }

    [HttpGet]
    [AllowAnonymous]
    [SsalddelApiContractName("GetActive")]
    public IActionResult 운행가능기사조회([FromQuery] string? operatingArea = null)
        => Ok(기사운행가능Service.GetActive(operatingArea));

    [HttpGet("my-inquiries")]
    [Authorize]
    [SsalddelApiContractName("GetMyInquiries")]
    public IActionResult 내문의목록조회()
        => Ok(기사운행가능Service.GetRequesterInquiries(CurrentUserId()));

    [HttpPost("{postId:guid}/inquiries")]
    [Authorize]
    [SsalddelApiContractName("CreateInquiry")]
    public IActionResult 문의생성(Guid postId, [FromBody] CommunityDriverInquiryCreateRequest request)
    {
        try
        {
            return Ok(기사운행가능Service.CreateInquiry(postId, CurrentUserId(), CurrentRole(), request));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");

    private string CurrentRole()
        => User.FindFirstValue(ClaimTypes.Role)
           ?? User.FindFirstValue("role")
           ?? "운송 요청자";
}

[ApiController]
[Authorize(Roles = 역할명.기사)]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.DomesticTransport)]
[Route("api/v1/driver/community-inquiries")]
[SsalddelApiContractName("DriverCommunityInquiriesController")]
public sealed class 기사커뮤니티문의Controller : DriverControllerBase
{
    private readonly ICommunityDriverAvailabilityService 기사운행가능Service;

    public 기사커뮤니티문의Controller(ICommunityDriverAvailabilityService 기사운행가능Service)
    {
        this.기사운행가능Service = 기사운행가능Service;
    }

    [HttpGet]
    [SsalddelApiContractName("GetMine")]
    public IActionResult 내운행가능상태조회()
        => Ok(기사운행가능Service.GetDriverInquiries(현재기사Id()));

    [HttpPost("{inquiryId:guid}/decision")]
    [SsalddelApiContractName("Decide")]
    public IActionResult 문의결정(Guid inquiryId, [FromBody] CommunityDriverInquiryDecisionRequest request)
    {
        try
        {
            return Ok(기사운행가능Service.Decide(현재기사Id(), inquiryId, request));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
