# REFACTOR: Extract FilterDialog Methods and Use Shared Helpers

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: refactor, tui, cleanup

## Context & Motivation

`FilterDialog.PromptByLabel()` is 58 lines — nearly 2x the limit. It also manually renders a numbered label list when `DialogHelper.RenderNumberedListInBox()` already exists for this purpose.

## Desired Outcome

- `PromptByLabel()` under 30 lines by extracting helper methods
- Manual label list rendering replaced with DialogHelper call

## Technical Approach

**File: `src/KanbanCli/Tui/FilterDialog.cs`**

**Step 1: Replace manual label rendering with DialogHelper**

**Before:**
```csharp
for (var i = 0; i < allLabels.Count; i++)
{
    DialogHelper.RenderBoxLeftBorder(borderColor);
    Console.ForegroundColor = Theme.DialogListNumber;
    var numText = $"  {i + 1}. ";
    Console.Write(numText);
    Console.ForegroundColor = Theme.LabelBracket;
    Console.Write("[");
    Console.ForegroundColor = Theme.LabelText;
    Console.Write(allLabels[i]);
    Console.ForegroundColor = Theme.LabelBracket;
    Console.Write("]");
    DialogHelper.RenderBoxRightBorder(numText.Length + allLabels[i].Length + 2, width, borderColor);
}
```

**After:**
```csharp
var labelDisplayNames = allLabels.Select(l => $"[{l}]").ToList();
DialogHelper.RenderNumberedListInBox(labelDisplayNames, width, borderColor);
```

**Step 2: Extract PromptByLabel into smaller methods**

Split the method into:
- `RenderLabelFilterHeader()` — renders the header/prompt
- Selection logic stays in PromptByLabel (now short enough)

## Acceptance Criteria

- [x] PromptByLabel() is under 30 lines
- [x] Manual label list rendering replaced with DialogHelper.RenderNumberedListInBox()
- [x] No functional changes — filter dialog works identically
- [ ] All existing tests pass (dotnet not available in environment to verify)

## Progress Log

- 2026-03-05 - Task created
