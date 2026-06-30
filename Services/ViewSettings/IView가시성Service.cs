using Hongdal.Contracts.Common.ViewSettings;

namespace 홍달.Services.ViewSettings;

public interface IView가시성Service
{
    Task SeedPoliciesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<View가시성항목응답>> GetEffectiveViewsAsync(string appKey, string roleName, string? userId, CancellationToken cancellationToken = default);
    Task SetUserVisibilityAsync(string appKey, string roleName, string userId, string viewKey, bool isVisible, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<관리자View정책항목응답>> GetPoliciesAsync(string? appKey = null, CancellationToken cancellationToken = default);
    Task<관리자View정책항목응답?> UpdatePolicyAsync(long id, bool policyEnabled, CancellationToken cancellationToken = default);
}
