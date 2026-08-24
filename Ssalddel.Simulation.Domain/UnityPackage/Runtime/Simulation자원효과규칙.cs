using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약과 도메인 정의는 실행 위치나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class Simulation자원효과묶음Validator
    {
        private static readonly HashSet<string> KnownRuleDomains = new HashSet<string>(
            new[]
            {
                Simulation업무규칙영역Codes.Production,
                Simulation업무규칙영역Codes.Consumption,
                Simulation업무규칙영역Codes.Transport,
                Simulation업무규칙영역Codes.Warehouse,
                Simulation업무규칙영역Codes.Market,
                Simulation업무규칙영역Codes.Facility,
                Simulation업무규칙영역Codes.Time,
            },
            StringComparer.Ordinal);

        private static readonly HashSet<string> KnownMutationKinds = new HashSet<string>(
            new[]
            {
                Simulation자원변동유형Codes.Production,
                Simulation자원변동유형Codes.Consumption,
                Simulation자원변동유형Codes.Reservation,
                Simulation자원변동유형Codes.ReservationRelease,
                Simulation자원변동유형Codes.Transfer,
                Simulation자원변동유형Codes.Transformation,
                Simulation자원변동유형Codes.Loss,
                Simulation자원변동유형Codes.Recovery,
                Simulation자원변동유형Codes.ExternalInflow,
                Simulation자원변동유형Codes.ExternalOutflow,
                Simulation자원변동유형Codes.CapacityChange,
                Simulation자원변동유형Codes.Reconciliation,
            },
            StringComparer.Ordinal);

        private static readonly HashSet<string> KnownRoles = new HashSet<string>(
            new[]
            {
                Simulation자원효과역할Codes.Output,
                Simulation자원효과역할Codes.Input,
                Simulation자원효과역할Codes.Source,
                Simulation자원효과역할Codes.Target,
                Simulation자원효과역할Codes.Available,
                Simulation자원효과역할Codes.Reserved,
                Simulation자원효과역할Codes.Byproduct,
                Simulation자원효과역할Codes.Loss,
                Simulation자원효과역할Codes.Record,
                Simulation자원효과역할Codes.Capacity,
            },
            StringComparer.Ordinal);

        private static readonly HashSet<string> PairedMutationKinds = new HashSet<string>(
            new[]
            {
                Simulation자원변동유형Codes.Reservation,
                Simulation자원변동유형Codes.ReservationRelease,
                Simulation자원변동유형Codes.Transfer,
                Simulation자원변동유형Codes.Transformation,
                Simulation자원변동유형Codes.Loss,
                Simulation자원변동유형Codes.Recovery,
            },
            StringComparer.Ordinal);

        public void Validate(Simulation자원효과묶음Snapshot bundle)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            RequireStableId(bundle.EffectBundleStableId, "SimulationResourceEffectBundleStableIdInvalid");
            RequireStableId(bundle.RuleStableId, "SimulationResourceRuleStableIdInvalid");
            if (bundle.RuleRevision <= 0)
                throw new SimulationContractException("SimulationResourceRuleRevisionInvalid");
            if (!KnownRuleDomains.Contains(bundle.RuleDomainCode))
                throw new SimulationContractException("SimulationResourceRuleDomainInvalid");
            if (!string.Equals(bundle.ModeCode, "Simulation", StringComparison.Ordinal))
                throw new SimulationContractException("SimulationResourceEffectModeInvalid");
            RequireStableId(
                bundle.CausedByDecisionStableId,
                "SimulationResourceEffectDecisionStableIdInvalid");
            RequireStableId(bundle.CausedByTaskStableId, "SimulationResourceEffectTaskStableIdInvalid");
            ValidateState(bundle);
            ValidateIds(bundle.SourceStableIds, true, "SimulationResourceEffectSourcesInvalid");
            ValidateLines(bundle.Lines);
        }

        private static void ValidateState(Simulation자원효과묶음Snapshot bundle)
        {
            var pending = string.Equals(
                bundle.StateCode,
                SimulationEffectStateCodes.Pending,
                StringComparison.Ordinal);
            var applied = string.Equals(
                bundle.StateCode,
                SimulationEffectStateCodes.Applied,
                StringComparison.Ordinal);
            var cancelled = string.Equals(
                bundle.StateCode,
                SimulationEffectStateCodes.Cancelled,
                StringComparison.Ordinal);
            if (!pending && !applied && !cancelled)
                throw new SimulationContractException("SimulationResourceEffectStateInvalid");
            if ((applied && (!bundle.AppliedTick.HasValue || bundle.AppliedTick.Value < 0))
                || (!applied && bundle.AppliedTick.HasValue))
            {
                throw new SimulationContractException("SimulationResourceEffectAppliedTickInvalid");
            }
        }

        private static void ValidateLines(Simulation자원효과선Snapshot[] lines)
        {
            if (lines == null || lines.Length == 0)
                throw new SimulationContractException("SimulationResourceEffectLinesMissing");

            var lineIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var line in lines)
            {
                ValidateLine(line);
                if (!lineIds.Add(line.EffectLineStableId.Trim()))
                    throw new SimulationContractException("SimulationResourceEffectLineDuplicate");
            }

            var groups = lines
                .Where(value => !string.IsNullOrWhiteSpace(value.ConservationGroupStableId))
                .GroupBy(value => value.ConservationGroupStableId!.Trim(), StringComparer.Ordinal);
            foreach (var group in groups)
                ValidateConservationGroup(group.Key, group.ToArray());

            var ledgerIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var line in lines)
            {
                if (!ledgerIds.Add(line.TargetLedgerStableId.Trim()))
                    throw new SimulationContractException("SimulationResourceEffectLedgerDuplicate");
            }

            foreach (var line in lines.Where(value => PairedMutationKinds.Contains(value.MutationKindCode)))
            {
                if (string.IsNullOrWhiteSpace(line.ConservationGroupStableId))
                    throw new SimulationContractException("SimulationResourceConservationGroupRequired");
            }
        }

        private static void ValidateLine(Simulation자원효과선Snapshot line)
        {
            if (line == null)
                throw new SimulationContractException("SimulationResourceEffectLineInvalid");
            RequireStableId(line.EffectLineStableId, "SimulationResourceEffectLineStableIdInvalid");
            if (!KnownMutationKinds.Contains(line.MutationKindCode))
                throw new SimulationContractException("SimulationResourceMutationKindInvalid");
            if (!KnownRoles.Contains(line.RoleCode))
                throw new SimulationContractException("SimulationResourceEffectRoleInvalid");
            RequireStableId(line.ResourceTypeCode, "SimulationResourceTypeCodeInvalid");
            RequireStableId(line.TargetLedgerStableId, "SimulationResourceLedgerStableIdInvalid");
            RequireOptionalStableId(line.ProductStableId, "SimulationResourceProductStableIdInvalid");
            RequireOptionalStableId(line.LotStableId, "SimulationResourceLotStableIdInvalid");
            RequireStableId(line.UnitCode, "SimulationResourceUnitCodeInvalid");
            ValidateIds(line.SourceStableIds, true, "SimulationResourceEffectLineSourcesInvalid");
            if (line.BeforeValue < 0m || line.AfterValue < 0m)
                throw new SimulationContractException("SimulationResourceNegativeLedgerValueInvalid");
            if (line.BeforeValue + line.Delta != line.AfterValue)
                throw new SimulationContractException("SimulationResourceValueConservationInvalid");

            ValidateRoleAndSign(line);
            ValidateConservationFields(line);
        }

        private static void ValidateRoleAndSign(Simulation자원효과선Snapshot line)
        {
            var kind = line.MutationKindCode;
            var role = line.RoleCode;
            var delta = line.Delta;
            var valid =
                (kind == Simulation자원변동유형Codes.Production
                    && role == Simulation자원효과역할Codes.Output && delta > 0m)
                || (kind == Simulation자원변동유형Codes.Consumption
                    && ((role == Simulation자원효과역할Codes.Input && delta < 0m)
                        || (role == Simulation자원효과역할Codes.Record && delta > 0m)))
                || (kind == Simulation자원변동유형Codes.Reservation
                    && ((role == Simulation자원효과역할Codes.Available && delta < 0m)
                        || (role == Simulation자원효과역할Codes.Reserved && delta > 0m)))
                || (kind == Simulation자원변동유형Codes.ReservationRelease
                    && ((role == Simulation자원효과역할Codes.Available && delta > 0m)
                        || (role == Simulation자원효과역할Codes.Reserved && delta < 0m)))
                || (kind == Simulation자원변동유형Codes.Transfer
                    && ((role == Simulation자원효과역할Codes.Source && delta < 0m)
                        || (role == Simulation자원효과역할Codes.Target && delta > 0m)
                        || (role == Simulation자원효과역할Codes.Loss && delta > 0m)))
                || (kind == Simulation자원변동유형Codes.Transformation
                    && ((role == Simulation자원효과역할Codes.Input && delta < 0m)
                        || (role == Simulation자원효과역할Codes.Output && delta > 0m)
                        || (role == Simulation자원효과역할Codes.Byproduct && delta > 0m)
                        || (role == Simulation자원효과역할Codes.Loss && delta > 0m)))
                || (kind == Simulation자원변동유형Codes.Loss
                    && ((role == Simulation자원효과역할Codes.Source && delta < 0m)
                        || (role == Simulation자원효과역할Codes.Loss && delta > 0m)))
                || (kind == Simulation자원변동유형Codes.Recovery
                    && ((role == Simulation자원효과역할Codes.Source && delta < 0m)
                        || (role == Simulation자원효과역할Codes.Target && delta > 0m)))
                || (kind == Simulation자원변동유형Codes.ExternalInflow
                    && role == Simulation자원효과역할Codes.Target && delta > 0m)
                || (kind == Simulation자원변동유형Codes.ExternalOutflow
                    && role == Simulation자원효과역할Codes.Source && delta < 0m)
                || (kind == Simulation자원변동유형Codes.CapacityChange
                    && role == Simulation자원효과역할Codes.Capacity && delta != 0m)
                || (kind == Simulation자원변동유형Codes.Reconciliation
                    && role == Simulation자원효과역할Codes.Record && delta == 0m);

            if (!valid)
                throw new SimulationContractException("SimulationResourceEffectRoleSignMismatch");
        }

        private static void ValidateConservationFields(Simulation자원효과선Snapshot line)
        {
            var hasGroup = !string.IsNullOrWhiteSpace(line.ConservationGroupStableId);
            var hasUnit = !string.IsNullOrWhiteSpace(line.ConservationUnitCode);
            if (hasGroup != hasUnit)
                throw new SimulationContractException("SimulationResourceConservationFieldsInvalid");
            if (!hasGroup)
            {
                if (line.ConservationQuantity != 0m)
                    throw new SimulationContractException("SimulationResourceConservationQuantityUnexpected");
                return;
            }

            if (!PairedMutationKinds.Contains(line.MutationKindCode))
                throw new SimulationContractException("SimulationResourceConservationGroupUnexpected");

            RequireStableId(
                line.ConservationGroupStableId!,
                "SimulationResourceConservationGroupStableIdInvalid");
            RequireStableId(
                line.ConservationUnitCode!,
                "SimulationResourceConservationUnitCodeInvalid");
            if (line.ConservationQuantity == 0m
                || Math.Sign(line.ConservationQuantity) != Math.Sign(line.Delta))
            {
                throw new SimulationContractException("SimulationResourceConservationQuantityInvalid");
            }
        }

        private static void ValidateConservationGroup(
            string groupStableId,
            Simulation자원효과선Snapshot[] lines)
        {
            RequireStableId(groupStableId, "SimulationResourceConservationGroupStableIdInvalid");
            var mutationKinds = lines
                .Select(value => value.MutationKindCode)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var units = lines
                .Select(value => value.ConservationUnitCode)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (mutationKinds.Length != 1 || units.Length != 1)
                throw new SimulationContractException("SimulationResourceConservationGroupMixed");
            if (lines.Sum(value => value.ConservationQuantity) != 0m)
                throw new SimulationContractException("SimulationResourceConservationImbalance");

            var roles = new HashSet<string>(lines.Select(value => value.RoleCode), StringComparer.Ordinal);
            var kind = mutationKinds[0];
            var required = kind == Simulation자원변동유형Codes.Reservation
                    || kind == Simulation자원변동유형Codes.ReservationRelease
                ? new[] { Simulation자원효과역할Codes.Available, Simulation자원효과역할Codes.Reserved }
                : kind == Simulation자원변동유형Codes.Transfer
                    || kind == Simulation자원변동유형Codes.Recovery
                ? new[] { Simulation자원효과역할Codes.Source, Simulation자원효과역할Codes.Target }
                : kind == Simulation자원변동유형Codes.Transformation
                ? new[] { Simulation자원효과역할Codes.Input, Simulation자원효과역할Codes.Output }
                : kind == Simulation자원변동유형Codes.Loss
                ? new[] { Simulation자원효과역할Codes.Source, Simulation자원효과역할Codes.Loss }
                : Array.Empty<string>();
            if (required.Any(value => !roles.Contains(value)))
                throw new SimulationContractException("SimulationResourceConservationRolesMissing");
        }

        private static void ValidateIds(string[] values, bool requireAny, string errorCode)
        {
            if (values == null || (requireAny && values.Length == 0))
                throw new SimulationContractException(errorCode);
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                RequireStableId(value, errorCode);
                if (!unique.Add(value.Trim()))
                    throw new SimulationContractException(errorCode);
            }
        }

        private static void RequireOptionalStableId(string? value, string errorCode)
        {
            if (value != null) RequireStableId(value, errorCode);
        }

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
            {
                throw new SimulationContractException(errorCode);
            }
        }
    }

    public sealed class Simulation자원효과적용기
    {
        private readonly Simulation자원효과묶음Validator validator;

        public Simulation자원효과적용기()
            : this(new Simulation자원효과묶음Validator())
        {
        }

        public Simulation자원효과적용기(Simulation자원효과묶음Validator value)
        {
            validator = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Simulation자원효과적용Result Apply(
            Simulation자원원장상태Snapshot state,
            Simulation자원효과묶음Snapshot pendingBundle,
            int appliedTick)
        {
            ValidateState(state);
            validator.Validate(pendingBundle);
            if (pendingBundle.StateCode != SimulationEffectStateCodes.Pending)
                throw new SimulationContractException("SimulationResourceEffectBundleNotPending");
            if (appliedTick < state.WorldTick)
                throw new SimulationContractException("SimulationResourceEffectAppliedTickStale");
            if (state.AppliedEffectBundleStableIds.Contains(
                pendingBundle.EffectBundleStableId,
                StringComparer.Ordinal))
            {
                throw new SimulationContractException("SimulationResourceEffectBundleAlreadyApplied");
            }

            var ledgerById = state.Ledgers.ToDictionary(
                value => value.LedgerStableId,
                CloneLedger,
                StringComparer.Ordinal);
            foreach (var line in pendingBundle.Lines)
            {
                if (ledgerById.TryGetValue(line.TargetLedgerStableId, out var ledger))
                {
                    ValidateLineAgainstLedger(line, ledger);
                }
                else
                {
                    if (line.BeforeValue != 0m)
                        throw new SimulationContractException("SimulationResourceLedgerMissing");
                    ledger = new Simulation자원원장항목Snapshot
                    {
                        LedgerStableId = line.TargetLedgerStableId,
                        ResourceTypeCode = line.ResourceTypeCode,
                        ProductStableId = line.ProductStableId,
                        LotStableId = line.LotStableId,
                        Value = 0m,
                        UnitCode = line.UnitCode,
                        SourceStableIds = Array.Empty<string>(),
                    };
                    ledgerById.Add(ledger.LedgerStableId, ledger);
                }

                ledger.Value = line.AfterValue;
                ledger.SourceStableIds = MergeSources(ledger.SourceStableIds, line.SourceStableIds);
            }

            var applied = CloneBundle(pendingBundle);
            applied.StateCode = SimulationEffectStateCodes.Applied;
            applied.AppliedTick = appliedTick;
            return new Simulation자원효과적용Result
            {
                State = new Simulation자원원장상태Snapshot
                {
                    Revision = state.Revision + 1,
                    WorldTick = appliedTick,
                    Ledgers = ledgerById.Values
                        .OrderBy(value => value.LedgerStableId, StringComparer.Ordinal)
                        .ToArray(),
                    AppliedEffectBundleStableIds = state.AppliedEffectBundleStableIds
                        .Concat(new[] { pendingBundle.EffectBundleStableId })
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray(),
                    SourceStableIds = MergeSources(
                        state.SourceStableIds,
                        pendingBundle.SourceStableIds),
                },
                AppliedEffectBundle = applied,
            };
        }

        private static void ValidateState(Simulation자원원장상태Snapshot state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Revision < 0 || state.WorldTick < 0
                || state.Ledgers == null
                || state.AppliedEffectBundleStableIds == null
                || state.SourceStableIds == null)
            {
                throw new SimulationContractException("SimulationResourceLedgerStateInvalid");
            }

            var ledgerIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var ledger in state.Ledgers)
            {
                if (ledger == null || ledger.Value < 0m)
                {
                    throw new SimulationContractException("SimulationResourceLedgerStateInvalid");
                }
                RequireStableId(ledger.LedgerStableId, "SimulationResourceLedgerStateInvalid");
                if (!ledgerIds.Add(ledger.LedgerStableId))
                    throw new SimulationContractException("SimulationResourceLedgerStateInvalid");
                RequireStableId(ledger.ResourceTypeCode, "SimulationResourceLedgerStateInvalid");
                RequireOptionalStableId(ledger.ProductStableId, "SimulationResourceLedgerStateInvalid");
                RequireOptionalStableId(ledger.LotStableId, "SimulationResourceLedgerStateInvalid");
                RequireStableId(ledger.UnitCode, "SimulationResourceLedgerStateInvalid");
                ValidateIds(ledger.SourceStableIds, true, "SimulationResourceLedgerStateInvalid");
            }

            ValidateIds(
                state.AppliedEffectBundleStableIds,
                false,
                "SimulationResourceAppliedBundleIdsInvalid");
            ValidateIds(state.SourceStableIds, true, "SimulationResourceLedgerSourcesInvalid");
        }

        private static void ValidateLineAgainstLedger(
            Simulation자원효과선Snapshot line,
            Simulation자원원장항목Snapshot ledger)
        {
            if (ledger.Value != line.BeforeValue)
                throw new SimulationContractException("SimulationResourceLedgerBeforeValueMismatch");
            if (ledger.ResourceTypeCode != line.ResourceTypeCode
                || ledger.UnitCode != line.UnitCode
                || ledger.ProductStableId != line.ProductStableId
                || ledger.LotStableId != line.LotStableId)
            {
                throw new SimulationContractException("SimulationResourceLedgerBindingMismatch");
            }
        }

        private static Simulation자원원장항목Snapshot CloneLedger(
            Simulation자원원장항목Snapshot value)
            => new Simulation자원원장항목Snapshot
            {
                LedgerStableId = value.LedgerStableId,
                ResourceTypeCode = value.ResourceTypeCode,
                ProductStableId = value.ProductStableId,
                LotStableId = value.LotStableId,
                Value = value.Value,
                UnitCode = value.UnitCode,
                SourceStableIds = value.SourceStableIds.ToArray(),
            };

        private static Simulation자원효과묶음Snapshot CloneBundle(
            Simulation자원효과묶음Snapshot value)
            => new Simulation자원효과묶음Snapshot
            {
                EffectBundleStableId = value.EffectBundleStableId,
                RuleStableId = value.RuleStableId,
                RuleRevision = value.RuleRevision,
                RuleDomainCode = value.RuleDomainCode,
                ModeCode = value.ModeCode,
                StateCode = value.StateCode,
                CausedByDecisionStableId = value.CausedByDecisionStableId,
                CausedByTaskStableId = value.CausedByTaskStableId,
                AppliedTick = value.AppliedTick,
                Lines = value.Lines.Select(CloneLine).ToArray(),
                SourceStableIds = value.SourceStableIds.ToArray(),
            };

        private static Simulation자원효과선Snapshot CloneLine(
            Simulation자원효과선Snapshot value)
            => new Simulation자원효과선Snapshot
            {
                EffectLineStableId = value.EffectLineStableId,
                MutationKindCode = value.MutationKindCode,
                RoleCode = value.RoleCode,
                ResourceTypeCode = value.ResourceTypeCode,
                TargetLedgerStableId = value.TargetLedgerStableId,
                ProductStableId = value.ProductStableId,
                LotStableId = value.LotStableId,
                BeforeValue = value.BeforeValue,
                Delta = value.Delta,
                AfterValue = value.AfterValue,
                UnitCode = value.UnitCode,
                ConservationGroupStableId = value.ConservationGroupStableId,
                ConservationQuantity = value.ConservationQuantity,
                ConservationUnitCode = value.ConservationUnitCode,
                SourceStableIds = value.SourceStableIds.ToArray(),
            };

        private static string[] MergeSources(string[] left, string[] right)
            => left.Concat(right)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        private static void ValidateIds(string[] values, bool requireAny, string errorCode)
        {
            if (values == null || (requireAny && values.Length == 0))
                throw new SimulationContractException(errorCode);
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                RequireStableId(value, errorCode);
                if (!unique.Add(value.Trim()))
                    throw new SimulationContractException(errorCode);
            }
        }

        private static void RequireOptionalStableId(string? value, string errorCode)
        {
            if (value != null) RequireStableId(value, errorCode);
        }

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
            {
                throw new SimulationContractException(errorCode);
            }
        }
    }
}
