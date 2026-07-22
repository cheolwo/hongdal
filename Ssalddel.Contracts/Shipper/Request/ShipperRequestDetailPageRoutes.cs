using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Contracts.Shipper.Request;

public enum ShipperRequestDetailScreenKind
{
    Summary,
    Timeline,
    Payment,
    Proofs
}

/// <summary>
/// Web과 모바일이 같은 운송 의뢰 ID로 상세 책임별 Screen을 여는 route 계약입니다.
/// </summary>
public static class ShipperRequestDetailPageRoutes
{
    public const string LegacyLookup = "/shipper/request/detail";
    public const string SummaryTemplate = "/shipper/request/{RequestId}";
    public const string TimelineTemplate = "/shipper/request/{RequestId}/timeline";
    public const string PaymentTemplate = "/shipper/request/{RequestId}/payment";
    public const string ProofsTemplate = "/shipper/request/{RequestId}/proofs";

    public static string SummaryFor(string requestId)
        => $"{ShipperRequestPageRoutes.Root}/{RequestSegment(requestId)}";

    public static string TimelineFor(string requestId)
        => $"{SummaryFor(requestId)}/timeline";

    public static string PaymentFor(string requestId)
        => $"{SummaryFor(requestId)}/payment";

    public static string ProofsFor(string requestId)
        => $"{SummaryFor(requestId)}/proofs";

    public static string PathFor(ShipperRequestDetailScreenKind screen, string requestId)
        => screen switch
        {
            ShipperRequestDetailScreenKind.Summary => SummaryFor(requestId),
            ShipperRequestDetailScreenKind.Timeline => TimelineFor(requestId),
            ShipperRequestDetailScreenKind.Payment => PaymentFor(requestId),
            ShipperRequestDetailScreenKind.Proofs => ProofsFor(requestId),
            _ => throw new ArgumentOutOfRangeException(nameof(screen), screen, "지원하지 않는 운송 의뢰 상세 Screen입니다.")
        };

    private static string RequestSegment(string requestId)
    {
        var normalized = requestId?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("운송 의뢰 ID가 필요합니다.", nameof(requestId));
        }

        return Uri.EscapeDataString(normalized);
    }
}

/// <summary>
/// 상세 책임 사이를 이동할 때 안전한 이전 화면과 등록 완료 문맥을 보존합니다.
/// </summary>
public sealed record ShipperRequestDetailNavigationContext
{
    public string? ReturnPath { get; init; }
    public bool Created { get; init; }

    public string PathFor(ShipperRequestDetailScreenKind screen, string requestId)
    {
        var path = ShipperRequestDetailPageRoutes.PathFor(screen, requestId);
        if (Created && screen == ShipperRequestDetailScreenKind.Summary)
        {
            path = $"{path}?created=true";
        }

        return PageNavigationContext.WithReturnPath(path, ReturnPath);
    }

    public string ResolveReturnPath(string fallbackPath)
        => PageNavigationContext.ResolveReturnPath(ReturnPath, fallbackPath);
}
