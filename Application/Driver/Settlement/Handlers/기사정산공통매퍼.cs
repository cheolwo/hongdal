using Hongdal.Contracts.Driver.Settlement;

namespace Hongdal.Application.Driver.Settlement;

internal static class 기사정산공통매퍼
{
    internal static 기사정산월요약응답 To월요약응답(기사월정산 settlement, decimal monthlyFeeCap)
    {
        return new 기사정산월요약응답
        {
            DriverId = settlement.기사Id,
            Year = settlement.년도,
            Month = settlement.월,
            DispatchCount = settlement.배차건수,
            UsageFee = settlement.이용료,
            MonthlyFeeCap = monthlyFeeCap,
            RemainingUntilCap = Math.Max(0, monthlyFeeCap - settlement.이용료),
            IsPaid = settlement.결제완료
        };
    }

    internal static 기사정산응답 To응답(기사월정산 settlement, decimal monthlyFeeCap)
    {
        return new 기사정산응답
        {
            DriverId = settlement.기사Id,
            Year = settlement.년도,
            Month = settlement.월,
            DispatchCount = settlement.배차건수,
            UsageFee = settlement.이용료,
            MonthlyFeeCap = monthlyFeeCap,
            RemainingUntilCap = Math.Max(0, monthlyFeeCap - settlement.이용료),
            IsPaid = settlement.결제완료
        };
    }
}
