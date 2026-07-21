using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using Ssalddel.Contracts.Common.Localization;

namespace Ssalddel.WebApp.Services;

public sealed class WebLocaleBrowserSignals
{
    public string? CookieLanguageCode { get; set; }
    public string[] BrowserLanguageCodes { get; set; } = [];
}

public sealed class WebLocalePreferenceService(
    HttpClient httpClient,
    IJSRuntime jsRuntime,
    WebAuthSessionService authSession,
    NavigationManager navigation)
{
    private bool _initializing;

    public string LanguageCode { get; private set; } = DisplayLanguageCodes.Korean;
    public string? CountryCode { get; private set; }
    public bool IsInitialized { get; private set; }
    public bool IsEnglish => LanguageCode == DisplayLanguageCodes.English;
    public string? LastPersistenceError { get; private set; }
    public event Action? Changed;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initializing)
        {
            return;
        }

        _initializing = true;
        try
        {
            var relativePath = navigation.ToBaseRelativePath(navigation.Uri);
            var pathLanguage = WebLocalePolicy.LanguageFromPath(relativePath);
            if (pathLanguage is not null)
            {
                await ApplyDocumentLanguageAsync(pathLanguage, cancellationToken);
                SetCurrent(pathLanguage, CountryCode);
                return;
            }

            var browserSignals = await ReadBrowserSignalsAsync(cancellationToken);
            var recommendation = await ReadRecommendationAsync(cancellationToken);
            var languageCode = WebLocalePolicy.ResolveLanguage(
                authSession.PreferredLanguageCode,
                browserSignals.CookieLanguageCode,
                browserSignals.BrowserLanguageCodes,
                recommendation?.CountryCode,
                recommendation?.RecommendedLanguageCode);

            await ApplyDocumentLanguageAsync(languageCode, cancellationToken);
            SetCurrent(languageCode, recommendation?.CountryCode);
        }
        finally
        {
            _initializing = false;
        }
    }

    public async Task SelectAsync(
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var normalized = DisplayLanguageCodes.Normalize(languageCode);
        LastPersistenceError = null;
        await jsRuntime.InvokeVoidAsync(
            "ssalddelLocale.writePreference",
            cancellationToken,
            normalized);

        if (authSession.IsLoggedIn)
        {
            try
            {
                normalized = await authSession.SetPreferredLanguageAsync(normalized, cancellationToken);
            }
            catch (Exception exception)
            {
                LastPersistenceError = exception.Message;
            }
        }

        SetCurrent(normalized, CountryCode);
        var relativePath = navigation.ToBaseRelativePath(navigation.Uri);
        if (WebLocalePolicy.IsCommunityPath(relativePath))
        {
            navigation.NavigateTo(WebLocalePolicy.LocalizedCommunityHome(normalized));
        }
    }

    public string Text(string korean, string english)
        => IsEnglish ? english : korean;

    private async Task<WebLocaleBrowserSignals> ReadBrowserSignalsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await jsRuntime.InvokeAsync<WebLocaleBrowserSignals>(
                       "ssalddelLocale.readSignals",
                       cancellationToken)
                   ?? new WebLocaleBrowserSignals();
        }
        catch (JSException)
        {
            return new WebLocaleBrowserSignals();
        }
    }

    private async Task<PublicLocaleRecommendationResponse?> ReadRecommendationAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<PublicLocaleRecommendationResponse>(
                "api/v1/public/localization/recommendation",
                cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or NotSupportedException or JsonException)
        {
            return null;
        }
    }

    private async Task ApplyDocumentLanguageAsync(
        string languageCode,
        CancellationToken cancellationToken)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(
                "ssalddelLocale.applyDocumentLanguage",
                cancellationToken,
                languageCode);
        }
        catch (JSException)
        {
            // The route still remains the authoritative language signal when JS is unavailable.
        }
    }

    private void SetCurrent(string languageCode, string? countryCode)
    {
        LanguageCode = DisplayLanguageCodes.Normalize(languageCode);
        CountryCode = PublicCountryLanguageRecommendation.NormalizeCountryCode(countryCode);
        IsInitialized = true;
        Changed?.Invoke();
    }
}
