using Hongdal.Contracts.Common.Orderer;
using Hongdal.Services.Community;

namespace Hongdal.Services.Orderer;

internal sealed class CommunityVoteOrdererDemandHandoff : ICommunityGroupPurchaseDemandHandoff
{
    private readonly I공동구매자동집단화저장소 _store;

    public CommunityVoteOrdererDemandHandoff(I공동구매자동집단화저장소 store)
    {
        _store = store;
    }

    public async Task<string> SyncAsync(
        CommunityGroupPurchaseDemandHandoffRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _store.수요등록Async(new 공동구매자동수요등록Command
        {
            수요출처키 = $"community-vote:{request.VoteId:N}:{request.VoterHash}",
            커뮤니티게시글Id = request.SourcePostId,
            커뮤니티원장Id = request.CommunityLedgerId ?? string.Empty,
            상품키 = request.ProductKey,
            상품명 = request.ProductName,
            HS코드 = request.HsCode,
            온도코드 = request.TemperatureCode,
            물류방식 = request.LogisticsMode,
            주문자키 = request.VoterHash,
            주문자표시명 = request.VoterDisplayName,
            배송권키 = request.DeliveryScopeKey,
            배송권명 = request.DeliveryScopeName,
            희망수량 = request.RequestedQuantity,
            수량단위 = request.QuantityUnit,
            수요유형 = 공동구매자동수요유형코드.관심표시,
            결제상태 = 공동구매자동결제상태코드.미결제,
            메모 = "커뮤니티 공동구매 수요 투표에서 등록된 비구속 구매 의향입니다.",
            목표참여자수 = request.MinimumParticipantCount,
            목표수량 = request.MinimumTotalQuantity
        }, cancellationToken);

        return result.자동집단Id;
    }
}
