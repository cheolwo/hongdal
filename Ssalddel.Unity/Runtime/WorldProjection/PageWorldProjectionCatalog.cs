using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.WorldProjection
{
    public static class PageWorldProjectionCatalog
    {
        private static readonly PageWorldProjectionDefinition[] RepresentativeDefinitions =
        {
            Define("/community/home", "커뮤니티 운영 지도", "01", WorldZoneCodes.CommunityMarketSquare,
                new[] { PageProjectionTypeCodes.Spatial, PageProjectionTypeCodes.Object }, "world-map-table", "open-world-map", "world-map-detail", "world-map", WorldViewerScopeCodes.Public, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Projected),
            Define("/community/boards", "커뮤니티 게시판", "01", WorldZoneCodes.CommunityMarketSquare,
                new[] { PageProjectionTypeCodes.Object, PageProjectionTypeCodes.Panel }, "community-board", "browse-posts", "community-board-list", "community-board", WorldViewerScopeCodes.Public, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Placeholder),
            Define("/community/posts/{PostId:long}", "커뮤니티 게시물 상세", "01", WorldZoneCodes.CommunityMarketSquare,
                new[] { PageProjectionTypeCodes.Panel }, "community-board", "read-post", "community-post-detail", "community-post", WorldViewerScopeCodes.Public, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.PanelOnly),
            Define("/community/write", "커뮤니티 글쓰기", "01", WorldZoneCodes.CommunityMarketSquare,
                new[] { PageProjectionTypeCodes.Object, PageProjectionTypeCodes.Panel }, "community-writing-desk", "compose-post", "community-post-editor", "community-draft", WorldViewerScopeCodes.Personal, WorldInteractionEffectCodes.ServerCommand, PageProjectionStageCodes.OperationalCommand, true, true),
            Define("/community/regions", "지역문화 전시", "01", WorldZoneCodes.CommunityMarketSquare,
                new[] { PageProjectionTypeCodes.Spatial, PageProjectionTypeCodes.Object }, "regional-exhibition", "browse-regions", "regional-culture-list", "regional-culture", WorldViewerScopeCodes.Public, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Placeholder),
            Define("/information/public-data", "공공데이터 정보", "01", WorldZoneCodes.PublicDataHall,
                new[] { PageProjectionTypeCodes.Spatial, PageProjectionTypeCodes.Panel }, "public-data-kiosk", "browse-public-data", "public-data-detail", "public-evidence", WorldViewerScopeCodes.Public, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Placeholder),
            Define("/information/korea-agricultural-map", "한국 농업 정보 지도", "01", WorldZoneCodes.PublicDataHall,
                new[] { PageProjectionTypeCodes.Spatial, PageProjectionTypeCodes.Object, PageProjectionTypeCodes.Panel }, "agriculture-map-table", "inspect-observation", "agriculture-observation-detail", "agriculture-observation", WorldViewerScopeCodes.Public, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Projected),
            Define("/community/group-purchase", "공동구매 모집", "01", WorldZoneCodes.CooperativeHall,
                new[] { PageProjectionTypeCodes.Spatial, PageProjectionTypeCodes.Object, PageProjectionTypeCodes.Panel }, "collective-recruitment-board", "browse-collective-cases", "group-purchase-list", "group-purchase", WorldViewerScopeCodes.Public, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Placeholder),
            Define("/community/group-purchase/{CampaignId:guid}/participation", "공동구매 참여", "01", WorldZoneCodes.CooperativeHall,
                new[] { PageProjectionTypeCodes.Action, PageProjectionTypeCodes.Panel }, "cooperative-table", "confirm-participation", "group-purchase-participation", "group-purchase-participation", WorldViewerScopeCodes.AuthorizedParty, WorldInteractionEffectCodes.ServerCommand, PageProjectionStageCodes.OperationalCommand, true, true),
            Define("/orderer/mart", "주문자 마트", "02", WorldZoneCodes.MarketOrder,
                new[] { PageProjectionTypeCodes.Spatial, PageProjectionTypeCodes.Object }, "market-stall", "browse-products", "mart-catalog", "mart-product", WorldViewerScopeCodes.Public, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Placeholder),
            Define("/orderer/mart/order/{ProductId:long}", "상품 주문 요청", "02", WorldZoneCodes.MarketOrder,
                new[] { PageProjectionTypeCodes.Panel, PageProjectionTypeCodes.Action }, "order-desk", "request-order", "mart-order-request", "order-intent", WorldViewerScopeCodes.Personal, WorldInteractionEffectCodes.ServerCommand, PageProjectionStageCodes.OperationalCommand, true, true),
            Define("/shipper/request", "운송 요청", "03", WorldZoneCodes.UrbanLogisticsCenter,
                new[] { PageProjectionTypeCodes.Object, PageProjectionTypeCodes.Panel }, "transport-request-board", "prepare-transport-request", "transport-request", "transport-request", WorldViewerScopeCodes.Organization, WorldInteractionEffectCodes.ServerCommand, PageProjectionStageCodes.OperationalCommand, true, true),
            Define("/driver/transports/current", "현재 운송", "04", WorldZoneCodes.UrbanLogisticsCenter,
                new[] { PageProjectionTypeCodes.Spatial, PageProjectionTypeCodes.Object, PageProjectionTypeCodes.Panel }, "transport-truck", "inspect-current-transport", "current-transport", "transport", WorldViewerScopeCodes.AuthorizedParty, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Placeholder),
            Define("/driver/transports/{TransportId:long}/pickup", "운송 상차", "04", WorldZoneCodes.UrbanLogisticsCenter,
                new[] { PageProjectionTypeCodes.Action, PageProjectionTypeCodes.Panel }, "loading-zone", "confirm-pickup", "pickup-proof", "transport-pickup", WorldViewerScopeCodes.AuthorizedParty, WorldInteractionEffectCodes.ServerCommand, PageProjectionStageCodes.OperationalCommand, true, true),
            Define("/warehouse/inbounds/expected", "입고 예정", "05", WorldZoneCodes.Warehouse,
                new[] { PageProjectionTypeCodes.Spatial, PageProjectionTypeCodes.Object, PageProjectionTypeCodes.Panel }, "inbound-dock", "inspect-expected-inbound", "expected-inbound-list", "warehouse-inbound", WorldViewerScopeCodes.Organization, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Placeholder),
            Define("/warehouse/work/inbound/inspection/{InboundItemId:long}", "입고 검수", "05", WorldZoneCodes.Warehouse,
                new[] { PageProjectionTypeCodes.Action, PageProjectionTypeCodes.Panel }, "inspection-desk", "inspect-inbound-item", "inbound-inspection", "warehouse-inspection", WorldViewerScopeCodes.Organization, WorldInteractionEffectCodes.ServerCommand, PageProjectionStageCodes.OperationalCommand, true, true),
            Define("/warehouse/inventory", "창고 재고", "05", WorldZoneCodes.Warehouse,
                new[] { PageProjectionTypeCodes.Spatial, PageProjectionTypeCodes.Object, PageProjectionTypeCodes.Panel }, "warehouse-rack", "inspect-inventory", "warehouse-inventory", "inventory-lot", WorldViewerScopeCodes.Organization, WorldInteractionEffectCodes.ReadOnly, PageProjectionStageCodes.Placeholder),
            Define("/driver/account/bank", "운전자 계좌", "04", WorldZoneCodes.PersonalMeditation,
                new[] { PageProjectionTypeCodes.KeepWeb }, "web-handoff", "open-secure-web", "", "secure-account", WorldViewerScopeCodes.Personal, WorldInteractionEffectCodes.WebHandoff, PageProjectionStageCodes.KeepWeb),
        };

        public static IReadOnlyList<PageWorldProjectionDefinition> RepresentativeRoutes => RepresentativeDefinitions;

        public static PageWorldProjectionDefinition? Find(string routePattern)
        {
            return RepresentativeDefinitions.FirstOrDefault(
                item => string.Equals(item.RoutePattern, routePattern, StringComparison.OrdinalIgnoreCase));
        }

        public static string[] Validate()
        {
            var errors = new List<string>();
            var duplicatedRoutes = RepresentativeDefinitions
                .GroupBy(item => item.RoutePattern, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
            errors.AddRange(duplicatedRoutes.Select(route => $"DuplicateRoute:{route}"));

            foreach (var definition in RepresentativeDefinitions)
            {
                if (string.IsNullOrWhiteSpace(definition.RoutePattern) || definition.RoutePattern[0] != '/')
                {
                    errors.Add($"InvalidRoute:{definition.RoutePattern}");
                }

                if (!StableDataId.IsValid($"{definition.StableIdPrefix}:placeholder"))
                {
                    errors.Add($"InvalidStableIdPrefix:{definition.RoutePattern}");
                }

                if (definition.ProjectionTypeCodes.Length == 0)
                {
                    errors.Add($"ProjectionTypeMissing:{definition.RoutePattern}");
                }

                if (string.Equals(definition.InteractionEffectCode, WorldInteractionEffectCodes.ServerCommand, StringComparison.Ordinal)
                    && (!definition.RequiresExplicitConfirmation || !definition.RequiresCanonicalStateRefresh))
                {
                    errors.Add($"UnsafeServerCommandBoundary:{definition.RoutePattern}");
                }
            }

            return errors.ToArray();
        }

        private static PageWorldProjectionDefinition Define(
            string routePattern,
            string businessName,
            string roleCode,
            string worldZoneCode,
            string[] projectionTypeCodes,
            string worldObjectKey,
            string interactionCode,
            string panelCode,
            string stableIdPrefix,
            string viewerScopeCode,
            string interactionEffectCode,
            string projectionStageCode,
            bool requiresExplicitConfirmation = false,
            bool requiresCanonicalStateRefresh = false)
        {
            return new PageWorldProjectionDefinition
            {
                RoutePattern = routePattern,
                BusinessName = businessName,
                RoleCodes = new[] { roleCode },
                WorldZoneCode = worldZoneCode,
                ProjectionTypeCodes = projectionTypeCodes,
                WorldObjectKey = worldObjectKey,
                InteractionCode = interactionCode,
                PanelCode = panelCode,
                StableIdPrefix = stableIdPrefix,
                ViewerScopeCode = viewerScopeCode,
                InteractionEffectCode = interactionEffectCode,
                ProjectionStageCode = projectionStageCode,
                RequiresExplicitConfirmation = requiresExplicitConfirmation,
                RequiresCanonicalStateRefresh = requiresCanonicalStateRefresh,
            };
        }
    }
}
