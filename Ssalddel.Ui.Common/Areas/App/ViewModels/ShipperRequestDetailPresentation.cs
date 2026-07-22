using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public enum ShipperRequestDetailMessageTone
{
    Info,
    Success,
    Warning,
    Error
}

public enum ShipperRequestProgressState
{
    Pending,
    Active,
    Completed,
    Attention
}

public sealed class ShipperRequestDetailPageState
{
    public string LookupRequestId { get; set; } = string.Empty;
    public ShipperRequestDetailSnapshot? Request { get; set; }
    public bool IsBusy { get; set; }
    public bool RequiresLogin { get; set; }
    public bool? IsWorkflowEnabled { get; set; } = true;
    public bool Created { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public ShipperRequestDetailMessageTone StatusTone { get; set; } = ShipperRequestDetailMessageTone.Info;
    public string SourceBoundaryMessage { get; set; }
        = "같은 운송 의뢰 ID의 저장 원장을 다시 조회해 표시합니다.";
}

public sealed record ShipperRequestDetailSnapshot
{
    public string RequestId { get; init; } = string.Empty;
    public string CargoType { get; init; } = string.Empty;
    public string RequestStatus { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string SettlementStatus { get; init; } = string.Empty;
    public string DispatchStatus { get; init; } = string.Empty;
    public string TransportMethod { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public int? ExpectedPaymentAmount { get; init; }
    public decimal? FinalFare { get; init; }
    public decimal? DriverFare { get; init; }
    public DateTime CreatedAt { get; init; }
    public string PickupAddress { get; init; } = string.Empty;
    public string DropoffAddress { get; init; } = string.Empty;
    public string SettlementTiming { get; init; } = string.Empty;
    public string EvidenceMethod { get; init; } = string.Empty;
    public string CollectionOwner { get; init; } = string.Empty;
    public bool TaxInvoiceRequired { get; init; }
    public bool CashReceiptRequired { get; init; }
    public string SettlementMemo { get; init; } = string.Empty;
    public string ReceiptNumber { get; init; } = string.Empty;
    public DateTime? ReceiptRegisteredAt { get; init; }
    public DateTime? OnSiteCollectionConfirmedAt { get; init; }
    public string OnSitePaymentMemo { get; init; } = string.Empty;
    public string CargoDimensions { get; init; } = string.Empty;
    public int? PalletCount { get; init; }
    public bool CanPay { get; init; }

    public static ShipperRequestDetailSnapshot FromContract(화주운송의뢰응답 source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ShipperRequestDetailSnapshot
        {
            RequestId = source.의뢰Id,
            CargoType = source.요약?.화물종류 ?? string.Empty,
            RequestStatus = source.의뢰상태,
            PaymentStatus = source.결제상태,
            SettlementStatus = source.정산상태,
            DispatchStatus = source.배차상태,
            TransportMethod = source.운송방식,
            VehicleType = source.차량종류,
            PaymentMethod = source.결제수단,
            ExpectedPaymentAmount = source.결제예정금액,
            FinalFare = source.최종운임,
            DriverFare = source.최종운임,
            CreatedAt = source.생성일시,
            PickupAddress = source.픽업지,
            DropoffAddress = source.하차지,
            SettlementTiming = source.정산시점?.ToString() ?? string.Empty,
            EvidenceMethod = source.증빙방식?.ToString() ?? string.Empty,
            CollectionOwner = source.수납주체?.ToString() ?? string.Empty,
            TaxInvoiceRequired = source.세금계산서필요,
            CashReceiptRequired = source.현금영수증필요,
            SettlementMemo = source.정산메모 ?? string.Empty,
            ReceiptNumber = source.인수증번호 ?? string.Empty,
            ReceiptRegisteredAt = source.인수증등록일시,
            OnSiteCollectionConfirmedAt = source.현장수금확인일시,
            OnSitePaymentMemo = source.현장지급메모 ?? string.Empty,
            CargoDimensions = BuildDimensions(source.화물길이Mm, source.화물폭Mm, source.화물높이Mm),
            PalletCount = source.팔레트개수,
            CanPay = ShipperRequestDetailPresentation.CanPay(source.배차상태, source.결제상태)
        };
    }

    private static string BuildDimensions(int? length, int? width, int? height)
        => length is > 0 && width is > 0 && height is > 0
            ? $"{length:N0} × {width:N0} × {height:N0} mm"
            : string.Empty;
}

public sealed record ShipperRequestTimelineStep(
    string Title,
    string Status,
    string Description,
    string Detail,
    ShipperRequestProgressState State);

public sealed record ShipperRequestProofItem(
    string Title,
    string Status,
    string Description,
    string Detail,
    ShipperRequestProgressState State);

public sealed record ShipperRequestPaymentReceiptPresentation(
    string ReceiptId,
    int Amount,
    string PaymentMethod,
    string SettlementMode,
    string PaymentStatus,
    DateTimeOffset ApprovedAt,
    string? PayerMemo);

public static class ShipperRequestDetailPresentation
{
    public static IReadOnlyList<ShipperRequestTimelineStep> BuildTimeline(ShipperRequestDetailSnapshot item)
        =>
        [
            new(
                "결제",
                Display(item.PaymentStatus),
                "화주 결제 또는 후불·현장 지급 조건을 확인하는 단계입니다.",
                $"{Display(item.PaymentMethod)} · {Money(item.ExpectedPaymentAmount)}",
                ResolvePaymentState(item)),
            new(
                "배차",
                Display(item.DispatchStatus),
                "저장된 배차 상태가 기사 추천과 수락 흐름으로 이어지는지 확인합니다.",
                $"{Display(item.VehicleType)} · {Display(item.TransportMethod)}",
                ResolveDispatchState(item)),
            new(
                "기사 수락",
                ResolveAcceptedStatus(item),
                "기사 응답 뒤 실제 운송 후보가 정해지는 단계입니다.",
                $"상차지 {Display(item.PickupAddress)}",
                ResolveAcceptedState(item)),
            new(
                "상차",
                ResolvePickupStatus(item),
                "상차지 도착과 화물 인수 상태를 확인합니다.",
                $"상차 상태 {Display(item.DispatchStatus)}",
                ResolvePickupState(item)),
            new(
                "하차",
                ResolveDropoffStatus(item),
                "하차 완료와 인수 확인 상태를 확인합니다.",
                $"하차지 {Display(item.DropoffAddress)}",
                ResolveDropoffState(item)),
            new(
                "정산",
                Display(item.SettlementStatus),
                "운송 완료 뒤 수납과 기사 정산 상태를 맞추는 단계입니다.",
                $"{Display(item.SettlementTiming)} · 기사 지급 {Money(item.DriverFare)}",
                ResolveSettlementState(item))
        ];

    public static IReadOnlyList<ShipperRequestProofItem> BuildProofs(ShipperRequestDetailSnapshot item)
        =>
        [
            new(
                "상차 증빙",
                ResolvePickupState(item) == ShipperRequestProgressState.Completed ? "상차 상태 기록됨" : "상차 전",
                "사진과 인수 내역은 기사 운송 원장에서 생성되며 이 화면은 연결 상태만 확인합니다.",
                $"배차 상태 {Display(item.DispatchStatus)}",
                ResolvePickupState(item)),
            new(
                "하차/POD",
                ResolveDropoffState(item) == ShipperRequestProgressState.Completed ? "하차 상태 기록됨" : "하차 전",
                "하차 사진과 POD는 원본 증빙 저장소를 기준으로 검수해야 합니다.",
                $"의뢰 상태 {Display(item.RequestStatus)}",
                ResolveDropoffState(item)),
            new(
                "인수증",
                string.IsNullOrWhiteSpace(item.ReceiptNumber) ? "등록 전" : "번호 등록됨",
                "운송 의뢰 원장에 저장된 인수증 번호와 등록 시각만 표시합니다.",
                string.IsNullOrWhiteSpace(item.ReceiptNumber)
                    ? $"증빙 방식 {Display(item.EvidenceMethod)}"
                    : $"{item.ReceiptNumber} · {DisplayDateTime(item.ReceiptRegisteredAt)}",
                ResolveReceiptState(item)),
            new(
                "세무 증빙 조건",
                ResolveTaxEvidenceStatus(item),
                "발급 요구 조건을 표시하며 실제 세금계산서나 현금영수증을 임의로 생성하지 않습니다.",
                $"세금계산서 {YesNo(item.TaxInvoiceRequired)} · 현금영수증 {YesNo(item.CashReceiptRequired)}",
                item.TaxInvoiceRequired || item.CashReceiptRequired
                    ? ShipperRequestProgressState.Active
                    : ShipperRequestProgressState.Pending)
        ];

    public static string ResolveOverallLabel(ShipperRequestDetailSnapshot item)
    {
        if (ResolveSettlementState(item) == ShipperRequestProgressState.Completed)
        {
            return "정산 완료";
        }

        if (ResolveDropoffState(item) == ShipperRequestProgressState.Completed)
        {
            return "하차 완료";
        }

        if (ResolvePickupState(item) == ShipperRequestProgressState.Completed)
        {
            return "운송 중";
        }

        if (ResolveAcceptedState(item) == ShipperRequestProgressState.Completed)
        {
            return "기사 수락";
        }

        return ResolveDispatchState(item) == ShipperRequestProgressState.Active ? "배차 진행" : "접수";
    }

    public static ShipperRequestProgressState ResolveOverallState(ShipperRequestDetailSnapshot item)
    {
        var timeline = BuildTimeline(item);
        if (timeline.Any(step => step.State == ShipperRequestProgressState.Attention))
        {
            return ShipperRequestProgressState.Attention;
        }

        if (ResolveSettlementState(item) == ShipperRequestProgressState.Completed)
        {
            return ShipperRequestProgressState.Completed;
        }

        return timeline.Any(step => step.State == ShipperRequestProgressState.Active)
            ? ShipperRequestProgressState.Active
            : ShipperRequestProgressState.Pending;
    }

    public static bool CanPay(string? dispatchStatus, string? paymentStatus)
        => ContainsAny(dispatchStatus, "상차완료", "운송중", "하차지도착", "하차완료", "인수완료")
           && !ContainsAny(paymentStatus, "결제완료", "결제확보", "입금확인", "승인완료");

    public static ShipperRequestProgressState ResolvePaymentState(ShipperRequestDetailSnapshot item)
    {
        if (HasProblem(item.PaymentStatus))
        {
            return ShipperRequestProgressState.Attention;
        }

        if (ContainsAny(item.PaymentStatus, "결제완료", "결제확보", "승인", "입금확인", "완료"))
        {
            return ShipperRequestProgressState.Completed;
        }

        return ContainsAny(item.PaymentStatus, "결제대기", "미결제", "입금대기", "청구", "대기")
            ? ShipperRequestProgressState.Active
            : ShipperRequestProgressState.Pending;
    }

    public static ShipperRequestProgressState ResolveDispatchState(ShipperRequestDetailSnapshot item)
    {
        if (HasProblem(item.DispatchStatus))
        {
            return ShipperRequestProgressState.Attention;
        }

        if (ContainsAny(item.DispatchStatus, "배차확정", "기사배정", "수락", "상차", "운송", "하차", "인수", "완료"))
        {
            return ShipperRequestProgressState.Completed;
        }

        return ContainsAny(item.DispatchStatus, "매칭", "배차대기", "추천", "대기")
            ? ShipperRequestProgressState.Active
            : ShipperRequestProgressState.Pending;
    }

    public static ShipperRequestProgressState ResolveAcceptedState(ShipperRequestDetailSnapshot item)
    {
        if (HasProblem(item.DispatchStatus))
        {
            return ShipperRequestProgressState.Attention;
        }

        if (ContainsAny(item.DispatchStatus, "수락", "배차확정", "기사배정", "상차", "운송", "하차", "인수", "완료"))
        {
            return ShipperRequestProgressState.Completed;
        }

        return ContainsAny(item.DispatchStatus, "추천", "매칭")
            ? ShipperRequestProgressState.Active
            : ShipperRequestProgressState.Pending;
    }

    public static ShipperRequestProgressState ResolvePickupState(ShipperRequestDetailSnapshot item)
    {
        if (HasProblem(item.DispatchStatus) || HasProblem(item.RequestStatus))
        {
            return ShipperRequestProgressState.Attention;
        }

        if (ContainsAny(item.DispatchStatus, "상차완료", "운송", "하차", "인수", "완료")
            || ContainsAny(item.RequestStatus, "상차완료", "운송", "하차", "완료"))
        {
            return ShipperRequestProgressState.Completed;
        }

        return ContainsAny(item.DispatchStatus, "상차지도착", "상차대기", "배차확정", "기사배정", "수락")
            ? ShipperRequestProgressState.Active
            : ShipperRequestProgressState.Pending;
    }

    public static ShipperRequestProgressState ResolveDropoffState(ShipperRequestDetailSnapshot item)
    {
        if (HasProblem(item.DispatchStatus) || HasProblem(item.RequestStatus))
        {
            return ShipperRequestProgressState.Attention;
        }

        if (ContainsAny(item.DispatchStatus, "하차완료", "배송완료", "인수완료", "완료")
            || ContainsAny(item.RequestStatus, "하차완료", "배송완료", "완료"))
        {
            return ShipperRequestProgressState.Completed;
        }

        return ContainsAny(item.DispatchStatus, "운송중", "하차지도착", "하차대기", "상차완료")
            ? ShipperRequestProgressState.Active
            : ShipperRequestProgressState.Pending;
    }

    public static ShipperRequestProgressState ResolveSettlementState(ShipperRequestDetailSnapshot item)
    {
        if (HasProblem(item.SettlementStatus) || HasProblem(item.PaymentStatus))
        {
            return ShipperRequestProgressState.Attention;
        }

        if (ContainsAny(item.SettlementStatus, "정산완료", "입금확인완료", "완료"))
        {
            return ShipperRequestProgressState.Completed;
        }

        return ContainsAny(item.SettlementStatus, "정산대기", "입금대기", "청구대기", "입금요청", "대기")
               || ResolveDropoffState(item) == ShipperRequestProgressState.Completed
            ? ShipperRequestProgressState.Active
            : ShipperRequestProgressState.Pending;
    }

    public static string Display(string? value, string fallback = "-")
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    public static string Money(decimal? amount)
        => amount.HasValue ? $"{amount.Value:N0}원" : "-";

    private static ShipperRequestProgressState ResolveReceiptState(ShipperRequestDetailSnapshot item)
    {
        if (!string.IsNullOrWhiteSpace(item.ReceiptNumber))
        {
            return ShipperRequestProgressState.Completed;
        }

        return ResolveDropoffState(item) == ShipperRequestProgressState.Completed
            ? ShipperRequestProgressState.Active
            : ShipperRequestProgressState.Pending;
    }

    private static string ResolveAcceptedStatus(ShipperRequestDetailSnapshot item)
        => ResolveAcceptedState(item) switch
        {
            ShipperRequestProgressState.Completed => "기사 수락 완료",
            ShipperRequestProgressState.Active => "기사 응답 대기",
            ShipperRequestProgressState.Attention => "수락 흐름 확인 필요",
            _ => "수락 전"
        };

    private static string ResolvePickupStatus(ShipperRequestDetailSnapshot item)
        => ResolvePickupState(item) switch
        {
            ShipperRequestProgressState.Completed => "상차 완료",
            ShipperRequestProgressState.Active => ContainsAny(item.DispatchStatus, "상차지도착") ? "상차지 도착" : "상차 준비",
            ShipperRequestProgressState.Attention => "상차 흐름 확인 필요",
            _ => "상차 전"
        };

    private static string ResolveDropoffStatus(ShipperRequestDetailSnapshot item)
        => ResolveDropoffState(item) switch
        {
            ShipperRequestProgressState.Completed => "하차 완료",
            ShipperRequestProgressState.Active => ContainsAny(item.DispatchStatus, "하차지도착") ? "하차지 도착" : "하차 진행",
            ShipperRequestProgressState.Attention => "하차 흐름 확인 필요",
            _ => "하차 전"
        };

    private static string ResolveTaxEvidenceStatus(ShipperRequestDetailSnapshot item)
    {
        if (item.TaxInvoiceRequired && item.CashReceiptRequired)
        {
            return "세금계산서·현금영수증 요청";
        }

        if (item.TaxInvoiceRequired)
        {
            return "세금계산서 요청";
        }

        return item.CashReceiptRequired ? "현금영수증 요청" : "추가 발급 요청 없음";
    }

    private static string DisplayDateTime(DateTime? value)
        => value.HasValue ? value.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "등록 시각 없음";

    private static string YesNo(bool value) => value ? "필요" : "불필요";

    private static bool HasProblem(string? value)
        => ContainsAny(value, "실패", "오류", "취소", "거절", "만료", "보류", "불일치", "부재", "분쟁");

    private static bool ContainsAny(string? value, params string[] tokens)
        => !string.IsNullOrWhiteSpace(value)
           && tokens.Any(token => value.Contains(token, StringComparison.OrdinalIgnoreCase));
}
