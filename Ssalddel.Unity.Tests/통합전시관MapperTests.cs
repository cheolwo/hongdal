using Ssalddel.Unity.Exhibition;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 통합전시관MapperTests
{
    [Fact]
    public void Mapper는_미수집상태와StableId계보와네증거축을_보존한다()
    {
        var result = new 통합전시관Mapper().Map(Manifest());

        var exhibit = Assert.Single(result.Exhibits);
        Assert.Equal("exhibit:public-data:potato-observation", exhibit.ExhibitStableId);
        Assert.Equal(통합전시관DataStateCodes.Uncollected, exhibit.DataStateCode);
        Assert.Equal("world:integrated-seedbed-exhibition:fixture", exhibit.WorldStableId.Value);
        Assert.Equal("source:public-data:kamis-potato", Assert.Single(exhibit.SourcePlan).SourceStableId.Value);
        Assert.Equal("product:potato", Assert.Single(exhibit.CanonicalRecordRelations).SourceStableId);
        Assert.Equal(4, exhibit.Evidence.Length);
        Assert.Contains("ActualObservationNotCollected", exhibit.BlockedReasonCodes);
    }

    [Fact]
    public void 중복ExhibitStableId를_거부한다()
    {
        var source = Manifest();
        source.Exhibits = new[] { source.Exhibits[0], source.Exhibits[0] };

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionDuplicate:exhibit:public-data:potato-observation",
            error.Message);
    }

    [Fact]
    public void 읽기전시에_GenericConfirm을_허용하지않는다()
    {
        var source = Manifest();
        source.Exhibits[0].AllowedInteractionIntentCodes = new[] { "Observe", "ConfirmExhibit" };

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionGenericConfirmForbidden:exhibit:public-data:potato-observation",
            error.Message);
    }

    [Fact]
    public void Live상태와_FixtureSource의모순을_거부한다()
    {
        var source = Manifest();
        source.Exhibits[0].DataStateCode = 통합전시관DataStateCodes.Live;
        source.Exhibits[0].SourcePlan[0].SourceModeCode = "SimulationFixture";

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionLiveFixtureContradiction:exhibit:public-data:potato-observation",
            error.Message);
    }

    [Fact]
    public void 네Evidence축중하나가없으면_거부한다()
    {
        var source = Manifest();
        source.Exhibits[0].Evidence = source.Exhibits[0].Evidence.Take(3).ToArray();

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionEvidenceAxesInvalid:exhibit:public-data:potato-observation",
            error.Message);
    }

    [Fact]
    public void Live표시는_운영Verified증거를_필수로한다()
    {
        var source = Manifest();
        source.Exhibits[0].DataStateCode = 통합전시관DataStateCodes.Live;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionLiveOperationalEvidenceRequired:exhibit:public-data:potato-observation",
            error.Message);
    }

    [Fact]
    public void EXH3는_Cargo계보와ExpectedRevision과_별도인수Checkpoint를_보존한다()
    {
        var source = Manifest();
        source.Exhibits = new[] { CargoHubWarehouse() };

        var result = new 통합전시관Mapper().Map(source);

        var exhibit = Assert.Single(result.Exhibits);
        Assert.Equal(5, exhibit.CanonicalRecordRelations.Length);
        Assert.Equal(7, exhibit.WorkflowCheckpoints.Length);
        Assert.Single(exhibit.WorkflowCheckpoints.Select(value => value.LineageStableId).Distinct());
        Assert.All(exhibit.CanonicalRecordRelations,
            relation => Assert.False(string.IsNullOrWhiteSpace(relation.ExpectedTargetRevision)));
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "WarehouseHandoff"
            && value.StateCode == "ArrivedAtWarehouse"
            && value.RequiresSeparateConfirmation);
    }

    [Fact]
    public void EXH3의_ExpectedRevision이없으면_거부한다()
    {
        var source = Manifest();
        source.Exhibits = new[] { CargoHubWarehouse() };
        source.Exhibits[0].CanonicalRecordRelations[2].ExpectedTargetRevision = string.Empty;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal("IntegratedExhibitionExpectedTargetRevisionMissing", error.Message);
    }

    [Fact]
    public void EXH4는_공개범위와_판매가재고경계를_보존한다()
    {
        var source = Manifest();
        source.Exhibits = new[] { OrdererMarket() };

        var exhibit = Assert.Single(new 통합전시관Mapper().Map(source).Exhibits);

        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "IndividualIntent"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.OwnerPrivate);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "GroupingPreview"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.PrivacySafeAggregate);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "MartPublicProduct"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.OrdererPublic);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "MarketInventory"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized);
        Assert.Contains(exhibit.CanonicalRecordRelations, value =>
            value.RelationCode == "ComparedWithNotUsedAsSalePrice");
    }

    [Fact]
    public void EXH4의_개인의향을_공개범위로바꾸면_거부한다()
    {
        var source = Manifest();
        source.Exhibits = new[] { OrdererMarket() };
        source.Exhibits[0].WorkflowCheckpoints[0].DisclosureScopeCode =
            통합전시관DisclosureScopeCodes.OrdererPublic;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionOrdererMarketDisclosureBoundaryInvalid:exhibit:town-city:orderer-group-urban-market",
            error.Message);
    }

    [Fact]
    public void OBJ7A는_EXH4의_다섯Object와_공개범위별Binding을매핑한다()
    {
        var source = Manifest();
        source.Exhibits = new[] { OrdererMarket() };
        source.Stories = source.Exhibits;
        source.SeedbedObjects = new[]
        {
            MarketObject("seedbed-object:town.resident-visual.a",
                "town.resident-visual.a", "placement-profile:town.resident-visual.a",
                new[] { "Perspective", "AggregateBoundary", "Interaction", "Label", "CameraFocus" },
                new[] { "IndividualIntent", "OwnerAuthorizedPerspective" }),
            MarketObject("seedbed-object:town.grouping-cart-table.a",
                "town.grouping-cart-table.a", "placement-profile:town.grouping-cart-table.a",
                new[] { "IntentInput", "AggregateOutput", "ConsentBoundary", "Interaction", "Label", "CameraFocus" },
                new[] { "GroupingPreview", "OrdererGroupSummary" }),
            MarketObject("seedbed-object:city.urban-market-building.a",
                "city.urban-market-building.a", "placement-profile:city.urban-market-building.a",
                new[] { "Entry", "PublicProduct", "DemandSignal", "Interaction", "Label", "CameraFocus" },
                new[] { "MartPublicProduct", "MarketDemandSignal" }),
            MarketObject("seedbed-object:city.operator-inventory-shelf.a",
                "city.operator-inventory-shelf.a", "placement-profile:city.operator-inventory-shelf.a",
                new[] { "Inventory", "ShelfTask", "Operator", "Interaction", "Label", "CameraFocus" },
                new[] { "MarketInventory", "ShelfTask" }),
            MarketObject("seedbed-object:city.market-operator-visual.a",
                "city.market-operator-visual.a", "placement-profile:city.market-operator-visual.a",
                new[] { "Perspective", "Inventory", "ShelfTask", "Interaction", "Label", "CameraFocus" },
                new[] { "MarketInventory", "ShelfTask", "MarketOperatorPerspective" }),
        };
        source.Exhibits[0].ReferencedSeedbedObjectStableIds =
            source.SeedbedObjects.Select(value => value.ObjectStableId).ToArray();

        var result = new 통합전시관Mapper().Map(source);

        Assert.Equal(5, result.SeedbedObjects.Length);
        Assert.All(result.SeedbedObjects, value =>
            Assert.Equal(통합전시관ObjectGateStateCodes.RuntimeVerified, value.GateStateCode));
        var shop = result.SeedbedObjects.Single(value =>
            value.ObjectStableId.Value == "seedbed-object:city.urban-market-building.a");
        Assert.Contains("MartPublicProduct", shop.DataBindingKeys);
        Assert.DoesNotContain("MarketInventory", shop.DataBindingKeys);
        var shelf = result.SeedbedObjects.Single(value =>
            value.ObjectStableId.Value == "seedbed-object:city.operator-inventory-shelf.a");
        Assert.Contains("MarketInventory", shelf.DataBindingKeys);
        Assert.DoesNotContain("MartPublicProduct", shelf.DataBindingKeys);
    }

    [Fact]
    public void OBJ7B는_도심마트Shop의_공개상품Placement를매핑한다()
    {
        var source = Manifest();
        var shop = MarketObject(
            "seedbed-object:city.urban-market-building.a",
            "city.urban-market-building.a",
            "placement-profile:city.urban-market-building.a",
            new[] { "Entry", "PublicProduct", "DemandSignal", "Interaction", "Label", "CameraFocus" },
            new[] { "MartPublicProduct", "MarketDemandSignal" });
        shop.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        shop.BlockedReasonCodes = Array.Empty<string>();
        shop.Evidence.Single(value =>
            value.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement).StatusCode =
            통합전시관EvidenceStatusCodes.Verified;
        source.SeedbedObjects = new[] { shop };
        source.ScenePlacements = new[] { UrbanMarketShopScenePlacement() };
        source.Exhibits[0].ReferencedSeedbedObjectStableIds = new[] { shop.ObjectStableId };

        var result = new 통합전시관Mapper().Map(source);

        var placement = Assert.Single(result.ScenePlacements);
        Assert.Equal("district:market", placement.ZoneStableId.Value);
        Assert.Equal(shop.ObjectStableId, placement.ObjectStableId.Value);
        Assert.Equal("MartPublicProduct:mart-product:sim.potato.public",
            placement.DataBindingKey);
        Assert.DoesNotContain("MarketInventory", placement.DataBindingKey);
    }

    [Fact]
    public void OBJ7C는_집단수요CartTable의_개인정보제거PreviewPlacement를매핑한다()
    {
        var source = Manifest();
        var cart = MarketObject(
            "seedbed-object:town.grouping-cart-table.a",
            "town.grouping-cart-table.a",
            "placement-profile:town.grouping-cart-table.a",
            new[] { "IntentInput", "AggregateOutput", "ConsentBoundary", "Interaction", "Label", "CameraFocus" },
            new[] { "GroupingPreview", "OrdererGroupSummary" });
        cart.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        cart.BlockedReasonCodes = Array.Empty<string>();
        cart.Evidence.Single(value =>
            value.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement).StatusCode =
            통합전시관EvidenceStatusCodes.Verified;
        source.SeedbedObjects = new[] { cart };
        source.ScenePlacements = new[] { GroupingCartTableScenePlacement() };
        source.Exhibits[0].ReferencedSeedbedObjectStableIds = new[] { cart.ObjectStableId };

        var result = new 통합전시관Mapper().Map(source);

        var placement = Assert.Single(result.ScenePlacements);
        Assert.Equal("district:town", placement.ZoneStableId.Value);
        Assert.Equal(cart.ObjectStableId, placement.ObjectStableId.Value);
        Assert.Equal("GroupingPreview:grouping-preview:sim.potato.town",
            placement.DataBindingKey);
        Assert.DoesNotContain("IndividualIntent", placement.DataBindingKey);
        Assert.DoesNotContain("DomainCommand", placement.DataBindingKey);
    }

    [Fact]
    public void EXH5는_기사후보주소축약과_전달수령분리를_보존한다()
    {
        var source = Manifest();
        source.Exhibits = new[] { FoodDelivery() };

        var exhibit = Assert.Single(new 통합전시관Mapper().Map(source).Exhibits);

        Assert.Equal(8, exhibit.WorkflowCheckpoints.Length);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "DriverOffer"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.DriverCandidateApproximate);
        Assert.Contains(exhibit.WorkflowCheckpoints, value =>
            value.StateMachineCode == "DriverAssignment"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized);
        Assert.NotEqual(
            exhibit.WorkflowCheckpoints.Single(value => value.StateCode == "전달완료").CanonicalRecordStableId,
            exhibit.WorkflowCheckpoints.Single(value => value.StateCode == "수령확인").CanonicalRecordStableId);
        Assert.DoesNotContain(exhibit.WorkflowCheckpoints, value => value.StateMachineCode == "CargoJourney");
    }

    [Fact]
    public void EXH5가_Cargo상태기계를재사용하면_거부한다()
    {
        var source = Manifest();
        source.Exhibits = new[] { FoodDelivery() };
        source.Exhibits[0].WorkflowCheckpoints[5].StateMachineCode = "CargoJourney";

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionFoodDeliveryFreightReuseForbidden:exhibit:city:food-delivery",
            error.Message);
    }

    [Fact]
    public void OBJ1은_기존Exhibits호환성을유지하며_Story와Object를분리한다()
    {
        var source = Manifest();
        source.Stories = source.Exhibits;
        source.SeedbedObjects = new[] { SeedbedObject() };
        source.Stories[0].ReferencedSeedbedObjectStableIds =
            new[] { source.SeedbedObjects[0].ObjectStableId };

        var result = new 통합전시관Mapper().Map(source);

        Assert.Single(result.Exhibits);
        var seedbedObject = Assert.Single(result.SeedbedObjects);
        Assert.Equal("seedbed-object:farm.potato-harvest-box.a", seedbedObject.ObjectStableId.Value);
        Assert.Equal(통합전시관ObjectGateStateCodes.RuntimeVerified, seedbedObject.GateStateCode);
        Assert.Empty(result.ScenePlacements);
        Assert.Equal(seedbedObject.ObjectStableId,
            Assert.Single(result.Exhibits[0].ReferencedSeedbedObjectStableIds));
    }

    [Fact]
    public void OBJ5는_승격Object와_SimulationWorldShellPlacement를함께매핑한다()
    {
        var source = Manifest();
        var seedbedObject = SeedbedObject();
        seedbedObject.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        seedbedObject.BlockedReasonCodes = Array.Empty<string>();
        seedbedObject.Evidence.Single(value =>
            value.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement).StatusCode =
            통합전시관EvidenceStatusCodes.Verified;
        source.SeedbedObjects = new[] { seedbedObject };
        source.ScenePlacements = new[] { ScenePlacement() };
        source.Exhibits[0].ReferencedSeedbedObjectStableIds = new[] { seedbedObject.ObjectStableId };

        var result = new 통합전시관Mapper().Map(source);

        Assert.Equal(통합전시관ObjectGateStateCodes.PromotedToScene,
            Assert.Single(result.SeedbedObjects).GateStateCode);
        var placement = Assert.Single(result.ScenePlacements);
        Assert.Equal("scene:simulation-world-shell", placement.SceneStableId.Value);
        Assert.Equal("district:farm", placement.ZoneStableId.Value);
        Assert.Equal(seedbedObject.ObjectStableId, placement.ObjectStableId.Value);
        Assert.Equal("r1", placement.PlacementProfileRevision);
    }

    [Fact]
    public void OBJ6는_HubGate의_물류구역Placement와_HubReceivingBinding을매핑한다()
    {
        var source = Manifest();
        var hubGate = HubGateSeedbedObject();
        hubGate.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        hubGate.BlockedReasonCodes = Array.Empty<string>();
        hubGate.Evidence.Single(value =>
            value.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement).StatusCode =
            통합전시관EvidenceStatusCodes.Verified;
        source.SeedbedObjects = new[] { hubGate };
        source.ScenePlacements = new[] { HubGateScenePlacement() };
        source.Exhibits[0].ReferencedSeedbedObjectStableIds = new[] { hubGate.ObjectStableId };

        var result = new 통합전시관Mapper().Map(source);

        var placement = Assert.Single(result.ScenePlacements);
        Assert.Equal("district:logistics", placement.ZoneStableId.Value);
        Assert.Equal(hubGate.ObjectStableId, placement.ObjectStableId.Value);
        Assert.Equal("HubReceiving:hub-receiving:sim.potato", placement.DataBindingKey);
    }

    [Fact]
    public void OBJ6B는_차량_Pallet_Crate의_서로다른Binding을보존한다()
    {
        var source = Manifest();
        var truck = LogisticsObject(
            "seedbed-object:town.delivery-truck.a",
            "town.delivery-truck.a",
            "placement-profile:town.delivery-truck.a",
            new[] { "Driver", "Cargo", "RouteEntry", "RouteExit" },
            new[] { "CargoJourney", "TransportTask", "ShipperRequestCandidate" });
        var pallet = LogisticsObject(
            "seedbed-object:shared.cargo-pallet.a",
            "shared.cargo-pallet.a",
            "placement-profile:shared.cargo-pallet.a",
            new[] { "Cargo", "Forklift" },
            new[] { "Cargo", "HubReceiving", "WarehouseHandoff" });
        var crate = LogisticsObject(
            "seedbed-object:farm.pallet-crate.a",
            "farm.pallet-crate.a",
            "placement-profile:farm.pallet-crate.a",
            new[] { "HarvestCargo", "Vehicle", "HubHandoff" },
            new[] { "CanonicalProductHarvestCargo", "CargoJourney", "HubReceiving" });
        source.SeedbedObjects = new[] { truck, pallet, crate };
        source.Exhibits[0].ReferencedSeedbedObjectStableIds =
            source.SeedbedObjects.Select(value => value.ObjectStableId).ToArray();

        var result = new 통합전시관Mapper().Map(source);

        Assert.Equal(3, result.SeedbedObjects.Count());
        Assert.Contains(result.SeedbedObjects.Single(value =>
                value.ObjectStableId.Value == truck.ObjectStableId).RequiredSocketCodes,
            value => value == "RouteEntry");
        Assert.Contains(result.SeedbedObjects.Single(value =>
                value.ObjectStableId.Value == pallet.ObjectStableId).DataBindingKeys,
            value => value == "WarehouseHandoff");
        Assert.Contains(result.SeedbedObjects.Single(value =>
                value.ObjectStableId.Value == crate.ObjectStableId).DataBindingKeys,
            value => value == "CanonicalProductHarvestCargo");
        Assert.Empty(result.ScenePlacements);
    }

    [Fact]
    public void OBJ6C는_배송차량의_CargoJourneyPlacement를매핑한다()
    {
        var source = Manifest();
        var truck = LogisticsObject(
            "seedbed-object:town.delivery-truck.a",
            "town.delivery-truck.a",
            "placement-profile:town.delivery-truck.a",
            new[] { "Driver", "Cargo", "RouteEntry", "RouteExit" },
            new[] { "CargoJourney", "TransportTask", "ShipperRequestCandidate" });
        truck.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        truck.BlockedReasonCodes = Array.Empty<string>();
        truck.Evidence.Single(value =>
            value.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement).StatusCode =
            통합전시관EvidenceStatusCodes.Verified;
        source.SeedbedObjects = new[] { truck };
        source.ScenePlacements = new[] { DeliveryTruckScenePlacement() };
        source.Exhibits[0].ReferencedSeedbedObjectStableIds = new[] { truck.ObjectStableId };

        var result = new 통합전시관Mapper().Map(source);

        Assert.Equal(통합전시관ObjectGateStateCodes.PromotedToScene,
            Assert.Single(result.SeedbedObjects).GateStateCode);
        var placement = Assert.Single(result.ScenePlacements);
        Assert.Equal("district:logistics", placement.ZoneStableId.Value);
        Assert.Equal(truck.ObjectStableId, placement.ObjectStableId.Value);
        Assert.Equal("CargoJourney:cargo-journey:sim.potato.farm-hub",
            placement.DataBindingKey);
    }

    [Fact]
    public void OBJ6D1은_공용Pallet의_WarehouseHandoffPlacement를매핑한다()
    {
        var source = Manifest();
        var pallet = LogisticsObject(
            "seedbed-object:shared.cargo-pallet.a",
            "shared.cargo-pallet.a",
            "placement-profile:shared.cargo-pallet.a",
            new[] { "Cargo", "Forklift" },
            new[] { "Cargo", "HubReceiving", "WarehouseHandoff" });
        pallet.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        pallet.BlockedReasonCodes = Array.Empty<string>();
        pallet.Evidence.Single(value =>
            value.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement).StatusCode =
            통합전시관EvidenceStatusCodes.Verified;
        source.SeedbedObjects = new[] { pallet };
        source.ScenePlacements = new[] { CargoPalletScenePlacement() };
        source.Exhibits[0].ReferencedSeedbedObjectStableIds = new[] { pallet.ObjectStableId };

        var result = new 통합전시관Mapper().Map(source);

        var placement = Assert.Single(result.ScenePlacements);
        Assert.Equal("district:logistics", placement.ZoneStableId.Value);
        Assert.Equal(pallet.ObjectStableId, placement.ObjectStableId.Value);
        Assert.Equal("WarehouseHandoff:cargo-handoff:sim.potato.20260407.r3.inbound-91",
            placement.DataBindingKey);
    }

    [Fact]
    public void OBJ6D2는_농장PalletCrate의_HarvestCargoPlacement를매핑한다()
    {
        var source = Manifest();
        var palletCrate = LogisticsObject(
            "seedbed-object:farm.pallet-crate.a",
            "farm.pallet-crate.a",
            "placement-profile:farm.pallet-crate.a",
            new[] { "Cargo", "Forklift" },
            new[] { "CanonicalProductHarvestCargo", "Cargo" });
        palletCrate.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        palletCrate.BlockedReasonCodes = Array.Empty<string>();
        palletCrate.Evidence.Single(value =>
            value.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.ScenePlacement).StatusCode =
            통합전시관EvidenceStatusCodes.Verified;
        source.SeedbedObjects = new[] { palletCrate };
        source.ScenePlacements = new[] { FarmPalletCrateScenePlacement() };
        source.Exhibits[0].ReferencedSeedbedObjectStableIds = new[] { palletCrate.ObjectStableId };

        var result = new 통합전시관Mapper().Map(source);

        var placement = Assert.Single(result.ScenePlacements);
        Assert.Equal("district:farm", placement.ZoneStableId.Value);
        Assert.Equal(palletCrate.ObjectStableId, placement.ObjectStableId.Value);
        Assert.Equal("CanonicalProductHarvestCargo:cargo:sim.potato.20260407.r3",
            placement.DataBindingKey);
    }

    [Fact]
    public void Story가_없는Object를참조하면_거부한다()
    {
        var source = Manifest();
        source.Exhibits[0].ReferencedSeedbedObjectStableIds = new[] { "seedbed-object:missing.a" };

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionStoryObjectReferenceMissing:exhibit:public-data:potato-observation:seedbed-object:missing.a",
            error.Message);
    }

    [Fact]
    public void O4_Object의_Binding증거가없으면_거부한다()
    {
        var source = Manifest();
        source.SeedbedObjects = new[] { SeedbedObject() };
        source.SeedbedObjects[0].Evidence.Single(value =>
            value.EvidenceKindCode == 통합전시관ObjectEvidenceKindCodes.BindingValidation).StatusCode =
            통합전시관EvidenceStatusCodes.Unverified;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionSeedbedObjectGateEvidenceRequired:"
            + source.SeedbedObjects[0].ObjectStableId + ":BindingValidation",
            error.Message);
    }

    [Fact]
    public void Object공통계약의_Prefab경로를_거부한다()
    {
        var source = Manifest();
        source.SeedbedObjects = new[] { SeedbedObject() };
        source.SeedbedObjects[0].VisualVariantKeys = new[] { "Assets/Synty/Farm/Potato.prefab" };

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 통합전시관Mapper().Map(source));

        Assert.Equal(
            "IntegratedExhibitionSeedbedObjectUnityAssetLocatorForbidden:"
            + source.SeedbedObjects[0].ObjectStableId,
            error.Message);
    }

    private static 통합전시관ApiModel Manifest()
        => new()
        {
            StableId = "exhibition-manifest:integrated-seedbed",
            Revision = "exhibition:fixture-r1",
            GeneratedAtUtc = new DateTimeOffset(2026, 8, 11, 10, 0, 0, TimeSpan.Zero),
            IsReadOnly = true,
            Exhibits = new[] { PotatoObservation() },
        };

    private static 통합전시관ExhibitApiModel PotatoObservation()
        => new()
        {
            ExhibitStableId = "exhibit:public-data:potato-observation",
            DisplayName = "감자 현실 관측",
            ExhibitKindCode = "PublicObservation",
            WorkflowKey = "CommunityTrust",
            ProductVersionCode = "0.0",
            PerspectiveCode = "PublicObserver",
            AuthorizationScopeCode = "Public",
            WorldStableId = "world:integrated-seedbed-exhibition:fixture",
            ZoneStableId = "zone:exhibition:public-data-hall",
            ObjectStableIds = new[] { "world-object:potato-observation-table" },
            CanonicalRecordRelations = new[]
            {
                new 통합전시관CanonicalRecordRelationApiModel
                {
                    RelationStableId = "relation:product-potato:public-observation-kamis-potato",
                    SourceRecordKindCode = "Product",
                    SourceStableId = "product:potato",
                    SourceRevision = "product-catalog:r1",
                    RelationCode = "ObservedBy",
                    TargetRecordKindCode = "PublicObservation",
                    TargetStableId = "public-observation:kamis:potato",
                    TargetRevision = "uncollected:r1",
                    ExpectedTargetRevision = "uncollected:r1",
                    VerificationStatusCode = "Unverified",
                },
            },
            SourcePlan = new[]
            {
                new 통합전시관SourcePlanSegmentApiModel
                {
                    SourceKey = "public-data:kamis-potato-observation",
                    SourceStableId = "source:public-data:kamis-potato",
                    SourceRevision = "kamis-potato-observation:uncollected-r1",
                    SourceModeCode = "PublicObservation",
                },
            },
            SourceRevision = "kamis-potato-observation:uncollected-r1",
            ProjectionRevision = "integrated-exhibition-projector:r1",
            DataStateCode = 통합전시관DataStateCodes.Uncollected,
            ExperienceModeCode = 통합전시관ExperienceModeCodes.ReadOnly,
            CompletionStateCode = 통합전시관CompletionStateCodes.Blocked,
            AllowedInteractionIntentCodes = new[]
            {
                통합전시관InteractionIntentCodes.Observe,
                통합전시관InteractionIntentCodes.ViewLineage,
                통합전시관InteractionIntentCodes.Compare,
            },
            BlockedReasonCodes = new[] { "ActualObservationNotCollected" },
            VisualKeys = new[] { "public-data.observation.potato" },
            PackRoleCodes = new[] { "Farm", "Shared" },
            Evidence = Evidence(),
        };

    private static 통합전시관SeedbedObjectApiModel SeedbedObject()
        => new()
        {
            ObjectStableId = "seedbed-object:farm.potato-harvest-box.a",
            DisplayName = "감자 수확 상자",
            SemanticRoleCode = "HarvestCargoVisual",
            ObjectKindCode = "CargoVisual",
            VisualVariantKeys = new[] { "farm.potato-harvest-box.a" },
            PackRoleCodes = new[] { "Farm" },
            CompatibleZoneRoleCodes = new[] { "FarmProduction" },
            PlacementProfileKey = "placement-profile:farm.harvest-box.a",
            RequiredSocketCodes = new[] { "Cargo", "Interaction", "Label", "CameraFocus" },
            DataBindingKeys = new[] { "CanonicalProductHarvestCargo", "HarvestLot" },
            PresentationStateCodes = new[] { "Normal", "Selected", "Blocked", "Stale" },
            GateStateCode = 통합전시관ObjectGateStateCodes.RuntimeVerified,
            BlockedReasonCodes = new[] { "TargetScenePlacementNotPromoted" },
            Evidence = ObjectEvidence(),
        };

    private static 통합전시관SeedbedObjectApiModel HubGateSeedbedObject()
        => new()
        {
            ObjectStableId = "seedbed-object:town.hub-inbound-gate.a",
            DisplayName = "Hub 입고 Gate",
            SemanticRoleCode = "HubInboundHandoff",
            ObjectKindCode = "Facility",
            VisualVariantKeys = new[] { "town.hub-inbound-gate.a" },
            PackRoleCodes = new[] { "Town" },
            CompatibleZoneRoleCodes = new[] { "LogisticsHub" },
            PlacementProfileKey = "placement-profile:town.hub-inbound-gate.a",
            RequiredSocketCodes = new[] { "Entry", "Exit", "Vehicle", "Cargo", "Interaction", "Label", "CameraFocus" },
            DataBindingKeys = new[] { "CargoJourney", "HubReceiving", "WarehouseHandoff" },
            PresentationStateCodes = new[] { "Normal", "Selected", "Blocked", "Stale" },
            GateStateCode = 통합전시관ObjectGateStateCodes.RuntimeVerified,
            BlockedReasonCodes = new[] { "TargetScenePlacementNotPromoted" },
            Evidence = ObjectEvidence(),
        };

    private static 통합전시관SeedbedObjectApiModel LogisticsObject(
        string stableId,
        string visualVariant,
        string placementProfile,
        string[] sockets,
        string[] bindings)
    {
        var value = SeedbedObject();
        value.ObjectStableId = stableId;
        value.DisplayName = stableId;
        value.SemanticRoleCode = "LogisticsObject";
        value.ObjectKindCode = "PresentationObject";
        value.VisualVariantKeys = new[] { visualVariant };
        value.PlacementProfileKey = placementProfile;
        value.RequiredSocketCodes = sockets;
        value.DataBindingKeys = bindings;
        return value;
    }

    private static 통합전시관SeedbedObjectApiModel MarketObject(
        string stableId,
        string visualVariant,
        string placementProfile,
        string[] sockets,
        string[] bindings)
    {
        var value = LogisticsObject(stableId, visualVariant, placementProfile, sockets, bindings);
        value.SemanticRoleCode = "OrdererMarketPresentationObject";
        value.PackRoleCodes = stableId.Contains(":town.", StringComparison.Ordinal)
            ? new[] { "Town" }
            : new[] { "City" };
        value.CompatibleZoneRoleCodes = stableId.Contains(":town.", StringComparison.Ordinal)
            ? new[] { "TownDemandAggregation" }
            : new[] { "UrbanMarketOperations" };
        return value;
    }

    private static 통합전시관EvidenceApiModel[] ObjectEvidence()
        => new[]
        {
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.SourceIndex, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.MeaningReview, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.VisualResolution, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.PlacementValidation, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.BindingValidation, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.ObjectPreview, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.ScenePlacement, 통합전시관EvidenceStatusCodes.Unverified),
        };

    private static 통합전시관EvidenceApiModel ObjectEvidence(string kind, string status)
        => new()
        {
            EvidenceKindCode = kind,
            StatusCode = status,
            Reference = "unity-object-catalog:obj-1",
            Note = "Object Gate 독립 증거 축",
        };

    private static 통합전시관ScenePlacementApiModel ScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.farm.potato-harvest-box.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:farm",
            ObjectStableId = "seedbed-object:farm.potato-harvest-box.a",
            VisualVariantKey = "farm.potato-harvest-box.a",
            PlacementProfileKey = "placement-profile:farm.harvest-box.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "farm.harvest-lot.potato-001",
            DataBindingKey = "HarvestLot:harvest-lot:potato-001",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = new[]
            {
                Evidence(통합전시관EvidenceKindCodes.Code, "Verified", "repo:obj-5"),
                Evidence(통합전시관EvidenceKindCodes.FocusedTest, "Verified", "validation:obj-5"),
                Evidence(통합전시관EvidenceKindCodes.Runtime, "Verified", "unity-change:obj-5"),
                Evidence(통합전시관EvidenceKindCodes.Operational, "NotApplicable", "operation:not-applicable"),
            },
        };

    private static 통합전시관ScenePlacementApiModel HubGateScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.logistics.hub-inbound-gate.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:logistics",
            ObjectStableId = "seedbed-object:town.hub-inbound-gate.a",
            VisualVariantKey = "town.hub-inbound-gate.a",
            PlacementProfileKey = "placement-profile:town.hub-inbound-gate.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "logistics.hub.inbound-gate",
            DataBindingKey = "HubReceiving:hub-receiving:sim.potato",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = new[]
            {
                Evidence(통합전시관EvidenceKindCodes.Code, "Verified", "repo:obj-6"),
                Evidence(통합전시관EvidenceKindCodes.FocusedTest, "Verified", "validation:obj-6"),
                Evidence(통합전시관EvidenceKindCodes.Runtime, "Verified", "unity-change:obj-6"),
                Evidence(통합전시관EvidenceKindCodes.Operational, "NotApplicable", "operation:not-applicable"),
            },
        };

    private static 통합전시관ScenePlacementApiModel DeliveryTruckScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.logistics.delivery-truck.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:logistics",
            ObjectStableId = "seedbed-object:town.delivery-truck.a",
            VisualVariantKey = "town.delivery-truck.a",
            PlacementProfileKey = "placement-profile:town.delivery-truck.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "logistics.cargo-journey.delivery-truck",
            DataBindingKey = "CargoJourney:cargo-journey:sim.potato.farm-hub",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = new[]
            {
                Evidence(통합전시관EvidenceKindCodes.Code, "Verified", "repo:obj-6c"),
                Evidence(통합전시관EvidenceKindCodes.FocusedTest, "Verified", "validation:obj-6c"),
                Evidence(통합전시관EvidenceKindCodes.Runtime, "Verified", "unity-change:obj-6c"),
                Evidence(통합전시관EvidenceKindCodes.Operational, "NotApplicable", "operation:not-applicable"),
            },
        };

    private static 통합전시관ScenePlacementApiModel CargoPalletScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.logistics.cargo-pallet.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:logistics",
            ObjectStableId = "seedbed-object:shared.cargo-pallet.a",
            VisualVariantKey = "shared.cargo-pallet.a",
            PlacementProfileKey = "placement-profile:shared.cargo-pallet.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "logistics.warehouse-handoff.cargo-pallet",
            DataBindingKey = "WarehouseHandoff:cargo-handoff:sim.potato.20260407.r3.inbound-91",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = new[]
            {
                Evidence(통합전시관EvidenceKindCodes.Code, "Verified", "repo:obj-6d1"),
                Evidence(통합전시관EvidenceKindCodes.FocusedTest, "Verified", "validation:obj-6d1"),
                Evidence(통합전시관EvidenceKindCodes.Runtime, "Verified", "unity-change:obj-6d1"),
                Evidence(통합전시관EvidenceKindCodes.Operational, "NotApplicable", "operation:not-applicable"),
            },
        };

    private static 통합전시관ScenePlacementApiModel FarmPalletCrateScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.farm.pallet-crate.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:farm",
            ObjectStableId = "seedbed-object:farm.pallet-crate.a",
            VisualVariantKey = "farm.pallet-crate.a",
            PlacementProfileKey = "placement-profile:farm.pallet-crate.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "farm.outbound.pallet-crate",
            DataBindingKey = "CanonicalProductHarvestCargo:cargo:sim.potato.20260407.r3",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = new[]
            {
                Evidence(통합전시관EvidenceKindCodes.Code, "Verified", "repo:obj-6d2"),
                Evidence(통합전시관EvidenceKindCodes.FocusedTest, "Verified", "validation:obj-6d2"),
                Evidence(통합전시관EvidenceKindCodes.Runtime, "Verified", "unity-change:obj-6d2"),
                Evidence(통합전시관EvidenceKindCodes.Operational, "NotApplicable", "operation:not-applicable"),
            },
        };

    private static 통합전시관ScenePlacementApiModel UrbanMarketShopScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.market.urban-market-shop.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:market",
            ObjectStableId = "seedbed-object:city.urban-market-building.a",
            VisualVariantKey = "city.urban-market-building.a",
            PlacementProfileKey = "placement-profile:city.urban-market-building.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "market.public-products.shop",
            DataBindingKey = "MartPublicProduct:mart-product:sim.potato.public",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = new[]
            {
                Evidence(통합전시관EvidenceKindCodes.Code, "Verified", "repo:obj-7b"),
                Evidence(통합전시관EvidenceKindCodes.FocusedTest, "Verified", "validation:obj-7b"),
                Evidence(통합전시관EvidenceKindCodes.Runtime, "Verified", "unity-change:obj-7b"),
                Evidence(통합전시관EvidenceKindCodes.Operational, "NotApplicable", "operation:not-applicable"),
            },
        };

    private static 통합전시관ScenePlacementApiModel GroupingCartTableScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.town.grouping-cart-table.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:town",
            ObjectStableId = "seedbed-object:town.grouping-cart-table.a",
            VisualVariantKey = "town.grouping-cart-table.a",
            PlacementProfileKey = "placement-profile:town.grouping-cart-table.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "town.orderer-group.grouping-cart-table",
            DataBindingKey = "GroupingPreview:grouping-preview:sim.potato.town",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = new[]
            {
                Evidence(통합전시관EvidenceKindCodes.Code, "Verified", "repo:obj-7c"),
                Evidence(통합전시관EvidenceKindCodes.FocusedTest, "Verified", "validation:obj-7c"),
                Evidence(통합전시관EvidenceKindCodes.Runtime, "Verified", "unity-change:obj-7c"),
                Evidence(통합전시관EvidenceKindCodes.Operational, "NotApplicable", "operation:not-applicable"),
            },
        };

    private static 통합전시관ExhibitApiModel CargoHubWarehouse()
    {
        const string cargo = "cargo:sim.potato.20260407.r3";
        const string request = "shipper-request-candidate:sim.potato.farm-hub.r1";
        const string journey = "cargo-journey:sim.potato.farm-hub";
        const string receiving = "hub-receiving:sim.potato";
        const string handoff = "cargo-handoff:sim.potato.20260407.r3.inbound-91";
        const string warehouse = "warehouse-zone:7";
        return new 통합전시관ExhibitApiModel
        {
            ExhibitStableId = "exhibit:logistics:cargo-hub-warehouse",
            DisplayName = "화물·Hub·창고 계보",
            ExhibitKindCode = "CargoHubWarehouseLineage",
            WorkflowKey = "WarehouseFulfillment",
            ProductVersionCode = "3.5-dev",
            PerspectiveCode = "ShipperWarehouse",
            AuthorizationScopeCode = "RoleScopedFixture",
            WorldStableId = "world:integrated-seedbed-exhibition:fixture",
            ZoneStableId = "zone:exhibition:cargo-hub-warehouse",
            ObjectStableIds = new[] { request, cargo, journey, receiving, handoff, warehouse },
            CanonicalRecordRelations = new[]
            {
                Relation("request-cargo", "ShipperRequestCandidate", request, "1", "RequestsTransportOf", "Cargo", cargo, "3"),
                Relation("cargo-journey", "Cargo", cargo, "3", "MovedBy", "CargoJourney", journey, "1"),
                Relation("journey-receiving", "CargoJourney", journey, "4", "ArrivesForInspectionAt", "HubReceiving", receiving, "1"),
                Relation("receiving-handoff", "HubReceiving", receiving, "1", "HandsOffThrough", "WarehouseHandoff", handoff, "2"),
                Relation("handoff-warehouse", "WarehouseHandoff", handoff, "2", "ProjectedInto", "WarehouseWorldSnapshot", warehouse, "warehouse-revision-1"),
            },
            WorkflowCheckpoints = new[]
            {
                Checkpoint(1, "ShipperRequestCandidate", "Candidate", cargo, request, "1", false, "ShipperRequestDoesNotCreateCargo"),
                Checkpoint(2, "CargoJourney", "Loaded", cargo, journey, "1", true, "DispatchConfirmRequired"),
                Checkpoint(3, "CargoJourney", "InTransit", cargo, journey, "2", false, "RouteTickOnly"),
                Checkpoint(4, "CargoJourney", "ArrivedAtHub", cargo, journey, "4", false, "ArrivalIsNotReceiving"),
                Checkpoint(5, "HubReceiving", "Inspection", cargo, receiving, "2", true, "InspectionConfirmRequired"),
                Checkpoint(6, "WarehouseHandoff", "ArrivedAtWarehouse", cargo, handoff, "2", true, "WarehouseArrivalIsNotReceiving"),
                Checkpoint(7, "WarehouseHandoff", "ReceivingCompleted", cargo, warehouse, "warehouse-revision-1", false, "ReceivingCommandRequired"),
            },
            SourcePlan = new[]
            {
                Source("simulation:potato-cargo-journey", "potato-cargo-journey-fixture:r1", "SimulationFixture"),
                Source("projection:cargo-warehouse-handoff", "cargo-warehouse-handoff-contract:r1", "OperationalContract"),
                Source("projection:warehouse-world-snapshot", "warehouse-world-snapshot-contract:r1", "AuthorizedOperationalContract"),
            },
            SourceRevision = "potato-cargo-hub-warehouse-fixture:r1",
            ProjectionRevision = "integrated-exhibition-projector:r1",
            DataStateCode = 통합전시관DataStateCodes.Fixture,
            ExperienceModeCode = 통합전시관ExperienceModeCodes.Simulation,
            CompletionStateCode = 통합전시관CompletionStateCodes.Linked,
            AllowedInteractionIntentCodes = new[]
            {
                통합전시관InteractionIntentCodes.Observe,
                통합전시관InteractionIntentCodes.ViewLineage,
                통합전시관InteractionIntentCodes.SimulationPreview,
                통합전시관InteractionIntentCodes.RefreshCanonical,
            },
            BlockedReasonCodes = new[] { "OperationalCargoSnapshotNotLoaded", "WarehouseReceivingCommandNotExposedInExhibition" },
            VisualKeys = new[] { "logistics.cargo-truck", "logistics.hub-inbound", "warehouse.inbound-dock" },
            PackRoleCodes = new[] { "Farm", "Town", "City", "Shared" },
            Evidence = new[]
            {
                Evidence(통합전시관EvidenceKindCodes.Code, "Verified", "repo:integrated-exhibition"),
                Evidence(통합전시관EvidenceKindCodes.FocusedTest, "Verified", "validation:focused"),
                Evidence(통합전시관EvidenceKindCodes.Runtime, "Verified", "unity-change:integrated-exhibition-exh3"),
                Evidence(통합전시관EvidenceKindCodes.Operational, "Partial", "operation:not-asserted"),
            },
        };
    }

    private static 통합전시관ExhibitApiModel OrdererMarket()
    {
        const string lineage = "demand-lineage:sim.potato.town-city";
        const string intent = "individual-intent:sim.potato.owner-private";
        const string preview = "grouping-preview:sim.potato.town";
        const string group = "orderer-group-summary:sim.potato.town";
        const string demand = "market-demand-signal:sim.potato.city";
        const string product = "mart-product:sim.potato.public";
        const string inventory = "market-inventory:sim.potato.operator";
        const string task = "market-task:sim.potato.shelf";
        const string kamis = "public-observation:kamis:potato";
        return new 통합전시관ExhibitApiModel
        {
            ExhibitStableId = "exhibit:town-city:orderer-group-urban-market",
            DisplayName = "주문자 집단·도심마트 경계",
            ExhibitKindCode = "OrdererGroupUrbanMarketLineage",
            WorkflowKey = "GroupPurchaseDemand",
            ProductVersionCode = "3.5-dev",
            PerspectiveCode = "OrdererMarketOperator",
            AuthorizationScopeCode = "PrivacyPartitionedFixture",
            WorldStableId = "world:integrated-seedbed-exhibition:fixture",
            ZoneStableId = "zone:exhibition:town-city-market",
            ObjectStableIds = new[] { lineage, preview, group, demand, product, inventory, task, kamis },
            CanonicalRecordRelations = new[]
            {
                Relation("intent-preview", "IndividualIntent", intent, "1", "AggregatedPrivatelyAs", "GroupingPreview", preview, "preview-r1"),
                Relation("preview-group", "GroupingPreview", preview, "preview-r1", "RequiresConsentBefore", "OrdererGroupSummary", group, "group-r1"),
                Relation("group-demand", "OrdererGroupSummary", group, "group-r1", "ProjectedAs", "MarketDemandSignal", demand, "demand-r1"),
                Relation("demand-product", "MarketDemandSignal", demand, "demand-r1", "PresentedAlongside", "MartPublicProduct", product, "product-r1"),
                Relation("product-inventory", "MartPublicProduct", product, "product-r1", "DoesNotReveal", "MarketOperationalInventory", inventory, "inventory-r1"),
                Relation("kamis-product", "KamisObservation", kamis, "uncollected-r1", "ComparedWithNotUsedAsSalePrice", "MartPublicProduct", product, "product-r1"),
            },
            WorkflowCheckpoints = new[]
            {
                Checkpoint(1, "IndividualIntent", "Withdrawable", lineage, intent, "1", true, "ParticipationConsentNotGranted", 통합전시관DisclosureScopeCodes.OwnerPrivate),
                Checkpoint(2, "GroupingPreview", "Candidate", lineage, preview, "1", true, "PreviewDoesNotEnroll", 통합전시관DisclosureScopeCodes.PrivacySafeAggregate),
                Checkpoint(3, "OrdererGroupSummary", "Recruiting", lineage, group, "1", true, "ExplicitParticipationRequired", 통합전시관DisclosureScopeCodes.PrivacySafeAggregate),
                Checkpoint(4, "MartPublicProduct", "PublishedProjection", lineage, product, "1", false, "SalePriceIsNotKamisObservation", 통합전시관DisclosureScopeCodes.OrdererPublic),
                Checkpoint(5, "MarketInventory", "AuthorizedProjection", lineage, inventory, "1", false, "PublicQuantityIsNotPhysicalInventory", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
                Checkpoint(6, "ShelfTask", "Candidate", lineage, task, "1", true, "OperationalCommandNotExposed", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
            },
            SourcePlan = new[] { Source("simulation:orderer-group-market", "fixture-r1", "SimulationFixture") },
            SourceRevision = "fixture-r1",
            ProjectionRevision = "integrated-exhibition-projector:r1",
            DataStateCode = 통합전시관DataStateCodes.Fixture,
            ExperienceModeCode = 통합전시관ExperienceModeCodes.Simulation,
            CompletionStateCode = 통합전시관CompletionStateCodes.Linked,
            AllowedInteractionIntentCodes = new[] { 통합전시관InteractionIntentCodes.Observe, 통합전시관InteractionIntentCodes.SimulationPreview },
            BlockedReasonCodes = new[] { "SalePriceIsNotKamisObservation", "PublicQuantityIsNotPhysicalInventory" },
            VisualKeys = new[] { "town.orderer-group.aggregate", "city.market.public-product" },
            PackRoleCodes = new[] { "Town", "City" },
            Evidence = Evidence(),
        };
    }

    private static 통합전시관ExhibitApiModel FoodDelivery()
    {
        const string order = "food-order:sim.city-meal.001";
        const string preparation = "restaurant-preparation:sim.city-meal.001";
        const string dispatch = "food-dispatch:sim.city-meal.001";
        const string offer = "food-driver-offer:sim.city-meal.001";
        const string assignment = "food-driver-assignment:sim.city-meal.001";
        const string pickup = "food-pickup-handoff:sim.city-meal.001";
        const string delivery = "food-delivery-handoff:sim.city-meal.001";
        const string receipt = "food-orderer-receipt:sim.city-meal.001";
        return new 통합전시관ExhibitApiModel
        {
            ExhibitStableId = "exhibit:city:food-delivery",
            DisplayName = "음식점·기사·주문자 인계",
            ExhibitKindCode = "FoodDeliveryLineage",
            WorkflowKey = "FoodDelivery",
            ProductVersionCode = "3.0-dev",
            PerspectiveCode = "FoodOrderParticipants",
            AuthorizationScopeCode = "ParticipantPartitionedFixture",
            WorldStableId = "world:integrated-seedbed-exhibition:fixture",
            ZoneStableId = "zone:exhibition:city-food-delivery",
            ObjectStableIds = new[] { order, preparation, dispatch, offer, assignment, pickup, delivery, receipt },
            CanonicalRecordRelations = new[]
            {
                Relation("food-order-preparation", "FoodOrder", order, "1", "PreparedBy", "RestaurantPreparation", preparation, "1"),
                Relation("food-preparation-dispatch", "RestaurantPreparation", preparation, "1", "RequestsDeliveryThrough", "FoodDispatchQueue", dispatch, "1"),
                Relation("food-dispatch-offer", "FoodDispatchQueue", dispatch, "1", "RecommendedAs", "DriverOffer", offer, "1"),
                Relation("food-offer-assignment", "DriverOffer", offer, "1", "RequiresDriverAcceptanceFor", "DriverAssignment", assignment, "1"),
                Relation("food-assignment-pickup", "DriverAssignment", assignment, "1", "AuthorizesPickupOf", "FoodPickupHandoff", pickup, "1"),
                Relation("food-pickup-delivery", "FoodPickupHandoff", pickup, "1", "DeliveredThrough", "FoodDeliveryHandoff", delivery, "1"),
                Relation("food-delivery-receipt", "FoodDeliveryHandoff", delivery, "1", "RequiresSeparateReceiptConfirmation", "OrdererReceipt", receipt, "1"),
            },
            WorkflowCheckpoints = new[]
            {
                Checkpoint(1, "FoodOrder", "주문대기", order, order, "1", true, "OrderConfirmRequired", 통합전시관DisclosureScopeCodes.OwnerPrivate),
                Checkpoint(2, "RestaurantPreparation", "조리중", order, preparation, "1", true, "RestaurantAcceptanceRequired", 통합전시관DisclosureScopeCodes.RestaurantAuthorized),
                Checkpoint(3, "RestaurantPreparation", "픽업대기", order, preparation, "2", true, "RestaurantPickupReadyRequired", 통합전시관DisclosureScopeCodes.RestaurantAuthorized),
                Checkpoint(4, "DriverOffer", "추천중", order, offer, "1", false, "ApproximateDropoffBeforeDriverAcceptance", 통합전시관DisclosureScopeCodes.DriverCandidateApproximate),
                Checkpoint(5, "DriverAssignment", "기사배정", order, assignment, "1", true, "DriverSelfAcceptanceRequired", 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized),
                Checkpoint(6, "FoodDelivery", "픽업완료", order, pickup, "1", true, "AssignedDriverPickupRequired", 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized),
                Checkpoint(7, "FoodDelivery", "전달완료", order, delivery, "1", true, "DeliveryCompletionIsNotReceiptConfirmation", 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized),
                Checkpoint(8, "OrdererReceipt", "수령확인", order, receipt, "1", true, "OrdererReceiptConfirmationRequired", 통합전시관DisclosureScopeCodes.OwnerPrivate),
            },
            SourcePlan = new[] { Source("simulation:food-delivery", "fixture-r1", "SimulationFixture") },
            SourceRevision = "fixture-r1",
            ProjectionRevision = "integrated-exhibition-projector:r1",
            DataStateCode = 통합전시관DataStateCodes.Fixture,
            ExperienceModeCode = 통합전시관ExperienceModeCodes.Simulation,
            CompletionStateCode = 통합전시관CompletionStateCodes.Linked,
            AllowedInteractionIntentCodes = new[] { 통합전시관InteractionIntentCodes.Observe, 통합전시관InteractionIntentCodes.SimulationPreview },
            BlockedReasonCodes = new[] { "ApproximateDropoffBeforeDriverAcceptance", "DeliveryCompletionIsNotReceiptConfirmation" },
            VisualKeys = new[] { "city.restaurant.preparation", "city.food-driver.route" },
            PackRoleCodes = new[] { "City", "Town" },
            Evidence = Evidence(),
        };
    }

    private static 통합전시관SourcePlanSegmentApiModel Source(string stableId, string revision, string mode)
        => new()
        {
            SourceKey = stableId,
            SourceStableId = stableId,
            SourceRevision = revision,
            SourceModeCode = mode,
        };

    private static 통합전시관CanonicalRecordRelationApiModel Relation(
        string key, string sourceKind, string sourceId, string sourceRevision,
        string code, string targetKind, string targetId, string targetRevision)
        => new()
        {
            RelationStableId = "relation:exhibit-logistics:" + key,
            SourceRecordKindCode = sourceKind,
            SourceStableId = sourceId,
            SourceRevision = sourceRevision,
            RelationCode = code,
            TargetRecordKindCode = targetKind,
            TargetStableId = targetId,
            TargetRevision = targetRevision,
            ExpectedTargetRevision = targetRevision,
            VerificationStatusCode = "SimulationLinked",
        };

    private static 통합전시관WorkflowCheckpointApiModel Checkpoint(
        int sequence, string machine, string state, string lineage, string canonical,
        string revision, bool confirm, string boundary,
        string disclosureScope = 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized)
        => new()
        {
            CheckpointStableId = "checkpoint:exhibit-logistics:" + sequence,
            Sequence = sequence,
            StateMachineCode = machine,
            StateCode = state,
            LineageStableId = lineage,
            CanonicalRecordStableId = canonical,
            Revision = revision,
            AuthorityCode = 통합전시관CheckpointAuthorityCodes.SimulationFixture,
            DisclosureScopeCode = disclosureScope,
            RequiresSeparateConfirmation = confirm,
            BoundaryCode = boundary,
        };

    private static 통합전시관EvidenceApiModel[] Evidence()
        => new[]
        {
            Evidence(통합전시관EvidenceKindCodes.Code, "Partial", "repo:integrated-exhibition"),
            Evidence(통합전시관EvidenceKindCodes.FocusedTest, "Verified", "validation:focused"),
            Evidence(통합전시관EvidenceKindCodes.Runtime, "Verified", "change:asset-soil-seedbed"),
            Evidence(통합전시관EvidenceKindCodes.Operational, "Unverified", "operation:not-asserted"),
        };

    private static 통합전시관EvidenceApiModel Evidence(string kind, string status, string reference)
        => new()
        {
            EvidenceKindCode = kind,
            StatusCode = status,
            Reference = reference,
            Note = "독립 증거 축",
        };
}
