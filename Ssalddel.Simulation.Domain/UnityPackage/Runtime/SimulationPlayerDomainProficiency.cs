using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation플레이어분야Engine
    {
        private readonly object gate = new object();
        private readonly Simulation플레이어분야CatalogSnapshot catalog;
        private Simulation플레이어분야ProfileSnapshot state;

        public Simulation플레이어분야Engine(string playerStableId,
            Simulation플레이어분야CatalogSnapshot? catalog = null)
        {
            this.catalog = catalog ?? Simulation기본플레이어분야Catalog.Create();
            ValidateCatalog(this.catalog);
            state = new Simulation플레이어분야ProfileSnapshot
            {
                PlayerStableId = Require(playerStableId,
                    "SimulationPlayerDomainPlayerInvalid"),
                CatalogRevision = this.catalog.CatalogRevision,
                RuleRevision = this.catalog.RuleRevision,
                분야진척들 = this.catalog.분야들.Select(CreateProgress).ToArray(),
            };
            RefreshHash();
        }

        private Simulation플레이어분야Engine(
            Simulation플레이어분야ProfileSnapshot snapshot,
            Simulation플레이어분야CatalogSnapshot catalog)
        {
            this.catalog = catalog;
            ValidateCatalog(catalog);
            state = Clone(snapshot);
            if (!string.Equals(state.SchemaCode,
                    Simulation플레이어분야SchemaCodes.분야Profile,
                    StringComparison.Ordinal)
                || !string.Equals(state.CatalogRevision, catalog.CatalogRevision,
                    StringComparison.Ordinal)
                || !string.Equals(state.RuleRevision, catalog.RuleRevision,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationPlayerDomainProfileRevisionInvalid");
            if (!string.Equals(state.StateHashSha256, CalculateHash(state),
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationPlayerDomainProfileHashMismatch");
            ValidateProfileShape();
        }

        public static Simulation플레이어분야Engine Restore(
            Simulation플레이어분야ProfileSnapshot snapshot,
            Simulation플레이어분야CatalogSnapshot? catalog = null)
            => new Simulation플레이어분야Engine(snapshot,
                catalog ?? Simulation기본플레이어분야Catalog.Create());

        public Simulation플레이어분야ProfileSnapshot ApplyField(
            Simulation현장숙련기여Request request)
        {
            if (request == null) throw new SimulationContractException(
                "SimulationPlayerDomainFieldRequestRequired");
            lock (gate)
            {
                ValidatePlayer(request.PlayerStableId);
                var record = ValidateRecord(request.행위기록);
                if (!string.Equals(record.ActorStableId, state.PlayerStableId,
                        StringComparison.Ordinal))
                    throw new SimulationContractException(
                        "SimulationPlayerDomainFieldActorMismatch");
                var binding = Binding(record.WorldInteractionId);
                if (binding.기여방식Code != Simulation분야기여방식Codes.PlayerDirect
                    && binding.기여방식Code !=
                        Simulation분야기여방식Codes.PlayerOrOperation)
                    return Snapshot();
                var amount = ResultAmount(record.결과분류Code);
                ApplyBindings(binding, record, Simulation분야기여SourceCodes.플레이어현장행동,
                    0, amount, 0, record.행위기록StableId);
                return Snapshot();
            }
        }

        public Simulation플레이어분야ProfileSnapshot ApplyLearning(
            Simulation분야학습기여Request request)
        {
            if (request == null) throw new SimulationContractException(
                "SimulationPlayerDomainLearningRequestRequired");
            lock (gate)
            {
                ValidatePlayer(request.PlayerStableId);
                var record = ValidateRecord(request.적용행위기록);
                Require(request.PublicationStableId,
                    "SimulationPlayerDomainPublicationInvalid");
                Require(request.PublicationRevision,
                    "SimulationPlayerDomainPublicationRevisionInvalid");
                Require(request.PublicationHashSha256,
                    "SimulationPlayerDomainPublicationHashInvalid");
                if (request.AppliedWorldRevision != record.AfterWorldRevision)
                    throw new SimulationContractException(
                        "SimulationPlayerDomainLearningRevisionMismatch");
                var changed = false;
                foreach (var effect in (request.효과선들 ??
                             Array.Empty<Simulation분야이해효과선Snapshot>())
                         .OrderBy(value => value.분야StableId, StringComparer.Ordinal)
                         .ThenBy(value => value.세부숙련StableId,
                             StringComparer.Ordinal))
                {
                    if (effect.이해도증가량 <= 0)
                        throw new SimulationContractException(
                            "SimulationPlayerDomainLearningAmountInvalid");
                    ValidateDomainSkill(effect.분야StableId,
                        effect.세부숙련StableId);
                    var id = ContributionId(record.행위기록StableId,
                        Simulation분야기여SourceCodes.승인자료성찰,
                        effect.분야StableId, effect.세부숙련StableId);
                    if (HasContribution(id)) continue;
                    AddContribution(new Simulation분야진척기여Snapshot
                    {
                        ContributionStableId = id,
                        PlayerStableId = state.PlayerStableId,
                        SourceCode = Simulation분야기여SourceCodes.승인자료성찰,
                        분야StableId = effect.분야StableId,
                        세부숙련StableId = effect.세부숙련StableId,
                        PublicationStableId = request.PublicationStableId,
                        PublicationRevision = request.PublicationRevision,
                        WorldInteractionId = record.WorldInteractionId,
                        OriginCommandId = record.CommandId,
                        SourceActionRecordStableId = record.행위기록StableId,
                        EffectBatchStableId = record.EffectBatchStableId,
                        EffectReceiptStableId = EffectReceiptId(id),
                        결과Code = record.결과분류Code,
                        이해도증가량 = effect.이해도증가량,
                        AppliedWorldRevision = record.AfterWorldRevision,
                        RuleRevision = effect.RuleRevision,
                    }, effect.해금후보Codes);
                    changed = true;
                }
                if (changed) CompleteMutation();
                return Snapshot();
            }
        }

        public Simulation플레이어분야ProfileSnapshot ApplyOperation(
            Simulation운영숙련기여Request request)
        {
            if (request == null) throw new SimulationContractException(
                "SimulationPlayerDomainOperationRequestRequired");
            lock (gate)
            {
                ValidatePlayer(request.PlayerStableId);
                var delegation = ValidateRecord(request.위임행위기록);
                var npc = ValidateRecord(request.Npc완료행위기록);
                var review = ValidateRecord(request.검토행위기록);
                if (!string.Equals(delegation.InitiatorStableId,
                        state.PlayerStableId, StringComparison.Ordinal)
                    || string.Equals(npc.ActorStableId, state.PlayerStableId,
                        StringComparison.Ordinal)
                    || !string.Equals(review.ActorStableId, state.PlayerStableId,
                        StringComparison.Ordinal)
                    || !npc.SourceReferenceIds.Contains(delegation.행위기록StableId,
                        StringComparer.Ordinal)
                    || !review.SourceReferenceIds.Contains(npc.행위기록StableId,
                        StringComparer.Ordinal)
                    || !review.SourceReferenceIds.Contains(delegation.행위기록StableId,
                        StringComparer.Ordinal))
                    throw new SimulationContractException(
                        "SimulationPlayerDomainOperationLineageInvalid");
                if (delegation.결과분류Code != Simulation행위결과분류Codes.성공
                    || npc.결과분류Code != Simulation행위결과분류Codes.성공
                    || review.결과분류Code != Simulation행위결과분류Codes.성공)
                    return Snapshot();
                var binding = Binding(delegation.WorldInteractionId);
                if (binding.기여방식Code != Simulation분야기여방식Codes.OperationOnly
                    && binding.기여방식Code !=
                        Simulation분야기여방식Codes.PlayerOrOperation)
                    return Snapshot();
                ApplyBindings(binding, review,
                    Simulation분야기여SourceCodes.플레이어운영위임,
                    0, 0, 1, delegation.행위기록StableId + "~" +
                    npc.행위기록StableId + "~" + review.행위기록StableId);
                return Snapshot();
            }
        }

        public Simulation플레이어분야PerspectiveWorldState CreatePerspective(
            string dataRevision, string interpretationRevision,
            string[] authorizedFactCodes)
        {
            lock (gate)
            {
                var profile = Snapshot();
                var combat = profile.분야진척들.First(value =>
                    value.분야StableId == Simulation플레이어분야Codes.전투사냥);
                return new Simulation플레이어분야PerspectiveWorldState
                {
                    PlayerStableId = state.PlayerStableId,
                    DataRevision = dataRevision ?? string.Empty,
                    InterpretationRevision = interpretationRevision ?? string.Empty,
                    ProfileRevision = state.Revision.ToString(CultureInfo.InvariantCulture),
                    강조분야들 = profile.분야진척들.OrderByDescending(value =>
                            value.이해도 + value.현장숙련도 + value.운영숙련도)
                        .ThenBy(value => value.분야StableId, StringComparer.Ordinal)
                        .Select(Clone).ToArray(),
                    전체자료접근Codes = Normalize(authorizedFactCodes),
                    선택형기회후보Codes = combat.이해도 >= 3
                        ? new[] { "optional-hunt-offer:nature" }
                        : Array.Empty<string>(),
                };
            }
        }

        public Simulation플레이어분야ProfileSnapshot Snapshot()
        {
            lock (gate) return Clone(state);
        }

        public static string CalculateHash(Simulation플레이어분야ProfileSnapshot value)
        {
            var canonical = new StringBuilder();
            Add(canonical, value.SchemaCode); Add(canonical, value.PlayerStableId);
            Add(canonical, value.Revision); Add(canonical, value.CatalogRevision);
            Add(canonical, value.RuleRevision);
            foreach (var progress in value.분야진척들.OrderBy(item =>
                         item.분야StableId, StringComparer.Ordinal))
            {
                Add(canonical, progress.분야StableId); Add(canonical, progress.이해도);
                Add(canonical, progress.현장숙련도); Add(canonical, progress.운영숙련도);
                Add(canonical, progress.이해도단계Code); Add(canonical, progress.현장숙련도단계Code);
                Add(canonical, progress.운영숙련도단계Code);
                foreach (var skill in progress.세부숙련진척들.OrderBy(item =>
                             item.세부숙련StableId, StringComparer.Ordinal))
                {
                    Add(canonical, skill.세부숙련StableId); Add(canonical, skill.이해도);
                    Add(canonical, skill.현장숙련도); Add(canonical, skill.운영숙련도);
                }
                AddStrings(canonical, progress.활성해금Codes);
            }
            foreach (var contribution in value.기여기록들.OrderBy(item =>
                         item.ContributionStableId, StringComparer.Ordinal))
            {
                Add(canonical, contribution.ContributionStableId);
                Add(canonical, contribution.PlayerStableId); Add(canonical, contribution.SourceCode);
                Add(canonical, contribution.분야StableId); Add(canonical, contribution.세부숙련StableId);
                Add(canonical, contribution.PublicationStableId); Add(canonical, contribution.PublicationRevision);
                Add(canonical, contribution.WorldInteractionId); Add(canonical, contribution.OriginCommandId);
                Add(canonical, contribution.SourceActionRecordStableId);
                Add(canonical, contribution.EffectBatchStableId); Add(canonical, contribution.EffectReceiptStableId);
                Add(canonical, contribution.결과Code); Add(canonical, contribution.이해도증가량);
                Add(canonical, contribution.현장숙련도증가량); Add(canonical, contribution.운영숙련도증가량);
                Add(canonical, contribution.AppliedWorldRevision); Add(canonical, contribution.RuleRevision);
            }
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical.ToString())))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private void ApplyBindings(SimulationWI분야결속Definition binding,
            Simulation행위발현Record record, string source, int understanding,
            int field, int operation, string sourceKey)
        {
            if (understanding == 0 && field == 0 && operation == 0) return;
            var changed = false;
            foreach (var line in binding.결속선들.OrderBy(value =>
                         value.분야StableId, StringComparer.Ordinal).ThenBy(value =>
                         value.세부숙련StableId, StringComparer.Ordinal))
            {
                ValidateDomainSkill(line.분야StableId, line.세부숙련StableId);
                var u = Weighted(understanding, line.기여가중치Permille);
                var f = Weighted(field, line.기여가중치Permille);
                var o = Weighted(operation, line.기여가중치Permille);
                var id = ContributionId(sourceKey, source, line.분야StableId,
                    line.세부숙련StableId);
                if (HasContribution(id)) continue;
                AddContribution(new Simulation분야진척기여Snapshot
                {
                    ContributionStableId = id, PlayerStableId = state.PlayerStableId,
                    SourceCode = source, 분야StableId = line.분야StableId,
                    세부숙련StableId = line.세부숙련StableId,
                    WorldInteractionId = record.WorldInteractionId,
                    OriginCommandId = record.CommandId,
                    SourceActionRecordStableId = record.행위기록StableId,
                    EffectBatchStableId = record.EffectBatchStableId,
                    EffectReceiptStableId = EffectReceiptId(id),
                    결과Code = record.결과분류Code, 이해도증가량 = u,
                    현장숙련도증가량 = f, 운영숙련도증가량 = o,
                    AppliedWorldRevision = record.AfterWorldRevision,
                    RuleRevision = catalog.RuleRevision,
                }, Array.Empty<string>());
                changed = true;
            }
            if (changed) CompleteMutation();
        }

        private void AddContribution(Simulation분야진척기여Snapshot contribution,
            string[] unlocks)
        {
            var progress = state.분야진척들.First(value =>
                value.분야StableId == contribution.분야StableId);
            var skill = progress.세부숙련진척들.First(value =>
                value.세부숙련StableId == contribution.세부숙련StableId);
            progress.이해도 += contribution.이해도증가량;
            progress.현장숙련도 += contribution.현장숙련도증가량;
            progress.운영숙련도 += contribution.운영숙련도증가량;
            skill.이해도 += contribution.이해도증가량;
            skill.현장숙련도 += contribution.현장숙련도증가량;
            skill.운영숙련도 += contribution.운영숙련도증가량;
            progress.활성해금Codes = Normalize(progress.활성해금Codes
                .Concat(unlocks ?? Array.Empty<string>()).ToArray());
            progress.이해도단계Code = Stage(progress.이해도,
                catalog.이해도단계기준들);
            progress.현장숙련도단계Code = Stage(progress.현장숙련도,
                catalog.숙련도단계기준들);
            progress.운영숙련도단계Code = Stage(progress.운영숙련도,
                catalog.숙련도단계기준들);
            state.기여기록들 = state.기여기록들.Concat(new[] { contribution })
                .OrderBy(value => value.ContributionStableId,
                    StringComparer.Ordinal).ToArray();
        }

        private void CompleteMutation()
        {
            state.Revision++;
            RefreshHash();
        }

        private void RefreshHash() => state.StateHashSha256 = CalculateHash(state);

        private Simulation행위발현Record ValidateRecord(Simulation행위발현Record record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.행위기록StableId)
                || !string.Equals(record.기록HashSha256,
                    Simulation행위발현Ledger.CalculateRecordHash(record),
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationPlayerDomainActionRecordInvalid");
            if (!record.변화의미Codes.Contains(
                    Simulation행위변화의미Codes.플레이어진척변경,
                    StringComparer.Ordinal))
                throw new SimulationContractException(
                    "SimulationPlayerDomainProgressEffectMissing");
            return record;
        }

        private SimulationWI분야결속Definition Binding(string wi)
            => catalog.Wi결속들.FirstOrDefault(value =>
                string.Equals(value.WorldInteractionId, wi, StringComparison.Ordinal))
                ?? throw new SimulationContractException(
                    "SimulationPlayerDomainWiBindingMissing");

        private void ValidatePlayer(string player)
        {
            if (!string.Equals(Require(player, "SimulationPlayerDomainPlayerInvalid"),
                    state.PlayerStableId, StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationPlayerDomainPlayerMismatch");
        }

        private void ValidateDomainSkill(string domain, string skill)
        {
            var definition = catalog.분야들.FirstOrDefault(value =>
                value.분야StableId == domain) ?? throw new SimulationContractException(
                "SimulationPlayerDomainUnknown");
            if (!definition.세부숙련들.Any(value => value.StableId == skill))
                throw new SimulationContractException("SimulationPlayerDomainSkillUnknown");
        }

        private void ValidateProfileShape()
        {
            if (state.분야진척들.Length != catalog.분야들.Length)
                throw new SimulationContractException("SimulationPlayerDomainProfileShapeInvalid");
            foreach (var domain in catalog.분야들)
            {
                var progress = state.분야진척들.SingleOrDefault(value =>
                    value.분야StableId == domain.분야StableId);
                if (progress == null || progress.세부숙련진척들.Length !=
                    domain.세부숙련들.Length)
                    throw new SimulationContractException("SimulationPlayerDomainProfileShapeInvalid");
            }
        }

        private static void ValidateCatalog(Simulation플레이어분야CatalogSnapshot value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.CatalogRevision)
                || string.IsNullOrWhiteSpace(value.RuleRevision)
                || value.분야들.GroupBy(item => item.분야StableId,
                    StringComparer.Ordinal).Any(group => group.Count() != 1)
                || value.Wi결속들.GroupBy(item => item.WorldInteractionId,
                    StringComparer.Ordinal).Any(group => group.Count() != 1))
                throw new SimulationContractException("SimulationPlayerDomainCatalogInvalid");
        }

        private static Simulation분야진척Snapshot CreateProgress(
            Simulation플레이어분야Definition definition)
            => new Simulation분야진척Snapshot
            {
                분야StableId = definition.분야StableId,
                세부숙련진척들 = definition.세부숙련들.Select(value =>
                    new Simulation세부숙련진척Snapshot
                        { 세부숙련StableId = value.StableId }).ToArray(),
            };

        private bool HasContribution(string id) => state.기여기록들.Any(value =>
            value.ContributionStableId == id);

        private static int ResultAmount(string code)
            => code == Simulation행위결과분류Codes.성공 ? 2
                : code == Simulation행위결과분류Codes.의미있는실패
                  || code == Simulation행위결과분류Codes.후퇴복구 ? 1 : 0;

        private static int Weighted(int value, int permille)
            => value <= 0 ? 0 : Math.Max(1, value * permille / 1_000);

        private static string Stage(int value,
            Simulation분야단계기준Definition[] thresholds)
            => thresholds.Where(item => item.최소진척 <= value)
                .OrderByDescending(item => item.최소진척).First().단계Code;

        private static string ContributionId(string sourceKey, string source,
            string domain, string skill)
            => "domain-contribution:" + Hash(string.Join("|", sourceKey,
                source, domain, skill)).Substring(0, 32);

        private static string EffectReceiptId(string contributionId)
            => "domain-progress-effect:" + Hash(contributionId).Substring(0, 32);

        private static string[] Normalize(string[]? values)
            => (values ?? Array.Empty<string>()).Where(value =>
                    !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();

        private static string Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(error);
            return value.Trim();
        }

        private static void Add(StringBuilder target, object? value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(text).Append('|');
        }

        private static void AddStrings(StringBuilder target, string[]? values)
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

        private static Simulation플레이어분야ProfileSnapshot Clone(
            Simulation플레이어분야ProfileSnapshot value)
            => new Simulation플레이어분야ProfileSnapshot
            {
                SchemaCode = value.SchemaCode, PlayerStableId = value.PlayerStableId,
                Revision = value.Revision, CatalogRevision = value.CatalogRevision,
                RuleRevision = value.RuleRevision,
                분야진척들 = value.분야진척들.Select(Clone).ToArray(),
                기여기록들 = value.기여기록들.Select(Clone).ToArray(),
                StateHashSha256 = value.StateHashSha256,
            };

        private static Simulation분야진척Snapshot Clone(Simulation분야진척Snapshot value)
            => new Simulation분야진척Snapshot
            {
                분야StableId = value.분야StableId, 이해도 = value.이해도,
                현장숙련도 = value.현장숙련도, 운영숙련도 = value.운영숙련도,
                이해도단계Code = value.이해도단계Code,
                현장숙련도단계Code = value.현장숙련도단계Code,
                운영숙련도단계Code = value.운영숙련도단계Code,
                세부숙련진척들 = value.세부숙련진척들.Select(item =>
                    new Simulation세부숙련진척Snapshot
                    {
                        세부숙련StableId = item.세부숙련StableId, 이해도 = item.이해도,
                        현장숙련도 = item.현장숙련도, 운영숙련도 = item.운영숙련도,
                    }).ToArray(),
                활성해금Codes = value.활성해금Codes.ToArray(),
            };

        private static Simulation분야진척기여Snapshot Clone(
            Simulation분야진척기여Snapshot value)
            => new Simulation분야진척기여Snapshot
            {
                SchemaCode = value.SchemaCode, ContributionStableId = value.ContributionStableId,
                PlayerStableId = value.PlayerStableId, SourceCode = value.SourceCode,
                분야StableId = value.분야StableId, 세부숙련StableId = value.세부숙련StableId,
                PublicationStableId = value.PublicationStableId,
                PublicationRevision = value.PublicationRevision,
                WorldInteractionId = value.WorldInteractionId, OriginCommandId = value.OriginCommandId,
                SourceActionRecordStableId = value.SourceActionRecordStableId,
                EffectBatchStableId = value.EffectBatchStableId,
                EffectReceiptStableId = value.EffectReceiptStableId, 결과Code = value.결과Code,
                이해도증가량 = value.이해도증가량, 현장숙련도증가량 = value.현장숙련도증가량,
                운영숙련도증가량 = value.운영숙련도증가량,
                AppliedWorldRevision = value.AppliedWorldRevision, RuleRevision = value.RuleRevision,
            };
    }
}
