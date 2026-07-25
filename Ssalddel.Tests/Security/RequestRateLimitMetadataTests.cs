using System.Reflection;
using Microsoft.AspNetCore.RateLimiting;
using Ssalddel.Controllers.Common;
using Ssalddel.Security;

namespace Ssalddel.Tests.Security;

public sealed class RequestRateLimitMetadataTests
{
    [Theory]
    [InlineData(typeof(인증Controller), "로그인")]
    [InlineData(typeof(인증Controller), "기사회원가입")]
    [InlineData(typeof(인증Controller), "커뮤니티회원가입")]
    [InlineData(typeof(인증Controller), "주문자회원가입")]
    [InlineData(typeof(인증Controller), "토큰갱신")]
    public void AuthenticationEntryPoints_UseAuthenticationRateLimit(
        Type controllerType,
        string actionName)
    {
        AssertPolicy(
            controllerType,
            actionName,
            RequestRateLimitPolicyNames.Authentication);
    }

    [Theory]
    [InlineData(typeof(커뮤니티게시글Controller), "생성")]
    [InlineData(typeof(커뮤니티게시글Controller), "수정")]
    [InlineData(typeof(커뮤니티게시글Controller), "삭제")]
    [InlineData(typeof(커뮤니티게시글첨부Controller), "첨부파일업로드")]
    [InlineData(typeof(커뮤니티게시글참여Controller), "댓글삭제")]
    [InlineData(typeof(커뮤니티게시글참여Controller), "첨부댓글삭제")]
    public void AnonymousCredentialMutations_UseCommunityMutationRateLimit(
        Type controllerType,
        string actionName)
    {
        AssertPolicy(
            controllerType,
            actionName,
            RequestRateLimitPolicyNames.CommunityMutation);
    }

    private static void AssertPolicy(
        Type controllerType,
        string actionName,
        string expectedPolicy)
    {
        var action = controllerType.GetMethod(
            actionName,
            BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(action);

        var attribute = action!
            .GetCustomAttribute<EnableRateLimitingAttribute>(inherit: true);
        Assert.NotNull(attribute);
        Assert.Equal(expectedPolicy, attribute!.PolicyName);
    }
}
