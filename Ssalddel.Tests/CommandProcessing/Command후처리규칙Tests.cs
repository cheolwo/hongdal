using FluentResults;
using Ssalddel.Application.Abstractions;
using Ssalddel.Application.CommandProcessing;
using MediatR;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.CommandProcessing;

public class Command후처리규칙Tests
{
    [Fact]
    public void IsCommandRequest_ICommand구현요청은_true를_반환한다()
    {
        var request = new 테스트단순Command();

        var result = Command후처리규칙.IsCommandRequest(request);

        Assert.True(result);
    }

    [Fact]
    public void IsCommandRequest_ICommandTResult구현요청은_true를_반환한다()
    {
        var request = new 테스트결과Command();

        var result = Command후처리규칙.IsCommandRequest(request);

        Assert.True(result);
    }

    [Fact]
    public void IsCommandRequest_Command가_아닌_요청은_false를_반환한다()
    {
        var request = new 테스트Query();

        var result = Command후처리규칙.IsCommandRequest(request);

        Assert.False(result);
    }

    [Fact]
    public void HasEnabled후처리Feature_스냅샷불가요청은_스냅샷만켜져있으면_false다()
    {
        var rule = new CommandProcessingRule
        {
            WorkRelationshipSnapshotEnabled = true
        };

        var result = Command후처리규칙.HasEnabled후처리Feature(rule, canHandleWorkRelationshipSnapshot: false);

        Assert.False(result);
    }

    [Fact]
    public void HasEnabled후처리Feature_알림기능이켜져있으면_true다()
    {
        var rule = new CommandProcessingRule
        {
            PushEnabled = true
        };

        var result = Command후처리규칙.HasEnabled후처리Feature(rule, canHandleWorkRelationshipSnapshot: false);

        Assert.True(result);
    }

    private sealed record 테스트단순Command : ICommand;

    private sealed record 테스트결과Command : ICommand<string>;

    private sealed record 테스트Query : IRequest<string>;
}
