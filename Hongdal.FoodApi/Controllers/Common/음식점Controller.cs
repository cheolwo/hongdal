using Hongdal.Contracts.Restaurants;
using Microsoft.AspNetCore.Mvc;
using Hongdal.FoodApi.Services;

namespace Hongdal.FoodApi.Controllers.Common;

[ApiController]
[Route("api/v1/restaurants")]
public sealed class 음식점Controller : ControllerBase
{
    private readonly 음식샘플Store _store;

    public 음식점Controller(음식샘플Store store)
    {
        _store = store;
    }

    [HttpGet("nearby")]
    public ActionResult<음식점목록응답> 가까운조회()
    {
        return Ok(_store.GetNearbyRestaurants());
    }

    [HttpGet("popular")]
    public ActionResult<음식점목록응답> 인기조회()
    {
        return Ok(_store.GetPopularRestaurants());
    }

    [HttpGet("{restaurantId:long}/reviews")]
    public ActionResult<음식점리뷰목록응답> 리뷰조회(long restaurantId)
    {
        return Ok(_store.GetReviews(restaurantId));
    }

    [HttpPost("{restaurantId:long}/reviews")]
    public ActionResult<음식점리뷰요약응답> 리뷰등록(long restaurantId, [FromBody] 음식점리뷰등록요청 request)
    {
        return Ok(_store.AddReview(restaurantId, request));
    }
}
