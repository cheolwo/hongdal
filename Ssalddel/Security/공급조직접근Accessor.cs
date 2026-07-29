using System.Security.Claims;
using Ssalddel.Application.ContractManagement;
using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Food;

namespace Ssalddel.Security;

public sealed class 공급조직접근Accessor(IHttpContextAccessor httpContextAccessor)
    : I공급조직접근Accessor
{
    public string? 조직참조Key조회(string organizationTypeCode)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            return null;
        }

        if (string.Equals(
                organizationTypeCode,
                공급이용조직유형코드.음식점,
                StringComparison.Ordinal))
        {
            var restaurantId = user.FindFirstValue(음식점접근ClaimTypes.음식점Id);
            return long.TryParse(restaurantId, out var value) && value > 0
                ? value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : null;
        }

        if (string.Equals(
                organizationTypeCode,
                공급이용조직유형코드.살들마트,
                StringComparison.Ordinal))
        {
            var martId = user.FindFirstValue(공급조직접근ClaimTypes.살들마트Id)?.Trim();
            return string.IsNullOrWhiteSpace(martId) || martId.Length > 160
                ? null
                : martId;
        }

        return null;
    }
}
