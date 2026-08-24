using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    internal static class SimulationBattleSaveReplay
    {
        public static SimulationSessionSavePackage AttachToPackage(
            SimulationSessionSavePackage package,
            SimulationBattleSaveRecordSnapshot[] battles)
        {
            if (battles == null)
                throw new ArgumentNullException(nameof(battles));
            return battles.Length == 0
                ? package
                : SimulationSaveReplayCloner.WithBattles(package, battles);
        }

        public static void ValidatePackage(SimulationSessionSavePackage package)
        {
            foreach (var battle in package.Battles)
            {
                SimulationBattleInstanceState.ValidateSaveRecord(battle);
                if (battle.State.SessionStableId != package.SessionStableId)
                    throw new SimulationConflictException(
                        "SimulationBattleSaveSessionIdentityMismatch");
            }
        }
    }

    public static partial class SimulationSaveReplayCloner
    {
        public static SimulationSessionSavePackage WithBattles(
            SimulationSessionSavePackage source,
            SimulationBattleSaveRecordSnapshot[] battles)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (battles == null) throw new ArgumentNullException(nameof(battles));
            var clone = ClonePackage(source);
            clone.Battles = battles.OrderBy(value => value.State.BattleStableId,
                    StringComparer.Ordinal)
                .Select(SimulationBattleInstanceState.CloneSaveRecord).ToArray();
            clone.ReplayHash = SimulationReplayHasher.Calculate(clone);
            return clone;
        }
    }
}
