using Hongdal.Contracts.Driver.Work;
using Hongdal.Contracts.Common.Community;

namespace DriverApp.Services;

public interface IDriverWorkApiService
{
    Task<기사운행시작응답?> 운행시작Async(기사운행시작요청 request, CancellationToken cancellationToken = default);
    Task 운행종료Async(CancellationToken cancellationToken = default);
    Task<기사위치갱신응답?> 위치갱신Async(기사위치갱신요청 request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommunityDriverInquiryResponse>> 커뮤니티의뢰목록Async(CancellationToken cancellationToken = default);
    Task<CommunityDriverInquiryResponse?> 커뮤니티의뢰답변Async(Guid inquiryId, CommunityDriverInquiryDecisionRequest request, CancellationToken cancellationToken = default);
}
