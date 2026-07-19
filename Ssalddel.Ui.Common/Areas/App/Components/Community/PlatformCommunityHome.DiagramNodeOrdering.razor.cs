using System.Globalization;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private bool CanMove선택원장블록노드(int offset)
    {
        _ = 정렬된원장블록노드목록가져오기(선택원장블록흐름도);
        return DiagramCanvas.CanMoveSelectedNode(offset);
    }

    private void Move선택원장블록노드(int offset)
    {
        _ = 정렬된원장블록노드목록가져오기(선택원장블록흐름도);
        DiagramCanvas.MoveSelectedNode(offset);
    }

    private void 원장블록흐름도배치초기화()
    {
        sharedLedgerDiagramSnapshot = null;
        DiagramCanvas.Reset();
        nodeDetailPanelNode = null;
    }
}
