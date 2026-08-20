using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public interface ISimulationAreaSetImmersionCatalogReader
    {
        bool TryRead(out SimulationAreaSetImmersionReadinessResponse readiness,
            out string errorCode);
    }

    public sealed class FileSimulationAreaSetImmersionCatalogReader :
        ISimulationAreaSetImmersionCatalogReader
    {
        private readonly string path;

        public FileSimulationAreaSetImmersionCatalogReader(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("AreaSet E6 몰입 판정 대장 경로가 필요합니다.", nameof(path));
            this.path = ResolvePath(path);
        }

        public bool TryRead(out SimulationAreaSetImmersionReadinessResponse readiness,
            out string errorCode)
        {
            readiness = new SimulationAreaSetImmersionReadinessResponse();
            if (!File.Exists(path))
            {
                errorCode = "AreaSetImmersionCatalogUnavailable";
                return false;
            }

            try
            {
                readiness = JsonSerializer.Deserialize<SimulationAreaSetImmersionReadinessResponse>(
                    File.ReadAllBytes(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("AreaSetImmersionCatalogInvalid");
                Validate(readiness);
                errorCode = string.Empty;
                return true;
            }
            catch (Exception error) when (error is IOException or JsonException or InvalidOperationException)
            {
                errorCode = error.Message;
                return false;
            }
        }

        private static void Validate(SimulationAreaSetImmersionReadinessResponse value)
        {
            Require(value.SchemaVersion == SimulationAreaSetImmersionCodes.SchemaVersion,
                "AreaSetImmersionSchemaMismatch");
            Require(!string.IsNullOrWhiteSpace(value.AreaSetStableId)
                    && value.SpatialMaturityCode == SimulationAreaSetImmersionCodes.SpatialE5Qualified,
                "AreaSetImmersionSpatialMaturityInvalid");
            Require(value.H3Audits.Length > 0
                    && value.H3Audits.Select(item => item.H3StableId).Distinct(StringComparer.Ordinal).Count()
                    == value.H3Audits.Length,
                "AreaSetImmersionH3AuditInvalid");
            Require(IsHash(value.AreaSetHashSha256) && IsHash(value.InputHashSha256)
                    && IsHash(value.QualificationHashSha256),
                "AreaSetImmersionHashInvalid");
            Require(!value.PublicDataChangesSimulationRules
                    && !value.PublicDataMovesSpatialDefinitions
                    && !value.RuntimeValidated,
                "AreaSetImmersionAuthorityBoundaryInvalid");
        }

        private static bool IsHash(string value) =>
            value.Length == 64 && value.All(Uri.IsHexDigit);

        private static void Require(bool condition, string errorCode)
        {
            if (!condition) throw new InvalidOperationException(errorCode);
        }

        private static string ResolvePath(string value)
        {
            if (Path.IsPathRooted(value)) return Path.GetFullPath(value);
            var direct = Path.GetFullPath(value);
            if (File.Exists(direct)) return direct;
            for (var current = new DirectoryInfo(AppContext.BaseDirectory);
                 current != null; current = current.Parent)
            {
                var candidate = Path.GetFullPath(Path.Combine(current.FullName, value));
                if (File.Exists(candidate)) return candidate;
            }
            return direct;
        }
    }

    public sealed class SimulationAreaSetImmersionService
    {
        private readonly ISimulationAreaSetImmersionCatalogReader reader;

        public SimulationAreaSetImmersionService(ISimulationAreaSetImmersionCatalogReader reader) =>
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));

        public Task<SimulationAreaSetImmersionReadinessResponse?> ReadAsync(
            string areaSetStableId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(reader.TryRead(out var value, out _)
                && string.Equals(value.AreaSetStableId, areaSetStableId, StringComparison.Ordinal)
                    ? value
                    : null);
        }

        public async Task<SimulationAreaSetImmersionReadinessResponse> RequireE7GateAsync(
            string areaSetStableId, CancellationToken cancellationToken = default)
        {
            var value = await ReadAsync(areaSetStableId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("AreaSetImmersionReadinessNotFound");
            if (value.ImmersionMaturityCode != SimulationAreaSetImmersionCodes.ImmersionQualified)
                throw new InvalidOperationException("AreaSetImmersionNotQualified");
            if (value.FreshnessStateCode != SimulationAreaSetImmersionCodes.Current)
                throw new InvalidOperationException("AreaSetImmersionStale");
            if (value.E7GateStateCode != SimulationAreaSetImmersionCodes.Open)
                throw new InvalidOperationException("AreaSetE7GateClosed");
            return value;
        }
    }
}
