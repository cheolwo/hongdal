using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers;

public sealed class 공동수입준비주문자조회ControllerTests
{
    [Fact]
    public void Controller는_인증과1_5기능플래그로보호된_Get조회만제공한다()
    {
        var type = typeof(공동수입준비주문자조회Controller);
        var version = type.GetCustomAttribute<SsalddelApiVersionAttribute>();
        var declaredActions = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(method => method.DeclaringType == type)
            .ToArray();

        Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(SsalddelProductVersion.V1_5, version?.Version);
        Assert.Equal(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, version?.FeatureKey);
        Assert.Equal(
            VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
            Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>()).Arguments?.Single());
        Assert.Equal(
            "api/v1/orderer/group-imports/{groupImportLedgerId}/readiness",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        var action = Assert.Single(declaredActions);
        Assert.NotNull(action.GetCustomAttribute<HttpGetAttribute>());
        Assert.Null(action.GetCustomAttribute<HttpPostAttribute>());
        Assert.Null(action.GetCustomAttribute<HttpPutAttribute>());
        Assert.Null(action.GetCustomAttribute<HttpDeleteAttribute>());
        Assert.Equal(
            typeof(공동수입준비주문자조회응답),
            action.GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Single(attribute => attribute.StatusCode == StatusCodes.Status200OK)
                .Type);
    }

    [Fact]
    public async Task 로그인주문자의식별자를_조회UseCase에전달한다()
    {
        const string ledgerId = "group-import-ledger-1";
        var response = new 공동수입준비주문자조회응답 { 상품명 = "쌀" };
        var useCase = new RecordingUseCase(response);
        var controller = new 공동수입준비주문자조회Controller(useCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "orderer-1")
                    ], "test"))
                }
            }
        };

        var action = await controller.조회(
            ledgerId,
            "auto-group-1",
            CancellationToken.None);

        Assert.Same(response, Assert.IsType<OkObjectResult>(action).Value);
        Assert.Equal(
            (ledgerId, "auto-group-1", "orderer-1"),
            useCase.LastRequest);
    }

    private sealed class RecordingUseCase(공동수입준비주문자조회응답? response)
        : I공동수입준비주문자조회UseCase
    {
        public (string LedgerId, string GroupId, string UserId) LastRequest { get; private set; }

        public Task<공동수입준비주문자조회응답?> 조회Async(
            string 공동수입원장Id,
            string 자동집단Id,
            string 주문자UserId,
            CancellationToken cancellationToken = default)
        {
            LastRequest = (공동수입원장Id, 자동집단Id, 주문자UserId);
            return Task.FromResult(response);
        }
    }
}
