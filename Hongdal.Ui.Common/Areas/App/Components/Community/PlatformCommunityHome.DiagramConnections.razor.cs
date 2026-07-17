using System.Globalization;
using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private void StartDiagramEdgeFromSelectedNode()
    {
        var selectedNode = 선택원장블록노드;
        if (selectedNode is null || !CanStartDiagramConnection(selectedNode))
        {
            return;
        }

        DiagramCanvas.BeginConnection(
            selectedNode.Title,
            BuildDefaultEdgeLabel(selectedNode, null),
            BuildDiagramConnectionGuidance(selectedNode));
    }

    private static 원장블록노드 ToSharedLedgerDiagramNode(DiagramNodeDto node)
        => new(
            node.Title,
            string.IsNullOrWhiteSpace(node.GroupLabel) ? "공개 원장" : node.GroupLabel,
            string.IsNullOrWhiteSpace(node.Description) ? "공개된 다이어그램 노드" : node.Description,
            string.IsNullOrWhiteSpace(node.Kind) ? "state" : node.Kind,
            ResolveSharedDiagramNodeColor(node.Kind));

    private static Color ResolveSharedDiagramNodeColor(string? kind)
        => kind?.Trim().ToLowerInvariant() switch
        {
            "order" or "request" => Color.Primary,
            "warehouse" or "inventory" => Color.Warning,
            "delivery" or "transport" => Color.Info,
            "payment" or "settlement" => Color.Success,
            _ => Color.Secondary
        };

    private void CancelDiagramEdgeCreation()
    {
        connectionStartNodeTitle = null;
        diagramConnectionMessage = null;
        CancelDiagramHandleDrag();
    }

    private bool AddCustomDiagramEdge(string fromTitle, string toTitle)
        => AddCustomDiagramEdge(
            fromTitle,
            toTitle,
            DiagramConnectionHandleKind.Right,
            DiagramConnectionHandleKind.Left);

    private bool AddCustomDiagramEdge(
        string fromTitle,
        string toTitle,
        DiagramConnectionHandleKind fromHandle,
        DiagramConnectionHandleKind toHandle)
    {
        var nodes = 선택원장블록흐름도.Nodes;
        var fromNode = nodes.FirstOrDefault(node =>
            string.Equals(node.Title, fromTitle, StringComparison.OrdinalIgnoreCase));
        var toNode = nodes.FirstOrDefault(node =>
            string.Equals(node.Title, toTitle, StringComparison.OrdinalIgnoreCase));
        if (fromNode is null ||
            toNode is null ||
            !CanStartDiagramConnection(fromNode, fromHandle) ||
            !CanCompleteDiagramConnection(toNode, toHandle))
        {
            diagramConnectionSeverity = Severity.Warning;
            diagramConnectionMessage = "선택한 연결점의 입력·출력 방향을 확인해 주세요.";
            return false;
        }

        if (!CanConnectDiagramNodes(fromNode, toNode))
        {
            diagramConnectionSeverity = Severity.Warning;
            diagramConnectionMessage = BuildDiagramFormConnectionFailureMessage(fromNode, toNode);
            return false;
        }

        var mutation = DiagramCanvas.AddOrSelectCustomEdge(
            fromTitle,
            toTitle,
            fromHandle,
            toHandle,
            newConnectionLabel);
        if (!mutation.IsNew)
        {
            diagramConnectionSeverity = Severity.Info;
            diagramConnectionMessage = $"'{fromNode.Title}'과(와) '{toNode.Title}'은 이미 연결되어 있습니다.";
            return true;
        }

        diagramConnectionSeverity = Severity.Success;
        diagramConnectionMessage = $"'{fromNode.Title}'에서 '{toNode.Title}'로 연결했습니다.";
        return true;
    }

    private async Task BeginDiagramHandleDragAsync(
        원장블록노드 node,
        DiagramConnectionHandleKind handle,
        MouseEventArgs eventArgs)
    {
        if (eventArgs.Button != 0)
        {
            return;
        }

        if (activeDiagramHandleDrag is not null)
        {
            CompleteDiagramHandleDrag(node, handle);
            return;
        }

        if (!CanStartDiagramConnection(node, handle))
        {
            return;
        }

        StartDiagramHandleDrag(node, handle);
        await UpdateDiagramDragPointerAsync(eventArgs.ClientX, eventArgs.ClientY);
    }

    private async Task BeginDiagramHandlePointerDragAsync(
        원장블록노드 node,
        DiagramConnectionHandleKind handle,
        PointerEventArgs eventArgs)
    {
        if (eventArgs.Button != 0)
        {
            return;
        }

        if (activeDiagramHandleDrag is not null)
        {
            CompleteDiagramHandleDrag(node, handle);
            return;
        }

        if (!CanStartDiagramConnection(node, handle))
        {
            return;
        }

        StartDiagramHandleDrag(node, handle);
        await UpdateDiagramDragPointerAsync(eventArgs.ClientX, eventArgs.ClientY);
    }

    private void StartDiagramHandleDrag(원장블록노드 node, DiagramConnectionHandleKind handle)
    {
        if (!CanStartDiagramConnection(node, handle))
        {
            return;
        }

        DiagramCanvas.BeginHandleDrag(
            node.Title,
            handle,
            BuildDefaultEdgeLabel(node, null),
            BuildDiagramConnectionGuidance(node));
    }

    private void HandleDiagramHandleClick(원장블록노드 node, DiagramConnectionHandleKind handle)
    {
        if (DiagramCanvas.ConsumeSuppressedHandleClick())
        {
            return;
        }

        if (activeDiagramHandleDrag is null)
        {
            if (!CanStartDiagramConnection(node, handle))
            {
                return;
            }

            StartDiagramHandleDrag(node, handle);
            return;
        }

        CompleteDiagramHandleDrag(node, handle);
    }

    private async Task HandleDiagramCanvasMouseMoveAsync(MouseEventArgs eventArgs)
    {
        if (activeDiagramHandleDrag is null)
        {
            return;
        }

        diagramHandleDragMoved = true;
        await UpdateDiagramDragPointerAsync(eventArgs.ClientX, eventArgs.ClientY);
    }

    private async Task HandleDiagramCanvasPointerMoveAsync(PointerEventArgs eventArgs)
    {
        if (activeDiagramHandleDrag is null)
        {
            return;
        }

        diagramHandleDragMoved = true;
        await UpdateDiagramDragPointerAsync(eventArgs.ClientX, eventArgs.ClientY);
    }

    private async Task HandleDiagramCanvasMouseUpAsync(MouseEventArgs eventArgs)
    {
        await CompleteDiagramHandleDragFromClientPointAsync(eventArgs.ClientX, eventArgs.ClientY);
    }

    private async Task HandleDiagramCanvasPointerUpAsync(PointerEventArgs eventArgs)
    {
        await CompleteDiagramHandleDragFromClientPointAsync(eventArgs.ClientX, eventArgs.ClientY);
    }

    private async Task CompleteDiagramHandleDragFromClientPointAsync(double clientX, double clientY)
    {
        if (activeDiagramHandleDrag is null)
        {
            return;
        }

        var module = await GetDiagramJsModuleAsync();
        var hit = module is null
            ? null
            : await module.InvokeAsync<DiagramHandleHit?>(
                "findConnectionHandle",
                clientX,
                clientY);

        if (hit is null ||
            !TryParseDiagramHandle(hit.Handle, out var targetHandle))
        {
            if (diagramHandleDragMoved)
            {
                CancelDiagramHandleDrag();
            }

            return;
        }

        var targetNode = 선택원장블록흐름도.Nodes.FirstOrDefault(node =>
            string.Equals(node.Title, hit.NodeTitle, StringComparison.OrdinalIgnoreCase));
        if (targetNode is null)
        {
            CancelDiagramHandleDrag();
            return;
        }

        CompleteDiagramHandleDrag(targetNode, targetHandle);
    }

    private async Task UpdateDiagramDragPointerAsync(double clientX, double clientY)
    {
        var module = await GetDiagramJsModuleAsync();
        if (module is null)
        {
            return;
        }

        diagramDragPointer = await module.InvokeAsync<DiagramDragPoint>(
            "toDiagramPoint",
            diagramCanvasElement,
            clientX,
            clientY);
    }

    private async ValueTask<IJSObjectReference?> GetDiagramJsModuleAsync()
    {
        if (diagramJsModule is not null)
        {
            return diagramJsModule;
        }

        try
        {
            diagramJsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/js/platformDiagram.js");
            return diagramJsModule;
        }
        catch
        {
            return null;
        }
    }

    private void CompleteDiagramHandleDrag(원장블록노드 node, DiagramConnectionHandleKind targetHandle)
    {
        if (activeDiagramHandleDrag is null)
        {
            return;
        }

        var drag = activeDiagramHandleDrag;
        var completed = false;
        if (!CanCompleteDiagramConnection(node, targetHandle))
        {
            CancelDiagramHandleDrag();
            return;
        }

        if (string.Equals(drag.NodeTitle, node.Title, StringComparison.OrdinalIgnoreCase))
        {
            CancelDiagramHandleDrag();
            return;
        }

        if (!string.Equals(drag.NodeTitle, node.Title, StringComparison.OrdinalIgnoreCase))
        {
            completed = AddCustomDiagramEdge(drag.NodeTitle, node.Title, drag.Handle, targetHandle);
            if (completed)
            {
                선택원장블록노드제목 = node.Title;
            }
        }

        CancelDiagramHandleDrag();
        suppressNextDiagramHandleClick = completed;
    }

    private void CancelDiagramHandleDrag()
        => DiagramCanvas.CancelHandleDrag();

    private void SelectDiagramEdge(원장블록연결선 edge)
    {
        DiagramCanvas.SelectEdge(edge.Id);
        선택원장블록노드제목 = null;
        nodeDetailPanelNode = null;
    }

    private void ClearDiagramEdgeSelection()
        => DiagramCanvas.ClearEdgeSelection();

    private void UpdateSelectedDiagramEdgeLabel(string value)
        => DiagramCanvas.UpdateSelectedEdgeLabel(value);

    private void UpdateSelectedDiagramEdgeStyle(DiagramEdgeStyleKind style)
        => DiagramCanvas.UpdateSelectedEdgeStyle(style);

    private void DeleteSelectedDiagramEdge()
        => DiagramCanvas.DeleteSelectedEdge();

    private static string? BuildDiagramConnectionGuidance(원장블록노드 node)
        => string.Equals(node.Kind, "form", StringComparison.OrdinalIgnoreCase)
            ? $"'{node.Title}' 연결 대상: {PlatformDiagramFormNodeCatalog.GetConnectionRule(node.FormKind).Description}"
            : null;

}
