# QUALITY: Complete Service Layer Test Coverage

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, tests, services

## Context & Motivation

While test coverage is good at 158 tests, the service layer could benefit from additional scenario coverage: GetBoard assembly tests, SavePlanningBoard error scenarios, and UpdateTask with various field changes. Also need tests verifying that BoardService.GetBoard() correctly assembles Board from repository data.

## Desired Outcome

Comprehensive service layer test coverage with all orchestration paths tested.

## Acceptance Criteria

- [x] Add BoardService.GetBoard tests: verifies Board is correctly assembled with all columns
- [x] Add BoardService.GetBoard tests: empty columns are present but empty
- [x] Add TaskService.UpdateTask tests: title change persisted, labels change persisted
- [x] Add TaskService.GetAllTasks tests: verifies delegation to repository
- [x] Add edge case: MoveTask to same status is no-op or handled gracefully
- [x] All tests pass

## Progress Log

- 2026-03-05 - Task created from backlog scan round 6
