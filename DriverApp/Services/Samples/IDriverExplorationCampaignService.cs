using Ssalddel.Contracts.Common.Exploration;

namespace DriverApp.Services.Samples;

public interface IDriverExplorationCampaignService
{
    IReadOnlyList<탐색캠페인목록항목응답> 캠페인목록();

    탐색캠페인상세응답 상세(long id);

    IReadOnlyList<탐색캠페인추천대상응답> 추천대상(long campaignId);
}
