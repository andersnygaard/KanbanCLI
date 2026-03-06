# QUALITY: Console Bounds Checking and Small Terminal Handling

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: quality, tui, robustness

## Context & Motivation

Multiple TUI components call `Console.SetCursorPosition()` without verifying the position is within the terminal window bounds. On very small terminals (< 40 chars wide, < 15 rows), this can throw exceptions and crash the app.

The audit identified this in BoardView, ColumnView, TaskCard, and StatusBar.

## Technical Approach

Add to `TuiHelpers`:
```csharp
public static void SafeSetCursorPosition(int x, int y)
{
    var safeX = Math.Clamp(x, 0, Math.Max(Console.WindowWidth - 1, 0));
    var safeY = Math.Clamp(y, 0, Math.Max(Console.WindowHeight - 1, 0));
    Console.SetCursorPosition(safeX, safeY);
}
```

Then find-and-replace `Console.SetCursorPosition` with `TuiHelpers.SafeSetCursorPosition` in the TUI components.

Add a minimum size check at the start of `KanbanApp.Run()`.

## Progress Log

- 2026-03-05 - Task created from audit findings

## Acceptance Criteria

- [ ] Add a safe wrapper `TuiHelpers.SafeSetCursorPosition(int x, int y)` that clamps to valid bounds
- [ ] Replace direct `Console.SetCursorPosition` calls in BoardView, ColumnView, TaskCard with the safe wrapper
- [ ] Add minimum terminal size check in KanbanApp.Run() — show friendly message if too small
- [ ] BoardView gracefully handles windows narrower than 40 chars
- [ ] All existing tests pass
