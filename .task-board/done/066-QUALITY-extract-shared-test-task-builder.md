# QUALITY: Extract Shared Test Task Builder and Fix Convention Violations

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: tests, conventions

## Context & Motivation

The test project has three separate `CreateTask` / `CreateSampleTask` helper methods duplicated across `TaskItemTests`, `TaskServiceTests`, and `BoardServiceTests`. Each file defines its own factory with slightly different signatures and defaults. The spec explicitly recommends "builder-pattern or factory-metoder for å lage TaskItems — unngå duplisering".

Additionally, the `CreateSampleTask` method in `TaskItemTests` (line 17) uses an expression-bodied member (`=>`), which violates the project convention in CLAUDE.md: "All methods and properties must use block bodies — no expression-bodied members."

## Current State

Three duplicate factories exist:
- `TaskItemTests.CreateSampleTask` (expression-bodied, 8 parameters)
- `TaskServiceTests.CreateTask` (block-bodied, 3 parameters)
- `BoardServiceTests.CreateTask` (block-bodied, 6 parameters)

All construct a `TaskItem` with slightly different defaults, making it hard to maintain consistency when the model changes.

## Desired Outcome

A single shared `TestTaskBuilder` class in the test project that all test files use, with a fluent API. All test factory methods use block bodies per convention.

## Technical Approach

### Change 1: Create shared TestTaskBuilder

**File:** `src/KanbanCli.Tests/TestTaskBuilder.cs` (new file)

```csharp
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests;

public class TestTaskBuilder
{
    private int _id = 1;
    private string _title = "Test task";
    private TaskType _type = TaskType.Feature;
    private Priority _priority = Priority.Medium;
    private TaskStatus _status = TaskStatus.Backlog;
    private IReadOnlyList<string> _labels = [];
    private DateTime? _createdDate = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc);
    private DateTime? _completedDate;

    public TestTaskBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public TestTaskBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TestTaskBuilder WithType(TaskType type)
    {
        _type = type;
        return this;
    }

    public TestTaskBuilder WithPriority(Priority priority)
    {
        _priority = priority;
        return this;
    }

    public TestTaskBuilder WithStatus(TaskStatus status)
    {
        _status = status;
        return this;
    }

    public TestTaskBuilder WithLabels(params string[] labels)
    {
        _labels = labels.ToList().AsReadOnly();
        return this;
    }

    public TestTaskBuilder WithCreatedDate(DateTime? createdDate)
    {
        _createdDate = createdDate;
        return this;
    }

    public TestTaskBuilder WithCompletedDate(DateTime? completedDate)
    {
        _completedDate = completedDate;
        return this;
    }

    public TaskItem Build()
    {
        return new TaskItem
        {
            Id = _id,
            Title = _title,
            Type = _type,
            Priority = _priority,
            Status = _status,
            Labels = _labels,
            CreatedDate = _createdDate,
            CompletedDate = _completedDate
        };
    }
}
```

### Change 2: Fix expression-bodied violation in TaskItemTests

**File:** `src/KanbanCli.Tests/Models/TaskItemTests.cs`

Before:
```csharp
private static TaskItem CreateSampleTask(
    int id = 1,
    string title = "Sample Task",
    TaskType type = TaskType.Feature,
    Priority priority = Priority.Medium,
    TaskStatus status = TaskStatus.Backlog,
    IReadOnlyList<string>? labels = null,
    DateTime? createdDate = null,
    DateTime? completedDate = null) =>
    new()
    {
        Id = id,
        Title = title,
        // ...
    };
```

After:
```csharp
private static TaskItem CreateSampleTask(
    int id = 1,
    string title = "Sample Task",
    TaskType type = TaskType.Feature,
    Priority priority = Priority.Medium,
    TaskStatus status = TaskStatus.Backlog,
    IReadOnlyList<string>? labels = null,
    DateTime? createdDate = null,
    DateTime? completedDate = null)
{
    return new TestTaskBuilder()
        .WithId(id)
        .WithTitle(title)
        .WithType(type)
        .WithPriority(priority)
        .WithStatus(status)
        .WithLabels(labels?.ToArray() ?? [])
        .WithCreatedDate(createdDate ?? DateTime.UtcNow)
        .WithCompletedDate(completedDate)
        .Build();
}
```

### Change 3: Update TaskServiceTests to use TestTaskBuilder

**File:** `src/KanbanCli.Tests/Services/TaskServiceTests.cs`

Before:
```csharp
private static TaskItem CreateTask(int id = 1, TaskStatus status = TaskStatus.Backlog, Priority priority = Priority.Medium)
{
    return new()
    {
        Id = id,
        Title = "Test task",
        Type = TaskType.Feature,
        Status = status,
        Priority = priority,
        Labels = [],
        CreatedDate = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc)
    };
}
```

After:
```csharp
private static TaskItem CreateTask(int id = 1, TaskStatus status = TaskStatus.Backlog, Priority priority = Priority.Medium)
{
    return new TestTaskBuilder()
        .WithId(id)
        .WithStatus(status)
        .WithPriority(priority)
        .Build();
}
```

### Change 4: Update BoardServiceTests similarly

**File:** `src/KanbanCli.Tests/Services/BoardServiceTests.cs`

Before:
```csharp
private static TaskItem CreateTask(
    int id,
    string title,
    TaskType type,
    TaskStatus status,
    Priority priority = Priority.Medium,
    DateTime? completedDate = null)
{
    return new()
    {
        Id = id,
        Title = title,
        Type = type,
        Status = status,
        Priority = priority,
        Labels = [],
        CreatedDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
        CompletedDate = completedDate
    };
}
```

After:
```csharp
private static TaskItem CreateTask(
    int id,
    string title,
    TaskType type,
    TaskStatus status,
    Priority priority = Priority.Medium,
    DateTime? completedDate = null)
{
    return new TestTaskBuilder()
        .WithId(id)
        .WithTitle(title)
        .WithType(type)
        .WithStatus(status)
        .WithPriority(priority)
        .WithCompletedDate(completedDate)
        .Build();
}
```

## Acceptance Criteria

- [x] `TestTaskBuilder` class created in test project root with fluent API
- [x] Expression-bodied `CreateSampleTask` in `TaskItemTests` converted to block body
- [x] `TaskServiceTests.CreateTask` delegates to `TestTaskBuilder`
- [x] `BoardServiceTests.CreateTask` delegates to `TestTaskBuilder`
- [x] All existing tests continue to pass
- [x] No expression-bodied members in the new builder class

## Progress Log

- 2026-03-05 - Task created from backlog scan
