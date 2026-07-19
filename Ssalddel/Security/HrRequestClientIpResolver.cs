using System.Net;

namespace Ssalddel.Security;

public static class HrRequestClientIpResolver
{
    public static IPAddress? Resolve(HttpContext context)
    {
        if (TryParseFirstHeaderIp(context.Request.Headers["X-Forwarded-For"].ToString(), out var forwardedIp))
        {
            return forwardedIp;
        }

        if (IPAddress.TryParse(context.Request.Headers["X-Real-IP"].ToString(), out var realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress;
    }

    private static bool TryParseFirstHeaderIp(string value, out IPAddress? ipAddress)
    {
        ipAddress = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var first = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(first) || !IPAddress.TryParse(first, out var parsed))
        {
            return false;
        }

        ipAddress = parsed;
        return true;
    }
}
