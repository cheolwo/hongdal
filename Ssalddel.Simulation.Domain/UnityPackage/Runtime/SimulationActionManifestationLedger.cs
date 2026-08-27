using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// 권위 행위 결과를 보존하는 중립 원장이다. 표현 엔진을 호출하거나
    /// 엔진별 파생 상태를 소유하지 않는다.
    /// </summary>
    public sealed class Simulation행위발현Ledger : ISimulation행위기록Reader
    {
        private readonly object gate = new object();
        private readonly string worldStableId;
        private readonly List<Simulation행위발현Record> tailRecords =
            new List<Simulation행위발현Record>();
        private Simulation행위기록CheckpointSnapshot checkpoint =
            new Simulation행위기록CheckpointSnapshot();

        public Simulation행위발현Ledger(string worldStableId)
        {
            this.worldStableId = Require(worldStableId,
                "SimulationActionManifestationWorldStableIdInvalid");
        }

        private Simulation행위발현Ledger(Simulation행위기록LedgerSnapshot snapshot)
        {
            if (snapshot == null)
                throw new SimulationContractException(
                    "SimulationActionManifestationSnapshotRequired");
            worldStableId = Require(snapshot.WorldStableId,
                "SimulationActionManifestationWorldStableIdInvalid");
            if (!string.Equals(snapshot.SchemaCode,
                    Simulation행위기록SchemaCodes.원장상태,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationActionManifestationSchemaInvalid");
            checkpoint = Clone(snapshot.Checkpoint);
            tailRecords.AddRange((snapshot.TailRecords ??
                Array.Empty<Simulation행위발현Record>()).Select(Clone));
            ValidateChain();
            var calculated = CalculateStateHash(CreateSnapshotCore());
            if (!string.Equals(calculated, snapshot.StateHashSha256,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationActionManifestationStateHashMismatch");
        }

        public static Simulation행위발현Ledger Restore(
            Simulation행위기록LedgerSnapshot snapshot)
            => new Simulation행위발현Ledger(snapshot);

        public Simulation행위발현Record Append(Simulation행위발현Record draft)
        {
            if (draft == null)
                throw new SimulationContractException(
                    "SimulationActionManifestationRecordRequired");
            lock (gate)
            {
                var normalized = Normalize(draft);
                var duplicate = tailRecords.FirstOrDefault(value =>
                    string.Equals(value.행위기록StableId,
                        normalized.행위기록StableId, StringComparison.Ordinal));
                if (duplicate != null)
                {
                    var candidate = Clone(normalized);
                    candidate.이전기록HashSha256 = duplicate.이전기록HashSha256;
                    candidate.기록HashSha256 = CalculateRecordHash(candidate);
                    if (!string.Equals(candidate.기록HashSha256,
                            duplicate.기록HashSha256, StringComparison.Ordinal))
                        throw new SimulationContractException(
                            "SimulationActionManifestationDuplicateConflict");
                    return Clone(duplicate);
                }

                if (normalized.AfterWorldRevision <=
                    checkpoint.ConsolidatedThroughWorldRevision)
                    throw new SimulationContractException(
                        "SimulationActionManifestationBeforeCheckpoint");
                var last = tailRecords.LastOrDefault();
                if (last != null && Compare(last, normalized) >= 0)
                    throw new SimulationContractException(
                        "SimulationActionManifestationOrderInvalid");
                normalized.이전기록HashSha256 = last?.기록HashSha256
                    ?? checkpoint.LastConsolidatedRecordHashSha256;
                normalized.기록HashSha256 = CalculateRecordHash(normalized);
                tailRecords.Add(normalized);
                return Clone(normalized);
            }
        }

        public Simulation행위기록Page Query(Simulation행위기록Query query)
        {
            if (query == null)
                throw new SimulationContractException(
                    "SimulationActionManifestationQueryRequired");
            lock (gate)
            {
                if (!string.Equals(Require(query.WorldStableId,
                            "SimulationActionManifestationQueryWorldInvalid"),
                        worldStableId, StringComparison.Ordinal))
                    throw new SimulationContractException(
                        "SimulationActionManifestationQueryWorldMismatch");
                if (query.MaxCount < 1 || query.MaxCount > 1_000)
                    throw new SimulationContractException(
                        "SimulationActionManifestationQueryLimitInvalid");
                if (query.ThroughWorldRevision < -1)
                    throw new SimulationContractException(
                        "SimulationActionManifestationQueryRevisionInvalid");

                var cursor = query.Cursor ?? new Simulation행위기록Cursor();
                var requiresRebuild = cursor.AfterWorldRevision <
                    checkpoint.ConsolidatedThroughWorldRevision;
                var wi = Set(query.WorldInteractionIds);
                var changes = Set(query.변화의미Codes);
                var spaces = Set(query.공간StableIds);
                var records = tailRecords.Where(value =>
                        value.AfterWorldRevision <= query.ThroughWorldRevision
                        && IsAfter(value, cursor)
                        && (wi.Count == 0 || wi.Contains(value.WorldInteractionId))
                        && (changes.Count == 0 || value.변화의미Codes.Any(changes.Contains))
                        && (spaces.Count == 0 || value.영향공간StableIds.Any(spaces.Contains)))
                    .Take(query.MaxCount).Select(Clone).ToArray();
                var next = records.Length == 0 ? Clone(cursor) : Cursor(records[^1]);
                return new Simulation행위기록Page
                {
                    Records = records,
                    NextCursor = next,
                    RequiresCheckpointRebuild = requiresRebuild,
                    CheckpointWorldRevision = checkpoint.ConsolidatedThroughWorldRevision,
                    CheckpointWorldStateHashSha256 = checkpoint.WorldStateHashSha256,
                };
            }
        }

        public Simulation행위기록LedgerSnapshot CreateCheckpoint(
            long consolidatedThroughWorldRevision,
            string worldStateHashSha256)
        {
            lock (gate)
            {
                if (consolidatedThroughWorldRevision <
                    checkpoint.ConsolidatedThroughWorldRevision)
                    throw new SimulationContractException(
                        "SimulationActionManifestationCheckpointRevisionInvalid");
                var stateHash = Require(worldStateHashSha256,
                    "SimulationActionManifestationCheckpointStateHashInvalid");
                var consolidated = tailRecords.Where(value =>
                        value.AfterWorldRevision <= consolidatedThroughWorldRevision)
                    .ToArray();
                var lastHash = consolidated.LastOrDefault()?.기록HashSha256
                    ?? checkpoint.LastConsolidatedRecordHashSha256;
                checkpoint = new Simulation행위기록CheckpointSnapshot
                {
                    ConsolidatedThroughWorldRevision = consolidatedThroughWorldRevision,
                    WorldStateHashSha256 = stateHash,
                    LastConsolidatedRecordHashSha256 = lastHash,
                };
                tailRecords.RemoveAll(value => value.AfterWorldRevision <=
                    consolidatedThroughWorldRevision);
                ValidateChain();
                return Snapshot();
            }
        }

        public Simulation행위기록LedgerSnapshot Snapshot()
        {
            lock (gate)
            {
                var snapshot = CreateSnapshotCore();
                snapshot.StateHashSha256 = CalculateStateHash(snapshot);
                return snapshot;
            }
        }

        public static string CalculateRecordStableId(Simulation행위발현Record value)
            => "action-manifestation:" + Hash(Canonical(builder =>
            {
                Add(builder, value.WorldStableId);
                Add(builder, value.SessionStableId);
                Add(builder, value.WorldInteractionId);
                Add(builder, value.CommandId);
                Add(builder, value.OutcomeStableId);
                Add(builder, value.AfterWorldRevision);
            })).Substring(0, 32).ToLowerInvariant();

        public static string CalculateRecordHash(Simulation행위발현Record value)
            => Hash(Canonical(builder =>
            {
                Add(builder, value.SchemaCode); Add(builder, value.행위기록StableId);
                Add(builder, value.이전기록HashSha256); Add(builder, value.WorldStableId);
                Add(builder, value.SessionStableId); Add(builder, value.PlayableLoopStableId);
                Add(builder, value.WorldInteractionId); Add(builder, value.CommandId);
                Add(builder, value.TriggerSourceCode); Add(builder, value.InitiatorStableId);
                Add(builder, value.ActorStableId); Add(builder, value.ActorKindCode);
                AddSorted(builder, value.TargetStableIds); Add(builder, value.OutcomeStableId);
                Add(builder, value.PrimaryOutcomeCode); Add(builder, value.결과분류Code);
                Add(builder, value.TaskStableId); Add(builder, value.BattleOutcomeStableId);
                Add(builder, value.EffectBatchStableId); AddSorted(builder, value.EffectReceiptStableIds);
                AddSorted(builder, value.변화의미Codes); AddSorted(builder, value.영향공간StableIds);
                AddSorted(builder, value.SourceReferenceIds); Add(builder, value.BeforeWorldRevision);
                Add(builder, value.AfterWorldRevision); Add(builder, value.AppliedWorldTick);
                Add(builder, value.RuleRevision); Add(builder, value.SpatialRevision);
                Add(builder, value.DataRevision);
            }));

        public static string CalculateStateHash(Simulation행위기록LedgerSnapshot value)
            => Hash(Canonical(builder =>
            {
                Add(builder, value.SchemaCode); Add(builder, value.WorldStableId);
                Add(builder, value.Checkpoint.SchemaCode);
                Add(builder, value.Checkpoint.ConsolidatedThroughWorldRevision);
                Add(builder, value.Checkpoint.WorldStateHashSha256);
                Add(builder, value.Checkpoint.LastConsolidatedRecordHashSha256);
                Add(builder, value.TailRecords.Length);
                foreach (var record in value.TailRecords) Add(builder, record.기록HashSha256);
            }));

        private Simulation행위발현Record Normalize(Simulation행위발현Record source)
        {
            var value = Clone(source);
            value.SchemaCode = Simulation행위기록SchemaCodes.발현기록;
            value.WorldStableId = Require(value.WorldStableId,
                "SimulationActionManifestationWorldStableIdInvalid");
            if (!string.Equals(value.WorldStableId, worldStableId,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationActionManifestationWorldMismatch");
            value.SessionStableId = Require(value.SessionStableId,
                "SimulationActionManifestationSessionInvalid");
            value.WorldInteractionId = Require(value.WorldInteractionId,
                "SimulationActionManifestationWiInvalid");
            value.CommandId = Require(value.CommandId,
                "SimulationActionManifestationCommandInvalid");
            value.ActorStableId = Require(value.ActorStableId,
                "SimulationActionManifestationActorInvalid");
            value.OutcomeStableId = Require(value.OutcomeStableId,
                "SimulationActionManifestationOutcomeInvalid");
            value.PrimaryOutcomeCode = Require(value.PrimaryOutcomeCode,
                "SimulationActionManifestationPrimaryOutcomeInvalid");
            ValidateResultCode(value.결과분류Code);
            if (value.BeforeWorldRevision < 0
                || value.AfterWorldRevision <= value.BeforeWorldRevision
                || value.AppliedWorldTick < 0)
                throw new SimulationContractException(
                    "SimulationActionManifestationRevisionInvalid");
            value.TargetStableIds = Normalize(value.TargetStableIds);
            value.EffectReceiptStableIds = Normalize(value.EffectReceiptStableIds);
            value.변화의미Codes = Normalize(value.변화의미Codes);
            value.영향공간StableIds = Normalize(value.영향공간StableIds);
            value.SourceReferenceIds = Normalize(value.SourceReferenceIds);
            value.행위기록StableId = CalculateRecordStableId(value);
            value.이전기록HashSha256 = string.Empty;
            value.기록HashSha256 = string.Empty;
            return value;
        }

        private void ValidateChain()
        {
            var previous = checkpoint.LastConsolidatedRecordHashSha256;
            Simulation행위발현Record? last = null;
            foreach (var record in tailRecords)
            {
                if (!string.Equals(record.WorldStableId, worldStableId,
                        StringComparison.Ordinal)
                    || record.AfterWorldRevision <=
                        checkpoint.ConsolidatedThroughWorldRevision
                    || (last != null && Compare(last, record) >= 0)
                    || !string.Equals(record.이전기록HashSha256, previous,
                        StringComparison.Ordinal)
                    || !string.Equals(record.기록HashSha256,
                        CalculateRecordHash(record), StringComparison.Ordinal))
                    throw new SimulationContractException(
                        "SimulationActionManifestationChainInvalid");
                previous = record.기록HashSha256;
                last = record;
            }
        }

        private Simulation행위기록LedgerSnapshot CreateSnapshotCore()
            => new Simulation행위기록LedgerSnapshot
            {
                WorldStableId = worldStableId,
                Checkpoint = Clone(checkpoint),
                TailRecords = tailRecords.Select(Clone).ToArray(),
            };

        private static bool IsAfter(Simulation행위발현Record value,
            Simulation행위기록Cursor cursor)
        {
            if (value.AfterWorldRevision != cursor.AfterWorldRevision)
                return value.AfterWorldRevision > cursor.AfterWorldRevision;
            if (value.AppliedWorldTick != cursor.AppliedWorldTick)
                return value.AppliedWorldTick > cursor.AppliedWorldTick;
            return string.CompareOrdinal(value.행위기록StableId,
                cursor.행위기록StableId) > 0;
        }

        private static int Compare(Simulation행위발현Record left,
            Simulation행위발현Record right)
        {
            var revision = left.AfterWorldRevision.CompareTo(right.AfterWorldRevision);
            if (revision != 0) return revision;
            var tick = left.AppliedWorldTick.CompareTo(right.AppliedWorldTick);
            return tick != 0 ? tick : string.CompareOrdinal(
                left.행위기록StableId, right.행위기록StableId);
        }

        private static Simulation행위기록Cursor Cursor(Simulation행위발현Record value)
            => new Simulation행위기록Cursor
            {
                AfterWorldRevision = value.AfterWorldRevision,
                AppliedWorldTick = value.AppliedWorldTick,
                행위기록StableId = value.행위기록StableId,
                기록HashSha256 = value.기록HashSha256,
            };

        private static Simulation행위기록Cursor Clone(Simulation행위기록Cursor value)
            => new Simulation행위기록Cursor
            {
                AfterWorldRevision = value.AfterWorldRevision,
                AppliedWorldTick = value.AppliedWorldTick,
                행위기록StableId = value.행위기록StableId,
                기록HashSha256 = value.기록HashSha256,
            };

        private static Simulation행위기록CheckpointSnapshot Clone(
            Simulation행위기록CheckpointSnapshot value)
            => new Simulation행위기록CheckpointSnapshot
            {
                SchemaCode = value?.SchemaCode ?? Simulation행위기록SchemaCodes.체크포인트,
                ConsolidatedThroughWorldRevision = value?.ConsolidatedThroughWorldRevision ?? -1,
                WorldStateHashSha256 = value?.WorldStateHashSha256 ?? string.Empty,
                LastConsolidatedRecordHashSha256 =
                    value?.LastConsolidatedRecordHashSha256 ?? string.Empty,
            };

        private static Simulation행위발현Record Clone(Simulation행위발현Record value)
            => new Simulation행위발현Record
            {
                SchemaCode = value.SchemaCode, 행위기록StableId = value.행위기록StableId,
                이전기록HashSha256 = value.이전기록HashSha256,
                기록HashSha256 = value.기록HashSha256, WorldStableId = value.WorldStableId,
                SessionStableId = value.SessionStableId,
                PlayableLoopStableId = value.PlayableLoopStableId,
                WorldInteractionId = value.WorldInteractionId, CommandId = value.CommandId,
                TriggerSourceCode = value.TriggerSourceCode,
                InitiatorStableId = value.InitiatorStableId, ActorStableId = value.ActorStableId,
                ActorKindCode = value.ActorKindCode,
                TargetStableIds = (value.TargetStableIds ?? Array.Empty<string>()).ToArray(),
                OutcomeStableId = value.OutcomeStableId,
                PrimaryOutcomeCode = value.PrimaryOutcomeCode,
                결과분류Code = value.결과분류Code, TaskStableId = value.TaskStableId,
                BattleOutcomeStableId = value.BattleOutcomeStableId,
                EffectBatchStableId = value.EffectBatchStableId,
                EffectReceiptStableIds = (value.EffectReceiptStableIds ?? Array.Empty<string>()).ToArray(),
                변화의미Codes = (value.변화의미Codes ?? Array.Empty<string>()).ToArray(),
                영향공간StableIds = (value.영향공간StableIds ?? Array.Empty<string>()).ToArray(),
                SourceReferenceIds = (value.SourceReferenceIds ?? Array.Empty<string>()).ToArray(),
                BeforeWorldRevision = value.BeforeWorldRevision,
                AfterWorldRevision = value.AfterWorldRevision,
                AppliedWorldTick = value.AppliedWorldTick, RuleRevision = value.RuleRevision,
                SpatialRevision = value.SpatialRevision, DataRevision = value.DataRevision,
            };

        private static string[] Normalize(string[]? values)
            => (values ?? Array.Empty<string>()).Where(value =>
                    !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();

        private static HashSet<string> Set(string[]? values)
            => new HashSet<string>(Normalize(values), StringComparer.Ordinal);

        private static void ValidateResultCode(string value)
        {
            if (value != Simulation행위결과분류Codes.성공
                && value != Simulation행위결과분류Codes.의미있는실패
                && value != Simulation행위결과분류Codes.후퇴복구
                && value != Simulation행위결과분류Codes.취소)
                throw new SimulationContractException(
                    "SimulationActionManifestationResultCodeInvalid");
        }

        private static string Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(error);
            return value.Trim();
        }

        private static string Canonical(Action<StringBuilder> append)
        {
            var builder = new StringBuilder(); append(builder); return builder.ToString();
        }

        private static void Add(StringBuilder target, object? value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(text).Append('|');
        }

        private static void AddSorted(StringBuilder target, string[]? values)
        {
            var normalized = Normalize(values); Add(target, normalized.Length);
            foreach (var value in normalized) Add(target, value);
        }

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
