using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationO6배치객체StableIds
    {
        public const string HarvestBox = "seedbed-object:farm.potato-harvest-box.a";
        public const string FarmCrate = "seedbed-object:farm.pallet-crate.a";
        public const string HubGate = "seedbed-object:town.hub-inbound-gate.a";
        public const string DeliveryTruck = "seedbed-object:town.delivery-truck.a";
        public const string CargoPallet = "seedbed-object:shared.cargo-pallet.a";
        public const string Market = "seedbed-object:city.urban-market-building.a";
        public const string GroupCart = "seedbed-object:town.grouping-cart-table.a";
        public const string HarvestBoxPlacement =
            "scene-placement:simulation-world-shell.farm.potato-harvest-box.a";
        public const string FarmCratePlacement =
            "scene-placement:simulation-world-shell.farm.pallet-crate.a";
        public const string HubGatePlacement =
            "scene-placement:simulation-world-shell.logistics.hub-inbound-gate.a";
        public const string DeliveryTruckPlacement =
            "scene-placement:simulation-world-shell.logistics.delivery-truck.a";
        public const string CargoPalletPlacement =
            "scene-placement:simulation-world-shell.logistics.cargo-pallet.a";
        public const string MarketPlacement =
            "scene-placement:simulation-world-shell.market.urban-market-shop.a";
        public const string GroupCartPlacement =
            "scene-placement:simulation-world-shell.town.grouping-cart-table.a";
    }

    public static class Simulation타로객체반응상태Codes
    {
        public const string CurrentlyAffected = "CurrentlyAffected";
        public const string StateUnavailable = "StateUnavailable";
    }

    public sealed class Simulation타로객체상태Snapshot
    {
        public string ObjectStableId { get; set; } = string.Empty;
        public string PlacementStableId { get; set; } = string.Empty;
        public bool HasRelevantState { get; set; }
        public string[] StateSourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation타로객체반응Snapshot
    {
        public string ObjectStableId { get; set; } = string.Empty;
        public string PlacementStableId { get; set; } = string.Empty;
        public string RuleDomainCode { get; set; } = string.Empty;
        public string ReactionStateCode { get; set; } = string.Empty;
        public bool CanHighlightInWorld { get; set; }
        public string KoreanSummary { get; set; } = string.Empty;
        public string[] StateSourceStableIds { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation타로CardObjectReactionSnapshot
    {
        public string OfferStableId { get; set; } = string.Empty;
        public string CardStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string OrientationCode { get; set; } = string.Empty;
        public Simulation타로객체반응Snapshot[] ObjectReactions { get; set; }
            = Array.Empty<Simulation타로객체반응Snapshot>();
        public string[] HighlightObjectStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation타로객체반응PreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string DrawStableId { get; set; } = string.Empty;
    }

    public sealed class Simulation타로객체반응PreviewSnapshot
    {
        public string PreviewStableId { get; set; } = string.Empty;
        public long BaseRevision { get; set; }
        public int TurnNumber { get; set; }
        public string DrawStableId { get; set; } = string.Empty;
        public string ObjectCatalogRevision { get; set; } = string.Empty;
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotMutateSession { get; set; }
        public Simulation타로CardObjectReactionSnapshot[] CardReactions { get; set; }
            = Array.Empty<Simulation타로CardObjectReactionSnapshot>();
        public string[] HighlightObjectStableIds { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }
}
