using Microsoft.AspNetCore.Mvc;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Server.Controllers;

namespace Ssalddel.Simulation.Tests;

public sealed class 경영SimulationSessionTests
{
    [Fact]
    public void Session은_ScenarioSeed와Lineage를보존하고_운영상태로표시하지않는다()
    {
        var result = Service().Create(CreateRequest());

        Assert.Equal("scenario:urban-market-potato-4w", result.ScenarioStableId);
        Assert.Equal("simulation-data:potato-4w:1", result.ScenarioDataRevision);
        Assert.Equal(240809, result.ScenarioSeed);
        Assert.Equal("supply-management-rule:1", result.RuleRevision);
        Assert.Equal(SimulationModeCodes.Simulation, result.ModeCode);
        Assert.False(result.IsOperationalState);
        Assert.Equal(0, result.CurrentTick);
        Assert.Equal(0, result.Revision);
    }

    [Fact]
    public void 같은ClientRequest는_같은Session을멱등반환한다()
    {
        var service = Service();
        var request = CreateRequest();

        var first = service.Create(request);
        var second = service.Create(request);

        Assert.Equal(first.SessionStableId, second.SessionStableId);
        Assert.Equal(first.Revision, second.Revision);
    }

    [Fact]
    public void 같은ClientRequest의다른Payload는_충돌로거부한다()
    {
        var service = Service();
        var request = CreateRequest();
        service.Create(request);
        request.RuleRevision = "supply-management-rule:2";

        var error = Assert.Throws<SimulationConflictException>(() => service.Create(request));

        Assert.Equal("SimulationCreateRequestPayloadConflict", error.ErrorCode);
    }

    [Fact]
    public void Tick은_expectedRevision을검증하고_revision을증가시킨다()
    {
        var service = Service();
        var session = service.Create(CreateRequest());

        var advanced = service.Advance(session.SessionStableId, new 경영SimulationTick진행Request
        {
            CommandId = "command:advance-week-1",
            ExpectedRevision = 0,
            TickCount = 7,
        });

        Assert.Equal(7, advanced.CurrentTick);
        Assert.Equal(1, advanced.Revision);
        Assert.False(advanced.IsCompleted);
    }

    [Fact]
    public void 같은TickCommand재시도는_staleExpectedRevision이어도_같은결과를반환한다()
    {
        var service = Service();
        var session = service.Create(CreateRequest());
        var command = new 경영SimulationTick진행Request
        {
            CommandId = "command:advance-day-1",
            ExpectedRevision = 0,
            TickCount = 1,
        };

        var first = service.Advance(session.SessionStableId, command);
        var retry = service.Advance(session.SessionStableId, command);

        Assert.Equal(first.CurrentTick, retry.CurrentTick);
        Assert.Equal(first.Revision, retry.Revision);
    }

    [Fact]
    public void 다른Command의_staleExpectedRevision은거부한다()
    {
        var service = Service();
        var session = service.Create(CreateRequest());
        service.Advance(session.SessionStableId, new 경영SimulationTick진행Request
        {
            CommandId = "command:advance-day-1",
            ExpectedRevision = 0,
            TickCount = 1,
        });

        var error = Assert.Throws<SimulationConflictException>(() =>
            service.Advance(session.SessionStableId, new 경영SimulationTick진행Request
            {
                CommandId = "command:advance-day-2",
                ExpectedRevision = 0,
                TickCount = 1,
            }));

        Assert.Equal("SimulationExpectedRevisionMismatch", error.ErrorCode);
    }

    [Fact]
    public void Scenario기간을넘는Tick은거부한다()
    {
        var service = Service();
        var request = CreateRequest();
        request.DurationTicks = 7;
        var session = service.Create(request);

        var error = Assert.Throws<SimulationConflictException>(() =>
            service.Advance(session.SessionStableId, new 경영SimulationTick진행Request
            {
                CommandId = "command:advance-beyond-scenario",
                ExpectedRevision = 0,
                TickCount = 8,
            }));

        Assert.Equal("SimulationDurationExceeded", error.ErrorCode);
    }

    [Fact]
    public void Tick재시도Snapshot의외부변경은_저장된멱등결과를오염시키지않는다()
    {
        var service = Service();
        var session = service.Create(CreateRequest());
        var command = new 경영SimulationTick진행Request
        {
            CommandId = "command:advance-day-immutable-retry",
            ExpectedRevision = 0,
            TickCount = 1,
        };
        var first = service.Advance(session.SessionStableId, command);
        first.CurrentTick = 99;

        var retry = service.Advance(session.SessionStableId, command);

        Assert.Equal(1, retry.CurrentTick);
        Assert.Equal(1, retry.Revision);
    }

    [Fact]
    public void Controller는_계약오류를_safeErrorCode의BadRequest로변환한다()
    {
        var controller = new 경영SimulationSessionsController(Service());

        var action = controller.Create(new 경영SimulationSession생성Request());

        var badRequest = Assert.IsType<BadRequestObjectResult>(action.Result);
        var error = Assert.IsType<SimulationErrorResponse>(badRequest.Value);
        Assert.Equal("SimulationClientRequestIdMissing", error.ErrorCode);
    }

    [Fact]
    public void SimulationDomain은_운영서버와UnityAssembly를참조하지않는다()
    {
        var references = typeof(경영SimulationSessionAggregate).Assembly
            .GetReferencedAssemblies()
            .Select(value => value.Name)
            .ToArray();

        Assert.DoesNotContain("Ssalddel", references);
        Assert.DoesNotContain("Ssalddel.Contracts", references);
        Assert.DoesNotContain("Ssalddel.Domain", references);
        Assert.DoesNotContain("Ssalddel.Infrastructure", references);
        Assert.DoesNotContain("Ssalddel.Unity", references);
    }

    [Fact]
    public void SimulationServer는_운영서버와UnityAssembly를참조하지않는다()
    {
        var references = typeof(경영SimulationSessionsController).Assembly
            .GetReferencedAssemblies()
            .Select(value => value.Name)
            .ToArray();

        Assert.DoesNotContain("Ssalddel", references);
        Assert.DoesNotContain("Ssalddel.Contracts", references);
        Assert.DoesNotContain("Ssalddel.Domain", references);
        Assert.DoesNotContain("Ssalddel.Infrastructure", references);
        Assert.DoesNotContain("Ssalddel.Unity", references);
    }

    private static 경영SimulationSessionService Service()
        => new(new InMemory경영SimulationSessionStore());

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.Parse("2d46514f-2982-4fc2-a3f6-fca9dd31a6aa"),
            ScenarioStableId = "scenario:urban-market-potato-4w",
            ScenarioDataRevision = "simulation-data:potato-4w:1",
            ScenarioSeed = 240809,
            RuleRevision = "supply-management-rule:1",
            DurationTicks = 28,
        };
}
