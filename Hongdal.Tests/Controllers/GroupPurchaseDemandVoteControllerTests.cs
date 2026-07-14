using System.Reflection;
using Hongdal.Contracts.Common.Community;
using Hongdal.Controllers.Common;
using Hongdal.Controllers.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Tests.Controllers;

public sealed class GroupPurchaseDemandVoteControllerTests
{
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

        var result = await controller.Create(new CommunityVoteCreateRequest
        {
            VoteKind = CommunityVoteKindCodes.GroupPurchaseDemand
        }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("orderer/group-purchase-demand-votes", badRequest.Value?.ToString());
    }
}
