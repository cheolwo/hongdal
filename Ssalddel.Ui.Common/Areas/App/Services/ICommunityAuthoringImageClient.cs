using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Ui.Common.Areas.App.Services;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityAuthoringImage,
    SsalddelCodeLayer.Contract,
    "글쓰기 이미지 계획·생성·상태 조회·게시글 첨부를 제공하는 UI client port",
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.MayIncurExternalCost,
    Boundary = "Kie.ai API key와 provider DTO는 UI 경계 밖에 둡니다.")]
public interface ICommunityAuthoringImageClient
{
    Task<CommunityAuthoringImagePromptPlanResponse> PlanAuthoringImagePromptsAsync(
        CommunityAuthoringImagePromptPlanRequest request,
        CancellationToken cancellationToken = default);

    Task<CommunityAuthoringImageTaskResponse> GenerateAuthoringImageAsync(
        CommunityAuthoringImageGenerateRequest request,
        CancellationToken cancellationToken = default);

    Task<CommunityAuthoringImageTaskResponse?> GetAuthoringImageAsync(
        string jobCode,
        bool refreshProvider = true,
        CancellationToken cancellationToken = default);

    Task<PlatformCommunityPostAttachmentResponse> AttachAuthoringImageAsync(
        string jobCode,
        long postId,
        string password,
        CancellationToken cancellationToken = default);
}
