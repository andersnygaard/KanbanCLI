# CLEANUP: Remove Dead Code and Polish TUI Edge Cases

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Low
**Labels**: cleanup, tui

## Context & Motivation

MoveDialog has unreachable bounds check code (lines 38-39). StatusBar truncates filter info abruptly without ellipsis. Minor polish items identified in iteration 5 audit.

## Desired Outcome

Clean, polished TUI code with no dead code and proper truncation handling.

## Technical Approach

- Remove lines 38-39 in MoveDialog.cs
- In StatusBar, add "..." when status text exceeds width
- Grep for unused methods across TUI classes

## Progress Log

- 2026-03-05 - Task created from backlog scan round 5
- 2026-03-05 - Removed dead bounds check in MoveDialog.cs (lines 38-39), added ellipsis truncation in StatusBar, verified no other dead code in TUI classes. All 140 tests pass.

## Acceptance Criteria

- [x] Remove dead bounds check in MoveDialog.cs (redundant check after existing validation)
- [x] Add ellipsis truncation in StatusBar when filter info is long
- [x] Verify no other dead code remains across TUI classes
- [x] All tests pass
