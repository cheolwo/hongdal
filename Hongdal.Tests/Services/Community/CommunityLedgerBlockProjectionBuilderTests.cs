using Hongdal.Contracts.Common.Community;
using Hongdal.Domain.Community;
using Hongdal.Services.Community;

namespace Hongdal.Tests.Services.Community;

public sealed class CommunityLedgerBlockProjectionBuilderTests
{
    [Fact]
    public void Builder_projects_diagram_edges_as_block_relations_with_cardinality()
    {
        var ledger = new 커뮤니티원장Dto
        {
            원장Id = "ledger-1",
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.WarehouseOutbound,
            블록목록 =
            [
                new()
                {
                    BlockId = "outbound-batch",
                    BlockType = CommunityLedgerBlockTypes.Inventory,
                    Title = "출고묶음"
                },
                new()
                {
                    BlockId = "outbound-item",
                    BlockType = CommunityLedgerBlockTypes.Item,
                    Title = "출고예정"
                }
            ],
            다이어그램스냅샷 = new DiagramSnapshotDto
            {
                DiagramId = "diagram-1",
                Nodes =
                [
                    new() { NodeId = "outbound-batch", Kind = "inventory", Title = "출고묶음" },
                    new() { NodeId = "outbound-item", Kind = "item", Title = "출고예정" }
                ],
                Edges =
                [
                    new()
                    {
                        EdgeId = "edge-1",
                        FromNodeId = "outbound-batch",
                        ToNodeId = "outbound-item",
                        Label = "출고묶음에 포함",
                        MeaningCode = "contains",
                        Data = new Dictionary<string, string>
                        {
                            ["관계Cardinality"] = "1:N",
                            ["필수여부"] = "true"
                        }
                    }
                ]
            }
        };

        var result = 커뮤니티원장블록관계투영Builder.생성(ledger);

        Assert.Equal(2, result.블록목록.Count);
        var relation = Assert.Single(result.관계목록);
        Assert.Equal("ledger-1", relation.커뮤니티원장Id);
        Assert.Equal("outbound-batch", relation.FromBlockId);
        Assert.Equal("outbound-item", relation.ToBlockId);
        Assert.Equal(원장블록관계유형.포함, relation.관계유형);
        Assert.Equal(원장블록관계Cardinality.일대다, relation.Cardinality);
        Assert.True(relation.필수여부);
        Assert.Same(result.블록목록.Single(x => x.BlockId == "outbound-batch"), relation.FromBlock);
        Assert.Same(result.블록목록.Single(x => x.BlockId == "outbound-item"), relation.ToBlock);
    }

    [Fact]
    public void Builder_uses_ordered_one_to_one_flow_when_diagram_has_no_edges()
    {
        var ledger = new 커뮤니티원장Dto
        {
            원장Id = "ledger-2",
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.CargoTransport,
            블록목록 =
            [
                new() { BlockId = "request", BlockType = CommunityLedgerBlockTypes.Order, Title = "운송 의뢰" },
                new() { BlockId = "pickup", BlockType = CommunityLedgerBlockTypes.Place, Title = "상차지" },
                new() { BlockId = "dropoff", BlockType = CommunityLedgerBlockTypes.Place, Title = "하차지" }
            ]
        };

        var result = 커뮤니티원장블록관계투영Builder.생성(ledger);

        Assert.Equal(3, result.블록목록.Count);
        Assert.Equal(2, result.관계목록.Count);
        Assert.All(result.관계목록, relation =>
        {
            Assert.Equal(원장블록관계유형.흐름, relation.관계유형);
            Assert.Equal(원장블록관계Cardinality.일대일, relation.Cardinality);
        });
    }
}
