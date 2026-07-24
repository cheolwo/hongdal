using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.WebApp.Services;
using Ssalddel.WebApp.ViewModels;

namespace Ssalddel.Tests.WebApp;

public sealed class DriverTransportDropoffPageViewModelTests
{
    [Fact]
    public async Task InitializeAsync_라우트_ID의_운송과_하차_예외만_준비한다()
    {
        long? loadedTransportId = null;
        var fixture = new OperationsFixture
        {
            LoadTransport = (transportId, _) =>
            {
                loadedTransportId = transportId;
                return Task.FromResult(Detail(transportId, "운송중"));
            }
        };
        using var viewModel = new DriverTransportDropoffPageViewModel(fixture.CreateOperations());

        await viewModel.InitializeAsync(41);

        Assert.Equal(41, loadedTransportId);
        Assert.Equal(41, viewModel.TransportId);
        Assert.Equal("REQ-41", viewModel.CurrentTransport?.운송번호);
        Assert.All(viewModel.Issue.Reasons, reason => Assert.Equal("하차", reason.Stage));
        Assert.DoesNotContain(viewModel.Issue.Reasons, reason => reason.Code == "상차물건없음");
    }

    [Fact]
    public async Task Route_ID가_바뀌면_이전_하차_증빙과_현장확인을_초기화한다()
    {
        var fixture = new OperationsFixture();
        using var viewModel = new DriverTransportDropoffPageViewModel(fixture.CreateOperations());
        await viewModel.InitializeAsync(20);
        await viewModel.Dropoff.UploadImageAsync(Image("dropoff.jpg"));
        await viewModel.Issue.UploadImageAsync(Image("issue.jpg"));
        viewModel.ReceiverName = "테스트 수령자";
        viewModel.DropoffPlaceConfirmed = true;
        viewModel.ReceiverConfirmed = true;
        viewModel.PaymentEvidenceConfirmed = true;

        await viewModel.InitializeAsync(21);

        Assert.Equal(21, viewModel.TransportId);
        Assert.Null(viewModel.Dropoff.Upload);
        Assert.Null(viewModel.Issue.Upload);
        Assert.Null(viewModel.ReceiverName);
        Assert.False(viewModel.DropoffPlaceConfirmed);
        Assert.False(viewModel.ReceiverConfirmed);
        Assert.False(viewModel.PaymentEvidenceConfirmed);
    }

    [Fact]
    public async Task 현장확인과_사진이_모두_있을때만_하차완료_Command를_전달한다()
    {
        long? completedTransportId = null;
        기사운송사진업로드결과? capturedUpload = null;
        var fixture = new OperationsFixture
        {
            CompleteDropoff = (transportId, upload, _) =>
            {
                completedTransportId = transportId;
                capturedUpload = upload;
                return Task.FromResult(State(transportId, "하차완료"));
            }
        };
        using var viewModel = new DriverTransportDropoffPageViewModel(fixture.CreateOperations());
        await viewModel.InitializeAsync(22);
        await viewModel.Dropoff.UploadImageAsync(Image("dropoff.jpg"));

        await viewModel.CompleteDropoffAsync();

        Assert.Null(completedTransportId);
        Assert.Equal(DriverTransportProofMessageTone.Warning, viewModel.StatusTone);

        viewModel.DropoffPlaceConfirmed = true;
        viewModel.ReceiverConfirmed = true;
        viewModel.PaymentEvidenceConfirmed = true;
        await viewModel.CompleteDropoffAsync();

        Assert.Equal(22, completedTransportId);
        Assert.Equal("object/dropoff.jpg", capturedUpload?.ObjectName);
        Assert.Equal("하차완료", viewModel.LastState?.상태);
        Assert.Equal(DriverTransportProofMessageTone.Success, viewModel.StatusTone);
    }

    [Fact]
    public async Task 하차_예외는_하차_단계로_신고한다()
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
        using var viewModel = new DriverTransportDropoffPageViewModel(fixture.CreateOperations());
        await viewModel.InitializeAsync(23);
        viewModel.Issue.IssueCode = "하차지부재";

        await viewModel.Issue.ReportAsync();

        Assert.NotNull(capturedRequest);
        Assert.Equal("하차", capturedRequest.단계);
        Assert.Equal("하차지부재", capturedRequest.예외코드);
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
            UpdatedAt = new DateTime(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc)
        };

    private static 기사운송상태변경응답 State(long id, string status)
        => new()
        {
            Id = id,
            운송번호 = $"REQ-{id}",
            상태 = status,
            UpdatedAt = new DateTime(2026, 7, 23, 12, 10, 0, DateTimeKind.Utc)
        };

    private sealed class OperationsFixture
    {
        public Func<long, CancellationToken, Task<기사운송상세응답>> LoadTransport { get; set; }
            = (transportId, _) => Task.FromResult(Detail(transportId, "운송중"));

        public Func<long, CancellationToken, Task<기사운송상태변경응답>> ArriveDropoff { get; set; }
            = (transportId, _) => Task.FromResult(State(transportId, "하차지 도착"));

        public Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> UploadPhoto { get; set; }
            = (_, _, fileName, _, _, _) => Task.FromResult(Upload(fileName));

        public Func<long, 기사운송사진업로드결과, CancellationToken, Task<기사운송상태변경응답>> CompleteDropoff { get; set; }
            = (transportId, _, _) => Task.FromResult(State(transportId, "하차완료"));

        public Func<long, 기사운송문제신고요청, CancellationToken, Task<기사운송상태변경응답>> ReportIssue { get; set; }
            = (transportId, _, _) => Task.FromResult(State(transportId, "예외신고"));

        public DriverTransportDropoffOperations CreateOperations()
            => new(LoadTransport, ArriveDropoff, UploadPhoto, CompleteDropoff, ReportIssue);
    }
}
