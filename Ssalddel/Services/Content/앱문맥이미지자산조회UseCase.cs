using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Content;
using 살뜰.Data;

namespace Ssalddel.Services.Content;

public interface I앱문맥이미지자산조회UseCase
{
    Task<AppContextImageAssetListResponse> 팩조회Async(
        string appPackId,
        CancellationToken cancellationToken = default);
}

public sealed class 앱문맥이미지자산조회UseCase(
    SsalddelContext db) : I앱문맥이미지자산조회UseCase
{
    public async Task<AppContextImageAssetListResponse> 팩조회Async(
        string appPackId,
        CancellationToken cancellationToken = default)
    {
        var normalizedPackId = NormalizePackId(appPackId);
        var entities = await db.앱문맥이미지자산들
            .AsNoTracking()
            .Where(item => item.앱PackId == normalizedPackId && item.활성화여부)
            .OrderBy(item => item.장면번호)
            .ToListAsync(cancellationToken);
        var items = entities.Select(item => new AppContextImageAssetDto(
                item.장면Key,
                item.앱PackId,
                item.장면번호,
                item.제목,
                item.대체Text,
                item.이미지Url,
                item.화면비율,
                item.품질상태.ToString(),
                ParseRouteRefs(item.RouteRefsJson)))
            .ToArray();
        return new AppContextImageAssetListResponse(
            normalizedPackId,
            items.Length,
            items);
    }

    private static string NormalizePackId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 80
            || normalized.Any(character =>
                !char.IsAsciiLetterOrDigit(character)
                && character != '-'))
        {
            throw new ArgumentException("앱 이미지 packId 형식이 올바르지 않습니다.");
        }

        return normalized;
    }

    private static IReadOnlyList<string> ParseRouteRefs(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
