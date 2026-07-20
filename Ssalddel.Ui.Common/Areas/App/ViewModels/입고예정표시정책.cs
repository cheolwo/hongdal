using Ssalddel.Contracts.Common.Inbound;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

/// <summary>입고 예정 응답의 빈 값을 현장 화면용 문구로 표현합니다.</summary>
public static class 입고예정표시정책
{
    public static string 공급처명(입고요청항목응답 item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return string.IsNullOrWhiteSpace(item.공급처명) ? "업체명 미등록" : item.공급처명;
    }

    public static string 공급처코드(입고요청항목응답 item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return string.IsNullOrWhiteSpace(item.공급처코드) ? "코드 미등록" : item.공급처코드;
    }

    public static string 상품명(입고요청항목응답 item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return string.IsNullOrWhiteSpace(item.예정상품명) ? "품목 정보 없음" : item.예정상품명;
    }

    public static string 상품상세(입고요청항목응답 item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var values = new List<string>(2);
        if (!string.IsNullOrWhiteSpace(item.예정SKU))
        {
            values.Add(item.예정SKU);
        }

        if (item.예정수량.HasValue)
        {
            values.Add($"{item.예정수량.Value:N0}개");
        }

        return string.Join(" · ", values);
    }

    public static string 참조번호(입고요청항목응답 item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return !string.IsNullOrWhiteSpace(item.주문참조번호)
            ? item.주문참조번호
            : !string.IsNullOrWhiteSpace(item.원주문참조번호)
                ? item.원주문참조번호
                : "참조번호 없음";
    }
}
