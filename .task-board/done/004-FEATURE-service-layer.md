# FEATURE: Service Layer (TaskService & BoardService)

**Status**: Backlog
**Created**: 2026-03-04
**Priority**: High
**Labels**: services, core
**Estimated Effort**: Medium - 2-3 days

## Context & Motivation

The Services layer is the orchestration layer between Storage and TUI. Per the spec's architecture diagram, Services depend on `ITaskRepository` and are consumed by the TUI layer via `ITaskService` and `IBoardService`. This layer handles all complex workflows: creating tasks with auto-numbering, moving tasks between columns (which involves both file moves and status updates), deleting tasks, and auto-generating the PLANNING-BOARD.md summary.

Without this layer, the TUI has no API to call — it would need to directly manipulate storage, violating the spec's layered architecture.

## Current State

No services code exists. The spec defines:
- `ITaskService` / `TaskService` — task CRUD and movement
- `IBoardService` / `BoardService` — board state and PLANNING-BOARD.md generation
- Test suite with mocked `ITaskRepository`

## Desired Outcome

Complete Services layer with interfaces, implementations, and full test coverage. The TUI layer can depend entirely on these services for all board operations.

## Acceptance Criteria

- [x] `ITaskService` interface defined with methods: CreateTask, MoveTask, DeleteTask, GetAllByColumn, GetAll
- [x] `TaskService` implementation orchestrating `ITaskRepository` and domain model logic
- [x] `TaskService.CreateTask` assigns next available ID via `ITaskRepository.GetNextId()`
- [x] `TaskService.MoveTask` updates task status (via `TaskItem.ChangeStatus`) and calls `ITaskRepository.Move`
- [x] `TaskService.MoveTask` to Done automatically sets CompletedDate (via domain model)
- [x] `IBoardService` interface defined with methods: GetBoard, GeneratePlanningBoard
- [x] `BoardService` implementation that builds `Board` model from all columns
- [x] `BoardService.GeneratePlanningBoard` generates markdown for PLANNING-BOARD.md
- [x] Planning board shows top priorities (High priority, In Progress first)
- [x] Planning board shows recently completed tasks from Done column
- [x] Planning board handles empty board gracefully
- [x] All services depend on interfaces only (DI-ready)
- [x] Full test suite with mocked `ITaskRepository` using NSubstitute
- [x] Tests use FluentAssertions
- [x] Test naming follows `MethodName_Scenario_ExpectedResult` convention

## Affected Components

### Files to Create
- `src/KanbanCli/Services/ITaskService.cs`
- `src/KanbanCli/Services/TaskService.cs`
- `src/KanbanCli/Services/IBoardService.cs`
- `src/KanbanCli/Services/BoardService.cs`
- `src/KanbanCli.Tests/Services/TaskServiceTests.cs`
- `src/KanbanCli.Tests/Services/BoardServiceTests.cs`

### Dependencies
- **External**: NSubstitute, FluentAssertions (added in task 001)
- **Internal**: Models layer (TaskItem, Board, Column, Enums, FilterCriteria) from task 002
- **Internal**: Storage interfaces (ITaskRepository) from task 003
- **Blocking**: Task 001 (scaffolding), Task 002 (domain models), Task 003 (storage layer)

## Technical Approach

### Architecture Decisions

- **Interface-first**: Define `ITaskService` and `IBoardService` before implementations — TUI will depend on these interfaces
- **Thin orchestration**: Services don't duplicate domain logic. `MoveTask` calls `TaskItem.ChangeStatus()` then `ITaskRepository.Move()` — the model owns the status transition logic
- **No direct file I/O**: Services only talk to `ITaskRepository`, never to the filesystem directly
- **PLANNING-BOARD.md generation**: `BoardService` builds the markdown string; writing it to disk goes through `ITaskRepository` or a dedicated method

### Interfaces

```csharp
public interface ITaskService
{
    TaskItem CreateTask(string title, TaskType type, Priority priority, IReadOnlyList<string> labels);
    void MoveTask(TaskItem task, TaskStatus targetColumn);
    void DeleteTask(TaskItem task);
    IReadOnlyList<TaskItem> GetAllByColumn(TaskStatus column);
    IReadOnlyList<TaskItem> GetAll();
}

public interface IBoardService
{
    Board GetBoard();
    string GeneratePlanningBoard();
}
```

### Implementation Steps

1. **Create ITaskService.cs and ITaskService.cs**
   - Define interface with CreateTask, MoveTask, DeleteTask, GetAllByColumn, GetAll
   - Implement `TaskService` with `ITaskRepository` dependency (constructor injection)
   - `CreateTask`: get next ID, build TaskItem, call `Save`
   - `MoveTask`: call `task.ChangeStatus(target)`, then `repository.Move()`
   - `DeleteTask`: delegate to `repository.Delete()`

2. **Create IBoardService.cs and BoardService.cs**
   - `GetBoard`: fetch all columns, build `Board` model with `Column` records
   - `GeneratePlanningBoard`: build markdown string with:
     - Top priorities: High priority + InProgress tasks, sorted
     - Recently completed: tasks from Done column
     - Empty state message when no tasks exist

3. **Create TaskServiceTests.cs** — all spec test cases:
   - `CreateTask_ValidInput_SavesAndReturnsTask`
   - `CreateTask_AssignsNextAvailableId`
   - `MoveTask_UpdatesStatusAndMovesFile`
   - `MoveTask_ToDone_SetsCompletedDate`
   - `DeleteTask_CallsRepositoryDelete`
   - `GetBoard_ReturnsAllColumnsWithTasks`

4. **Create BoardServiceTests.cs** — all spec test cases:
   - `GeneratePlanningBoard_TopPriorities_FormatsCorrectly`
   - `GeneratePlanningBoard_RecentlyCompleted_IncludesDone`
   - `GeneratePlanningBoard_EmptyBoard_ShowsEmptyMessage`

### Test Strategy

All tests mock `ITaskRepository` with NSubstitute:

```csharp
[Fact]
public void CreateTask_ValidInput_SavesAndReturnsTask()
{
    // Arrange
    var repository = Substitute.For<ITaskRepository>();
    repository.GetNextId().Returns(5);
    var service = new TaskService(repository);

    // Act
    var task = service.CreateTask("My Task", TaskType.Feature, Priority.High, new List<string> { "frontend" });

    // Assert
    task.Id.Should().Be(5);
    task.Title.Should().Be("My Task");
    repository.Received(1).Save(Arg.Is<TaskItem>(t => t.Id == 5));
}
```

### Risks & Considerations

- **Risk**: PLANNING-BOARD.md format may need iteration — **Mitigation**: Match the template format from specs.md exactly
- **Risk**: Coupling between TaskService and BoardService — **Mitigation**: Keep them independent; BoardService reads via ITaskRepository, not via ITaskService
- **Readability**: Keep service methods short and focused — one operation per method, delegate logic to domain models

## Code References

### From specs.md — Layer diagram
```
Services (Services/)
  Orkestrerer operasjoner
  Avhenger av: ITaskRepository
```

### From specs.md — PLANNING-BOARD.md format
```markdown
# Planning Board

**Current Focus**: Feature development

## Top Priorities

1. **#003** FEATURE: Dark mode - In Progress
2. **#005** BUG: Memory leak in parser - Backlog

## Recently Completed

- **#004** BUG: Header overlap fix - Done 2026-03-03
```

### From specs.md — Service test cases
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

- 2026-03-04 - Task created via backlog-scan

---

**Next Steps**: Implement after tasks 001, 002, and 003 are complete. Move to `.task-board/in-progress/` when starting work.
