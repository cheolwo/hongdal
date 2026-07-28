using System.Globalization;

namespace Ssalddel.Ui.Common.Areas.App.Models;

public static class 같이주문수량Presentation
{
    public const string 미확정단위 = "단위 미확정";

    public static string 수량(decimal quantity, string? sourceUnit)
        => $"{quantity.ToString("#,0.##", CultureInfo.CurrentCulture)} {단위(sourceUnit)}";

    public static string 선택수량(decimal? quantity, string? sourceUnit)
        => quantity.HasValue
            ? 수량(quantity.Value, sourceUnit)
            : "미정";

    public static string 단위(string? sourceUnit)
        => string.IsNullOrWhiteSpace(sourceUnit)
            ? 미확정단위
            : sourceUnit.Trim();

    public static string 원거래단위(string? sourceUnit)
        => string.IsNullOrWhiteSpace(sourceUnit)
            ? "공급자 원 거래단위 확인 전 미정"
            : $"{sourceUnit.Trim()} · 원 거래단위 유지";
}
