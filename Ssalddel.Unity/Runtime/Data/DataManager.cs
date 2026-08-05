using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ssalddel.Unity.Data
{
    public static class DataLoadStateCodes
    {
        public const string NotLoaded = "NotLoaded";
        public const string Loading = "Loading";
        public const string ReadyLive = "ReadyLive";
        public const string ReadyCached = "ReadyCached";
        public const string ReadyFixture = "ReadyFixture";
        public const string Invalid = "Invalid";
        public const string Failed = "Failed";
    }

    public static class ScenarioPackageSourceCodes
    {
        public const string Live = "Live";
        public const string Cached = "Cached";
        public const string Fixture = "Fixture";
    }

    public sealed class ScenarioPackageLoadResult
    {
        public 농업ScenarioPackage? Package { get; set; }

        public string SourceCode { get; set; } = string.Empty;

        public string ErrorCode { get; set; } = string.Empty;
    }

    public interface IScenarioPackageRepository
    {
        Task<ScenarioPackageLoadResult> LoadAsync(
            string scenarioKey,
            string scenarioVersion,
            CancellationToken cancellationToken);
    }

    public interface IDataStatusProvider
    {
        string StateCode { get; }

        string ErrorCode { get; }
    }

    public sealed class DataManager : IDataStatusProvider
    {
        private readonly IScenarioPackageRepository _scenarioRepository;
        private readonly 농업ScenarioValidator _validator;

        public DataManager(
            IScenarioPackageRepository scenarioRepository,
            농업ScenarioValidator? validator = null)
        {
            _scenarioRepository = scenarioRepository ?? throw new ArgumentNullException(nameof(scenarioRepository));
            _validator = validator ?? new 농업ScenarioValidator();
        }

        public string StateCode { get; private set; } = DataLoadStateCodes.NotLoaded;

        public string ErrorCode { get; private set; } = string.Empty;

        public 농업ScenarioPackage? CurrentScenario { get; private set; }

        public async Task<농업ScenarioPackage?> LoadScenarioAsync(
            string scenarioKey,
            string scenarioVersion,
            CancellationToken cancellationToken = default)
        {
            StableDataId.EnsureValid(scenarioKey, nameof(scenarioKey));
            if (string.IsNullOrWhiteSpace(scenarioVersion))
            {
                throw new ArgumentException("Scenario version이 필요합니다.", nameof(scenarioVersion));
            }

            StateCode = DataLoadStateCodes.Loading;
            ErrorCode = string.Empty;
            CurrentScenario = null;

            try
            {
                var loaded = await _scenarioRepository
                    .LoadAsync(scenarioKey, scenarioVersion, cancellationToken)
                    .ConfigureAwait(false);
                if (loaded == null || loaded.Package == null)
                {
                    StateCode = DataLoadStateCodes.Failed;
                    ErrorCode = loaded?.ErrorCode ?? "ScenarioPackageMissing";
                    return null;
                }

                if (!string.Equals(loaded.Package.Manifest.ScenarioKey, scenarioKey, StringComparison.Ordinal)
                    || !string.Equals(loaded.Package.Manifest.ScenarioVersion, scenarioVersion, StringComparison.Ordinal))
                {
                    StateCode = DataLoadStateCodes.Invalid;
                    ErrorCode = "ScenarioIdentityMismatch";
                    return null;
                }

                var validation = _validator.Validate(loaded.Package);
                if (!validation.IsValid)
                {
                    StateCode = DataLoadStateCodes.Invalid;
                    ErrorCode = validation.Issues[0].Code;
                    return null;
                }

                StateCode = ResolveReadyState(loaded.SourceCode);
                if (string.Equals(StateCode, DataLoadStateCodes.Invalid, StringComparison.Ordinal))
                {
                    ErrorCode = "ScenarioSourceUnsupported";
                    return null;
                }

                CurrentScenario = loaded.Package;
                return CurrentScenario;
            }
            catch (OperationCanceledException)
            {
                StateCode = DataLoadStateCodes.Failed;
                ErrorCode = "Cancelled";
                throw;
            }
            catch (Exception)
            {
                StateCode = DataLoadStateCodes.Failed;
                ErrorCode = "RepositoryFailure";
                return null;
            }
        }

        private static string ResolveReadyState(string sourceCode)
        {
            switch (sourceCode)
            {
                case ScenarioPackageSourceCodes.Live:
                    return DataLoadStateCodes.ReadyLive;
                case ScenarioPackageSourceCodes.Cached:
                    return DataLoadStateCodes.ReadyCached;
                case ScenarioPackageSourceCodes.Fixture:
                    return DataLoadStateCodes.ReadyFixture;
                default:
                    return DataLoadStateCodes.Invalid;
            }
        }
    }
}
