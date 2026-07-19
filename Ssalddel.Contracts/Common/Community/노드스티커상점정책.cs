namespace Ssalddel.Contracts.Common.Community;

public static class 노드스티커상점정책
{
    public static bool 상점노출가능한가(노드스티커상점상품Response 상품)
        => string.Equals(상품.검수상태, 노드스티커검수상태.승인, StringComparison.OrdinalIgnoreCase)
           && string.Equals(상품.판매상태, 노드스티커판매상태.판매중, StringComparison.OrdinalIgnoreCase)
           && 상품.이미지목록.Count > 0
           && 상품.이미지목록.All(노드스티커Catalog.표준준수여부);

    public static bool 구매필요한가(노드스티커상점상품Response 상품)
        => string.Equals(상품.거래정책.가격모드, 노드스티커가격모드.유료, StringComparison.OrdinalIgnoreCase);

    public static 노드스티커노드적용판정Response 노드적용판정(
        노드스티커이미지Response? 이미지,
        노드스티커상점상품Response? 상품,
        노드스티커보유권Response? 보유권)
    {
        if (이미지 is null)
        {
            return 판정(노드스티커노드적용판정Codes.이미지없음, "선택한 노드 스티커 이미지를 찾을 수 없습니다.");
        }

        if (!string.Equals(이미지.검수상태, 노드스티커검수상태.승인, StringComparison.OrdinalIgnoreCase))
        {
            return 판정(노드스티커노드적용판정Codes.검수미승인, "관리자 승인이 완료된 노드 스티커만 적용할 수 있습니다.");
        }

        if (상품 is not null && 구매필요한가(상품) && !보유권이이미지를포함하는가(보유권, 이미지))
        {
            return 판정(노드스티커노드적용판정Codes.구매필요, "유료 노드 스티커는 구매하거나 지급받은 뒤 적용할 수 있습니다.");
        }

        return new()
        {
            적용가능 = true,
            판정Code = 노드스티커노드적용판정Codes.적용가능,
            안내문구 = "노드에 적용할 수 있는 스티커입니다."
        };
    }

    private static bool 보유권이이미지를포함하는가(노드스티커보유권Response? 보유권, 노드스티커이미지Response 이미지)
        => 보유권 is not null
           && string.Equals(보유권.팩Key, 이미지.팩Key, StringComparison.OrdinalIgnoreCase)
           && (보유권.이미지Keys.Count == 0 ||
               보유권.이미지Keys.Contains(이미지.이미지Key, StringComparer.OrdinalIgnoreCase));

    private static 노드스티커노드적용판정Response 판정(string code, string message)
        => new()
        {
            적용가능 = false,
            판정Code = code,
            안내문구 = message
        };
}
