using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public const string 타로화물운송기준PolicyStableId =
            "tarot-baseline-policy:freight.v1";

        public Simulation타로화물운송통합PreviewSnapshot Preview타로화물운송(
            Simulation타로화물운송PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.ResponseStableId != Simulation전차운송대응StableIds.FastTransport
                && request.ResponseStableId
                    != Simulation전차운송대응StableIds.SafeTransport
                && request.ResponseStableId
                    != Simulation전차운송대응StableIds.ConsolidatedTransport)
            {
                throw new SimulationContractException(
                    "SimulationTarotTransportResponseInvalid");
            }
            ValidateFreightTransportPreviewRequest(request.Freight);

            lock (gate)
            {
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");

                var lower = CreateFreightTransportPreview(request.Freight);
                var activeTurnNumber = CurrentTick + 1;
                var activeCard = activeTurnCardEffects.SingleOrDefault(value =>
                    value.ActiveTurnNumber == activeTurnNumber
                    && value.CardStableId == TarotChariotCardStableId
                    && value.CardKindCode == SimulationTurnCardKindCodes.Tarot
                    && value.EffectCode == SimulationTurnCardEffectCodes.ChariotFastTransport
                    && value.OrientationCode == Simulation타로카드방향Codes.Upright);
                var blocks = lower.BlockReasonCodes.ToArray();
                Simulation타로운송보정PreviewSnapshot? tarot = null;

                if (activeCard != null && blocks.Length == 0)
                {
                    var movement = lower.LogisticsMovement;
                    tarot = new Simulation전차화물운송상위규칙()
                        .CreateUprightResponsePreview(
                            new Simulation타로운송기준후보Snapshot
                            {
                                TransportRequestStableId = lower.TransportRequestStableId,
                                LowerRuleStableId = "rule:transport.travel.v1",
                                LowerRuleRevision = 1,
                                CurrentTurnNumber = activeTurnNumber,
                                DurationTicks = movement.RequiredRouteTicks,
                                CargoQuantity = movement.Quantity,
                                ThroughputCapacity = movement.Quantity,
                                VehicleCapacity = request.Freight.Transport.VehicleCapacity,
                                QuantityUnitCode = movement.UnitCode,
                                FuelConsumption = movement.RequiredRouteTicks * 10m,
                                FuelUnitCode = "liter",
                                LaborConsumption = movement.RequiredRouteTicks,
                                LaborUnitCode = "labor-hour",
                                RiskPercentPoint = movement.RequiredRouteTicks * 2m,
                                SourceStableIds = lower.SourceStableIds
                                    .Concat(new[] { 타로화물운송기준PolicyStableId })
                                    .Distinct(StringComparer.Ordinal)
                                    .OrderBy(value => value, StringComparer.Ordinal)
                                    .ToArray(),
                            },
                            activeCard.SourceTurnClosingStableId,
                            request.ResponseStableId);
                }

                return new Simulation타로화물운송통합PreviewSnapshot
                {
                    PreviewStableId = "tarot-freight-preview:"
                        + lower.TransportRequestStableId + ":"
                        + request.ResponseStableId + ":" + activeTurnNumber,
                    BaseRevision = Revision,
                    ActiveTurnNumber = activeTurnNumber,
                    IsCandidateOnly = true,
                    DoesNotApplyResourceLedgers = true,
                    BaselinePolicyStableId = 타로화물운송기준PolicyStableId,
                    LowerRulePreview = lower,
                    ActiveTarotCard = activeCard == null
                        ? null
                        : CloneActiveTurnCardEffect(activeCard),
                    TarotRulePreview = tarot,
                    BlockReasonCodes = blocks,
                    SourceStableIds = lower.SourceStableIds
                        .Concat(tarot?.SourceStableIds ?? Array.Empty<string>())
                        .Concat(new[] { 타로화물운송기준PolicyStableId })
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray(),
                };
            }
        }
    }
}
