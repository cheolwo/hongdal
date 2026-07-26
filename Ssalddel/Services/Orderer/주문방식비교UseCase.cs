using Ssalddel.Contracts.Common.CollectiveProcurement;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.CollectiveProcurement;

namespace Ssalddel.Services.Orderer;

public interface I주문방식비교UseCase
{
    주문방식비교응답 비교(주문방식비교요청 request);
}

/// <summary>
/// 개별주문과 같이 주문의 비용·시간을 같은 수량 기준으로 비교합니다.
/// 이 UseCase는 비교 결과만 만들며 주문 저장, 자동 집단화, 결제 또는 계약을 실행하지 않습니다.
/// </summary>
public sealed class 주문방식비교UseCase(
    ICollectiveProcurementEconomicsEngine 경제성Engine,
    TimeProvider timeProvider) : I주문방식비교UseCase
{
    public 주문방식비교응답 비교(주문방식비교요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var evaluatedAtUtc = request.기준시각Utc ?? timeProvider.GetUtcNow();
        Validate(request, evaluatedAtUtc);

        var individualGoods = Money(request.개별주문.상품단가 * request.요청수량);
        var individualExtras = Money(request.개별주문.배송비 + request.개별주문.기타비용);
        var individualTotal = Money(individualGoods + individualExtras);
        var individualUnitCost = UnitPrice(individualTotal / request.요청수량);
        var comparisonQuantity = Math.Max(
            request.같이주문.현재잠재수량,
            request.같이주문.최소성립수량);

        var assessmentRequest = BuildAssessment(
            request,
            individualUnitCost,
            comparisonQuantity);
        var assessment = 경제성Engine.Evaluate(assessmentRequest, evaluatedAtUtc);
        var scenario = assessment.CandidateScenarios.Single(item =>
            item.Quantity == comparisonQuantity);
        var groupUnitCost = scenario.EstimatedUnitLandedCost;
        var groupTotal = Money(groupUnitCost * request.요청수량);
        var groupTier = request.같이주문.공급가격구간
            .OrderBy(item => item.최소수량)
            .LastOrDefault(item => item.최소수량 <= comparisonQuantity)
            ?? request.같이주문.공급가격구간.OrderBy(item => item.최소수량).First();
        var groupGoods = Money(groupTier.상품단가 * request.요청수량);
        var groupExtras = Money(Math.Max(0m, groupTotal - groupGoods));
        var savings = Money(individualTotal - groupTotal);
        var savingsPercent = individualTotal == 0m
            ? 0m
            : Math.Round(
                savings / individualTotal * 100m,
                2,
                MidpointRounding.AwayFromZero);
        var recruitmentClosed = request.같이주문.모집마감시각Utc <= evaluatedAtUtc;
        var minimumMet = request.같이주문.현재잠재수량 >= request.같이주문.최소성립수량;
        var waitWithinLimit = ResolveWaitWithinLimit(request);
        var additionalWaitHours = ResolveAdditionalWaitHours(request);
        var signal = ResolveSignal(
            recruitmentClosed,
            savings > 0m,
            waitWithinLimit,
            minimumMet);
        var progress = Math.Min(
            100m,
            Math.Round(
                request.같이주문.현재잠재수량
                / request.같이주문.최소성립수량
                * 100m,
                1,
                MidpointRounding.AwayFromZero));

        return new 주문방식비교응답
        {
            상품키 = request.상품키.Trim(),
            상품명 = request.상품명.Trim(),
            요청수량 = request.요청수량,
            수량단위 = request.수량단위.Trim(),
            통화코드 = request.통화코드.Trim().ToUpperInvariant(),
            기준시각Utc = evaluatedAtUtc,
            개별주문 = new 주문방식비용응답
            {
                상품금액 = individualGoods,
                물류및부대비용 = individualExtras,
                총예상비용 = individualTotal,
                단위당예상비용 = individualUnitCost,
                예상수령시각Utc = request.개별주문.예상수령시각Utc
            },
            같이주문 = new 주문방식비용응답
            {
                상품금액 = groupGoods,
                물류및부대비용 = groupExtras,
                총예상비용 = groupTotal,
                단위당예상비용 = groupUnitCost,
                예상수령시각Utc = request.같이주문.예상수령시각Utc
            },
            같이주문모집 = new 같이주문모집비교응답
            {
                현재참여자수 = request.같이주문.현재참여자수,
                목표참여자수 = request.같이주문.목표참여자수,
                현재잠재수량 = request.같이주문.현재잠재수량,
                비교기준모집수량 = comparisonQuantity,
                최소성립수량 = request.같이주문.최소성립수량,
                추가필요수량 = Math.Max(
                    0m,
                    request.같이주문.최소성립수량 - request.같이주문.현재잠재수량),
                모집진척률 = progress,
                최소성립조건충족 = minimumMet,
                모집마감 = recruitmentClosed,
                모집마감시각Utc = request.같이주문.모집마감시각Utc
            },
            판단 = new 주문방식비교판단응답
            {
                신호코드 = signal.Code,
                안내 = signal.Message,
                예상절감액 = savings,
                예상절감률 = savingsPercent,
                추가대기시간Hours = additionalWaitHours,
                같이비용절감가능 = savings > 0m,
                최대대기허용범위안 = waitWithinLimit,
                같이주문검토가능 = !recruitmentClosed
            },
            계산근거 = BuildEvidence(request, comparisonQuantity, groupTier),
            주의사항 = assessment.Warnings
                .Append("표시 금액은 입력된 가격·물류비·위험 예비비를 같은 수량 기준으로 계산한 추정치입니다.")
                .Append("같이 주문 참여는 별도 동의가 필요하며 비교만으로 자동 가입·결제·계약이 실행되지 않습니다.")
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static CollectiveProcurementAssessmentRequest BuildAssessment(
        주문방식비교요청 request,
        decimal individualUnitCost,
        decimal comparisonQuantity)
        => new()
        {
            CurrencyCode = request.통화코드,
            QuantityUnit = request.수량단위,
            CurrentCommittedQuantity = request.같이주문.현재확정수량,
            CurrentPotentialQuantity = request.같이주문.현재잠재수량,
            MinimumOrderQuantity = request.같이주문.최소성립수량,
            MaximumSafeQuantity = request.같이주문.최대안전수량,
            QuantityIncrement = request.같이주문.계산증분,
            ComparisonUnitPrice = individualUnitCost,
            TargetSavingsPercent = request.같이주문.목표절감률,
            RiskReservePercent = request.같이주문.위험예비비율,
            CandidateQuantities = [comparisonQuantity],
            SupplierPriceTiers = request.같이주문.공급가격구간
                .Select(item => new CollectiveProcurementSupplierPriceTierRequest
                {
                    Label = item.이름,
                    MinimumQuantity = item.최소수량,
                    UnitPrice = item.상품단가,
                    SourceReference = item.근거,
                    ValidUntilUtc = item.유효시각Utc
                })
                .ToList(),
            CostComponents = request.같이주문.비용항목
                .Select(item => new CollectiveProcurementCostComponentRequest
                {
                    Code = item.코드,
                    Label = item.이름,
                    CategoryCode = item.비용분류코드,
                    ModelCode = item.계산방식코드,
                    Amount = item.금액,
                    CapacityQuantity = item.용량수량,
                    SourceReference = item.근거,
                    ValidUntilUtc = item.유효시각Utc
                })
                .ToList()
        };

    private static bool? ResolveWaitWithinLimit(주문방식비교요청 request)
    {
        if (!request.최대대기가능시각Utc.HasValue
            || !request.같이주문.예상수령시각Utc.HasValue)
        {
            return null;
        }

        return request.같이주문.예상수령시각Utc <= request.최대대기가능시각Utc;
    }

    private static decimal? ResolveAdditionalWaitHours(주문방식비교요청 request)
    {
        if (!request.개별주문.예상수령시각Utc.HasValue
            || !request.같이주문.예상수령시각Utc.HasValue)
        {
            return null;
        }

        return Math.Round(
            Math.Max(
                0m,
                (decimal)(request.같이주문.예상수령시각Utc.Value
                    - request.개별주문.예상수령시각Utc.Value).TotalHours),
            1,
            MidpointRounding.AwayFromZero);
    }

    private static (string Code, string Message) ResolveSignal(
        bool recruitmentClosed,
        bool groupCostLower,
        bool? waitWithinLimit,
        bool minimumMet)
    {
        if (recruitmentClosed)
        {
            return (
                주문방식비교신호코드.같이모집마감,
                "이 같이 주문 모집은 마감되었습니다. 개별 주문을 계속하거나 다른 모집을 확인하세요.");
        }

        if (!groupCostLower)
        {
            return (
                주문방식비교신호코드.개별비용우위,
                "현재 추정에서는 같이 주문이 더 저렴하지 않습니다. 두 방식의 비용 근거를 확인해 선택하세요.");
        }

        if (waitWithinLimit == false)
        {
            return (
                주문방식비교신호코드.같이비용절감대기초과,
                "같이 주문 예상 비용은 낮지만 입력한 최대 대기 가능 시각을 넘습니다.");
        }

        if (!minimumMet)
        {
            return (
                주문방식비교신호코드.같이비용절감성립대기,
                "최소 수량이 모이면 비용을 줄일 가능성이 있습니다. 남은 수량과 예상 대기시간을 확인하세요.");
        }

        return (
            주문방식비교신호코드.같이비용절감가능,
            "현재 모집 조건에서는 같이 주문 예상 비용이 낮습니다. 시간과 역할을 확인한 뒤 별도로 참여할 수 있습니다.");
    }

    private static IReadOnlyList<string> BuildEvidence(
        주문방식비교요청 request,
        decimal comparisonQuantity,
        같이주문공급가격구간입력 groupTier)
    {
        var evidence = new List<string>
        {
            $"같은 요청 수량 {request.요청수량:0.####} {request.수량단위.Trim()} 기준",
            $"같이 주문 비교 모집 수량 {comparisonQuantity:0.####} {request.수량단위.Trim()} 기준",
            $"공동 공급가격 구간: {groupTier.이름.Trim()}"
        };

        if (!string.IsNullOrWhiteSpace(request.개별주문.가격근거))
        {
            evidence.Add($"개별 가격 근거: {request.개별주문.가격근거.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(groupTier.근거))
        {
            evidence.Add($"공동 가격 근거: {groupTier.근거.Trim()}");
        }

        return evidence;
    }

    private static void Validate(
        주문방식비교요청 request,
        DateTimeOffset evaluatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(request.개별주문);
        ArgumentNullException.ThrowIfNull(request.같이주문);
        ArgumentNullException.ThrowIfNull(request.같이주문.공급가격구간);
        ArgumentNullException.ThrowIfNull(request.같이주문.비용항목);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.상품키);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.상품명);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.수량단위);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.통화코드);

        if (request.요청수량 <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "요청 수량은 0보다 커야 합니다.");
        }

        if (request.개별주문.상품단가 <= 0m
            || request.개별주문.배송비 < 0m
            || request.개별주문.기타비용 < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "개별주문 상품 단가는 양수이고 추가 비용은 0 이상이어야 합니다.");
        }

        if (request.같이주문.현재참여자수 < 0
            || request.같이주문.목표참여자수 <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "같이 주문 참여자 수 조건이 올바르지 않습니다.");
        }

        if (request.같이주문.공급가격구간.Count == 0)
        {
            throw new ArgumentException("같이 주문 공급가격 구간이 하나 이상 필요합니다.", nameof(request));
        }

        var unsupportedCostModel = request.같이주문.비용항목.FirstOrDefault(item =>
            !CollectiveProcurementCostModelCodes.All.Contains(item.계산방식코드));
        if (unsupportedCostModel is not null)
        {
            throw new ArgumentException(
                $"지원하지 않는 같이 주문 비용 계산 방식입니다: {unsupportedCostModel.계산방식코드}",
                nameof(request));
        }

        if (request.최대대기가능시각Utc < evaluatedAtUtc)
        {
            throw new ArgumentException("최대 대기 가능 시각은 기준 시각보다 빠를 수 없습니다.", nameof(request));
        }
    }

    private static decimal Money(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal UnitPrice(decimal value)
        => Math.Round(value, 4, MidpointRounding.AwayFromZero);
}
