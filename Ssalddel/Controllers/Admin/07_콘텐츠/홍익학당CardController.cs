using Ssalddel.ApiMetadata;
using Ssalddel.Services.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Admin.Content07;

[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[ApiController]
[Route("api/v1/admin/content/hongik-hakdang/cards")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("HongikHakdangCardController")]
public sealed class 홍익학당CardController : ControllerBase
{
    private readonly IHongikHakdangCardService _홍익학당CardService;
    private readonly IHongikHakdangCardVariantService _variantService;

    public 홍익학당CardController(
        IHongikHakdangCardService 홍익학당CardService,
        IHongikHakdangCardVariantService variantService)
    {
        _홍익학당CardService = 홍익학당CardService;
        _variantService = variantService;
    }

    [HttpGet]
    [SsalddelApiContractName("GetCollections")]
    public async Task<IActionResult> 모음목록조회(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
        => Ok(await _홍익학당CardService.GetCollectionsAsync(includeInactive, cancellationToken));

    [HttpPost("sync")]
    [SsalddelApiContractName("Sync")]
    public async Task<IActionResult> 동기화(CancellationToken cancellationToken)
        => Ok(await _홍익학당CardService.SyncAsync(cancellationToken));

    [HttpPost("variants/prepare")]
    [SsalddelApiContractName("PrepareVariants")]
    public async Task<IActionResult> 변형준비(CancellationToken cancellationToken)
        => Ok(await _variantService.EnsureActiveVariantsAsync(cancellationToken));

    [HttpPut("collections/{collectionId:long}/activation")]
    [SsalddelApiContractName("SetCollectionActivation")]
    public async Task<IActionResult> 모음활성설정(
        long collectionId,
        [FromBody] Ssalddel.Contracts.Common.Content.HongikHakdangCardActivationUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _홍익학당CardService.SetCollectionEnabledAsync(collectionId, request.Enabled, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{cardId:long}/activation")]
    [SsalddelApiContractName("SetCardActivation")]
    public async Task<IActionResult> Card활성설정(
        long cardId,
        [FromBody] Ssalddel.Contracts.Common.Content.HongikHakdangCardActivationUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _홍익학당CardService.SetCardEnabledAsync(cardId, request.Enabled, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{cardId:long}/community-publication")]
    [SsalddelApiContractName("SetCardCommunityPublication")]
    public async Task<IActionResult> Card커뮤니티게시설정(
        long cardId,
        [FromBody] Ssalddel.Contracts.Common.Content.HongikHakdangCardCommunityPublicationUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _홍익학당CardService.SetCardCommunityPublicationApprovedAsync(
            cardId,
            request.Approved,
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{cardId:long}/image")]
    [SsalddelApiContractName("GetCardImage")]
    public async Task<IActionResult> Card이미지조회(long cardId, CancellationToken cancellationToken)
    {
        var result = await _홍익학당CardService.GetCardImageAsync(cardId, cancellationToken);
        return result is null ? NotFound() : File(result.Bytes, result.ContentType);
    }
}
