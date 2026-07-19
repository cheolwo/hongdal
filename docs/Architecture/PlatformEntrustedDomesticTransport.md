# Platform Entrusted Domestic Transport

This document defines how group purchase imports connect to the 1.0 domestic cargo transport flow.

## Principle

An orderer group is often an informal group without a business registration number or legal shipper status. Ssalddel should not require that group to become the direct shipper for the bonded-area domestic transport step.

The default operating model is:

| Role | Actor |
| --- | --- |
| Domestic cargo transport principal | Platform |
| Cost owner and allocation scope | Orderer group |
| Dispatch engine | `CargoYongdalDispatchEngine` |
| Dispatch business type | `20` / cargo-yongdal transport |
| Source request type | `ImportCargoTransport`, `FclCargoTransport`, or `LclCargoTransport` |
| Destination type | `ThreePlWarehouse`, `ApartmentComplexDirectDistribution`, or `OrdererGroupRepresentativeDropoff` |
| Default destination | `ApartmentComplexDirectDistribution` with driver unit-door delivery |
| Default settlement model | Platform collects orderer payments and pays the driver after dropoff |

## Flow

1. Overseas shipment is tracked by B/L, AWB, or cargo management number.
2. Customs or bonded release readiness is confirmed.
3. A commerce fulfillment plan identifies the target destination.
4. The platform creates a domestic cargo transport draft.
5. The draft uses the bonded area as pickup and one of these destinations as dropoff:
   - domestic 3PL warehouse for storage, sales-channel listing, and later outbound batching
   - apartment complex for direct group-purchase receipt and internal distribution
   - orderer group representative dropoff point
6. The draft can move into the 1.0 dispatch queue after release, pickup/dropoff, cargo specification, and platform confirmation are complete.
7. Driver assignment proceeds through the existing cargo-yongdal dispatch flow.
8. Driver recommendation cards distinguish general cargo transport from group-purchase cargo transport so the driver can see whether the scope may include apartment unit delivery.

The platform may create this draft before customs release is complete. In that case the draft is not dispatch-ready yet, but it can still show destination options, cold-chain confirmation gaps, and cost estimates so the orderer group lead can decide the delivery method early.

## Destination Options

The default destination decision is apartment direct distribution through a Ssalddel 1.0 cargo-yongdal driver. The default driver scope is unit-door delivery, because it keeps the group-purchase delivery promise simple for orderers. 3PL inbound or representative dropoff remains available as an option when the orderer group deliberately chooses storage, later sales-channel fulfillment, or internal distribution.

After the orderer group confirms a delivery method, the platform treats the transport decision as locked by default. A later request to switch from driver home delivery to 3PL, or from 3PL to direct distribution, is not applied silently; it produces `ConfirmTransportDecisionRevision` so the platform can re-check cost, labor scope, privacy handling, and participant agreement.

| Destination type | Meaning |
| --- | --- |
| `ThreePlWarehouse` | Bonded area to domestic 3PL warehouse. This is useful when goods will be stored, listed on Smart Store/Coupang, and shipped later by outbound batch. |
| `ApartmentComplexDirectDistribution` | Bonded area to apartment complex. The driver may only complete complex dropoff, or may also perform building/unit distribution when the scope, fee, checklist, and recipient address privacy handling are confirmed. |
| `OrdererGroupRepresentativeDropoff` | Bonded area to a representative pickup point controlled by the orderer group. Internal distribution is handled outside the driver transport scope. |

For apartment direct distribution, the platform records:

- apartment complex code and name when known
- whether the driver distributes to a pickup point, building entrance, or unit door
- expected unit delivery count
- whether imported product information was registered, who registered it, and where the product-info record or sticker source is stored
- whether overseas seller or overseas forwarder uses product-info stickers, unit invoice labels, or no unit label
- whether packages are pre-sorted by building/unit or route sequence before bonded-area pickup
- whether the unit distribution checklist is confirmed
- whether recipient address privacy handling is confirmed before assignment

## Overseas Pre-Sortation

Driver unit-door delivery should not assume that the driver will classify mixed cargo at the apartment site. Before bonded-area pickup, the overseas seller or overseas forwarder can be assigned to:

1. receive the orderer-level demand breakdown, such as 3 kg for one orderer and 5 kg for another
2. register imported product information before the cargo enters the domestic handover flow
3. record who registered the product information and where the product-info record or sticker source is stored
4. choose the package labeling mode
5. attach the selected label or sticker to the matching package when required
6. pre-sort packages by building/unit or delivery route sequence
7. hand over a loading sequence manifest so the driver can load and unload without opening mixed cargo

Labeling modes:

| Mode | Meaning |
| --- | --- |
| `ProductInfoSticker` | Default for homogeneous split goods, such as 100 kg of pork belly divided into 3 kg bundles. The sticker identifies the product and may include a product barcode, but it does not require an orderer invoice label or order-number barcode. |
| `UnitInvoiceLabel` | Used when individualized packages need orderer-level traceability. The overseas seller or forwarder issues invoice/package labels by demand quantity. Barcode scan lookup is enabled only when that operational traceability is required. |
| `NoUnitLabel` | Allowed only when the operation can rely on the sortation manifest and product identity is otherwise clear. |

If driver unit delivery is selected, the planner treats this as a dispatch gate. `ConfirmImportedProductInfoRegistration` remains open until imported product information is shared into the platform flow. `ConfirmProductInfoStickerStorage` remains open until the source record, sticker file, or manual archive location is confirmed. `ConfirmUnitSortationBeforePickup` remains open until unit demand, the selected label/sticker condition, package count, and loading order are confirmed. If `UnitInvoiceLabel` is selected and the responsible party is `OverseasSeller` or `OverseasForwarder`, `ConfirmOverseasUnitInvoiceAndLabeling` also remains open until overseas invoice and label output is confirmed. `ConfirmUnitBarcodeScanLookup` remains open only when barcode lookup is enabled and the scan can resolve to the masked recipient, unit, demand quantity, and delivery sequence needed by the driver app.

## Cold Chain Constraints

If the cargo is chilled or frozen, the destination and vehicle must be compatible with that temperature condition.

| Route | Required confirmation |
| --- | --- |
| 3PL warehouse | Refrigerated/frozen-capable vehicle and 3PL refrigerated/frozen storage facility |
| Apartment direct distribution | Refrigerated/frozen-capable vehicle through dropoff and handover |
| Representative dropoff | Refrigerated/frozen-capable vehicle through representative handover |

The planner returns `ColdChainPlan` with the normalized temperature code, whether cold chain is required, and the missing confirmation codes. A selected route cannot move to dispatch while `ConfirmColdChainVehicle` or `ConfirmColdChainThreePlFacility` remains unresolved.

## Cost Decision Options

The planner returns `DestinationCostOptions` before dispatch so the orderer group lead or platform operator can compare route choices:

| Cost option | Cost fields |
| --- | --- |
| 3PL warehouse inbound | transport fare, 3PL inbound fee, 3PL storage fee |
| Apartment direct dropoff with separate distribution | transport fare, separate worker distribution fee |
| Apartment direct with driver unit distribution | transport fare, driver unit distribution fee |
| Representative dropoff | transport fare |

When cargo weight is available, the planner also returns an estimated cost per kg. Options that need cold-chain or responsibility confirmation are shown as `NeedsConfirmation` rather than being hidden, because the decision can happen before customs release while operational details are still being confirmed.

## Payment And Cost Handling

The domestic transport step does not have to be treated as an invoice-receipt transaction by default. The preferred group-purchase model is:

1. Each orderer pays through an allowed method such as card, cash-like transfer, bank transfer, or platform credit.
2. The platform aggregates those payments into the group-purchase transport ledger.
3. The platform holds the transport amount until dropoff completion or pickup/dropoff evidence verification.
4. After the configured delay, usually 3 to 5 days, the platform pays the driver to the registered settlement account.
5. Receipt or cash-receipt evidence can be requested when the transaction requires it, but it is not mandatory for every platform-entrusted transport.

Settlement policies:

| Policy | Meaning |
| --- | --- |
| `PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff` | Default. Platform collects orderer payments, holds funds until dropoff or evidence verification, then pays the driver after the configured delay. |
| `PlatformPaysAndRechargesOrdererGroup` | Platform pays the transport cost first and recharges or allocates it to the orderer group ledger. |
| `PlatformAbsorbsAsPromotion` | Platform absorbs the cost as promotion or subsidy. |
| `ManualSettlement` | Admin confirms the final payer and allocation manually. |

The orderer group remains a settlement scope, not necessarily a legal business entity.

The planner returns `DriverPayoutPlan` so the caller can show or persist the payout trigger, payout delay, supported orderer payment methods, evidence requirements, and whether the driver settlement account has been confirmed.

## Driver Recommendation Display

Group-purchase transport recommendations are shown separately from general cargo recommendations in the driver app. The recommendation DTO carries:

| Field | Meaning |
| --- | --- |
| `운송의뢰유형코드` | `GeneralCargoTransport` or `GroupPurchaseCargoTransport` |
| `운송의뢰유형표시` | Driver-facing label such as `일반 화물` or `공동주문 운송` |
| `공동주문운송여부` | Whether the request originated from the group-purchase import/domestic transport flow |
| `세대배송포함여부` | Whether the recommendation may include apartment unit delivery beyond pickup/dropoff |
| `세대배송건수` | Expected unit-delivery count when known |
| `세대배송업무표시` | Driver-facing work-scope summary, such as `상하차 + 세대 문앞 33건` |

The driver list and detail screen surface this as chips before acceptance so the driver does not mistake a group-purchase unit-delivery job for a normal pickup/dropoff job.

The current implementation keeps the database handoff intentionally small. The dispatch queue preserves the source request type in `배차대기.원본의뢰유형`; `DispatchRecommendationRequestTypeClassifier` then maps that source type to the driver-facing recommendation metadata. When a platform domestic transport draft is available, the classifier can also consume `PlatformEntrustedDispatchQueueDraftDto` directly so apartment unit-delivery count and destination-specific work scope are preserved before the job is displayed to the driver.

Verified handoff cases:

| Transport draft | Driver recommendation result |
| --- | --- |
| Apartment direct distribution with driver unit delivery | `GroupPurchaseCargoTransport`, `공동주문 운송`, `세대배송포함여부=true`, `세대배송업무표시=상하차 + 세대 문앞 N건` |
| 3PL warehouse inbound | `GroupPurchaseCargoTransport`, `공동주문 운송`, `세대배송포함여부=false`, `세대배송업무표시=상하차 + 3PL 입고` |
| General cargo source type | `GeneralCargoTransport`, `일반 화물`, `세대배송포함여부=false` |

Next schema candidate: if the platform starts persisting unit-delivery count and destination type directly in `배차대기`, the live `api/v1/driver/recommendations` response can carry the same detailed scope without reconstructing it from the platform draft.

## Required Gates

| Gate | Required action code |
| --- | --- |
| Platform shipper profile exists | `ConfirmPlatformShipperProfile` |
| Customs or bonded release is ready | `ConfirmCustomsReleaseOrBondedRelease` |
| Bonded area pickup address is confirmed | `ConfirmBondedAreaPickupAddress` |
| 3PL dropoff address is confirmed | `ConfirmThreePlDropoffAddress` |
| Apartment complex dropoff address is confirmed | `ConfirmApartmentComplexDropoffAddress` |
| Apartment unit distribution scope is confirmed | `ConfirmApartmentUnitDistributionPlan` |
| Recipient address privacy handling is confirmed | `ConfirmRecipientAddressPrivacy` |
| Unit packages are pre-sorted before pickup | `ConfirmUnitSortationBeforePickup` |
| Overseas invoice and package labels are confirmed | `ConfirmOverseasUnitInvoiceAndLabeling` |
| Imported product information is registered | `ConfirmImportedProductInfoRegistration` |
| Product-info sticker source or storage is confirmed | `ConfirmProductInfoStickerStorage` |
| Product-info sticker matches imported product information | `ConfirmUnitProductInfoSticker` |
| Unit barcode scan lookup is confirmed | `ConfirmUnitBarcodeScanLookup` |
| Apartment distribution responsibility is confirmed | `ConfirmDistributionResponsibility` |
| Locked transport decision revision is confirmed | `ConfirmTransportDecisionRevision` |
| Cold-chain vehicle is confirmed | `ConfirmColdChainVehicle` |
| 3PL refrigerated/frozen facility is confirmed | `ConfirmColdChainThreePlFacility` |
| Cargo weight, volume, or pallet count is confirmed | `ConfirmCargoSpecification` |
| Admin confirms platform-entrusted transport | `ConfirmPlatformEntrustedTransport` |
| Orderer payment collection is confirmed | `ConfirmOrdererPaymentCollection` |
| Driver registered settlement account is confirmed | `ConfirmDriverSettlementAccount` |
| Driver payout policy is within the allowed range | `ConfirmDriverPayoutPolicy` |

## Implementation

- Planner: `Ssalddel.Contracts/Common/Orderer/공동구매플랫폼국내운송계획기.cs`
- Admin API: `POST /api/v1/admin/orderer/group-purchase-commerce-fulfillment-plans/{planId}/platform-domestic-transport-draft`
- Tests: `Ssalddel.Tests/Contracts/Common/Orderer/공동구매플랫폼국내운송계획기Tests.cs`
