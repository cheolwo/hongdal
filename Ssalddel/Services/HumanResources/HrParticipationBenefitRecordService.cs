using System.Collections.Concurrent;
using Ssalddel.Contracts.Common.Hr;

namespace Ssalddel.Services.HumanResources;

public interface IHrParticipationBenefitRecordService
{
    Task<IReadOnlyList<HrParticipationBenefitRecordResponse>> ListAsync(
        string? userId,
        string? sourceType,
        CancellationToken cancellationToken = default);

    Task<HrParticipationBenefitRecordResponse> TransferAsync(
        HrParticipationBenefitTransferRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class InMemoryHrParticipationBenefitRecordService : IHrParticipationBenefitRecordService
{
    private readonly ConcurrentDictionary<Guid, HrParticipationBenefitRecordResponse> _records = new();

    public Task<IReadOnlyList<HrParticipationBenefitRecordResponse>> ListAsync(
        string? userId,
        string? sourceType,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = _records.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(x => string.Equals(x.UserId, userId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(sourceType))
        {
            query = query.Where(x => string.Equals(x.SourceType, sourceType.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<HrParticipationBenefitRecordResponse>>(
            query
                .OrderByDescending(x => x.ParticipatedAtUtc)
                .ThenByDescending(x => x.RecordedAtUtc)
                .ToArray());
    }

    public Task<HrParticipationBenefitRecordResponse> TransferAsync(
        HrParticipationBenefitTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validate(request);

        var now = DateTimeOffset.UtcNow;
        var record = new HrParticipationBenefitRecordResponse
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId.Trim(),
            ParticipantName = request.ParticipantName.Trim(),
            SourceType = NormalizeSourceType(request.SourceType),
            SourceId = request.SourceId.Trim(),
            SourceTitle = request.SourceTitle.Trim(),
            ParticipationStatus = NormalizeParticipationStatus(request.ParticipationStatus),
            BenefitStatus = ResolveBenefitStatus(request),
            BenefitPolicyId = request.BenefitPolicyId,
            BenefitName = request.BenefitName.Trim(),
            BenefitDescription = request.BenefitDescription.Trim(),
            BenefitAmount = request.BenefitAmount,
            CurrencyCode = NormalizeCurrencyCode(request.CurrencyCode),
            ParticipatedAtUtc = request.ParticipatedAtUtc ?? now,
            GrantedAtUtc = request.GrantedAtUtc ?? now,
            ExpiresAtUtc = request.ExpiresAtUtc,
            RecordedByUserId = string.IsNullOrWhiteSpace(request.RecordedByUserId) ? "system" : request.RecordedByUserId.Trim(),
            RecordedAtUtc = now,
            Memo = request.Memo.Trim()
        };

        _records[record.Id] = record;
        return Task.FromResult(record);
    }

    private static void Validate(HrParticipationBenefitTransferRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new ArgumentException("UserId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SourceId))
        {
            throw new ArgumentException("SourceId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SourceTitle))
        {
            throw new ArgumentException("SourceTitle is required.");
        }

        if (string.IsNullOrWhiteSpace(request.BenefitName))
        {
            throw new ArgumentException("BenefitName is required.");
        }
    }

    private static string NormalizeSourceType(string value)
    {
        return value.Trim() switch
        {
            HrParticipationSourceTypes.EducationCourse => HrParticipationSourceTypes.EducationCourse,
            HrParticipationSourceTypes.PlatformCommunityEvent => HrParticipationSourceTypes.PlatformCommunityEvent,
            _ => HrParticipationSourceTypes.OfflineMeeting
        };
    }

    private static string NormalizeParticipationStatus(string value)
    {
        return value.Trim() switch
        {
            HrParticipationStatuses.Registered => HrParticipationStatuses.Registered,
            HrParticipationStatuses.Attended => HrParticipationStatuses.Attended,
            HrParticipationStatuses.Cancelled => HrParticipationStatuses.Cancelled,
            _ => HrParticipationStatuses.Confirmed
        };
    }

    private static string ResolveBenefitStatus(HrParticipationBenefitTransferRequest request)
    {
        if (string.Equals(request.ParticipationStatus, HrParticipationStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            return HrParticipationBenefitStatuses.Cancelled;
        }

        return request.GrantedAtUtc is null
            ? HrParticipationBenefitStatuses.Pending
            : HrParticipationBenefitStatuses.Granted;
    }

    private static string NormalizeCurrencyCode(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "KRW" : value.Trim().ToUpperInvariant();
    }
}
