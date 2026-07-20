using System.Globalization;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private void OpenCommunityMode()
    {
        isCompactHomeSummary = false;
        DiagramPalette.SetDiagramMode(false);
        HomeModeState.SetWorkMode(false);
        diagramConnectionMessage = null;
    }

    private void OpenPublicCommunity()
    {
        if (UseDedicatedCommunityRoutes && WorkspaceOnly)
        {
            Navigation.NavigateTo("/community");
            return;
        }

        OpenCommunityMode();
    }

    private void OpenCompactHomeSummary()
    {
        isComposeOpen = false;
        isLedgerDetailOpen = false;
        isLedgerPickerOpen = false;
        isOrderLedgerHierarchyOpen = false;
        isCompactHomeSummary = UseCompactHomeSummary;
        DiagramPalette.SetDiagramMode(false);
        HomeModeState.SetWorkMode(false);
        diagramConnectionMessage = null;
    }

    private async Task 공통홈게시판열기Async()
    {
        if (UseDedicatedCommunityRoutes && WorkspaceOnly)
        {
            Navigation.NavigateTo("/community");
            return;
        }

        await OpenBaguaAnchorAsync(workMode: false, "community-board-list");
    }

    private void 공통홈글쓰기열기()
    {
        OpenCommunityMode();
        OpenCompose();
    }

    private async Task 공통홈베스트글열기Async(공통홈베스트글요약 베스트글)
    {
        OpenCommunityMode();
        실시간베스트글열기(베스트글);
        if (베스트글.게시글Id is long 게시글Id)
        {
            try
            {
                await LoadPostDetailAsync(게시글Id);
            }
            catch (HttpRequestException)
            {
                statusSeverity = Severity.Warning;
                statusMessage = "게시글 상세 정보를 불러오지 못했습니다.";
            }
        }

        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        var currentPage = Navigation.Uri.Split('#', 2)[0];
        Navigation.NavigateTo($"{currentPage}#community-forum-list", replace: true);
    }

    private void OpenWorkMode()
    {
        if (UseDedicatedCommunityRoutes)
        {
            Navigation.NavigateTo("/community/workspace");
            return;
        }

        isCompactHomeSummary = false;
        isComposeOpen = false;
        isLedgerDetailOpen = false;
        isLedgerPickerOpen = false;
        DiagramPalette.SetDiagramMode(false);
        HomeModeState.SetWorkMode(true);
    }

    private IReadOnlyList<SsalddelCardinalNavigationOption> EffectiveCardinalNavigationOptions
    {
        get
        {
            var inferredOptions = BuildRoleCardinalNavigationOptions();
            if (CardinalNavigationOptions.Count == 0)
            {
                return inferredOptions;
            }

            return inferredOptions
                .Select(fallback => CardinalNavigationOptions.FirstOrDefault(option =>
                    string.Equals(option.TrigramKey, fallback.TrigramKey, StringComparison.OrdinalIgnoreCase)) ?? fallback)
                .ToArray();
        }
    }

    private string EffectiveBaguaCenterSymbol
        => string.IsNullOrWhiteSpace(BaguaCenterSymbol)
            ? DecorationState.BaguaSymbol
            : BaguaCenterSymbol;

    private EventCallback BaguaCenterSelectedCallback
        => BaguaCenterOpensCommunity && !BaguaCenterShowsTaegeuk
            ? EventCallback.Factory.Create(this, HandleBaguaCenterSelectedAsync)
            : default;

    private EventCallback BaguaTaegeukYangSelectedCallback
        => BaguaCenterShowsTaegeuk
            ? EventCallback.Factory.Create(this, HandleBaguaCenterSelectedAsync)
            : default;

    private EventCallback BaguaTaegeukYinSelectedCallback
        => BaguaCenterShowsTaegeuk
            ? EventCallback.Factory.Create(this, HandleBaguaCenterStoreSelected)
            : default;

    private EventCallback PrajnaUpayaUpayaSelectedCallback
        => ShowPrajnaUpayaNavigator
            ? EventCallback.Factory.Create(this, HandlePrajnaUpayaUpayaSelected)
            : default;

    private EventCallback PrajnaUpayaPrajnaSelectedCallback
        => !string.IsNullOrWhiteSpace(PrajnaHref)
            ? EventCallback.Factory.Create(this, () => NavigateFromPrajnaUpaya(PrajnaHref))
            : default;

    private IReadOnlyList<SsalddelCardinalNavigationOption> BuildRoleCardinalNavigationOptions()
    {
        var roleActions = QuickActions
            .Select(action => (Title: action.Title, Href: action.Href))
            .Concat(Workspaces.Select(workspace => (Title: workspace.Title, Href: workspace.EntryHref)))
            .Where(action => !string.IsNullOrWhiteSpace(action.Href))
            .DistinctBy(action => action.Href, StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToArray();
        var defaults = new[]
        {
            new SsalddelCardinalNavigationOption("li", "리", "☲", "남", "판매", SsalddelCardinalNavigationActionKinds.Diagram),
            new SsalddelCardinalNavigationOption("dui", "태", "☱", "서", "창고", SsalddelCardinalNavigationActionKinds.Work),
            new SsalddelCardinalNavigationOption("kan", "감", "☵", "북", "운송", SsalddelCardinalNavigationActionKinds.CommunityHome),
            new SsalddelCardinalNavigationOption("zhen", "진", "☳", "동", "주문", SsalddelCardinalNavigationActionKinds.Compose)
        };

        return defaults
            .Select((fallback, index) => index < roleActions.Length
                ? fallback with
                {
                    DestinationLabel = roleActions[index].Title,
                    ActionKind = SsalddelCardinalNavigationActionKinds.Route,
                    Target = roleActions[index].Href
                }
                : fallback)
            .ToArray();
    }

    private async Task HandleBaguaDestinationSelectedAsync(SsalddelCardinalNavigationOption option)
    {
        isBaguaNavigatorOpen = false;
        if (option.ActionKind == SsalddelCardinalNavigationActionKinds.Route &&
            !string.IsNullOrWhiteSpace(option.Target))
        {
            Navigation.NavigateTo(option.Target);
            return;
        }

        switch (option.ActionKind)
        {
            case SsalddelCardinalNavigationActionKinds.CommunityHome:
                await OpenBaguaAnchorAsync(workMode: false, "community-home-top");
                break;
            case SsalddelCardinalNavigationActionKinds.Compose:
                OpenCompose();
                break;
            case SsalddelCardinalNavigationActionKinds.Diagram:
                현재원장다이어그램열기();
                break;
            case SsalddelCardinalNavigationActionKinds.Work:
                await OpenBaguaAnchorAsync(workMode: true, "community-work-panel");
                break;
        }
    }

    private Task HandleBaguaCenterTransferCompletedAsync(SsalddelCardinalNavigationOption option)
    {
        isBaguaNavigatorOpen = false;
        if (UseBaguaRoleTransitionPages)
        {
            var route = string.IsNullOrWhiteSpace(BaguaPerspectiveRoleCode)
                ? BaguaRoleTransitionRoutes.BuildRolePicker(option.TrigramKey, BaguaTrigramKeys.Gen)
                : BaguaRoleTransitionRoutes.Build(
                    BaguaPerspectiveRoleCode,
                    option.TrigramKey,
                    BaguaTrigramKeys.Gen);
            Navigation.NavigateTo(route);
            return Task.CompletedTask;
        }

        Navigation.NavigateTo("/community/group-purchase");
        return Task.CompletedTask;
    }

    private async Task HandleBaguaCenterSelectedAsync()
    {
        isBaguaNavigatorOpen = false;
        await OpenBaguaAnchorAsync(workMode: false, "community-forum-list");
    }

    private void HandleBaguaCenterStoreSelected()
    {
        isBaguaNavigatorOpen = false;
        OpenDecorationStore();
    }

    private void NavigateFromPrajnaUpaya(string? href)
    {
        if (string.IsNullOrWhiteSpace(href))
        {
            return;
        }

        isBaguaNavigatorOpen = false;
        Navigation.NavigateTo(href);
    }

    private void HandlePrajnaUpayaUpayaSelected()
    {
        if (!string.IsNullOrWhiteSpace(UpayaHref))
        {
            NavigateFromPrajnaUpaya(UpayaHref);
            return;
        }

        isBaguaNavigatorOpen = false;
        OpenWorkMode();
    }

    private void OpenDecorationStore()
        => Navigation.NavigateTo("/community/decorations");

    private async Task OpenBaguaAnchorAsync(bool workMode, string anchorId)
    {
        if (workMode)
        {
            OpenWorkMode();
        }
        else
        {
            OpenCommunityMode();
        }

        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        var currentPage = Navigation.Uri.Split('#', 2)[0];
        Navigation.NavigateTo($"{currentPage}#{anchorId}", replace: true);
    }

}
