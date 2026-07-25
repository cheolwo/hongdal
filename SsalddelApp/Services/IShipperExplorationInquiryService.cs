using Ssalddel.Contracts.Common.Exploration;

namespace SsalddelApp.Services;

public interface IShipperExplorationInquiryService
{
    Task<IReadOnlyList<탐색문의목록항목응답>> 목록조회Async(
        CancellationToken cancellationToken = default);

    Task<탐색문의상세응답?> 상세조회Async(
        long campaignId,
        CancellationToken cancellationToken = default);

    Task 응답Async(
        long campaignId,
        탐색문의응답요청 request,
        CancellationToken cancellationToken = default);
}
