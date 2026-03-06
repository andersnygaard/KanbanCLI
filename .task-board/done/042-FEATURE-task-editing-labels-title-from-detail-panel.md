# FEATURE: Task Editing (Labels, Title) from Detail Panel

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: feature, tui

## Context & Motivation

The TaskDetailPanel already has edit handlers for title, labels, and priority (implemented in earlier iterations). However, the audit noted that label management (add/remove on existing tasks) is a key missing feature. Let's verify the existing edit functionality works correctly and add any missing polish.

The audit also noted that the `default:` case in the detail panel's key handler returns immediately — meaning any unrecognized key exits the panel. This is poor UX since the user might accidentally press a wrong key and lose their view.

## Technical Approach

Change the `default:` case in `TaskDetailPanel.Show()` from `return current` to `break` (continue the loop instead of exiting).

Verify the existing edit handlers (HandleEditTitle, HandleEditLabels, HandleEditPriority) work correctly with the new scrolling system.

## Progress Log

- 2026-03-05 - Task created from audit findings

## Acceptance Criteria

- [ ] Pressing an unrecognized key in the detail panel does NOT exit — only Escape exits
- [ ] Title editing works correctly (prompt, save, re-render)
- [ ] Label add/remove works correctly from the detail panel
- [ ] Priority change works correctly from the detail panel
- [ ] After any edit, the detail panel re-renders with updated data
- [ ] All existing tests pass
