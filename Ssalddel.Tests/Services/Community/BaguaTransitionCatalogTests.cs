using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class BaguaTransitionCatalogTests
{
    [Fact]
    public void Areas_DefineFiveBusinessTrigramsInStableOrder()
    {
        Assert.Equal(5, BaguaTransitionCatalog.Areas.Count);
        Assert.Equal(
            [
                (BaguaTrigramKeys.Zhen, BaguaBusinessCodes.Order, "주문"),
                (BaguaTrigramKeys.Li, BaguaBusinessCodes.Sales, "판매"),
                (BaguaTrigramKeys.Dui, BaguaBusinessCodes.Warehouse, "창고"),
                (BaguaTrigramKeys.Kan, BaguaBusinessCodes.Transport, "운송"),
                (BaguaTrigramKeys.Gen, BaguaBusinessCodes.Agreement, "합의")
            ],
            BaguaTransitionCatalog.Areas
                .Select(area => (area.TrigramKey, area.BusinessCode, area.BusinessName))
                .ToArray());
    }

    [Fact]
    public void All_CoversTheCompleteDirectedFiveByFiveMatrixWithoutDuplicates()
    {
        var transitions = BaguaTransitionCatalog.All;
        var pairs = transitions
            .Select(transition => $"{transition.SourceTrigramKey}>{transition.TargetTrigramKey}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Equal(25, transitions.Count);
        Assert.Equal(25, pairs.Count);
        Assert.Equal(25, transitions.Select(transition => transition.TransitionKey)
            .Distinct(StringComparer.OrdinalIgnoreCase).Count());

        foreach (var source in BaguaTransitionCatalog.Areas)
        {
            foreach (var target in BaguaTransitionCatalog.Areas)
            {
                Assert.Contains($"{source.TrigramKey}>{target.TrigramKey}", pairs);
            }
        }

        Assert.All(transitions, transition =>
        {
            Assert.False(string.IsNullOrWhiteSpace(transition.PageTitle));
            Assert.False(string.IsNullOrWhiteSpace(transition.Purpose));
            Assert.False(string.IsNullOrWhiteSpace(transition.WorkflowKind));
        });
    }

    [Fact]
    public void HomeTransitions_StayWithinTheirAreaAndNeedNoSourceSelection()
    {
        var homeTransitions = BaguaTransitionCatalog.All
            .Where(transition => transition.WorkflowKind == BaguaTransitionWorkflowKinds.Home)
            .ToArray();

        Assert.Equal(5, homeTransitions.Length);
        Assert.All(homeTransitions, transition =>
        {
            Assert.Equal(transition.SourceTrigramKey, transition.TargetTrigramKey);
            Assert.Equal(transition.SourceBusinessCode, transition.TargetBusinessCode);
            Assert.False(transition.RequiresSourceSelection);
            Assert.False(transition.OpensAgreementFlow);
        });
    }

    [Fact]
    public void MovingAnOuterTrigramToGen_OpensTheAgreementFlow()
    {
        var agreementTransitions = BaguaTransitionCatalog.All
            .Where(transition => transition.TargetTrigramKey == BaguaTrigramKeys.Gen
                                 && transition.SourceTrigramKey != BaguaTrigramKeys.Gen)
            .ToArray();

        Assert.Equal(4, agreementTransitions.Length);
        Assert.All(agreementTransitions, transition =>
        {
            Assert.Equal(BaguaTransitionWorkflowKinds.Governance, transition.WorkflowKind);
            Assert.True(transition.RequiresSourceSelection);
            Assert.True(transition.OpensAgreementFlow);
        });

        var groupPurchase = BaguaTransitionCatalog.Find(BaguaTrigramKeys.Zhen, BaguaTrigramKeys.Gen);
        Assert.Equal("order-to-agreement", groupPurchase.TransitionKey);
        Assert.Equal("공동구매 · 공동주문 합의", groupPurchase.PageTitle);
        Assert.Contains("픽업 장소", groupPurchase.Purpose);
    }

    [Fact]
    public void MovingGenToAnOuterTrigram_ExecutesAnAlreadyConfirmedDecision()
    {
        var executionTransitions = BaguaTransitionCatalog.All
            .Where(transition => transition.SourceTrigramKey == BaguaTrigramKeys.Gen
                                 && transition.TargetTrigramKey != BaguaTrigramKeys.Gen)
            .ToArray();

        Assert.Equal(4, executionTransitions.Length);
        Assert.All(executionTransitions, transition =>
        {
            Assert.Equal(BaguaTransitionWorkflowKinds.Execution, transition.WorkflowKind);
            Assert.True(transition.RequiresSourceSelection);
            Assert.False(transition.OpensAgreementFlow);
        });

        var confirmedOrder = BaguaTransitionCatalog.Find("AGREEMENT-TO-ORDER");
        Assert.Equal("확정안 주문 생성", confirmedOrder.PageTitle);
        Assert.Contains("전자서명", confirmedOrder.Purpose);
    }

    [Fact]
    public void RolePerspectives_CoverFiveCompleteFiveByFiveMatrices()
    {
        Assert.Equal(5, BaguaTransitionCatalog.Roles.Count);
        Assert.Equal(125, BaguaTransitionCatalog.RolePerspectives.Count);
        Assert.Equal(125, BaguaTransitionCatalog.RolePerspectives
            .Select(perspective => perspective.PerspectiveKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count());

        foreach (var role in BaguaTransitionCatalog.Roles)
        {
            var matrix = BaguaTransitionCatalog.GetRoleMatrix(role.RoleCode);

            Assert.Equal(25, matrix.Count);
            Assert.Equal(
                BaguaTransitionCatalog.All.Select(transition => transition.TransitionKey),
                matrix.Select(perspective => perspective.TransitionKey));
            Assert.All(matrix, perspective =>
            {
                Assert.Equal(role.RoleCode, perspective.RoleCode);
                Assert.Contains(role.RoleName, perspective.ViewTitle);
                Assert.False(string.IsNullOrWhiteSpace(perspective.Interpretation));
                Assert.False(string.IsNullOrWhiteSpace(perspective.PrimaryAction));
            });
        }
    }

    [Theory]
    [InlineData(BaguaActorRoleCodes.Orderer, BaguaBusinessCodes.Order)]
    [InlineData(BaguaActorRoleCodes.Seller, BaguaBusinessCodes.Sales)]
    [InlineData(BaguaActorRoleCodes.WarehouseManager, BaguaBusinessCodes.Warehouse)]
    [InlineData(BaguaActorRoleCodes.TransportOperator, BaguaBusinessCodes.Transport)]
    public void OperationalRoleMatrices_DistinguishOwnerInitiatorReceiverAndObserver(
        string roleCode,
        string businessCode)
    {
        var matrix = BaguaTransitionCatalog.GetRoleMatrix(roleCode);
        var ownArea = BaguaTransitionCatalog.Areas.Single(area => area.BusinessCode == businessCode);

        Assert.Single(matrix, perspective => perspective.PerspectiveMode == BaguaRolePerspectiveModes.Owner);
        Assert.Equal(4, matrix.Count(perspective => perspective.PerspectiveMode == BaguaRolePerspectiveModes.Initiator));
        Assert.Equal(4, matrix.Count(perspective => perspective.PerspectiveMode == BaguaRolePerspectiveModes.Receiver));
        Assert.Equal(16, matrix.Count(perspective => perspective.PerspectiveMode == BaguaRolePerspectiveModes.Observer));

        var home = BaguaTransitionCatalog.FindPerspective(roleCode, ownArea.TrigramKey, ownArea.TrigramKey);
        Assert.Equal(BaguaRolePerspectiveModes.Owner, home.PerspectiveMode);
    }

    [Fact]
    public void SameTransition_HasDifferentInterpretationsForOrdererSellerAndWarehouseManager()
    {
        var orderer = BaguaTransitionCatalog.FindPerspective(
            BaguaActorRoleCodes.Orderer,
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Li);
        var seller = BaguaTransitionCatalog.FindPerspective(
            BaguaActorRoleCodes.Seller,
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Li);
        var warehouseManager = BaguaTransitionCatalog.FindPerspective(
            BaguaActorRoleCodes.WarehouseManager,
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Li);

        Assert.Equal(BaguaRolePerspectiveModes.Initiator, orderer.PerspectiveMode);
        Assert.Contains("주문 맥락", orderer.Interpretation);
        Assert.Contains("필요 수량", orderer.PrimaryAction);

        Assert.Equal(BaguaRolePerspectiveModes.Receiver, seller.PerspectiveMode);
        Assert.Contains("판매 업무로 실행", seller.Interpretation);
        Assert.Contains("수요", seller.PrimaryAction);

        Assert.Equal(BaguaRolePerspectiveModes.Observer, warehouseManager.PerspectiveMode);
        Assert.Contains("기본 처리 주체가 아니며", warehouseManager.Interpretation);
        Assert.Contains("입출고", warehouseManager.PrimaryAction);
    }

    [Fact]
    public void CooperativeCoordinator_GovernsEveryAgreementBoundary()
    {
        var matrix = BaguaTransitionCatalog.GetRoleMatrix(BaguaActorRoleCodes.CooperativeCoordinator);

        Assert.Single(matrix, perspective => perspective.PerspectiveMode == BaguaRolePerspectiveModes.Owner);
        Assert.Equal(8, matrix.Count(perspective => perspective.PerspectiveMode == BaguaRolePerspectiveModes.Governor));
        Assert.Equal(16, matrix.Count(perspective => perspective.PerspectiveMode == BaguaRolePerspectiveModes.Observer));

        var groupPurchase = BaguaTransitionCatalog.FindPerspective(
            BaguaActorRoleCodes.CooperativeCoordinator,
            BaguaTrigramKeys.Zhen,
            BaguaTrigramKeys.Gen);
        Assert.Equal(BaguaRolePerspectiveModes.Governor, groupPurchase.PerspectiveMode);
        Assert.Contains("투표", groupPurchase.PrimaryAction);
        Assert.Contains("서명", groupPurchase.Interpretation);
    }
}
