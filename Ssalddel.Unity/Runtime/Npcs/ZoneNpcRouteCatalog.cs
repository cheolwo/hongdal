using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.WorldProjection;

namespace Ssalddel.Unity.Npcs
{
    public sealed class ZoneNpcRouteDefinition
    {
        public string RouteCode { get; set; } = string.Empty;

        public string WorldZoneCode { get; set; } = string.Empty;

        public string ActorRoleCode { get; set; } = string.Empty;

        public string[] WaypointKeys { get; set; } = Array.Empty<string>();

        public bool LoopsInSimulation { get; set; }
    }

    public static class ZoneNpcRouteCatalog
    {
        private static readonly ZoneNpcRouteDefinition[] Definitions =
        {
            Route("farm-producer-round", WorldZoneCodes.Farm, "Producer",
                true, "farm.entry", "farm.field-a", "farm.sensor-a", "farm.packout"),
            Route("market-orderer-browse", WorldZoneCodes.MarketOrder, "Orderer",
                true, "market.entrance", "market.product-shelf", "market.order-desk", "market.exit"),
            Route("market-stock-clerk-round", WorldZoneCodes.MarketOrder, "MarketStockClerk",
                true, "market.stockroom", "market.product-shelf", "market.loading-door"),
            Route("market-group-representative-consultation", WorldZoneCodes.MarketOrder,
                "ResidentialGroupRepresentative", false,
                "market.entrance", "market.manager-desk", "market.exit"),
            Route("residential-orderer-pickup", WorldZoneCodes.ResidentialCommunity, "Orderer",
                false, "residential.home", "residential.community-board", "residential.pickup-point"),
            Route("residential-distribution-round", WorldZoneCodes.ResidentialCommunity, "DistributionWorker",
                true, "residential.loading-point", "residential.pickup-point", "residential.community-office"),
            Route("residential-group-representative-briefing", WorldZoneCodes.ResidentialCommunity,
                "ResidentialGroupRepresentative", false,
                "residential.community-office", "residential.community-board", "residential.departure-point"),
            Route("traditional-market-merchant-round", WorldZoneCodes.TraditionalMarket, "MarketMerchant",
                true, "traditional-market.stall", "traditional-market.storage", "traditional-market.loading-point"),
            Route("traditional-market-transporter-handoff", WorldZoneCodes.TraditionalMarket, "Transporter",
                false, "traditional-market.entrance", "traditional-market.loading-point", "traditional-market.exit"),
            Route("logistics-center-dock-worker-round", WorldZoneCodes.UrbanLogisticsCenter, "DockWorker",
                true, "logistics.staff-entry", "logistics.inbound-dock", "logistics.sorting-zone", "logistics.outbound-dock"),
            Route("logistics-center-transporter-handoff", WorldZoneCodes.UrbanLogisticsCenter, "Transporter",
                false, "logistics.vehicle-gate", "logistics.loading-bay", "logistics.vehicle-exit"),
            Route("transport-network-hub-delivery", WorldZoneCodes.TransportNetwork, "Transporter",
                false, "network.logistics-center", "network.warehouse"),
            Route("warehouse-transporter-dropoff", WorldZoneCodes.Warehouse, "Transporter",
                false, "warehouse.approach", "warehouse.inbound-dock", "warehouse.vehicle-exit"),
            Route("warehouse-inbound-worker-handoff", WorldZoneCodes.Warehouse, "WarehouseInboundWorker",
                false, "warehouse.staff-entry", "warehouse.inbound-dock", "warehouse.inspection-zone", "warehouse.storage-zone"),
            Route("warehouse-picker-round", WorldZoneCodes.Warehouse, "WarehousePicker",
                true, "warehouse.workbench", "warehouse.rack-a", "warehouse.packing-zone", "warehouse.outbound-dock"),
            Route("community-square-member-round", WorldZoneCodes.CommunityMarketSquare, "CommunityMember",
                true, "square.entrance", "square.community-board", "square.market-table", "square.exit"),
            Route("public-data-guide-round", WorldZoneCodes.PublicDataHall, "PublicDataGuide",
                true, "data-hall.entrance", "data-hall.kiosk", "data-hall.map-table"),
            Route("cooperative-facilitator-round", WorldZoneCodes.CooperativeHall, "CooperativeFacilitator",
                true, "cooperative.entrance", "cooperative.ledger-board", "cooperative.meeting-table"),
        };

        public static IReadOnlyList<ZoneNpcRouteDefinition> All => Definitions;

        public static ZoneNpcRouteDefinition? Find(string routeCode)
        {
            return Definitions.FirstOrDefault(item =>
                string.Equals(item.RouteCode, routeCode, StringComparison.Ordinal));
        }

        public static IReadOnlyList<ZoneNpcRouteDefinition> ForZone(string worldZoneCode)
        {
            return Definitions
                .Where(item => string.Equals(item.WorldZoneCode, worldZoneCode, StringComparison.Ordinal))
                .ToArray();
        }

        public static string[] Validate()
        {
            var errors = new List<string>();
            var duplicateRoutes = Definitions
                .GroupBy(item => item.RouteCode, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
            errors.AddRange(duplicateRoutes.Select(route => "DuplicateNpcRoute:" + route));

            foreach (var definition in Definitions)
            {
                if (string.IsNullOrWhiteSpace(definition.RouteCode)
                    || string.IsNullOrWhiteSpace(definition.WorldZoneCode)
                    || string.IsNullOrWhiteSpace(definition.ActorRoleCode))
                {
                    errors.Add("NpcRouteMetadataMissing:" + definition.RouteCode);
                }

                if (definition.WaypointKeys == null || definition.WaypointKeys.Length < 2)
                {
                    errors.Add("NpcRouteWaypointsInsufficient:" + definition.RouteCode);
                    continue;
                }

                if (definition.WaypointKeys.Any(string.IsNullOrWhiteSpace)
                    || definition.WaypointKeys.Distinct(StringComparer.Ordinal).Count()
                        != definition.WaypointKeys.Length)
                {
                    errors.Add("NpcRouteWaypointsInvalid:" + definition.RouteCode);
                }
            }

            return errors.ToArray();
        }

        private static ZoneNpcRouteDefinition Route(
            string routeCode,
            string zoneCode,
            string roleCode,
            bool loops,
            params string[] waypointKeys)
        {
            return new ZoneNpcRouteDefinition
            {
                RouteCode = routeCode,
                WorldZoneCode = zoneCode,
                ActorRoleCode = roleCode,
                WaypointKeys = waypointKeys,
                LoopsInSimulation = loops,
            };
        }
    }
}
