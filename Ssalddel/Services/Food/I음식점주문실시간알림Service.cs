using Ssalddel.Contracts.Food;

namespace Ssalddel.Services.Food;

public interface I음식점주문실시간알림Service
{
    Task 신규주문알림발송Async(음식주문응답 order, CancellationToken cancellationToken = default);
}
