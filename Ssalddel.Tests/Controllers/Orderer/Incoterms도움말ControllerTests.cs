using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Localization;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Controllers.Orderer;

public sealed class Incoterms도움말ControllerTests
{
    [Fact]
    public void Controller는_인증과1_5기능플래그로보호된_Get조회만제공한다()
    {
        var type = typeof(Incoterms도움말Controller);
        var version = type.GetCustomAttribute<SsalddelApiVersionAttribute>();
        var action = Assert.Single(
            type.GetMethods(BindingFlags.Instance | BindingFlags.Public),
            method => method.DeclaringType == type);

        Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal(SsalddelProductVersion.V1_5, version?.Version);
        Assert.Equal(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, version?.FeatureKey);
        Assert.Equal(
            VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
            Assert.Single(type.GetCustomAttributes<RequireVersionFeatureAttribute>()).Arguments?.Single());
        Assert.Equal(
            "api/v1/orderer/trade/incoterms/help",
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(action.GetCustomAttribute<HttpGetAttribute>());
        Assert.Equal(
            typeof(Incoterms도움말응답),
            action.GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Single(attribute => attribute.StatusCode == StatusCodes.Status200OK)
                .Type);
    }

    [Fact]
    public void 선택코드와언어를_조회UseCase에전달한다()
    {
        var response = new Incoterms도움말응답 { 선택코드 = "CIF" };
        var useCase = new RecordingUseCase(response);
        var controller = new Incoterms도움말Controller(useCase);

        var result = controller.조회("CIF", DisplayLanguageCodes.Korean);

        Assert.Same(response, Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal(("CIF", DisplayLanguageCodes.Korean), useCase.LastRequest);
    }

    [Fact]
    public void 지원하지않는코드는_400Problem으로응답한다()
    {
        var controller = new Incoterms도움말Controller(new ThrowingUseCase());

        var result = controller.조회("EXW", DisplayLanguageCodes.Korean);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    private sealed class RecordingUseCase(Incoterms도움말응답 response)
        : IIncoterms도움말조회UseCase
    {
        public (string? Code, string? Language) LastRequest { get; private set; }

        public Incoterms도움말응답 조회(string? 선택코드, string? 언어코드)
        {
            LastRequest = (선택코드, 언어코드);
            return response;
        }
    }

    private sealed class ThrowingUseCase : IIncoterms도움말조회UseCase
    {
        public Incoterms도움말응답 조회(string? 선택코드, string? 언어코드)
            => throw new ArgumentException("unsupported");
    }
}
