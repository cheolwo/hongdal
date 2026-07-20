using System.Globalization;
using Ssalddel.Contracts.Food;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I주문자음식주문읽기Service
{
    Task<주문자음식주문목록응답> 목록Async(
        주문자음식주문목록조회요청 request,
        CancellationToken cancellationToken = default);

    Task<주문자음식주문상세응답?> 상세Async(
        string orderNo,
        CancellationToken cancellationToken = default);
}

/// <summary>로그인 주문자의 음식 주문 목록과 정확한 주문번호 상세만 보호 API에서 읽습니다.</summary>
public sealed class 주문자음식주문Client(
    ISsalddelJsonApiClient client) : I주문자음식주문읽기Service
{
    private const string BasePath = "api/v1/food-orders";

    public async Task<주문자음식주문목록응답> 목록Async(
        주문자음식주문목록조회요청 request,
        CancellationToken cancellationToken = default)
        => await client.GetAsync<주문자음식주문목록응답>(
               BuildListPath(request),
               "내 음식 주문 목록 조회",
               allowNotFound: false,
               cancellationToken)
           ?? throw new InvalidOperationException("내 음식 주문 목록 응답이 비어 있습니다.");

    public Task<주문자음식주문상세응답?> 상세Async(
        string orderNo,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNo);
        return client.GetAsync<주문자음식주문상세응답>(
            $"{BasePath}/{Uri.EscapeDataString(orderNo.Trim())}",
            "내 음식 주문 상세 조회",
            allowNotFound: true,
            cancellationToken);
    }

    private static string BuildListPath(주문자음식주문목록조회요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = new List<string>
        {
            $"page={Math.Max(1, request.Page).ToString(CultureInfo.InvariantCulture)}",
            $"pageSize={Math.Clamp(request.PageSize, 1, 50).ToString(CultureInfo.InvariantCulture)}"
        };
        AddValue(query, "검색어", request.검색어);
        AddValue(query, "상태", request.상태);
        return $"{BasePath}?{string.Join('&', query)}";
    }

    private static void AddValue(ICollection<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
