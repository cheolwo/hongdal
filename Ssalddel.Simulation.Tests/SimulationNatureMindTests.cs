using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Infrastructure;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationNatureMindTests
{
    [Fact]
    public void 같은FarmFact를_서로다른개인Nature가_다르게해석한다()
    {
        var service = Service();
        var session = service.Create(CreateRequest());

        var recovery = service.GetNatureFarmInterpretation(
            session.SessionStableId, "player:mind:recovery");
        var threat = service.GetNatureFarmInterpretation(
            session.SessionStableId, "player:mind:threat");

        Assert.Equal(recovery.WorldRevision, threat.WorldRevision);
        Assert.Equal(recovery.FactStableId, threat.FactStableId);
        Assert.Equal(recovery.FactValue, threat.FactValue);
        Assert.Equal(recovery.FactStateHashSha256, threat.FactStateHashSha256);
        Assert.Equal(SimulationNatureMindCodes.RecoveryDominantBand,
            recovery.Balance.InterpretationBandCode);
        Assert.Equal(SimulationNatureMindCodes.ThreatDominantBand,
            threat.Balance.InterpretationBandCode);
        Assert.NotEqual(recovery.InferenceCode, threat.InferenceCode);
        Assert.NotEqual(recovery.MoodProjectionCode, threat.MoodProjectionCode);
        Assert.NotEqual(recovery.PrioritizedCardStableIds[0],
            threat.PrioritizedCardStableIds[0]);
        Assert.False(recovery.ChangesSharedFact);
        Assert.False(threat.ChangesSharedFact);
    }

    [Fact]
    public void Farm판로Effect는_개인회복위협원장에한번만적용되고_SaveReplay된다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = Service(saveStore);
        var session = service.Create(CreateRequest());
        var impact = ImpactRequest();
        service.PreviewHarvestDispositionImpact(session.SessionStableId, impact);
        var confirmed = service.ConfirmHarvestDispositionImpact(
            session.SessionStableId,
            new SimulationHarvestDispositionImpactConfirmRequest
            {
                CommandId = "command:nature-mind:farm-choice",
                ExpectedRevision = session.Revision,
                Impact = impact,
            });
        var firstTick = service.Advance(session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:nature-mind:tick-1",
                ExpectedRevision = confirmed.Revision,
                TickCount = 1,
            });
        var completed = service.Advance(session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:nature-mind:tick-2",
                ExpectedRevision = firstTick.Revision,
                TickCount = 1,
            });
        var retried = service.Advance(session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:nature-mind:tick-2",
                ExpectedRevision = firstTick.Revision,
                TickCount = 1,
            });

        Assert.Equal(completed.Revision, retried.Revision);
        Assert.Equal(4, completed.NatureMind.Effects.Length);
        Assert.All(completed.NatureMind.Balances, balance =>
        {
            Assert.Equal(2, balance.Revision);
            Assert.Equal(1m, balance.RecoveryShare + balance.ThreatShare);
            Assert.NotEmpty(balance.BalanceHashSha256);
        });
        Assert.Contains(completed.NatureMind.Effects, value =>
            value.SourceCode
                == SimulationNatureMindCodes.FarmHarvestDispositionCompleted
            && value.AxisCode == SimulationNatureMindCodes.RecoveryAxis
            && value.Magnitude == 2m);
        Assert.Contains(completed.NatureMind.Effects, value =>
            value.SourceCode
                == SimulationNatureMindCodes.FarmHarvestDispositionCompleted
            && value.AxisCode == SimulationNatureMindCodes.ThreatAxis
            && value.Magnitude == -1m);

        var saved = service.Save(session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:nature-mind:farm-choice",
                ExpectedRevision = completed.Revision,
            });
        var verified = service.VerifyReplay(new SimulationSessionRestoreRequest
        {
            SaveStableId = saved.SaveStableId,
        });
        Assert.Equal(SimulationSaveSchemaVersions.V9, saved.SchemaVersion);
        Assert.Equal(saved.ReplayHash, verified.ReplayHash);
        Assert.Equal(
            completed.NatureMind.Balances[0].BalanceHashSha256,
            verified.Session.NatureMind.Balances[0].BalanceHashSha256);
    }

    private static 경영SimulationSessionService Service(
        ISimulationSessionSaveStore? saveStore = null)
        => new(new InMemory경영SimulationSessionStore(),
            saveStore ?? new InMemorySimulationSessionSaveStore());

    private static SimulationHarvestDispositionImpactPreviewRequest ImpactRequest()
        => new()
        {
            DispositionDecisionStableId = "harvest-disposition:nature-mind:potato",
            DispositionDecisionRevision = 1,
            HarvestLotStableId = "harvest-lot:nature-mind:potato",
            HarvestLotRevision = 1,
            ProductStableId = "product:potato",
            Quantity = 300m,
            UnitCode = "KGM",
            ChoiceCode = SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
            NextWorkflowCode =
                SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate,
            ActorStableId = "actor:nature-mind:farmer",
            SourceStableIds = new[]
            {
                "source:nature-mind:farm",
                "harvest-lot:nature-mind:potato",
            },
        };

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:nature-mind:farm",
            ScenarioDataRevision = "scenario-data:nature-mind:r1",
            ScenarioSeed = 20260821,
            RuleRevision = "rule:nature-mind:r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:nature-mind",
                TerritoryStableId = "territory:nature-mind",
                SettlementStableId = "settlement:nature-mind",
                GameDateStartsOn = new DateTimeOffset(
                    2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
            },
            NatureMind = new SimulationNatureMindInitialStateRequest
            {
                Players = new[]
                {
                    new SimulationNatureMindPlayerInitialStateRequest
                    {
                        PlayerStableId = "player:mind:recovery",
                        RecoveryBaseOutput = 7m,
                        ThreatBaseOutput = 3m,
                    },
                    new SimulationNatureMindPlayerInitialStateRequest
                    {
                        PlayerStableId = "player:mind:threat",
                        RecoveryBaseOutput = 3m,
                        ThreatBaseOutput = 7m,
                    },
                },
            },
            Settlement = new SimulationSettlementInitialStateRequest
            {
                TreasuryBalance = 1_000_000m,
                CurrencyCode = "KRW",
                LaborCapacityTotal = 100m,
                LaborReserved = 25m,
                StorageCapacity = 2000m,
                StorageOccupied = 1200m,
                StorageUnitCode = "KGM",
                PopulationCount = 100,
                PopulationFoodDemandPerTick = 100m,
                GarrisonCount = 20,
                GarrisonFoodDemandPerTick = 20m,
                FoodEquivalentUnitCode = "FoodEquivalentUnit",
                FoodEquivalentRuleRevision = "food-equivalent:fixture-r1",
                Districts = new[]
                {
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:nature-mind:farm",
                        DistrictTypeCode = "FarmDistrict",
                        SourceStableIds = new[] { "source:nature-mind:farm" },
                    },
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:nature-mind:central",
                        DistrictTypeCode = "CentralDistrict",
                        SourceStableIds = new[] { "source:nature-mind:farm" },
                    },
                },
                Facilities = new[]
                {
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:nature-mind:storage",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Storage,
                        DistrictStableId = "district:nature-mind:farm",
                        SourceStableIds = new[] { "source:nature-mind:farm" },
                    },
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:nature-mind:market",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Market,
                        DistrictStableId = "district:nature-mind:central",
                        SourceStableIds = new[] { "source:nature-mind:farm" },
                    },
                },
                MarketSupplyByProduct = new[]
                {
                    new SimulationMarketSupplyRequest
                    {
                        ProductStableId = "product:potato",
                        Quantity = 300m,
                        UnitCode = "KGM",
                        SourceStableIds = new[] { "source:nature-mind:farm" },
                    },
                },
                ReserveStockLots = new[]
                {
                    new SimulationReserveStockLotRequest
                    {
                        StockLotStableId = "stock-lot:nature-mind:potato",
                        ProductStableId = "product:potato",
                        StorageFacilityStableId = "facility:nature-mind:storage",
                        Quantity = 1000m,
                        UnitCode = "KGM",
                        FoodEquivalentQuantity = 1200m,
                        SourceStableIds = new[] { "source:nature-mind:farm" },
                    },
                },
                SourceStableIds = new[] { "source:nature-mind:farm" },
            },
        };
}
