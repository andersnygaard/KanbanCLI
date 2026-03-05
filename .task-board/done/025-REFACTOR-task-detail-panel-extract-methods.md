# REFACTOR: Extract Long Methods in TaskDetailPanel

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Low
**Labels**: refactor, tui, quality

## Context & Motivation

TaskDetailPanel has 3 methods exceeding 30 lines: RenderDetailView (34 lines), HandleRemoveLabel (39 lines), HandleEditPriority (34 lines). The spec and CLAUDE.md say to keep methods short and single-purpose.

## Desired Outcome

All methods in TaskDetailPanel under 30 lines through extracting rendering and input logic into focused sub-methods.

## Acceptance Criteria

- [x] Extract RenderDetailView into smaller render helpers (header, metadata, sections)
- [x] Extract HandleRemoveLabel list rendering into a helper
- [x] Extract HandleEditPriority common dialog pattern
- [x] No behavior changes — all existing functionality preserved
- [x] All tests pass

## Progress Log

- 2026-03-05 - Task created from backlog scan round 6
