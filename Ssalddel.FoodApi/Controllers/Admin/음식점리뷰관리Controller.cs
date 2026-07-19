using Ssalddel.Contracts.Admin.Restaurants;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.FoodApi.Services;

namespace Ssalddel.FoodApi.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/restaurant-reviews")]
public sealed class 음식점리뷰관리Controller : ControllerBase
{
    private readonly 음식샘플Store _store;

    public 음식점리뷰관리Controller(음식샘플Store store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<음식점리뷰관리목록응답> 목록조회()
    {
        return Ok(_store.GetModerationItems());
    }

    [HttpGet("policy")]
    public ActionResult<음식점리뷰운영정책응답> 정책조회()
    {
        return Ok(_store.GetPolicy());
    }
}
