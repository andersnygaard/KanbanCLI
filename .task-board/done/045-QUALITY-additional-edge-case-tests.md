# QUALITY: Additional Edge Case Tests

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, tests

## Context & Motivation

The audit identified missing test coverage for:
- CreateTask with empty title (should throw)
- MoveTask to the same status (should be no-op or handle gracefully)
- TaskItem with very long title (>200 chars)
- MarkdownParser roundtrip with "In Progress" status formatting
- Repository Move where source and target paths are the same
- NavigationState edge cases (negative maxColumns, zero tasks)

## Acceptance Criteria

- [x] Add test: CreateTask_EmptyTitle_ThrowsArgumentException
- [x] Add test: CreateTask_TitleExceedsMaxLength_ThrowsArgumentException
- [x] Add test: MoveTask_ToSameStatus_HandlesGracefully
- [x] Add test: Parse_Serialize_Roundtrip_PreservesInProgressStatus
- [x] Add test: Move_ToSameColumn_DoesNotDeleteFile
- [x] Add test: NavigationState_MoveToColumn_WithZeroColumns_ReturnsUnchanged
- [x] All existing tests pass

## Progress Log

- 2026-03-05 - Task created from audit findings
