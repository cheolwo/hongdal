using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.Content;

namespace Ssalddel.Controllers.Common;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.AppContextImageAsset,
    SsalddelCodeLayer.Api,
    "앱별 화면 문맥 이미지의 공개 URL과 장면 metadata 조회 API",
    ContractType = typeof(I앱문맥이미지자산조회UseCase),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.ObjectStorageRead,
    Boundary = "활성화된 공개 자산만 반환하며 Blob 원본 credential이나 내부 storage 위치를 공개하지 않습니다.")]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[ApiController]
[Route(AppContextImageAssetRoutes.Base)]
[AllowAnonymous]
public sealed class 앱문맥이미지자산Controller(
    I앱문맥이미지자산조회UseCase useCase) : ControllerBase
{
    [HttpGet("{appPackId}")]
    public async Task<ActionResult<AppContextImageAssetListResponse>> 팩조회(
        string appPackId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await useCase.팩조회Async(appPackId, cancellationToken);
            var absoluteItems = response.Items
                .Select(item => item with
                {
                    ImageUrl = ToPublicAbsoluteUrl(item.ImageUrl)
                })
                .ToArray();
            return Ok(response with { Items = absoluteItems });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "앱 이미지 팩 조건을 확인해 주세요.",
                Detail = exception.Message
            });
        }
    }

    private string ToPublicAbsoluteUrl(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var absolute))
        {
            return absolute.AbsoluteUri;
        }

        var relative = value.StartsWith('/') ? value : $"/{value}";
        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}{relative}";
    }
}
