using System.Globalization;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>
/// 공개 상품 상세에서 판매 페이지 작성 화면으로 전달하는 검토용 초안 자료입니다.
/// 원장 ID, 참여자, 주문·결제 값은 포함하지 않으며 가격도 판매자가 다시 확정해야 하는 참고값입니다.
/// </summary>
public sealed record 판매페이지공개상품Seed(
    long SourceProductId,
    string ProductName,
    string? Category,
    string? Description,
    string? SellingUnit,
    decimal? ReferencePrice,
    bool CompletedLedgerVerified,
    int ReviewCount,
    DateTime? EvidenceAtUtc)
{
    public string Fingerprint
        => string.Join('|',
            SourceProductId,
            ProductName.Trim(),
            CompletedLedgerVerified,
            Math.Max(0, ReviewCount),
            EvidenceAtUtc?.ToString("O", CultureInfo.InvariantCulture));

    public string ToNavigationUri(string basePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);

        var query = new List<string>
        {
            Pair("sourceProductId", SourceProductId.ToString(CultureInfo.InvariantCulture)),
            Pair("productName", TrimTo(ProductName, 200)),
            Pair("completedLedgerVerified", CompletedLedgerVerified ? "true" : "false"),
            Pair("reviewCount", Math.Max(0, ReviewCount).ToString(CultureInfo.InvariantCulture))
        };
        Add(query, "category", Category, 100);
        Add(query, "description", Description, 240);
        Add(query, "sellingUnit", SellingUnit, 100);
        if (ReferencePrice is decimal price)
        {
            query.Add(Pair("referencePrice", price.ToString(CultureInfo.InvariantCulture)));
        }

        if (EvidenceAtUtc is DateTime evidenceAtUtc)
        {
            query.Add(Pair("evidenceAtUtc", evidenceAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)));
        }

        return $"{basePath.TrimEnd('/')}?{string.Join('&', query)}";
    }

    public string BuildSuggestedDescription()
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(Description))
        {
            lines.Add(Description.Trim());
        }

        var facts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Category))
        {
            facts.Add($"분류: {Category.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(SellingUnit))
        {
            facts.Add($"공개 판매 단위: {SellingUnit.Trim()}");
        }

        if (ReferencePrice is decimal price)
        {
            facts.Add($"공개 상품 참고 가격: {price:N0}원");
        }

        if (facts.Count > 0)
        {
            lines.Add(string.Join(" · ", facts));
        }

        lines.Add(CompletedLedgerVerified
            ? $"완료 구매 원장 확인 · 공개 후기 {Math.Max(0, ReviewCount):N0}건 (공개 투영 기준)"
            : "완료 구매 원장 미확인");
        lines.Add("이 내용은 판매 페이지 초안용 참고 자료입니다. 판매자는 원산지, 가격, 재고와 상품 설명을 직접 확인·수정해야 합니다.");
        return string.Join(Environment.NewLine + Environment.NewLine, lines);
    }

    private static void Add(List<string> query, string name, string? value, int maxLength)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add(Pair(name, TrimTo(value, maxLength)));
        }
    }

    private static string Pair(string name, string value)
        => $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";

    private static string TrimTo(string value, int maxLength)
    {
        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
