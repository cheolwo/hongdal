using Hongdal.Contracts.Common;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Contracts.Common.PublicData;
using 홍달.Services.External.PublicData;

namespace Hongdal.Services.Orderer;

public interface I주문자집단자동배정Service
{
    주문자집단자동배정응답? Resolve(주문자회원가입요청 request);
    주문자집단자동배정응답? Resolve(주문자집단온보딩요청 request);
}

public sealed class 주문자집단자동배정Service : I주문자집단자동배정Service
{
    private readonly I주문자집단배송권조회Service _scopeLookupService;

    public 주문자집단자동배정Service(I주문자집단배송권조회Service scopeLookupService)
    {
        _scopeLookupService = scopeLookupService;
    }

    public 주문자집단자동배정응답? Resolve(주문자회원가입요청 request)
        => Resolve(new 주문자집단자동배정입력(
            request.RoadAddress,
            request.JibunAddress,
            request.KakaoRegionLevel1,
            request.KakaoRegionLevel2,
            request.KakaoRegionLevel3,
            request.ApartmentComplexCode,
            request.ApartmentComplexName));

    public 주문자집단자동배정응답? Resolve(주문자집단온보딩요청 request)
        => Resolve(new 주문자집단자동배정입력(
            request.RoadAddress,
            request.JibunAddress,
            request.KakaoRegionLevel1,
            request.KakaoRegionLevel2,
            request.KakaoRegionLevel3,
            request.ApartmentComplexCode,
            request.ApartmentComplexName));

    private 주문자집단자동배정응답? Resolve(주문자집단자동배정입력 input)
    {
        if (!string.IsNullOrWhiteSpace(input.ApartmentComplexCode))
        {
            return ResolveApartmentScope(input);
        }

        var candidates = _scopeLookupService.후보검색(new 주문자집단배송권조회요청
        {
            RoadAddress = input.RoadAddress,
            JibunAddress = input.JibunAddress,
            KakaoRegionLevel1 = input.KakaoRegionLevel1,
            KakaoRegionLevel2 = input.KakaoRegionLevel2,
            KakaoRegionLevel3 = input.KakaoRegionLevel3,
            PageSize = 1
        });

        var candidate = candidates.Items.FirstOrDefault(x => x.IsDefaultScope)
                        ?? candidates.Items.FirstOrDefault();
        return candidate is null
            ? null
            : new 주문자집단자동배정응답
            {
                ScopeKey = candidate.ScopeKey,
                DisplayName = candidate.DisplayName,
                Basis = candidate.Basis,
                AddressHint = candidate.AddressHint,
                IsApartmentScope = false,
                PrivacyNote = candidate.PrivacyNote
            };
    }

    private static 주문자집단자동배정응답 ResolveApartmentScope(주문자집단자동배정입력 input)
    {
        var complexCode = input.ApartmentComplexCode!.Trim();
        var complexName = string.IsNullOrWhiteSpace(input.ApartmentComplexName)
            ? "아파트 단지"
            : input.ApartmentComplexName.Trim();
        var addressHint = FirstNonBlank(input.RoadAddress, input.JibunAddress) ?? complexName;

        return new 주문자집단자동배정응답
        {
            ScopeKey = $"apartment-complex:{NormalizeKey(complexCode)}",
            DisplayName = $"{complexName} 공동주문 집단",
            Basis = "ApartmentComplex",
            AddressHint = addressHint.Trim(),
            ApartmentComplexCode = complexCode,
            ApartmentComplexName = complexName,
            IsApartmentScope = true,
            PrivacyNote = "단지코드와 단지명까지만 집단 배정에 사용하고 동/호수 상세주소는 집단 범위에 포함하지 않습니다."
        };
    }

    private static string? FirstNonBlank(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static string NormalizeKey(string value)
        => value.Trim().ToLowerInvariant().Replace(" ", "-");

    private sealed record 주문자집단자동배정입력(
        string RoadAddress,
        string? JibunAddress,
        string? KakaoRegionLevel1,
        string? KakaoRegionLevel2,
        string? KakaoRegionLevel3,
        string? ApartmentComplexCode,
        string? ApartmentComplexName);
}
