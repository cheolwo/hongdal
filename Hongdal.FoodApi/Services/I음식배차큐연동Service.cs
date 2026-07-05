using Hongdal.Contracts.Food;

namespace Hongdal.FoodApi.Services;

public interface I음식배차큐연동Service
{
    Task 배차대기생성요청Async(음식주문응답 order, decimal? 픽업위도, decimal? 픽업경도, string 픽업주소, CancellationToken cancellationToken = default);
}
