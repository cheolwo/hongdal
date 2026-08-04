using MediatR;
using Ssalddel.Application.Community.Events;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Application.Community.Handlers;

public static class 커뮤니티세계지도원장Projection무효화ReasonCodes
{
    public const string LedgerSaved = "ledger-saved";
    public const string StateChanged = "state-changed";
    public const string ConsentWithdrawn = "consent-withdrawn";
    public const string Cancelled = "cancelled";
}

public sealed class 커뮤니티세계지도원장Projection무효화EventHandler(
    I커뮤니티세계지도원장ProjectionCache cache,
    ILogger<커뮤니티세계지도원장Projection무효화EventHandler> logger)
    : INotificationHandler<커뮤니티원장변경됨Event>
{
    public Task Handle(
        커뮤니티원장변경됨Event notification,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!notification.원장.외부참조.TryGetValue("MapMarkerId", out var markerId)
            || string.IsNullOrWhiteSpace(markerId)
            || string.IsNullOrWhiteSpace(notification.원장.원장템플릿Key))
        {
            return Task.CompletedTask;
        }

        var reasonCode = ResolveReasonCode(notification);
        cache.Invalidate(notification.원장.원장템플릿Key, markerId);
        logger.LogDebug(
            "지도 원장 projection cache를 무효화했습니다. EventId={EventId}, Template={Template}, Marker={Marker}, Reason={Reason}",
            notification.EventId,
            notification.원장.원장템플릿Key,
            markerId,
            reasonCode);
        return Task.CompletedTask;
    }

    public static string ResolveReasonCode(커뮤니티원장변경됨Event notification)
    {
        if (ReadBool(notification.원장.확장속성, "OperationalApplicationCancelled")
            || string.Equals(
                notification.원장.현재단계Key,
                지도신청가원장정책.운영신청취소단계,
                StringComparison.Ordinal))
        {
            return 커뮤니티세계지도원장Projection무효화ReasonCodes.Cancelled;
        }

        if (ReadBool(notification.원장.확장속성, 지도신청가원장정책.개인정보동의철회Key)
            || string.Equals(
                notification.원장.현재단계Key,
                지도신청가원장정책.동의철회확인단계,
                StringComparison.Ordinal))
        {
            return 커뮤니티세계지도원장Projection무효화ReasonCodes.ConsentWithdrawn;
        }

        return string.Equals(
            notification.변경유형,
            커뮤니티원장변경유형.상태변경,
            StringComparison.Ordinal)
            ? 커뮤니티세계지도원장Projection무효화ReasonCodes.StateChanged
            : 커뮤니티세계지도원장Projection무효화ReasonCodes.LedgerSaved;
    }

    private static bool ReadBool(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) && parsed;
}
