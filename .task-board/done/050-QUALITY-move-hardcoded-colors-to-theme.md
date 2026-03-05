# QUALITY: Move Hardcoded ConsoleColor Values to Theme.cs

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: quality, tui

## Context & Motivation

CLAUDE.md states: "All TUI colors come from Theme.cs — never hardcode ConsoleColor values in TUI files." Currently `TuiHelpers.cs` has 18 hardcoded `ConsoleColor` values in `GetPriorityColor()` and `GetTypeColor()` methods. These should be centralized in Theme.cs for consistent theming.

## Desired Outcome

All color values in the TUI layer reference Theme.cs constants. Zero hardcoded `ConsoleColor` values in any TUI file except Theme.cs itself.

## Technical Approach

**Step 1: Add new color constants to `src/KanbanCli/Tui/Theme.cs`**

```csharp
// ── Priority Colors ────────────────────────────────────────────
public static readonly ConsoleColor PriorityHigh = ConsoleColor.Red;
public static readonly ConsoleColor PriorityMedium = ConsoleColor.Yellow;
public static readonly ConsoleColor PriorityLow = ConsoleColor.Green;
public static readonly ConsoleColor PriorityDefault = ConsoleColor.Gray;

// ── Task Type Colors ───────────────────────────────────────────
public static readonly ConsoleColor TypeFeature = ConsoleColor.Cyan;
public static readonly ConsoleColor TypeBug = ConsoleColor.Red;
public static readonly ConsoleColor TypeSecurity = ConsoleColor.DarkYellow;
public static readonly ConsoleColor TypeRefactor = ConsoleColor.Green;
public static readonly ConsoleColor TypeTest = ConsoleColor.Magenta;
public static readonly ConsoleColor TypePerf = ConsoleColor.DarkCyan;
public static readonly ConsoleColor TypeDocs = ConsoleColor.Blue;
public static readonly ConsoleColor TypeDesign = ConsoleColor.DarkMagenta;
public static readonly ConsoleColor TypeEpic = ConsoleColor.White;
public static readonly ConsoleColor TypeExplore = ConsoleColor.DarkGreen;
public static readonly ConsoleColor TypeCleanup = ConsoleColor.DarkGray;
public static readonly ConsoleColor TypeA11y = ConsoleColor.DarkYellow;
public static readonly ConsoleColor TypeQuality = ConsoleColor.Yellow;
public static readonly ConsoleColor TypeDefault = ConsoleColor.Gray;
```

**Step 2: Update `src/KanbanCli/Tui/TuiHelpers.cs`**

**Before:**
```csharp
public static ConsoleColor GetPriorityColor(Priority priority)
{
    return priority switch
    {
        Priority.High => ConsoleColor.Red,
        Priority.Medium => ConsoleColor.Yellow,
        Priority.Low => ConsoleColor.Green,
        _ => ConsoleColor.Gray
    };
}
```

**After:**
```csharp
public static ConsoleColor GetPriorityColor(Priority priority)
{
    return priority switch
    {
        Priority.High => Theme.PriorityHigh,
        Priority.Medium => Theme.PriorityMedium,
        Priority.Low => Theme.PriorityLow,
        _ => Theme.PriorityDefault
    };
}
```

Same pattern for `GetTypeColor()` — replace all 13 `ConsoleColor.Xxx` with `Theme.TypeXxx`.

## Acceptance Criteria

- [x] Theme.cs has new constants for all priority colors (4 values)
- [x] Theme.cs has new constants for all task type colors (14 values including default)
- [x] TuiHelpers.GetPriorityColor() references Theme constants instead of hardcoded values
- [x] TuiHelpers.GetTypeColor() references Theme constants instead of hardcoded values
- [x] No hardcoded ConsoleColor values remain in any TUI file except Theme.cs
- [ ] All existing tests pass

## Progress Log

- 2026-03-05 - Task created
