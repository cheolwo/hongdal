using Hongdal.Contracts.Common.Content;

namespace Hongdal.Ui.Common.Areas.App.Services;

public interface ICommunityInformationReviewClient
{
    Task<IReadOnlyList<CommunityInformationSourceDto>> GetSourcesAsync(
        CancellationToken cancellationToken = default);

    Task<CommunityInformationCollectionResponse> GetCandidatesAsync(
        CommunityInformationCollectionQuery query,
        CancellationToken cancellationToken = default);
}
