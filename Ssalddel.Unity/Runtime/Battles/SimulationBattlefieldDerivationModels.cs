using System;
using System.Linq;

namespace Ssalddel.Unity.Battles
{
    public static class BattlefieldPresentationCodes
    {
        public const string BattleLocalMeters = "BattleLocalMeters";
        public const string Required = "Required";
        public const string Preferred = "Preferred";
        public const string Allied = "Allied";
        public const string Hostile = "Hostile";
    }

    public sealed class BattleSpatialPoseApiModel
    {
        public string CoordinateSpaceCode { get; set; } = string.Empty;
        public double XMeters { get; set; }
        public double ZMeters { get; set; }
        public double RotationDegrees { get; set; }
    }

    public sealed class BattlefieldAnchorApiModel
    {
        public string BattlefieldAnchorStableId { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string WorldEffectTargetStableId { get; set; } = string.Empty;
        public string SemanticCode { get; set; } = string.Empty;
        public string[] AnchorTypeCodes { get; set; } = Array.Empty<string>();
        public string PreservationPolicyCode { get; set; } = string.Empty;
    }

    public sealed class BattlefieldAnchorPlacementApiModel
    {
        public string BattlefieldAnchorStableId { get; set; } = string.Empty;
        public BattleSpatialPoseApiModel BattlePose { get; set; } = new();
        public double WidthMeters { get; set; }
        public double DepthMeters { get; set; }
        public string SizeVariantCode { get; set; } = string.Empty;
    }

    public sealed class BattlefieldTerrainCellApiModel
    {
        public int CellX { get; set; }
        public int CellZ { get; set; }
        public int HeightCentimeters { get; set; }
        public int MovementCostPermille { get; set; }
        public string TerrainCode { get; set; } = string.Empty;
        public bool Walkable { get; set; }
    }

    public sealed class BattlefieldZoneApiModel
    {
        public string ZoneStableId { get; set; } = string.Empty;
        public string ZoneKindCode { get; set; } = string.Empty;
        public BattleSpatialPoseApiModel CenterPose { get; set; } = new();
        public double WidthMeters { get; set; }
        public double DepthMeters { get; set; }
        public string SourceAnchorStableId { get; set; } = string.Empty;
    }

    public sealed class BattleWorldContextApiModel
    {
        public string ContextStableId { get; set; } = string.Empty;
        public double ContextWidthMeters { get; set; }
        public double ContextDepthMeters { get; set; }
        public BattlefieldAnchorApiModel[] Anchors { get; set; }
            = Array.Empty<BattlefieldAnchorApiModel>();
        public string ContextHashSha256 { get; set; } = string.Empty;
        public string AnchorSetHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class BattlefieldPlanApiModel
    {
        public string BattlefieldPlanStableId { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public string GeneratorRevision { get; set; } = string.Empty;
        public string CoordinateSpaceCode { get; set; } = string.Empty;
        public double WidthMeters { get; set; }
        public double DepthMeters { get; set; }
        public double GridCellSizeMeters { get; set; }
        public string BattlefieldDerivationInputHashSha256 { get; set; } = string.Empty;
        public BattlefieldAnchorPlacementApiModel[] AnchorPlacements { get; set; }
            = Array.Empty<BattlefieldAnchorPlacementApiModel>();
        public BattlefieldZoneApiModel[] Zones { get; set; }
            = Array.Empty<BattlefieldZoneApiModel>();
        public BattlefieldTerrainCellApiModel[] TerrainCells { get; set; }
            = Array.Empty<BattlefieldTerrainCellApiModel>();
        public string[] ValidationCodes { get; set; } = Array.Empty<string>();
        public string BattlefieldPlanHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class BattlefieldDerivationApiModel
    {
        public BattleWorldContextApiModel WorldContext { get; set; } = new();
        public BattlefieldPlanApiModel BattlefieldPlan { get; set; } = new();
        public string BattlefieldDerivationInputHashSha256 { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class BattleUnitApiModel
    {
        public string UnitStableId { get; set; } = string.Empty;
        public string SideCode { get; set; } = string.Empty;
        public string[] MemberActorStableIds { get; set; } = Array.Empty<string>();
        public string ThreatTypeCode { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int CombatStrength { get; set; }
        public string FormationCode { get; set; } = string.Empty;
        public BattleSpatialPoseApiModel InitialPose { get; set; } = new();
    }

    public sealed class BattleCardModifierApiModel
    {
        public string CardCopyStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ModifierCode { get; set; } = string.Empty;
        public int BasisPoints { get; set; }
    }

    public sealed class BattleUnitRosterApiModel
    {
        public string CombatSimulationRevision { get; set; } = string.Empty;
        public BattleUnitApiModel[] Units { get; set; } = Array.Empty<BattleUnitApiModel>();
        public BattleCardModifierApiModel[] CardModifiers { get; set; }
            = Array.Empty<BattleCardModifierApiModel>();
        public string BattleUnitRosterHashSha256 { get; set; } = string.Empty;
        public string CardModifierHashSha256 { get; set; } = string.Empty;
        public string CombatSeedHashSha256 { get; set; } = string.Empty;
    }

    public sealed class BattleCreatePreviewApiModel
    {
        public long WorldRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
        public BattlefieldDerivationApiModel BattlefieldDerivation { get; set; } = new();
        public BattleUnitRosterApiModel UnitRoster { get; set; } = new();
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class BattleCreateConfirmDraft
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public string ExpectedBattleWorldContextHashSha256 { get; set; } = string.Empty;
        public string ExpectedBattlefieldDerivationInputHashSha256 { get; set; }
            = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string RequestingActorStableId { get; set; } = string.Empty;
    }

    public static class BattleCreateCommandFactory
    {
        public static BattleCreateConfirmDraft Create(BattleCreatePreviewApiModel preview,
            string commandId, string actorStableId)
        {
            if (preview == null || !preview.CanConfirm
                || preview.BattlefieldDerivation == null
                || !preview.BattlefieldDerivation.CanConfirm
                || preview.BattlefieldDerivation.BlockingReasonCodes.Any()
                || string.IsNullOrWhiteSpace(preview.BattlefieldDerivation.WorldContext
                    .ContextHashSha256)
                || string.IsNullOrWhiteSpace(preview.BattlefieldDerivation
                    .BattlefieldDerivationInputHashSha256)
                || string.IsNullOrWhiteSpace(commandId)
                || string.IsNullOrWhiteSpace(actorStableId))
                throw new InvalidOperationException("BattleCreatePreviewNotConfirmable");
            return new BattleCreateConfirmDraft
            {
                CommandId = commandId.Trim(),
                ExpectedWorldRevision = preview.WorldRevision,
                ExpectedBattleWorldContextHashSha256 = preview
                    .BattlefieldDerivation.WorldContext.ContextHashSha256,
                ExpectedBattlefieldDerivationInputHashSha256 = preview
                    .BattlefieldDerivation.BattlefieldDerivationInputHashSha256,
                EncounterStableId = preview.EncounterStableId,
                RequestingActorStableId = actorStableId.Trim(),
            };
        }
    }
}
