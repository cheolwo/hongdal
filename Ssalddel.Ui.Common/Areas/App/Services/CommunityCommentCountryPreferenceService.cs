using System.Globalization;
using System.Text.Json;
using Microsoft.JSInterop;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public sealed record CommunityCommentCountryPreference(
    bool IsDisplayCountryPublic,
    string? CountryCode);

public interface ICommunityCommentCountryPreferenceService
{
    Task<CommunityCommentCountryPreference> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CommunityCommentCountryPreference preference, CancellationToken cancellationToken = default);
}

public sealed class CommunityCommentCountryPreferenceService(
    IJSRuntime jsRuntime,
    IOperatingMarketProfileClient operatingMarketProfileClient)
    : ICommunityCommentCountryPreferenceService
{
    private const string StorageKey = "ssalddel.community.comment-country.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CommunityCommentCountryPreference> GetAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stored = await jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem",
                cancellationToken,
                StorageKey);
            if (!string.IsNullOrWhiteSpace(stored))
            {
                var preference = JsonSerializer.Deserialize<CommunityCommentCountryPreference>(
                    stored,
                    JsonOptions);
                if (preference is not null)
                {
                    return Normalize(preference);
                }
            }
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // prerender 또는 localStorage 비지원 환경에서는 공개 locale과 운영시장 순서로 계속합니다.
        }

        var localeCountry = ResolveCurrentPublicLocaleCountry();
        if (localeCountry is not null)
        {
            return new(true, localeCountry.Code);
        }

        try
        {
            var market = await operatingMarketProfileClient.GetCurrentAsync(cancellationToken);
            var marketCountry = CommunityDisplayCountryCatalog.Find(market?.CountryCode);
            if (marketCountry is not null)
            {
                return new(true, marketCountry.Code);
            }
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // 운영시장 조회 실패를 위치 추정이나 임의 기본값으로 숨기지 않습니다.
        }

        return new(false, null);
    }

    public async Task SaveAsync(
        CommunityCommentCountryPreference preference,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(preference);
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        await jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            cancellationToken,
            StorageKey,
            json);
    }

    private static CommunityCommentCountryPreference Normalize(
        CommunityCommentCountryPreference preference)
    {
        var country = preference.IsDisplayCountryPublic
            ? CommunityDisplayCountryCatalog.Find(preference.CountryCode)
            : null;
        return country is null
            ? new(false, null)
            : new(true, country.Code);
    }

    private static CommunityDisplayCountry? ResolveCurrentPublicLocaleCountry()
    {
        try
        {
            var region = new RegionInfo(CultureInfo.CurrentCulture.Name);
            return CommunityDisplayCountryCatalog.Find(region.TwoLetterISORegionName);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
