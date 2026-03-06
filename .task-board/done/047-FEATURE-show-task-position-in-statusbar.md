# FEATURE: Show Task Position in Status Bar

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Low
**Labels**: feature, tui

## Context & Motivation

When navigating a column with many tasks, there's no indication of the current position. The status bar should show something like "Task 3/14" to help users orient themselves.

## Resolution

Already implemented. BoardView.Render() calculates positionInfo as "Task N/Total" and passes it to StatusBar.Render(). StatusBar appends it to the keybindings text. Empty columns produce null positionInfo which is not displayed.

## Progress Log

- 2026-03-05 - Task created
- 2026-03-05 - Verified already implemented, marked complete

## Acceptance Criteria

- [x] StatusBar shows current task position (e.g., "3/14") when a column has tasks
- [x] Position updates when navigating up/down
- [x] Empty columns show no position indicator
- [x] All existing tests pass
