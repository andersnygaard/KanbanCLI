# FEATURE: Rich Domain Models

**Status**: Done
**Created**: 2026-03-04
**Priority**: High
**Labels**: models, domain, core
**Estimated Effort**: Medium - 2-3 days

## Context & Motivation

The spec defines rich domain models as the core of the application — immutable records with behavior. Every layer (Storage, Services, TUI) depends on these models. They are pure, testable, and have no external dependencies, making them the ideal second task after scaffolding.

## Current State

No models exist. The spec (.docs/specs.md) defines:
- `TaskItem` — rich record with Id, Title, Type, Priority, Status, Labels + behavior methods
- `Board` — board with columns
- `Column` — column with tasks
- `Enums.cs` — TaskType, Priority, TaskStatus

## Desired Outcome

Complete domain model layer with full test coverage. All models are immutable records with behavior methods, ready to be consumed by Storage and Services layers.

## Acceptance Criteria

- [x] `TaskItem` record with all properties from spec (Id, Title, Type, Priority, Status, Labels, CreatedDate, CompletedDate)
- [x] `TaskItem` behavior: `ChangeStatus()`, `AddLabel()`, `SetPriority()`, `MatchesFilter()`
- [x] `TaskItem.ChangeStatus(Done)` sets CompletedDate automatically
- [x] `TaskItem.ChangeStatus(fromDone)` clears CompletedDate
- [x] `TaskItem.AddLabel()` prevents duplicates
- [x] `Board` record containing columns
- [x] `Column` record containing tasks
- [x] Enums: `TaskType` (FEATURE, BUG, REFACTOR, TEST, SECURITY, PERF, DESIGN, DOCS, EPIC, EXPLORE, CLEANUP, A11Y, QUALITY)
- [x] Enums: `Priority` (High, Medium, Low)
- [x] Enums: `TaskStatus` (Backlog, InProgress, Done, OnHold)
- [x] `FilterCriteria` type for filtering support
- [x] `FileName` generation method: `{Id:D3}-{Type}-{kebab-title}.md`
- [x] All models use `record` with `init` properties
- [x] Full xUnit test coverage for all behavior methods
- [x] Tests use FluentAssertions
- [x] Test naming: `MethodName_Scenario_ExpectedResult`

## Affected Components

### Files to Create
- `src/KanbanCli/Models/TaskItem.cs`
- `src/KanbanCli/Models/Board.cs`
- `src/KanbanCli/Models/Column.cs`
- `src/KanbanCli/Models/Enums.cs`
- `src/KanbanCli/Models/FilterCriteria.cs`
- `src/KanbanCli.Tests/Models/TaskItemTests.cs`

### Dependencies
- **External**: None (pure domain models)
- **Internal**: None
- **Blocking**: Task 001 (project scaffolding must exist first)

## Technical Approach

### Architecture Decisions

- **Records over classes**: Immutable by default, value equality, `with` expressions for changes
- **Behavior in models**: `ChangeStatus`, `AddLabel`, `SetPriority`, `MatchesFilter` live on the record
- **No magic strings**: All type prefixes, statuses, column names are enums
- **Defensive**: `AddLabel` deduplicates, `ChangeStatus` manages CompletedDate automatically

### Implementation Steps

1. **Create Enums.cs**
   ```csharp
   public enum TaskType { Feature, Bug, Refactor, Test, Security, Perf, Design, Docs, Epic, Explore, Cleanup, A11y, Quality }
   public enum Priority { High, Medium, Low }
   public enum TaskStatus { Backlog, InProgress, Done, OnHold }
   ```

2. **Create FilterCriteria.cs**
   ```csharp
   public record FilterCriteria(TaskType? Type = null, Priority? Priority = null, string? Label = null);
   ```

3. **Create TaskItem.cs** — rich record:
   - All properties with `init`
   - `ChangeStatus(TaskStatus)` → returns new TaskItem, auto-manages CompletedDate
   - `AddLabel(string)` → returns new TaskItem, prevents duplicates
   - `SetPriority(Priority)` → returns new TaskItem
   - `MatchesFilter(FilterCriteria)` → bool
   - `GenerateFileName()` → string in format `{Id:D3}-{TYPE}-{kebab-title}.md`

4. **Create Column.cs and Board.cs**

5. **Create TaskItemTests.cs** following spec test list:
   - `ChangeStatus_ToDone_SetsCompletedDate`
   - `ChangeStatus_FromDoneToBacklog_ClearsCompletedDate`
   - `AddLabel_NewLabel_AppendsToList`
   - `AddLabel_DuplicateLabel_DoesNotDuplicate`
   - `SetPriority_High_UpdatesPriority`
   - `MatchesFilter_ByLabel_ReturnsTrue`
   - `MatchesFilter_ByType_ReturnsTrue`
   - `MatchesFilter_NoMatch_ReturnsFalse`
   - `FileName_GeneratesCorrectFormat`

### Risks & Considerations

- **Risk**: Over-engineering models before knowing exact usage — **Mitigation**: Stick to spec, add behavior only for methods explicitly listed
- **Readability**: Names must be descriptive and self-documenting per spec principles

## Code References

### From specs.md — TaskItem definition
```csharp
public record TaskItem
{
    public int Id { get; init; }
    public string Title { get; init; }
    public TaskType Type { get; init; }
    public Priority Priority { get; init; }
    public TaskStatus Status { get; init; }
    public IReadOnlyList<string> Labels { get; init; }

    public TaskItem ChangeStatus(TaskStatus newStatus) => ...
    public TaskItem AddLabel(string label) => ...
    public TaskItem SetPriority(Priority priority) => ...
    public bool MatchesFilter(FilterCriteria filter) => ...
}
```

## Progress Log

- 2026-03-04 - Task created via backlog-scan
- 2026-03-04 - Implementation complete. All 6 model files created, 13 tests passing.

---

**Next Steps**: Implement after task 001 is complete. Move to `.task-board/in-progress/` when starting work.
