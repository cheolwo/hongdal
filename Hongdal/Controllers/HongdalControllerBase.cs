using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers;

public abstract class HongdalControllerBase : ControllerBase
{
    protected string? CurrentUserId
        => User.FindFirstValue(ClaimTypes.NameIdentifier);

    protected string RequireCurrentUserId(string missingUserMessage)
        => CurrentUserId ?? throw new InvalidOperationException(missingUserMessage);
}

public abstract class DriverControllerBase : HongdalControllerBase
{
    protected string CurrentDriverId()
        => RequireCurrentUserId("기사 인증 정보가 없습니다.");

    protected string 현재기사Id()
        => CurrentDriverId();
}
