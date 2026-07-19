using System.Net.Http.Json;
using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Contracts.Common.Versioning;
using Microsoft.AspNetCore.Components.Forms;

namespace Ssalddel.Ui.Common.Areas.App.Services;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "공통 UI의 게시판·게시글·댓글·참여·원장 HTTP 요청을 서버 API에 연결",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.IndependentExecution,
    Boundary = "후속 버전 API client가 함께 있어도 0.0 화면은 CommunityTrustWorkflow와 사용자 직접 선택 경계를 따릅니다.")]
public partial class CommunityPlatformClient :
    ICommunityPostClient,
    ICommunityParticipationClient,
    ICommunityLedgerClient,
    ICommunityProcurementClient,
    ICommunityVoteClient
{
    private readonly HttpClient _httpClient;
    private readonly SsalddelProtectedApiClient _protectedApiClient;

    public CommunityPlatformClient(
        HttpClient httpClient,
        SsalddelProtectedApiClient protectedApiClient)
    {
        _httpClient = httpClient;
        _protectedApiClient = protectedApiClient;
    }

    private static void AddQueryValue(List<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private static async Task EnsureCommunityWriteSucceededAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var fallback = $"요청을 처리하지 못했습니다. HTTP {(int)response.StatusCode}";
        string? message = null;
        try
        {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(payload))
            {
                using var document = JsonDocument.Parse(payload);
                var root = document.RootElement;
                message = ReadProblemText(root, "detail")
                          ?? ReadProblemText(root, "message")
                          ?? ReadProblemText(root, "title");
            }
        }
        catch (JsonException)
        {
            // An invalid error payload still returns a stable HTTP fallback below.
        }

        throw new HttpRequestException(message ?? fallback, null, response.StatusCode);
    }

    private static string? ReadProblemText(JsonElement root, string propertyName)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
