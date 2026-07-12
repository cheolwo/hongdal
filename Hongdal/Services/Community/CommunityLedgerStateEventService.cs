using System.Text.Json;
using Hongdal.Domain.Community;
using 홍달.Data;

namespace Hongdal.Services.Community;

public interface I커뮤니티원장상태이벤트Service
{
    Task 저장이벤트기록Async(
        커뮤니티원장Dto 원장,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task 상태변경이벤트기록Async(
        커뮤니티원장상태변경요청 request,
        커뮤니티원장Dto 원장,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

public sealed class 커뮤니티원장상태이벤트Service : I커뮤니티원장상태이벤트Service
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HongdalContext _db;

    public 커뮤니티원장상태이벤트Service(HongdalContext db)
    {
        _db = db;
    }

    public Task 저장이벤트기록Async(
        커뮤니티원장Dto 원장,
        string updatedBy,
        CancellationToken cancellationToken = default)
        => 기록Async(
            원장,
            커뮤니티원장상태이벤트유형.저장,
            이전상태: 원장.상태이력.LastOrDefault()?.이전상태,
            상태: 원장.상태,
            현재단계Key: 원장.현재단계Key,
            변경사유: "원장 저장",
            updatedBy,
            cancellationToken);

    public Task 상태변경이벤트기록Async(
        커뮤니티원장상태변경요청 request,
        커뮤니티원장Dto 원장,
        string updatedBy,
        CancellationToken cancellationToken = default)
        => 기록Async(
            원장,
            커뮤니티원장상태이벤트유형.상태변경,
            request.이전상태,
            request.상태,
            request.현재단계Key ?? 원장.현재단계Key,
            request.메모,
            updatedBy,
            cancellationToken);

    private async Task 기록Async(
        커뮤니티원장Dto 원장,
        string eventType,
        string? 이전상태,
        string 상태,
        string? 현재단계Key,
        string? 변경사유,
        string updatedBy,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(원장.원장Id))
        {
            throw new InvalidOperationException("커뮤니티 원장 상태 이벤트에는 원장Id가 필요합니다.");
        }

        var now = DateTime.UtcNow;
        var snapshot = new
        {
            원장.원장Id,
            원장.커뮤니티Id,
            원장.원장템플릿Key,
            원장.제목,
            원장.원함,
            상태,
            현재단계Key,
            대상OsCode = 원장.대상OsCode,
            대상OsName = 원장.대상OsName,
            블록수 = 원장.블록목록.Count,
            참여자수 = 원장.참여자목록.Count,
            원장.외부참조,
            원장.확장속성,
            원장.수정시각Utc
        };

        _db.커뮤니티원장상태이벤트.Add(new 커뮤니티원장상태이벤트
        {
            EventId = Guid.NewGuid().ToString("N"),
            커뮤니티원장Id = Clean(원장.원장Id),
            커뮤니티Id = Clean(원장.커뮤니티Id),
            원장템플릿Key = Clean(원장.원장템플릿Key),
            EventType = eventType,
            이전상태 = CleanNullable(이전상태),
            상태 = Clean(상태),
            현재단계Key = CleanNullable(현재단계Key),
            변경사유 = CleanNullable(변경사유),
            UpdatedBy = Clean(updatedBy),
            CorrelationId = CleanNullable(원장.외부참조.TryGetValue("CorrelationId", out var correlationId) ? correlationId : null),
            SnapshotJson = JsonSerializer.Serialize(snapshot, JsonOptions),
            OccurredAtUtc = now,
            CreatedAtUtc = now
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string? CleanNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
