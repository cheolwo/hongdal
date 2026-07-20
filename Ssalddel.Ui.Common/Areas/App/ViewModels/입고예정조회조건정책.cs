using Ssalddel.Contracts.Common.Inbound;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>일반 입고 목록 요청을 입고 예정 전용 조회 조건으로 변환합니다.</summary>
public static class 입고예정조회조건정책
{
    public static 목록조회요청 적용(목록조회요청 요청)
    {
        ArgumentNullException.ThrowIfNull(요청);

        var normalized = 요청.정규화();
        return normalized with
        {
            필터조건 = normalized.필터조건
                .Where(filter => !string.Equals(
                    filter.필드,
                    nameof(입고요청항목응답.상태),
                    StringComparison.OrdinalIgnoreCase))
                .Append(new 목록필터조건(
                    nameof(입고요청항목응답.상태),
                    "Equal",
                    입고상태코드.예정))
                .ToArray()
        };
    }
}
