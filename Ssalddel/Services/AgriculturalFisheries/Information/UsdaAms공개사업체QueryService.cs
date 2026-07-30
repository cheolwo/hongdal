using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed class UsdaAms공개사업체QueryService(
    AgriculturalFisheriesDbContext db)
    : IUsdaAms공개사업체QueryService
{
    public async Task<UsdaAms공개사업체조회응답> SearchAsync(
        UsdaAms공개사업체조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var searchText = string.IsNullOrWhiteSpace(request.SearchText)
            ? null
            : UsdaAms공개사업체TextNormalizer.NormalizeSearchText(
                request.SearchText);
        var directoryType = string.IsNullOrWhiteSpace(
            request.DirectoryTypeCode)
            ? null
            : UsdaAms공개사업체DirectoryCatalog.Normalize(
                request.DirectoryTypeCode);
        var stateCode = NormalizeStateCode(request.StateCode);
        var productKey = string.IsNullOrWhiteSpace(request.ProductKey)
            ? null
            : UsdaAms공개사업체TextNormalizer.CreateProductKey(
                request.ProductKey);

        var query = db.UsdaAmsPublicBusinessProfiles
            .AsNoTracking()
            .Where(item =>
                item.SourceKey
                == UsdaAms공개사업체원천Keys.LocalFoodDirectories);

        if (request.CurrentOnly)
        {
            query = query.Where(item => item.IsCurrentlyListed);
        }

        if (searchText is not null)
        {
            query = query.Where(item =>
                item.BusinessNameNormalized.Contains(searchText));
        }

        if (directoryType is not null)
        {
            query = query.Where(item =>
                item.DirectoryTypeCode == directoryType);
        }

        if (stateCode is not null)
        {
            query = query.Where(item => item.StateCode == stateCode);
        }

        if (productKey is not null)
        {
            query = query.Where(item =>
                item.Products.Any(product => product.ProductKey == productKey));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var skip = (long)(page - 1) * pageSize;
        var profiles = skip > int.MaxValue
            ? []
            : await query
                .Include(item => item.Products)
                .OrderBy(item => item.BusinessNameNormalized)
                .ThenBy(item => item.Id)
                .Skip((int)skip)
                .Take(pageSize)
                .ToArrayAsync(cancellationToken);

        return new UsdaAms공개사업체조회응답
        {
            SourceKey = UsdaAms공개사업체원천Keys.LocalFoodDirectories,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Notices =
            [
                "이 목록은 사업자가 자발적으로 등재한 공급 후보 탐색 자료이며 인증·허가·거래 가능 여부를 보증하지 않습니다.",
                "계약이나 발주 전에 업체 본인 동의, 최신 공식 listing, 품목·지역별 자격을 다시 확인해야 합니다.",
                "상세 주소·좌표·담당자·전화·이메일은 살뜰 DB와 이 응답에 포함하지 않습니다.",
                "AMS 시장가격은 지역·품목의 비교 근거이며 특정 업체가 제시한 가격으로 연결하지 않습니다."
            ],
            Items = profiles.Select(Map).ToArray()
        };
    }

    private static string? NormalizeStateCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var stateCode = value.Trim().ToUpperInvariant();
        if (stateCode.Length == 2)
        {
            return stateCode;
        }

        throw new ArgumentException(
            "StateCode는 미국 주 2자리 약어여야 합니다.",
            nameof(value));
    }

    private static UsdaAms공개사업체항목 Map(
        UsdaAms공개사업체Profile profile)
        => new()
        {
            ProfileKey = profile.ProfileKey,
            SourceKey = profile.SourceKey,
            DirectoryTypeCode = profile.DirectoryTypeCode,
            ExternalListingId = profile.ExternalListingId,
            BusinessName = profile.BusinessName,
            CityName = profile.CityName,
            StateCode = profile.StateCode,
            LocationPrecisionCode = profile.LocationPrecisionCode,
            EstablishedYear = profile.EstablishedYear,
            LegalStatus = profile.LegalStatus,
            Products = profile.Products
                .OrderBy(product => product.ProductName)
                .Select(product => product.ProductName)
                .ToArray(),
            HasRetailChannel = profile.HasRetailChannel,
            HasWholesaleChannel = profile.HasWholesaleChannel,
            HasProducerService = profile.HasProducerService,
            HasProcurementService = profile.HasProcurementService,
            IsCurrentlyListed = profile.IsCurrentlyListed,
            SourceUpdatedAt = profile.SourceUpdatedAt,
            LastSeenAtUtc = profile.LastSeenAtUtc,
            OfficialListingUrl = profile.OfficialListingUrl
        };
}
