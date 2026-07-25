using System.Reflection;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Controllers.Common;
using Ssalddel.Controllers.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Tests.Controllers;

public sealed class GroupPurchaseDemandVoteControllerTests
{
    [Theory]
    [InlineData(nameof(커뮤니티투표Controller.목록조회))]
    [InlineData(nameof(커뮤니티투표Controller.상세조회))]
    public void GenericCommunityVoteReadEndpoints_AllowAnonymous(string methodName)
    {
        var method = typeof(커뮤니티투표Controller).GetMethod(methodName);

        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void OrdererGroupPurchaseDemandVoteController_RequiresAuthentication()
    {
        var authorize = typeof(공동구매수요투표Controller).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
    }

    [Fact]
    public async Task GenericCommunityVoteEndpoint_RejectsGroupPurchaseDemandCreation()
    {
        var controller = new 커뮤니티투표Controller(null!);

        var result = await controller.생성(new CommunityVoteCreateRequest
        {
            VoteKind = CommunityVoteKindCodes.GroupPurchaseDemand
        }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("orderer/group-purchase-demand-votes", badRequest.Value?.ToString());
    }
}
