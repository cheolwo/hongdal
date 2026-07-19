using Ssalddel.Contracts.Common.Hr;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;
using 살뜰.도메인.사용자;

namespace Ssalddel.Services.HumanResources;

public interface IHrEmploymentContractService
{
    Task<HrEmploymentContractResponse> CreateDraftAsync(HrEmploymentContractDraftRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<HrEmploymentContractResponse>> ListAsync(string? workerUserId, string? employerScopeType, string? employerScopeId, CancellationToken cancellationToken);
    Task<HrEmploymentContractResponse?> GetAsync(Guid contractId, CancellationToken cancellationToken);
    Task<HrEmploymentContractResponse> SignAsync(Guid contractId, string signedByUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<HrPayrollScheduleResponse>> CreatePayrollSchedulesAsync(Guid contractId, DateOnly scheduleStartDate, DateOnly scheduleEndDate, CancellationToken cancellationToken);
}

public sealed class HrEmploymentContractService : IHrEmploymentContractService
{
    private readonly SsalddelContext _db;

    public HrEmploymentContractService(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<HrEmploymentContractResponse> CreateDraftAsync(HrEmploymentContractDraftRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var minimumWageDecision = EvaluateMinimumWage(request.WageType, request.WageAmount, request.MinimumWageAmount);

        var record = new HrEmploymentContractRecord
        {
            Id = Guid.NewGuid(),
            WorkerUserId = NormalizeRequired(request.WorkerUserId, nameof(request.WorkerUserId)),
            WorkerName = NormalizeOptional(request.WorkerName),
            EmployerScopeType = NormalizeOptional(request.EmployerScopeType, HrScopeTypes.Platform),
            EmployerScopeId = NormalizeOptional(request.EmployerScopeId, HrScopeIds.Global),
            EmployerName = NormalizeOptional(request.EmployerName),
            ContractType = NormalizeContractType(request.ContractType),
            ContractStatus = HrEmploymentContractStatuses.Draft,
            ContractStartDate = request.ContractStartDate == default ? DateOnly.FromDateTime(DateTime.Today) : request.ContractStartDate,
            ContractEndDate = request.ContractEndDate,
            WorkDescription = NormalizeOptional(request.WorkDescription),
            WageType = NormalizeWageType(request.WageType),
            WageAmount = request.WageAmount,
            MinimumWageAmount = request.MinimumWageAmount,
            MinimumWageCheckPassed = minimumWageDecision.Passed,
            MinimumWageCheckMessage = minimumWageDecision.Message,
            PaymentCycle = NormalizePaymentCycle(request.PaymentCycle),
            PaymentDayOfMonth = ClampPaymentDay(request.PaymentDayOfMonth),
            PaymentMethod = NormalizePaymentMethod(request.PaymentMethod),
            BankName = NormalizeOptional(request.BankName),
            AccountNumber = NormalizeOptional(request.AccountNumber),
            AccountHolderName = NormalizeOptional(request.AccountHolderName),
            Memo = NormalizeOptional(request.Memo),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.HrEmploymentContracts.Add(record);
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(record);
    }

    public async Task<IReadOnlyList<HrEmploymentContractResponse>> ListAsync(string? workerUserId, string? employerScopeType, string? employerScopeId, CancellationToken cancellationToken)
    {
        var query = _db.HrEmploymentContracts
            .AsNoTracking()
            .Include(x => x.PayrollSchedules)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(workerUserId))
        {
            var normalizedWorkerUserId = workerUserId.Trim();
            query = query.Where(x => x.WorkerUserId == normalizedWorkerUserId);
        }

        if (!string.IsNullOrWhiteSpace(employerScopeType))
        {
            var normalizedScopeType = employerScopeType.Trim();
            query = query.Where(x => x.EmployerScopeType == normalizedScopeType);
        }

        if (!string.IsNullOrWhiteSpace(employerScopeId))
        {
            var normalizedScopeId = employerScopeId.Trim();
            query = query.Where(x => x.EmployerScopeId == normalizedScopeId);
        }

        var records = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(200)
            .ToArrayAsync(cancellationToken);

        return records.Select(ToResponse).ToArray();
    }

    public async Task<HrEmploymentContractResponse?> GetAsync(Guid contractId, CancellationToken cancellationToken)
    {
        var record = await _db.HrEmploymentContracts
            .AsNoTracking()
            .Include(x => x.PayrollSchedules)
            .FirstOrDefaultAsync(x => x.Id == contractId, cancellationToken);

        return record is null ? null : ToResponse(record);
    }

    public async Task<HrEmploymentContractResponse> SignAsync(Guid contractId, string signedByUserId, CancellationToken cancellationToken)
    {
        var record = await _db.HrEmploymentContracts
            .Include(x => x.PayrollSchedules)
            .FirstOrDefaultAsync(x => x.Id == contractId, cancellationToken)
            ?? throw new InvalidOperationException("근로계약서를 찾을 수 없습니다.");

        if (record.ContractStatus is not HrEmploymentContractStatuses.Draft)
        {
            throw new InvalidOperationException("초안 상태의 근로계약서만 체결할 수 있습니다.");
        }

        if (!record.MinimumWageCheckPassed)
        {
            throw new InvalidOperationException($"임금 조건 확인이 필요합니다. {record.MinimumWageCheckMessage}");
        }

        record.ContractStatus = HrEmploymentContractStatuses.Signed;
        record.SignedAtUtc = DateTime.UtcNow;
        record.SignedByUserId = string.IsNullOrWhiteSpace(signedByUserId) ? "system" : signedByUserId.Trim();
        record.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(record);
    }

    public async Task<IReadOnlyList<HrPayrollScheduleResponse>> CreatePayrollSchedulesAsync(Guid contractId, DateOnly scheduleStartDate, DateOnly scheduleEndDate, CancellationToken cancellationToken)
    {
        if (scheduleEndDate < scheduleStartDate)
        {
            throw new ArgumentException("스케줄 종료일은 시작일보다 빠를 수 없습니다.", nameof(scheduleEndDate));
        }

        var contract = await _db.HrEmploymentContracts
            .Include(x => x.PayrollSchedules)
            .FirstOrDefaultAsync(x => x.Id == contractId, cancellationToken)
            ?? throw new InvalidOperationException("근로계약서를 찾을 수 없습니다.");

        if (contract.ContractStatus is HrEmploymentContractStatuses.Draft or HrEmploymentContractStatuses.Cancelled)
        {
            throw new InvalidOperationException("체결 이후의 근로계약서만 지급 스케줄을 만들 수 있습니다.");
        }

        var effectiveStart = Max(scheduleStartDate, contract.ContractStartDate);
        var effectiveEnd = contract.ContractEndDate is null ? scheduleEndDate : Min(scheduleEndDate, contract.ContractEndDate.Value);
        if (effectiveEnd < effectiveStart)
        {
            return [];
        }

        var created = new List<HrPayrollScheduleRecord>();
        foreach (var period in BuildPeriods(contract.PaymentCycle, effectiveStart, effectiveEnd))
        {
            var paymentDate = ResolvePaymentDate(period.EndDate, contract.PaymentCycle, contract.PaymentDayOfMonth);
            var duplicate = contract.PayrollSchedules.Any(x =>
                x.WorkPeriodStartDate == period.StartDate
                && x.WorkPeriodEndDate == period.EndDate
                && x.Status != HrPayrollScheduleStatuses.Cancelled);

            if (duplicate)
            {
                continue;
            }

            var record = new HrPayrollScheduleRecord
            {
                Id = Guid.NewGuid(),
                ContractId = contract.Id,
                WorkerUserId = contract.WorkerUserId,
                EmployerScopeType = contract.EmployerScopeType,
                EmployerScopeId = contract.EmployerScopeId,
                WorkPeriodStartDate = period.StartDate,
                WorkPeriodEndDate = period.EndDate,
                ScheduledPaymentDate = paymentDate,
                PlannedAmount = EstimatePlannedAmount(contract),
                CurrencyCode = "KRW",
                PaymentMethod = contract.PaymentMethod,
                Status = HrPayrollScheduleStatuses.Planned,
                Memo = CreateScheduleMemo(contract),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            created.Add(record);
            _db.HrPayrollSchedules.Add(record);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return created.Select(ToResponse).ToArray();
    }

    private static MinimumWageDecision EvaluateMinimumWage(string wageType, decimal wageAmount, decimal? minimumWageAmount)
    {
        var normalizedWageType = NormalizeWageType(wageType);
        if (wageAmount <= 0)
        {
            return new MinimumWageDecision(false, "임금 금액은 0보다 커야 합니다.");
        }

        if (normalizedWageType != HrWageTypes.Hourly)
        {
            return new MinimumWageDecision(false, "시급 외 계약은 소정근로시간 기준 환산 검토가 필요합니다.");
        }

        if (minimumWageAmount is null or <= 0)
        {
            return new MinimumWageDecision(false, "시급 계약 체결에는 기준 최저임금 설정값이 필요합니다.");
        }

        if (wageAmount < minimumWageAmount.Value)
        {
            return new MinimumWageDecision(false, $"시급 {wageAmount:N0}원이 기준 최저임금 {minimumWageAmount.Value:N0}원보다 낮습니다.");
        }

        return new MinimumWageDecision(true, "시급 기준 최저임금 확인이 완료되었습니다.");
    }

    private static IEnumerable<PayrollPeriod> BuildPeriods(string paymentCycle, DateOnly startDate, DateOnly endDate)
    {
        var current = startDate;
        while (current <= endDate)
        {
            var periodEnd = NormalizePaymentCycle(paymentCycle) switch
            {
                HrPaymentCycles.Weekly => current.AddDays(6),
                HrPaymentCycles.Biweekly => current.AddDays(13),
                _ => new DateOnly(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month))
            };

            if (periodEnd > endDate)
            {
                periodEnd = endDate;
            }

            yield return new PayrollPeriod(current, periodEnd);
            current = periodEnd.AddDays(1);
        }
    }

    private static DateOnly ResolvePaymentDate(DateOnly periodEndDate, string paymentCycle, int paymentDayOfMonth)
    {
        if (NormalizePaymentCycle(paymentCycle) is HrPaymentCycles.Weekly or HrPaymentCycles.Biweekly)
        {
            return periodEndDate.AddDays(3);
        }

        var nextMonth = periodEndDate.AddMonths(1);
        var day = Math.Min(ClampPaymentDay(paymentDayOfMonth), DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
        return new DateOnly(nextMonth.Year, nextMonth.Month, day);
    }

    private static decimal EstimatePlannedAmount(HrEmploymentContractRecord contract)
        => contract.WageType == HrWageTypes.Monthly ? contract.WageAmount : 0;

    private static string CreateScheduleMemo(HrEmploymentContractRecord contract)
        => contract.WageType == HrWageTypes.Monthly
            ? "월급 계약 기준 예정 금액입니다."
            : "근무기록 확정 후 실제 지급 금액을 계산해야 합니다.";

    private static HrEmploymentContractResponse ToResponse(HrEmploymentContractRecord record)
    {
        return new HrEmploymentContractResponse
        {
            Id = record.Id,
            WorkerUserId = record.WorkerUserId,
            WorkerName = record.WorkerName,
            EmployerScopeType = record.EmployerScopeType,
            EmployerScopeId = record.EmployerScopeId,
            EmployerName = record.EmployerName,
            ContractType = record.ContractType,
            ContractStatus = record.ContractStatus,
            ContractStartDate = record.ContractStartDate,
            ContractEndDate = record.ContractEndDate,
            WorkDescription = record.WorkDescription,
            WageType = record.WageType,
            WageAmount = record.WageAmount,
            MinimumWageAmount = record.MinimumWageAmount,
            MinimumWageCheckPassed = record.MinimumWageCheckPassed,
            MinimumWageCheckMessage = record.MinimumWageCheckMessage,
            PaymentCycle = record.PaymentCycle,
            PaymentDayOfMonth = record.PaymentDayOfMonth,
            PaymentMethod = record.PaymentMethod,
            BankName = record.BankName,
            MaskedAccountNumber = MaskAccount(record.AccountNumber),
            AccountHolderName = record.AccountHolderName,
            Memo = record.Memo,
            CreatedAtUtc = record.CreatedAtUtc,
            UpdatedAtUtc = record.UpdatedAtUtc,
            PayrollSchedules = record.PayrollSchedules.OrderBy(x => x.ScheduledPaymentDate).Select(ToResponse).ToArray()
        };
    }

    private static HrPayrollScheduleResponse ToResponse(HrPayrollScheduleRecord record)
    {
        return new HrPayrollScheduleResponse
        {
            Id = record.Id,
            ContractId = record.ContractId,
            WorkerUserId = record.WorkerUserId,
            EmployerScopeType = record.EmployerScopeType,
            EmployerScopeId = record.EmployerScopeId,
            WorkPeriodStartDate = record.WorkPeriodStartDate,
            WorkPeriodEndDate = record.WorkPeriodEndDate,
            ScheduledPaymentDate = record.ScheduledPaymentDate,
            PlannedAmount = record.PlannedAmount,
            CurrencyCode = record.CurrencyCode,
            PaymentMethod = record.PaymentMethod,
            Status = record.Status,
            Memo = record.Memo
        };
    }

    private static string MaskAccount(string accountNumber)
    {
        var digits = new string((accountNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length <= 4)
        {
            return string.IsNullOrWhiteSpace(digits) ? string.Empty : "****" + digits;
        }

        return new string('*', Math.Max(4, digits.Length - 4)) + digits[^4..];
    }

    private static string NormalizeContractType(string? value)
        => value?.Trim() switch
        {
            HrEmploymentContractTypes.FixedTerm => HrEmploymentContractTypes.FixedTerm,
            HrEmploymentContractTypes.Regular => HrEmploymentContractTypes.Regular,
            HrEmploymentContractTypes.Contractor => HrEmploymentContractTypes.Contractor,
            _ => HrEmploymentContractTypes.PartTime
        };

    private static string NormalizeWageType(string? value)
        => value?.Trim() switch
        {
            HrWageTypes.Daily => HrWageTypes.Daily,
            HrWageTypes.Monthly => HrWageTypes.Monthly,
            HrWageTypes.PerTask => HrWageTypes.PerTask,
            _ => HrWageTypes.Hourly
        };

    private static string NormalizePaymentCycle(string? value)
        => value?.Trim() switch
        {
            HrPaymentCycles.Weekly => HrPaymentCycles.Weekly,
            HrPaymentCycles.Biweekly => HrPaymentCycles.Biweekly,
            _ => HrPaymentCycles.Monthly
        };

    private static string NormalizePaymentMethod(string? value)
        => value?.Trim() switch
        {
            HrPaymentMethods.Cash => HrPaymentMethods.Cash,
            HrPaymentMethods.PlatformSettlement => HrPaymentMethods.PlatformSettlement,
            _ => HrPaymentMethods.BankTransfer
        };

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("필수값이 비어 있습니다.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeOptional(string? value, string fallback = "")
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static int ClampPaymentDay(int value)
        => Math.Clamp(value <= 0 ? 10 : value, 1, 28);

    private static DateOnly Max(DateOnly left, DateOnly right)
        => left > right ? left : right;

    private static DateOnly Min(DateOnly left, DateOnly right)
        => left < right ? left : right;

    private sealed record MinimumWageDecision(bool Passed, string Message);
    private sealed record PayrollPeriod(DateOnly StartDate, DateOnly EndDate);
}
