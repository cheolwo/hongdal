using Ssalddel.Contracts.Common.Localization;

namespace Ssalddel.Contracts.Common.Orderer;

/// <summary>
/// 사용자에게 표시하는 주문 방식 용어입니다.
/// 기존 GroupOrder, 공동주문 식별자는 API와 저장 데이터 호환을 위해 유지합니다.
/// </summary>
public static class 같이주문용어Catalog
{
    public const string 한국어 = "같이 주문";
    public const string 영어 = "Order Together";
    public const string 일본어 = "一緒に注文";

    public static string 표시명(string? languageCode)
        => DisplayLanguageCodes.Select(languageCode, 한국어, 영어, 일본어);

    /// <summary>
    /// 과거 게시글과 저장 자료도 검색할 수 있도록 읽기 호환 용어를 유지합니다.
    /// 새 화면과 새 문구에는 <see cref="한국어"/>를 사용합니다.
    /// </summary>
    public static IReadOnlySet<string> 검색호환용어 { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "같이주문",
            한국어,
            영어,
            일본어,
            "공동주문",
            "공동 주문",
            "공동구매",
            "공동 구매",
            "group order",
            "group purchase"
        };
}
