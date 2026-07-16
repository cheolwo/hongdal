using System.Reflection;
using System.Security.Claims;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers.Orderer;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Tests.Controllers.Orderer;

public sealed class 공동구매자동집단화ControllerTests
{
    [Fact]
    public void Controller는인증사용자만허용한다()
        => Assert.NotNull(typeof(공동구매자동집단화Controller)
            .GetCustomAttribute<AuthorizeAttribute>());

    [Fact]
    public async Task 수요등록은요청의주문자키를로그인사용자로교체한다()
    {
        var useCase = new RecordingUseCase();
        var controller = new 공동구매자동집단화Controller(useCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "authenticated-orderer"),
                        new Claim(ClaimTypes.Name, "인증 주문자")
                    ], "test"))
                }
            }
        };
        var command = new 공동구매자동수요등록Command
        {
            주문자키 = "spoofed-orderer",
            주문자표시명 = string.Empty
        };

        var result = await controller.수요등록(command, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Same(command, useCase.LastCommand);
        Assert.Equal("authenticated-orderer", useCase.LastCommand!.주문자키);
        Assert.Equal("인증 주문자", useCase.LastCommand.주문자표시명);
    }

    private sealed class RecordingUseCase : I공동구매자동집단화UseCase
    {
        public 공동구매자동수요등록Command? LastCommand { get; private set; }

        public Task<공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>> 목록조회Async(
            공동구매자동집단조회조건 조건,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                공동구매처리결과<IReadOnlyList<공동구매자동집단응답>>.성공결과([]));

        public Task<공동구매처리결과<공동구매자동집단응답>> 수요등록Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
        {
            LastCommand = command;
            return Task.FromResult(
                공동구매처리결과<공동구매자동집단응답>.성공결과(new 공동구매자동집단응답()));
        }
    }
}
