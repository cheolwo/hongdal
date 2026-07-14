using Hongdal.Contracts.Common.Community;

namespace Hongdal.Services.Community;

public static class 주문원장구성정책
{
    private static readonly IReadOnlySet<string> 주문루트템플릿Keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        CommunityLedgerTemplateKeys.Order,
        CommunityLedgerTemplateKeys.FoodOrder,
        CommunityLedgerTemplateKeys.HongdalMart
    };

    public static bool 주문루트인가(string? 원장템플릿Key)
        => !string.IsNullOrWhiteSpace(원장템플릿Key)
           && 주문루트템플릿Keys.Contains(원장템플릿Key.Trim());

    public static bool 공동주문묶음인가(string? 원장템플릿Key)
        => string.Equals(
            원장템플릿Key?.Trim(),
            CommunityLedgerTemplateKeys.GroupPurchase,
            StringComparison.OrdinalIgnoreCase);

    public static bool 통합대상인가(string? 원장템플릿Key)
        => 주문루트인가(원장템플릿Key) || 공동주문묶음인가(원장템플릿Key);

    public static void 저장요청검증(커뮤니티원장저장요청 request)
    {
        if (request.포함원장목록 is null)
        {
            return;
        }

        if (request.포함원장목록.Count > 0 && !통합대상인가(request.원장템플릿Key))
        {
            throw new InvalidOperationException("하위 원장을 포함할 수 있는 원장은 주문 원장 또는 공동주문 묶음 원장이어야 합니다.");
        }

        var 중복원장Ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var 포함원장 in request.포함원장목록)
        {
            if (string.IsNullOrWhiteSpace(포함원장.원장Id))
            {
                throw new InvalidOperationException("포함 원장 ID는 필수입니다.");
            }

            if (!중복원장Ids.Add(포함원장.원장Id.Trim()))
            {
                throw new InvalidOperationException($"같은 하위 원장을 중복해서 포함할 수 없습니다. 원장Id={포함원장.원장Id.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(request.원장Id)
                && string.Equals(request.원장Id.Trim(), 포함원장.원장Id.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("주문 원장은 자기 자신을 하위 원장으로 포함할 수 없습니다.");
            }

            if (string.IsNullOrWhiteSpace(포함원장.원장템플릿Key))
            {
                throw new InvalidOperationException("포함 원장 템플릿 Key는 필수입니다.");
            }

            if (string.IsNullOrWhiteSpace(포함원장.역할) || !주문원장포함역할.All.Contains(포함원장.역할.Trim()))
            {
                throw new InvalidOperationException($"포함 원장 역할은 {string.Join(", ", 주문원장포함역할.All)} 중 하나여야 합니다.");
            }

            if (포함원장.표시순서 < 0)
            {
                throw new InvalidOperationException("포함 원장 표시순서는 0 이상이어야 합니다.");
            }

            구성역할검증(request.원장템플릿Key, 포함원장.원장템플릿Key, 포함원장.역할);
        }
    }

    public static void 연결검증(
        커뮤니티원장Dto 주문원장,
        커뮤니티원장Dto 하위원장,
        string? 역할)
    {
        if (!통합대상인가(주문원장.원장템플릿Key))
        {
            throw new InvalidOperationException("하위 원장을 연결할 대상은 주문 원장 또는 공동주문 묶음 원장이어야 합니다.");
        }

        if (string.Equals(주문원장.원장Id, 하위원장.원장Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("주문 원장은 자기 자신을 하위 원장으로 포함할 수 없습니다.");
        }

        if (!string.Equals(주문원장.커뮤니티Id, 하위원장.커뮤니티Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("같은 커뮤니티에 속한 원장만 주문 원장에 연결할 수 있습니다.");
        }

        if (string.IsNullOrWhiteSpace(역할) || !주문원장포함역할.All.Contains(역할.Trim()))
        {
            throw new InvalidOperationException($"포함 원장 역할은 {string.Join(", ", 주문원장포함역할.All)} 중 하나여야 합니다.");
        }

        구성역할검증(주문원장.원장템플릿Key, 하위원장.원장템플릿Key, 역할);
    }

    private static void 구성역할검증(string 기준원장템플릿Key, string 하위원장템플릿Key, string 역할)
    {
        if (공동주문묶음인가(기준원장템플릿Key))
        {
            if (!string.Equals(역할.Trim(), 주문원장포함역할.개별주문, StringComparison.OrdinalIgnoreCase)
                || !주문루트인가(하위원장템플릿Key))
            {
                throw new InvalidOperationException("공동주문 묶음에는 개별 주문 원장만 연결할 수 있습니다.");
            }

            return;
        }

        if (string.Equals(역할.Trim(), 주문원장포함역할.개별주문, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("개별주문 역할은 공동주문 묶음에서만 사용할 수 있습니다.");
        }
    }
}
