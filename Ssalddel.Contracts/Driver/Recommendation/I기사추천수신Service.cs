namespace Ssalddel.Contracts.Driver.Recommendation;

public interface I기사추천수신Service : IAsyncDisposable
{
    event Func<IReadOnlyList<기사추천수신항목>, Task>? 추천수신;
    event Func<string, Task>? 상태변경;

    string 연결상태 { get; }
    기사추천수신항목? 선택추천 { get; }
    string 선택추천출처 { get; }
    DateTimeOffset? 선택추천마감시각 { get; }
    int? 선택추천응답초 { get; }

    void 선택추천설정(
        기사추천수신항목 item,
        string source,
        DateTimeOffset? deadlineUtc = null,
        int? responseSeconds = null);

    기사추천수신항목? 선택추천조회(string? requestId);

    void 선택추천해제(string? requestId = null);

    Task 연결Async(CancellationToken cancellationToken = default);

    Task 운행중상태전송Async(CancellationToken cancellationToken = default);

    Task 위치전송Async(
        decimal 위도,
        decimal 경도,
        decimal? 상차접근허용반경Km = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<기사추천수신항목>> 추천조회Async(
        기사추천조회범위 범위,
        CancellationToken cancellationToken = default);

    Task<기사운송의뢰상세응답> 상세조회Async(
        string requestId,
        CancellationToken cancellationToken = default);

    Task<기사추천처리결과> 수락Async(
        string requestId,
        CancellationToken cancellationToken = default);

    Task<기사추천처리결과> 거절Async(
        string requestId,
        string? 사유,
        CancellationToken cancellationToken = default);

    IReadOnlyList<기사추천수신항목> 모의추천목록();

    ValueTask 연결해제Async();
}
