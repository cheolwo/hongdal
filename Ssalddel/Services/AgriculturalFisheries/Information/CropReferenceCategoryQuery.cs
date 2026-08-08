using System.Text;
using Ssalddel.Contracts.Common.PublicData;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface I작물기준정보분류조회UseCase
{
    Task<CropReferenceCategoryListResponse> 조회Async(
        CancellationToken cancellationToken = default);
}

public sealed class 작물기준정보분류조회UseCase(
    I농사로작목기술Module module) : I작물기준정보분류조회UseCase
{
    private const string SourceKey = "nongsaro:crop-ebook";
    private const string SourceName = "농촌진흥청 농사로 작목별 농업기술정보";
    private const string Boundary =
        "공개 작목기술 분류 기준이며 특정 농장의 현재 재배 상태, 생산량, 재고, 판매 가능성 또는 위치를 의미하지 않습니다.";

    public async Task<CropReferenceCategoryListResponse> 조회Async(
        CancellationToken cancellationToken = default)
    {
        var source = await module.주분류조회Async(cancellationToken);
        var items = MapItems(source.Items);

        return new CropReferenceCategoryListResponse(
            CropReferenceSourceTypeCodes.PublicReference,
            SourceKey,
            SourceName,
            source.SourceDocumentationUrl,
            source.RetrievedAtUtc,
            Boundary,
            items);
    }

    internal static IReadOnlyList<CropReferenceCategoryItem> MapItems(
        IReadOnlyList<Nongsaro공공데이터Item> sourceItems)
    {
        ArgumentNullException.ThrowIfNull(sourceItems);

        var items = new List<CropReferenceCategoryItem>(sourceItems.Count);
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in sourceItems)
        {
            if (source is null)
            {
                throw new InvalidOperationException("농사로 작목 분류 item이 비어 있습니다.");
            }

            var code = source.Get("mainCategoryCode").Trim();
            var name = source.Get("mainCategoryNm").Trim();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("농사로 작목 분류 code 또는 이름이 없습니다.");
            }

            if (!codes.Add(code))
            {
                throw new InvalidOperationException($"농사로 작목 분류 code가 중복되었습니다: {code}");
            }

            items.Add(new CropReferenceCategoryItem(
                $"crop-reference-category:{NormalizeStableSegment(code)}",
                code,
                name));
        }

        return items;
    }

    private static string NormalizeStableSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value.ToLowerInvariant())
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(character);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var result = builder.ToString().Trim('-');
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException("농사로 작목 분류 code를 stable ID로 변환할 수 없습니다.");
        }

        return result;
    }
}
