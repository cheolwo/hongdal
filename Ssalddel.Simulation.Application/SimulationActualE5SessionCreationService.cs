using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 위치 독립 실제 E5 경관과 H5 배치를 검증한 뒤 세션 공간 상태를 생성한다.
    /// 클라이언트가 임의 공간 정의를 주입하거나 E5 실패를 Scenario로 대체할 수 없다.
    /// </summary>
    public sealed class SimulationActualE5SessionCreationService
    {
        private readonly SimulationWorld상호작용NetworkService interactionNetwork;
        private readonly SimulationWorldLayoutService worldLayouts;
        private readonly 경영SimulationSession생명주기Service sessions;
        private readonly SimulationRealityContextService realityContexts;

        public SimulationActualE5SessionCreationService(
            SimulationWorld상호작용NetworkService interactionNetwork,
            SimulationWorldLayoutService worldLayouts,
            경영SimulationSession생명주기Service sessions,
            SimulationRealityContextService realityContexts)
        {
            this.interactionNetwork = interactionNetwork
                ?? throw new ArgumentNullException(nameof(interactionNetwork));
            this.worldLayouts = worldLayouts
                ?? throw new ArgumentNullException(nameof(worldLayouts));
            this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            this.realityContexts = realityContexts
                ?? throw new ArgumentNullException(nameof(realityContexts));
        }

        public async Task<SimulationActualE5SessionCreateResponse> CreateAsync(
            SimulationActualE5SessionCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Session == null) throw new ArgumentNullException(nameof(request.Session));
            if (request.Session.SpatialWorld != null)
                throw new InvalidOperationException("SimulationActualE5ClientSpatialWorldForbidden");
            if (string.IsNullOrWhiteSpace(request.AreaSetNetworkStableId)
                || string.IsNullOrWhiteSpace(request.AreaSetStableId)
                || string.IsNullOrWhiteSpace(request.WorldLayoutStableId))
                throw new InvalidOperationException("SimulationActualE5SelectionInvalid");

            var layout = await worldLayouts.ReadDefinitionAsync(
                request.WorldLayoutStableId.Trim(), cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("SimulationWorldLayoutNotFound");
            if (layout.WorldLayoutRevision != request.ExpectedWorldLayoutRevision
                || !string.Equals(layout.WorldLayoutHashSha256,
                    request.ExpectedWorldLayoutHashSha256, StringComparison.Ordinal))
                throw new InvalidOperationException("SimulationWorldLayoutRevisionMismatch");
            if (!string.Equals(layout.AreaSetNetworkStableId,
                    request.AreaSetNetworkStableId, StringComparison.Ordinal)
                || !layout.AreaSetInstances.Any(item =>
                    string.Equals(item.AreaSetInstanceStableId,
                        request.AreaSetStableId, StringComparison.Ordinal)))
                throw new InvalidOperationException("SimulationActualE5AreaSetNotInWorldLayout");

            var wiIds = request.WorldInteractionIds
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToArray();
            request.Session.SpatialWorld = await interactionNetwork.ResolveSpatialWorldAsync(
                request.AreaSetNetworkStableId,
                request.AreaSetStableId,
                wiIds,
                cancellationToken).ConfigureAwait(false);
            SimulationRealityContextSnapshot? realityContext = null;
            if (!string.IsNullOrWhiteSpace(request.Session.RealityContextProfileStableId))
            {
                realityContext = realityContexts.FreezeForSession(
                    request.Session.RealityContextProfileStableId,
                    request.AreaSetStableId,
                    request.Session.ClientRequestId);
            }
            var snapshot = sessions.Create(request.Session, realityContext);

            return new SimulationActualE5SessionCreateResponse
            {
                AreaSetNetworkStableId = request.AreaSetNetworkStableId,
                AreaSetStableId = request.AreaSetStableId,
                WorldLayoutStableId = layout.WorldLayoutStableId,
                WorldLayoutRevision = layout.WorldLayoutRevision,
                WorldLayoutHashSha256 = layout.WorldLayoutHashSha256,
                WorldInteractionIds = wiIds,
                RealityContextSnapshotStableId = realityContext?.ContextSnapshotStableId
                    ?? string.Empty,
                RealityContextAvailabilityCode = realityContext?.AvailabilityCode
                    ?? SimulationRealityContextCodes.Unavailable,
                Session = snapshot,
            };
        }
    }
}
