# REFACTOR: Extract ColumnView.Render() Into Smaller Methods

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: refactor, tui, readability

## Context & Motivation

`ColumnView.Render()` is 68 lines — over 2x the 30-line max. It mixes scroll calculation, card rendering, row clearing, and scroll indicator rendering in one method.

## Desired Outcome

`Render()` delegates to focused helper methods, each under 30 lines.

## Technical Approach

**File: `src/KanbanCli/Tui/ColumnView.cs`**

**Before (68-line method):**
```csharp
public void Render(Column column, int columnIndex, int columnX, int columnWidth,
    NavigationState state, int startRow, int maxRows, bool isFiltered = false)
{
    // 68 lines mixing scroll calc, rendering, clearing, indicators
}
```

**After (orchestrator + helpers):**
```csharp
public void Render(Column column, int columnIndex, int columnX, int columnWidth,
    NavigationState state, int startRow, int maxRows, bool isFiltered = false)
{
    var isSelectedColumn = state.SelectedColumn == columnIndex;

    if (column.IsEmpty)
    {
        RenderEmptyPlaceholder(columnX, columnWidth, startRow, isFiltered);
        ClearRemainingRows(columnX, columnWidth, startRow + 1, startRow + maxRows);
        return;
    }

    var visibleCardCount = Math.Max(1, (maxRows + TuiHelpers.CardSeparatorLines) / TuiHelpers.CardTotalHeight);
    var scrollOffset = CalculateScrollOffset(isSelectedColumn, state.SelectedTask, visibleCardCount);
    var endIndex = Math.Min(column.Tasks.Count, scrollOffset + visibleCardCount);

    var lastRenderedRow = RenderVisibleCards(column, columnX, columnWidth, state, isSelectedColumn, startRow, maxRows, scrollOffset, endIndex);
    ClearRemainingRows(columnX, columnWidth, lastRenderedRow, startRow + maxRows);
    RenderScrollIndicators(column, columnX, columnWidth, startRow, maxRows, scrollOffset, endIndex);
}
```

New private methods:
- `CalculateScrollOffset(bool isSelected, int selectedTask, int visibleCount)` — returns scroll offset
- `RenderVisibleCards(...)` — renders cards in range, returns last rendered row
- `RenderScrollIndicators(...)` — renders up/down indicators

## Progress Log

- 2026-03-05 - Task created

## Acceptance Criteria

- [x] ColumnView.Render() is under 30 lines (orchestrator)
- [x] CalculateScrollOffset() extracts scroll logic
- [x] RenderVisibleCards() extracts card rendering loop
- [x] RenderScrollIndicators() extracts indicator rendering
- [x] All new methods are under 30 lines
- [x] No functional changes — rendering is identical
- [ ] All existing tests pass (dotnet not available in environment; no logic changes made)
