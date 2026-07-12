using Hongdal.Contracts.Common.Community;
using Hongdal.Services.Community;

namespace Hongdal.Tests.Services.Community;

public sealed class CommunityLedgerStoreContractTests
{
    [Fact]
    public void Ledger_save_request_can_hold_blocks_participants_and_diagram_snapshot()
    {
        var request = new 커뮤니티원장저장요청
        {
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.CargoTransport,
            제목 = "동네 책장 운송 원장",
            원함 = "책장을 옮기고 하차 완료 후 정산 표시를 남기고 싶어요.",
            대상OsCode = CommunityLedgerOperatingSystemCodes.DomesticCargoTransport,
            대상OsName = "국내 화물 운송 OS",
            블록목록 =
            [
                new()
                {
                    BlockId = "request",
                    BlockType = CommunityLedgerBlockTypes.Order,
                    Title = "운송 의뢰",
                    Data = new Dictionary<string, string>
                    {
                        ["상차지"] = "파주시",
                        ["하차지"] = "은평구"
                    }
                },
                new()
                {
                    BlockId = "settlement",
                    BlockType = CommunityLedgerBlockTypes.Settlement,
                    Title = "결제/정산",
                    State = "대기"
                }
            ],
            참여자목록 =
            [
                new()
                {
                    UserId = "user-1",
                    DisplayName = "익명 화주",
                    RoleLabel = "요청자"
                }
            ],
            다이어그램스냅샷 = new DiagramSnapshotDto
            {
                DiagramId = "diagram-1",
                DiagramName = "운송 의뢰-상차-하차-정산",
                LedgerTemplateKey = CommunityLedgerTemplateKeys.CargoTransport,
                Nodes =
                [
                    new() { NodeId = "request", Kind = "order", Title = "운송 의뢰", X = 10, Y = 80 },
                    new() { NodeId = "settlement", Kind = "confirm", Title = "결제/정산", X = 80, Y = 80 }
                ],
                Edges =
                [
                    new() { EdgeId = "edge-1", FromNodeId = "request", ToNodeId = "settlement", Label = "완료 후 정산 표시" }
                ]
            }
        };

        Assert.Equal("platform", request.커뮤니티Id);
        Assert.Equal(2, request.블록목록.Count);
        Assert.Equal("익명 화주", request.참여자목록.Single().DisplayName);
        Assert.Equal("완료 후 정산 표시", request.다이어그램스냅샷.Edges.Single().Label);
    }

    [Fact]
    public void Ledger_state_change_request_keeps_state_history_boundary_explicit()
    {
        var request = new 커뮤니티원장상태변경요청
        {
            원장Id = "ledger-1",
            이전상태 = 커뮤니티원장상태.초안,
            상태 = 커뮤니티원장상태.진행중,
            현재단계Key = "pickup",
            메모 = "상차 단계로 이동"
        };

        Assert.Equal("ledger-1", request.원장Id);
        Assert.Equal(커뮤니티원장상태.진행중, request.상태);
        Assert.Equal("pickup", request.현재단계Key);
    }
}
