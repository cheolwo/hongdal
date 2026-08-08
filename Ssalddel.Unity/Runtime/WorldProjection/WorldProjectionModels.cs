using System;

namespace Ssalddel.Unity.WorldProjection
{
    public static class WorldZoneCodes
    {
        public const string CommunityMarketSquare = "community-market-square";
        public const string PublicDataHall = "public-data-hall";
        public const string Farm = "farm";
        public const string CooperativeHall = "cooperative-hall";
        public const string MarketOrder = "market-order";
        public const string ResidentialCommunity = "residential-community";
        public const string TraditionalMarket = "traditional-market";
        public const string UrbanLogisticsCenter = "urban-logistics-center";
        public const string TransportNetwork = "transport-network";
        public const string Warehouse = "warehouse";
        public const string PersonalMeditation = "personal-meditation";
    }

    public static class PageProjectionTypeCodes
    {
        public const string Spatial = "Spatial";
        public const string Object = "Object";
        public const string Panel = "Panel";
        public const string Action = "Action";
        public const string KeepWeb = "KeepWeb";
    }

    public static class PageProjectionStageCodes
    {
        public const string KeepWeb = "KeepWeb";
        public const string PanelOnly = "PanelOnly";
        public const string Placeholder = "Placeholder";
        public const string Projected = "Projected";
        public const string InteractiveSimulation = "InteractiveSimulation";
        public const string OperationalCommand = "OperationalCommand";
    }

    public static class WorldViewerScopeCodes
    {
        public const string Public = "Public";
        public const string Personal = "Personal";
        public const string Organization = "Organization";
        public const string AuthorizedParty = "AuthorizedParty";
        public const string Operator = "Operator";
    }

    public static class WorldInteractionEffectCodes
    {
        public const string ReadOnly = "ReadOnly";
        public const string LocalSimulation = "LocalSimulation";
        public const string ServerCommand = "ServerCommand";
        public const string WebHandoff = "WebHandoff";
    }

    public sealed class PageWorldProjectionDefinition
    {
        public string RoutePattern { get; set; } = string.Empty;

        public string BusinessName { get; set; } = string.Empty;

        public string[] RoleCodes { get; set; } = Array.Empty<string>();

        public string WorldZoneCode { get; set; } = string.Empty;

        public string[] ProjectionTypeCodes { get; set; } = Array.Empty<string>();

        public string WorldObjectKey { get; set; } = string.Empty;

        public string InteractionCode { get; set; } = string.Empty;

        public string PanelCode { get; set; } = string.Empty;

        public string StableIdPrefix { get; set; } = string.Empty;

        public string ViewerScopeCode { get; set; } = string.Empty;

        public string InteractionEffectCode { get; set; } = WorldInteractionEffectCodes.ReadOnly;

        public string ProjectionStageCode { get; set; } = PageProjectionStageCodes.Placeholder;

        public bool RequiresExplicitConfirmation { get; set; }

        public bool RequiresCanonicalStateRefresh { get; set; }
    }

    public sealed class WorldObjectProjection
    {
        public string StableId { get; set; } = string.Empty;

        public long Revision { get; set; }

        public string WorldZoneCode { get; set; } = string.Empty;

        public string WorldObjectKey { get; set; } = string.Empty;

        public string DisplayStateCode { get; set; } = string.Empty;

        public string DataStatusCode { get; set; } = string.Empty;

        public string[] EvidenceCardIds { get; set; } = Array.Empty<string>();

        public DateTimeOffset ProjectedAt { get; set; }
    }
}
