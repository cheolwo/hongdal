using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Driver.Transport;

namespace Ssalddel.Application.Driver.Transport;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Application,
    "현재 기사에게 배정된 운송만 도심 물류센터 Role Perspective로 변환한다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(RolePerspectiveResponse),
    FlowOrder = 20,
    Boundary = "주소, 연락처, 운임과 다른 기사의 운송을 projection하지 않고 상태전이 정책상 가능한 interaction만 반환한다.")]
public sealed class 도심물류센터운송자관점조회QueryHandler
    : IRequestHandler<도심물류센터운송자관점조회Query, RolePerspectiveResponse?>
{
    private readonly IRequestHandler<운송현재조회Query, 기사운송요약응답?> currentTransportReader;

    public 도심물류센터운송자관점조회QueryHandler(
        IRequestHandler<운송현재조회Query, 기사운송요약응답?> currentTransportReader)
    {
        this.currentTransportReader = currentTransportReader;
    }

    public async Task<RolePerspectiveResponse?> Handle(
        도심물류센터운송자관점조회Query request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.기사Id);

        var transport = await currentTransportReader.Handle(
            new 운송현재조회Query(request.기사Id),
            cancellationToken);
        if (transport is null)
        {
            return null;
        }

        var revision = transport.UpdatedAt.Ticks;
        var transportStableId = $"transport:{transport.Id}";
        var pickupStableId = $"transport-stop:{transport.Id}.pickup";
        var dropoffStableId = $"transport-stop:{transport.Id}.dropoff";

        return new RolePerspectiveResponse
        {
            StableId = $"role-perspective:urban-logistics-center.transport-{transport.Id}",
            Revision = revision,
            AuthorizedRoleCode = RolePerspectiveRoleCodes.Transporter,
            WorldZoneCode = RolePerspectiveWorldZoneCodes.UrbanLogisticsCenter,
            ViewerScopeCode = RolePerspectiveViewerScopeCodes.AuthorizedParty,
            SourceTypeCode = RolePerspectiveSourceTypeCodes.OperationalProjection,
            AuthorizationDecisionId = $"driver-transport-assignment:{transport.Id}.{revision}",
            GeneratedAt = DateTimeOffset.UtcNow,
            ObjectEmphases = BuildObjectEmphases(
                transport,
                transportStableId,
                pickupStableId,
                dropoffStableId),
            AllowedInteractions = BuildAllowedInteractions(
                transport,
                transportStableId,
                pickupStableId,
                dropoffStableId),
        };
    }

    private static IReadOnlyList<RoleObjectEmphasisResponse> BuildObjectEmphases(
        기사운송요약응답 transport,
        string transportStableId,
        string pickupStableId,
        string dropoffStableId)
    {
        var pickupIsNext = 기사운송상태전이Policy.가능한가(
            transport.상태,
            기사운송상태코드.상차지도착)
            || 기사운송상태전이Policy.가능한가(
                transport.상태,
                기사운송상태코드.상차완료);
        var dropoffIsNext = 기사운송상태전이Policy.가능한가(
            transport.상태,
            기사운송상태코드.하차지도착)
            || 기사운송상태전이Policy.가능한가(
                transport.상태,
                기사운송상태코드.인수완료);

        return
        [
            new RoleObjectEmphasisResponse
            {
                TargetStableId = transportStableId,
                EmphasisCode = RolePerspectiveEmphasisCodes.Primary,
                Label = $"내 운송 · {transport.상태}",
                DetailPanelCode = "current-transport-detail",
            },
            new RoleObjectEmphasisResponse
            {
                TargetStableId = pickupStableId,
                EmphasisCode = pickupIsNext
                    ? RolePerspectiveEmphasisCodes.Destination
                    : RolePerspectiveEmphasisCodes.Related,
                Label = "상차 위치",
                DetailPanelCode = "pickup-task-detail",
            },
            new RoleObjectEmphasisResponse
            {
                TargetStableId = dropoffStableId,
                EmphasisCode = dropoffIsNext
                    ? RolePerspectiveEmphasisCodes.Destination
                    : RolePerspectiveEmphasisCodes.Related,
                Label = "하차 위치",
                DetailPanelCode = "dropoff-task-detail",
            },
        ];
    }

    private static IReadOnlyList<RoleAllowedInteractionResponse> BuildAllowedInteractions(
        기사운송요약응답 transport,
        string transportStableId,
        string pickupStableId,
        string dropoffStableId)
    {
        var interactions = new List<RoleAllowedInteractionResponse>
        {
            new()
            {
                InteractionCode = "inspect-current-transport",
                TargetStableId = transportStableId,
                EffectCode = RolePerspectiveInteractionEffectCodes.ReadOnly,
            },
        };

        AddTransition(
            interactions,
            transport.상태,
            기사운송상태코드.상차지도착,
            "arrive-pickup",
            pickupStableId);
        AddTransition(
            interactions,
            transport.상태,
            기사운송상태코드.상차완료,
            "complete-pickup",
            pickupStableId);
        AddTransition(
            interactions,
            transport.상태,
            기사운송상태코드.하차지도착,
            "arrive-dropoff",
            dropoffStableId);
        AddTransition(
            interactions,
            transport.상태,
            기사운송상태코드.인수완료,
            "complete-transport",
            dropoffStableId);

        return interactions;
    }

    private static void AddTransition(
        ICollection<RoleAllowedInteractionResponse> interactions,
        string currentState,
        string targetState,
        string interactionCode,
        string targetStableId)
    {
        if (string.Equals(currentState, targetState, StringComparison.Ordinal)
            || !기사운송상태전이Policy.가능한가(currentState, targetState))
        {
            return;
        }

        interactions.Add(new RoleAllowedInteractionResponse
        {
            InteractionCode = interactionCode,
            TargetStableId = targetStableId,
            EffectCode = RolePerspectiveInteractionEffectCodes.ServerCommand,
            RequiresExplicitConfirmation = true,
            RequiresCanonicalStateRefresh = true,
        });
    }
}
