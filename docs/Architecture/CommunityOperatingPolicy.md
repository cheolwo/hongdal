# Community Operating Policy

Hongdal community is a gathering and coordination layer, not a paywalled social network. The platform should keep ordinary communication free so people can meet, ask, share, report, and coordinate without feeling forced into payment.

## Principle

- Communication is free by default.
- Community posts, comments, image comments, recommendations, questions, work stories, reports, suggestions, and lightweight announcements should remain free.
- Fees should apply only when the user uses tools that reduce real work, administrative, legal, or operational friction.
- Paid features should be small, optional, and clearly tied to practical convenience.
- The platform should not force participation. It should make voluntary cooperation easier.

## Free Community Surface

These functions are treated as community infrastructure:

- General posts and comments
- Privacy-safe activity signals derived from successful work logs
- Image attachments and image-level comments within normal limits
- Recommendations and engagement-based sorting
- Report-board posting and observer-safe masking
- Work-to-community draft creation
- International communication drafts and public coordination posts
- Basic event, education, and offline meeting announcements

The goal is to let people gather first. A community that charges too early loses the social density that makes the platform useful.

## Activity Signal Policy

The platform records work logs for audit, debugging, and operational accountability. Those raw logs are not community content. Community mode may use them only after converting them into privacy-safe activity signals.

Activity signals should:

- show that similar work is happening nearby in time, domain, or topic;
- help users discover related reviews, coordination needs, and peer behavior;
- use role-level labels such as anonymous driver, anonymous shipper, or anonymous warehouse worker;
- keep raw user identity, contact details, trace ids, IP addresses, user agents, raw URLs, query strings, and raw metadata out of the response.

The first implementation exposes `GET api/v1/community/activity-signals`. It reads successful work logs and returns anonymized signal cards for driver work, shipper transport, warehouse work, product journey, sales commerce, and community trust.

## Voting And Resolution Policy

Community voting is an information-exchange and coordination tool. It can help participants decide what to buy together, how to operate a shared process, whether to open a demand campaign, or which work rule should be adopted.

The platform should keep these stages separate:

1. Vote creation and participation
2. Vote close and result calculation
3. Resolution document draft
4. Legal or operator review
5. Participant signature
6. Signed resolution record

The platform must not label a vote result as legally effective merely because a majority option won. A resolution document may become useful evidence only after the right parties, authority, notice, consent, document text, signature evidence, and receiving-party requirements are checked.

The first implementation exposes `api/v1/community/votes`. Resolution documents store a document hash and use the shared electronic signature evidence model. Legal review is represented explicitly by `LegalReviewRequired` and `ReadyToSign` states.

## Small Paid Utility Surface

Fees can be considered for tools that help users complete work faster or with lower risk:

- Legal or contract document helpers
- Business document generation, export, printing, and filing aids
- Premium notice placement or operator-approved promoted posts
- Recruiting, education, event, or offline meeting operation tools
- Advanced matching and trust/profile enhancement tools
- Work relationship snapshot analytics beyond the basic personal view
- Bulk notifications, scheduled posts, and campaign tools for businesses
- Customs, HS code, import agency, or legal-review support workflows

These features should be optional. The base platform should still work without them.

## Admin Controls

The admin surface should distinguish:

- Free communication features: normally always enabled unless abuse control is needed.
- Required work features: cannot be disabled because they keep the service legally or operationally valid.
- Optional utility features: can be enabled, disabled, priced, or scoped globally or per user.
- Paid utility features: should have a low-friction fee policy and clear usage boundary.

Existing `AuxiliaryFeatureSettings` can handle on/off scope for optional utility features. A later pricing screen should manage the fee amount, free quota, trial range, and refund/waiver rules.

## Product Tone

Do not present the community as a marketplace where every interaction is monetized. Present it as a shared platform space. Paid functions should feel like practical tools attached to work, not admission fees for belonging.

The desired shape is:

1. People gather because communication is open.
2. Trust forms through repeated work, posts, comments, and relationship snapshots.
3. Users optionally pay small amounts for tools that make work, legal handling, operations, or promotion easier.
4. The platform uses subscription and utility fees to operate sustainably without pushing unnecessary fees into ordinary conversation.
