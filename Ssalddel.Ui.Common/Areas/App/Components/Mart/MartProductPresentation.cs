using Ssalddel.Contracts.Mart;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Mart;

public static class MartProductPresentation
{
    public static string ProjectionTime(DateTime value)
        => $"{value:yyyy.MM.dd HH:mm} UTC";

    public static string EvidenceTime(DateTime? value)
        => value.HasValue ? $"{value.Value:yyyy.MM.dd HH:mm} UTC" : "기준 시각 없음";

    public static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    public static string ErrorMessage(string? value)
        => string.IsNullOrWhiteSpace(value) ? "서버 응답을 확인할 수 없습니다." : value;

    public static string CommunityPostHref(string basePath, long postId)
        => $"{basePath.TrimEnd('/')}/{postId}";

    public static string SalesPageHref(string basePath, 마트공개상품상세응답 detail)
        => new 판매페이지공개상품Seed(
                detail.Id,
                detail.상품명,
                detail.카테고리,
                detail.설명,
                detail.판매단위,
                detail.판매가,
                detail.구매근거.완료원장확인여부,
                detail.구매근거.공개후기수,
                detail.구매근거.근거기준시각Utc ?? detail.수정일시Utc)
            .ToNavigationUri(basePath);
}
