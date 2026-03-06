# BUG: Fix Dialog Boundary Check Vulnerabilities

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: bug, tui, quality

## Context & Motivation

Code audit found boundary check issues in dialog selection methods that could cause IndexOutOfRangeException:

1. `DialogHelper.PromptEnum<T>()` — when `allowZeroCancel=true`, choice=0 bypasses validation and accesses `values[-1]`
2. `MoveDialog.Show()` — uses `BoardConstants.ColumnOrder[choice - 1]` which may not match `board.Columns` if columns are ever reordered
3. `DialogHelper.PromptNumericChoice()` — doesn't handle choice=0 when `allowZeroCancel=true`

These are correctness bugs that could crash the application.

## Desired Outcome

All dialog selection methods correctly validate input bounds and return null on cancel, with no possibility of index-out-of-range access.

## Technical Approach

**File 1: `src/KanbanCli/Tui/DialogHelper.cs` — PromptEnum<T>()**

**Before:**
```csharp
if (!int.TryParse(input, out var choice) || choice < 1 || choice > values.Length)
{
    if (!allowZeroCancel)
    {
        ShowError("Invalid selection. Press any key to cancel.");
        Console.ReadKey(intercept: true);
    }
    return null;
}
return values[choice - 1];
```

**After:**
```csharp
if (!int.TryParse(input, out var choice))
{
    if (!allowZeroCancel)
    {
        ShowError("Invalid selection. Press any key to cancel.");
        Console.ReadKey(intercept: true);
    }
    return null;
}

if (allowZeroCancel && choice == 0)
{
    return null;
}

if (choice < 1 || choice > values.Length)
{
    ShowError("Invalid selection. Press any key to cancel.");
    Console.ReadKey(intercept: true);
    return null;
}

return values[choice - 1];
```

**File 2: `src/KanbanCli/Tui/DialogHelper.cs` — PromptNumericChoice()**

**Before:**
```csharp
if (!int.TryParse(input, out var choice) || choice < 1 || choice > maxValue)
    return null;
return choice;
```

**After:**
```csharp
if (!int.TryParse(input, out var choice))
{
    return null;
}

if (allowZeroCancel && choice == 0)
{
    return null;
}

if (choice < 1 || choice > maxValue)
{
    return null;
}

return choice;
```

**File 3: `src/KanbanCli/Tui/MoveDialog.cs` — Show()**

**Before:**
```csharp
return BoardConstants.ColumnOrder[choice - 1];
```

**After:**
```csharp
return board.Columns[choice - 1].Status;
```

## Progress Log

- 2026-03-05 - Task created

## Acceptance Criteria

- [x] PromptEnum<T> correctly returns null when choice is 0 and allowZeroCancel is true
- [x] PromptEnum<T> shows error for out-of-range choices regardless of allowZeroCancel
- [x] PromptNumericChoice correctly returns null when choice is 0 and allowZeroCancel is true
- [x] MoveDialog.Show uses board.Columns instead of BoardConstants.ColumnOrder for indexing
- [ ] All existing tests pass
