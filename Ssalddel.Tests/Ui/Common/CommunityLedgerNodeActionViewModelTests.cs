using Microsoft.AspNetCore.Components.Forms;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class CommunityLedgerNodeActionViewModelTests
{
    [Fact]
    public void 사진_필수_업무는_이미지와_현장_확인이_모두_있어야_실행할_수_있다()
    {
        var viewModel = new CommunityLedgerNodeActionViewModel(new FakeActionService());
        viewModel.Begin(CreateAction(photoRequired: true));

        Assert.False(viewModel.CanExecutePendingAction);

        viewModel.SelectEvidence(new TestBrowserFile("pickup.jpg", "image/jpeg", 1024));
        Assert.False(viewModel.CanExecutePendingAction);

        viewModel.EvidenceConfirmed = true;
        Assert.True(viewModel.CanExecutePendingAction);
    }

    [Theory]
    [InlineData("pickup.txt", "text/plain", 1024, "이미지 형식")]
    [InlineData("pickup.jpg", "image/jpeg", CommunityLedgerEvidencePolicy.MaxFileBytes + 1, "8MB 이하")]
    public void 허용하지_않는_증빙은_실행_상태에_들어가지_않는다(
        string name,
        string contentType,
        long size,
        string expectedMessage)
    {
        var viewModel = new CommunityLedgerNodeActionViewModel(new FakeActionService());
        viewModel.Begin(CreateAction(photoRequired: true));

        viewModel.SelectEvidence(new TestBrowserFile(name, contentType, size));
        viewModel.EvidenceConfirmed = true;

        Assert.Null(viewModel.EvidenceFile);
        Assert.False(viewModel.CanExecutePendingAction);
        Assert.Contains(expectedMessage, viewModel.ActionStatusMessage);
    }

    [Fact]
    public async Task 실행_성공은_증빙_업로드와_Command를_순서대로_완료하고_상태를_초기화한다()
    {
        var service = new FakeActionService();
        var viewModel = new CommunityLedgerNodeActionViewModel(service);
        viewModel.Begin(CreateAction(photoRequired: true));
        viewModel.SelectEvidence(new TestBrowserFile("pickup.jpg", "image/jpeg", 1024));
        viewModel.EvidenceConfirmed = true;

        var succeeded = await viewModel.ExecuteAsync();

        Assert.True(succeeded);
        Assert.Equal(1, service.UploadCount);
        Assert.Equal(1, service.ExecuteCount);
        Assert.NotNull(service.LastEvidence);
        Assert.True(viewModel.ActionSucceeded);
        Assert.Contains("상차완료", viewModel.ActionStatusMessage);
        Assert.Null(viewModel.PendingAction);
        Assert.Null(viewModel.EvidenceFile);
        Assert.False(viewModel.EvidenceConfirmed);
    }

    [Fact]
    public async Task 실행_실패는_화면_상태로_남기고_선택한_업무는_유지한다()
    {
        var service = new FakeActionService
        {
            ExecuteException = new InvalidOperationException("현재 운송 상태를 다시 확인해 주세요.")
        };
        var viewModel = new CommunityLedgerNodeActionViewModel(service);
        var action = CreateAction(photoRequired: false);
        viewModel.Begin(action);

        var succeeded = await viewModel.ExecuteAsync();

        Assert.False(succeeded);
        Assert.False(viewModel.ActionSucceeded);
        Assert.Same(action, viewModel.PendingAction);
        Assert.Equal("현재 운송 상태를 다시 확인해 주세요.", viewModel.ActionStatusMessage);
    }

    private static PlatformCommunityLedgerNodeActionResponse CreateAction(bool photoRequired)
        => new()
        {
            행동Code = photoRequired
                ? CommunityLedgerNodeActionCodes.TransportCompletePickup
                : CommunityLedgerNodeActionCodes.TransportArrivePickup,
            블록Id = "pickup",
            표시명 = photoRequired ? "상차 완료" : "상차지 도착",
            설명 = "현장 상태를 확인합니다.",
            실행대상Id = "17",
            실행가능여부 = true,
            사진필수여부 = photoRequired
        };

    private sealed class FakeActionService : ICommunityLedgerNodeActionService
    {
        public int UploadCount { get; private set; }

        public int ExecuteCount { get; private set; }

        public CommunityLedgerEvidenceUploadResult? LastEvidence { get; private set; }

        public Exception? ExecuteException { get; init; }

        public Task<CommunityLedgerEvidenceUploadResult> 상차증빙업로드Async(
            PlatformCommunityLedgerNodeActionResponse action,
            IBrowserFile file,
            CancellationToken cancellationToken = default)
        {
            UploadCount++;
            return Task.FromResult(new CommunityLedgerEvidenceUploadResult
            {
                BucketName = "test",
                ObjectName = "pickup.jpg",
                Url = "https://example.test/pickup.jpg"
            });
        }

        public Task<기사운송상태변경응답> 실행Async(
            PlatformCommunityLedgerNodeActionResponse action,
            CommunityLedgerEvidenceUploadResult? evidence = null,
            CancellationToken cancellationToken = default)
        {
            ExecuteCount++;
            LastEvidence = evidence;
            if (ExecuteException is not null)
            {
                throw ExecuteException;
            }

            return Task.FromResult(new 기사운송상태변경응답
            {
                Id = 17,
                운송번호 = "TR-17",
                상태 = "상차완료",
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    private sealed class TestBrowserFile(
        string name,
        string contentType,
        long size) : IBrowserFile
    {
        public string Name { get; } = name;

        public DateTimeOffset LastModified { get; } = DateTimeOffset.UtcNow;

        public long Size { get; } = size;

        public string ContentType { get; } = contentType;

        public Stream OpenReadStream(
            long maxAllowedSize = 512000,
            CancellationToken cancellationToken = default)
        {
            if (Size > maxAllowedSize)
            {
                throw new IOException("파일 크기 제한을 넘었습니다.");
            }

            return new MemoryStream(new byte[checked((int)Size)]);
        }
    }
}
