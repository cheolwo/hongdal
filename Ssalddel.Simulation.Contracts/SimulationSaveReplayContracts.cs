using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationSaveSchemaVersions
    {
        public const string V1 = "simulation-save.v1";
    }

    public static class SimulationReplayHashAlgorithmCodes
    {
        public const string Sha256 = "SHA-256";
    }

    public static class SimulationCommandTypeCodes
    {
        public const string DecisionConfirm = "DecisionConfirm";
        public const string HarvestDispositionImpactConfirm = "HarvestDispositionImpactConfirm";
        public const string LogisticsMovementConfirm = "LogisticsMovementConfirm";
        public const string TickAdvance = "TickAdvance";
    }

    public sealed class SimulationSessionSaveRequest
    {
        public string SaveStableId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
    }

    public sealed class SimulationSessionRestoreRequest
    {
        public string SaveStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationCommandLogEntrySnapshot
    {
        public long Sequence { get; set; }
        public string CommandTypeCode { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
        public long ResultingWorldRevision { get; set; }
        public 경영SimulationTick진행Request? TickRequest { get; set; }
        public SimulationDecisionConfirmRequest? DecisionConfirmRequest { get; set; }
        public SimulationHarvestDispositionImpactConfirmRequest? HarvestDispositionImpactConfirmRequest { get; set; }
        public SimulationLogisticsMovementConfirmRequest? LogisticsMovementConfirmRequest { get; set; }
    }

    public sealed class SimulationSessionSavePackage
    {
        public string SchemaVersion { get; set; } = SimulationSaveSchemaVersions.V1;
        public string SaveStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public int SavedWorldTick { get; set; }
        public long SavedWorldRevision { get; set; }
        public string ReplayHashAlgorithmCode { get; set; }
            = SimulationReplayHashAlgorithmCodes.Sha256;
        public string ReplayHash { get; set; } = string.Empty;
        public 경영SimulationSession생성Request SessionCreateRequest { get; set; }
            = new 경영SimulationSession생성Request();
        public 경영SimulationSessionSnapshot Snapshot { get; set; }
            = new 경영SimulationSessionSnapshot();
        public SimulationCommandLogEntrySnapshot[] CommandLog { get; set; }
            = Array.Empty<SimulationCommandLogEntrySnapshot>();
    }

    public sealed class SimulationSessionRestoreResult
    {
        public string SaveStableId { get; set; } = string.Empty;
        public string SchemaVersion { get; set; } = string.Empty;
        public string ReplayHash { get; set; } = string.Empty;
        public int ReplayedCommandCount { get; set; }
        public 경영SimulationSessionSnapshot Session { get; set; }
            = new 경영SimulationSessionSnapshot();
    }
}
