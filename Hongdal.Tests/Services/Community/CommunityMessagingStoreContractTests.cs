using Hongdal.Contracts.Common.Community;
using Hongdal.Services.Community;

namespace Hongdal.Tests.Services.Community;

public sealed class CommunityMessagingStoreContractTests
{
    [Fact]
    public void Conversation_request_can_hold_dm_group_diagram_and_ledger_context()
    {
        var request = new 커뮤니티대화방저장요청
        {
            대화방Id = "conversation-diagram-1",
            커뮤니티Id = "platform",
            유형 = 커뮤니티대화방유형.Diagram,
            제목 = "책장 운송 다이어그램 대화",
            원장Id = "ledger-cargo-1",
            원장템플릿Key = CommunityLedgerTemplateKeys.CargoTransport,
            다이어그램Id = "diagram-cargo-1",
            다이어그램이름 = "운송 의뢰-상차-하차-정산",
            업무Context = new DiagramWorkContextDto
            {
                WorkType = "CargoTransport",
                WorkLabel = "홍달 1.0 운송",
                PrimaryRoute = "/shipper/requests"
            },
            참여자목록 =
            [
                new()
                {
                    UserId = "user-shipper",
                    DisplayName = "익명 화주",
                    RoleLabel = "요청자"
                },
                new()
                {
                    UserId = "user-driver",
                    DisplayName = "익명 기사",
                    RoleLabel = "수행자"
                }
            ]
        };

        Assert.Equal(커뮤니티대화방유형.Diagram, request.유형);
        Assert.Equal("ledger-cargo-1", request.원장Id);
        Assert.Equal("홍달 1.0 운송", request.업무Context!.WorkLabel);
        Assert.Equal(2, request.참여자목록.Count);
    }

    [Fact]
    public void Message_request_can_hold_text_diagram_snapshot_and_work_action_metadata()
    {
        var request = new 커뮤니티메시지저장요청
        {
            대화방Id = "conversation-diagram-1",
            커뮤니티Id = "platform",
            유형 = 커뮤니티대화방유형.Diagram,
            제목 = "책장 운송 다이어그램 대화",
            메시지 = "이 흐름대로 운송 원장을 진행해요.",
            메시지종류 = 커뮤니티메시지종류.Diagram,
            원장Id = "ledger-cargo-1",
            원장템플릿Key = CommunityLedgerTemplateKeys.CargoTransport,
            다이어그램Id = "diagram-cargo-1",
            다이어그램이름 = "운송 의뢰-상차-하차-정산",
            다이어그램스냅샷 = new DiagramSnapshotDto
            {
                DiagramId = "diagram-cargo-1",
                DiagramName = "운송 의뢰-상차-하차-정산",
                LedgerId = "ledger-cargo-1",
                Nodes =
                [
                    new() { NodeId = "request", Kind = "order", Title = "화주/기사/본래 총 노드", X = 0, Y = 0 },
                    new() { NodeId = "pickup", Kind = "pickup", Title = "상차", X = 120, Y = 0 },
                    new() { NodeId = "dropoff", Kind = "dropoff", Title = "하차", X = 240, Y = 0 },
                    new() { NodeId = "payment", Kind = "payment", Title = "결제", X = 360, Y = 0 }
                ],
                Edges =
                [
                    new() { EdgeId = "edge-1", FromNodeId = "request", ToNodeId = "pickup", Label = "수락 후 상차" },
                    new() { EdgeId = "edge-2", FromNodeId = "pickup", ToNodeId = "dropoff", Label = "상차 완료 후 이동" },
                    new() { EdgeId = "edge-3", FromNodeId = "dropoff", ToNodeId = "payment", Label = "하차 완료 후 정산" }
                ]
            },
            확장속성 = new Dictionary<string, string>
            {
                ["ActionCode"] = DiagramWorkActionCodes.CheckTransportProgress,
                ["TargetRoute"] = "/shipper/requests/ledger-cargo-1"
            }
        };

        Assert.Equal(커뮤니티메시지종류.Diagram, request.메시지종류);
        Assert.Equal(4, request.다이어그램스냅샷!.Nodes.Count);
        Assert.Equal("/shipper/requests/ledger-cargo-1", request.확장속성["TargetRoute"]);
    }

    [Fact]
    public void Read_marker_keeps_conversation_and_message_boundary_explicit()
    {
        var request = new 커뮤니티메시지읽음표시요청
        {
            대화방Id = "conversation-diagram-1",
            MessageId = "message-10"
        };

        Assert.Equal("conversation-diagram-1", request.대화방Id);
        Assert.Equal("message-10", request.MessageId);
    }

    [Fact]
    public async Task Diagram_message_history_maps_store_message_to_collaboration_contract()
    {
        var store = new Fake커뮤니티대화저장소
        {
            메시지목록 =
            [
                new()
                {
                    MessageId = "message-diagram-1",
                    대화방Id = "conversation-diagram-1",
                    커뮤니티Id = "platform",
                    보낸사람UserId = "user-1",
                    보낸사람표시명 = "익명 참여자",
                    메시지 = "다이어그램 흐름을 공유합니다.",
                    메시지종류 = 커뮤니티메시지종류.Diagram,
                    다이어그램Id = "diagram-cargo-1",
                    다이어그램이름 = "운송 의뢰-상차-하차-정산",
                    생성시각Utc = DateTime.UtcNow
                }
            ]
        };
        var useCase = new 커뮤니티대화UseCase(store);

        var result = await useCase.다이어그램메시지목록Async("conversation-diagram-1", 80, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var message = Assert.Single(result.Value.Items);
        Assert.Equal("message-diagram-1", message.MessageId);
        Assert.Equal(DiagramCollaborationMessageKinds.DiagramNote, message.MessageKind);
        Assert.Equal("diagram-cargo-1", message.DiagramId);
    }

    private sealed class Fake커뮤니티대화저장소 : I커뮤니티대화저장소
    {
        public IReadOnlyList<커뮤니티메시지Dto> 메시지목록 { get; set; } = [];

        public Task<커뮤니티대화방Dto> 대화방저장Async(
            커뮤니티대화방저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<커뮤니티대화방Dto?> 대화방조회Async(
            string 대화방Id,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<커뮤니티대화방Dto>> 대화방목록조회Async(
            커뮤니티대화방조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티대화방Dto>>([]);

        public Task<커뮤니티메시지Dto> 메시지저장Async(
            커뮤니티메시지저장요청 request,
            string senderUserId,
            string senderDisplayName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<커뮤니티메시지Dto>> 메시지목록조회Async(
            커뮤니티메시지조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult(메시지목록);

        public Task<커뮤니티대화방Dto?> 읽음표시Async(
            커뮤니티메시지읽음표시요청 request,
            string userId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
