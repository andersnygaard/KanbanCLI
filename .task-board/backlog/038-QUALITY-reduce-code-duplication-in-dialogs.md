# QUALITY: Reduce Code Duplication in Dialog Classes

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, refactor, tui

## Context & Motivation

Several dialog classes (NewTaskDialog, TaskDetailPanel, MoveDialog, ConfirmDialog, PriorityDialog, FilterDialog) have duplicate patterns for:
1. Prompting text input (Console.Write label + Console.ReadLine)
2. Rendering numbered selection lists
3. Parsing numeric choices
4. Error/success message display

The `NewTaskDialog` has its own `PromptText` and `PromptEnumInBox<T>` methods that duplicate what `DialogHelper` already offers. The `TaskDetailPanel` has `RenderNumberedList` and `PromptForChoice` that could be consolidated into `DialogHelper`.

## Current State

### NewTaskDialog has its own prompt methods (lines 59-113):
```csharp
private static string PromptText(string prompt, int width, ConsoleColor borderColor)
{
    DialogHelper.RenderBoxLeftBorder(borderColor);
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write(prompt);
    Console.ResetColor();
    Console.ForegroundColor = ConsoleColor.White;
    var input = Console.ReadLine() ?? string.Empty;
    Console.ResetColor();
    return input;
}
```

### TaskDetailPanel has its own list/choice helpers (lines 231-256):
```csharp
private static void RenderNumberedList(IReadOnlyList<string> items) { ... }
private static int? PromptForChoice(int maxValue) { ... }
```

## Desired Outcome

Consolidate these shared patterns into `DialogHelper` so all dialogs use the same building blocks. This reduces duplication and ensures consistent styling.

## Acceptance Criteria

- [ ] Move `PromptText` pattern into `DialogHelper.PromptTextInBox(string prompt, int width, ConsoleColor borderColor)`
- [ ] Move `RenderNumberedList` into `DialogHelper.RenderNumberedListInBox(IReadOnlyList<string> items, int width, ConsoleColor borderColor)`
- [ ] Move `PromptForChoice` into `DialogHelper.PromptNumericChoice(int maxValue, bool allowZeroCancel = false)`
- [ ] Update `NewTaskDialog` to use `DialogHelper` methods
- [ ] Update `TaskDetailPanel` to use `DialogHelper` methods
- [ ] Verify consistent styling across all dialogs
- [ ] All existing tests pass (`dotnet build src/` and `dotnet test src/`)

## Technical Approach

Add to `DialogHelper`:

```csharp
public static string PromptTextInBox(string prompt, int width, ConsoleColor borderColor)
{
    RenderBoxLeftBorder(borderColor);
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write(prompt);
    Console.ResetColor();
    Console.ForegroundColor = ConsoleColor.White;
    var input = Console.ReadLine() ?? string.Empty;
    Console.ResetColor();
    return input;
}

public static void RenderNumberedListInBox(IReadOnlyList<string> items, int width, ConsoleColor borderColor)
{
    for (var i = 0; i < items.Count; i++)
    {
        RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        var numText = $"  {i + 1}. ";
        Console.Write(numText);
        Console.ForegroundColor = ConsoleColor.White;
        var valText = items[i];
        Console.Write(valText);
        RenderBoxRightBorder(numText.Length + valText.Length, width, borderColor);
    }
}

public static int? PromptNumericChoice(int maxValue, bool allowZeroCancel = false)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    var cancelHint = allowZeroCancel ? ", or 0 to cancel" : "";
    Console.Write($"  Enter number (1-{maxValue}{cancelHint}): ");
    Console.ResetColor();

    var input = Console.ReadLine()?.Trim() ?? string.Empty;
    if (!int.TryParse(input, out var choice) || choice < 1 || choice > maxValue)
        return null;

    return choice;
}
```

## Progress Log

- 2026-03-05 - Task created
