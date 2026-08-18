using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public sealed class SimulationRegionalIncidentService
    {
        private readonly 경영SimulationSessionAccessor sessions;

        public SimulationRegionalIncidentService(경영SimulationSessionAccessor sessions)
            => this.sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));

        public SimulationRegionalIncidentResponsePreviewSnapshot Preview(
            string sessionStableId, string eventStableId,
            SimulationRegionalIncidentResponsePreviewRequest request)
            => sessions.Require(sessionStableId)
                .PreviewRegionalIncidentResponse(eventStableId, request);

        public 경영SimulationSessionSnapshot Confirm(
            string sessionStableId, string eventStableId,
            SimulationRegionalIncidentResponseConfirmRequest request)
            => sessions.Require(sessionStableId)
                .ConfirmRegionalIncidentResponse(eventStableId, request);
    }
}
