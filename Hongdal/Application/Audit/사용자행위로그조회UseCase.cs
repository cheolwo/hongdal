using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Admin.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Application.Audit;

public interface I사용자행위로그조회UseCase
{
    Task<Result<사용자행위로그목록응답>> 조회Async(
        사용자행위로그검색요청? request,
        CancellationToken cancellationToken);

    Task<Result<사용자행위로그상세응답>> 상세Async(
        long id,
        CancellationToken cancellationToken);

    Task<Result<Trace행위로그묶음응답>> Trace조회Async(
        string? traceId,
        CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalUseCase("사용자 행위 로그 조회", Summary = "운영자가 사용자 행위 로그를 조회하고 커뮤니티 활동 신호의 원천 기록을 확인합니다.")]
[HongdalUseCaseActor(HongdalActor.PlatformOperator)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "커뮤니티활동신호UseCase",
    Condition = "공개 가능한 활동만 커뮤니티 신뢰 신호로 투영하는 경우",
    Summary = "행위 로그 조회를 개인정보 보호 필터가 적용된 커뮤니티 활동 신호로 확장합니다.")]
public sealed class 사용자행위로그조회UseCase : I사용자행위로그조회UseCase
{
    private readonly HongdalContext _db;

    public 사용자행위로그조회UseCase(HongdalContext db)
    {
        _db = db;
    }

    public async Task<Result<사용자행위로그목록응답>> 조회Async(
        사용자행위로그검색요청? request,
        CancellationToken cancellationToken)
    {
        request ??= new 사용자행위로그검색요청();
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

        return Result.Ok(new 사용자행위로그목록응답
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<Result<사용자행위로그상세응답>> 상세Async(long id, CancellationToken cancellationToken)
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

        return item is null
            ? NotFound<사용자행위로그상세응답>("사용자 행위 로그를 찾을 수 없습니다.")
            : Result.Ok(item);
    }

    public async Task<Result<Trace행위로그묶음응답>> Trace조회Async(
        string? traceId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(traceId))
        {
            return Result.Fail<Trace행위로그묶음응답>("traceId is required");
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

        return Result.Ok(new Trace행위로그묶음응답
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
        return digits.Length <= 4 ? digits : digits[^4..];
    }

    private static Result<T> NotFound<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));
}
