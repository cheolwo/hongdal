# Community Operating Policy

Hongdal community is a gathering and coordination layer, not a paywalled social network. The platform should keep ordinary communication free so people can meet, ask, share, report, and coordinate without feeling forced into payment.

## Development Philosophy

Hongdal is built around the idea of helping people live more `알뜰살뜰`: careful with money, time, movement, labor, trust, and relationships. This is not only a brand phrase for mart workflows. It is the product philosophy behind the community-first platform.

The platform should therefore be judged by whether it reduces real-life friction: fewer wasted trips, fewer unclear promises, fewer repeated explanations, less anxiety around handoff and settlement, and more voluntary cooperation between people who already share a neighborhood, task, or need. Revenue features may exist later, but they should not override the basic aim of helping people gather, coordinate, record, and complete ordinary work with less waste.

## 원장보다 먼저 원함 확인

Before a user creates a ledger, the UI should ask what the user wants and show what Hongdal can and cannot do for that wish. The Korean word `원장` can be treated productively as starting from `원함` or `願`: a wish, request, or desired outcome that has not yet become executable work.

This pre-ledger step should show:

- what the user wants to solve or make happen;
- who should participate, confirm, or help;
- where, when, and under what conditions the work should happen;
- how Hongdal can turn the wish into ledger blocks, composition rules, OS scheduling, engine judgment, and API handoff candidates;
- what the user and counterpart still need to enter, confirm, prove, or dispute themselves.

This keeps expectations honest. Hongdal can structure, guide, schedule, recommend, and record. It should not imply that every wish can be automatically fulfilled, legally guaranteed, paid, or verified by the platform.

## 원함-원장 판단 보고서

원함 확인의 결과는 단순 안내 문구로 끝나지 않고, 원함을 원장으로 바꿀 수 있는지 판단하는 보고서 형태로 남기는 것이 바람직하다. 이 보고서는 사용자의 바람을 넓게 듣되, 홍달이 더 좋게 만들 수 있는 범위를 좁고 책임 있게 정리한다.

보고서는 다음 순서로 정리한다.

| 항목 | 정리할 내용 | 판단 기준 |
| --- | --- | --- |
| 사용자의 원함 | 사용자가 바라는 일, 해결하고 싶은 생활 문제, 함께 처리하고 싶은 일 | 원함이 너무 추상적이면 커뮤니티 대화로 먼저 남긴다 |
| 홍달이 다룰 수 있는 범위 | 참여자, 장소, 시간, 물건/업무, 상태, 증빙, 정산 표시, 확인 책임으로 정리 가능한 부분 | 적어도 하나 이상의 원장 블록으로 구조화할 수 있어야 한다 |
| 원장화 판정 | 바로 원장 생성, 추가 정보 필요, 커뮤니티 대화 유지, 홍달 처리 범위 밖으로 분류 | 플랫폼 보증이나 자동 실행 약속으로 오해될 요청은 원장화를 보류한다 |
| 필요한 원장 구성 | 참여자, 장소, 물건, 재고, 상태, 증빙, 정산, 인계 같은 원장 블록 | 다음 행동을 열기 전에 필요한 최소 블록을 표시한다 |
| 홍달이 도울 일 | 다음 행동 안내, 상태 변경, 알림, 추천, 보류 판단, 증빙 첨부, 정산 표시 | 시스템이 구조화, 기록, 스케줄링, 추천, handoff로 도울 수 있는 일만 적는다 |
| 사용자가 직접 해야 할 일 | 실제 약속, 상대방 확인, 현장 확인, 결제 사실, 분쟁 대응, 신고 보완 | 플랫폼이 자동 보증하지 않는 책임을 분리해 적는다 |
| OS/엔진/API 연결 | 어떤 하위 OS가 흐름을 잡고, 어떤 엔진이 판단을 돕고, 어떤 API가 상태를 바꾸는지 | 실제 실행은 OS가 아니라 API, UseCase, 메시지, application service가 맡는다 |

원장화 판정은 다음 네 가지 중 하나로 둔다.

- `원장 생성 가능`: 원함이 원장 블록으로 충분히 구조화되어 다음 행동을 열 수 있다.
- `추가 정보 필요`: 참여자, 장소, 시간, 물건, 상태, 증빙, 정산 표시 중 핵심 정보가 부족하다.
- `커뮤니티 대화 유지`: 아직 실행할 업무보다 의견, 모집, 질문, 제안에 가깝다.
- `홍달 처리 범위 밖`: 홍달이 구조화하거나 기록할 수는 있어도 보증, 법적 판단, 강제 이행, 자동 결제 확정처럼 플랫폼 책임으로 처리하면 안 되는 요청이다.

이 보고서는 나중에 화면과 AI 판단의 공통 입력이 된다. 화면은 보고서를 읽어 어떤 섹션을 먼저 보여줄지 정하고, AI 판단 보조는 보고서의 원장 블록과 판정 사유를 근거로 추천, 보류, 추가 질문을 만든다.

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
- Community ledger drafts that turn a conversation into a shared work board

The goal is to let people gather first. A community that charges too early loses the social density that makes the platform useful.

## Community Ledger Skeleton

Community conversations can become lightweight ledgers when participants decide to handle a real-life task together. A ledger draft is not a platform-guaranteed contract. It is a shared board where participants can name roles, record state, attach optional evidence, and mark payment or completion confirmations for each other.

The first skeleton keeps ledger creation inside the community posting flow. A user selects a ledger type, receives a role template, and then edits the post body before publishing. This avoids forcing every casual conversation into a rigid workflow while still giving users a path from talk to action.

Community is the center of entry, not the only processor. When a community ledger becomes concrete enough to run as work, the server classifies its character and hands it to the appropriate HIOPS sub-OS. The selected OS is primarily a scheduler and orchestrator: it reads ledger state, applies composition rules, chooses queues or engines, and decides which API or application service should be called next. The actual execution remains in HTTP APIs, controller/use-case boundaries, messages, or internal application service calls.

Initial ledger-to-OS routing:

| Ledger template | Target OS | Engine hints |
| --- | --- | --- |
| Cargo transport ledger | Domestic cargo transport OS | Transport request dispatch engine |
| Food order ledger | Food delivery OS | Food delivery dispatch engine |
| Food delivery ledger | Food delivery OS | Food delivery dispatch engine, transport request dispatch engine |
| HongdalMart instant-delivery ledger | HongdalMart urban logistics OS | Picking batch engine, food delivery dispatch engine, transport request dispatch engine |
| Warehouse outbound ledger | Warehouse-commerce fulfillment OS | Outbound batch engine, picking batch engine, transport request dispatch engine |
| Warehouse inbound ledger | Warehouse-commerce fulfillment OS | Community activity signal engine first, then warehouse workflow policy |
| Local sale ledger | Warehouse-commerce fulfillment OS | Outbound batch engine, transport request dispatch engine |
| Group purchase ledger | Group purchase import OS | Grouping engine, outbound batch engine, transport request dispatch engine |
| Errand or generic life-request ledger | Community trust OS | Community activity signal engine |

## OS Scheduling And API Execution

In this architecture, an OS does not mean a monolithic service that performs every business action itself. It is the scheduling layer for a ledger. The OS should answer questions such as:

- which composition rules are already satisfied;
- which section or action can be opened next;
- which queue, engine, API, or use case should receive the next command;
- whether a handoff should create a new relational projection or update an existing one;
- whether a failed call should be retried, paused, or returned to the community ledger for participant correction.

Actual processing belongs to practical execution surfaces. For example, a cargo transport ledger may be scheduled by the domestic cargo transport OS, but transport request creation still uses `POST api/v1/shipper/requests`, driver progress still uses `api/v1/driver/transports`, and warehouse work still uses `api/v1/warehouse-operations`. The OS keeps the order and policy coherent; APIs and application services mutate the concrete work records.

Supported starter ledger templates:

- Cargo transport ledger: requester, carrier, pickup confirmer, receiver, settlement confirmer
- Food order ledger: orderer, seller, cook, handoff person, receiver
- Food delivery ledger: delivery requester, pickup handler, deliverer, receiver confirmer, settlement confirmer
- HongdalMart instant-delivery ledger: orderer, mart picker, packer, deliverer, receiver confirmer
- Warehouse outbound ledger: outbound requester, picker, inspector, packer, carrier
- Warehouse inbound ledger: inbound requester, supplier, inbound inspector, storage handler, close confirmer
- Local sale ledger: seller, buyer, handoff person, confirmer, settlement confirmer
- Group purchase ledger: recruiter, participant, buyer, distributor, settlement confirmer
- Errand or generic life-request ledger: requester, performer, confirmer, participant, closer

The template roles are defaults only. Participants should be able to rename, add, remove, and reassign roles. By default, a normal user should be able to participate across roles when the ledger context makes it reasonable. A role is therefore a visible participation label and work-context hint, not a hard authorization boundary.

Community participation should be anonymous or pseudonymous by default. The public display name for posts, comments, ledgers, reports, votes, and activity signals should be a nickname, handle, role label, or anonymous participant label rather than a required real name. Identity verification can exist as an optional trust signal when a user wants it or when a specific regulated workflow later requires stronger checks, but it must stay separate from the public display name and should not force real-name community activity.

HongdalMart should be separated from the general warehouse outbound workflow at the ledger boundary. A warehouse outbound ledger is for sales-channel or ordinary warehouse work where picking, inspection, packing, and transport handoff can happen as a broader fulfillment process. A HongdalMart ledger is for short-cycle urban inventory work where a mart order, nearby stock, pick-pack completion, driver pickup, and customer delivery are coordinated as one instant-delivery flow. It can reuse warehouse events and dispatch engines, but it should route through the HongdalMart urban logistics OS rather than being treated as a `warehouse-outbound` ledger.

## AI-Assisted Ledger Flow Classification

Users should not always have to choose the perfect ledger template first. A community post or loose ledger draft can already contain enough shape for AI to suggest which workflow it belongs to. The classifier should look at the ledger title, body, UI sections, action hints, states, flexible attributes, and evidence labels, then return scored candidates rather than silently mutating the ledger.

The first implementation can be deterministic and rule-based. It should compare the draft shape with the ledger template catalog, composition rules, target OS metadata, engine hints, and practical API surfaces. Later, HIOPS AI or an LLM can sit behind the same input/output contract to add richer natural-language interpretation.

The classifier response should include:

- the primary ledger template candidate and target OS;
- alternative candidates with scores when the shape is ambiguous;
- matched signals such as `도심 재고`, `포장 완료`, `운송 인계`, or `상차지`;
- missing required signals from composition rules;
- related composition rule codes and processing surface hints;
- a human-review flag when top candidates are too close or the shape is too weak.

This keeps AI in the interpretation layer. The AI may say that a draft looks like a HongdalMart instant-delivery flow, a warehouse outbound flow, a cargo transport flow, or only a loose community request. It should not directly create relational work records, award experience, or call operational APIs until the ledger is confirmed and handed to the proper OS/API boundary.

The system should reason about action hints such as state change, evidence attachment, payment marking, completion confirmation, participant invitation, and ledger closing separately from the visible role name. These action hints can guide UI layout and audit messages, but they should not prevent ordinary participation merely because the user picked a different role label.

Role or action restrictions should be exceptional. They should be applied only after signals such as a report, dispute state, repeated abuse pattern, explicit operator moderation, or a ledger-specific safety rule. A restriction should be scoped narrowly to a user, ledger, role label, or action rather than globally reducing the user's ability to participate in the community.

## Community Level And Experience Policy

A new member starts at level 1 after signup and login. Level is not a role lock, legal qualification, payment guarantee, or platform certification. It is a visible participation and trust signal that helps the community understand how much ordinary activity the user has accumulated.

Experience should come from helpful community and ledger actions, such as writing posts, joining comments, creating ledger drafts, confirming ledger states, attaching optional evidence, confirming completion, or submitting reports that are accepted after review. These actions should be recorded from auditable community or ledger events rather than from arbitrary client-side counters.

API handlers should not update user levels directly. A business API should complete its own work first, publish a domain/application event, and let a community experience handler translate that event into an experience award. Any API whose purpose is to move a work item from one state to another should be treated as an experience-event candidate because the user has usually advanced a physical or operational situation, not merely read data.

For Hongdal 1.0 domestic transport, the first concrete mappings are:

| Source event | Experience event | Base experience | Processing boundary |
| --- | --- | ---: | --- |
| `운송상차지도착됨Event` from `POST api/v1/driver/transports/{id}/arrive-pickup` | `TransportPickupArrived` | 10 | `운송경험치EventHandler` records a low-weight state-change experience award after pickup arrival commits. |
| `운송상차완료됨Event` from `POST api/v1/driver/transports/{id}/pickup-complete` | `TransportPickupCompleted` | 20 | `운송경험치EventHandler` records an experience award request after the transport state change commits. |
| `운송하차지도착됨Event` from `POST api/v1/driver/transports/{id}/arrive-dropoff` | `TransportDropoffArrived` | 10 | `운송경험치EventHandler` records a low-weight state-change experience award after dropoff arrival commits. |
| `운송인수완료됨Event` from `POST api/v1/driver/transports/{id}/complete` | `TransportDropoffCompleted` | 30 | `운송경험치EventHandler` records an experience award request after final delivery confirmation commits. |
| `운송문제신고됨Event` from `POST api/v1/driver/transports/{id}/report-issue` | `TransportIssueReported` | 4 | `운송경험치EventHandler` records a low-weight issue-report signal after the transport memo and optional evidence commit. |
| `음식점주문수락됨Event` from `POST api/v1/food-orders/{orderNo}/restaurant-acceptance` | `FoodOrderAccepted` | 12 | `음식주문경험치EventHandler` records the restaurant-side acting user when `처리UserId` is present. |
| `창고입고완료됨Event` from `POST api/v1/warehouse-operations/inbounds/{inboundId}/complete` | `WarehouseInboundCompleted` | 20 | `창고작업경험치EventHandler` records inbound completion after inventory items are created. |
| `창고입고검수완료됨Event` from `POST api/v1/warehouse-operations/inventory/{inboundItemId}/inspect` | `WarehouseInboundInspected` | 12 | `창고작업경험치EventHandler` records inspection completion after available and defect quantities are updated. |
| `창고적재위치배정됨Event` from `POST api/v1/warehouse-operations/inventory/{inboundItemId}/put-away` | `WarehousePutAwayCompleted` | 8 | `창고작업경험치EventHandler` records put-away location assignment. |
| `창고피킹완료됨Event` from planned `POST api/v1/warehouse-operations/picking-tasks/{taskKey}/complete` | `WarehousePickingCompleted` | 12 | `창고작업경험치EventHandler` records picking completion. HongdalMart may reuse the event, but the ledger remains routed to the HongdalMart urban logistics OS. |
| `창고포장완료됨Event` from `POST api/v1/warehouse-operations/inventory/{inboundItemId}/pack` | `WarehouseInventoryPacked` | 14 | `창고작업경험치EventHandler` records packing completion after inventory is prepared for outbound work. |
| `창고재위탁운송생성됨Event` from `POST api/v1/warehouse-operations/inventory/reconsignment` | `WarehouseReconsignmentCreated` | 18 | `창고작업경험치EventHandler` records the handoff from warehouse inventory to transport request. |

The experience handler is intentionally a separate layer. It may start by writing an auditable `ExperienceAward` activity log, then later switch to a dedicated MongoDB collection or relational projection for user level aggregation without changing the transport API contract.

State-changing APIs are candidates, not automatic rewards. Dispatch acceptance is an operational commitment and relationship signal, but it should not award experience until the transport reaches dropoff or delivery completion. Negative or reversible actions such as dispatch rejection and acceptance cancellation still publish operational events, but they should not award experience by default because that can encourage noisy behavior. They can remain audit and moderation signals unless an operator later marks a report or correction as helpful.

Initial levels are deliberately simple:

| Level | Label | Required experience | Meaning |
| --- | --- | ---: | --- |
| 1 | Joined member | 0 | Can post, comment, draft ledgers, and participate in basic state confirmation. |
| 2 | Active member | 100 | Has repeated community or ledger participation signals. |
| 3 | Trusted member | 300 | Has completion and low-dispute signals that can help other users judge collaboration. |
| 4 | Operations collaborator | 700 | Can be surfaced as a helpful reviewer or pattern organizer without becoming an administrator. |

Reports, disputes, repeated abuse signals, and operator moderation should pause, exclude, or reverse experience gains for the affected action until the issue is reviewed. A higher level should never override a ledger restriction, privacy rule, or operator moderation decision.

Evidence is optional by default. A participant may attach an image, memo, or link when it helps, but the platform should not block progress merely because a proof image was not uploaded. Payment language should stay participant-centered: use terms like payment marked, counterpart confirmed, and settlement note instead of implying platform custody or platform verification unless an actual regulated payment provider flow is attached.

## Ledger Block Model

A ledger should be treated as a composition of reusable blocks, not as one rigid form. The block is the smallest unit that UI, AI judgment, composition rules, and OS/API handoff can all understand together.

The full layer boundary between ledger blocks, composition rules, OS, engines, APIs, and stores is defined in [HIOPS Layer Model](HIOPSLayerModel.md). This section applies that layer model to the community-ledger intake surface.

Starter block types:

| Block type | Meaning | Typical judgment use |
| --- | --- | --- |
| Participant | people, role labels, confirmation actors | who can confirm, who may be restricted, who needs notification |
| Place | pickup, dropoff, warehouse, delivery, storage location | distance, route, service area, driver or worker matching |
| Item or order | cargo, menu, sale item, mart order, request body | workflow classification and handling constraints |
| Inventory or quantity | urban stock, inbound basis, outbound items, group quantity | warehouse choice, picking, batching, group purchase thresholds |
| State | progress state, cooking, picking, packing, delivery, close | next available actions and experience-event candidates |
| Evidence | image, memo, signature, barcode, exception record | proof, dispute review, settlement hold or release |
| Settlement | payment mark, counterpart confirmation, hold, dispute note | participant-centered settlement status without implying platform custody |
| Handoff | target OS, engine, API, service hint, external reference | when a ledger becomes executable work and where it should go next |

The community ledger template catalog should expose these blocks explicitly. Dynamic UI can render sections from the blocks, the AI classifier can score a ledger by matched block signals, composition rules can decide which blocks must exist before an action opens, and HIOPS can use the handoff block to schedule the proper API or application service call.

This keeps MongoDB flexible without turning ledgers into unstructured JSON. A ledger may add custom attributes, but the reusable block catalog gives the system a stable vocabulary for AI judgment, UI rendering, RDB projection, and OS scheduling.

## Ledger Composition Rules

A dynamic page should be generated from a ledger that already has enough structure. The UI should not expose every action merely because a template exists. It should read the ledger composition rules first, then decide which sections, actions, and follow-up pages are available.

Initial composition rules:

| Rule | Meaning | Gated surface |
| --- | --- | --- |
| Transport request before pickup/dropoff | A cargo transport ledger needs participants, pickup place, dropoff place, and cargo conditions before pickup or dropoff actions are shown. | Pickup arrival, pickup confirmation, dropoff completion, receiver confirmation |
| Food order before delivery | A food delivery ledger should be generated from a food order ledger or a pickup request that already has pickup place, destination, and receiver conditions. | Pickup arrival, pickup completion, delivery completion, receiver confirmation |
| Mart order before picking/packing | A HongdalMart ledger needs a mart order, urban stock, and participants before picking or packing actions are shown. | Stock check, picking, packing completion |
| Mart packed before delivery pickup | A HongdalMart delivery pickup can be recommended early, but actual driver handoff and customer delivery actions open after packing is complete. | Pickup readiness, driver handoff, delivery completion |
| Inbound or stock before outbound | A warehouse outbound ledger needs an inbound ledger, stock record, or operator-approved stock basis before picking starts. | Picking, inspection request, outbound progress |
| Outbound before handoff transport | A transport handoff from warehouse work opens only after picking, inspection, and packing are settled. | Transport handoff, later cargo transport request |
| Sale item before reservation settlement | A local sale ledger needs the item and counterpart before reservation, payment mark, or delivery handoff actions are useful. | Reservation, payment mark, handoff schedule, delivery completion |
| Recruitment before purchase distribution | A group purchase ledger needs participants and quantity decisions before purchase, distribution, and settlement surfaces are opened. | Purchase progress, distribution, payment mark |
| Request and participant before progress | Generic requests, inbound work, and food orders need at least a request shape and participants before progress states are meaningful. | Start progress, completion confirmation, hold/dispute state |

These rules are not meant to make the community rigid. They are the guardrails that let Hongdal render dynamic pages from a ledger without showing impossible actions. If a user creates a loose community post first, the post can remain loose. Once the post becomes a ledger, the composition rules decide what must be filled before the OS or engine receives it.

## Mongo Ledger Source And Relational Projection

The community ledger source of truth should be a MongoDB document, not a fixed relational table. A ledger can start as a loose community conversation and then gain different sections, states, roles, evidence, external references, and OS handoff records depending on its type. That shape is too flexible for a single stable SQL table.

The recommended primary collection is `community_ledgers`. Each document should keep:

- ledger id, template key, target OS code, current state, and community post/thread references;
- flexible attributes for the selected ledger type such as pickup place, inbound item, menu, warehouse location, sale item, group purchase quantity, or errand details;
- participant roles and permissions as the user actually configured them, not only the default template;
- composition-rule satisfaction state;
- evidence references such as uploaded image object names, links, memo ids, and optional payment marks;
- OS/API handoff history with route/service hints, request ids, relational entity ids, and timestamps;
- privacy-safe public summary fields that can be shown back in community.

Relational DB records should be projections or linked work entities, not the flexible ledger source. When a ledger becomes concrete enough, the OS/API may create or update relational records such as:

- transport request and driver transport progress records;
- food order and restaurant acceptance records;
- warehouse inbound, inventory, packing, and reconsignment records;
- group purchase demand, shipment, and domestic transport records;
- community post summaries and privacy-safe activity signals.

Every relational projection should keep a reverse link such as `CommunityLedgerId` or a domain-specific equivalent. This lets MongoDB preserve the full community-ledger shape while the relational DB keeps the stable data needed for transactional workflows, indexed queries, authorization, settlement, and reporting.

The OS handoff remains conceptual and scheduling-oriented. In code, the handoff can be an HTTP API call, a controller/use-case boundary, a message, or an internal application service call. The ledger template should therefore record both the target OS scheduler and the practical processing surfaces that currently exist.

## Best Ledger Pattern Sharing

Community ledgers should also become a way for people to learn better ways of working together. A useful ledger can be shared back to the community as a best ledger pattern, but the shared pattern is not the raw private ledger. It should be a reusable work method with sensitive details removed.

A best ledger pattern should preserve:

- the ledger type and target OS;
- the role split that made the work easier;
- the UI sections that helped participants see the right information;
- the actions that moved the work forward;
- the optional evidence and confirmation points;
- discussion prompts that help other users adapt the pattern.

A best ledger pattern should not expose:

- private participant names, contacts, addresses, raw payment proof, or private memo text;
- exact cargo, order, or transaction details unless the participants deliberately make them public;
- claims that the platform verified payment, identity, legal effect, or performance unless the relevant OS and provider integration actually did so.

This creates a loop: the community starts a ledger, the right OS schedules the next work steps, APIs and use cases execute them, and the useful shape of that ledger can return to the community as a reusable work pattern. Over time, Hongdal can rank or recommend these patterns by completion rate, low dispute rate, clarity of roles, and participant feedback.

## Unified Community Home

The common community home is the first unified app shell. Client-specific apps may still provide their own work-mode content, but their entry points should be progressively exposed through the shared home rather than only through app-local navigation.

The shared home uses a workspace catalog to map:

- a visible life-work area such as cargo transport, food order, warehouse inbound, local sale, or errand;
- the ledger template that defines default roles and permissions;
- the target OS that should schedule and orchestrate the ledger when it becomes concrete work;
- the current route that can open the existing static screen while the dynamic UI engine is still being built.

This lets Hongdal move from many predetermined client apps toward one community-centered workspace. A user can start from a conversation, choose a ledger shape, fill a draft, and then continue into the current work screen. Over time, app-specific home panels should become dynamic ledger sections rendered from the ledger template and OS metadata, not separate hard-coded product surfaces.

## Activity Signal Policy

The platform records work logs for audit, debugging, and operational accountability. Those raw logs are not community content. Community mode may use them only after converting them into privacy-safe activity signals.

Activity signals should:

- show that similar work is happening nearby in time, domain, or topic;
- help users discover related reviews, coordination needs, and peer behavior;
- use role-level labels such as anonymous driver, anonymous shipper, or anonymous warehouse worker;
- keep raw user identity, contact details, trace ids, IP addresses, user agents, raw URLs, query strings, and raw metadata out of the response.

The first implementation exposes `GET api/v1/community/activity-signals`. It reads successful work logs and returns anonymized signal cards for driver work, shipper transport, warehouse work, product journey, sales commerce, and community trust.

## Implementation Boundary

Community controllers should stay thin. HTTP routing, authorization policy, request binding, and file stream opening can remain in controllers. Post creation, comment handling, recommendations, reports, moderation state, response projection, and validation belong in `커뮤니티게시글UseCase` so that community behavior can be tested and reused by app-specific community modes.

Community voting follows the same boundary. `커뮤니티투표Controller` delegates voting, closing, resolution document draft creation, and signature state changes to `커뮤니티투표UseCase`; the controller only maps the HTTP route and response shape.

Activity signals also use the same boundary. `커뮤니티활동신호Controller` delegates privacy-safe work-log projection to `커뮤니티활동신호UseCase`, while `CommunityActivitySignalService` remains the lower-level query/projection service.

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
