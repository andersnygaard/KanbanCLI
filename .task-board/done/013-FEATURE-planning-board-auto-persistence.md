# FEATURE: PLANNING-BOARD.md Auto-Persistence

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: feature, services, spec-gap

## Context & Motivation

`BoardService.GeneratePlanningBoard()` generates the markdown string but never writes it to disk. No `SavePlanningBoard()` method exists. The spec requires auto-maintained PLANNING-BOARD.md that updates after every create, move, and delete operation.

## Desired Outcome

PLANNING-BOARD.md is automatically persisted to disk whenever the board state changes (task created, moved, or deleted).

## Technical Approach

- Add `void SavePlanningBoard(string boardPath)` to `IBoardService`
- Implementation calls `GeneratePlanningBoard()` then `_fileSystem.WriteAllText()`
- In `KanbanApp.cs`, call `_boardService.SavePlanningBoard(_boardPath)` after HandleCreateTask, HandleMoveTask, HandleDeleteTask

## Progress Log

- 2026-03-05 - Task created from backlog scan round 2

## Acceptance Criteria

- [x] Add SavePlanningBoard() method to IBoardService and BoardService
- [x] SavePlanningBoard writes GeneratePlanningBoard() output to .task-board/PLANNING-BOARD.md via IFileSystem
- [x] KanbanApp calls SavePlanningBoard() after create, move, and delete operations
- [x] Add tests for SavePlanningBoard in BoardServiceTests
- [x] Add integration-style test verifying file is written correctly
