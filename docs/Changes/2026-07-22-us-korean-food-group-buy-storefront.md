# U.S. Korean food group-buy storefront

## Goal

Give U.S. users one continuous page where they can:

1. Browse Korean dishes from the official recipe catalog.
2. Choose a structured recipe ingredient.
3. Load and explicitly select an indexed Korean HSK or U.S. HTS planning reference.
4. Enter a five-digit U.S. ZIP code, desired quantity, and storage temperature.
5. Preview the server-side buyer group without saving anything.
6. Register nonbinding interest in that group without payment or order creation.

The route is `/us/korean-food-group-buy` in both `Ssalddel.WebApp` and `SsalddelApp`.

## Server grouping

The storefront reuses the existing authenticated automatic-grouping API:

- `POST /api/v1/orderer/group-purchase-auto-groups/placement-preview`
- `POST /api/v1/orderer/group-purchase-auto-groups/demands`

The grouping policy combines demand with the same:

- official ingredient product key;
- U.S. ZIP delivery scope;
- ambient, chilled, or frozen storage code;
- LCL logistics mode.

The initial storefront target is five interested buyers or 30 kg. Reaching that target only opens a confirmation review. It does not automatically select a supplier or service provider, collect payment, create a purchase order, or make an import declaration.

The demand source key is a stable SHA-256-derived pseudonymous value. The browser submits the authenticated user context, but the controller remains authoritative and replaces the orderer key with the authenticated claim.

## HS and HTS boundary

Tariff mappings remain evidence-backed candidates. The page does not automatically choose a code. A user must explicitly select a planning reference after reviewing its jurisdiction, catalog revision, confidence, required product details, source, and last-check date.

- A Korean HSK reference can support export planning but is not automatically a U.S. entry code.
- A U.S. HTS reference can support import planning but is not declaration-ready unless the stored evidence says so and the responsible professional confirms the actual product.
- Product form, processing, composition, origin, packaging, and intended use can change classification.

## Commercial boundary

The page describes pooled volume as negotiation leverage, not as a guaranteed discount. A purchasable offer still needs a reviewed total that can include:

- supplier product price;
- international and domestic freight;
- customs, duties, taxes, and payment fees;
- food eligibility, labeling, origin, and supplier evidence work;
- broker, warehouse, fulfillment, and final-mile charges.

The current action stores `InterestOnly` demand with `NotPaid` payment status. No customer funds are held and no platform fee is charged in this flow.

## Main implementation files

- `Ssalddel.Ui.Common/Areas/App/Components/Information/UnitedStatesKoreanFoodGroupBuyStorefront.razor`
- `Ssalddel.Ui.Common/Areas/App/ViewModels/UnitedStatesKoreanFoodGroupBuyStorefrontViewModel.cs`
- `Ssalddel.Ui.Common/Areas/App/Services/공동구매실행Service.cs`
- `Ssalddel.WebApp/Pages/UnitedStatesKoreanFoodGroupBuyPage.razor`
- `SsalddelApp/Components/Pages/UnitedStatesKoreanFoodGroupBuyPage.razor`

## Follow-up checkpoints

Before converting interest into a paid group purchase, add explicit operator-reviewed checkpoints for supplier offer acceptance, landed-cost disclosure, importer responsibilities, food and labeling eligibility, payment authorization, cancellation and refund terms, and fulfillment service selection.
