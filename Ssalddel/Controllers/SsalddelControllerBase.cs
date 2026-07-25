using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers;

public abstract class SsalddelControllerBase : ControllerBase
{
    protected string? CurrentUserId
        => User.FindFirstValue(ClaimTypes.NameIdentifier);

    protected string RequireCurrentUserId(string missingUserMessage)
        => CurrentUserId ?? throw new InvalidOperationException(missingUserMessage);
}

[SsalddelApiAudience(SsalddelActor.Driver)]
public abstract class DriverControllerBase : SsalddelControllerBase
{
    protected string CurrentDriverId()
        => RequireCurrentUserId("기사 인증 정보가 없습니다.");

    protected string 현재기사Id()
        => CurrentDriverId();
}

[SsalddelApiAudience(SsalddelActor.Orderer)]
public abstract class OrdererControllerBase : ControllerBase
{
}

[SsalddelApiAudience(SsalddelActor.Shipper)]
public abstract class ShipperControllerBase : ControllerBase
{
}

[SsalddelApiAudience(SsalddelActor.CommunityMember)]
public abstract class CommunityControllerBase : ControllerBase
{
}
