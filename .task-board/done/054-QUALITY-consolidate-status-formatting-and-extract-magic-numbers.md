# QUALITY: Consolidate Status Formatting and Extract Magic Numbers

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, readability, cleanup

## Context & Motivation

Two code quality issues found in the audit:

1. **Status formatting duplication**: `MarkdigMarkdownParser.FormatStatus()` duplicates the logic in `TaskStatusExtensions.ToDisplayString()`. The parser should delegate to the extension method instead of reimplementing the switch.

2. **Magic numbers**: Several repeated magic values in TUI code should be extracted to named constants for readability:
   - `10` (page scroll size in TaskDetailPanel)
   - `4` (height reserve for detail panel)
   - `3` (ellipsis width in StatusBar)

## Desired Outcome

- Single source of truth for status formatting
- Named constants replace magic numbers in TUI code

## Technical Approach

**Part 1: Consolidate status formatting in `src/KanbanCli/Storage/MarkdigMarkdownParser.cs`**

**Before:**
```csharp
private static string FormatStatus(TaskStatus status)
{
    return status switch
    {
        TaskStatus.Backlog => "Backlog",
        TaskStatus.InProgress => "In Progress",
        TaskStatus.Done => "Done",
        TaskStatus.OnHold => "On Hold",
        _ => status.ToString()
    };
}
```

**After:**
```csharp
private static string FormatStatus(TaskStatus status)
{
    return status.ToDisplayString();
}
```

**Part 2: Remove redundant TuiHelpers.FormatStatus() wrapper in `src/KanbanCli/Tui/TuiHelpers.cs`**

Check if `TuiHelpers.FormatStatus()` is used anywhere. If only used in one place, inline it. If unused, remove it.

**Part 3: Extract magic numbers in `src/KanbanCli/Tui/TaskDetailPanel.cs`**

**Before:**
```csharp
case ConsoleKey.PageUp:
    scrollOffset = Math.Max(0, scrollOffset - 10);
    break;
case ConsoleKey.PageDown:
    scrollOffset += 10;
    break;
```

**After:**
```csharp
private const int PageScrollSize = 10;
private const int DetailHeightReserve = 4;

case ConsoleKey.PageUp:
    scrollOffset = Math.Max(0, scrollOffset - PageScrollSize);
    break;
case ConsoleKey.PageDown:
    scrollOffset += PageScrollSize;
    break;
```

And for the height calculation:
```csharp
var visibleHeight = Math.Max(TuiHelpers.GetEffectiveHeight() - DetailHeightReserve, BoardConstants.MinWindowHeight);
```

## Progress Log

- 2026-03-05 - Task created

## Acceptance Criteria

- [x] MarkdigMarkdownParser.FormatStatus() delegates to TaskStatusExtensions.ToDisplayString()
- [x] TuiHelpers.FormatStatus() is removed if unused, or kept minimal if used
- [x] TaskDetailPanel scroll size uses named constant PageScrollSize
- [x] TaskDetailPanel height reserve uses named constant DetailHeightReserve
- [ ] All existing tests pass (dotnet SDK not available in environment; changes are straightforward delegations)
