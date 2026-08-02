using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Controllers.Common;
using Ssalddel.Domain.Community;
using Ssalddel.Infrastructure.Persistence.Configurations.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Architecture;

public sealed class CommunityActivityPaidDetailWorkflowMetadataTests
{
    [Fact]
    public void 유료상세Metadata는_계약부터Api와구매ProcessManager까지_세로흐름을표시한다()
    {
        var metadata = SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.CommunityActivityPaidDetail,
            typeof(커뮤니티활동유료상세등록Request).Assembly,
            typeof(커뮤니티활동유료상세Policy).Assembly,
            typeof(커뮤니티활동유료상세).Assembly,
            typeof(커뮤니티활동유료상세Configuration).Assembly,
            typeof(커뮤니티활동유료상세UseCase).Assembly);

        Assert.Contains(metadata, item => item.ComponentType == typeof(커뮤니티활동유료상세등록Request));
        Assert.Contains(metadata, item => item.ComponentType == typeof(커뮤니티활동유료상세Policy));
        Assert.Contains(metadata, item => item.ComponentType == typeof(커뮤니티활동유료상세));
        Assert.Contains(metadata, item => item.ComponentType == typeof(커뮤니티활동유료상세Configuration));
        Assert.Contains(metadata, item => item.ComponentType == typeof(커뮤니티활동유료상세UseCase));
        Assert.Contains(metadata, item => item.ComponentType == typeof(커뮤니티활동상세구매ProcessManager));
        Assert.Contains(metadata, item => item.ComponentType == typeof(커뮤니티활동유료상세Controller));
        Assert.All(metadata, item => Assert.False(string.IsNullOrWhiteSpace(item.Boundary)));
        Assert.Equal(
            metadata.OrderBy(item => item.FlowOrder).ThenBy(
                item => item.ComponentType.FullName,
                StringComparer.Ordinal),
            metadata);
    }

    [Fact]
    public void 구매와본문조회Api는_인증경계와안정Route를유지한다()
    {
        var controller = typeof(커뮤니티활동유료상세Controller);
        Assert.Equal(
            "api/v1/community/activity-paid-details",
            controller.GetCustomAttribute<RouteAttribute>()?.Template);

        Assert.NotNull(controller.GetMethod("모의결제확인")?.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(controller.GetMethod("상세내용조회")?.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(controller.GetMethod("구매조회")?.GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(controller.GetMethod("내구매목록조회")?.GetCustomAttribute<AuthorizeAttribute>());
    }
}
