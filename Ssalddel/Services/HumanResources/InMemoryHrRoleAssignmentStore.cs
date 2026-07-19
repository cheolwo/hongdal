using System.Collections.Concurrent;
using System.Net;
using Ssalddel.Application.HumanResources;
using Ssalddel.Contracts.Common.Hr;

namespace Ssalddel.Services.HumanResources;

public sealed class InMemoryHrRoleAssignmentStore : IHrRoleAssignmentStore
{
    private readonly ConcurrentDictionary<Guid, HrRoleAssignment> _assignments = new();

    public Task<HrRoleAssignment> AssignAsync(
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

        var existing = _assignments.Values.FirstOrDefault(x =>
            x.IsActive
            && string.Equals(x.UserId, normalizedUserId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.ScopeType, normalizedScopeType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.ScopeId, normalizedScopeId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.ParticipantCategory, normalizedParticipantCategory, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.RoleCode, normalizedRoleCode, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return Task.FromResult(existing);
        }

        var assignment = new HrRoleAssignment(
            Guid.NewGuid(),
            normalizedUserId,
            normalizedScopeType,
            normalizedScopeId,
            normalizedParticipantCategory,
            normalizedRoleCode,
            string.IsNullOrWhiteSpace(roleName) ? normalizedRoleCode : roleName.Trim(),
            true,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(assignedByUserId) ? "system" : assignedByUserId.Trim(),
            workScheduleEnabled,
            NormalizeOptional(timeZoneId, HrWorkScheduleDefaults.TimeZoneId),
            NormalizeDays(allowedDaysOfWeek),
            workStartLocalTime,
            workEndLocalTime,
            worksiteIpRestrictionEnabled,
            NormalizeIpRanges(allowedWorksiteIpRanges));

        _assignments[assignment.Id] = assignment;
        return Task.FromResult(assignment);
    }

    public Task<IReadOnlyList<HrRoleAssignment>> ListAsync(
        string? userId,
        string? scopeType,
        string? scopeId,
        CancellationToken cancellationToken)
    {
        var query = _assignments.Values.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(x => string.Equals(x.UserId, userId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(scopeType))
        {
            query = query.Where(x => string.Equals(x.ScopeType, scopeType.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(scopeId))
        {
            query = query.Where(x => string.Equals(x.ScopeId, scopeId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<HrRoleAssignment>>(
            query.OrderBy(x => x.ScopeType)
                .ThenBy(x => x.ScopeId)
                .ThenBy(x => x.UserId)
                .ThenBy(x => x.RoleCode)
                .ToArray());
    }

    public Task<bool> RevokeAsync(Guid assignmentId, CancellationToken cancellationToken)
    {
        if (!_assignments.TryGetValue(assignmentId, out var existing) || !existing.IsActive)
        {
            return Task.FromResult(false);
        }

        _assignments[assignmentId] = existing with { IsActive = false };
        return Task.FromResult(true);
    }

    public Task<bool> HasAnyRoleAsync(
        string userId,
        string scopeType,
        string scopeId,
        IReadOnlyCollection<string> roleCodes,
        CancellationToken cancellationToken)
    {
        return AuthorizeAccessAsync(
                userId,
                scopeType,
                scopeId,
                roleCodes,
                clientIpAddress: null,
                DateTimeOffset.UtcNow,
                cancellationToken)
            .ContinueWith(x => x.Result.IsAllowed, cancellationToken);
    }

    public Task<HrRoleAccessDecision> AuthorizeAccessAsync(
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
            return Task.FromResult(new HrRoleAccessDecision(false, "UserMissing", null));
        }

        if (roleCodes.Count == 0)
        {
            return Task.FromResult(new HrRoleAccessDecision(false, "RequiredHrRoleMissing", null));
        }

        var normalizedUserId = userId.Trim();
        var normalizedScopeType = NormalizeOptional(scopeType, HrScopeTypes.Platform);
        var normalizedScopeId = NormalizeOptional(scopeId, HrScopeIds.Global);

        var candidates = _assignments.Values
            .Where(x =>
                x.IsActive
                && string.Equals(x.UserId, normalizedUserId, StringComparison.OrdinalIgnoreCase)
                && roleCodes.Any(role => string.Equals(role, x.RoleCode, StringComparison.OrdinalIgnoreCase))
                && (
                    IsGlobalAssignment(x)
                    || (string.Equals(x.ScopeType, normalizedScopeType, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(x.ScopeId, normalizedScopeId, StringComparison.OrdinalIgnoreCase))))
            .ToArray();

        if (candidates.Length == 0)
        {
            return Task.FromResult(new HrRoleAccessDecision(false, "HrRoleNotAssigned", null));
        }

        var scheduleMatched = candidates.Where(x => IsWithinWorkSchedule(x, nowUtc)).ToArray();
        if (scheduleMatched.Length == 0)
        {
            return Task.FromResult(new HrRoleAccessDecision(false, "OutsideWorkSchedule", candidates[0]));
        }

        var ipMatched = scheduleMatched.FirstOrDefault(x => IsWithinWorksiteIpRange(x, clientIpAddress));
        if (ipMatched is null)
        {
            return Task.FromResult(new HrRoleAccessDecision(false, "OutsideWorksiteIpRange", scheduleMatched[0]));
        }

        return Task.FromResult(new HrRoleAccessDecision(true, string.Empty, ipMatched));
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

    private static IReadOnlyList<DayOfWeek> NormalizeDays(IReadOnlyList<DayOfWeek>? days)
    {
        if (days is null || days.Count == 0)
        {
            return [];
        }

        return days.Distinct().OrderBy(x => x).ToArray();
    }

    private static IReadOnlyList<string> NormalizeIpRanges(IReadOnlyList<string>? ranges)
    {
        if (ranges is null || ranges.Count == 0)
        {
            return [];
        }

        return ranges
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
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
