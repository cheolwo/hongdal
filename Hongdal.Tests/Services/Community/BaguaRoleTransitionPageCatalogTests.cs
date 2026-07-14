using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Models;

namespace Hongdal.Tests.Services.Community;

public sealed class BaguaRoleTransitionPageCatalogTests
{
    [Fact]
    public void Routes_ProvideOneUniqueCanonicalAddressForEveryRolePerspective()
    {
        var routes = BaguaTransitionCatalog.Roles
            .SelectMany(role => BaguaTransitionCatalog.All.Select(transition =>
                BaguaRoleTransitionRoutes.Build(
                    role.RoleCode,
                    transition.SourceTrigramKey,
                    transition.TargetTrigramKey)))
            .ToArray();

        Assert.Equal(125, routes.Length);
        Assert.Equal(125, routes.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(routes, route => Assert.StartsWith("/community/bagua/", route));
        Assert.Contains("/community/bagua/orderer/zhen/gen", routes);
        Assert.Contains("/community/bagua/seller/zhen/gen", routes);
        Assert.Contains("/community/bagua/cooperative-coordinator/gen/zhen", routes);
    }

    [Fact]
    public void RolePickerRoute_IdentifiesTheTransitionWithoutGuessingTheViewerRole()
    {
        var route = BaguaRoleTransitionRoutes.BuildRolePicker(
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Gen);

        Assert.Equal("/community/bagua/zhen/gen", route);
        Assert.DoesNotContain(BaguaActorRoleCodes.Orderer, route);
        Assert.DoesNotContain(BaguaActorRoleCodes.Seller, route);
    }

    [Fact]
    public void PageModels_CoverAllPerspectivesWithWorkflowAndProcessorHandoff()
    {
        var models = BaguaTransitionCatalog.Roles
            .SelectMany(role => BaguaTransitionCatalog.All.Select(transition =>
                BaguaRoleTransitionPageCatalog.Build(
                    role.RoleCode,
                    transition.SourceTrigramKey,
                    transition.TargetTrigramKey)))
            .ToArray();

        Assert.Equal(125, models.Length);
        Assert.Equal(125, models
            .Select(model => model.Animation.AssetSlotKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count());
        Assert.All(models, model =>
        {
            Assert.False(string.IsNullOrWhiteSpace(model.Animation.Title));
            Assert.False(string.IsNullOrWhiteSpace(model.Animation.Storyboard));
            Assert.True(model.Animation.DurationMilliseconds >= 1000);
            Assert.NotEmpty(model.Animation.ContributorDisciplines);
            Assert.NotEmpty(model.Steps);
            Assert.Equal(Enumerable.Range(1, model.Steps.Count), model.Steps.Select(step => step.Number));
            Assert.False(string.IsNullOrWhiteSpace(model.TargetWorkspaceName));
            Assert.StartsWith("/", model.TargetWorkspaceHref);
            Assert.Contains("권한", model.PermissionNotice);
            Assert.Contains("서버", model.PermissionNotice);
        });
    }

    [Fact]
    public void GovernancePage_UsesProposalObjectionSignatureAndExecutionFlow()
    {
        var page = BaguaRoleTransitionPageCatalog.Build(
            BaguaActorRoleCodes.CooperativeCoordinator,
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Gen);

        Assert.Equal(6, page.Steps.Count);
        Assert.Equal(
            ["안건 제안", "참여·수요 확인", "이의 검토", "확정안 작성", "전자서명", "실행 연결"],
            page.Steps.Select(step => step.Title));
        Assert.Equal("공동구매 합의", page.TargetWorkspaceName);
        Assert.Equal("/community/group-purchase", page.TargetWorkspaceHref);
        Assert.Equal(BaguaRolePerspectiveModes.Governor, page.Perspective.PerspectiveMode);
        Assert.Equal("gather", page.Animation.MotionKind);
        Assert.Equal("✍", page.Animation.PayloadSymbol);
    }

    [Fact]
    public void ZhenToKanAnimation_DescribesAnOrdererRunningFromOrderToTransport()
    {
        var page = BaguaRoleTransitionPageCatalog.Build(
            BaguaActorRoleCodes.Orderer,
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Kan);

        Assert.Equal("bagua-motion:orderer:order-to-transport", page.Animation.AssetSlotKey);
        Assert.Equal("relay", page.Animation.MotionKind);
        Assert.Equal("➜", page.Animation.PayloadSymbol);
        Assert.Contains("주문에서 운송", page.Animation.Title);
        Assert.Contains("주문자 캐릭터", page.Animation.Storyboard);
        Assert.Contains("캐릭터 애니메이션", page.Animation.ContributorDisciplines);
        Assert.Contains("SVG·Lottie 제작", page.Animation.ContributorDisciplines);
    }

    [Fact]
    public void SameTransition_BuildsDifferentRolePagesWithoutChangingTheCanonicalWorkflow()
    {
        var orderer = BaguaRoleTransitionPageCatalog.Build(
            BaguaActorRoleCodes.Orderer,
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Li);
        var seller = BaguaRoleTransitionPageCatalog.Build(
            BaguaActorRoleCodes.Seller,
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Li);

        Assert.Equal(orderer.Transition, seller.Transition);
        Assert.Equal(orderer.Steps.Select(step => step.Title), seller.Steps.Select(step => step.Title));
        Assert.NotEqual(orderer.Perspective.ViewTitle, seller.Perspective.ViewTitle);
        Assert.NotEqual(orderer.Perspective.PrimaryAction, seller.Perspective.PrimaryAction);
        Assert.Equal(BaguaRolePerspectiveModes.Initiator, orderer.Perspective.PerspectiveMode);
        Assert.Equal(BaguaRolePerspectiveModes.Receiver, seller.Perspective.PerspectiveMode);
    }

    [Fact]
    public void InvalidRoleOrTrigram_DoesNotSilentlyFallBackToAnotherPerspective()
    {
        Assert.Throws<KeyNotFoundException>(() => BaguaRoleTransitionRoutes.Build(
            "unknown-role",
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Gen));
        Assert.Throws<KeyNotFoundException>(() => BaguaRoleTransitionPageCatalog.Build(
            BaguaActorRoleCodes.Orderer,
            "unknown-trigram",
            BaguaTrigramKeys.Gen));
    }
}
