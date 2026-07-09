using 홍달.도메인.화주;

namespace 홍달.Services.Dispatch.Recommendation
{
    public sealed partial class 화물배차추천Service
    {
        private static decimal? ResolveEstimatedRevenue(화주운송의뢰? request)
        {
            if (request is null)
            {
                return null;
            }

            if (request.최종운임.HasValue)
            {
                return request.최종운임.Value;
            }

            return request.결제예정금액.HasValue ? request.결제예정금액.Value : null;
        }
    }
}
