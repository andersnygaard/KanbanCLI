# REFACTOR: Extract Methods in BoardService to Eliminate Duplication

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: refactor, services, quality

## Context & Motivation

BoardService.GeneratePlanningBoard() (65 lines) and BuildEmptyBoard() duplicate markdown header generation. The method also mixes filtering, sorting, and formatting in a single long method.

## Desired Outcome

BoardService methods are shorter, share common header generation, and have clearer single-purpose methods.

## Technical Approach

- Create `string BuildHeader()` that generates the common `# Planning Board` header
- Create `string BuildPrioritySection(IEnumerable<TaskItem> priorities)`
- Create `string BuildCompletedSection(IEnumerable<TaskItem> completed)`
- GeneratePlanningBoard composes these methods
- BuildEmptyBoard delegates to BuildHeader

## Progress Log

- 2026-03-05 - Task created from backlog scan round 3

## Acceptance Criteria

- [x] Extract shared header generation into private BuildHeader() method
- [x] Extract priority section formatting into a dedicated method
- [x] Extract recently completed section formatting into a dedicated method
- [x] BuildEmptyBoard() uses the shared BuildHeader() method
- [x] GeneratePlanningBoard() is under 30 lines
- [x] All existing BoardService tests pass unchanged
