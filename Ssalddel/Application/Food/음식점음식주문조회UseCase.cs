using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;

namespace Ssalddel.Application.Food;

public interface I음식점음식주문조회UseCase
{
    음식주문목록응답 목록(long 음식점Id);

    음식주문응답? 상세(string 주문번호, long 음식점Id);
}

/// <summary>
/// 로그인한 음식점 운영자의 음식점 범위에서 서버 원장을 다시 읽습니다.
/// 실시간 알림 유실이나 앱 재시작 뒤에도 이 조회가 수신함의 기준이 됩니다.
/// </summary>
public sealed class 음식점음식주문조회UseCase(
    ISsalddelFoodOrderStore orderStore) : I음식점음식주문조회UseCase
{
    public 음식주문목록응답 목록(long 음식점Id)
    {
        if (음식점Id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(음식점Id));
        }

        return new 음식주문목록응답
        {
            Items = orderStore.GetOrders().Items
                .Where(order => order.음식점Id == 음식점Id)
                .OrderByDescending(order => order.CreatedAt)
                .ToArray()
        };
    }

    public 음식주문응답? 상세(string 주문번호, long 음식점Id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(주문번호);
        if (음식점Id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(음식점Id));
        }

        var order = orderStore.GetOrder(주문번호.Trim());
        return order?.음식점Id == 음식점Id ? order : null;
    }
}
