using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Components.Community;

namespace Ssalddel.Tests.Ui.Common;

public sealed class CommunityLedgerDiagramPresentationTests
{
    [Fact]
    public void 다이어그램_배치는_원본_좌표_순서대로_일관된_격자에_놓인다()
    {
        var later = new DiagramNodeDto { NodeId = "later", X = 0, Y = 20 };
        var first = new DiagramNodeDto { NodeId = "first", X = 10, Y = 0 };
        var diagram = new DiagramSnapshotDto { Nodes = [later, first] };

        var layouts = CommunityLedgerDiagramPresentation.BuildNodeLayouts(diagram);

        Assert.Collection(
            layouts,
            item =>
            {
                Assert.Same(first, item.Node);
                Assert.Equal(30, item.X);
                Assert.Equal(48, item.Y);
            },
            item =>
            {
                Assert.Same(later, item.Node);
                Assert.Equal(210, item.X);
                Assert.Equal(48, item.Y);
            });
    }

    [Fact]
    public void 블록_상태는_노드_설명보다_서버_원장_상태를_우선한다()
    {
        var node = new DiagramNodeDto
        {
            NodeId = "pickup",
            Description = "예정"
        };
        var context = new PlatformCommunityPostLedgerContextResponse
        {
            블록목록 =
            [
                new PlatformCommunityLedgerBlockResponse
                {
                    블록Id = "pickup",
                    상태 = "상차 준비"
                }
            ]
        };

        var state = CommunityLedgerDiagramPresentation.BuildNodeState(context, node);

        Assert.Equal("상차 준비", state);
        Assert.Equal(
            "ledger-diagram-detail__state--active",
            CommunityLedgerDiagramPresentation.BuildNodeStateClass(state));
    }

    [Fact]
    public void 담당자_요약은_목록_순서와_무관하게_주담당을_표시한다()
    {
        var context = new PlatformCommunityPostLedgerContextResponse
        {
            블록목록 =
            [
                new PlatformCommunityLedgerBlockResponse
                {
                    블록Id = "pickup",
                    담당자목록 =
                    [
                        new PlatformCommunityLedgerBlockAssigneeResponse
                        {
                            DisplayName = "협업자",
                            ResponsibilityType = CommunityLedgerBlockResponsibilityTypes.Collaborator
                        },
                        new PlatformCommunityLedgerBlockAssigneeResponse
                        {
                            DisplayName = "주담당자",
                            ResponsibilityType = CommunityLedgerBlockResponsibilityTypes.Primary
                        }
                    ]
                }
            ]
        };

        Assert.Equal(
            "주담당자",
            CommunityLedgerDiagramPresentation.BuildAssigneeSummary(context, "pickup"));
    }

    [Theory]
    [InlineData("https://example.test/source", "https://example.test/source")]
    [InlineData("http://example.test/source", "http://example.test/source")]
    [InlineData("javascript:alert(1)", null)]
    [InlineData("/relative", null)]
    public void 외부_링크는_http와_https_절대_URL만_허용한다(string value, string? expected)
        => Assert.Equal(expected, CommunityLedgerDiagramPresentation.ResolveExternalHttpUrl(value));
}
