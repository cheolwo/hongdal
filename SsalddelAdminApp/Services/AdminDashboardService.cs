using Ssalddel.Contracts.Admin.Dashboard;

namespace SsalddelAdminApp.Services;

public sealed class AdminDashboardService
{
    private readonly AdminAuthenticatedApiClient apiClient;

    public AdminDashboardService(AdminAuthenticatedApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public Task<관리자대시보드요약응답> GetAsync(
        CancellationToken cancellationToken = default)
        => apiClient.GetAsync<관리자대시보드요약응답>(
            "api/v1/admin/dashboard",
            cancellationToken);
}
