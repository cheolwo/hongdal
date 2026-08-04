using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.Contracts.Common.Community;

public sealed class 지도신청가원장생성Request
{
    public Guid 신청개인정보동의증적Id { get; set; }
    public string 업무Code { get; set; } = string.Empty;
    public string 신청출처Code { get; set; } = 신청개인정보출처Codes.커뮤니티지도;
    public string MarkerId { get; set; } = string.Empty;
    public string MarkerName { get; set; } = string.Empty;
    public string LayerCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

public sealed class 지도신청가원장Response
{
    public string 원장Id { get; set; } = string.Empty;
    public Guid 신청개인정보동의증적Id { get; set; }
    public long Revision { get; set; }
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 원장템플릿명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 현재단계Key { get; set; } = string.Empty;
    public bool 기존가원장재사용 { get; set; }
    public bool 외부실행발생 { get; set; }
    public bool 실원장전환됨 { get; set; }
    public bool 동의철회보류 { get; set; }
    public bool 운영신청자동취소됨 { get; set; }
    public bool 운영신청취소됨 { get; set; }
    public string 운송취소검토상태Code { get; set; } = string.Empty;
    public string 운송취소검토사유 { get; set; } = string.Empty;
    public string 운송취소검토결과사유 { get; set; } = string.Empty;
    public string 운영원본종류 { get; set; } = string.Empty;
    public string 운영원본Id { get; set; } = string.Empty;
}

public sealed class 지도신청실원장전환Request
{
    public Guid 신청개인정보동의증적Id { get; set; }
    public string 업무Code { get; set; } = string.Empty;
    public string 신청출처Code { get; set; } = 신청개인정보출처Codes.커뮤니티지도;
    public string 운영원본종류 { get; set; } = string.Empty;
    public string 운영원본Id { get; set; } = string.Empty;
}

public sealed class 지도신청동의철회반영Request
{
    public Guid 신청개인정보동의증적Id { get; set; }
}

public sealed class 지도신청운영취소반영Request
{
    public string 운영원본종류 { get; set; } = string.Empty;
    public string 운영원본Id { get; set; } = string.Empty;
}

public sealed class 지도신청운송취소검토요청Request
{
    public string 운영원본Id { get; set; } = string.Empty;
    public string 사유 { get; set; } = string.Empty;
}

public sealed class 지도신청운송취소검토처리Request
{
    public bool 승인 { get; set; }
    public string 확인운영원본Id { get; set; } = string.Empty;
    public string 검토사유 { get; set; } = string.Empty;
}

public static class 지도신청가원장정책
{
    public const string 신청접수BlockId = "map-application-intake";
    public const string 신청접수단계 = "application-intake";
    public const string 신청제출단계 = "application-submitted";
    public const string 동의철회확인단계 = "privacy-consent-withdrawn";
    public const string 운영신청취소단계 = "application-cancelled";
    public const string 운송취소검토단계 = "transport-cancellation-review";
    public const string 실원장성숙도Code = "Established";
    public const string 신청제출효과Code = "ApplicationSubmitted";
    public const string 신청취소효과Code = "ApplicationCancelled";
    public const string 운영원본종류Key = "OperationalSourceType";
    public const string 운영원본IdKey = "OperationalSourceId";
    public const string 개인정보동의철회Key = "PrivacyConsentWithdrawn";
    public const string 운송취소검토상태Key = "TransportCancellationReviewState";
    public const string 운송취소검토사유Key = "TransportCancellationReviewReason";
    public const string 운송취소검토요청됨Code = "Requested";
    public const string 운송취소검토승인Code = "Approved";
    public const string 운송취소검토거절Code = "Rejected";
    public const string 운송취소검토결과사유Key = "TransportCancellationReviewDecisionReason";

    public static string 원장템플릿Key(string? 업무Code)
        => 업무Code?.Trim() switch
        {
            신청개인정보업무Codes.물류대행 => CommunityLedgerTemplateKeys.WarehouseInbound,
            신청개인정보업무Codes.운송대행 => CommunityLedgerTemplateKeys.CargoTransport,
            신청개인정보업무Codes.개별주문 => CommunityLedgerTemplateKeys.Order,
            _ => throw new ArgumentOutOfRangeException(nameof(업무Code), 업무Code, "지원하지 않는 지도 신청 업무입니다.")
        };

    public static string 신청자역할(string 업무Code)
        => 업무Code switch
        {
            신청개인정보업무Codes.물류대행 => "입고 요청자",
            신청개인정보업무Codes.운송대행 => "운송 요청자",
            신청개인정보업무Codes.개별주문 => "주문 요청자",
            _ => "신청자"
        };

    public static string 운영원본종류(string 업무Code)
        => 업무Code switch
        {
            신청개인정보업무Codes.물류대행 => "WarehouseInboundRequest",
            신청개인정보업무Codes.운송대행 => "CargoTransportRequest",
            신청개인정보업무Codes.개별주문 => "MartOrderRequest",
            _ => throw new ArgumentOutOfRangeException(nameof(업무Code), 업무Code, "지원하지 않는 지도 신청 업무입니다.")
        };
}
