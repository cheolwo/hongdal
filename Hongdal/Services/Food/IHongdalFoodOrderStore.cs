using Hongdal.Contracts.Food;

namespace Hongdal.Services.Food;

public interface IHongdalFoodOrderStore
{
    음식주문목록응답 GetOrders();

    음식주문응답? GetOrder(string orderNo);

    음식주문응답 AddOrder(음식주문등록요청 request);

    음식주문응답? 음식점수락(string orderNo, 음식점주문수락요청 request);

    음식주문응답? 배차대기반영(string orderNo, long dispatchWaitId, DateTime dispatchRequestedAtUtc);
}

public interface I커뮤니티원장반영가능음식주문Store
{
    음식주문응답? 커뮤니티원장반영(
        string orderNo,
        string ledgerId,
        string ledgerTemplateKey,
        string ledgerState,
        DateTime syncedAtUtc);
}
