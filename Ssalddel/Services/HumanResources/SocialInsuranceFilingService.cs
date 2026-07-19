using System.Collections.Concurrent;
using Ssalddel.Contracts.Common.Hr;

namespace Ssalddel.Services.HumanResources;

public interface ISocialInsuranceFilingService
{
    Task<SocialInsuranceEligibilityAssessmentResponse> AssessAsync(
        SocialInsuranceEligibilityAssessmentRequest request,
        CancellationToken cancellationToken = default);

    Task<SocialInsuranceFilingPlanResponse> CreatePlanAsync(
        SocialInsuranceFilingPlanCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SocialInsuranceFilingPlanResponse>> ListAsync(
        string? workerUserId,
        string? employerScopeType,
        string? employerScopeId,
        string? filingStatus,
        CancellationToken cancellationToken = default);

    Task<SocialInsuranceFilingPlanResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SocialInsuranceFilingPlanResponse> UpdateStatusAsync(
        Guid id,
        SocialInsuranceFilingStatusUpdateRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class InMemorySocialInsuranceFilingService : ISocialInsuranceFilingService
{
    private const decimal NationalPensionMonthlyIncomeThreshold = 2_200_000m;
    private readonly ConcurrentDictionary<Guid, SocialInsuranceFilingPlanResponse> _plans = new();

    public Task<SocialInsuranceEligibilityAssessmentResponse> AssessAsync(
        SocialInsuranceEligibilityAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateAssessment(request);
        return Task.FromResult(BuildAssessment(request));
    }

    public Task<SocialInsuranceFilingPlanResponse> CreatePlanAsync(
        SocialInsuranceFilingPlanCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request.Assessment is null)
        {
            throw new ArgumentException("Assessment is required.", nameof(request));
        }

        var assessment = BuildAssessment(request.Assessment);
        var selectedTypes = NormalizeSelectedInsuranceTypes(request.SelectedInsuranceTypes);
        var items = selectedTypes.Count == 0
            ? assessment.Items
            : assessment.Items.Where(x => selectedTypes.Contains(x.InsuranceType, StringComparer.OrdinalIgnoreCase)).ToArray();

        if (items.Count == 0)
        {
            throw new ArgumentException("At least one insurance item must be selected.", nameof(request));
        }

        var filingChannel = ResolvePlanChannel(items, request.Assessment.PreferEdi);
        var status = ResolvePlanStatus(items, filingChannel);
        var requiredActions = MergeRequiredActions(items, status);
        var now = DateTimeOffset.UtcNow;
        var plan = new SocialInsuranceFilingPlanResponse
        {
            Id = Guid.NewGuid(),
            EmploymentContractId = assessment.EmploymentContractId,
            WorkerUserId = assessment.WorkerUserId,
            WorkerName = assessment.WorkerName,
            EmployerScopeType = assessment.EmployerScopeType,
            EmployerScopeId = assessment.EmployerScopeId,
            EmployerName = assessment.EmployerName,
            FilingChannel = filingChannel,
            FilingStatus = status,
            DueDate = request.DueDate,
            Items = items,
            RequiredActionCodes = requiredActions,
            PreparedByUserId = NormalizeOptional(request.PreparedByUserId, "system"),
            PreparedAtUtc = now,
            Memo = NormalizeOptional(request.Memo),
            UpdatedAtUtc = now
        };

        _plans[plan.Id] = plan;
        return Task.FromResult(plan);
    }

    public Task<IReadOnlyList<SocialInsuranceFilingPlanResponse>> ListAsync(
        string? workerUserId,
        string? employerScopeType,
        string? employerScopeId,
        string? filingStatus,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = _plans.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(workerUserId))
        {
            query = query.Where(x => string.Equals(x.WorkerUserId, workerUserId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(employerScopeType))
        {
            query = query.Where(x => string.Equals(x.EmployerScopeType, employerScopeType.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(employerScopeId))
        {
            query = query.Where(x => string.Equals(x.EmployerScopeId, employerScopeId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filingStatus))
        {
            query = query.Where(x => string.Equals(x.FilingStatus, filingStatus.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<SocialInsuranceFilingPlanResponse>>(
            query
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenByDescending(x => x.PreparedAtUtc)
                .ToArray());
    }

    public Task<SocialInsuranceFilingPlanResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _plans.TryGetValue(id, out var plan);
        return Task.FromResult(plan);
    }

    public Task<SocialInsuranceFilingPlanResponse> UpdateStatusAsync(
        Guid id,
        SocialInsuranceFilingStatusUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var existing = _plans.TryGetValue(id, out var plan)
            ? plan
            : throw new InvalidOperationException("Social insurance filing plan was not found.");

        var normalizedStatus = NormalizeStatus(request.FilingStatus);
        var now = DateTimeOffset.UtcNow;
        var updated = new SocialInsuranceFilingPlanResponse
        {
            Id = existing.Id,
            EmploymentContractId = existing.EmploymentContractId,
            WorkerUserId = existing.WorkerUserId,
            WorkerName = existing.WorkerName,
            EmployerScopeType = existing.EmployerScopeType,
            EmployerScopeId = existing.EmployerScopeId,
            EmployerName = existing.EmployerName,
            FilingChannel = existing.FilingChannel,
            FilingStatus = normalizedStatus,
            DueDate = existing.DueDate,
            Items = existing.Items,
            RequiredActionCodes = ResolveStatusRequiredActions(normalizedStatus),
            PreparedByUserId = existing.PreparedByUserId,
            PreparedAtUtc = existing.PreparedAtUtc,
            SubmittedByUserId = ResolveSubmittedBy(existing, request, normalizedStatus),
            SubmittedAtUtc = ResolveSubmittedAt(existing, normalizedStatus, now),
            SubmissionReferenceNumber = NormalizeOptional(request.SubmissionReferenceNumber, existing.SubmissionReferenceNumber),
            RejectionReason = normalizedStatus == SocialInsuranceFilingStatusCodes.Rejected
                ? NormalizeOptional(request.RejectionReason, "Rejected by filing channel.")
                : string.Empty,
            Memo = MergeMemo(existing.Memo, request.Memo),
            UpdatedAtUtc = now
        };

        _plans[id] = updated;
        return Task.FromResult(updated);
    }

    private static SocialInsuranceEligibilityAssessmentResponse BuildAssessment(
        SocialInsuranceEligibilityAssessmentRequest request)
    {
        ValidateAssessment(request);

        var context = AssessmentContext.From(request);
        var items = new[]
        {
            AssessHealthInsurance(context),
            AssessNationalPension(context),
            AssessEmploymentInsurance(context),
            AssessIndustrialAccidentInsurance(context)
        };

        var overallStatus = ResolveAssessmentStatus(items, request.PreferEdi);
        return new SocialInsuranceEligibilityAssessmentResponse
        {
            EmploymentContractId = request.EmploymentContractId,
            WorkerUserId = NormalizeRequired(request.WorkerUserId, nameof(request.WorkerUserId)),
            WorkerName = NormalizeOptional(request.WorkerName),
            EmployerScopeType = NormalizeOptional(request.EmployerScopeType, HrScopeTypes.Platform),
            EmployerScopeId = NormalizeOptional(request.EmployerScopeId, HrScopeIds.Global),
            EmployerName = NormalizeOptional(request.EmployerName),
            OverallStatus = overallStatus,
            Items = items,
            RequiredActionCodes = MergeRequiredActions(items, overallStatus),
            AssessedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private static SocialInsuranceEligibilityItem AssessHealthInsurance(AssessmentContext context)
    {
        var blocked = BuildEmployerOrContractorReview(context, SocialInsuranceTypeCodes.HealthInsurance);
        if (blocked is not null)
        {
            return blocked;
        }

        if (!context.DurationAtLeastOneMonth)
        {
            return NotRequired(
                SocialInsuranceTypeCodes.HealthInsurance,
                "EmploymentUnderOneMonth",
                "Health insurance workplace enrollment is usually not prepared for employment under one month.");
        }

        if (context.HasHighWorkHours)
        {
            return Required(
                SocialInsuranceTypeCodes.HealthInsurance,
                context,
                "MonthlyWorkHoursAtLeast60",
                "Prepare workplace subscriber filing through EDI or manual filing.");
        }

        return ManualReview(
            SocialInsuranceTypeCodes.HealthInsurance,
            context,
            ["ShortTimeWorkerReview"],
            "Short-time worker cases need a labor and insurance review before excluding filing.");
    }

    private static SocialInsuranceEligibilityItem AssessNationalPension(AssessmentContext context)
    {
        var blocked = BuildEmployerOrContractorReview(context, SocialInsuranceTypeCodes.NationalPension);
        if (blocked is not null)
        {
            return blocked;
        }

        if (!context.DurationAtLeastOneMonth)
        {
            return ManualReview(
                SocialInsuranceTypeCodes.NationalPension,
                context,
                ["EmploymentUnderOneMonthReview"],
                "National pension one-month and month-end criteria should be checked before filing is skipped.");
        }

        if (context.HasHighWorkHours
            || (context.ExpectedMonthlyWorkDays.HasValue && context.ExpectedMonthlyWorkDays.Value >= 8)
            || (context.ExpectedMonthlyWage.HasValue && context.ExpectedMonthlyWage.Value >= NationalPensionMonthlyIncomeThreshold)
            || context.MultipleWorkplacesTotalMonthlyHoursAtLeast60
            || context.WorkerWantsNationalPensionWhenShortTime)
        {
            return Required(
                SocialInsuranceTypeCodes.NationalPension,
                context,
                "PensionWorkPatternMatches",
                "Prepare workplace subscriber filing through EDI or manual filing.");
        }

        return ManualReview(
            SocialInsuranceTypeCodes.NationalPension,
            context,
            ["ShortTimeWorkerReview"],
            "Short-time pension exclusions and voluntary workplace enrollment cases need confirmation.");
    }

    private static SocialInsuranceEligibilityItem AssessEmploymentInsurance(AssessmentContext context)
    {
        var blocked = BuildEmployerOrContractorReview(context, SocialInsuranceTypeCodes.EmploymentInsurance);
        if (blocked is not null)
        {
            return blocked;
        }

        if (context.IsDailyWorker || context.HasHighWorkHours || context.DurationAtLeastThreeMonths)
        {
            return Required(
                SocialInsuranceTypeCodes.EmploymentInsurance,
                context,
                "EmploymentInsuranceWorkPatternMatches",
                "Prepare employment insurance filing through EDI or manual filing.");
        }

        if (context.ExpectedMonthlyWorkHours is null && context.ExpectedWeeklyWorkHours is null)
        {
            return ManualReview(
                SocialInsuranceTypeCodes.EmploymentInsurance,
                context,
                ["WorkHoursUnknown"],
                "Expected weekly or monthly work hours are needed before filing is skipped.");
        }

        return NotRequired(
            SocialInsuranceTypeCodes.EmploymentInsurance,
            "ShortTimeUnderThreeMonths",
            "Short-time employment under three months is generally treated as an exclusion candidate.");
    }

    private static SocialInsuranceEligibilityItem AssessIndustrialAccidentInsurance(AssessmentContext context)
    {
        var blocked = BuildEmployerOrContractorReview(context, SocialInsuranceTypeCodes.IndustrialAccidentInsurance);
        if (blocked is not null)
        {
            return blocked;
        }

        return Required(
            SocialInsuranceTypeCodes.IndustrialAccidentInsurance,
            context,
            "WorkerCoveredByIndustrialAccidentInsurance",
            "Prepare industrial accident insurance administration together with the worker filing checklist.");
    }

    private static SocialInsuranceEligibilityItem? BuildEmployerOrContractorReview(
        AssessmentContext context,
        string insuranceType)
    {
        if (context.IsContractor)
        {
            return ManualReview(
                insuranceType,
                context,
                ["ContractorRelationshipReview"],
                "Contractor and worker classification must be reviewed before social insurance filing is prepared.");
        }

        if (!context.EmployerCanEmployWorkers || !context.EmployerHasBusinessRegistration)
        {
            return ManualReview(
                insuranceType,
                context,
                ["EmployerEntityReview"],
                "Confirm that the orderer group or delegated operator can act as employer before filing.");
        }

        return null;
    }

    private static SocialInsuranceEligibilityItem Required(
        string insuranceType,
        AssessmentContext context,
        string reasonCode,
        string note)
    {
        return new SocialInsuranceEligibilityItem
        {
            InsuranceType = insuranceType,
            Decision = SocialInsuranceEligibilityDecisionCodes.Required,
            RecommendedFilingChannel = context.PreferEdi
                ? SocialInsuranceFilingChannelCodes.Edi
                : SocialInsuranceFilingChannelCodes.Manual,
            ReasonCodes = [reasonCode],
            RequiredActionCodes = context.PreferEdi
                ? [SocialInsuranceFilingRequiredActionCodes.PrepareEdiSubmission]
                : [SocialInsuranceFilingRequiredActionCodes.PrepareManualSubmission],
            Note = note
        };
    }

    private static SocialInsuranceEligibilityItem NotRequired(
        string insuranceType,
        string reasonCode,
        string note)
    {
        return new SocialInsuranceEligibilityItem
        {
            InsuranceType = insuranceType,
            Decision = SocialInsuranceEligibilityDecisionCodes.NotRequired,
            RecommendedFilingChannel = SocialInsuranceFilingChannelCodes.Manual,
            ReasonCodes = [reasonCode],
            RequiredActionCodes = [],
            Note = note
        };
    }

    private static SocialInsuranceEligibilityItem ManualReview(
        string insuranceType,
        AssessmentContext context,
        IReadOnlyList<string> reasonCodes,
        string note)
    {
        var actions = new List<string>
        {
            SocialInsuranceFilingRequiredActionCodes.ReviewLaborRules,
            SocialInsuranceFilingRequiredActionCodes.ConfirmWorkPattern
        };

        if (!context.EmployerCanEmployWorkers)
        {
            actions.Add(SocialInsuranceFilingRequiredActionCodes.ConfirmEmployerEntity);
        }

        if (!context.EmployerHasBusinessRegistration)
        {
            actions.Add(SocialInsuranceFilingRequiredActionCodes.ConfirmBusinessRegistration);
        }

        return new SocialInsuranceEligibilityItem
        {
            InsuranceType = insuranceType,
            Decision = SocialInsuranceEligibilityDecisionCodes.ManualReviewRequired,
            RecommendedFilingChannel = SocialInsuranceFilingChannelCodes.Manual,
            ReasonCodes = reasonCodes,
            RequiredActionCodes = actions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            Note = note
        };
    }

    private static string ResolveAssessmentStatus(
        IReadOnlyCollection<SocialInsuranceEligibilityItem> items,
        bool preferEdi)
    {
        if (items.Any(x => x.Decision == SocialInsuranceEligibilityDecisionCodes.ManualReviewRequired))
        {
            return SocialInsuranceFilingStatusCodes.ManualReviewRequired;
        }

        return preferEdi
            ? SocialInsuranceFilingStatusCodes.EdiPreparationReady
            : SocialInsuranceFilingStatusCodes.ManualPreparationReady;
    }

    private static string ResolvePlanChannel(
        IReadOnlyCollection<SocialInsuranceEligibilityItem> items,
        bool preferEdi)
    {
        if (!preferEdi || items.Any(x => x.Decision == SocialInsuranceEligibilityDecisionCodes.ManualReviewRequired))
        {
            return SocialInsuranceFilingChannelCodes.Manual;
        }

        return SocialInsuranceFilingChannelCodes.Edi;
    }

    private static string ResolvePlanStatus(
        IReadOnlyCollection<SocialInsuranceEligibilityItem> items,
        string filingChannel)
    {
        if (items.Any(x => x.Decision == SocialInsuranceEligibilityDecisionCodes.ManualReviewRequired))
        {
            return SocialInsuranceFilingStatusCodes.ManualReviewRequired;
        }

        return filingChannel == SocialInsuranceFilingChannelCodes.Edi
            ? SocialInsuranceFilingStatusCodes.EdiPreparationReady
            : SocialInsuranceFilingStatusCodes.ManualPreparationReady;
    }

    private static IReadOnlyList<string> MergeRequiredActions(
        IReadOnlyCollection<SocialInsuranceEligibilityItem> items,
        string status)
    {
        var actions = items
            .SelectMany(x => x.RequiredActionCodes)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (status is SocialInsuranceFilingStatusCodes.SubmittedByEdi
            or SocialInsuranceFilingStatusCodes.SubmittedManually)
        {
            actions.Add(SocialInsuranceFilingRequiredActionCodes.UpdateSubmissionResult);
        }

        return actions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<string> ResolveStatusRequiredActions(string status)
    {
        return status switch
        {
            SocialInsuranceFilingStatusCodes.SubmittedByEdi
                or SocialInsuranceFilingStatusCodes.SubmittedManually
                => [SocialInsuranceFilingRequiredActionCodes.UpdateSubmissionResult],
            SocialInsuranceFilingStatusCodes.ManualReviewRequired
                => [SocialInsuranceFilingRequiredActionCodes.ReviewLaborRules],
            SocialInsuranceFilingStatusCodes.EdiPreparationReady
                => [SocialInsuranceFilingRequiredActionCodes.PrepareEdiSubmission],
            SocialInsuranceFilingStatusCodes.ManualPreparationReady
                => [SocialInsuranceFilingRequiredActionCodes.PrepareManualSubmission],
            _ => []
        };
    }

    private static HashSet<string> NormalizeSelectedInsuranceTypes(IReadOnlyList<string>? values)
    {
        return values is null
            ? []
            : values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeInsuranceType)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeInsuranceType(string value)
    {
        return value.Trim() switch
        {
            SocialInsuranceTypeCodes.NationalPension => SocialInsuranceTypeCodes.NationalPension,
            SocialInsuranceTypeCodes.EmploymentInsurance => SocialInsuranceTypeCodes.EmploymentInsurance,
            SocialInsuranceTypeCodes.IndustrialAccidentInsurance => SocialInsuranceTypeCodes.IndustrialAccidentInsurance,
            _ => SocialInsuranceTypeCodes.HealthInsurance
        };
    }

    private static string NormalizeStatus(string value)
    {
        return NormalizeOptional(value) switch
        {
            SocialInsuranceFilingStatusCodes.EdiPreparationReady => SocialInsuranceFilingStatusCodes.EdiPreparationReady,
            SocialInsuranceFilingStatusCodes.ManualPreparationReady => SocialInsuranceFilingStatusCodes.ManualPreparationReady,
            SocialInsuranceFilingStatusCodes.SubmittedManually => SocialInsuranceFilingStatusCodes.SubmittedManually,
            SocialInsuranceFilingStatusCodes.Accepted => SocialInsuranceFilingStatusCodes.Accepted,
            SocialInsuranceFilingStatusCodes.Rejected => SocialInsuranceFilingStatusCodes.Rejected,
            SocialInsuranceFilingStatusCodes.ManualReviewRequired => SocialInsuranceFilingStatusCodes.ManualReviewRequired,
            SocialInsuranceFilingStatusCodes.Cancelled => SocialInsuranceFilingStatusCodes.Cancelled,
            _ => SocialInsuranceFilingStatusCodes.SubmittedByEdi
        };
    }

    private static string? ResolveSubmittedBy(
        SocialInsuranceFilingPlanResponse existing,
        SocialInsuranceFilingStatusUpdateRequest request,
        string status)
    {
        if (status is SocialInsuranceFilingStatusCodes.SubmittedByEdi
            or SocialInsuranceFilingStatusCodes.SubmittedManually)
        {
            return NormalizeOptional(request.SubmittedByUserId, "system");
        }

        return existing.SubmittedByUserId;
    }

    private static DateTimeOffset? ResolveSubmittedAt(
        SocialInsuranceFilingPlanResponse existing,
        string status,
        DateTimeOffset now)
    {
        if (status is SocialInsuranceFilingStatusCodes.SubmittedByEdi
            or SocialInsuranceFilingStatusCodes.SubmittedManually)
        {
            return existing.SubmittedAtUtc ?? now;
        }

        return existing.SubmittedAtUtc;
    }

    private static string MergeMemo(string existing, string next)
    {
        if (string.IsNullOrWhiteSpace(next))
        {
            return existing;
        }

        if (string.IsNullOrWhiteSpace(existing))
        {
            return next.Trim();
        }

        return $"{existing.Trim()}\n{next.Trim()}";
    }

    private static void ValidateAssessment(SocialInsuranceEligibilityAssessmentRequest request)
    {
        _ = NormalizeRequired(request.WorkerUserId, nameof(request.WorkerUserId));
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeOptional(string? value, string fallback = "")
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private sealed record AssessmentContext(
        string ContractType,
        decimal? ExpectedWeeklyWorkHours,
        decimal? ExpectedMonthlyWorkHours,
        int? ExpectedMonthlyWorkDays,
        decimal? ExpectedMonthlyWage,
        bool IsDailyWorker,
        bool EmployerCanEmployWorkers,
        bool EmployerHasBusinessRegistration,
        bool MultipleWorkplacesTotalMonthlyHoursAtLeast60,
        bool WorkerWantsNationalPensionWhenShortTime,
        bool PreferEdi,
        bool DurationAtLeastOneMonth,
        bool DurationAtLeastThreeMonths,
        bool HasHighWorkHours)
    {
        public bool IsContractor { get; } = ContractType == HrEmploymentContractTypes.Contractor;

        public static AssessmentContext From(SocialInsuranceEligibilityAssessmentRequest request)
        {
            var monthlyHours = request.ExpectedMonthlyWorkHours
                ?? (request.ExpectedWeeklyWorkHours.HasValue ? request.ExpectedWeeklyWorkHours.Value * 4.345m : null);
            var months = ResolveExpectedEmploymentMonths(request);
            var weeklyHours = request.ExpectedWeeklyWorkHours;
            var hasHighWorkHours =
                (monthlyHours.HasValue && monthlyHours.Value >= 60m)
                || (weeklyHours.HasValue && weeklyHours.Value >= 15m);

            return new AssessmentContext(
                NormalizeContractType(request.ContractType),
                request.ExpectedWeeklyWorkHours,
                monthlyHours,
                request.ExpectedMonthlyWorkDays,
                request.ExpectedMonthlyWage,
                request.IsDailyWorker,
                request.EmployerCanEmployWorkers,
                request.EmployerHasBusinessRegistration,
                request.MultipleWorkplacesTotalMonthlyHoursAtLeast60,
                request.WorkerWantsNationalPensionWhenShortTime,
                request.PreferEdi,
                months >= 1,
                months >= 3,
                hasHighWorkHours);
        }

        private static int ResolveExpectedEmploymentMonths(SocialInsuranceEligibilityAssessmentRequest request)
        {
            if (request.ExpectedEmploymentMonths.HasValue)
            {
                return Math.Max(0, request.ExpectedEmploymentMonths.Value);
            }

            if (request.ContractEndDate.HasValue && request.ContractStartDate != default)
            {
                var days = request.ContractEndDate.Value.DayNumber - request.ContractStartDate.DayNumber + 1;
                return days <= 0 ? 0 : (int)Math.Ceiling(days / 30m);
            }

            return request.ContractStartDate == default ? 0 : 1;
        }

        private static string NormalizeContractType(string? value)
        {
            return value?.Trim() switch
            {
                HrEmploymentContractTypes.FixedTerm => HrEmploymentContractTypes.FixedTerm,
                HrEmploymentContractTypes.Regular => HrEmploymentContractTypes.Regular,
                HrEmploymentContractTypes.Contractor => HrEmploymentContractTypes.Contractor,
                _ => HrEmploymentContractTypes.PartTime
            };
        }
    }
}
