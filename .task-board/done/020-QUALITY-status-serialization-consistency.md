# QUALITY: Fix Status Serialization Consistency and Date Handling

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, storage

## Context & Motivation

The parser accepts "InProgress", "In Progress", "In-Progress" flexibly but serializes as "InProgress" — causing roundtrip inconsistency if a human edited the file with spaces. Also, date parsing silently defaults to DateTime.UtcNow on failure which could hide data corruption. Status serialization should use the human-readable "In Progress" form matching the spec format.

## Desired Outcome

- Status serialized as human-readable form ("In Progress", "On Hold") matching spec examples
- Date parsing provides meaningful feedback on failure rather than silently defaulting

## Acceptance Criteria

- [x] Serialize Status as "In Progress" and "On Hold" (with spaces) matching spec format
- [x] Parse continues accepting all variants ("InProgress", "In Progress", "In-Progress")
- [x] Date parsing returns null or logs a warning instead of silently defaulting to UtcNow
- [x] Add test: roundtrip with "In Progress" status preserves format
- [x] Add test: invalid date in metadata is handled gracefully (not silently replaced)

## Technical Approach

- In MarkdigMarkdownParser.SerializeStatus(), map TaskStatus enum to spec-compliant strings
- In ParseCreatedDate(), return DateTime? and let caller decide fallback behavior
- Update tests to verify human-readable status format

## Progress Log

- 2026-03-05 - Task created from backlog scan round 4
