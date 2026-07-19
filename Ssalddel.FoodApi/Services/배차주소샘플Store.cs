using Ssalddel.FoodApi.Contracts;

namespace Ssalddel.FoodApi.Services;

public sealed class 배차주소샘플Store
{
    private readonly List<배차주소저장요청> _items = [];

    public 배차주소저장응답 저장(배차주소저장요청 request, (double 위도, double 경도)? 상차좌표, (double 위도, double 경도)? 하차좌표)
    {
        _items.Add(request);

        return new 배차주소저장응답
        {
            메시지 = $"배차주소 저장 완료: 상차지({request.상차지기본주소}) -> 하차지({request.하차지기본주소})",
            상차지위도 = 상차좌표?.위도,
            상차지경도 = 상차좌표?.경도,
            하차지위도 = 하차좌표?.위도,
            하차지경도 = 하차좌표?.경도
        };
    }
}
