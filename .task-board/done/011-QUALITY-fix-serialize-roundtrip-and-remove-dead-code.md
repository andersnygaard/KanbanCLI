# QUALITY: Fix Serialize Roundtrip Safety and Remove Dead Code

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: quality, storage, models

## Context & Motivation

The `Serialize()` method in `MarkdigMarkdownParser` injects template sections (Context & Motivation, Acceptance Criteria, Progress Log) into tasks that never had them, violating the roundtrip safety guarantee from the spec. Additionally, `TaskCard.Render()` is dead code (only `RenderWithColors()` is used), and `HandleChangePriority()` incorrectly calls `MoveTask()` for priority changes.

## Desired Outcome

- `Serialize()` only outputs sections that exist in the `TaskItem.Sections` dictionary
- Dead code is removed
- Priority changes use a dedicated update path, not `MoveTask()`

## Technical Approach

- In `MarkdigMarkdownParser.Serialize()`, iterate only over `task.Sections` instead of hardcoded section names
- Add `ITaskRepository.Save(TaskItem)` overload or `Update()` method for in-place updates
- Add `ITaskService.UpdateTask()` for priority/label changes without moving files
- Remove `TaskCard.Render()` entirely

## Progress Log

- 2026-03-05 - Task created from backlog scan round 1
- 2026-03-05 - All acceptance criteria verified complete: Serialize() already only writes sections from TaskItem.Sections, TaskCard.Render() already removed (only RenderWithColors exists), HandleChangePriority() already uses UpdateTask (with Update in repo/service), added 3 new tests for roundtrip identity and no-section-injection

## Acceptance Criteria

- [x] Fix Serialize() to only write sections present in TaskItem.Sections
- [x] Remove dead TaskCard.Render() method
- [x] Fix HandleChangePriority() to not use MoveTask() — add UpdateTask to service/repo
- [x] Add roundtrip test: parse → serialize → parse produces identical TaskItem
- [x] Add test that Serialize does not inject sections not in the original
