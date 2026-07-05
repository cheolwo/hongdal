using System.Net;
using Hongdal.Application.HumanResources;
using Hongdal.Contracts.Common.Hr;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.사용자;

namespace Hongdal.Services.HumanResources;

public sealed class EfCoreHrRoleAssignmentStore : IHrRoleAssignmentStore
{
    private readonly HongdalContext _db;

    public EfCoreHrRoleAssignmentStore(HongdalContext db)
    {
        _db = db;
    }

    public async Task<HrRoleAssignment> AssignAsync(
        string userId,
        string scopeType,
        string scopeId,
        string participantCategory,
        string roleCode,
        string roleName,
        string assignedByUserId,
        bool workScheduleEnabled,
        string timeZoneId,
        IReadOnlyList<DayOfWeek> allowedDaysOfWeek,
        TimeOnly? workStartLocalTime,
        TimeOnly? workEndLocalTime,
        bool worksiteIpRestrictionEnabled,
        IReadOnlyList<string> allowedWorksiteIpRanges,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = NormalizeRequired(userId, nameof(userId));
        var normalizedScopeType = NormalizeOptional(scopeType, HrScopeTypes.Platform);
        var normalizedScopeId = NormalizeOptional(scopeId, HrScopeIds.Global);
        var normalizedParticipantCategory = HrParticipantCategoryCodes.Normalize(participantCategory);
        var normalizedRoleCode = NormalizeRequired(roleCode, nameof(roleCode));

        var existing = await _db.HrRoleAssignments.FirstOrDefaultAsync(x =>
            x.IsActive
            && x.UserId == normalizedUserId
            && x.ScopeType == normalizedScopeType
            && x.ScopeId == normalizedScopeId
            && x.ParticipantCategory == normalizedParticipantCategory
            && x.RoleCode == normalizedRoleCode,
            cancellationToken);

        if (existing is not null)
        {
            existing.RoleName = string.IsNullOrWhiteSpace(roleName) ? normalizedRoleCode : roleName.Trim();
            existing.WorkScheduleEnabled = workScheduleEnabled;
            existing.TimeZoneId = NormalizeOptional(timeZoneId, HrWorkScheduleDefaults.TimeZoneId);
            existing.AllowedDaysOfWeekCsv = SerializeDays(allowedDaysOfWeek);
            existing.WorkStartLocalTimeText = SerializeTime(workStartLocalTime);
            existing.WorkEndLocalTimeText = SerializeTime(workEndLocalTime);
            existing.WorksiteIpRestrictionEnabled = worksiteIpRestrictionEnabled;
            existing.AllowedWorksiteIpRangesText = SerializeIpRanges(allowedWorksiteIpRanges);
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return ToAssignment(existing);
        }

        var record = new HrRoleAssignmentRecord
        {
            Id = Guid.NewGuid(),
            UserId = normalizedUserId,
            ScopeType = normalizedScopeType,
            ScopeId = normalizedScopeId,
            ParticipantCategory = normalizedParticipantCategory,
            RoleCode = normalizedRoleCode,
            RoleName = string.IsNullOrWhiteSpace(roleName) ? normalizedRoleCode : roleName.Trim(),
            IsActive = true,
            AssignedAtUtc = DateTime.UtcNow,
            AssignedByUserId = string.IsNullOrWhiteSpace(assignedByUserId) ? "system" : assignedByUserId.Trim(),
            WorkScheduleEnabled = workScheduleEnabled,
            TimeZoneId = NormalizeOptional(timeZoneId, HrWorkScheduleDefaults.TimeZoneId),
            AllowedDaysOfWeekCsv = SerializeDays(allowedDaysOfWeek),
            WorkStartLocalTimeText = SerializeTime(workStartLocalTime),
            WorkEndLocalTimeText = SerializeTime(workEndLocalTime),
            WorksiteIpRestrictionEnabled = worksiteIpRestrictionEnabled,
            AllowedWorksiteIpRangesText = SerializeIpRanges(allowedWorksiteIpRanges),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.HrRoleAssignments.Add(record);
        await _db.SaveChangesAsync(cancellationToken);
        return ToAssignment(record);
    }

    public async Task<IReadOnlyList<HrRoleAssignment>> ListAsync(
        string? userId,
        string? scopeType,
        string? scopeId,
        CancellationToken cancellationToken)
    {
        var query = _db.HrRoleAssignments.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var normalizedUserId = userId.Trim();
            query = query.Where(x => x.UserId == normalizedUserId);
        }

        if (!string.IsNullOrWhiteSpace(scopeType))
        {
            var normalizedScopeType = scopeType.Trim();
            query = query.Where(x => x.ScopeType == normalizedScopeType);
        }

        if (!string.IsNullOrWhiteSpace(scopeId))
        {
            var normalizedScopeId = scopeId.Trim();
            query = query.Where(x => x.ScopeId == normalizedScopeId);
        }

        var records = await query
            .OrderBy(x => x.ScopeType)
            .ThenBy(x => x.ScopeId)
            .ThenBy(x => x.UserId)
            .ThenBy(x => x.RoleCode)
            .ToArrayAsync(cancellationToken);

        return records.Select(ToAssignment).ToArray();
    }

    public async Task<bool> RevokeAsync(Guid assignmentId, CancellationToken cancellationToken)
    {
        var record = await _db.HrRoleAssignments.FirstOrDefaultAsync(x => x.Id == assignmentId && x.IsActive, cancellationToken);
        if (record is null)
        {
            return false;
        }

        record.IsActive = false;
        record.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HasAnyRoleAsync(
        string userId,
        string scopeType,
        string scopeId,
        IReadOnlyCollection<string> roleCodes,
        CancellationToken cancellationToken)
    {
        var decision = await AuthorizeAccessAsync(
            userId,
            scopeType,
            scopeId,
            roleCodes,
            clientIpAddress: null,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return decision.IsAllowed;
    }

    public async Task<HrRoleAccessDecision> AuthorizeAccessAsync(
        string userId,
        string scopeType,
        string scopeId,
        IReadOnlyCollection<string> roleCodes,
        IPAddress? clientIpAddress,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new HrRoleAccessDecision(false, "UserMissing", null);
        }

        if (roleCodes.Count == 0)
        {
            return new HrRoleAccessDecision(false, "RequiredHrRoleMissing", null);
        }

        var normalizedUserId = userId.Trim();
        var normalizedScopeType = NormalizeOptional(scopeType, HrScopeTypes.Platform);
        var normalizedScopeId = NormalizeOptional(scopeId, HrScopeIds.Global);
        var normalizedRoleCodes = roleCodes.Select(x => x.Trim()).ToArray();

        var records = await _db.HrRoleAssignments.AsNoTracking()
            .Where(x => x.IsActive && x.UserId == normalizedUserId && normalizedRoleCodes.Contains(x.RoleCode))
            .ToArrayAsync(cancellationToken);

        var candidates = records
            .Select(ToAssignment)
            .Where(x => IsGlobalAssignment(x)
                || (string.Equals(x.ScopeType, normalizedScopeType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.ScopeId, normalizedScopeId, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        if (candidates.Length == 0)
        {
            return new HrRoleAccessDecision(false, "HrRoleNotAssigned", null);
        }

        var scheduleMatched = candidates.Where(x => IsWithinWorkSchedule(x, nowUtc)).ToArray();
        if (scheduleMatched.Length == 0)
        {
            return new HrRoleAccessDecision(false, "OutsideWorkSchedule", candidates[0]);
        }

        var ipMatched = scheduleMatched.FirstOrDefault(x => IsWithinWorksiteIpRange(x, clientIpAddress));
        if (ipMatched is null)
        {
            return new HrRoleAccessDecision(false, "OutsideWorksiteIpRange", scheduleMatched[0]);
        }

        return new HrRoleAccessDecision(true, string.Empty, ipMatched);
    }

    private static HrRoleAssignment ToAssignment(HrRoleAssignmentRecord record)
    {
        return new HrRoleAssignment(
            record.Id,
            record.UserId,
            record.ScopeType,
            record.ScopeId,
            record.ParticipantCategory,
            record.RoleCode,
            record.RoleName,
            record.IsActive,
            record.AssignedAtUtc,
            record.AssignedByUserId,
            record.WorkScheduleEnabled,
            string.IsNullOrWhiteSpace(record.TimeZoneId) ? HrWorkScheduleDefaults.TimeZoneId : record.TimeZoneId,
            ParseDays(record.AllowedDaysOfWeekCsv),
            ParseTime(record.WorkStartLocalTimeText),
            ParseTime(record.WorkEndLocalTimeText),
            record.WorksiteIpRestrictionEnabled,
            ParseIpRanges(record.AllowedWorksiteIpRangesText));
    }

    private static bool IsGlobalAssignment(HrRoleAssignment assignment)
    {
        return string.Equals(assignment.ScopeType, HrScopeTypes.Platform, StringComparison.OrdinalIgnoreCase)
            && string.Equals(assignment.ScopeId, HrScopeIds.Global, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWithinWorkSchedule(HrRoleAssignment assignment, DateTimeOffset nowUtc)
    {
        if (!assignment.WorkScheduleEnabled)
        {
            return true;
        }

        if (assignment.AllowedDaysOfWeek.Count == 0
            || assignment.WorkStartLocalTime is null
            || assignment.WorkEndLocalTime is null)
        {
            return false;
        }

        var timeZone = ResolveTimeZone(assignment.TimeZoneId);
        var localNow = TimeZoneInfo.ConvertTime(nowUtc, timeZone);
        var currentDay = localNow.DayOfWeek;
        var currentTime = TimeOnly.FromDateTime(localNow.DateTime);
        var start = assignment.WorkStartLocalTime.Value;
        var end = assignment.WorkEndLocalTime.Value;

        if (start == end)
        {
            return assignment.AllowedDaysOfWeek.Contains(currentDay);
        }

        if (start < end)
        {
            return assignment.AllowedDaysOfWeek.Contains(currentDay)
                && currentTime >= start
                && currentTime < end;
        }

        var previousDay = currentDay == DayOfWeek.Sunday ? DayOfWeek.Saturday : currentDay - 1;
        return (assignment.AllowedDaysOfWeek.Contains(currentDay) && currentTime >= start)
            || (assignment.AllowedDaysOfWeek.Contains(previousDay) && currentTime < end);
    }

    private static bool IsWithinWorksiteIpRange(HrRoleAssignment assignment, IPAddress? clientIpAddress)
    {
        if (!assignment.WorksiteIpRestrictionEnabled)
        {
            return true;
        }

        if (clientIpAddress is null || assignment.AllowedWorksiteIpRanges.Count == 0)
        {
            return false;
        }

        return assignment.AllowedWorksiteIpRanges.Any(range => IpRangeContains(range, clientIpAddress));
    }

    private static bool IpRangeContains(string range, IPAddress clientIpAddress)
    {
        var normalizedRange = range.Trim();
        if (string.IsNullOrWhiteSpace(normalizedRange))
        {
            return false;
        }

        if (!normalizedRange.Contains('/'))
        {
            return IPAddress.TryParse(normalizedRange, out var exact)
                && exact.MapToIPv6().Equals(clientIpAddress.MapToIPv6());
        }

        var parts = normalizedRange.Split('/', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !IPAddress.TryParse(parts[0], out var network)
            || !int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        var networkBytes = network.GetAddressBytes();
        var addressBytes = clientIpAddress.GetAddressBytes();
        if (network.AddressFamily != clientIpAddress.AddressFamily)
        {
            networkBytes = network.MapToIPv6().GetAddressBytes();
            addressBytes = clientIpAddress.MapToIPv6().GetAddressBytes();
        }

        var maxPrefixLength = addressBytes.Length * 8;
        if (prefixLength < 0 || prefixLength > maxPrefixLength)
        {
            return false;
        }

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        for (var i = 0; i < fullBytes; i++)
        {
            if (networkBytes[i] != addressBytes[i])
            {
                return false;
            }
        }

        if (remainingBits == 0)
        {
            return true;
        }

        var mask = (byte)(0xff << (8 - remainingBits));
        return (networkBytes[fullBytes] & mask) == (addressBytes[fullBytes] & mask);
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Local;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Local;
        }
    }

    private static IReadOnlyList<DayOfWeek> ParseDays(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => Enum.TryParse<DayOfWeek>(x, out var day) ? day : (DayOfWeek?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .OrderBy(x => x)
            .ToArray();
    }

    private static string SerializeDays(IReadOnlyList<DayOfWeek>? days)
    {
        if (days is null || days.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(',', days.Distinct().OrderBy(x => x));
    }

    private static TimeOnly? ParseTime(string? value)
    {
        return TimeOnly.TryParse(value, out var parsed) ? parsed : null;
    }

    private static string? SerializeTime(TimeOnly? value)
    {
        return value?.ToString("HH:mm:ss");
    }

    private static IReadOnlyList<string> ParseIpRanges(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string SerializeIpRanges(IReadOnlyList<string>? ranges)
    {
        if (ranges is null || ranges.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(';', ranges.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("HR role assignment requires a non-empty value.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeOptional(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
