using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Driver.Food;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services;

namespace Hongdal.Application.Driver.Food;

public interface I배달기사월정산UseCase
{
    Task<Result<배달기사월정산응답>> 당월조회Async(
        string driverId,
        string? currentUserId,
        CancellationToken cancellationToken);

    Task<Result<배달기사월정산결제완료응답>> 결제완료처리Async(
        string driverId,
        int year,
        int month,
        string? currentUserId,
        CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.FoodDelivery)]
[HongdalUseCase("배달 기사 월정산", Summary = "배달 기사가 당월 정산과 이용료 결제 완료 상태를 조회하고 확인합니다.")]
[HongdalUseCaseActor(HongdalActor.FoodDeliveryDriver)]
[HongdalUseCaseRelation(
    HongdalUseCaseRelationKind.Extend,
    "플랫폼수익환급UseCase",
    Condition = "배달 기사 이용료, 수익 배분, 환급 일정이 플랫폼 정산으로 이어지는 경우",
    Summary = "배달 기사 월정산을 플랫폼 수익 환급과 지급 일정 흐름으로 확장합니다.")]
public sealed class 배달기사월정산UseCase : I배달기사월정산UseCase
{
    private readonly HongdalContext _db;
    private readonly I기사월정산Service _driverMonthlySettlementService;

    public 배달기사월정산UseCase(HongdalContext db, I기사월정산Service driverMonthlySettlementService)
    {
        _db = db;
        _driverMonthlySettlementService = driverMonthlySettlementService;
    }

    public async Task<Result<배달기사월정산응답>> 당월조회Async(
        string driverId,
        string? currentUserId,
        CancellationToken cancellationToken)
    {
        if (!현재기사확인(driverId, currentUserId))
        {
            return Forbidden<배달기사월정산응답>("다른 기사의 월정산은 조회할 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        var settlement = await _db.기사월정산
            .Where(x => x.기사Id == driverId && x.년도 == now.Year && x.월 == now.Month)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Ok(settlement is null
            ? new 배달기사월정산응답
            {
                기사Id = driverId,
                년도 = now.Year,
                월 = now.Month,
                배차건수 = 0,
                이용료 = 0,
                결제완료 = false
            }
            : 응답변환(settlement));
    }

    public async Task<Result<배달기사월정산결제완료응답>> 결제완료처리Async(
        string driverId,
        int year,
        int month,
        string? currentUserId,
        CancellationToken cancellationToken)
    {
        if (!현재기사확인(driverId, currentUserId))
        {
            return Forbidden<배달기사월정산결제완료응답>("다른 기사의 월정산은 변경할 수 없습니다.");
        }

        if (month < 1 || month > 12)
        {
            return Result.Fail<배달기사월정산결제완료응답>("month must be between 1 and 12");
        }

        var settlement = await _driverMonthlySettlementService.월말청구결제완료처리Async(driverId, year, month, DateTime.UtcNow);

        return Result.Ok(new 배달기사월정산결제완료응답
        {
            기사Id = settlement.기사Id,
            년도 = settlement.년도,
            월 = settlement.월,
            배차건수 = settlement.배차건수,
            차감이용료 = settlement.이용료,
            결제완료 = settlement.결제완료,
            처리일시Utc = settlement.UpdatedAt
        });
    }

    private static 배달기사월정산응답 응답변환(홍달.도메인.기사.기사월정산 settlement)
    {
        return new 배달기사월정산응답
        {
            기사Id = settlement.기사Id,
            년도 = settlement.년도,
            월 = settlement.월,
            배차건수 = settlement.배차건수,
            이용료 = settlement.이용료,
            결제완료 = settlement.결제완료
        };
    }

    private static bool 현재기사확인(string driverId, string? currentUserId)
        => !string.IsNullOrWhiteSpace(currentUserId)
           && string.Equals(currentUserId, driverId, StringComparison.Ordinal);

    private static Result<T> Forbidden<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status403Forbidden));
}
