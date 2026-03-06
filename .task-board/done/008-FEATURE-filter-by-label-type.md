# FEATURE: Filter Board by Label and Type

**Status**: In Progress
**Created**: 2026-03-04
**Priority**: High
**Labels**: tui, filtering, core
**Estimated Effort**: Medium - 2-3 days

## Context & Motivation

The spec defines filtering as a core interactive feature: pressing `f` should filter the board by label or type. The key binding is mapped (`BoardCommand.ToggleFilter`) and `TaskItem.MatchesFilter(FilterCriteria)` already works with full test coverage. However, the `ToggleFilter` command is completely unhandled in `Program.cs` -- pressing `f` does nothing. The `StatusBar` already accepts a `filterInfo` parameter but it is never passed.

This is a visible gap in the TUI's interactivity. All the domain infrastructure exists; only the TUI wiring is missing.

## Current State

- `FilterCriteria` record exists with `Type`, `Priority`, and `Label` fields
- `TaskItem.MatchesFilter()` is implemented and tested (3 test cases)
- `BoardCommand.ToggleFilter` enum value exists
- `KeyboardInputHandler` maps `f` to `BoardCommand.ToggleFilter`
- `StatusBar.Render()` accepts optional `filterInfo` parameter
- `Program.cs` has no `case BoardCommand.ToggleFilter` handler -- the command is silently ignored
- No filter state is tracked in the main loop
- `BoardView.Render()` does not pass filter info to `StatusBar`
- `Column` model has no filtering method

## Desired Outcome

Pressing `f` opens a filter prompt where the user can filter by label or type. The board only shows tasks matching the active filter. The status bar shows the active filter. Pressing `f` again clears the filter. The feature integrates cleanly with existing domain logic.

## Affected Components

### Files to Modify
- `src/KanbanCli/Program.cs` -- add ToggleFilter case, filter state variable, apply filter to board
- `src/KanbanCli/Tui/BoardView.cs` -- pass filter info to StatusBar
- `src/KanbanCli/Tui/IBoardRenderer.cs` -- optionally extend Render to accept filter info
- `src/KanbanCli/Tui/StatusBar.cs` -- no changes needed (already supports filterInfo parameter)

### Files to Create
- `src/KanbanCli/Tui/FilterDialog.cs` -- prompt for filter type (by Type or Label) and value

### Dependencies
- **External**: None
- **Internal**: FilterCriteria model, TaskItem.MatchesFilter(), StatusBar filterInfo support
- **Blocking**: None -- all prerequisite code exists

## Technical Approach

### Architecture Decisions

- **Filter state in main loop**: Track `FilterCriteria? activeFilter` alongside `NavigationState`. When non-null, apply to displayed tasks.
- **Toggle behavior**: If filter is active and user presses `f`, clear it. If no filter, show prompt.
- **Apply filter at display time**: Don't filter at repository level. Filter the tasks in each column when building the board view. This keeps the data layer unaware of UI concerns.
- **FilterDialog**: Small dialog similar to NewTaskDialog and PromptMoveTarget. User chooses "By Type" or "By Label", then selects value.

### Implementation Steps

1. **Create FilterDialog.cs**
   ```
   - Show menu: 1. Filter by Type, 2. Filter by Label, 3. Clear filter
   - If Type: show enum picker for TaskType
   - If Label: prompt for label text (or show existing labels from current board)
   - Return FilterCriteria or null (cancel)
   ```

2. **Update Program.cs**
   - Add `FilterCriteria? activeFilter = null;` to main loop state
   - Add `case BoardCommand.ToggleFilter:` handler
     - If activeFilter is not null, clear it (set to null)
     - If null, show FilterDialog, set activeFilter from result
   - After `boardService.GetBoard()`, filter each column's tasks if activeFilter is set
   - Pass filter info string to boardRenderer or StatusBar
   - Reset navigation state when filter changes

3. **Update BoardView/IBoardRenderer**
   - Pass optional filter info to StatusBar.Render() call
   - Consider extending IBoardRenderer.Render() to accept filter info, or pass it separately

4. **Adjust NavigationState**
   - When filter is toggled, reset SelectedTask to 0 to avoid out-of-bounds on filtered list

### Risks & Considerations

- **Risk**: Filtering at display time means task indices in the filtered view don't match original indices -- MoveTask/DeleteTask must reference the correct task -- **Mitigation**: Apply filter to create a filtered Board, operate on filtered task objects (which carry their real IDs)
- **Risk**: Scope creep into multi-filter support -- **Mitigation**: Start with single filter only (one criteria at a time), matches spec
- **Readability**: Keep FilterDialog concise, reuse PromptEnum pattern from NewTaskDialog

## Code References

### From specs.md -- Key Bindings
```
| f  | Filter by label/type |
```

### From specs.md -- Features
```
- Label display with distinct styling
```

### Existing infrastructure
- `FilterCriteria` at `src/KanbanCli/Models/FilterCriteria.cs`
- `TaskItem.MatchesFilter()` at `src/KanbanCli/Models/TaskItem.cs:32-44`
- `StatusBar.Render(filterInfo)` at `src/KanbanCli/Tui/StatusBar.cs:8`

## Progress Log

- 2026-03-04 - Task created via backlog-scan
- 2026-03-05 - Implemented: FilterDialog.cs created, IBoardRenderer extended, BoardView passes filterInfo to StatusBar, Program.cs wires ToggleFilter handler with activeFilter state and ApplyFilter/BuildFilterInfo helpers, ColumnView shows "(no matching tasks)" placeholder. Build: 0 errors. Tests: 49/49 passed.

## Acceptance Criteria

- [x] `Program.cs` handles `BoardCommand.ToggleFilter` command
- [x] Filter prompt allows selecting filter by Type or by Label
- [x] When a filter is active, `BoardService.GetBoard()` or the main loop filters tasks using `TaskItem.MatchesFilter()`
- [x] `StatusBar` displays active filter info (e.g., "Filter: Bug" or "Filter: frontend")
- [x] Pressing `f` when a filter is active clears the filter (toggle behavior)
- [x] Empty filtered columns show "(no matching tasks)" placeholder
- [x] Navigation adjusts correctly when filter reduces visible tasks
- [x] Filter state is preserved across board refreshes (move, create, delete)