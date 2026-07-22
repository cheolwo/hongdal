using MudBlazor;
using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Ui.Common.Areas.App.Components.WarehouseOperations;

namespace Ssalddel.Tests.Architecture;

public sealed class InboundReceivingWorkspaceCompositionTests
{
    [Fact]
    public void 입고수령_루트는_페이지상태와업무영역만조립한다()
    {
        var componentDirectory = FindComponentDirectory();
        var workspacePath = Path.Combine(componentDirectory, "SsalddelInboundReceivingWorkspace.razor");
        var source = File.ReadAllText(workspacePath);

        Assert.True(File.ReadLines(workspacePath).Count() <= 50);
        Assert.Contains("<InboundReceivingBoundaryNotice", source);
        Assert.Contains("<InboundReceivingPageState", source);
        Assert.Contains("<InboundReceivingSearchPanel", source);
        Assert.Contains("<InboundReceivingCandidatePanel", source);
        Assert.Contains("<InboundReceivingUnplannedRequestForm", source);
        Assert.Contains("<InboundReceivingPersistedResult", source);
        Assert.DoesNotContain("<MudTextField", source);
        Assert.DoesNotContain("<MudSelect", source);
        Assert.DoesNotContain("<MudGrid", source);
        Assert.DoesNotContain("@foreach", source);
        Assert.DoesNotContain("@code", source);
    }

    [Theory]
    [InlineData("InboundReceivingBoundaryNotice.razor")]
    [InlineData("InboundReceivingBoundaryNotice.razor.css")]
    [InlineData("InboundReceivingPageState.razor")]
    [InlineData("InboundReceivingPageState.razor.css")]
    [InlineData("InboundReceivingSearchPanel.razor")]
    [InlineData("InboundReceivingSearchPanel.razor.css")]
    [InlineData("InboundReceivingCandidatePanel.razor")]
    [InlineData("InboundReceivingCandidatePanel.razor.css")]
    [InlineData("InboundReceivingUnplannedRequestForm.razor")]
    [InlineData("InboundReceivingUnplannedRequestForm.razor.css")]
    [InlineData("InboundReceivingPersistedResult.razor")]
    [InlineData("InboundReceivingPersistedResult.razor.css")]
    [InlineData("InboundReceivingPresentation.cs")]
    [InlineData("SsalddelInboundReceivingWorkspace.razor.cs")]
    public void 입고수령_상태와화면과표현책임은_전용파일로존재한다(string fileName)
    {
        var path = Path.Combine(FindComponentDirectory(), fileName);

        Assert.True(File.Exists(path), $"입고상품 수령 전용 파일이 없습니다: {fileName}");
        Assert.NotEmpty(File.ReadAllText(path));
    }

    [Fact]
    public void 저장과선택은_성공한정확한입고Id만경로에반영한다()
    {
        var componentDirectory = FindComponentDirectory();
        var coordinator = File.ReadAllText(Path.Combine(
            componentDirectory,
            "SsalddelInboundReceivingWorkspace.razor.cs"));
        var viewModels = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "ViewModels",
            "입고상품수령페이지ViewModels.cs"));

        Assert.Contains("ViewModel.현장입고등록후조회Async()", coordinator);
        Assert.Contains("_loadedInboundId = item.Id", coordinator);
        Assert.Contains("OnInboundSelected.InvokeAsync(item.Id)", coordinator);
        Assert.Contains("ViewModel.입고선택Async(inboundId)", coordinator);
        Assert.Contains("var reloaded = await 입고선택Async(created.Id", viewModels);
        Assert.Contains("상세.항목?.Id == inboundId", viewModels);
        Assert.DoesNotContain("CompleteInbound", coordinator, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Inventory", coordinator, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void 입고수령_표현규칙은_빈값과상태와작업보드경로를일관되게만든다()
    {
        Assert.Equal("-", InboundReceivingPresentation.Display("  "));
        Assert.Equal("상품 A", InboundReceivingPresentation.Display("상품 A"));
        Assert.Equal("-", InboundReceivingPresentation.FormatCreatedAt(default));
        Assert.Equal(Color.Info, InboundReceivingPresentation.StatusColor(입고상태코드.예정));
        Assert.Equal(Color.Default, InboundReceivingPresentation.StatusColor("입고완료"));
        Assert.Equal(
            "/work-board?inboundId=81",
            InboundReceivingPresentation.WorkBoardHref(" /work-board ", 81));
        Assert.Equal(
            "/work-board?mode=readonly&inboundId=81",
            InboundReceivingPresentation.WorkBoardHref("/work-board?mode=readonly", 81));
        Assert.Null(InboundReceivingPresentation.WorkBoardHref("/work-board", null));
    }

    [Fact]
    public void 입고수령_화면은_좁은폭에서동작영역을단일열과터치크기로전환한다()
    {
        var componentDirectory = FindComponentDirectory();
        var responsiveFiles = new[]
        {
            "InboundReceivingPageState.razor.css",
            "InboundReceivingSearchPanel.razor.css",
            "InboundReceivingCandidatePanel.razor.css",
            "InboundReceivingUnplannedRequestForm.razor.css",
            "InboundReceivingPersistedResult.razor.css"
        };

        foreach (var fileName in responsiveFiles)
        {
            var css = File.ReadAllText(Path.Combine(componentDirectory, fileName));
            Assert.Contains("@media (max-width: 720px)", css);
            Assert.Contains("min-height: 44px", css);
        }
    }

    private static string FindComponentDirectory()
        => Path.Combine(
            FindRepositoryRoot(),
            "Ssalddel.Ui.Common",
            "Areas",
            "App",
            "Components",
            "WarehouseOperations");

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }
}
