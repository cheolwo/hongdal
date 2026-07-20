using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 인사역할지원페이지ViewModelTests
{
    [Fact]
    public async Task 작성은_역할과세가지확인이모두있을때만Command를보낸다()
    {
        var service = new FakeRoleApplicationService();
        var viewModel = new 인사역할지원작성ViewModel(service)
        {
            선택역할코드 = HrDetailedRoleCodes.WarehouseInboundOperator,
            자발적지원확인 = true,
            역할고용비보장확인 = true
        };

        Assert.False(viewModel.제출가능);
        Assert.False(await viewModel.제출Async());
        Assert.Equal(0, service.SubmitCount);

        viewModel.검토정보이용동의 = true;
        Assert.True(viewModel.제출가능);
        Assert.True(await viewModel.제출Async());
        Assert.Equal(1, service.SubmitCount);
        Assert.Equal(HrRoleApplicationConsent.CurrentVersion, service.LastSubmitRequest?.ConsentVersion);
    }

    [Fact]
    public async Task PageViewModel은_제출과철회성공뒤서버원장을다시조회한다()
    {
        var service = new FakeRoleApplicationService();
        var page = CreatePage(service);
        page.작성.선택역할코드 = HrDetailedRoleCodes.WarehouseInboundOperator;
        page.작성.자발적지원확인 = true;
        page.작성.역할고용비보장확인 = true;
        page.작성.검토정보이용동의 = true;

        Assert.True(await page.초기화Async());
        Assert.True(await page.제출후재조회Async());
        var application = Assert.Single(page.목록.지원목록);
        Assert.True(application.CanWithdraw);
        Assert.True(await page.철회후재조회Async(application.ApplicationId));

        Assert.Equal(3, service.QueryCount);
        Assert.Equal(1, service.SubmitCount);
        Assert.Equal(1, service.WithdrawCount);
        Assert.False(Assert.Single(page.목록.지원목록).CanWithdraw);
    }

    private static 인사역할지원PageViewModel CreatePage(FakeRoleApplicationService service)
        => new(
            new 인사역할지원목록ViewModel(service),
            new 인사역할지원작성ViewModel(service),
            new 인사역할지원철회ViewModel(service));

    private sealed class FakeRoleApplicationService : I인사역할지원Service
    {
        private readonly List<HrRoleApplicationResponse> _applications = [];

        public int QueryCount { get; private set; }
        public int SubmitCount { get; private set; }
        public int WithdrawCount { get; private set; }
        public HrRoleApplicationSubmitRequest? LastSubmitRequest { get; private set; }

        public Task<HrRoleApplicationPageResponse> 내지원목록Async(
            CancellationToken cancellationToken = default)
        {
            QueryCount++;
            return Task.FromResult(new HrRoleApplicationPageResponse
            {
                Options = HrRoleApplicationCatalog.Items,
                Applications = _applications.ToArray()
            });
        }

        public Task<HrRoleApplicationResponse> 제출Async(
            HrRoleApplicationSubmitRequest request,
            CancellationToken cancellationToken = default)
        {
            SubmitCount++;
            LastSubmitRequest = request;
            var role = HrRoleApplicationCatalog.Find(request.RoleCode)!;
            var response = new HrRoleApplicationResponse
            {
                ApplicationId = Guid.NewGuid(),
                RoleCode = role.RoleCode,
                RoleName = role.RoleName,
                StatusCode = HrRoleApplicationStatusCodes.Submitted,
                StatusName = HrRoleApplicationStatusCodes.GetDisplayName(HrRoleApplicationStatusCodes.Submitted),
                SubmittedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                CanWithdraw = true
            };
            _applications.Add(response);
            return Task.FromResult(response);
        }

        public Task<HrRoleApplicationResponse> 철회Async(
            Guid applicationId,
            CancellationToken cancellationToken = default)
        {
            WithdrawCount++;
            var application = _applications.Single(item => item.ApplicationId == applicationId);
            application.StatusCode = HrRoleApplicationStatusCodes.Withdrawn;
            application.StatusName = HrRoleApplicationStatusCodes.GetDisplayName(HrRoleApplicationStatusCodes.Withdrawn);
            application.WithdrawnAtUtc = DateTime.UtcNow;
            application.CanWithdraw = false;
            return Task.FromResult(application);
        }
    }
}
