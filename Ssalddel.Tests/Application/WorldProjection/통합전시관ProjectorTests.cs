using Ssalddel.Application.WorldProjection;
using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Tests.Application.WorldProjection;

public sealed class 통합전시관ProjectorTests
{
    private static readonly DateTimeOffset GeneratedAt =
        new(2026, 8, 11, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FixtureManifest는_여섯후보와_독립증거축을_보존한다()
    {
        var result = new 통합전시관Projector().Project(
            통합전시관FixtureCatalog.Create(GeneratedAt));

        Assert.True(result.IsSuccess);
        var manifest = result.Value;
        Assert.Equal("exhibition-manifest:integrated-seedbed", manifest.StableId);
        Assert.StartsWith("exhibition:", manifest.Revision);
        Assert.True(manifest.IsReadOnly);
        Assert.Equal(6, manifest.Exhibits.Count);
        Assert.Equal(manifest.Exhibits.Select(value => value.ExhibitStableId),
            manifest.Stories.Select(value => value.ExhibitStableId));
        Assert.Equal(15, manifest.SeedbedObjects.Count);
        Assert.Equal(7, manifest.ScenePlacements.Count);
        var harvestPlacement = manifest.ScenePlacements.Single(value =>
            value.ObjectStableId == "seedbed-object:farm.potato-harvest-box.a");
        Assert.Equal("district:farm", harvestPlacement.ZoneStableId);
        var hubPlacement = manifest.ScenePlacements.Single(value =>
            value.ObjectStableId == "seedbed-object:town.hub-inbound-gate.a");
        Assert.Equal("district:logistics", hubPlacement.ZoneStableId);
        Assert.Equal("HubReceiving:hub-receiving:sim.potato", hubPlacement.DataBindingKey);
        var truckPlacement = manifest.ScenePlacements.Single(value =>
            value.ObjectStableId == "seedbed-object:town.delivery-truck.a");
        Assert.Equal("district:logistics", truckPlacement.ZoneStableId);
        Assert.Equal("CargoJourney:cargo-journey:sim.potato.farm-hub",
            truckPlacement.DataBindingKey);
        var palletPlacement = manifest.ScenePlacements.Single(value =>
            value.ObjectStableId == "seedbed-object:shared.cargo-pallet.a");
        Assert.Equal("district:logistics", palletPlacement.ZoneStableId);
        Assert.Equal("WarehouseHandoff:cargo-handoff:sim.potato.20260407.r3.inbound-91",
            palletPlacement.DataBindingKey);
        var farmPalletCratePlacement = manifest.ScenePlacements.Single(value =>
            value.ObjectStableId == "seedbed-object:farm.pallet-crate.a");
        Assert.Equal("district:farm", farmPalletCratePlacement.ZoneStableId);
        Assert.Equal("CanonicalProductHarvestCargo:cargo:sim.potato.20260407.r3",
            farmPalletCratePlacement.DataBindingKey);
        var marketShopPlacement = manifest.ScenePlacements.Single(value =>
            value.ObjectStableId == "seedbed-object:city.urban-market-building.a");
        Assert.Equal("district:market", marketShopPlacement.ZoneStableId);
        Assert.Equal("MartPublicProduct:mart-product:sim.potato.public",
            marketShopPlacement.DataBindingKey);
        Assert.DoesNotContain("MarketInventory", marketShopPlacement.DataBindingKey);
        var groupingCartPlacement = manifest.ScenePlacements.Single(value =>
            value.ObjectStableId == "seedbed-object:town.grouping-cart-table.a");
        Assert.Equal("district:town", groupingCartPlacement.ZoneStableId);
        Assert.Equal("GroupingPreview:grouping-preview:sim.potato.town",
            groupingCartPlacement.DataBindingKey);
        Assert.DoesNotContain("IndividualIntent", groupingCartPlacement.DataBindingKey);
        Assert.All(manifest.ScenePlacements, placement =>
        {
            Assert.Equal("scene:simulation-world-shell", placement.SceneStableId);
            Assert.Equal(통합전시관ObjectGateStateCodes.PromotedToScene,
                placement.ValidationStatusCode);
        });
        Assert.All(manifest.SeedbedObjects.Where(value =>
            value.ObjectStableId is not "seedbed-object:town.delivery-truck.a"
                and not "seedbed-object:shared.cargo-pallet.a"
                and not "seedbed-object:farm.pallet-crate.a"
                and not "seedbed-object:town.resident-visual.a"
                and not "seedbed-object:town.grouping-cart-table.a"
                and not "seedbed-object:city.urban-market-building.a"
                and not "seedbed-object:city.operator-inventory-shelf.a"
                and not "seedbed-object:city.market-operator-visual.a"), value =>
        {
            Assert.Contains(value.Evidence, evidence =>
                evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ObjectPreview
                && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
                && evidence.Reference == "unity-change:2026-08-11-integrated-object-seedbed-obj4");
        });
        Assert.All(manifest.SeedbedObjects.Where(value =>
            value.ObjectStableId is "seedbed-object:town.delivery-truck.a"
                or "seedbed-object:shared.cargo-pallet.a"
                or "seedbed-object:farm.pallet-crate.a"), value =>
        {
            Assert.Contains(value.Evidence, evidence =>
                evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ObjectPreview
                && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
                && evidence.Reference ==
                "unity-change:2026-08-12-integrated-logistics-object-seedbed-obj6b");
        });
        Assert.All(manifest.SeedbedObjects.Where(value =>
            value.ObjectStableId is "seedbed-object:town.resident-visual.a"
                or "seedbed-object:town.grouping-cart-table.a"
                or "seedbed-object:city.urban-market-building.a"
                or "seedbed-object:city.operator-inventory-shelf.a"
                or "seedbed-object:city.market-operator-visual.a"), value =>
        {
            Assert.Contains(value.Evidence, evidence =>
                evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ObjectPreview
                && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
                && evidence.Reference ==
                "unity-change:2026-08-12-integrated-orderer-market-object-seedbed-obj7a");
        });
        var promoted = manifest.SeedbedObjects.Where(value =>
            value.GateStateCode == 통합전시관ObjectGateStateCodes.PromotedToScene).ToArray();
        Assert.Equal(7, promoted.Length);
        Assert.All(promoted, value => Assert.Empty(value.BlockedReasonCodes));
        Assert.Contains(promoted.Single(value =>
            value.ObjectStableId == "seedbed-object:farm.potato-harvest-box.a").Evidence, evidence =>
            evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement
            && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
            && evidence.Reference == "unity-change:2026-08-12-integrated-object-scene-placement-obj5");
        Assert.Contains(promoted.Single(value =>
            value.ObjectStableId == "seedbed-object:town.hub-inbound-gate.a").Evidence, evidence =>
            evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement
            && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
            && evidence.Reference == "unity-change:2026-08-12-integrated-hub-scene-placement-obj6");
        Assert.Contains(promoted.Single(value =>
            value.ObjectStableId == "seedbed-object:town.delivery-truck.a").Evidence, evidence =>
            evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement
            && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
            && evidence.Reference ==
            "unity-change:2026-08-12-integrated-delivery-truck-scene-placement-obj6c");
        Assert.Contains(promoted.Single(value =>
            value.ObjectStableId == "seedbed-object:shared.cargo-pallet.a").Evidence, evidence =>
            evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement
            && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
            && evidence.Reference ==
            "unity-change:2026-08-12-integrated-cargo-pallet-scene-placement-obj6d1");
        Assert.Contains(promoted.Single(value =>
            value.ObjectStableId == "seedbed-object:farm.pallet-crate.a").Evidence, evidence =>
            evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement
            && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
            && evidence.Reference ==
            "unity-change:2026-08-12-integrated-farm-pallet-crate-scene-placement-obj6d2");
        Assert.Contains(promoted.Single(value =>
            value.ObjectStableId == "seedbed-object:city.urban-market-building.a").Evidence, evidence =>
            evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement
            && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
            && evidence.Reference ==
            "unity-change:2026-08-12-integrated-urban-market-shop-scene-placement-obj7b");
        Assert.Contains(promoted.Single(value =>
            value.ObjectStableId == "seedbed-object:town.grouping-cart-table.a").Evidence, evidence =>
            evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement
            && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Verified
            && evidence.Reference ==
            "unity-change:2026-08-12-integrated-grouping-cart-table-scene-placement-obj7c");
        Assert.All(manifest.SeedbedObjects.Where(value => !promoted.Contains(value)), value =>
        {
            Assert.Equal(통합전시관ObjectGateStateCodes.RuntimeVerified, value.GateStateCode);
            Assert.Contains(value.BlockedReasonCodes, code => code == "TargetScenePlacementNotPromoted");
            Assert.Contains(value.Evidence, evidence =>
                evidence.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement
                && evidence.StatusCode == 통합전시관EvidenceStatusCodes.Unverified);
        });
        var logisticsObjects = manifest.SeedbedObjects.Where(value =>
            value.ObjectStableId is "seedbed-object:town.delivery-truck.a"
                or "seedbed-object:shared.cargo-pallet.a"
                or "seedbed-object:farm.pallet-crate.a").ToArray();
        Assert.Equal(3, logisticsObjects.Length);
        Assert.Contains(logisticsObjects.Single(value =>
                value.ObjectStableId == "seedbed-object:town.delivery-truck.a").RequiredSocketCodes,
            value => value == "RouteEntry");
        Assert.Contains(logisticsObjects.Single(value =>
                value.ObjectStableId == "seedbed-object:shared.cargo-pallet.a").DataBindingKeys,
            value => value == "WarehouseHandoff");
        Assert.Contains(logisticsObjects.Single(value =>
                value.ObjectStableId == "seedbed-object:farm.pallet-crate.a").DataBindingKeys,
            value => value == "CanonicalProductHarvestCargo");
        Assert.All(manifest.Exhibits, exhibit => Assert.Equal(4, exhibit.Evidence.Count));
        Assert.Equal(
            통합전시관DataStateCodes.Uncollected,
            manifest.Exhibits.Single(value => value.ExhibitStableId == "exhibit:public-data:potato-observation").DataStateCode);
    }

    [Fact]
    public void O6_Object의_승격Placement가없으면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt) with
        {
            ScenePlacements = [],
        };

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionPromotedObjectPlacementMissing:seedbed-object:farm.potato-harvest-box.a",
            result.Errors[0].Message);
    }

    [Fact]
    public void O6_HubGate의_개별Placement가없으면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        input = input with
        {
            ScenePlacements = input.ScenePlacements.Where(value =>
                value.ObjectStableId != "seedbed-object:town.hub-inbound-gate.a").ToArray(),
        };

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionPromotedObjectPlacementMissing:seedbed-object:town.hub-inbound-gate.a",
            result.Errors[0].Message);
    }

    [Fact]
    public void O6_배송차량의_CargoJourneyPlacement가없으면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        input = input with
        {
            ScenePlacements = input.ScenePlacements.Where(value =>
                value.ObjectStableId != "seedbed-object:town.delivery-truck.a").ToArray(),
        };

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionPromotedObjectPlacementMissing:seedbed-object:town.delivery-truck.a",
            result.Errors[0].Message);
    }

    [Fact]
    public void O6_공용Pallet의_WarehouseHandoffPlacement가없으면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        input = input with
        {
            ScenePlacements = input.ScenePlacements.Where(value =>
                value.ObjectStableId != "seedbed-object:shared.cargo-pallet.a").ToArray(),
        };

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionPromotedObjectPlacementMissing:seedbed-object:shared.cargo-pallet.a",
            result.Errors[0].Message);
    }

    [Fact]
    public void O6_농장PalletCrate의_HarvestCargoPlacement가없으면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        input = input with
        {
            ScenePlacements = input.ScenePlacements.Where(value =>
                value.ObjectStableId != "seedbed-object:farm.pallet-crate.a").ToArray(),
        };

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionPromotedObjectPlacementMissing:seedbed-object:farm.pallet-crate.a",
            result.Errors[0].Message);
    }

    [Fact]
    public void O6_도심마트Shop의_공개상품Placement가없으면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        input = input with
        {
            ScenePlacements = input.ScenePlacements.Where(value =>
                value.ObjectStableId != "seedbed-object:city.urban-market-building.a").ToArray(),
        };

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionPromotedObjectPlacementMissing:seedbed-object:city.urban-market-building.a",
            result.Errors[0].Message);
    }

    [Fact]
    public void O6_GroupingCartTable의_개인정보제거PreviewPlacement가없으면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        input = input with
        {
            ScenePlacements = input.ScenePlacements.Where(value =>
                value.ObjectStableId != "seedbed-object:town.grouping-cart-table.a").ToArray(),
        };

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionPromotedObjectPlacementMissing:seedbed-object:town.grouping-cart-table.a",
            result.Errors[0].Message);
    }

    [Fact]
    public void Story가_등록되지않은모판Object를참조하면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        input.Exhibits[0].ReferencedSeedbedObjectStableIds = ["seedbed-object:missing.a"];

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionStoryObjectReferenceMissing:exhibit:asset-lab:synty:seedbed-object:missing.a",
            result.Errors[0].Message);
    }

    [Fact]
    public void O4_Object는_BindingValidationVerified증거를_필수로한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        var seedbedObject = input.SeedbedObjects[0];
        seedbedObject.Evidence.Single(value =>
            value.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.BindingValidation).StatusCode =
            통합전시관EvidenceStatusCodes.Unverified;

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionSeedbedObjectGateEvidenceRequired:"
            + seedbedObject.ObjectStableId + ":BindingValidation",
            result.Errors[0].Message);
    }

    [Fact]
    public void 모판Object공통계약에_UnityPrefab경로를넣으면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        input.SeedbedObjects[0].VisualVariantKeys = ["Assets/Synty/Farm/Potato.prefab"];

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionSeedbedObjectUnityAssetLocatorForbidden:"
            + input.SeedbedObjects[0].ObjectStableId,
            result.Errors[0].Message);
    }

    [Fact]
    public void EXH4는_개인의향_공개집계_마트공개_운영재고를_분리한다()
    {
        var result = new 통합전시관Projector().Project(
            통합전시관FixtureCatalog.Create(GeneratedAt));

        Assert.True(result.IsSuccess);
        var exhibit = result.Value.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:town-city:orderer-group-urban-market");
        Assert.Equal(6, exhibit.CanonicalRecordRelations.Count);
        Assert.Equal(6, exhibit.WorkflowCheckpoints.Count);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "IndividualIntent"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.OwnerPrivate
            && value.RequiresSeparateConfirmation);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "GroupingPreview"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.PrivacySafeAggregate);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "MartPublicProduct"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.OrdererPublic);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "MarketInventory"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized);
        Assert.DoesNotContain(통합전시관InteractionIntentCodes.DomainCommand,
            exhibit.AllowedInteractionIntentCodes);
        Assert.Contains(exhibit.CanonicalRecordRelations, value =>
            value.SourceRecordKindCode == "KamisObservation"
            && value.RelationCode == "ComparedWithNotUsedAsSalePrice");
    }

    [Fact]
    public void OBJ7A는_EXH4의_주민CartShopShelf운영자를_권위별Object로분리한다()
    {
        var result = new 통합전시관Projector().Project(
            통합전시관FixtureCatalog.Create(GeneratedAt));

        Assert.True(result.IsSuccess);
        var manifest = result.Value;
        var exhibit = manifest.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:town-city:orderer-group-urban-market");
        var objects = exhibit.ReferencedSeedbedObjectStableIds.Select(stableId =>
            manifest.SeedbedObjects.Single(value => value.ObjectStableId == stableId)).ToArray();

        Assert.Equal(5, objects.Length);
        Assert.All(objects.Where(value =>
            value.ObjectStableId is not "seedbed-object:city.urban-market-building.a"
                and not "seedbed-object:town.grouping-cart-table.a"), value =>
        {
            Assert.Equal(통합전시관ObjectGateStateCodes.RuntimeVerified, value.GateStateCode);
            Assert.Contains("TargetScenePlacementNotPromoted", value.BlockedReasonCodes);
        });
        var resident = objects.Single(value =>
            value.ObjectStableId == "seedbed-object:town.resident-visual.a");
        Assert.Contains("IndividualIntent", resident.DataBindingKeys);
        Assert.DoesNotContain("MarketInventory", resident.DataBindingKeys);
        var cart = objects.Single(value =>
            value.ObjectStableId == "seedbed-object:town.grouping-cart-table.a");
        Assert.Equal(통합전시관ObjectGateStateCodes.PromotedToScene, cart.GateStateCode);
        Assert.Empty(cart.BlockedReasonCodes);
        Assert.Contains("GroupingPreview", cart.DataBindingKeys);
        Assert.Contains("OrdererGroupSummary", cart.DataBindingKeys);
        Assert.DoesNotContain("IndividualIntent", cart.DataBindingKeys);
        Assert.Contains("ConsentBoundary", cart.RequiredSocketCodes);
        var shop = objects.Single(value =>
            value.ObjectStableId == "seedbed-object:city.urban-market-building.a");
        Assert.Equal(통합전시관ObjectGateStateCodes.PromotedToScene, shop.GateStateCode);
        Assert.Empty(shop.BlockedReasonCodes);
        Assert.Contains("MartPublicProduct", shop.DataBindingKeys);
        Assert.DoesNotContain("MarketInventory", shop.DataBindingKeys);
        var shelf = objects.Single(value =>
            value.ObjectStableId == "seedbed-object:city.operator-inventory-shelf.a");
        Assert.Contains("MarketInventory", shelf.DataBindingKeys);
        Assert.DoesNotContain("MartPublicProduct", shelf.DataBindingKeys);
        var operatorVisual = objects.Single(value =>
            value.ObjectStableId == "seedbed-object:city.market-operator-visual.a");
        Assert.Contains("MarketOperatorPerspective", operatorVisual.DataBindingKeys);
        Assert.DoesNotContain("IndividualIntent", operatorVisual.DataBindingKeys);
    }

    [Fact]
    public void EXH4의_공개상품과운영재고가_같은원장이면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        var exhibit = input.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:town-city:orderer-group-urban-market");
        var publicProduct = exhibit.WorkflowCheckpoints.Single(value =>
            value.StateMachineCode == "MartPublicProduct");
        exhibit.WorkflowCheckpoints.Single(value =>
            value.StateMachineCode == "MarketInventory").CanonicalRecordStableId =
            publicProduct.CanonicalRecordStableId;

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionOrdererMarketAuthorityBoundaryInvalid:exhibit:town-city:orderer-group-urban-market",
            result.Errors[0].Message);
    }

    [Fact]
    public void EXH5는_음식주문부터_기사전달과_주문자수령을_별도경계로보존한다()
    {
        var result = new 통합전시관Projector().Project(
            통합전시관FixtureCatalog.Create(GeneratedAt));

        Assert.True(result.IsSuccess);
        var exhibit = result.Value.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:city:food-delivery");
        Assert.Equal(7, exhibit.CanonicalRecordRelations.Count);
        Assert.Equal(8, exhibit.WorkflowCheckpoints.Count);
        Assert.DoesNotContain(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode is "CargoJourney" or "WarehouseHandoff");
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "DriverOffer"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.DriverCandidateApproximate);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "DriverAssignment"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized
            && value.RequiresSeparateConfirmation);
        var delivered = exhibit.WorkflowCheckpoints.Single(value => value.StateCode == "전달완료");
        var receipt = exhibit.WorkflowCheckpoints.Single(value => value.StateCode == "수령확인");
        Assert.NotEqual(delivered.CanonicalRecordStableId, receipt.CanonicalRecordStableId);
        Assert.True(delivered.RequiresSeparateConfirmation);
        Assert.True(receipt.RequiresSeparateConfirmation);
    }

    [Fact]
    public void EXH5에서_전달완료와수령확인을_같은원장으로합치면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        var exhibit = input.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:city:food-delivery");
        var delivered = exhibit.WorkflowCheckpoints.Single(value => value.StateCode == "전달완료");
        exhibit.WorkflowCheckpoints.Single(value => value.StateCode == "수령확인")
            .CanonicalRecordStableId = delivered.CanonicalRecordStableId;

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionFoodDeliveryHandoffBoundaryInvalid:exhibit:city:food-delivery",
            result.Errors[0].Message);
    }

    [Fact]
    public void EXH3는_같은Cargo계보와ExpectedRevision과_별도창고인수상태를_보존한다()
    {
        var result = new 통합전시관Projector().Project(
            통합전시관FixtureCatalog.Create(GeneratedAt));

        Assert.True(result.IsSuccess);
        var exhibit = result.Value.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:logistics:cargo-hub-warehouse");
        Assert.Equal(5, exhibit.CanonicalRecordRelations.Count);
        Assert.Equal(7, exhibit.WorkflowCheckpoints.Count);
        Assert.Single(exhibit.WorkflowCheckpoints.Select(value => value.LineageStableId).Distinct());
        Assert.All(exhibit.CanonicalRecordRelations,
            relation => Assert.False(string.IsNullOrWhiteSpace(relation.ExpectedTargetRevision)));
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "CargoJourney" && value.StateCode == "ArrivedAtHub");
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "WarehouseHandoff"
            && value.StateCode == "ArrivedAtWarehouse"
            && value.RequiresSeparateConfirmation);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "WarehouseHandoff" && value.StateCode == "ReceivingCompleted");
        Assert.Equal(통합전시관EvidenceStatusCodes.Partial,
            exhibit.Evidence.Single(value => value.EvidenceKindCode == 통합전시관EvidenceKindCodes.Operational).StatusCode);
    }

    [Fact]
    public void EXH3의_CargoLineage가갈라지면_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        var exhibit = input.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:logistics:cargo-hub-warehouse");
        exhibit.WorkflowCheckpoints[5].LineageStableId = "cargo:other";

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionCargoLineageMismatch:exhibit:logistics:cargo-hub-warehouse",
            result.Errors[0].Message);
    }

    [Fact]
    public void 입력순서와무관하게_동일Revision과Stable순서를_만든다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        var reversed = new 통합전시관ProjectionInput(
            input.Exhibits.Reverse().ToArray(),
            GeneratedAt)
        {
            SeedbedObjects = input.SeedbedObjects.Reverse().ToArray(),
            ScenePlacements = input.ScenePlacements,
        };
        var projector = new 통합전시관Projector();

        var first = projector.Project(input).Value;
        var second = projector.Project(reversed).Value;

        Assert.Equal(first.Revision, second.Revision);
        Assert.Equal(
            first.Exhibits.Select(value => value.ExhibitStableId),
            second.Exhibits.Select(value => value.ExhibitStableId));
    }

    [Fact]
    public void 중복ExhibitStableId를_거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        var duplicate = new 통합전시관ProjectionInput(
            [input.Exhibits[0], input.Exhibits[0]],
            GeneratedAt);

        var result = new 통합전시관Projector().Project(duplicate);

        Assert.True(result.IsFailed);
        Assert.Equal("IntegratedExhibitionDuplicate:exhibit:asset-lab:synty", result.Errors[0].Message);
    }

    [Fact]
    public void 연구전시에_GenericConfirm을_허용하지않는다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        input.Exhibits[0].AllowedInteractionIntentCodes = ["Observe", "ConfirmExhibit"];

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionGenericConfirmForbidden:exhibit:asset-lab:synty",
            result.Errors[0].Message);
    }

    [Fact]
    public void Live상태가_FixtureSource를_사칭하면거부한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        var lifecycle = input.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:farm:potato-lifecycle");
        lifecycle.DataStateCode = 통합전시관DataStateCodes.Live;

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionLiveFixtureContradiction:exhibit:farm:potato-lifecycle",
            result.Errors[0].Message);
    }

    [Fact]
    public void 미수집상태는_차단사유를_필수로한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        var observation = input.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:public-data:potato-observation");
        observation.BlockedReasonCodes = [];

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionUncollectedReasonRequired:exhibit:public-data:potato-observation",
            result.Errors[0].Message);
    }

    [Fact]
    public void Live표시는_운영Verified증거를_필수로한다()
    {
        var input = 통합전시관FixtureCatalog.Create(GeneratedAt);
        var observation = input.Exhibits.Single(value =>
            value.ExhibitStableId == "exhibit:public-data:potato-observation");
        observation.DataStateCode = 통합전시관DataStateCodes.Live;

        var result = new 통합전시관Projector().Project(input);

        Assert.True(result.IsFailed);
        Assert.Equal(
            "IntegratedExhibitionLiveOperationalEvidenceRequired:exhibit:public-data:potato-observation",
            result.Errors[0].Message);
    }
}
