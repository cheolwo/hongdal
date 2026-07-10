using Hongdal.ApiMetadata;
using Hongdal.Application.Admin.Management;
using Hongdal.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Admin.Master06;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[ApiController]
[Route("api/v1/admin/contact-search")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 관리자연락처검색Controller : ControllerBase
{
    private readonly ISender _sender;

    public 관리자연락처검색Controller(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> 전화번호뒤8자리검색([FromQuery] string phoneLast8)
    {
        var digits = OnlyDigits(phoneLast8);
        if (digits.Length != 8)
        {
            return this.ToProblemActionResult("전화번호 뒤 8자리 숫자를 입력해야 합니다.", StatusCodes.Status400BadRequest);
        }

        var result = await _sender.Send(new 관리자연락처검색Query(digits));
        return Ok(result);
    }

    private static string OnlyDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsDigit).ToArray());
    }
}
