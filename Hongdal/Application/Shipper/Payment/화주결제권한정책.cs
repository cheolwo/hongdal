using 홍달.도메인.결제;
using 홍달.도메인.화주;

namespace Hongdal.Application.Shipper.Payment;

public static class 화주결제권한정책
{
    public static bool 의뢰결제권한있음(화주운송의뢰 request, string? userId, string? role)
        => 서버관리자인가(role)
           || string.Equals(request.주문자UserId, userId, StringComparison.Ordinal)
           || (string.IsNullOrWhiteSpace(request.주문자UserId)
               && string.Equals(request.화주Id, userId, StringComparison.Ordinal));

    public static bool 결제승인권한있음(결제 payment, 화주운송의뢰? request, string? userId, string? role)
    {
        if (서버관리자인가(role))
        {
            return true;
        }

        if (request is not null)
        {
            return 의뢰결제권한있음(request, userId, role);
        }

        return !string.IsNullOrWhiteSpace(userId)
               && string.Equals(payment.화주Id, userId, StringComparison.Ordinal);
    }

    private static bool 서버관리자인가(string? role)
        => string.Equals(role, 역할명.서버관리자, StringComparison.OrdinalIgnoreCase);
}
