using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.ViewSettings;
using Ssalddel.Contracts.Driver.Settlement;
using 살뜰.Data;
using 살뜰.도메인.기사;
using 살뜰.도메인.설정;

namespace Ssalddel.Application.Driver.Settlement;

public interface I기사정산계좌UseCase
{
    Task<Result<기사정산계좌응답>> 조회Async(
        string? 기사Id,
        CancellationToken cancellationToken = default);

    Task<Result<기사정산계좌응답>> 저장Async(
        string? 기사Id,
        기사정산계좌수정요청? request,
        CancellationToken cancellationToken = default);

    Task<Result> 삭제Async(
        string? 기사Id,
        CancellationToken cancellationToken = default);
}

[SsalddelApiWorkflow(SsalddelWorkflow.DomesticTransport)]
[SsalddelUseCase(
    "기사 정산계좌 관리",
    Summary = "기사 본인이 정산 입금 계좌를 등록, 조회, 철회하며 원문 계좌번호는 저장 경계에서 암호화합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Driver)]
public sealed class 기사정산계좌UseCase : I기사정산계좌UseCase
{
    private const string Route = "api/v1/driver/settlement-account";
    private readonly SsalddelContext _db;

    public 기사정산계좌UseCase(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<Result<기사정산계좌응답>> 조회Async(
        string? 기사Id,
        CancellationToken cancellationToken = default)
    {
        var driverResult = await 기사확인Async(기사Id, cancellationToken);
        if (driverResult.IsFailed)
        {
            return Result.Fail<기사정산계좌응답>(driverResult.Errors);
        }

        var account = await _db.Set<기사정산계좌>()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.기사Id == 기사Id, cancellationToken);

        return Result.Ok(응답생성(기사Id!, account));
    }

    public async Task<Result<기사정산계좌응답>> 저장Async(
        string? 기사Id,
        기사정산계좌수정요청? request,
        CancellationToken cancellationToken = default)
    {
        var driverResult = await 기사확인Async(기사Id, cancellationToken);
        if (driverResult.IsFailed)
        {
            return Result.Fail<기사정산계좌응답>(driverResult.Errors);
        }

        var validation = 요청검증(request);
        if (validation.IsFailed)
        {
            return Result.Fail<기사정산계좌응답>(validation.Errors);
        }

        var normalizedCountryCode = request!.CountryCode.Trim().ToUpperInvariant();
        var normalizedAccountNumber = request.AccountNumber.Trim();
        var now = DateTime.UtcNow;
        var account = await _db.Set<기사정산계좌>()
            .SingleOrDefaultAsync(x => x.기사Id == 기사Id, cancellationToken);

        if (account is null)
        {
            account = new 기사정산계좌
            {
                기사Id = 기사Id!,
                CreatedAtUtc = now
            };
            _db.Set<기사정산계좌>().Add(account);
        }

        account.국가코드 = normalizedCountryCode;
        account.은행명 = request.BankName.Trim();
        account.예금주명 = request.AccountHolderName.Trim();
        account.계좌번호 = normalizedAccountNumber;
        // 계좌가 바뀌면 외부 실명 확인 전까지 다시 미확인 상태로 둡니다.
        account.확인상태 = 기사정산계좌확인상태.미확인;
        account.UpdatedAtUtc = now;

        감사로그추가(기사Id!, "Update", "기사 정산계좌 등록 또는 변경", normalizedCountryCode, now);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(응답생성(기사Id!, account));
    }

    public async Task<Result> 삭제Async(
        string? 기사Id,
        CancellationToken cancellationToken = default)
    {
        var driverResult = await 기사확인Async(기사Id, cancellationToken);
        if (driverResult.IsFailed)
        {
            return Result.Fail(driverResult.Errors);
        }

        var account = await _db.Set<기사정산계좌>()
            .SingleOrDefaultAsync(x => x.기사Id == 기사Id, cancellationToken);
        if (account is null)
        {
            return Result.Ok();
        }

        _db.Set<기사정산계좌>().Remove(account);
        감사로그추가(기사Id!, "Delete", "기사 정산계좌 삭제", account.국가코드, DateTime.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task<Result> 기사확인Async(
        string? 기사Id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(기사Id))
        {
            return 상태실패("기사 인증 정보가 없습니다.", StatusCodes.Status401Unauthorized);
        }

        var exists = await _db.용달기사
            .AsNoTracking()
            .AnyAsync(x => x.기사Id == 기사Id, cancellationToken);
        return exists
            ? Result.Ok()
            : 상태실패("용달기사 정보를 찾을 수 없습니다.", StatusCodes.Status404NotFound);
    }

    private static Result 요청검증(기사정산계좌수정요청? request)
    {
        if (request is null)
        {
            return 상태실패("request body is required", StatusCodes.Status400BadRequest);
        }

        if (!request.개인정보저장동의)
        {
            return 상태실패("정산계좌 개인정보 저장 동의가 필요합니다.", StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.CountryCode) ||
            request.CountryCode.Trim().Length != 2 ||
            !request.CountryCode.Trim().All(char.IsLetter))
        {
            return 상태실패("CountryCode는 영문 ISO 2자리 국가코드여야 합니다.", StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.BankName) || request.BankName.Trim().Length > 100)
        {
            return 상태실패("BankName은 1자 이상 100자 이하여야 합니다.", StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.AccountHolderName) || request.AccountHolderName.Trim().Length > 100)
        {
            return 상태실패("AccountHolderName은 1자 이상 100자 이하여야 합니다.", StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.AccountNumber) || request.AccountNumber.Trim().Length > 200)
        {
            return 상태실패("AccountNumber는 1자 이상 200자 이하여야 합니다.", StatusCodes.Status400BadRequest);
        }

        var identifierLength = request.AccountNumber.Count(char.IsLetterOrDigit);
        return identifierLength >= 4
            ? Result.Ok()
            : 상태실패("AccountNumber는 식별 가능한 문자 4자 이상이어야 합니다.", StatusCodes.Status400BadRequest);
    }

    private static 기사정산계좌응답 응답생성(string 기사Id, 기사정산계좌? account)
    {
        if (account is null)
        {
            return new 기사정산계좌응답
            {
                DriverId = 기사Id,
                HasAccount = false
            };
        }

        return new 기사정산계좌응답
        {
            DriverId = 기사Id,
            HasAccount = true,
            CountryCode = account.국가코드,
            BankName = account.은행명,
            AccountHolderName = account.예금주명,
            MaskedAccountNumber = 계좌번호마스킹(account.계좌번호),
            VerificationStatus = account.확인상태,
            UpdatedAtUtc = account.UpdatedAtUtc
        };
    }

    private static string 계좌번호마스킹(string accountNumber)
    {
        var compact = new string(accountNumber.Where(char.IsLetterOrDigit).ToArray());
        if (compact.Length == 0)
        {
            return string.Empty;
        }

        var visibleLength = Math.Min(4, compact.Length);
        return $"****{compact[^visibleLength..]}";
    }

    private void 감사로그추가(
        string 기사Id,
        string actionType,
        string actionName,
        string countryCode,
        DateTime occurredAtUtc)
    {
        _db.사용자행위로그.Add(new 사용자행위로그
        {
            AppKey = App식별자.DriverApp,
            UserId = 기사Id,
            RoleName = 역할명.기사,
            ActionType = actionType,
            ActionName = actionName,
            Route = Route,
            IsSuccess = true,
            MetadataJson = $"{{\"countryCode\":\"{countryCode}\"}}",
            OccurredAtUtc = occurredAtUtc
        });
    }

    private static Result 상태실패(string message, int statusCode)
        => Result.Fail(new Error(message).WithMetadata("StatusCode", statusCode));
}
