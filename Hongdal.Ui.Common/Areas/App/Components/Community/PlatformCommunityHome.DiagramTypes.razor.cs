using Hongdal.Contracts.Common.Community;
using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private sealed record 도형상세동작(
        string Url,
        string Description,
        string Icon,
        Color Color);

    private sealed record 다이어그램레이어정의(
        string Key,
        string Label,
        string Description,
        int DisplayOrder,
        int ConflictPriority,
        string Icon,
        Color Color,
        bool DefaultVisible,
        bool IsLocked = false);

    private sealed record 도형레이어배지(
        string LayerKey,
        string Label,
        string Description,
        string Icon,
        int DisplayOrder,
        int ConflictPriority,
        string CssClass);

    private sealed record 도형레이어신호(
        string LayerKey,
        int DisplayOrder,
        int ConflictPriority,
        string CssClass,
        도형레이어배지 Badge,
        bool ShouldEmphasizeNode);

    private sealed record 도형입력항목(
        string Label,
        string InputType,
        string Description,
        bool IsRequired);

    private enum 원장블록처리상태
    {
        대기,
        진행중,
        완료
    }

    private sealed record 현재원장컨텍스트(
        string Id,
        string Title,
        string TemplateKey,
        string StateLabel,
        string LastUpdatedLabel,
        string Wish,
        string ConditionSummary,
        string Summary,
        IReadOnlyDictionary<string, string> ContextValues)
    {
        public IReadOnlyList<현재원장컨텍스트연결선> DiagramEdges { get; init; } = [];
    }

    private sealed record 현재원장컨텍스트연결선(
        string FromTitle,
        string ToTitle,
        string Label,
        DiagramConnectionHandleKind FromHandle = DiagramConnectionHandleKind.Right,
        DiagramConnectionHandleKind ToHandle = DiagramConnectionHandleKind.Left,
        DiagramEdgeStyleKind Style = DiagramEdgeStyleKind.Curve);

    private sealed record 원장블록흐름도(
        IReadOnlyList<원장블록노드> Nodes,
        IReadOnlyList<string> Rules);

    private sealed record 원장블록노드(
        string Title,
        string GroupLabel,
        string Description,
        string Kind,
        Color Color,
        string? Condition = null,
        string? 스티커이미지Key = null);

    private sealed record 원장블록연결선(
        string Id,
        string FromTitle,
        string ToTitle,
        string Label,
        bool IsCustom,
        DiagramConnectionHandleKind FromHandle = DiagramConnectionHandleKind.Right,
        DiagramConnectionHandleKind ToHandle = DiagramConnectionHandleKind.Left,
        DiagramEdgeStyleKind Style = DiagramEdgeStyleKind.Curve);

    private sealed record 다이어그램커뮤니티초안안내(
        CommunityLedgerTemplateResponse Template,
        string BoardCategory,
        string Title,
        string Body,
        string Reason,
        bool IsReportBoardPost);

    private sealed record 다이어그램대화방표시메시지(
        string Id,
        string SenderUserId,
        string SenderDisplayName,
        string Message,
        string MessageKind,
        DateTime SentAt,
        bool IsMine,
        bool IsSystem);

    private sealed record 다이어그램창고대행후보(
        string Key,
        long? WarehouseId,
        string Name,
        string ScopeLabel,
        string ProxyTypeCode,
        string ProxyTypeLabel,
        string Address,
        string Description,
        bool IsWorkspaceWarehouse);

    private enum DiagramConnectionHandleKind
    {
        Top,
        Right,
        Bottom,
        Left
    }

    private enum DiagramEdgeStyleKind
    {
        Curve,
        Straight,
        Elbow
    }

    private sealed record DiagramPoint(double X, double Y);

    private sealed record DiagramDragPoint(double X, double Y);

    private sealed record DiagramHandleDrag(string NodeTitle, DiagramConnectionHandleKind Handle);

    private sealed record DiagramHandleHit(string NodeTitle, string Handle);

    private sealed record DiagramEdgeGeometry(
        string Path,
        double LabelX,
        double LabelY);
}
