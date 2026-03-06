# QUALITY: Comprehensive Model and Storage Edge Case Tests

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, tests

## Context & Motivation

While test coverage is good (140 tests), some edge cases remain untested: CompletedDate transitions (Done → InProgress clears date), unicode titles, empty sections in markdown, very large task files, concurrent status transitions. Adding these ensures robustness.

## Desired Outcome

Comprehensive edge case coverage for models and storage layers.

## Technical Approach

- Add tests to TaskItemTests.cs for status transition edge cases
- Add tests to MarkdownParserTests.cs for edge case markdown formats
- Add filename generation edge case tests

## Progress Log

- 2026-03-05 - Task created from backlog scan round 5

## Acceptance Criteria

- [x] Test: ChangeStatus from Done to InProgress clears CompletedDate
- [x] Test: ChangeStatus from Done to Backlog clears CompletedDate
- [x] Test: TaskItem with unicode title generates valid filename
- [x] Test: Parse markdown with empty sections preserves them
- [x] Test: Parse markdown with only title and no metadata uses defaults
- [x] Test: GenerateFileName with various special character titles
- [x] All tests pass
