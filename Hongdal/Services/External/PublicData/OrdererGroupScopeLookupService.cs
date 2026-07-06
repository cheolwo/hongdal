using System.Text;
using System.Text.RegularExpressions;
using Hongdal.Contracts.Common.PublicData;

namespace 홍달.Services.External.PublicData;

public sealed class OrdererGroupScopeLookupService : IOrdererGroupScopeLookupService
{
    public PublicDataLookupResponse<OrdererGroupScopeCandidateItem> FindCandidates(
        OrdererGroupScopeLookupRequest request)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 10);
        var addressParts = ResolveAddressParts(request);
        if (addressParts is null)
        {
            return new PublicDataLookupResponse<OrdererGroupScopeCandidateItem>
            {
                Success = false,
                ErrorMessage = "주문자 집단 범위를 계산할 도로명주소 또는 Kakao 지역 1/2단계 정보가 필요합니다.",
                Page = 1,
                PageSize = pageSize,
                Items = []
            };
        }

        var items = BuildCandidates(addressParts.Value)
            .Take(pageSize)
            .ToArray();

        return new PublicDataLookupResponse<OrdererGroupScopeCandidateItem>
        {
            Success = true,
            Page = 1,
            PageSize = pageSize,
            TotalCount = items.Length,
            Items = items
        };
    }

    private static IEnumerable<OrdererGroupScopeCandidateItem> BuildCandidates(AddressParts parts)
    {
        yield return new OrdererGroupScopeCandidateItem
        {
            ScopeKey = BuildScopeKey("road-address-level-2", parts.Level1, parts.Level2),
            DisplayName = $"{parts.Level1} {parts.Level2} 주문자 집단권",
            Basis = "RoadAddressLevel2",
            RoadAddressLevel1 = parts.Level1,
            RoadAddressLevel2 = parts.Level2,
            RoadAddressLevel3 = parts.Level3,
            AddressHint = BuildAddressHint(parts.Level1, parts.Level2),
            IsDefaultScope = true,
            SupportsApartmentSubScope = true,
            PrivacyNote = "상세주소, 동/호수, 세대 단위 정보는 집단 범위 후보에 포함하지 않습니다."
        };

        if (!string.IsNullOrWhiteSpace(parts.Level3))
        {
            yield return new OrdererGroupScopeCandidateItem
            {
                ScopeKey = BuildScopeKey("road-address-level-3", parts.Level1, parts.Level2, parts.Level3),
                DisplayName = $"{parts.Level1} {parts.Level2} {parts.Level3} 세부 주문자 집단권",
                Basis = "RoadAddressLevel3",
                RoadAddressLevel1 = parts.Level1,
                RoadAddressLevel2 = parts.Level2,
                RoadAddressLevel3 = parts.Level3,
                AddressHint = BuildAddressHint(parts.Level1, parts.Level2, parts.Level3),
                IsDefaultScope = false,
                SupportsApartmentSubScope = true,
                PrivacyNote = "세부 범위는 모집 밀도가 충분할 때만 사용하고, 기본 집단 형성은 2단계를 우선합니다."
            };
        }
    }

    private static AddressParts? ResolveAddressParts(OrdererGroupScopeLookupRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.KakaoRegionLevel1) &&
            !string.IsNullOrWhiteSpace(request.KakaoRegionLevel2))
        {
            return new AddressParts(
                NormalizeDisplay(request.KakaoRegionLevel1),
                NormalizeDisplay(request.KakaoRegionLevel2),
                NormalizeNullableDisplay(request.KakaoRegionLevel3));
        }

        var source = FirstNonBlank(request.RoadAddress, request.JibunAddress);
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        return ParseKoreanAddress(source);
    }

    private static AddressParts? ParseKoreanAddress(string address)
    {
        var tokens = Regex.Split(NormalizeDisplay(address), @"\s+")
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();

        if (tokens.Length < 2)
        {
            return null;
        }

        var level1 = tokens[0];
        var level2 = tokens[1];
        var level3Index = 2;

        if (tokens.Length >= 3 &&
            tokens[1].EndsWith("시", StringComparison.Ordinal) &&
            tokens[2].EndsWith("구", StringComparison.Ordinal))
        {
            level2 = $"{tokens[1]} {tokens[2]}";
            level3Index = 3;
        }

        var level3 = tokens.Length > level3Index ? tokens[level3Index] : null;
        return new AddressParts(level1, level2, level3);
    }

    private static string BuildAddressHint(params string?[] values)
        => string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string BuildScopeKey(string basis, params string?[] values)
    {
        var builder = new StringBuilder(basis);
        foreach (var value in values)
        {
            builder.Append(':');
            builder.Append(NormalizeKey(value));
        }

        return builder.ToString();
    }

    private static string NormalizeKey(string? value)
    {
        var normalized = NormalizeDisplay(value ?? string.Empty).ToLowerInvariant();
        return Regex.Replace(normalized, @"[^0-9a-z가-힣]+", "-").Trim('-');
    }

    private static string NormalizeDisplay(string value)
        => Regex.Replace(value.Trim(), @"\s+", " ");

    private static string? NormalizeNullableDisplay(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : NormalizeDisplay(value);

    private static string? FirstNonBlank(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private readonly record struct AddressParts(string Level1, string Level2, string? Level3);
}
