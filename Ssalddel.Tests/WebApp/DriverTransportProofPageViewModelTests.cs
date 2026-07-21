using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.WebApp.Services;
using Ssalddel.WebApp.ViewModels;

namespace Ssalddel.Tests.WebApp;

public sealed class DriverTransportProofPageViewModelTests
{
    [Fact]
    public async Task LoadCurrentTransportAsync_조회한_운송_ID와_요약을_같이_적용한다()
    {
        var fixture = new OperationsFixture
        {
            LoadCurrent = _ => Task.FromResult<기사운송요약응답>(Transport(42, "REQ-42", "배차확정"))
        };
        using var viewModel = new DriverTransportProofPageViewModel(fixture.CreateOperations());

        await viewModel.LoadCurrentTransportAsync();

        Assert.Equal(42, viewModel.TransportId);
        Assert.Equal("REQ-42", viewModel.CurrentTransport?.운송번호);
        Assert.Equal(DriverTransportProofMessageTone.Success, viewModel.StatusTone);
        Assert.False(viewModel.IsBusy);
    }

    [Fact]
    public async Task TransportId_변경은_이전_운송의_증빙과_입력값을_초기화한다()
    {
        var fixture = new OperationsFixture();
        using var viewModel = new DriverTransportProofPageViewModel(fixture.CreateOperations());
        await viewModel.Pickup.UploadImageAsync(Image("pickup.jpg"));
        await viewModel.Dropoff.UploadImageAsync(Image("dropoff.jpg"));
        await viewModel.Issue.UploadImageAsync(Image("issue.jpg"));
        viewModel.Pickup.RecipientName = "테스트 인수자";
        viewModel.Issue.Memo = "테스트 메모";

        viewModel.TransportId = 77;

        Assert.Null(viewModel.Pickup.Upload);
        Assert.Null(viewModel.Dropoff.Upload);
        Assert.Null(viewModel.Issue.Upload);
        Assert.Null(viewModel.Pickup.RecipientName);
        Assert.Null(viewModel.Issue.Memo);
        Assert.Equal(DriverTransportIssueViewModel.Reasons[0].Code, viewModel.Issue.IssueCode);
    }

    [Fact]
    public async Task PickupComplete_선택한_운송과_인수증을_Command에_전달한다()
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
        using var viewModel = new DriverTransportProofPageViewModel(fixture.CreateOperations());
        viewModel.ConfigureTransportId(15);
        await viewModel.Pickup.UploadImageAsync(Image("pickup.jpg"));
        viewModel.Pickup.ReceiptEvidenceMethod = "서명생략";
        viewModel.Pickup.RecipientName = "테스트 인수자";
        viewModel.Pickup.SignatureOmitted = true;
        viewModel.Pickup.SignatureOmissionReason = "현장 단말기 점검";

        await viewModel.Pickup.CompleteAsync();

        Assert.Equal(15, completedTransportId);
        Assert.NotNull(capturedReceipt);
        Assert.Equal("서명생략", capturedReceipt.인수증증빙방식);
        Assert.Equal("테스트 인수자", capturedReceipt.인수자명);
        Assert.True(capturedReceipt.인수증서명생략확인);
        Assert.Equal("현장 단말기 점검", capturedReceipt.인수증서명생략사유);
        Assert.Equal("상차완료", viewModel.LastState?.상태);
    }

    [Fact]
    public async Task ReportIssue_선택한_사유와_증빙만_payload로_조립한다()
    {
        long? reportedTransportId = null;
        기사운송문제신고요청? capturedRequest = null;
        var fixture = new OperationsFixture
        {
            ReportIssue = (transportId, request, _) =>
            {
                reportedTransportId = transportId;
                capturedRequest = request;
                return Task.FromResult(State(transportId, "예외신고"));
            }
        };
        using var viewModel = new DriverTransportProofPageViewModel(fixture.CreateOperations());
        viewModel.ConfigureTransportId(21);
        viewModel.Issue.IssueCode = "하차지부재";
        viewModel.Issue.Memo = "연락 후 재방문 예정";
        viewModel.Issue.RequireAdminReview = false;
        await viewModel.Issue.UploadImageAsync(Image("issue.jpg"));

        await viewModel.Issue.ReportAsync();

        Assert.Equal(21, reportedTransportId);
        Assert.NotNull(capturedRequest);
        Assert.Equal("하차", capturedRequest.단계);
        Assert.Equal("하차지부재", capturedRequest.예외코드);
        Assert.Equal("연락 후 재방문 예정", capturedRequest.메모);
        Assert.Equal("object/issue.jpg", capturedRequest.증빙ObjectName);
        Assert.False(capturedRequest.관리자확인요청);
    }

    [Fact]
    public async Task DropoffComplete_업로드_전에는_Command를_실행하지_않는다()
    {
        var completeCalled = false;
        var fixture = new OperationsFixture
        {
            CompleteDropoff = (_, _, _) =>
            {
                completeCalled = true;
                return Task.FromResult(State(1, "인수완료"));
            }
        };
        using var viewModel = new DriverTransportProofPageViewModel(fixture.CreateOperations());

        await viewModel.Dropoff.CompleteAsync();

        Assert.False(completeCalled);
        Assert.Equal(DriverTransportProofMessageTone.Warning, viewModel.StatusTone);
        Assert.Contains("하차 사진 업로드", viewModel.StatusMessage);
    }

    [Fact]
    public async Task 새_사진_업로드가_실패하면_이전_성공_증빙을_재사용하지_않는다()
    {
        var uploadCount = 0;
        var fixture = new OperationsFixture
        {
            UploadPhoto = (transportId, _, fileName, _, _, _) =>
            {
                uploadCount++;
                if (uploadCount > 1)
                {
                    throw new InvalidOperationException("업로드 실패");
                }

                return Task.FromResult(Upload(fileName));
            }
        };
        using var viewModel = new DriverTransportProofPageViewModel(fixture.CreateOperations());
        await viewModel.Pickup.UploadImageAsync(Image("first.jpg"));

        await viewModel.Pickup.UploadImageAsync(Image("retry.jpg"));

        Assert.Null(viewModel.Pickup.Upload);
        Assert.Null(viewModel.Pickup.PreviewUrl);
        Assert.Equal(DriverTransportProofMessageTone.Error, viewModel.StatusTone);
        Assert.Equal("업로드 실패", viewModel.StatusMessage);
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

    private static 기사운송요약응답 Transport(long id, string requestId, string status)
        => new()
        {
            Id = id,
            운송번호 = requestId,
            상태 = status,
            출발지 = "샘플 공동창고",
            도착지 = "샘플 공동수령점",
            UpdatedAt = new DateTime(2026, 7, 21, 10, 0, 0, DateTimeKind.Utc)
        };

    private static 기사운송상세응답 Detail(long id)
        => new()
        {
            Id = id,
            운송번호 = $"REQ-{id}",
            상태 = "진행중",
            출발지 = "샘플 공동창고",
            도착지 = "샘플 공동수령점"
        };

    private static 기사운송상태변경응답 State(long id, string status)
        => new()
        {
            Id = id,
            운송번호 = $"REQ-{id}",
            상태 = status,
            UpdatedAt = new DateTime(2026, 7, 21, 10, 10, 0, DateTimeKind.Utc)
        };

    private sealed class OperationsFixture
    {
        public Func<CancellationToken, Task<기사운송요약응답>> LoadCurrent { get; set; }
            = _ => Task.FromResult<기사운송요약응답>(Transport(1, "REQ-1", "배차확정"));

        public Func<long, CancellationToken, Task<기사운송상세응답>> LoadDetail { get; set; }
            = (transportId, _) => Task.FromResult(Detail(transportId));

        public Func<long, CancellationToken, Task<기사운송상태변경응답>> ArrivePickup { get; set; }
            = (transportId, _) => Task.FromResult(State(transportId, "상차지 도착"));

        public Func<long, CancellationToken, Task<기사운송상태변경응답>> ArriveDropoff { get; set; }
            = (transportId, _) => Task.FromResult(State(transportId, "하차지 도착"));

        public Func<long, 운송증빙단계, string, string, byte[], CancellationToken, Task<기사운송사진업로드결과>> UploadPhoto { get; set; }
            = (_, _, fileName, _, _, _) => Task.FromResult(Upload(fileName));

        public Func<long, 기사운송사진업로드결과, 기사상차인수증입력, CancellationToken, Task<기사운송상태변경응답>> CompletePickup { get; set; }
            = (transportId, _, _, _) => Task.FromResult(State(transportId, "상차완료"));

        public Func<long, 기사운송사진업로드결과, CancellationToken, Task<기사운송상태변경응답>> CompleteDropoff { get; set; }
            = (transportId, _, _) => Task.FromResult(State(transportId, "인수완료"));

        public Func<long, 기사운송문제신고요청, CancellationToken, Task<기사운송상태변경응답>> ReportIssue { get; set; }
            = (transportId, _, _) => Task.FromResult(State(transportId, "예외신고"));

        public DriverTransportProofOperations CreateOperations()
            => new(
                LoadCurrent,
                LoadDetail,
                ArrivePickup,
                ArriveDropoff,
                UploadPhoto,
                CompletePickup,
                CompleteDropoff,
                ReportIssue);
    }
}
