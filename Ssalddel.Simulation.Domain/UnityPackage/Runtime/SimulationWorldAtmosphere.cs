using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private SimulationAtmosphereInitialStateRequest? atmosphereCreationState;

        private bool HasWorldAtmosphere => atmosphereCreationState != null;

        private void InitializeWorldAtmosphere(
            SimulationAtmosphereInitialStateRequest? request)
        {
            ValidateWorldAtmosphereInitialState(request, natureSurvivalCreationState);
            atmosphereCreationState = CloneWorldAtmosphereInitialState(request);
        }

        private SimulationAtmosphereStateSnapshot CreateWorldAtmosphereStateSnapshot()
        {
            if (atmosphereCreationState == null)
                return new SimulationAtmosphereStateSnapshot();

            var projection = WorldAtmosphereRules.Evaluate(
                atmosphereCreationState.ProfileStableId,
                ScenarioSeed,
                natureCycleIndex,
                natureElapsedSecondsInCycle);
            return new SimulationAtmosphereStateSnapshot
            {
                IsEnabled = true,
                ProfileStableId = atmosphereCreationState.ProfileStableId,
                RuleRevision = atmosphereCreationState.RuleRevision,
                ClockSourceCode = atmosphereCreationState.ClockSourceCode,
                ScopeCode = WorldAtmosphereScopeCodes.World,
                WeatherCode = projection.WeatherCode,
                NextWeatherCode = projection.NextWeatherCode,
                TransitionProgressPermille = projection.TransitionProgressPermille,
                CloudCoverPermille = projection.CloudCoverPermille,
                PrecipitationPermille = projection.PrecipitationPermille,
                WindIntensityPermille = projection.WindIntensityPermille,
                LightningSequenceIndex = projection.LightningSequenceIndex,
                CycleIndex = natureCycleIndex,
                ElapsedSecondsInCycle = natureElapsedSecondsInCycle,
            };
        }

        internal static void ValidateWorldAtmosphereInitialState(
            SimulationAtmosphereInitialStateRequest? request,
            SimulationNatureSurvivalInitialStateRequest? natureSurvival)
        {
            if (request == null) return;
            if (!string.Equals(request.ProfileStableId,
                    WorldAtmosphereProfileCodes.NatureNightDay2FixtureR1,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationAtmosphereProfileInvalid");
            if (!string.Equals(request.RuleRevision,
                    WorldAtmosphereRuleRevisions.R1, StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationAtmosphereRuleRevisionInvalid");
            if (!string.Equals(request.ClockSourceCode,
                    WorldAtmosphereClockSourceCodes.NatureCycleClock,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationAtmosphereClockSourceInvalid");
            if (natureSurvival == null
                || !SimulationNatureSurvivalCodes.IsR5(
                    natureSurvival.ProfileRevision))
                throw new SimulationContractException(
                    "SimulationAtmosphereNatureR5Required");
        }

        internal static SimulationAtmosphereInitialStateRequest?
            CloneWorldAtmosphereInitialState(
                SimulationAtmosphereInitialStateRequest? source)
            => source == null ? null : new SimulationAtmosphereInitialStateRequest
            {
                ProfileStableId = source.ProfileStableId,
                RuleRevision = source.RuleRevision,
                ClockSourceCode = source.ClockSourceCode,
            };

        internal static SimulationAtmosphereStateSnapshot CloneWorldAtmosphereState(
            SimulationAtmosphereStateSnapshot source)
            => new SimulationAtmosphereStateSnapshot
            {
                IsEnabled = source.IsEnabled,
                ProfileStableId = source.ProfileStableId,
                RuleRevision = source.RuleRevision,
                ClockSourceCode = source.ClockSourceCode,
                ScopeCode = source.ScopeCode,
                WeatherCode = source.WeatherCode,
                NextWeatherCode = source.NextWeatherCode,
                TransitionProgressPermille = source.TransitionProgressPermille,
                CloudCoverPermille = source.CloudCoverPermille,
                PrecipitationPermille = source.PrecipitationPermille,
                WindIntensityPermille = source.WindIntensityPermille,
                LightningSequenceIndex = source.LightningSequenceIndex,
                CycleIndex = source.CycleIndex,
                ElapsedSecondsInCycle = source.ElapsedSecondsInCycle,
            };

        internal static string BuildWorldAtmosphereInitialPayloadKey(
            SimulationAtmosphereInitialStateRequest? value)
            => value == null ? string.Empty : string.Join("|", new[]
            {
                value.ProfileStableId?.Trim() ?? string.Empty,
                value.RuleRevision?.Trim() ?? string.Empty,
                value.ClockSourceCode?.Trim() ?? string.Empty,
            });
    }
}
