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

        public SimulationNatureThreatObservationPreviewSnapshot PreviewThreatObservation(
            string sessionStableId,
            SimulationNatureThreatObservationPreviewRequest request)
            => sessions.Require(sessionStableId)
                .PreviewNatureThreatObservation(request);

        public 경영SimulationSessionSnapshot ConfirmThreatObservation(
            string sessionStableId,
            SimulationNatureThreatObservationConfirmRequest request)
            => sessions.Require(sessionStableId)
                .ConfirmNatureThreatObservation(request);

        public SimulationNatureEmergencyRetreatPreviewSnapshot PreviewEmergencyRetreat(
            string sessionStableId,
            SimulationNatureEmergencyRetreatPreviewRequest request)
            => sessions.Require(sessionStableId)
                .PreviewNatureEmergencyRetreat(request);

        public 경영SimulationSessionSnapshot ConfirmEmergencyRetreat(
            string sessionStableId,
            SimulationNatureEmergencyRetreatConfirmRequest request)
            => sessions.Require(sessionStableId)
                .ConfirmNatureEmergencyRetreat(request);

        public SimulationNatureRestorationPreviewSnapshot PreviewRestoration(
            string sessionStableId,
            SimulationNatureRestorationPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewNatureRestoration(request);

        public 경영SimulationSessionSnapshot ConfirmRestoration(
            string sessionStableId,
            SimulationNatureRestorationConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmNatureRestoration(request);

        public SimulationNaturePartyRecoveryPreviewSnapshot PreviewPartyRecovery(
            string sessionStableId,
            SimulationNaturePartyRecoveryPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewNaturePartyRecovery(request);

        public 경영SimulationSessionSnapshot ConfirmPartyRecovery(
            string sessionStableId,
            SimulationNaturePartyRecoveryConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmNaturePartyRecovery(request);
    }
}
