using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.WebApp.Services;
using Ssalddel.WebApp.ViewModels;

namespace Ssalddel.Tests.WebApp;

public sealed class DriverTransportPickupPageViewModelTests
{
    [Fact]
    public async Task InitializeAsync_라우트_ID의_운송과_상차_예외만_준비한다()
    {
        long? loadedTransportId = null;
        var fixture = new OperationsFixture
        {
            LoadTransport = (transportId, _) =>
            {
                loadedTransportId = transportId;
                return Task.FromResult(Detail(transportId, "배차확정"));
            }
        };
        using var viewModel = new DriverTransportPickupPageViewModel(fixture.CreateOperations());

        await viewModel.InitializeAsync(31);

        Assert.Equal(31, loadedTransportId);
        Assert.Equal(31, viewModel.TransportId);
        Assert.Equal("REQ-31", viewModel.CurrentTransport?.운송번호);
        Assert.All(viewModel.Issue.Reasons, reason => Assert.Equal("상차", reason.Stage));
        Assert.DoesNotContain(viewModel.Issue.Reasons, reason => reason.Code == "하차지부재");
    }

    [Fact]
    public async Task Route_ID가_바뀌면_이전_상차_증빙과_입력을_초기화한다()
    {
        var fixture = new OperationsFixture();
        using var viewModel = new DriverTransportPickupPageViewModel(fixture.CreateOperations());
        await viewModel.InitializeAsync(10);
        await viewModel.Pickup.UploadImageAsync(Image("pickup.jpg"));
        await viewModel.Issue.UploadImageAsync(Image("issue.jpg"));
        viewModel.Pickup.RecipientName = "테스트 인수자";
        viewModel.Issue.Memo = "테스트 메모";

        await viewModel.InitializeAsync(11);

        Assert.Equal(11, viewModel.TransportId);
        Assert.Null(viewModel.Pickup.Upload);
        Assert.Null(viewModel.Issue.Upload);
        Assert.Null(viewModel.Pickup.RecipientName);
        Assert.Null(viewModel.Issue.Memo);
    }

    [Fact]
    public async Task PickupComplete_라우트_ID와_인수증을_상차_Command에_전달한다()
    {
        long? completedTransportId = null;
        기사상차인수증입력? capturedReceipt = null;
        var fixture = new OperationsFixture
        {
            CompletePickup = (transportId, _, receipt, _) =>
            {
                completedTransportId = transportId;
                capturedReceipt = receipt;
                return Task.FromResult(State(transportId, "상차완료"));
            }
        };
        using var viewModel = new DriverTransportPickupPageViewModel(fixture.CreateOperations());
        await viewModel.InitializeAsync(12);
        await viewModel.Pickup.UploadImageAsync(Image("pickup.jpg"));
        viewModel.Pickup.RecipientName = "테스트 인수자";
        viewModel.Pickup.ReceiptConfirmed = true;

        await viewModel.Pickup.CompleteAsync();

        Assert.Equal(12, completedTransportId);
        Assert.NotNull(capturedReceipt);
        Assert.Equal("테스트 인수자", capturedReceipt.인수자명);
        Assert.True(capturedReceipt.인수증확인완료);
        Assert.Equal("상차완료", viewModel.LastState?.상태);
        Assert.Equal(DriverTransportProofMessageTone.Success, viewModel.StatusTone);
    }

    [Fact]
    public async Task PickupIssue_사진재촬영_사유를_상차_단계로_신고한다()
    {
        기사운송문제신고요청? capturedRequest = null;
        var fixture = new OperationsFixture
        {
            ReportIssue = (transportId, request, _) =>
            {
                capturedRequest = request;
                return Task.FromResult(State(transportId, "예외신고"));
            }
        };
        using var viewModel = new DriverTransportPickupPageViewModel(fixture.CreateOperations());
        await viewModel.InitializeAsync(13);
        viewModel.Issue.IssueCode = "사진재촬영필요";

        await viewModel.Issue.ReportAsync();

        Assert.NotNull(capturedRequest);
        Assert.Equal("상차", capturedRequest.단계);
        Assert.Equal("사진재촬영필요", capturedRequest.예외코드);
    }

    private static DriverTransportProofImage Image(string fileName)
        => new(
            $"data:image/jpeg;base64,{Convert.ToBase64String([1, 2, 3])}",
            fileName,
            "image/jpeg",
            [1, 2, 3]);

    private static 기사운송사진업로드결과 Upload(string fileName)
        => new()
        {
            ObjectName = $"object/{fileName}",
            Url = $"https://example.invalid/{fileName}"
        };

    private static 기사운송상세응답 Detail(long id, string status)
        => new()
        {
            Id = id,
            운송번호 = $"REQ-{id}",
            상태 = status,
            출발지 = "샘플 공동창고",
            도착지 = "샘플 공동수령점",
            UpdatedAt = new DateTime(2026, 7, 21, 11, 0, 0, DateTimeKind.Utc)
        };

    private static 기사운송상태변경응답 State(long id, string status)
        => new()
        {
            Id = id,
            운송번호 = $"REQ-{id}",
            상태 = status,
            UpdatedAt = new DateTime(2026, 7, 21, 11, 10, 0, DateTimeKind.Utc)
        };

    private sealed class OperationsFixture
    {
        public Func<long, CancellationToken, Task<기사운송상세응답>> LoadTransport { get; set; }
            = (transportId, _) => Task.FromResult(Detail(transportId, "배차확정"));

        public Func<long, CancellationToken, Task<기사운송상태변경응답>> ArrivePickup { get; set; }
            = (transportId, _) => Task.FromResult(State(transportId, "상차지 도착"));

        public Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> UploadPhoto { get; set; }
            = (_, _, fileName, _, _, _) => Task.FromResult(Upload(fileName));

        public Func<long, 기사운송사진업로드결과, 기사상차인수증입력, CancellationToken, Task<기사운송상태변경응답>> CompletePickup { get; set; }
            = (transportId, _, _, _) => Task.FromResult(State(transportId, "상차완료"));

        public Func<long, 기사운송문제신고요청, CancellationToken, Task<기사운송상태변경응답>> ReportIssue { get; set; }
            = (transportId, _, _) => Task.FromResult(State(transportId, "예외신고"));

        public DriverTransportPickupOperations CreateOperations()
            => new(LoadTransport, ArrivePickup, UploadPhoto, CompletePickup, ReportIssue);
    }
}
