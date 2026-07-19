using Ssalddel.Application.Shipper.Payment.Events;
using 살뜰.도메인.결제;
using 살뜰.Services.Settlement;

namespace Ssalddel.Application.Shipper.Payment.Handlers;

public sealed class 기사이용료결제승인완료EventHandler : INotificationHandler<결제승인완료Event>
{
    private readonly I기사월정산Service _settlementService;
    private readonly ILogger<기사이용료결제승인완료EventHandler> _logger;

    public 기사이용료결제승인완료EventHandler(
        I기사월정산Service settlementService,
        ILogger<기사이용료결제승인완료EventHandler> logger)
    {
        _settlementService = settlementService;
        _logger = logger;
    }

    public async Task Handle(결제승인완료Event notification, CancellationToken cancellationToken)
    {
        if (notification.결제대상유형 != 결제공통정의.결제대상유형.기사이용료)
        {
            return;
        }

        if (!TryParseTarget(notification.대상Id, out var 기사Id, out var 년도, out var 월))
        {
            _logger.LogWarning("기사이용료 결제승인 후처리 대상 파싱 실패: 대상Id={대상Id}", notification.대상Id);
            return;
        }

        await _settlementService.월말청구결제완료처리Async(기사Id, 년도, 월, notification.승인일시Utc, cancellationToken);

        _logger.LogInformation(
            "기사이용료 결제승인 후처리 완료: 결제Id={결제Id}, 기사Id={기사Id}, 년월={년도}-{월}",
            notification.결제Id,
            기사Id,
            년도,
            월);
    }

    private static bool TryParseTarget(string 대상Id, out string 기사Id, out int 년도, out int 월)
    {
        기사Id = string.Empty;
        년도 = DateTime.UtcNow.Year;
        월 = DateTime.UtcNow.Month;

        if (string.IsNullOrWhiteSpace(대상Id))
        {
            return false;
        }

        // 형식1: "driverId:yyyyMM"
        var parts = 대상Id.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
        {
            기사Id = parts[0];
            if (parts[1].Length == 6
                && int.TryParse(parts[1][..4], out 년도)
                && int.TryParse(parts[1][4..], out 월)
                && 월 is >= 1 and <= 12)
            {
                return !string.IsNullOrWhiteSpace(기사Id);
            }

            return false;
        }

        // 형식2: "driverId" (현재 년/월로 처리)
        기사Id = 대상Id.Trim();
        return !string.IsNullOrWhiteSpace(기사Id);
    }
}
