# QUALITY: Enhance Board and Column Models with Utility Methods

**Status**: Done
**Created**: 2026-03-05
**Priority**: Low
**Labels**: quality, models

## Context & Motivation

Board and Column records are pure data carriers with no behavior methods. Adding utility methods like Board.GetColumn(TaskStatus), Column.IsEmpty, Column.TaskCount would improve readability at call sites and align with the "rich domain models" principle in the spec.

## Desired Outcome

Board and Column models have useful utility properties/methods that simplify code at call sites.

## Progress Log

- 2026-03-05 - Task created from backlog scan round 6

## Acceptance Criteria

- [x] Add Board.GetColumn(TaskStatus) method that finds column by status
- [x] Add Column.IsEmpty property
- [x] Add Board.TotalTaskCount property
- [x] Add tests for all new model methods
- [x] Refactor existing call sites to use new methods where cleaner
- [x] All tests pass
