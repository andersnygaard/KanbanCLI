# REFACTOR: Extract Shared Dialog Helper to Eliminate Duplication

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: refactor, tui, quality

## Context & Motivation

Enum list rendering code is duplicated across NewTaskDialog, PriorityDialog, and FilterDialog (31-33 lines each). All dialog classes also duplicate the Console setup pattern (Clear, CursorVisible, header rendering). This violates DRY and makes changes error-prone.

## Desired Outcome

A shared DialogHelper utility class that consolidates common dialog patterns, reducing duplication across all dialog classes.

## Acceptance Criteria

- [x] Create Tui/DialogHelper.cs with shared dialog utilities
- [x] Extract PromptEnum<T> generic method for enum selection used by multiple dialogs
- [x] Extract dialog setup/teardown pattern (Clear, CursorVisible, header)
- [x] Refactor NewTaskDialog to use DialogHelper
- [x] Refactor PriorityDialog to use DialogHelper
- [x] Refactor FilterDialog to use DialogHelper
- [x] All existing tests still pass, build succeeds

## Technical Approach

- Create `static class DialogHelper` in Tui/ folder
- Add `T? PromptEnum<T>(string label)` generic method
- Add `void SetupDialog(string? header)` for console setup pattern
- Refactor each dialog class to delegate to DialogHelper

## Progress Log

- 2026-03-05 - Task created from backlog scan round 3
