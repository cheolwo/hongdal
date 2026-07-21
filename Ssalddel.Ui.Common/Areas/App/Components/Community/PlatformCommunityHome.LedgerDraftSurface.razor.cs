using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using MudBlazor;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private LedgerDraftSurface? ledgerDraftSurface;

    private LedgerDraftSurface LedgerDraft
        => ledgerDraftSurface ??= new(this);

    public sealed class LedgerDraftSurface
    {
        private readonly PlatformCommunityHome owner;

        internal LedgerDraftSurface(PlatformCommunityHome owner)
        {
            this.owner = owner;
        }

        public string Wish
        {
            get => owner.원함입력;
            set => owner.원함입력 = value;
        }

        public string Conditions
        {
            get => owner.원함조건입력;
            set => owner.원함조건입력 = value;
        }

        public bool CanAnalyze => owner.원함입력됨;

        public bool HasAnalysis => owner.원함분석결과 is not null;

        public string DecisionLabel => owner.원장화판정;

        public Color DecisionColor => owner.원장화판정Color;

        public string RecommendedTemplateName => owner.원함추천템플릿.DisplayName;

        public CommunityLedgerTemplateResponse Template => owner.SelectedLedgerTemplate;

        public IReadOnlyList<현재원장컨텍스트> Ledgers => 현재원장스냅샷목록;

        public 현재원장컨텍스트? SelectedLedger => owner.선택현재원장;

        public string ContextSummary => owner.현재원장컨텍스트요약생성(Template);

        public string WorkflowModeLabel
            => ResolveDiagramWorkflowModeLabel(owner.DiagramPalette.WorkflowModeKey);

        public string OperatingSystemLabel
            => 원장처리체계표시명(Template.TargetOperatingSystemName);

        public string WishTitle => owner.현재원장원함제목생성(Template);

        public string WishDetails => owner.현재원장원함상세생성(Template);

        public IReadOnlyList<원장블록노드> FlowNodes => owner.DiagramStage.FlowNodes;

        public IReadOnlyList<원장블록연결선> BuildEdges(IReadOnlyList<원장블록노드> nodes)
            => owner.BuildDiagramEdges(nodes);

        public PlatformHomeWorkspaceProfile? Workspace => owner.현재원장업무공간해결(Template);

        public IReadOnlyList<string> BuildImprovementItems(
            IReadOnlyList<원장블록노드> nodes,
            IReadOnlyList<원장블록연결선> edges)
            => owner.현재원장컨텍스트보완항목생성(Template, nodes, edges);

        public IReadOnlyList<CommunityLedgerTemplateResponse> Templates => LedgerTemplates;

        public string SelectedTemplateKey
        {
            get => owner.selectedLedgerTemplateKey;
            set => owner.selectedLedgerTemplateKey = value;
        }

        public DiagramStageSurface Diagram => owner.DiagramStage;

        public PlatformCommunityDiagramDesktopCanvasPresentation BuildInlinePresentation(
            IReadOnlyList<원장블록노드> nodes,
            IReadOnlyList<원장블록연결선> edges)
            => owner.BuildDesktopDiagramPresentation(
                nodes,
                edges,
                DefaultDiagramCanvasMinHeight,
                useStageCanvasStyle: false);

        public IReadOnlyList<CommunityLedgerBlockResponse> ResolveBlocks(원장블록노드 node)
            => owner.흐름노드관련블록해결(node);

        public IReadOnlyList<CommunityLedgerProcessingSurfaceResponse> ResolveSurfaces(원장블록노드 node)
            => owner.흐름노드처리표면해결(node);

        public Color ResolveSurfaceColor(CommunityLedgerProcessingSurfaceResponse surface)
            => owner.ResolveSurfaceStatusColor(surface);

        public string ResolveSurfaceRoute(CommunityLedgerProcessingSurfaceResponse surface)
            => $"{owner.ResolveSurfaceMethod(surface)} {owner.BuildResolvedApiRoute(surface)}";

        public PlatformCommunityDiagramWorkspaceViewModel DiagramWorkspace => owner.DiagramWorkspace;

        public IReadOnlyList<string> ApiRouteParameterNames => owner.SelectedApiRouteParameterNames;

        public IReadOnlyList<PlatformCommunityLedgerApiSurfacePresentation> ApiSurfaces
            => owner.BuildLedgerApiSurfacePresentations();

        public string? ResultMessage => owner.원장전송결과메시지;

        public Severity ResultSeverity => owner.원장전송결과Severity;

        public void Analyze() => owner.원함분석하기();

        public void SelectLedger(string ledgerId) => owner.현재원장컨텍스트불러오기(ledgerId);

        public void OpenDiagram() => owner.현재원장다이어그램열기();

        public void PrepareCommunityDraft() => owner.PrepareLedgerCommunityDraft();

        public void ClearMetadataResult() => owner.ClearLedgerMetadataResult();

        public void PrepareApiRoute(CommunityLedgerProcessingSurfaceResponse surface)
            => owner.원장Api경로준비(surface);
    }
}
