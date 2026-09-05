using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public Simulation플레이어이데아맵ProjectionSnapshot
            GetPlayerIdeaMapProjection(string playerStableId)
        {
            lock (gate)
            {
                var profile = GetPlayerDomainProfile(playerStableId);
                Simulation학습중점StateSnapshot? focus = null;
                if (learningFocusState != null)
                {
                    var candidate = learningFocusState.Snapshot();
                    if (candidate.PlayerStableId == playerStableId.Trim())
                        focus = candidate;
                }
                return Simulation플레이어이데아맵Projection.Build(
                    SessionStableId, playerStableId, Revision, CurrentTick,
                    GetActionManifestationLedger(), profile, focus);
            }
        }
    }
}
