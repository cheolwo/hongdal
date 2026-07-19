# Admin Community And Utility Fee Settings

This document fixes the admin-side requirement for Ssalddel's community and utility fee model.

## Operating Split

Admin configuration should not treat all platform features as paid features. It should separate four groups:

1. Free communication
2. Required work
3. Optional work utility
4. Small paid utility

## Free Communication

These features should normally stay free:

- Community posts
- Post comments
- Image attachments within normal limits
- Image-level comments
- Recommendations
- Report-board posts and observer-safe masking
- Questions, suggestions, field stories, and improvement proposals
- Work-to-community draft creation
- International communication drafts
- Basic event, education, and offline meeting announcements

The purpose is to gather people and keep voluntary cooperation alive.

## Required Work

These features are not optional utility features and should not be blocked by a paid setting:

- Transport completion photos
- Required file uploads and proof-of-delivery evidence
- Warehouse inbound, inspection, put-away, outbound, and packing state changes
- Legally or operationally required audit logs
- Payment, settlement, and delivery state transitions needed to complete the service

## Optional Or Paid Utility

These features can be configured, scoped, or priced:

- Legal and contract helper tools
- Business document generation, export, printing, and filing helpers
- Operator-approved promoted posts
- Recruiting, education, event, and offline meeting operation tools
- Scheduled posts and bulk notifications
- Advanced matching or profile/trust enhancement tools
- Work relationship analytics beyond the basic personal view
- Customs, HS code, import agency, or legal-review support workflows

## Admin Screen Direction

`AuxiliaryFeatureSettings` already provides the on/off model for optional features. A later pricing screen should add:

- Fee amount
- Free quota
- Trial period
- Waiver rules
- Refund rules
- Global and per-user scope
- Whether the feature is free communication, required work, optional utility, or paid utility

The guiding rule is simple: do not charge for ordinary communication; charge small amounts for tools that reduce real work.
