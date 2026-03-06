# FEATURE: Priority Filtering UI and RemoveLabel Support

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: feature, tui, models

## Context & Motivation

FilterDialog only supports filter-by-Type and filter-by-Label but not filter-by-Priority, even though `FilterCriteria` and `MatchesFilter()` already support it. Additionally, `TaskItem` has `AddLabel()` but no `RemoveLabel()` method, which is needed for full label management.

## Desired Outcome

- Users can filter the board by priority (High/Medium/Low)
- Users can remove labels from tasks
- Both features have comprehensive tests

## Technical Approach

- In FilterDialog.cs, add `PromptByPriority()` method that shows High/Medium/Low options
- In TaskItem.cs, add `RemoveLabel(string label)` that returns new record without the label
- In TaskCard.cs, wrap labels in `[brackets]` for distinct styling

## Progress Log

- 2026-03-05 - Task created from backlog scan round 2

## Acceptance Criteria

- [x] Add "By Priority" option to FilterDialog following existing PromptByLabel/PromptByType pattern
- [x] Add RemoveLabel(string label) method to TaskItem
- [x] Add tests for priority filtering in FilterDialog integration
- [x] Add tests for RemoveLabel (removes existing, no-op for non-existent, case handling)
- [x] Enhance label display in TaskCard with bracket wrapping for visual distinction
