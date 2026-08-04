using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Controllers.Common;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Controllers;

public sealed class 커뮤니티세계지도원장ProjectionControllerTests
{
    [Fact]
    public void Controller_is_anonymous_read_only_no_store_endpoint()
    {
        var type = typeof(커뮤니티세계지도원장ProjectionController);

        Assert.NotNull(type.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).SingleOrDefault());
        Assert.NotNull(type.GetCustomAttributes(typeof(SsalddelApiVersionAttribute), true).SingleOrDefault());
        var route = Assert.Single(type.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>());
        Assert.Equal(커뮤니티세계지도Routes.LedgerProjectionApi, route.Template);
        var cache = Assert.Single(type.GetCustomAttributes(typeof(ResponseCacheAttribute), true).Cast<ResponseCacheAttribute>());
        Assert.True(cache.NoStore);
        Assert.Equal(ResponseCacheLocation.None, cache.Location);
        Assert.NotNull(type.GetMethod(nameof(커뮤니티세계지도원장ProjectionController.조회))!
            .GetCustomAttributes(typeof(HttpGetAttribute), true)
            .SingleOrDefault());
    }

    [Fact]
    public async Task Anonymous_request_never_sets_viewer_or_privileged_flags()
    {
        var useCase = new RecordingUseCase([Projection("public-1")]);
        var authorization = new RecordingAuthorizationService(succeedAll: true);
        var controller = Controller(useCase, authorization, new ClaimsPrincipal(new ClaimsIdentity()));

        var result = await controller.조회(
            CommunityLedgerTemplateKeys.GroupPurchase,
            "marker-1",
            administrativeRegionKey: "region:seoul");

        var batch = Ok(result);
        Assert.Single(batch.Items);
        Assert.Null(useCase.LastQuery!.ViewerUserId);
        Assert.False(useCase.LastQuery.OperatorAuthorized);
        Assert.False(useCase.LastQuery.ReviewerAuthorized);
        Assert.Empty(authorization.Policies);
    }

    [Fact]
    public async Task Authenticated_request_resolves_user_and_privileges_from_server_authorization()
    {
        var useCase = new RecordingUseCase([]);
        var authorization = new RecordingAuthorizationService(succeedAll: true);
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-1")],
            authenticationType: "test");
        var controller = Controller(useCase, authorization, new ClaimsPrincipal(identity));

        await controller.조회(
            CommunityLedgerTemplateKeys.CargoTransport,
            "marker-1",
            administrativeRegionKey: "region:seoul");

        Assert.Equal("user-1", useCase.LastQuery!.ViewerUserId);
        Assert.True(useCase.LastQuery.OperatorAuthorized);
        Assert.True(useCase.LastQuery.ReviewerAuthorized);
        Assert.Equal(["물류운영사용자전용", "서버관리자전용"], authorization.Policies);
    }

    [Fact]
    public void Public_action_has_no_query_parameter_for_privilege_escalation()
    {
        var parameterNames = typeof(커뮤니티세계지도원장ProjectionController)
            .GetMethod(nameof(커뮤니티세계지도원장ProjectionController.조회))!
            .GetParameters()
            .Select(parameter => parameter.Name)
            .ToArray();

        Assert.DoesNotContain("operatorAuthorized", parameterNames);
        Assert.DoesNotContain("reviewerAuthorized", parameterNames);
        Assert.DoesNotContain("viewerUserId", parameterNames);
    }

    [Fact]
    public async Task Invalid_template_or_pagination_is_bad_request_without_use_case_call()
    {
        var useCase = new RecordingUseCase([]);
        var controller = Controller(
            useCase,
            new RecordingAuthorizationService(succeedAll: false),
            new ClaimsPrincipal(new ClaimsIdentity()));

        var unknown = await controller.조회("unknown", "marker-1");
        var invalidLimit = await controller.조회(
            CommunityLedgerTemplateKeys.GroupPurchase,
            "marker-1",
            limit: 51);

        Assert.IsType<BadRequestObjectResult>(unknown.Result);
        Assert.IsType<BadRequestObjectResult>(invalidLimit.Result);
        Assert.Equal(0, useCase.CallCount);
    }

    [Fact]
    public async Task Response_is_deterministically_paged_and_reports_more_items()
    {
        var useCase = new RecordingUseCase(
            Enumerable.Range(1, 5).Select(index => Projection($"projection-{index}")).ToArray());
        var controller = Controller(
            useCase,
            new RecordingAuthorizationService(succeedAll: false),
            new ClaimsPrincipal(new ClaimsIdentity()));

        var result = await controller.조회(
            CommunityLedgerTemplateKeys.GroupPurchase,
            "marker-1",
            administrativeRegionKey: "region:seoul",
            offset: 2,
            limit: 2);

        var batch = Ok(result);
        Assert.Equal(["projection-3", "projection-4"], batch.Items.Select(item => item.ProjectionId));
        Assert.Equal(2, batch.ReturnedCount);
        Assert.Equal(5, batch.AvailableCount);
        Assert.True(batch.HasMore);
        Assert.False(batch.SourceMayBeTruncated);
    }

    [Fact]
    public async Task Coarsened_source_keeps_has_more_true_even_for_one_aggregate()
    {
        var projection = Projection("aggregate-1");
        projection.AggregateBucketCode = 커뮤니티세계지도원장집계BucketCodes.Coarsened;
        var controller = Controller(
            new RecordingUseCase([projection]),
            new RecordingAuthorizationService(succeedAll: false),
            new ClaimsPrincipal(new ClaimsIdentity()));

        var batch = Ok(await controller.조회(
            CommunityLedgerTemplateKeys.GroupPurchase,
            "marker-1",
            administrativeRegionKey: "region:seoul"));

        Assert.True(batch.SourceMayBeTruncated);
        Assert.True(batch.HasMore);
    }

    private static 커뮤니티세계지도원장ProjectionController Controller(
        I커뮤니티세계지도원장ProjectionUseCase useCase,
        IAuthorizationService authorizationService,
        ClaimsPrincipal user)
        => new(useCase, authorizationService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };

    private static 커뮤니티세계지도원장ProjectionBatchDto Ok(
        ActionResult<커뮤니티세계지도원장ProjectionBatchDto> result)
        => Assert.IsType<OkObjectResult>(result.Result).Value is 커뮤니티세계지도원장ProjectionBatchDto batch
            ? batch
            : throw new Xunit.Sdk.XunitException("Expected projection batch response.");

    private static 커뮤니티세계지도원장ProjectionDto Projection(string id)
        => new()
        {
            ProjectionId = id,
            LedgerTemplateKey = CommunityLedgerTemplateKeys.GroupPurchase,
            ViewerScopeCode = 커뮤니티세계지도원장ViewerScopeCodes.Public
        };

    private sealed class RecordingUseCase(IReadOnlyList<커뮤니티세계지도원장ProjectionDto> result)
        : I커뮤니티세계지도원장ProjectionUseCase
    {
        public 커뮤니티세계지도원장ProjectionQuery? LastQuery { get; private set; }
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<커뮤니티세계지도원장ProjectionDto>> 조회Async(
            커뮤니티세계지도원장ProjectionQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            CallCount++;
            return Task.FromResult(result);
        }
    }

    private sealed class RecordingAuthorizationService(bool succeedAll) : IAuthorizationService
    {
        public List<string> Policies { get; } = [];

        public Task<AuthorizationResult> AuthorizeAsync(
            ClaimsPrincipal user,
            object? resource,
            IEnumerable<IAuthorizationRequirement> requirements)
            => Task.FromResult(succeedAll
                ? AuthorizationResult.Success()
                : AuthorizationResult.Failed());

        public Task<AuthorizationResult> AuthorizeAsync(
            ClaimsPrincipal user,
            object? resource,
            string policyName)
        {
            Policies.Add(policyName);
            return Task.FromResult(succeedAll
                ? AuthorizationResult.Success()
                : AuthorizationResult.Failed());
        }
    }
}
