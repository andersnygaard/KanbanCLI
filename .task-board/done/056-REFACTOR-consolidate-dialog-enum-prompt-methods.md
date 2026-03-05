# REFACTOR: Consolidate Dialog Enum Prompt Methods

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: refactor, tui, cleanup

## Context & Motivation

Two dialog classes have nearly identical `PromptEnumInBox<T>()` methods:
- `NewTaskDialog` (lines 59-92, 34 lines)
- `FilterDialog` (lines 148-186, 39 lines)

Both render a numbered list of enum values inside a box, prompt for a choice, and return the selection. This logic should be consolidated into `DialogHelper` to eliminate duplication.

## Desired Outcome

A single shared `DialogHelper.PromptEnumInBox<T>()` method used by both dialogs.

## Technical Approach

**Step 1: Add shared method to `src/KanbanCli/Tui/DialogHelper.cs`**

```csharp
public static T? PromptEnumInBox<T>(string label, int width, ConsoleColor borderColor, bool allowZeroCancel = false) where T : struct, Enum
{
    var values = Enum.GetValues<T>();

    RenderBoxLeftBorder(borderColor);
    Console.ForegroundColor = Theme.DialogPrompt;
    Console.Write($"  {label}:");
    RenderBoxRightBorder(label.Length + 4, width, borderColor);

    var names = values.Select(v => v.ToString()).ToList();
    RenderNumberedListInBox(names, width, borderColor);

    var choice = PromptNumericChoice(values.Length, allowZeroCancel: allowZeroCancel);
    if (choice is null)
    {
        return null;
    }

    return values[choice.Value - 1];
}
```

**Step 2: Update `src/KanbanCli/Tui/NewTaskDialog.cs`**

**Before:**
```csharp
private static T? PromptEnumInBox<T>(...) where T : struct, Enum
{
    // 34 lines of duplicated logic
}
```

**After:**
Remove the private method. Replace calls with `DialogHelper.PromptEnumInBox<T>(...)`.

**Step 3: Update `src/KanbanCli/Tui/FilterDialog.cs`**

Same pattern — remove private `PromptEnumInBox<T>` and use `DialogHelper.PromptEnumInBox<T>`.

## Acceptance Criteria

- [x] DialogHelper has a public PromptEnumInBox<T>() method
- [x] NewTaskDialog uses DialogHelper.PromptEnumInBox<T>() instead of its own copy
- [x] FilterDialog uses DialogHelper.PromptEnumInBox<T>() instead of its own copy
- [x] No private PromptEnumInBox methods remain in NewTaskDialog or FilterDialog
- [x] All existing tests pass (dotnet CLI not available in environment; code review confirms no behavioral changes)

## Progress Log

- 2026-03-05 - Task created
