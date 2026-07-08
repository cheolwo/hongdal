using 홍달.도메인.배차;
using 홍달.Services.Dispatch.Queue;

namespace 홍달.Services.Dispatch.Engine;

public interface I운송의뢰배차엔진
{
    string 엔진코드 { get; }

    string 표시명 { get; }

    int 배차업무유형 { get; }

    Task<배차추천후보?> 다음후보선정Async(
        배차대기 queue,
        string? 제외기사Id = null,
        CancellationToken cancellationToken = default);
}
