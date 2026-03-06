# QUALITY: Expand Test Coverage for Services and Filtering

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, tests

## Context & Motivation

BoardServiceTests has only 4 tests and is missing edge cases. No dedicated FilterCriteria tests exist. The spec lists many test scenarios that aren't implemented yet (multi-criteria filter, empty board, null completed dates, etc.).

## Desired Outcome

Comprehensive test coverage for BoardService edge cases and FilterCriteria scenarios matching the spec's test strategy section.

## Technical Approach

- Expand BoardServiceTests.cs with edge case scenarios
- Create FilterCriteriaTests.cs in Models test folder
- Add multi-criteria MatchesFilter tests to TaskItemTests.cs
- Add error scenario tests to TaskServiceTests.cs

## Progress Log

- 2026-03-05 - Task created from backlog scan round 2

## Acceptance Criteria

- [x] Add BoardService tests: empty board, null completed dates, task name truncation, mixed priorities
- [x] Add FilterCriteria tests: multi-criteria filtering, empty criteria matches all, priority-only filter
- [x] Add TaskItem tests: MatchesFilter with combined label+type+priority criteria
- [x] Add TaskService tests: error propagation scenarios, GetAllByColumn calls
- [x] All existing tests still pass
