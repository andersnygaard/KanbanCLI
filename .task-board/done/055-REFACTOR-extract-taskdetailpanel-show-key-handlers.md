# REFACTOR: Extract TaskDetailPanel.Show() Key Handlers

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: refactor, tui, readability

## Context & Motivation

CLAUDE.md requires methods to be max ~30 lines. `TaskDetailPanel.Show()` is 55 lines with a large switch statement handling 9 key cases. Extracting the switch into a handler method will bring Show() under the limit.

## Desired Outcome

`Show()` becomes a short loop (~20 lines) that delegates key handling to a separate method.

## Technical Approach

**File: `src/KanbanCli/Tui/TaskDetailPanel.cs`**

**Before (55 lines):**
```csharp
public TaskItem Show(TaskItem task)
{
    var current = task;
    var scrollOffset = 0;

    while (true)
    {
        RenderDetailView(current, scrollOffset);
        var key = Console.ReadKey(intercept: true);

        switch (key.Key)
        {
            case ConsoleKey.T:
                current = HandleEditTitle(current);
                scrollOffset = 0;
                break;
            // ... 8 more cases ...
        }
    }
}
```

**After (~20 lines + ~30-line handler):**
```csharp
public TaskItem Show(TaskItem task)
{
    var current = task;
    var scrollOffset = 0;

    while (true)
    {
        RenderDetailView(current, scrollOffset);
        var key = Console.ReadKey(intercept: true);
        var (updatedTask, updatedScroll, shouldExit) = HandleKeyPress(current, scrollOffset, key);
        current = updatedTask;
        scrollOffset = updatedScroll;
        if (shouldExit)
        {
            return current;
        }
    }
}

private (TaskItem Task, int ScrollOffset, bool ShouldExit) HandleKeyPress(
    TaskItem current, int scrollOffset, ConsoleKeyInfo key)
{
    return key.Key switch
    {
        ConsoleKey.T => (HandleEditTitle(current), 0, false),
        ConsoleKey.L => (HandleEditLabels(current), 0, false),
        ConsoleKey.P => (HandleEditPriority(current), 0, false),
        ConsoleKey.UpArrow => (current, Math.Max(0, scrollOffset - 1), false),
        ConsoleKey.DownArrow => (current, scrollOffset + 1, false),
        ConsoleKey.PageUp => (current, Math.Max(0, scrollOffset - PageScrollSize), false),
        ConsoleKey.PageDown => (current, scrollOffset + PageScrollSize, false),
        ConsoleKey.Home => (current, 0, false),
        ConsoleKey.Escape => (current, scrollOffset, true),
        _ => (current, scrollOffset, false)
    };
}
```

## Progress Log

- 2026-03-05 - Task created

## Acceptance Criteria

- [x] Show() is under 30 lines
- [x] HandleKeyPress() extracts all switch logic into a separate method
- [x] HandleKeyPress() is under 30 lines
- [x] No functional changes — all key bindings work identically
- [ ] All existing tests pass (dotnet not available in environment to verify)
