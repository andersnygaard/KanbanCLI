# FEATURE: Add TaskType Color Coding in Task Cards

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Low
**Labels**: feature, tui

## Context & Motivation

Task cards currently only color-code by priority (High=Red, Medium=Yellow, Low=Green via TuiHelpers.GetPriorityColor). Adding type-based color coding would improve visual scanning — users could instantly distinguish Features from Bugs from Refactors. The spec mentions "color-coded priority indicators" and "label display with distinct styling" but type differentiation would enhance the board further.

## Desired Outcome

Task type prefix in card view is color-coded by type, complementing the existing priority colors.

## Acceptance Criteria

- [x] Add GetTypeColor(TaskType) method to TuiHelpers
- [x] Apply type color to the type prefix in TaskCard.RenderWithColors()
- [x] Ensure type colors don't clash with priority colors
- [x] All tests pass

## Technical Approach

### TuiHelpers — BEFORE (src/KanbanCli/Tui/TuiHelpers.cs):
```csharp
public static ConsoleColor GetPriorityColor(Priority priority) => priority switch
{
    Priority.High => ConsoleColor.Red,
    Priority.Medium => ConsoleColor.Yellow,
    Priority.Low => ConsoleColor.Green,
    _ => ConsoleColor.Gray
};
```

### TuiHelpers — AFTER (add new method):
```csharp
public static ConsoleColor GetTypeColor(TaskType type) => type switch
{
    TaskType.Feature => ConsoleColor.Cyan,
    TaskType.Bug => ConsoleColor.Red,
    TaskType.Security => ConsoleColor.DarkYellow,
    TaskType.Refactor => ConsoleColor.Green,
    TaskType.Test => ConsoleColor.Magenta,
    TaskType.Perf => ConsoleColor.DarkCyan,
    TaskType.Docs => ConsoleColor.Blue,
    _ => ConsoleColor.Gray
};
```

### TaskCard.RenderWithColors() — BEFORE:
```csharp
// Type prefix rendered in default color
Console.Write($"#{task.Id:D3} {task.Type.ToString().ToUpperInvariant()}: ");
```

### TaskCard.RenderWithColors() — AFTER:
```csharp
// Type prefix rendered in type-specific color
Console.Write($"#{task.Id:D3} ");
Console.ForegroundColor = TuiHelpers.GetTypeColor(task.Type);
Console.Write($"{task.Type.ToString().ToUpperInvariant()}");
Console.ResetColor();
Console.Write(": ");
```

## Progress Log

- 2026-03-05 - Task created from backlog scan round 7
- 2026-03-05 - Added before/after code examples with color mapping
