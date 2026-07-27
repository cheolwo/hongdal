namespace 살뜰.Services.Sales;

public sealed record 판매채널자격증명Set(
    long 판매채널계정Id,
    string 채널종류,
    IReadOnlyDictionary<string, string> Values);

/// <summary>
/// 서버 내부의 채널 adapter만 사용하는 복호화 경계입니다.
/// Controller와 client contract에는 등록하지 않습니다.
/// </summary>
public interface ISalesChannelCredentialProvider
{
    Task<판매채널자격증명Set?> GetAsync(
        long 판매채널계정Id,
        CancellationToken cancellationToken);
}
