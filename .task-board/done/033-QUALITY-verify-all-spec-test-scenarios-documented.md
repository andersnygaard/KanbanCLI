# QUALITY: Verify All Spec Test Scenarios Are Covered and Well-Named

**Status**: Done
**Created**: 2026-03-05
**Priority**: Low
**Labels**: quality, tests

## Context & Motivation

The spec (.docs/specs.md lines 351-408) lists specific test scenarios with naming conventions. While the implementation has 183 tests exceeding spec requirements, we should verify every explicitly named test from the spec exists with the correct name and validate no scenarios were accidentally skipped.

## Spec Test Scenarios to Verify

### Models-tester (specs.md lines 356-365):
```
├── ChangeStatus_ToDone_SetsCompletedDate
├── ChangeStatus_FromDoneToBacklog_ClearsCompletedDate
├── AddLabel_NewLabel_AppendsToList
├── AddLabel_DuplicateLabel_DoesNotDuplicate
├── SetPriority_High_UpdatesPriority
├── MatchesFilter_ByLabel_ReturnsTrue
├── MatchesFilter_ByType_ReturnsTrue
├── MatchesFilter_NoMatch_ReturnsFalse
└── FileName_GeneratesCorrectFormat
```

### Storage-tester (specs.md lines 372-389):
```
MarkdownParserTests
├── Parse_FullTaskFile_ReturnsCorrectTaskItem
├── Parse_MinimalFile_DefaultsMissingFields
├── Parse_AcceptanceCriteria_ParsesCheckboxes
├── Parse_UnknownType_HandlesGracefully
├── Serialize_TaskItem_ProducesValidMarkdown
├── Roundtrip_ParseThenSerialize_PreservesContent
└── ParseFileName_ExtractsIdTypeAndDescription

TaskRepositoryTests
├── GetAllByColumn_BacklogWithTasks_ReturnsParsedItems
├── GetAllByColumn_EmptyFolder_ReturnsEmpty
├── Save_NewTask_CreatesFileWithCorrectName
├── Move_BacklogToInProgress_MovesFileAndUpdatesStatus
├── Delete_ExistingTask_RemovesFile
└── GetNextId_WithExistingTasks_ReturnsIncrementedId
```

### Service-tester (specs.md lines 395-408):
```
TaskServiceTests
├── CreateTask_ValidInput_SavesAndReturnsTask
├── CreateTask_AssignsNextAvailableId
├── MoveTask_UpdatesStatusAndMovesFile
├── MoveTask_ToDone_SetsCompletedDate
├── DeleteTask_CallsRepositoryDelete
└── GetBoard_ReturnsAllColumnsWithTasks

BoardServiceTests
├── GeneratePlanningBoard_TopPriorities_FormatsCorrectly
├── GeneratePlanningBoard_RecentlyCompleted_IncludesDone
└── GeneratePlanningBoard_EmptyBoard_ShowsEmptyMessage
```

## Progress Log

- 2026-03-05 - Task created from spec review round 8

## Acceptance Criteria

- [x] Verify every spec-listed test name exists (exact or equivalent) in test files
- [x] Add any missing spec-listed tests with correct names
- [x] Ensure test naming follows MethodName_Scenario_ExpectedResult convention
- [x] All tests pass
