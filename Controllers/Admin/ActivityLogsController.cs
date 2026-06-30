using Hongdal.Contracts.Admin.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Controllers.Admin;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/activity-logs")]
public sealed class ActivityLogsController : ControllerBase
{
    private readonly HongdalContext _db;

    public ActivityLogsController(HongdalContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<사용자행위로그목록응답>> 조회([FromQuery] 사용자행위로그검색요청 request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 50 : Math.Min(request.PageSize, 200);

        var query = _db.사용자행위로그.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.AppKey))
        {
            query = query.Where(x => x.AppKey == request.AppKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var userId = request.UserId.Trim();
            query = query.Where(x => x.UserId.Contains(userId));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var maskedEmail = MaskEmail(request.Email.Trim());
            query = query.Where(x => x.EmailMasked.Contains(maskedEmail));
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneLast4))
        {
            var digits = GetPhoneLast4(request.PhoneLast4.Trim());
            query = query.Where(x => x.PhoneLast4 == digits);
        }

        if (!string.IsNullOrWhiteSpace(request.ActionType))
        {
            var actionType = request.ActionType.Trim();
            query = query.Where(x => x.ActionType == actionType);
        }

        if (!string.IsNullOrWhiteSpace(request.ActionName))
        {
            var actionName = request.ActionName.Trim();
            query = query.Where(x => x.ActionName.Contains(actionName));
        }

        if (!string.IsNullOrWhiteSpace(request.TraceId))
        {
            var traceId = request.TraceId.Trim();
            query = query.Where(x => x.TraceId == traceId);
        }

        if (request.IsSuccess.HasValue)
        {
            query = query.Where(x => x.IsSuccess == request.IsSuccess.Value);
        }

        if (request.FromUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc <= request.ToUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new 사용자행위로그요약응답
            {
                Id = x.Id,
                AppKey = x.AppKey,
                UserId = x.UserId,
                UserName = x.UserName,
                RoleName = x.RoleName,
                EmailMasked = x.EmailMasked,
                PhoneLast4 = x.PhoneLast4,
                ActionType = x.ActionType,
                ActionName = x.ActionName,
                Route = x.Route,
                TraceId = x.TraceId,
                IsSuccess = x.IsSuccess,
                OccurredAtUtc = x.OccurredAtUtc
            })
            .ToArrayAsync(cancellationToken);

        return Ok(new 사용자행위로그목록응답
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<사용자행위로그상세응답>> 상세(long id, CancellationToken cancellationToken)
    {
        var item = await _db.사용자행위로그.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new 사용자행위로그상세응답
            {
                Id = x.Id,
                AppKey = x.AppKey,
                UserId = x.UserId,
                UserName = x.UserName,
                RoleName = x.RoleName,
                EmailMasked = x.EmailMasked,
                PhoneLast4 = x.PhoneLast4,
                ActionType = x.ActionType,
                ActionName = x.ActionName,
                Route = x.Route,
                TraceId = x.TraceId,
                IsSuccess = x.IsSuccess,
                ErrorCode = x.ErrorCode,
                ErrorMessage = x.ErrorMessage,
                ClientIp = x.ClientIp,
                UserAgent = x.UserAgent,
                OccurredAtUtc = x.OccurredAtUtc,
                MetadataJson = x.MetadataJson
            })
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("trace/{traceId}")]
    public async Task<ActionResult<Trace행위로그묶음응답>> Trace조회(string traceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(traceId))
        {
            return BadRequest("traceId is required");
        }

        var normalizedTraceId = traceId.Trim();
        var items = await _db.사용자행위로그.AsNoTracking()
            .Where(x => x.TraceId == normalizedTraceId)
            .OrderBy(x => x.OccurredAtUtc)
            .Select(x => new 사용자행위로그요약응답
            {
                Id = x.Id,
                AppKey = x.AppKey,
                UserId = x.UserId,
                UserName = x.UserName,
                RoleName = x.RoleName,
                EmailMasked = x.EmailMasked,
                PhoneLast4 = x.PhoneLast4,
                ActionType = x.ActionType,
                ActionName = x.ActionName,
                Route = x.Route,
                TraceId = x.TraceId,
                IsSuccess = x.IsSuccess,
                OccurredAtUtc = x.OccurredAtUtc
            })
            .ToArrayAsync(cancellationToken);

        return Ok(new Trace행위로그묶음응답
        {
            TraceId = normalizedTraceId,
            Items = items
        });
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
        {
            return email;
        }

        var local = email[..atIndex];
        var domain = email[(atIndex + 1)..];
        return string.IsNullOrWhiteSpace(domain)
            ? local[0] + "***"
            : $"{local[0]}***@{domain}";
    }

    private static string GetPhoneLast4(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length <= 4)
        {
            return digits;
        }

        return digits[^4..];
    }
}
