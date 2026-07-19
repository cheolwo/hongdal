using System.Reflection;
using Ssalddel.Controllers.Common;
using Ssalddel.Controllers.Shipper.Payment02;
using Microsoft.AspNetCore.Authorization;

namespace Ssalddel.Tests.Controllers;

public sealed class OperationalEndpointAuthorizationTests
{
    private const string PaymentActorRoles = "화주,판매자,서버관리자";

    [Theory]
    [InlineData(nameof(화주결제Controller.공통결제준비))]
    [InlineData(nameof(화주결제Controller.공통결제승인))]
    [InlineData(nameof(화주결제Controller.페이크결제승인))]
    [InlineData(nameof(화주결제Controller.토스결제준비))]
    [InlineData(nameof(화주결제Controller.토스결제승인))]
    public void PaymentWriteEndpoints_RequireShipperSellerOrAdminRole(string methodName)
    {
        var authorize = GetAuthorizeAttribute(typeof(화주결제Controller), methodName);

        Assert.Equal(PaymentActorRoles, authorize.Roles);
    }

    [Fact]
    public void FileUploadEndpoint_RequiresLogisticsOperatorPolicy()
    {
        var authorize = GetAuthorizeAttribute(typeof(파일업로드Controller), nameof(파일업로드Controller.업로드));

        Assert.Equal("물류운영사용자전용", authorize.Policy);
    }

    [Fact]
    public void NodeStickerStoreFakePgConfirm_RequiresAuthenticatedUser()
    {
        var authorize = GetAuthorizeAttribute(
            typeof(노드스티커상점Controller),
            nameof(노드스티커상점Controller.ConfirmFakePg));

        Assert.Null(authorize.Policy);
        Assert.Null(authorize.Roles);
    }

    [Fact]
    public void NodeStickerStoreMyEntitlements_RequiresAuthenticatedUser()
    {
        var authorize = GetAuthorizeAttribute(
            typeof(노드스티커상점Controller),
            nameof(노드스티커상점Controller.GetMyEntitlements));

        Assert.Null(authorize.Policy);
        Assert.Null(authorize.Roles);
    }

    private static AuthorizeAttribute GetAuthorizeAttribute(Type controllerType, string methodName)
    {
        var method = controllerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public)
                     ?? throw new InvalidOperationException($"{controllerType.Name}.{methodName} method was not found.");
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorize);
        return authorize;
    }
}
