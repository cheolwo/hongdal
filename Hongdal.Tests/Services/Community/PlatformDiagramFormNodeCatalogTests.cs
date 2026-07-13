using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Tests.Services.Community;

public sealed class PlatformDiagramFormNodeCatalogTests
{
    [Fact]
    public void All_ExposesDistinctBusinessFormNodeKinds()
    {
        var forms = PlatformDiagramFormNodeCatalog.All;

        Assert.Equal(6, forms.Count);
        Assert.All(forms, form =>
        {
            Assert.Equal("form", form.Kind);
            Assert.False(string.IsNullOrWhiteSpace(form.FormKind));
        });
        Assert.Equal(forms.Count, forms.Select(form => form.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(forms.Count, forms.Select(form => form.FormKind).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Contains(forms, form => form.FormKind == PlatformDiagramFormKinds.TransportRequest);
        Assert.Contains(forms, form => form.FormKind == PlatformDiagramFormKinds.WarehouseOutbound);
        Assert.Contains(forms, form => form.FormKind == PlatformDiagramFormKinds.WarehouseInbound);
        Assert.Contains(forms, form => form.FormKind == PlatformDiagramFormKinds.TransportPickupConfirmation);
        Assert.Contains(forms, form => form.FormKind == PlatformDiagramFormKinds.TransportDropoffConfirmation);
    }

    [Fact]
    public void ConnectionRules_RestrictEachBusinessFormToItsWorkflowTarget()
    {
        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.TransportRequest,
            "delivery",
            "운송 상차"));
        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.TransportRequest,
            "delivery",
            "기사 호출"));
        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.TransportRequest,
            "work",
            "배차 확정"));
        Assert.False(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.TransportRequest,
            "warehouse",
            "창고 입고"));

        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.WarehouseInbound,
            "warehouse",
            "도심 창고"));
        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.WarehouseOutbound,
            "warehouse",
            "출고 거점"));
        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.WarehouseOutbound,
            "work",
            "피킹 완료"));
        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.WarehouseInbound,
            "work",
            "입고 검수"));
        Assert.False(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.WarehouseOutbound,
            "delivery",
            "운송 상차"));

        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.TransportPickupConfirmation,
            "confirm",
            "상차 확인"));
        Assert.False(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.TransportPickupConfirmation,
            "confirm",
            "하차 확인"));
        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.TransportDropoffConfirmation,
            "confirm",
            "하차 확인"));

        Assert.True(PlatformDiagramFormNodeCatalog.CanConnect(
            PlatformDiagramFormKinds.Generic,
            "work",
            "사용자 정의 작업"));
    }

    [Fact]
    public void ConnectionRules_ProvideFormSpecificEdgeLabels()
    {
        Assert.Equal(6, PlatformDiagramFormNodeCatalog.ConnectionRules.Count);
        Assert.Equal(
            2,
            PlatformDiagramFormNodeCatalog
                .GetConnectionRule(PlatformDiagramFormKinds.TransportRequest)
                .AllowedTargets.Count);
        Assert.Equal(
            2,
            PlatformDiagramFormNodeCatalog
                .GetConnectionRule(PlatformDiagramFormKinds.WarehouseInbound)
                .AllowedTargets.Count);
        Assert.Equal(
            "운송 의뢰 제출",
            PlatformDiagramFormNodeCatalog
                .GetConnectionRule(PlatformDiagramFormKinds.TransportRequest)
                .ConnectionLabel);
        Assert.Equal(
            "입고 요청 제출",
            PlatformDiagramFormNodeCatalog
                .GetConnectionRule(PlatformDiagramFormKinds.WarehouseInbound)
                .ConnectionLabel);
        Assert.Equal(
            "출고 요청 제출",
            PlatformDiagramFormNodeCatalog
                .GetConnectionRule(PlatformDiagramFormKinds.WarehouseOutbound)
                .ConnectionLabel);
    }
}
