using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string TarotJourneyRootRuleRevision =
            "tarot-journey-root.r1";
        private const string TarotFrameRuleRevision = "tarot-context-frame.r1";
        private const string TarotProposalRuleRevision = "tarot-context-proposal.r1";
        private const string TarotRelationRuleRevision = "tarot-card-relation.r1";
        private const string TarotIncidentEvaluationRuleRevision =
            "tarot-incident-evaluation-lineage.r1";

        private SimulationTarotContextStateSnapshot tarotContextState = EmptyTarotContext();

        public SimulationTarotContextStateSnapshot GetTarotContext()
        {
            lock (gate) return CloneTarotContext(tarotContextState);
        }

        private void ApplyTarotContext(SimulationTurnClosingSnapshot closing)
        {
            var selected = closing.SelectedCards.SingleOrDefault(value =>
                value.CardKindCode == SimulationTurnCardKindCodes.Tarot);
            var nextFrameSetRevision = tarotContextState.FrameSet.Revision + 1;
            if (selected == null)
            {
                tarotContextState = EmptyTarotContext(nextFrameSetRevision,
                    Revision, CurrentTick + 1);
                return;
            }

            var frameId = string.Concat("tarot-frame:", SessionStableId, ":",
                closing.ClosedTurnNumber.ToString(CultureInfo.InvariantCulture), ":",
                selected.CardCopyStableId.Length > 0
                    ? selected.CardCopyStableId : selected.CardStableId);
            var proposalCode = ProposalCode(selected);
            var proposalId = frameId + ":proposal:" + proposalCode.ToLowerInvariant();
            var frame = new SimulationTarotFrameSnapshot
            {
                FrameStableId = frameId,
                CardStableId = selected.CardStableId,
                CardCopyStableId = selected.CardCopyStableId,
                CardRevision = selected.CardRevision,
                OrientationCode = selected.OrientationCode,
                ParentJourneyFrameStableId =
                    SimulationTarotJourneyRootCodes.FoolFrameStableId,
                MetaLayerCode = SimulationTarotMetaLayerCodes.ActiveMajorArcana,
                FrameScopeCode = SimulationTarotFrameScopeCodes.Turn,
                ScopeTargetStableId = SessionStableId,
                StartsAtTurnNumber = CurrentTick + 1,
                EndsAtTurnNumber = CurrentTick + 1,
                ThemeCodes = ThemeCodes(selected),
                ContextProposalStableIds = new[] { proposalId },
                SourceDrawStableId = closing.TurnClosingStableId + ":tarot-draw",
                SourceOfferStableId = selected.OfferStableId,
                RuleRevision = TarotFrameRuleRevision,
                SourceStableId = selected.SourceStableId,
            };
            var proposal = new SimulationTarotContextProposalSnapshot
            {
                ProposalStableId = proposalId,
                ContextProposalCode = proposalCode,
                SourceFrameStableId = frameId,
                FrameScopeCode = frame.FrameScopeCode,
                ScopeTargetStableId = frame.ScopeTargetStableId,
                SourceThemeCode = frame.ThemeCodes.First(),
                SourceWorldRevision = Revision,
                SourceTurnNumber = CurrentTick + 1,
                RuleRevision = TarotProposalRuleRevision,
            };
            var frameSet = new SimulationTarotFrameSetSnapshot
            {
                Revision = nextFrameSetRevision,
                SourceWorldRevision = Revision,
                SourceTurnNumber = CurrentTick + 1,
                JourneyRoot = BuildFoolJourneyRoot(),
                ActiveFrames = new[] { frame },
            };
            frameSet.FrameSetHashSha256 = Hash(CanonicalFrameSet(frameSet));
            var relations = BuildTarotRelations(frame, proposal);
            var evaluation = new SimulationTarotIncidentEvaluationSnapshot
            {
                EvaluationStableId = frameId + ":incident-evaluation",
                ProposalStableIds = new[] { proposalId },
                EvaluationResultCode =
                    SimulationTarotIncidentEvaluationResultCodes.NoIncident,
                IncidentStableId = string.Empty,
                EffectStableIds = Array.Empty<string>(),
                EvaluatedWorldRevision = Revision,
                EvaluatedTurnNumber = CurrentTick + 1,
                RuleRevision = TarotIncidentEvaluationRuleRevision,
            };
            tarotContextState = new SimulationTarotContextStateSnapshot
            {
                FrameSet = frameSet,
                Proposals = new[] { proposal },
                Relations = relations,
                IncidentEvaluations = new[] { evaluation },
            };
            tarotContextState.ContextStateHashSha256 = Hash(
                CanonicalTarotContext(tarotContextState));
        }

        private SimulationCardContextRelationSnapshot[] BuildTarotRelations(
            SimulationTarotFrameSnapshot frame,
            SimulationTarotContextProposalSnapshot proposal)
        {
            var relations = new List<SimulationCardContextRelationSnapshot>();
            var roleCards = teamRoleCardState?.Snapshot().Cards
                ?? Array.Empty<SimulationTeamRoleCardSnapshot>();
            foreach (var card in roleCards)
            {
                var relation = RelationForRoleCard(proposal.ContextProposalCode, card);
                if (card.IsLocked)
                    relation = SimulationCardContextRelationCodes.BlockExplained;
                if (relation.Length == 0) continue;
                relations.Add(Relation(frame, SimulationCardFamilyCodes.TeamRole,
                    card.CardDefinitionStableId, card.CardCopyStableId, relation,
                    card.IsLocked ? new[] { "SimulationTeamRoleCardActiveLock" }
                        : new[] { proposal.ContextProposalCode },
                    teamRoleCardState?.Revision ?? Revision));
            }

            foreach (var card in CreateAvailableTurnCards())
            {
                var family = card.CardKindCode == SimulationTurnCardKindCodes.Culture
                    ? SimulationCardFamilyCodes.Culture
                    : SimulationCardFamilyCodes.TurnClosing;
                var relation = card.CardKindCode == SimulationTurnCardKindCodes.Culture
                    ? SimulationCardContextRelationCodes.Relevant
                    : SimulationCardContextRelationCodes.Contrasted;
                relations.Add(Relation(frame, family, card.CardStableId,
                    card.CardCopyStableId, relation,
                    new[] { proposal.ContextProposalCode }, Revision));
            }
            return relations.OrderBy(value => value.TargetCardFamilyCode,
                    StringComparer.Ordinal)
                .ThenBy(value => value.TargetCardStableId, StringComparer.Ordinal)
                .ThenBy(value => value.TargetCardCopyStableId, StringComparer.Ordinal)
                .ToArray();
        }

        private SimulationCardContextRelationSnapshot Relation(
            SimulationTarotFrameSnapshot frame, string family, string cardId,
            string copyId, string code, string[] reasons, long availabilityRevision)
            => new()
            {
                RelationStableId = string.Join(":", "card-relation",
                    frame.FrameStableId, family, copyId.Length > 0 ? copyId : cardId),
                SourceFrameStableId = frame.FrameStableId,
                TargetCardFamilyCode = family,
                TargetCardStableId = cardId,
                TargetCardCopyStableId = copyId,
                RelationCode = code,
                ReasonCodes = reasons,
                RuleRevision = TarotRelationRuleRevision,
                AvailabilityRevision = availabilityRevision,
                SourceWorldRevision = Revision,
                SourceTurnNumber = CurrentTick + 1,
                ChangesAvailability = false,
            };

        private static string RelationForRoleCard(string proposal,
            SimulationTeamRoleCardSnapshot card)
        {
            if (proposal == SimulationTarotContextProposalCodes.Growth
                && card.ActivityRoleCodes.Contains(SimulationTeamRoleCardCodes.FarmWork,
                    StringComparer.Ordinal))
                return SimulationCardContextRelationCodes.Recommended;
            if (proposal == SimulationTarotContextProposalCodes.Movement
                && card.ActivityRoleCodes.Contains(SimulationTeamRoleCardCodes.Logistics,
                    StringComparer.Ordinal))
                return SimulationCardContextRelationCodes.Recommended;
            if (proposal == SimulationTarotContextProposalCodes.Disruption
                && card.ActivityRoleCodes.Contains(SimulationTeamRoleCardCodes.Exploration,
                    StringComparer.Ordinal))
                return SimulationCardContextRelationCodes.Warned;
            if (proposal == SimulationTarotContextProposalCodes.Balance)
                return SimulationCardContextRelationCodes.Relevant;
            return string.Empty;
        }

        private static string ProposalCode(SimulationTurnCardSnapshot card)
        {
            if (card.OrientationCode == Simulation타로카드방향Codes.Reversed)
                return SimulationTarotContextProposalCodes.Disruption;
            return card.EffectCode switch
            {
                SimulationTurnCardEffectCodes.EmpressProductionGrowth
                    => SimulationTarotContextProposalCodes.Growth,
                SimulationTurnCardEffectCodes.ChariotFastTransport
                    => SimulationTarotContextProposalCodes.Movement,
                SimulationTurnCardEffectCodes.JusticeTradeBalance
                    => SimulationTarotContextProposalCodes.Balance,
                SimulationTurnCardEffectCodes.TemperanceFlowBalance
                    => SimulationTarotContextProposalCodes.Balance,
                _ => SimulationTarotContextProposalCodes.Balance,
            };
        }

        private static string[] ThemeCodes(SimulationTurnCardSnapshot card)
        {
            if (card.OrientationCode == Simulation타로카드방향Codes.Reversed)
                return new[]
                {
                    SimulationTarotThemeCodes.Disruption,
                    SimulationTarotThemeCodes.Collapse,
                };
            return card.EffectCode switch
            {
                SimulationTurnCardEffectCodes.EmpressProductionGrowth => new[]
                {
                    SimulationTarotThemeCodes.Growth,
                    SimulationTarotThemeCodes.Abundance,
                    SimulationTarotThemeCodes.Nurture,
                },
                SimulationTurnCardEffectCodes.ChariotFastTransport => new[]
                {
                    SimulationTarotThemeCodes.Movement,
                    SimulationTarotThemeCodes.Flow,
                },
                _ => new[]
                {
                    SimulationTarotThemeCodes.Balance,
                    SimulationTarotThemeCodes.Flow,
                },
            };
        }

        private void ExpireTarotContext()
        {
            if (tarotContextState.FrameSet.ActiveFrames.Any(value =>
                    value.EndsAtTurnNumber < CurrentTick + 1))
                tarotContextState = EmptyTarotContext(
                    tarotContextState.FrameSet.Revision + 1, Revision,
                    CurrentTick + 1);
        }

        private SimulationTarotContextStateSnapshot CreateTarotContextSnapshot()
            => CloneTarotContext(tarotContextState);

        internal static SimulationTarotContextStateSnapshot CloneTarotContext(
            SimulationTarotContextStateSnapshot source) => new()
            {
                FrameSet = new SimulationTarotFrameSetSnapshot
                {
                    Revision = source.FrameSet.Revision,
                    SourceWorldRevision = source.FrameSet.SourceWorldRevision,
                    SourceTurnNumber = source.FrameSet.SourceTurnNumber,
                    FrameSetHashSha256 = source.FrameSet.FrameSetHashSha256,
                    JourneyRoot = CloneJourneyRoot(source.FrameSet.JourneyRoot),
                    ActiveFrames = source.FrameSet.ActiveFrames.Select(value => new
                        SimulationTarotFrameSnapshot
                    {
                        FrameStableId = value.FrameStableId,
                        CardStableId = value.CardStableId,
                        CardCopyStableId = value.CardCopyStableId,
                        CardRevision = value.CardRevision,
                        OrientationCode = value.OrientationCode,
                        ParentJourneyFrameStableId =
                            value.ParentJourneyFrameStableId,
                        MetaLayerCode = value.MetaLayerCode,
                        FrameScopeCode = value.FrameScopeCode,
                        ScopeTargetStableId = value.ScopeTargetStableId,
                        StartsAtTurnNumber = value.StartsAtTurnNumber,
                        EndsAtTurnNumber = value.EndsAtTurnNumber,
                        ThemeCodes = value.ThemeCodes.ToArray(),
                        ContextProposalStableIds =
                            value.ContextProposalStableIds.ToArray(),
                        SourceDrawStableId = value.SourceDrawStableId,
                        SourceOfferStableId = value.SourceOfferStableId,
                        RuleRevision = value.RuleRevision,
                        SourceStableId = value.SourceStableId,
                    }).ToArray(),
                },
                Proposals = source.Proposals.Select(value => new
                    SimulationTarotContextProposalSnapshot
                {
                    ProposalStableId = value.ProposalStableId,
                    ContextProposalCode = value.ContextProposalCode,
                    SourceFrameStableId = value.SourceFrameStableId,
                    FrameScopeCode = value.FrameScopeCode,
                    ScopeTargetStableId = value.ScopeTargetStableId,
                    SourceThemeCode = value.SourceThemeCode,
                    SourceWorldRevision = value.SourceWorldRevision,
                    SourceTurnNumber = value.SourceTurnNumber,
                    RuleRevision = value.RuleRevision,
                }).ToArray(),
                Relations = source.Relations.Select(value => new
                    SimulationCardContextRelationSnapshot
                {
                    RelationStableId = value.RelationStableId,
                    SourceFrameStableId = value.SourceFrameStableId,
                    TargetCardFamilyCode = value.TargetCardFamilyCode,
                    TargetCardStableId = value.TargetCardStableId,
                    TargetCardCopyStableId = value.TargetCardCopyStableId,
                    RelationCode = value.RelationCode,
                    ReasonCodes = value.ReasonCodes.ToArray(),
                    RuleRevision = value.RuleRevision,
                    AvailabilityRevision = value.AvailabilityRevision,
                    SourceWorldRevision = value.SourceWorldRevision,
                    SourceTurnNumber = value.SourceTurnNumber,
                    ChangesAvailability = value.ChangesAvailability,
                }).ToArray(),
                IncidentEvaluations = source.IncidentEvaluations.Select(value => new
                    SimulationTarotIncidentEvaluationSnapshot
                {
                    EvaluationStableId = value.EvaluationStableId,
                    ProposalStableIds = value.ProposalStableIds.ToArray(),
                    EvaluationResultCode = value.EvaluationResultCode,
                    IncidentStableId = value.IncidentStableId,
                    EffectStableIds = value.EffectStableIds.ToArray(),
                    EvaluatedWorldRevision = value.EvaluatedWorldRevision,
                    EvaluatedTurnNumber = value.EvaluatedTurnNumber,
                    RuleRevision = value.RuleRevision,
                }).ToArray(),
                ContextStateHashSha256 = source.ContextStateHashSha256,
            };

        private static SimulationTarotContextStateSnapshot EmptyTarotContext(
            long revision = 0, long worldRevision = 0, int turnNumber = 0)
        {
            var frameSet = new SimulationTarotFrameSetSnapshot
            {
                Revision = revision,
                SourceWorldRevision = worldRevision,
                SourceTurnNumber = turnNumber,
                JourneyRoot = BuildFoolJourneyRoot(),
                ActiveFrames = Array.Empty<SimulationTarotFrameSnapshot>(),
            };
            frameSet.FrameSetHashSha256 = Hash(CanonicalFrameSet(frameSet));
            var state = new SimulationTarotContextStateSnapshot
            {
                FrameSet = frameSet,
            };
            state.ContextStateHashSha256 = Hash(CanonicalTarotContext(state));
            return state;
        }

        private static SimulationTarotJourneyRootSnapshot BuildFoolJourneyRoot()
            => new()
            {
                FrameStableId = SimulationTarotJourneyRootCodes.FoolFrameStableId,
                CardStableId = SimulationTarotJourneyRootCodes.FoolCardStableId,
                CardRevision = "evening-hakdang.fixture-r1",
                Title = "0. 바보 · 여정의 시작",
                Summary = "모름을 인정하고 가능성을 열어 둔 채 아르카나의 변화를 지나간다.",
                TraditionalArcanaNumber =
                    SimulationTarotJourneyRootCodes.TraditionalArcanaNumber,
                JourneySequenceOrder =
                    SimulationTarotJourneyRootCodes.JourneySequenceOrder,
                HierarchyTierCode = SimulationCardHierarchyTierCodes.Meta,
                MetaLayerCode = SimulationTarotMetaLayerCodes.JourneyRoot,
                IsAlwaysActive = true,
                ThemeCodes = new[]
                {
                    SimulationTarotThemeCodes.Beginning,
                    SimulationTarotThemeCodes.Possibility,
                    SimulationTarotThemeCodes.Unknown,
                    SimulationTarotThemeCodes.Cycle,
                },
                RuleRevision = TarotJourneyRootRuleRevision,
                SourceStableId = "source:hongik-hakdang.fool.beginner-mind",
            };

        private static SimulationTarotJourneyRootSnapshot CloneJourneyRoot(
            SimulationTarotJourneyRootSnapshot? source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.CardStableId))
                return BuildFoolJourneyRoot();
            return new SimulationTarotJourneyRootSnapshot
            {
                FrameStableId = source.FrameStableId,
                CardStableId = source.CardStableId,
                CardRevision = source.CardRevision,
                Title = source.Title,
                Summary = source.Summary,
                TraditionalArcanaNumber = source.TraditionalArcanaNumber,
                JourneySequenceOrder = source.JourneySequenceOrder,
                HierarchyTierCode = source.HierarchyTierCode,
                MetaLayerCode = source.MetaLayerCode,
                IsAlwaysActive = source.IsAlwaysActive,
                ThemeCodes = source.ThemeCodes.ToArray(),
                RuleRevision = source.RuleRevision,
                SourceStableId = source.SourceStableId,
            };
        }

        private static string CanonicalFrameSet(
            SimulationTarotFrameSetSnapshot value,
            bool includeJourneyRoot = true)
        {
            var target = new StringBuilder();
            Add(target, value.Revision); Add(target, value.SourceWorldRevision);
            Add(target, value.SourceTurnNumber);
            if (includeJourneyRoot)
            {
                var root = CloneJourneyRoot(value.JourneyRoot);
                Add(target, root.FrameStableId); Add(target, root.CardStableId);
                Add(target, root.CardRevision); Add(target, root.Title);
                Add(target, root.Summary); Add(target, root.TraditionalArcanaNumber);
                Add(target, root.JourneySequenceOrder); Add(target, root.HierarchyTierCode);
                Add(target, root.MetaLayerCode); Add(target, root.IsAlwaysActive);
                foreach (var theme in root.ThemeCodes.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(target, theme);
                Add(target, root.RuleRevision); Add(target, root.SourceStableId);
            }
            foreach (var frame in value.ActiveFrames.OrderBy(item => item.FrameScopeCode,
                         StringComparer.Ordinal).ThenBy(item => item.ScopeTargetStableId,
                         StringComparer.Ordinal).ThenBy(item => item.FrameStableId,
                         StringComparer.Ordinal))
            {
                Add(target, frame.FrameStableId); Add(target, frame.CardStableId);
                Add(target, frame.CardCopyStableId); Add(target, frame.CardRevision);
                Add(target, frame.OrientationCode);
                if (includeJourneyRoot)
                {
                    Add(target, frame.ParentJourneyFrameStableId);
                    Add(target, frame.MetaLayerCode);
                }
                Add(target, frame.FrameScopeCode);
                Add(target, frame.ScopeTargetStableId); Add(target, frame.StartsAtTurnNumber);
                Add(target, frame.EndsAtTurnNumber);
                foreach (var theme in frame.ThemeCodes.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(target, theme);
                foreach (var proposal in frame.ContextProposalStableIds.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(target, proposal);
                Add(target, frame.SourceDrawStableId); Add(target, frame.SourceOfferStableId);
                Add(target, frame.RuleRevision); Add(target, frame.SourceStableId);
            }
            return target.ToString();
        }

        private static string CanonicalTarotContext(
            SimulationTarotContextStateSnapshot value,
            bool includeJourneyRoot = true)
        {
            var target = new StringBuilder(CanonicalFrameSet(
                value.FrameSet, includeJourneyRoot));
            foreach (var proposal in value.Proposals.OrderBy(item => item.ProposalStableId,
                         StringComparer.Ordinal))
            {
                Add(target, proposal.ProposalStableId); Add(target, proposal.ContextProposalCode);
                Add(target, proposal.SourceFrameStableId); Add(target, proposal.FrameScopeCode);
                Add(target, proposal.ScopeTargetStableId); Add(target, proposal.SourceThemeCode);
                Add(target, proposal.SourceWorldRevision); Add(target, proposal.SourceTurnNumber);
                Add(target, proposal.RuleRevision);
            }
            foreach (var relation in value.Relations.OrderBy(item => item.RelationStableId,
                         StringComparer.Ordinal))
            {
                Add(target, relation.RelationStableId); Add(target, relation.SourceFrameStableId);
                Add(target, relation.TargetCardFamilyCode); Add(target, relation.TargetCardStableId);
                Add(target, relation.TargetCardCopyStableId); Add(target, relation.RelationCode);
                foreach (var reason in relation.ReasonCodes.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(target, reason);
                Add(target, relation.RuleRevision); Add(target, relation.AvailabilityRevision);
                Add(target, relation.SourceWorldRevision); Add(target, relation.SourceTurnNumber);
                Add(target, relation.ChangesAvailability);
            }
            foreach (var evaluation in value.IncidentEvaluations.OrderBy(item =>
                         item.EvaluationStableId, StringComparer.Ordinal))
            {
                Add(target, evaluation.EvaluationStableId);
                foreach (var proposal in evaluation.ProposalStableIds.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(target, proposal);
                Add(target, evaluation.EvaluationResultCode); Add(target, evaluation.IncidentStableId);
                foreach (var effect in evaluation.EffectStableIds.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(target, effect);
                Add(target, evaluation.EvaluatedWorldRevision);
                Add(target, evaluation.EvaluatedTurnNumber); Add(target, evaluation.RuleRevision);
            }
            return target.ToString();
        }

        internal static string BuildTarotContextStatePayloadKey(
            SimulationTarotContextStateSnapshot value)
            => CanonicalTarotContext(value);

        internal static string BuildLegacyTarotContextStatePayloadKey(
            SimulationTarotContextStateSnapshot value)
            => CanonicalTarotContext(value, false);

        internal static string BuildLegacyTarotContextStateHash(
            SimulationTarotContextStateSnapshot value)
            => Hash(CanonicalTarotContext(value, false));

        private static void Add(StringBuilder target, object? value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(text).Append('|');
        }

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
