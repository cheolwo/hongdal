using Microsoft.AspNetCore.Http;
using Ssalddel.Middleware;

namespace Ssalddel.Tests.Middleware;

public sealed class 사용자행위로그MiddlewareTests
{
    [Theory]
    [InlineData("/api/v1/platform/runtime/google-maps")]
    [InlineData("/api/v1/community/world-map/observations")]
    public void 개발ReadOnlySimulation의_지도Get은_공통감사DB기록을건너뛴다(string path)
    {
        var skip = 사용자행위로그Middleware.ShouldSkipDevelopmentMapReadAudit(
            isDevelopment: true,
            isSimulation: true,
            developmentReadOnly: true,
            HttpMethods.Get,
            new PathString(path));

        Assert.True(skip);
    }

    [Theory]
    [InlineData(false, true, true, "GET", "/api/v1/platform/runtime/google-maps")]
    [InlineData(true, false, true, "GET", "/api/v1/platform/runtime/google-maps")]
    [InlineData(true, true, false, "GET", "/api/v1/platform/runtime/google-maps")]
    [InlineData(true, true, true, "POST", "/api/v1/platform/runtime/google-maps")]
    [InlineData(true, true, true, "GET", "/api/v1/community/posts")]
    public void 운영경계나_기타요청은_공통감사를유지한다(
        bool isDevelopment,
        bool isSimulation,
        bool developmentReadOnly,
        string method,
        string path)
    {
        var skip = 사용자행위로그Middleware.ShouldSkipDevelopmentMapReadAudit(
            isDevelopment,
            isSimulation,
            developmentReadOnly,
            method,
            new PathString(path));

        Assert.False(skip);
    }
}
