using System.Reflection;
using FluentResults;
using Ssalddel.Application.Shipper.Request;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class 지도신청운송취소검토AdminWorkflowTests
{
    [Fact]
    public async Task 승인은_원장확인_운송취소_원장결과순으로처리한다()
    {
        var calls = new List<string>();
        var ledger = Proxy<I지도신청가원장UseCase>((method, _) => method.Name switch
        {
            "관리자운송취소검토확인Async" => Record("confirm", Response()),
            "관리자운송취소검토결과반영Async" => Record("ledger-result", Response()),
            _ => throw new NotSupportedException(method.Name)
        });
        var transport = Proxy<I화주운송의뢰UseCase>((method, _) => method.Name switch
        {
            "관리자취소환불Async" => Record("transport", Result.Ok(new 화주운송의뢰응답 { 의뢰Id = "cargo-1" })),
            _ => throw new NotSupportedException(method.Name)
        });
        var workflow = new 지도신청운송취소검토AdminWorkflow(ledger, transport);

        await workflow.처리Async("ledger-1", Decision(approve: true), "admin-1");

        Assert.Equal(["confirm", "transport", "ledger-result"], calls);

        Task<T> Record<T>(string call, T value)
        {
            calls.Add(call);
            return Task.FromResult(value);
        }
    }

    [Fact]
    public async Task 원장확인실패시_운송취소를호출하지않는다()
    {
        var transportCalled = false;
        var ledger = Proxy<I지도신청가원장UseCase>((method, _) => method.Name switch
        {
            "관리자운송취소검토확인Async" => Task.FromException<지도신청가원장Response>(
                new InvalidOperationException("원본 불일치")),
            _ => throw new NotSupportedException(method.Name)
        });
        var transport = Proxy<I화주운송의뢰UseCase>((method, _) =>
        {
            transportCalled = true;
            throw new NotSupportedException(method.Name);
        });
        var workflow = new 지도신청운송취소검토AdminWorkflow(ledger, transport);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            workflow.처리Async("ledger-1", Decision(approve: true), "admin-1"));

        Assert.False(transportCalled);
    }

    [Fact]
    public async Task 거절은_운송취소없이_공동원장결과만기록한다()
    {
        var transportCalled = false;
        var resultRecorded = false;
        var ledger = Proxy<I지도신청가원장UseCase>((method, _) => method.Name switch
        {
            "관리자운송취소검토확인Async" => Task.FromResult(Response()),
            "관리자운송취소검토결과반영Async" => RecordResult(),
            _ => throw new NotSupportedException(method.Name)
        });
        var transport = Proxy<I화주운송의뢰UseCase>((method, _) =>
        {
            transportCalled = true;
            throw new NotSupportedException(method.Name);
        });
        var workflow = new 지도신청운송취소검토AdminWorkflow(ledger, transport);

        var result = await workflow.처리Async("ledger-1", Decision(approve: false), "admin-1");

        Assert.False(transportCalled);
        Assert.True(resultRecorded);
        Assert.Equal("ledger-1", result.원장Id);

        Task<지도신청가원장Response> RecordResult()
        {
            resultRecorded = true;
            return Task.FromResult(Response());
        }
    }

    [Fact]
    public async Task 운송취소실패시_공동원장승인결과를기록하지않는다()
    {
        var resultRecorded = false;
        var ledger = Proxy<I지도신청가원장UseCase>((method, _) => method.Name switch
        {
            "관리자운송취소검토확인Async" => Task.FromResult(Response()),
            "관리자운송취소검토결과반영Async" => RecordResult(),
            _ => throw new NotSupportedException(method.Name)
        });
        var transport = Proxy<I화주운송의뢰UseCase>((method, _) => method.Name switch
        {
            "관리자취소환불Async" => Task.FromResult(Result.Fail<화주운송의뢰응답>("Operational 차단")),
            _ => throw new NotSupportedException(method.Name)
        });
        var workflow = new 지도신청운송취소검토AdminWorkflow(ledger, transport);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            workflow.처리Async("ledger-1", Decision(approve: true), "admin-1"));

        Assert.Contains("Operational", exception.Message);
        Assert.False(resultRecorded);

        Task<지도신청가원장Response> RecordResult()
        {
            resultRecorded = true;
            return Task.FromResult(Response());
        }
    }

    private static 지도신청운송취소검토처리Request Decision(bool approve)
        => new()
        {
            승인 = approve,
            확인운영원본Id = "cargo-1",
            검토사유 = "관리자 확인"
        };

    private static 지도신청가원장Response Response()
        => new() { 원장Id = "ledger-1", 운영원본Id = "cargo-1" };

    private static T Proxy<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, RecordingProxy>();
        ((RecordingProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    private class RecordingProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            => Handler(targetMethod ?? throw new InvalidOperationException("호출 메서드가 없습니다."), args);
    }
}
