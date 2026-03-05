# REFACTOR: Extract KanbanApp.Run() into Command Dispatcher Pattern

**Status**: Done
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: refactor, tui, quality

## Context & Motivation

KanbanApp.Run() is 88 lines with a 9-branch switch statement handling all commands directly. Extracting into a command dispatcher pattern would improve readability, testability, and extensibility.

## Desired Outcome

Clean command dispatch: each command is a clearly separated handler, and the main loop is short and readable.

## Acceptance Criteria

- [x] Extract command dispatch from Run() into a dictionary-based or method-based dispatcher
- [x] Run() should be under 30 lines focusing only on the main loop structure
- [x] Each command handler remains a focused private method
- [x] No behavior changes — all existing functionality preserved
- [x] All existing tests pass

## Technical Approach

- Create a `Dictionary<BoardCommand, Action>` or `Dictionary<BoardCommand, Func<Task>>` mapping commands to handlers
- Initialize the dispatch table in the constructor
- Run() becomes: read command → look up handler → execute handler → render

## Progress Log

- 2026-03-05 - Task created from backlog scan round 4
