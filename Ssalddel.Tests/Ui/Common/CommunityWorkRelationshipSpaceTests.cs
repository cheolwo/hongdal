using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class CommunityWorkRelationshipSpaceTests
{
    [Fact]
    public void 커뮤니티01은_업무앱의친구후보기록을확인하는독립Route를제공한다()
    {
        var routes = Read(
            "Ssalddel.Contracts",
            "Common",
            "Community",
            "CommunityPageRoutes.cs");
        var page = Read(
            "SsalddelApp",
            "Components",
            "Pages",
            "CommunityWorkRelationshipsPage.razor");
        var layout = Read(
            "SsalddelApp",
            "Components",
            "Layout",
            "CommunityMobileLayout.razor");

        Assert.Contains("WorkRelationships = \"/community/relationships\"", routes);
        Assert.Contains("@layout CommunityMobileLayout", page);
        Assert.Contains("<CommunityWorkRelationshipSpace />", page);
        Assert.Contains("친구 요청", layout);
    }

    [Fact]
    public void 업무친구요청장은_익명후보기록과별도동의를표현하고_상대식별자를렌더하지않는다()
    {
        var component = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "Community",
            "CommunityWorkRelationshipSpace.razor");

        Assert.Contains("02 주문자, 03 화주, 04 기사, 05 창고 앱", component);
        Assert.Contains("업무 로그는 공개 게시글이나 자동 친구 관계가 아닙니다", component);
        Assert.Contains("CounterpartyAnonymousLabel", component);
        Assert.Contains("친구 요청 보내기", component);
        Assert.Contains("상대가 수락하며 공개 항목을 직접 선택", component);
        Assert.DoesNotContain("CounterpartyUserId", component);
    }

    [Fact]
    public void 업무친구요청Client는_기존내스냅샷조회와_스냅샷기반연결요청API를사용한다()
    {
        var source = Read(
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Services",
            "업무친구요청Client.cs");

        Assert.Contains("api/v1/work-relationship-snapshots/me", source);
        Assert.Contains("api/v1/connections/requests/from-work-relationship", source);
    }

    [Fact]
    public async Task 연결가능한업무관계는_명시적친구요청과메시지로만요청하고_완료상태를남긴다()
    {
        var snapshot = new WorkRelationshipSnapshotResponse
        {
            Id = Guid.NewGuid(),
            CounterpartyAnonymousLabel = "user-abcd1234",
            CounterpartyRoleCode = "Shipper",
            PrivacyLevel = WorkRelationshipPrivacyCodes.ConnectionRequestEligible
        };
        var service = new FakeWorkRelationshipCommunityService(snapshot);
        var viewModel = new 업무친구요청ViewModel(service);

        Assert.True(await viewModel.조회Async());
        Assert.True(viewModel.친구요청가능(snapshot));
        Assert.True(await viewModel.친구요청Async(snapshot, "다음 운송 제안", "함께 일해서 감사했습니다."));

        Assert.Equal(snapshot.Id, service.RequestedSnapshotId);
        Assert.Equal("다음 운송 제안", service.Request?.Purpose);
        Assert.Equal("함께 일해서 감사했습니다.", service.Request?.Message);
        Assert.True(viewModel.친구요청완료(snapshot.Id));
        Assert.False(viewModel.친구요청가능(snapshot));
    }

    private static string Read(params string[] segments)
        => File.ReadAllText(Path.Combine(new[] { FindRepositoryRoot() }.Concat(segments).ToArray()));

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }

    private sealed class FakeWorkRelationshipCommunityService(
        WorkRelationshipSnapshotResponse snapshot) : I업무친구요청Service
    {
        public Guid? RequestedSnapshotId { get; private set; }
        public WorkRelationshipConnectionRequestCreateRequest? Request { get; private set; }

        public Task<WorkRelationshipSnapshotListResponse> 내업무친구후보조회Async(
            int take = 50,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new WorkRelationshipSnapshotListResponse
            {
                Items = [snapshot]
            });

        public Task<long> 친구요청Async(
            Guid snapshotId,
            WorkRelationshipConnectionRequestCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            RequestedSnapshotId = snapshotId;
            Request = request;
            return Task.FromResult(17L);
        }
    }
}
