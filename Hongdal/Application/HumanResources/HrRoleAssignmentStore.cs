using Hongdal.Contracts.Common.Hr;

namespace Hongdal.Application.HumanResources;

public sealed record HrRoleAssignment(
    Guid Id,
    string UserId,
    string ScopeType,
    string ScopeId,
    string ParticipantCategory,
    string RoleCode,
    string RoleName,
    bool IsActive,
    DateTime AssignedAtUtc,
    string AssignedByUserId,
    bool WorkScheduleEnabled,
    string TimeZoneId,
    IReadOnlyList<DayOfWeek> AllowedDaysOfWeek,
    TimeOnly? WorkStartLocalTime,
    TimeOnly? WorkEndLocalTime,
    bool WorksiteIpRestrictionEnabled,
    IReadOnlyList<string> AllowedWorksiteIpRanges);

public sealed record HrRoleAccessDecision(
    bool IsAllowed,
    string DenyReason,
    HrRoleAssignment? MatchedAssignment);

public interface IHrRoleAssignmentStore
{
    Task<HrRoleAssignment> AssignAsync(
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
        CancellationToken cancellationToken);

    Task<IReadOnlyList<HrRoleAssignment>> ListAsync(
        string? userId,
        string? scopeType,
        string? scopeId,
        CancellationToken cancellationToken);

    Task<bool> RevokeAsync(Guid assignmentId, CancellationToken cancellationToken);

    Task<bool> HasAnyRoleAsync(
        string userId,
        string scopeType,
        string scopeId,
        IReadOnlyCollection<string> roleCodes,
        CancellationToken cancellationToken);

    Task<HrRoleAccessDecision> AuthorizeAccessAsync(
        string userId,
        string scopeType,
        string scopeId,
        IReadOnlyCollection<string> roleCodes,
        System.Net.IPAddress? clientIpAddress,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken);
}

public static class HrRoleAssignmentMapping
{
    public static HrRoleAssignmentResponse ToResponse(HrRoleAssignment assignment)
    {
        return new HrRoleAssignmentResponse
        {
            Id = assignment.Id,
            UserId = assignment.UserId,
            ScopeType = assignment.ScopeType,
            ScopeId = assignment.ScopeId,
            ParticipantCategory = assignment.ParticipantCategory,
            RoleCode = assignment.RoleCode,
            RoleName = assignment.RoleName,
            IsActive = assignment.IsActive,
            AssignedAtUtc = assignment.AssignedAtUtc,
            AssignedByUserId = assignment.AssignedByUserId,
            WorkScheduleEnabled = assignment.WorkScheduleEnabled,
            TimeZoneId = assignment.TimeZoneId,
            AllowedDaysOfWeek = assignment.AllowedDaysOfWeek,
            WorkStartLocalTime = assignment.WorkStartLocalTime,
            WorkEndLocalTime = assignment.WorkEndLocalTime,
            WorksiteIpRestrictionEnabled = assignment.WorksiteIpRestrictionEnabled,
            AllowedWorksiteIpRanges = assignment.AllowedWorksiteIpRanges
        };
    }
}
