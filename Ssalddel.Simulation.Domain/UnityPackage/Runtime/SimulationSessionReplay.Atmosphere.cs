using System;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static partial class SimulationSessionReplay
    {
        private static void ValidateWorldAtmosphere(
            SimulationSessionSavePackage package)
        {
            var initial = package.SessionCreateRequest.Atmosphere;
            var natureInitial = package.SessionCreateRequest.NatureSurvival;
            var state = package.Snapshot.Atmosphere;
            경영SimulationSessionAggregate.ValidateWorldAtmosphereInitialState(
                initial, natureInitial);
            if (initial == null || state == null || !state.IsEnabled
                || !string.Equals(state.ProfileStableId,
                    initial.ProfileStableId, StringComparison.Ordinal)
                || !string.Equals(state.RuleRevision,
                    initial.RuleRevision, StringComparison.Ordinal)
                || !string.Equals(state.ClockSourceCode,
                    initial.ClockSourceCode, StringComparison.Ordinal)
                || !string.Equals(state.ScopeCode,
                    WorldAtmosphereScopeCodes.World, StringComparison.Ordinal)
                || state.CycleIndex != package.Snapshot.NatureSurvival.CycleIndex
                || state.ElapsedSecondsInCycle != package.Snapshot.NatureSurvival
                    .ElapsedSecondsInCycle)
                throw new SimulationContractException(
                    "SimulationAtmosphereStateInvalid");

            var expected = WorldAtmosphereRules.Evaluate(
                initial.ProfileStableId,
                package.SessionCreateRequest.ScenarioSeed,
                state.CycleIndex,
                state.ElapsedSecondsInCycle);
            if (!string.Equals(state.WeatherCode, expected.WeatherCode,
                    StringComparison.Ordinal)
                || !string.Equals(state.NextWeatherCode,
                    expected.NextWeatherCode, StringComparison.Ordinal)
                || state.TransitionProgressPermille
                    != expected.TransitionProgressPermille
                || state.CloudCoverPermille != expected.CloudCoverPermille
                || state.PrecipitationPermille
                    != expected.PrecipitationPermille
                || state.WindIntensityPermille
                    != expected.WindIntensityPermille
                || state.LightningSequenceIndex
                    != expected.LightningSequenceIndex
                || !WorldWeatherCodes.IsKnown(state.WeatherCode)
                || !WorldWeatherCodes.IsKnown(state.NextWeatherCode))
                throw new SimulationContractException(
                    "SimulationAtmosphereStateInvalid");
        }
    }
}
