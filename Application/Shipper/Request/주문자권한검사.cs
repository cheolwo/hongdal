using Hongdal.Application.CommandProcessing;

namespace Hongdal.Application.Shipper.Request;

internal static class 주문자권한검사
{
    internal static bool IsServerAdmin(ICurrentUserAccessor currentUserAccessor)
    {
        return string.Equals(currentUserAccessor.Role, 역할명.서버관리자, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsOwner(홍달.도메인.화주.화주운송의뢰 entity, string? currentUserId)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return false;
        }

        return string.Equals(entity.주문자UserId, currentUserId, StringComparison.Ordinal)
               || (string.IsNullOrWhiteSpace(entity.주문자UserId)
                   && string.Equals(entity.화주Id, currentUserId, StringComparison.Ordinal));
    }
}
