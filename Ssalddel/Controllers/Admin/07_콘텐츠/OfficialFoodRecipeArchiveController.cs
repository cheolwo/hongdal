using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Controllers.Admin.Content07;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Authoring,
    SsalddelModuleKind.Api,
    "각국 정부 공식 음식 레시피의 권리 정책·대표 음식 후보·원문 변형을 보관하고 검토하는 HTTP 경계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "서버관리자만 수집하며 사진 파일 저장, 자동 대표 선정, 커뮤니티 자동 게시, 주문·원장 생성은 수행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/official-food-recipes")]
[Authorize(Policy = "서버관리자전용")]
public sealed class OfficialFoodRecipeArchiveController : ControllerBase
{
    private readonly IOfficialFoodRecipeArchiveService _service;
    private readonly IOfficialFoodRecipeIngredientIndexService _ingredientIndexService;
    private readonly IOfficialFoodIngredientPublicPriceService _ingredientPriceService;

    public OfficialFoodRecipeArchiveController(
        IOfficialFoodRecipeArchiveService service,
        IOfficialFoodRecipeIngredientIndexService ingredientIndexService,
        IOfficialFoodIngredientPublicPriceService ingredientPriceService)
    {
        _service = service;
        _ingredientIndexService = ingredientIndexService;
        _ingredientPriceService = ingredientPriceService;
    }

    [HttpGet("sources")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodRecipeSourceDto>>> GetSources(
        CancellationToken cancellationToken)
        => Ok(await _service.GetSourcesAsync(cancellationToken));

    [HttpGet("dishes")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodRecipeDishDto>>> GetDishes(
        [FromQuery] OfficialFoodRecipeQuery query,
        CancellationToken cancellationToken)
        => Ok(await _service.SearchDishesAsync(query, cancellationToken));

    [HttpGet("dishes/{dishKey}/variants")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodRecipeVariantDto>>> GetVariants(
        string dishKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var variants = await _service.GetVariantsAsync(dishKey, cancellationToken);
            return variants.Count == 0 ? NotFound() : Ok(variants);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(exception.Message));
        }
    }

    [HttpGet("ingredients/categories")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodIngredientCategoryDto>>> GetIngredientCategories(
        CancellationToken cancellationToken)
        => Ok(await _ingredientIndexService.GetCategoriesAsync(cancellationToken));

    [HttpGet("ingredients")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodIngredientDto>>> GetIngredients(
        [FromQuery] OfficialFoodIngredientQuery query,
        CancellationToken cancellationToken)
        => Ok(await _ingredientIndexService.SearchIngredientsAsync(query, cancellationToken));

    [HttpPost("ingredients/index")]
    public async Task<ActionResult<OfficialFoodIngredientIndexResponse>> IndexIngredients(
        [FromBody] OfficialFoodIngredientIndexRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _ingredientIndexService.RebuildAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(CreateProblem(exception.Message));
        }
    }

    [HttpPost("ingredients/prices/index")]
    public async Task<ActionResult<OfficialFoodIngredientPriceIndexResponse>> IndexIngredientPrices(
        [FromBody] OfficialFoodIngredientPriceIndexRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _ingredientPriceService.RebuildMappingsAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(exception.Message));
        }
    }

    [HttpPost("collections")]
    public async Task<ActionResult<OfficialFoodRecipeCollectionResponse>> Collect(
        [FromBody] OfficialFoodRecipeCollectionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.CollectAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(CreateProblem(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(CreateProblem(exception.Message));
        }
    }

    private static ProblemDetails CreateProblem(string detail)
        => new()
        {
            Title = "공식 음식 레시피 아카이브 요청을 처리하지 못했습니다.",
            Detail = detail
        };
}
