using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class CommunityLedgerDiagramDetailViewModelTests
{
    [Fact]
    public async Task 원장_Context를_받으면_첫_블록을_선택하고_해당_실시간_방에_입장한다()
    {
        var client = new FakeCollaborationClient { JoinResult = true };
        await using var viewModel = new CommunityLedgerDiagramDetailViewModel(client);
        var context = CreateContext();

        await viewModel.ApplyContextAsync(context);

        Assert.Equal("pickup", viewModel.SelectedNodeId);
        Assert.Equal("산지 상차", viewModel.SelectedNode?.Title);
        Assert.Equal("community:ledger:ledger-17:diagram", client.LastJoinRequest?.RoomId);
        Assert.Equal("diagram-17", client.LastJoinRequest?.DiagramId);
        Assert.True(viewModel.RealtimeConnected);
        Assert.Equal("원장 변경 실시간 반영 중", viewModel.RealtimeStatus);
    }

    [Fact]
    public async Task 같은_원장의_새_Context는_사용자가_선택한_블록을_유지한다()
    {
        var client = new FakeCollaborationClient { JoinResult = true };
        await using var viewModel = new CommunityLedgerDiagramDetailViewModel(client);
        await viewModel.ApplyContextAsync(CreateContext());
        viewModel.SelectNode("delivery");

        var refreshed = CreateContext(revision: 4);
        await viewModel.ApplyContextAsync(refreshed);

        Assert.Equal("delivery", viewModel.SelectedNodeId);
        Assert.Equal("공동 인수", viewModel.SelectedNode?.Title);
        Assert.Equal(1, client.JoinCount);
    }

    [Fact]
    public async Task 새_Revision_알림만_상위_원장_재조회를_요청한다()
    {
        var client = new FakeCollaborationClient { JoinResult = true };
        await using var viewModel = new CommunityLedgerDiagramDetailViewModel(client);
        await viewModel.ApplyContextAsync(CreateContext(revision: 3));
        var refreshCount = 0;
        viewModel.RefreshRequested += () =>
        {
            refreshCount++;
            return Task.CompletedTask;
        };

        await client.PublishLedgerChangedAsync(new DiagramLedgerChangedResponse
        {
            LedgerId = "ledger-17",
            Revision = 3,
            CurrentStep = "이전 단계"
        });
        await client.PublishLedgerChangedAsync(new DiagramLedgerChangedResponse
        {
            LedgerId = "ledger-17",
            Revision = 4,
            CurrentStep = "상차 완료"
        });

        Assert.Equal(1, refreshCount);
        Assert.False(viewModel.IsRefreshing);
        Assert.Equal("원장 변경 실시간 반영 중", viewModel.RealtimeStatus);
    }

    [Fact]
    public async Task Dispose는_실시간_Event를_해제하고_입장한_방을_종료한다()
    {
        var client = new FakeCollaborationClient { JoinResult = true };
        var viewModel = new CommunityLedgerDiagramDetailViewModel(client);
        await viewModel.ApplyContextAsync(CreateContext());

        await viewModel.DisposeAsync();
        await client.PublishStatusAsync("연결 끊김");

        Assert.Equal(1, client.DisconnectCount);
        Assert.Equal("원장 변경 실시간 반영 중", viewModel.RealtimeStatus);
    }

    private static PlatformCommunityPostLedgerContextResponse CreateContext(long revision = 3)
        => new()
        {
            원장Id = "ledger-17",
            Revision = revision,
            원장템플릿Key = CommunityLedgerTemplateKeys.CargoTransport,
            제목 = "공동구매 운송",
            다이어그램 = new DiagramSnapshotDto
            {
                DiagramId = "diagram-17",
                DiagramName = "공동구매 운송 흐름",
                Nodes =
                [
                    new DiagramNodeDto { NodeId = "pickup", Title = "산지 상차", X = 0, Y = 0 },
                    new DiagramNodeDto { NodeId = "delivery", Title = "공동 인수", X = 1, Y = 0 }
                ]
            },
            블록목록 =
            [
                new PlatformCommunityLedgerBlockResponse { 블록Id = "pickup", 제목 = "산지 상차" },
                new PlatformCommunityLedgerBlockResponse { 블록Id = "delivery", 제목 = "공동 인수" }
            ]
        };

    private sealed class FakeCollaborationClient : IDiagramCollaborationClientService
    {
        public event Func<DiagramChatMessageResponse, Task>? 메시지수신
        {
            add { }
            remove { }
        }

        public event Func<DiagramLedgerChangedResponse, Task>? 원장변경수신;

        public event Func<string, Task>? 상태변경;

        public bool JoinResult { get; init; }

        public int JoinCount { get; private set; }

        public int DisconnectCount { get; private set; }

        public DiagramRoomJoinRequest? LastJoinRequest { get; private set; }

        public string 연결상태 => 연결됨 ? "Connected" : "Disconnected";

        public string? 현재사용자Id => "user-17";

        public string 현재사용자표시명 => "검증 사용자";

        public bool 연결됨 { get; private set; }

        public Task<bool> 방입장Async(
            DiagramRoomJoinRequest request,
            CancellationToken cancellationToken = default)
        {
            JoinCount++;
            LastJoinRequest = request;
            연결됨 = JoinResult;
            return Task.FromResult(JoinResult);
        }

        public Task<IReadOnlyList<DiagramChatMessageResponse>> 메시지목록조회Async(
            string roomId,
            int limit = 80,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DiagramChatMessageResponse>>([]);

        public Task<bool> 메시지전송Async(
            DiagramChatMessageRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task 연결해제Async(CancellationToken cancellationToken = default)
        {
            DisconnectCount++;
            연결됨 = false;
            return Task.CompletedTask;
        }

        public async Task PublishLedgerChangedAsync(DiagramLedgerChangedResponse changed)
        {
            if (원장변경수신 is not { } handler)
            {
                return;
            }

            foreach (Func<DiagramLedgerChangedResponse, Task> callback in handler.GetInvocationList())
            {
                await callback(changed);
            }
        }

        public async Task PublishStatusAsync(string message)
        {
            if (상태변경 is not { } handler)
            {
                return;
            }

            foreach (Func<string, Task> callback in handler.GetInvocationList())
            {
                await callback(message);
            }
        }
    }
}
