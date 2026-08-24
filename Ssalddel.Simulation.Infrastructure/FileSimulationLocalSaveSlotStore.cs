using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Infrastructure
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
        "결정성·Save/Replay 또는 회귀 검증 책임을 제공한다.",
        Boundary = "저장 구현 존재만으로 상위 E 증거를 승격하지 않는다.")]
    public sealed class FileSimulationLocalSaveSlotStore : ISimulationLocalSaveSlotStore
    {
        private const string FileFormatVersion = "ssalddel-local-save.v1";
        private readonly string rootPath;
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            WriteIndented = false,
        };

        public FileSimulationLocalSaveSlotStore(string savesRootPath)
        {
            if (string.IsNullOrWhiteSpace(savesRootPath))
                throw new ArgumentException("SimulationLocalSaveRootInvalid",
                    nameof(savesRootPath));
            rootPath = Path.GetFullPath(savesRootPath.Trim());
            Directory.CreateDirectory(rootPath);
        }

        public void Write(string slotStableId, SimulationSessionSavePackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            var slot = NormalizeSlot(slotStableId);
            SimulationSessionReplay.Restore(package);

            var packageJson = JsonSerializer.Serialize(package, jsonOptions);
            var envelope = new LocalSaveEnvelope
            {
                FileFormatVersion = FileFormatVersion,
                SlotStableId = slot,
                WrittenAtUtc = DateTimeOffset.UtcNow,
                PackageSha256 = Hash(packageJson),
                Package = package,
            };
            var json = JsonSerializer.Serialize(envelope, jsonOptions);
            var path = ResolvePath(slot);
            var temporaryPath = path + ".tmp";
            var backupPath = path + ".bak";

            try
            {
                using (var stream = new FileStream(temporaryPath, FileMode.Create,
                           FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush();
                }

                ReadEnvelope(temporaryPath, slot);
                if (File.Exists(path))
                {
                    File.Replace(temporaryPath, path, backupPath, true);
                }
                else
                {
                    File.Move(temporaryPath, path);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        public SimulationLocalSaveSlotPackage Read(string slotStableId)
        {
            var slot = NormalizeSlot(slotStableId);
            var path = ResolvePath(slot);
            var backupPath = path + ".bak";
            try
            {
                var envelope = ReadEnvelope(path, slot);
                return Result(envelope, false);
            }
            catch (Exception primaryError) when (CanRecover(primaryError)
                                                 && File.Exists(backupPath))
            {
                try
                {
                    var envelope = ReadEnvelope(backupPath, slot);
                    return Result(envelope, true);
                }
                catch (Exception backupError) when (CanRecover(backupError))
                {
                    throw new SimulationContractException(
                        "SimulationLocalSaveCorrupted");
                }
            }
            catch (FileNotFoundException)
            {
                throw new SimulationNotFoundException("SimulationLocalSaveSlotNotFound");
            }
            catch (DirectoryNotFoundException)
            {
                throw new SimulationNotFoundException("SimulationLocalSaveSlotNotFound");
            }
            catch (Exception error) when (CanRecover(error))
            {
                throw new SimulationContractException("SimulationLocalSaveCorrupted");
            }
        }

        private LocalSaveEnvelope ReadEnvelope(string path, string expectedSlot)
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            var envelope = JsonSerializer.Deserialize<LocalSaveEnvelope>(json, jsonOptions)
                ?? throw new SimulationContractException("SimulationLocalSaveCorrupted");
            if (!string.Equals(envelope.FileFormatVersion, FileFormatVersion,
                    StringComparison.Ordinal)
                || !string.Equals(envelope.SlotStableId, expectedSlot,
                    StringComparison.Ordinal)
                || envelope.Package == null)
                throw new SimulationContractException("SimulationLocalSaveCorrupted");

            var packageJson = JsonSerializer.Serialize(envelope.Package, jsonOptions);
            if (!string.Equals(envelope.PackageSha256, Hash(packageJson),
                    StringComparison.Ordinal))
                throw new SimulationContractException("SimulationLocalSaveChecksumMismatch");
            SimulationSessionReplay.Restore(envelope.Package);
            return envelope;
        }

        private string ResolvePath(string slot)
        {
            var path = Path.GetFullPath(Path.Combine(rootPath, slot + ".ssalddel"));
            var rootWithSeparator = rootPath.TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!path.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                throw new SimulationContractException("SimulationLocalSaveSlotInvalid");
            return path;
        }

        private static string NormalizeSlot(string slotStableId)
        {
            if (string.IsNullOrWhiteSpace(slotStableId))
                throw new SimulationContractException("SimulationLocalSaveSlotInvalid");
            var slot = slotStableId.Trim();
            if (slot.Length > 64 || slot.Any(character =>
                    !char.IsLetterOrDigit(character) && character != '-'
                    && character != '_'))
                throw new SimulationContractException("SimulationLocalSaveSlotInvalid");
            return slot;
        }

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(bytes).Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static bool CanRecover(Exception error)
            => error is IOException
               || error is UnauthorizedAccessException
               || error is JsonException
               || error is SimulationContractException;

        private static SimulationLocalSaveSlotPackage Result(
            LocalSaveEnvelope envelope, bool recoveredFromBackup)
            => new SimulationLocalSaveSlotPackage
            {
                SlotStableId = envelope.SlotStableId,
                RecoveredFromBackup = recoveredFromBackup,
                Package = envelope.Package,
            };

        private sealed class LocalSaveEnvelope
        {
            public string FileFormatVersion { get; set; } = string.Empty;
            public string SlotStableId { get; set; } = string.Empty;
            public DateTimeOffset WrittenAtUtc { get; set; }
            public string PackageSha256 { get; set; } = string.Empty;
            public SimulationSessionSavePackage Package { get; set; }
                = new SimulationSessionSavePackage();
        }
    }
}
