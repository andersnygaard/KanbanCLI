# FEATURE: Task Edit Dialog for Enter Key

**Status**: Done
**Created**: 2026-03-05
**Priority**: High
**Labels**: feature, tui, spec-gap

## Context & Motivation

The spec says Enter should "View/edit task details" but the current implementation only views (read-only TaskDetailPanel). Users cannot edit task title, priority, labels, or status from the detail view. This is a key spec gap.

## Desired Outcome

When pressing Enter on a task, users see the task detail and can edit its fields (title, priority, labels). Changes are persisted via the service layer.

## Technical Approach

- Modify TaskDetailPanel to show edit options at the bottom (e.g., [T]itle, [L]abels, [P]riority, [Esc] back)
- Reuse existing dialog patterns for input prompts
- Call _taskService.UpdateTask() to persist changes
- Return updated task to caller so board refreshes

## Progress Log

- 2026-03-05 - Task created from backlog scan round 3
- 2026-03-05 - All acceptance criteria verified as already implemented. Build and 131 tests pass.

## Acceptance Criteria

- [x] Add edit options to TaskDetailPanel or create a TaskEditDialog
- [x] Allow editing task title from the detail view
- [x] Allow editing labels (add/remove) from the detail view
- [x] Allow editing priority from the detail view
- [x] Changes persist via ITaskService.UpdateTask()
- [x] Updated task is reflected on the board immediately
