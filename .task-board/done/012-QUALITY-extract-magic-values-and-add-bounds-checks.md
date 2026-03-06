# QUALITY: Extract Magic Values into Constants and Add Bounds Checks

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, refactor, tui

## Context & Motivation

Hard-coded magic values are scattered across the codebase: folder names in `MarkdownTaskRepository`, column names in `BoardService`, colors and min-widths throughout TUI code. The `KanbanApp` lacks null/bounds checks when indexing into `Columns` and `Tasks`, risking `IndexOutOfRangeException`. The `MoveDialog` uses `StatusMap.Length` instead of `board.Columns.Count`.

## Desired Outcome

- Shared constants for folder names, column names, format strings, and min-widths
- Bounds-safe indexing in all TUI navigation code
- `MoveDialog` uses actual column count

## Technical Approach

- Create `Models/BoardConstants.cs` with `static class` holding column folder names, default widths, date formats
- Use these constants in `MarkdownTaskRepository`, `BoardService`, and TUI components
- Add `Math.Clamp` or bounds checks in `KanbanApp` navigation methods
- Fix `MoveDialog` constructor to accept column count dynamically

## Progress Log

- 2026-03-05 - Task created from backlog scan round 1
- 2026-03-05 - Implemented: replaced magic strings with BoardConstants, added bounds checks in KanbanApp, fixed MoveDialog, added 20 new tests

## Acceptance Criteria

- [x] Create a Constants or BoardConstants class with folder names, column names, format strings
- [x] Replace all magic strings/values in MarkdownTaskRepository with constants
- [x] Replace all magic strings/values in BoardService with constants
- [x] Add bounds checks before indexing Columns and Tasks in KanbanApp
- [x] Fix MoveDialog to use board.Columns.Count
- [x] Add tests verifying constants are used consistently
