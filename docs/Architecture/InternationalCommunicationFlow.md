# International Communication Flow

Ssalddel should help logistics participants communicate across countries, languages, customs rules, and warehouse locations. The goal is not only overseas processing, but reducing misunderstanding when Korean and non-Korean participants work through the same platform.

## Scope

- Customs and HS-code review
- Overseas warehouse or domestic warehouse distinction
- Overseas partner, customs broker, shipper, seller, and foreign worker participation
- Import/export agency, delivery agency, and global commerce channel workflows
- Community posts that ask for cross-border clarification

## Community Surface

`PlatformCommunityHome` includes an `국제 소통` draft button in work mode.

The draft template asks for:

- related country or region
- related work area: customs, overseas warehouse, overseas partner, foreign participant, language
- communication counterpart
- question or clarification needed
- language or cultural care point
- next operational step

This keeps international communication attached to actual work instead of becoming a generic chat area.

## Visibility Principle

- Public/community observers should see enough context to learn from the issue.
- Personal identity, company-sensitive details, and import/customs case details should be masked unless disclosure is explicitly allowed.
- Customs brokers, operators, and direct parties may receive a role-checked detailed view later.
- The platform should prefer "minimum necessary disclosure" for cross-border issues because legal, cultural, and commercial risk can differ by country.

## Future Data Model Direction

Future work can add structured fields to international posts or linked work records:

- `CountryCode`
- `PreferredLanguage`
- `CounterpartyType`
- `CustomsRelated`
- `OverseasWarehouseRelated`
- `HS코드`
- `DisclosureLevel`
- `TranslationNeeded`

The first implementation remains a lightweight community draft template so the communication habit can form before heavier workflow tables are introduced.
