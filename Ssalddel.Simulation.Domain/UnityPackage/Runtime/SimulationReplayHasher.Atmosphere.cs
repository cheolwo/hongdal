using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    internal static partial class SimulationReplayHasher
    {
        private static void AddWorldAtmosphereInitialState(StringBuilder target,
            SimulationAtmosphereInitialStateRequest value)
        {
            Add(target, value.ProfileStableId);
            Add(target, value.RuleRevision);
            Add(target, value.ClockSourceCode);
        }

        private static void AddWorldAtmosphereState(StringBuilder target,
            SimulationAtmosphereStateSnapshot value)
        {
            Add(target, value.IsEnabled);
            Add(target, value.ProfileStableId);
            Add(target, value.RuleRevision);
            Add(target, value.ClockSourceCode);
            Add(target, value.ScopeCode);
            Add(target, value.WeatherCode);
            Add(target, value.NextWeatherCode);
            Add(target, value.TransitionProgressPermille);
            Add(target, value.CloudCoverPermille);
            Add(target, value.PrecipitationPermille);
            Add(target, value.WindIntensityPermille);
            Add(target, value.LightningSequenceIndex);
            Add(target, value.CycleIndex);
            Add(target, value.ElapsedSecondsInCycle);
        }
    }
}
