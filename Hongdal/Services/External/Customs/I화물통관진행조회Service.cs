using 홍달.도메인.통관;

namespace 홍달.Services.External.Customs;

public sealed class 화물통관진행조회Request
{
    public string? 화물관리번호 { get; init; }
    public string? MasterBl { get; init; }
    public string? HouseBl { get; init; }
}

public sealed class 화물통관진행조회Result
{
    public bool 조회성공여부 { get; init; }
    public 통관진행단계 진행단계 { get; init; }
    public string? 장치장명 { get; init; }
    public string? 처리단계명 { get; init; }
    public string? 오류메시지 { get; init; }
    public DateTimeOffset 조회시각 { get; init; } = DateTimeOffset.UtcNow;
}

public interface I화물통관진행조회Service
{
    Task<화물통관진행조회Result> 조회Async(
        화물통관진행조회Request request,
        CancellationToken cancellationToken = default);
}
