# HS Code Database Roadmap

## Goal

HS code data should be treated as reference data first, not as ad hoc user input. The platform needs two separate layers:

- Official HS/HSK reference catalog: stable code hierarchy, names, descriptions, revisions, and source metadata.
- Platform agency intelligence: customs/import agency history, required documents, risk notes, disclosure consent, and paid access policy.

## Source Candidates

- Korea Customs Service CLIP: https://unipass.customs.go.kr/clip/index.do
- Korea Customs Service classification cases: https://unipass.customs.go.kr/clip/prlstclsfsrch/openULS0203042S.do
- KITA HS code guide: https://okfta.kita.net/hsCode?mnSn=207
- WCO Harmonized System overview: https://www.wcoomd.org/en/topics/nomenclature/overview/what-is-the-harmonized-system.aspx
- US CBP CROSS rulings: https://rulings.cbp.gov/
- EU Binding Tariff Information: https://taxation-customs.ec.europa.eu/customs/common-customs-tariff-cct/tariff-classification-goods/eu-binding-tariff-information-bti_en

## Implemented Schema

### `hs_code_catalog_versions`

Tracks the imported reference dataset version.

- `StandardCode`: e.g. `HS`, `HSK`
- `CountryCode`: e.g. `KR`, `US`, `EU`
- `CodeDigits`: e.g. 6 for international HS, 10 for Korean HSK
- `Revision`: e.g. `2022`, `2026`
- `SourceName`, `SourceUrl`
- Effective date range and import timestamp

### `hs_code_entries`

Stores the code tree.

- Code and normalized code
- Parent code
- Level: chapter, heading, subheading, national
- Korean/English names
- Description and search keywords
- Business category: unknown, food, general cargo, mixed
- Business category reason

### `hs_code_entry_risk_tags`

Stores operational risk tags that can be attached to one HS code entry.

- Tag type: food, quarantine/food notification, prepared food/supplement review, textile, chemical, electrical certification, battery, furniture, broker review recommended
- Label and reason shown to operators and shippers
- Source: system rule, admin override, broker review
- Active flag so an automatically generated tag can be hidden without deleting the audit trail

### `hs_code_classification_cases`

Stores public official classification cases.

- HS code
- Country and issuing authority
- Source reference number and URL
- Product name, goods description, decision reason
- Decision date

### `hs_code_platform_agency_experiences`

Stores platform-owned operational intelligence.

- HS code
- Agency type: customs agency or import agency
- Country route, case status, risk level
- Summary and required documents
- Contributor shipper
- Contributor consent flag
- Paid detail flag
- Paid access price and contributor reward rate
- Disclosure policy

## Access Policy

The default rule is privacy-first:

- Official code and official public cases can be shown as reference data.
- Platform operational cases are hidden by default.
- A shipper's case can be exposed only after explicit contributor consent.
- Paid detail access should reveal only anonymized, consented, non-identifying information.
- Payment revenue can be split between the platform and the data-contributing shipper.

## Admin Operation

The operation app now has an HS code operation screen at `/customs/hs-codes`.

- Search HS codes by code or name.
- Filter by business category or risk tag.
- Correct the large business category manually.
- Add, edit, activate, or hide risk tags.
- Store admin overrides and broker reviews separately from system-generated tags through the tag source field.

System rules should be treated as first-pass classification only. Customs brokers and admins should refine the data over time, especially for food, quarantine, battery, chemical, and certification-sensitive cargo.

## Import Plan

1. Load catalog metadata into `hs_code_catalog_versions`.
2. Import official HS/HSK code rows into `hs_code_entries`.
3. Assign initial business categories and system risk tags from HS chapter rules.
4. Import official public classification cases into `hs_code_classification_cases`.
5. Summarize internal customs/import agency operations into `hs_code_platform_agency_experiences`.
6. Keep contributor consent and paid-access checks outside the official catalog tables.
7. Let admins and customs brokers override risk tags where real operations show a different practical risk.

## Next Practical Step

Create a CSV/Excel importer for official HS/HSK rows with columns:

- `code`
- `parent_code`
- `level`
- `korean_name`
- `english_name`
- `description`
- `search_keywords`
- `effective_from`
- `source_reference`
