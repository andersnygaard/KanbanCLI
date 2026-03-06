# QUALITY: Add Missing TUI Interfaces from Spec (IColumnView, ITaskCard)

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, tui, spec-gap

## Context & Motivation

The spec's interface table (.docs/specs.md lines 207-208) explicitly lists `IColumnView` and `ITaskCard` as required interfaces, but neither exists in the codebase. `ColumnView` and `TaskCard` are concrete classes with no interfaces. The spec principle is "Interfaces overalt" (interfaces everywhere) for full testability and flexibility.

## Current State

**BEFORE** — spec says these interfaces should exist:
```
| IColumnView | Rendrer én kolonne med tasks | ColumnView |
| ITaskCard   | Rendrer én task-rad          | TaskCard   |
```

But currently:
```csharp
// src/KanbanCli/Tui/ColumnView.cs
public class ColumnView  // ← No interface!
{
    public void Render(Column column, int columnWidth, int selectedTask, bool isActiveColumn, FilterCriteria? filter)
    ...
}

// src/KanbanCli/Tui/TaskCard.cs
public class TaskCard  // ← No interface!
{
    public void RenderWithColors(TaskItem task, int columnWidth, bool isSelected)
    ...
}
```

## Desired Outcome

**AFTER** — interfaces extracted and classes implement them:
```csharp
// src/KanbanCli/Tui/IColumnView.cs
/// <summary>Renders a single column with its task list.</summary>
public interface IColumnView
{
    void Render(Column column, int columnWidth, int selectedTask, bool isActiveColumn, FilterCriteria? filter);
}

// src/KanbanCli/Tui/ITaskCard.cs
/// <summary>Renders a single task row in a column.</summary>
public interface ITaskCard
{
    void RenderWithColors(TaskItem task, int columnWidth, bool isSelected);
}
```

```csharp
// src/KanbanCli/Tui/ColumnView.cs
public class ColumnView : IColumnView { ... }

// src/KanbanCli/Tui/TaskCard.cs
public class TaskCard : ITaskCard { ... }
```

## Progress Log

- 2026-03-05 - Task created from spec review round 8

## Acceptance Criteria

- [x] Create IColumnView interface in src/KanbanCli/Tui/IColumnView.cs
- [x] Create ITaskCard interface in src/KanbanCli/Tui/ITaskCard.cs
- [x] Have ColumnView implement IColumnView
- [x] Have TaskCard implement ITaskCard
- [x] Update BoardView to depend on IColumnView instead of concrete ColumnView (if applicable)
- [x] Update ColumnView to depend on ITaskCard instead of concrete TaskCard (if applicable)
- [x] All tests pass
