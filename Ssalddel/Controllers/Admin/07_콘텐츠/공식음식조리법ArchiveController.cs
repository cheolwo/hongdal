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
    Boundary = "서버관리자만 수집·대표성 검토하며 사진 파일 저장, 자동 대표 선정, 요청 즉시 커뮤니티 게시, 주문·원장 생성은 수행하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/admin/content/official-food-recipes")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("OfficialFoodRecipeArchiveController")]
public sealed class 공식음식조리법ArchiveController : ControllerBase
{
    private readonly IOfficialFoodRecipeArchiveService _음식조리법ArchiveService;
    private readonly IOfficialFoodRecipeIngredientIndexService _ingredientIndexService;
    private readonly IOfficialFoodIngredientPublicPriceService _ingredientPriceService;

    public 공식음식조리법ArchiveController(
        IOfficialFoodRecipeArchiveService 음식조리법ArchiveService,
        IOfficialFoodRecipeIngredientIndexService ingredientIndexService,
        IOfficialFoodIngredientPublicPriceService ingredientPriceService)
    {
        _음식조리법ArchiveService = 음식조리법ArchiveService;
        _ingredientIndexService = ingredientIndexService;
        _ingredientPriceService = ingredientPriceService;
    }

    [HttpGet("sources")]
    [SsalddelApiContractName("GetSources")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodRecipeSourceDto>>> 출처목록조회(
        CancellationToken cancellationToken)
        => Ok(await _음식조리법ArchiveService.GetSourcesAsync(cancellationToken));

    [HttpGet("dishes")]
    [SsalddelApiContractName("GetDishes")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodRecipeDishDto>>> 음식목록조회(
        [FromQuery] OfficialFoodRecipeQuery query,
        CancellationToken cancellationToken)
        => Ok(await _음식조리법ArchiveService.SearchDishesAsync(query, cancellationToken));

    [HttpGet("dishes/{dishKey}/variants")]
    [SsalddelApiContractName("GetVariants")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodRecipeVariantDto>>> 변형목록조회(
        string dishKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var variants = await _음식조리법ArchiveService.GetVariantsAsync(dishKey, cancellationToken);
            return variants.Count == 0 ? NotFound() : Ok(variants);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(exception.Message));
        }
    }

    [HttpPut("dishes/{dishKey}/review")]
    [SsalddelApiContractName("ReviewDish")]
    public async Task<ActionResult<OfficialFoodRecipeDishDto>> 음식검토(
        string dishKey,
        [FromBody] OfficialFoodRecipeDishReviewRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _음식조리법ArchiveService.ReviewDishAsync(
                dishKey,
                request,
                cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(exception.Message));
        }
    }

    [HttpGet("ingredients/categories")]
    [SsalddelApiContractName("GetIngredientCategories")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodIngredientCategoryDto>>> 식재료범주목록조회(
        CancellationToken cancellationToken)
        => Ok(await _ingredientIndexService.GetCategoriesAsync(cancellationToken));

    [HttpGet("ingredients")]
    [SsalddelApiContractName("GetIngredients")]
    public async Task<ActionResult<IReadOnlyList<OfficialFoodIngredientDto>>> 식재료목록조회(
        [FromQuery] OfficialFoodIngredientQuery query,
        CancellationToken cancellationToken)
        => Ok(await _ingredientIndexService.SearchIngredientsAsync(query, cancellationToken));

    [HttpPost("ingredients/index")]
    [SsalddelApiContractName("IndexIngredients")]
    public async Task<ActionResult<OfficialFoodIngredientIndexResponse>> 식재료색인(
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
    [SsalddelApiContractName("IndexIngredientPrices")]
    public async Task<ActionResult<OfficialFoodIngredientPriceIndexResponse>> 식재료가격색인(
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
    [SsalddelApiContractName("Collect")]
    public async Task<ActionResult<OfficialFoodRecipeCollectionResponse>> 수집(
        [FromBody] OfficialFoodRecipeCollectionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _음식조리법ArchiveService.CollectAsync(request, cancellationToken));
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
