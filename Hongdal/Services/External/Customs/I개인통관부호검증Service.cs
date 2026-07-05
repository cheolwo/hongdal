namespace 홍달.Services.External.Customs;

public sealed class 개인통관부호검증Request
{
    public string 개인통관고유부호 { get; init; } = string.Empty;
    public string 이름 { get; init; } = string.Empty;
    public string 휴대폰번호 { get; init; } = string.Empty;
    public string? 우편번호 { get; init; }
}

public sealed class 개인통관부호검증Result
{
    public bool 성공여부 { get; init; }
    public string 결과코드 { get; init; } = string.Empty;
    public string 메시지 { get; init; } = string.Empty;
}

public interface I개인통관부호검증Service
{
    Task<개인통관부호검증Result> 검증Async(
        개인통관부호검증Request request,
        CancellationToken cancellationToken = default);
}
