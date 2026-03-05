# 061 - BUG: Fix Board Rendering — Triple Display and Missing Colors

**Type:** Bug
**Priority:** High (Critical — app is unusable)
**Status:** Backlog

## Context & Motivation

The board TUI has two critical rendering bugs that make the application unusable:

1. **Triple/repeated board rendering**: The board renders 3 times vertically stacked. Root cause: `GetEffectiveHeight()` has no maximum cap — if `Console.WindowHeight` returns a large value (e.g., 200+), `CalculateLayout()` computes `statusBarRow` and `bottomBorderRow` far below the visible area, causing content to wrap/repeat.

2. **No colors displayed**: The `RenderBuffered()` method in `KanbanApp.cs` redirects `Console.Out` to a `BufferedStream` via `Console.SetOut()`. On .NET on Linux, `Console.ForegroundColor`/`Console.BackgroundColor` write ANSI escape codes through `Console.Out`. However, when the output is redirected, .NET may suppress ANSI codes or the buffer may not flush them correctly, resulting in no colors.

## Technical Approach

### Fix 1: Add MaxBoardHeight and cap GetEffectiveHeight()

**Add constant to BoardConstants.cs:**

**Before:**
```csharp
public const int MinWindowHeight = 10;
```

**After:**
```csharp
public const int MinWindowHeight = 10;

/// <summary>
/// Maximum board height to prevent overly stretched layouts.
/// Can be overridden via the KANBAN_HEIGHT environment variable.
/// </summary>
public const int MaxBoardHeight = 40;

/// <summary>
/// Environment variable name to override board height.
/// </summary>
public const string HeightEnvVar = "KANBAN_HEIGHT";
```

**Update GetEffectiveHeight() in TuiHelpers.cs:**

**Before:**
```csharp
public static int GetEffectiveHeight()
{
    try
    {
        return Math.Max(Console.WindowHeight, BoardConstants.MinWindowHeight);
    }
    catch (IOException)
    {
        return BoardConstants.MinWindowHeight;
    }
}
```

**After:**
```csharp
public static int GetEffectiveHeight()
{
    var envValue = Environment.GetEnvironmentVariable(BoardConstants.HeightEnvVar);
    if (!string.IsNullOrEmpty(envValue) && int.TryParse(envValue, out var envHeight) && envHeight >= BoardConstants.MinWindowHeight)
    {
        return envHeight;
    }

    var consoleHeight = TryGetConsoleHeight();
    return Math.Clamp(consoleHeight, BoardConstants.MinWindowHeight, BoardConstants.MaxBoardHeight);
}
```

**Add TryGetConsoleHeight() in TuiHelpers.cs:**

**Before:**
```csharp
private static int TryGetConsoleWidth()
{
    try
    {
        return Console.WindowWidth;
    }
    catch (IOException)
    {
        return BoardConstants.MaxBoardWidth;
    }
}
```

**After (add new method after TryGetConsoleWidth):**
```csharp
private static int TryGetConsoleHeight()
{
    try
    {
        return Console.WindowHeight;
    }
    catch (IOException)
    {
        return BoardConstants.MaxBoardHeight;
    }
}
```

### Fix 2: Remove buffered rendering to restore colors

The `RenderBuffered()` method may be suppressing ANSI color codes. Replace it with direct rendering. The flicker-free optimization is less important than having a working, colored display.

**In KanbanApp.cs:**

**Before:**
```csharp
// Buffer all console output during rendering to reduce system calls
RenderBuffered(_displayBoard, _state, filterInfo);
```

**After:**
```csharp
_boardRenderer.Render(_displayBoard, _state, filterInfo);
```

**Remove the RenderBuffered method entirely.**

### Fix 3: Update SafeSetCursorPosition to use effective dimensions

**Before:**
```csharp
public static void SafeSetCursorPosition(int x, int y)
{
    try
    {
        var safeX = Math.Clamp(x, 0, Math.Max(Console.WindowWidth - 1, 0));
        var safeY = Math.Clamp(y, 0, Math.Max(Console.WindowHeight - 1, 0));
        Console.SetCursorPosition(safeX, safeY);
    }
    catch (IOException)
    {
        // Terminal may have been resized between check and set
    }
}
```

**After:**
```csharp
public static void SafeSetCursorPosition(int x, int y)
{
    try
    {
        var safeX = Math.Clamp(x, 0, Math.Max(GetEffectiveWidth() - 1, 0));
        var safeY = Math.Clamp(y, 0, Math.Max(GetEffectiveHeight() - 1, 0));
        Console.SetCursorPosition(safeX, safeY);
    }
    catch (IOException)
    {
        // Terminal may have been resized between check and set
    }
}
```

## Dependencies

None — this is a standalone fix.

## Risks

- Removing buffered rendering may introduce flicker on some terminals. This is acceptable — a working display with colors is far more important than flicker-free rendering without colors.
- The height cap of 40 might be too small for very large terminals. The KANBAN_HEIGHT env var provides an escape hatch.

## Acceptance Criteria

- [x] `BoardConstants.MaxBoardHeight` constant added (value: 40)
- [x] `BoardConstants.HeightEnvVar` constant added ("KANBAN_HEIGHT")
- [x] `GetEffectiveHeight()` caps height to `MaxBoardHeight` with env var override
- [x] `TryGetConsoleHeight()` helper added to TuiHelpers
- [x] `RenderBuffered()` removed from KanbanApp; direct rendering used instead
- [x] `SafeSetCursorPosition` uses `GetEffectiveWidth()`/`GetEffectiveHeight()` instead of raw Console dimensions
- [x] `using System.Text;` removed from KanbanApp.cs (no longer needed without BufferedStream)
- [x] Board renders exactly once (no triple display)
- [x] Colors are visible in the rendered board
