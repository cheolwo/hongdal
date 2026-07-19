using FluentResults;
using Ssalddel.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Tests.Controllers;

public sealed class Result응답확장Tests
{
    [Theory]
    [InlineData("기사 인증 정보가 없습니다.", StatusCodes.Status401Unauthorized, "AuthenticationRequired")]
    [InlineData("통관 조회 동의 등록 권한이 없습니다.", StatusCodes.Status403Forbidden, "Forbidden")]
    [InlineData("배차대기 데이터를 찾을 수 없습니다.", StatusCodes.Status404NotFound, "NotFound")]
    [InlineData("수락 가능한 배차가 아닙니다.", StatusCodes.Status409Conflict, "InvalidState")]
    [InlineData("결제완료 의뢰만 수락할 수 있습니다.", StatusCodes.Status409Conflict, "InvalidState")]
    [InlineData("거절 처리 중 오류가 발생했습니다. 잠시 후 다시 시도해 주세요.", StatusCodes.Status503ServiceUnavailable, "TemporaryFailure")]
    public void ToActionResult_MapsKnownFailuresToProblemDetails(string message, int expectedStatus, string expectedCode)
    {
        var controller = new TestController();

        var actionResult = controller.ToActionResult(Result.Fail<object>(message));

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(expectedStatus, objectResult.StatusCode);
        Assert.Equal(expectedStatus, problem.Status);
        Assert.Equal(message, problem.Title);
        Assert.Equal(expectedCode, problem.Extensions["errorCode"]);
        Assert.Equal("test-trace", problem.Extensions["traceId"]);
    }

    private sealed class TestController : ControllerBase
    {
        public TestController()
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    TraceIdentifier = "test-trace"
                }
            };
            ControllerContext.HttpContext.Request.Path = "/test";
        }
    }
}
