# FEATURE: TUI Board Rendering (BoardView, ColumnView, TaskCard)

**Status**: Backlog
**Created**: 2026-03-04
**Priority**: High
**Labels**: tui, rendering, ui
**Estimated Effort**: Medium-Complex - 3-5 days

## Context & Motivation

The board rendering is the visual heart of the application — the spec describes a live board view with columns side by side, color-coded priorities, label styling, and a task detail preview panel. This is what users see and interact with.

The spec defines a component-based TUI architecture: `BoardView` (overall layout), `ColumnView` (one column), `TaskCard` (one task row), `TaskDetailPanel` (expanded view), and `StatusBar` (bottom keybindings display). Each component is a small, focused class with clear responsibility.

## Current State

No TUI rendering code exists. The spec defines:
- `IBoardRenderer` / `BoardView` — overall board layout
- `IColumnView` / `ColumnView` — single column rendering
- `ITaskCard` / `TaskCard` — single task row
- `TaskDetailPanel` — expanded task view
- `StatusBar` — keybinding hints
- `NewTaskDialog` — task creation form
- Color-coded priority indicators
- Label display with distinct styling

## Desired Outcome

A fully rendered terminal board with columns side by side, task cards with priority colors and labels, a detail panel for selected tasks, and a status bar showing available keybindings. Combined with the input handler (task 005), this completes the interactive TUI.

## Acceptance Criteria

- [x] `IBoardRenderer` interface defined
- [x] `BoardView` renders columns side by side in the terminal
- [x] `ColumnView` renders column header + task list
- [x] `TaskCard` renders one task row: ID, type prefix, title, priority color, labels
- [x] Priority color coding: High = red, Medium = yellow, Low = green (or similar)
- [x] Labels displayed with distinct styling (brackets, different color)
- [x] Selected task/column visually highlighted
- [x] `TaskDetailPanel` shows full task content when Enter is pressed
- [x] `StatusBar` displays available keybindings at the bottom
- [x] `NewTaskDialog` prompts for type, title, priority, labels
- [x] Board refreshes after operations (move, create, delete)
- [x] Handles varying terminal sizes gracefully
- [x] Column widths distribute evenly across available terminal width
- [x] Empty columns show a placeholder message

## Affected Components

### Files to Create
- `src/KanbanCli/Tui/IBoardRenderer.cs`
- `src/KanbanCli/Tui/BoardView.cs`
- `src/KanbanCli/Tui/ColumnView.cs`
- `src/KanbanCli/Tui/TaskCard.cs`
- `src/KanbanCli/Tui/TaskDetailPanel.cs`
- `src/KanbanCli/Tui/StatusBar.cs`
- `src/KanbanCli/Tui/NewTaskDialog.cs`

### Dependencies
- **External**: TUI framework (Terminal.Gui or Spectre.Console — decided in task 001)
- **Internal**: Models (Board, Column, TaskItem, Enums) from task 002
- **Internal**: Services (ITaskService, IBoardService) from task 004
- **Internal**: Input Handler (NavigationState) from task 005
- **Blocking**: Task 001, 002, 004, 005

## Technical Approach

### Architecture Decisions

- **Component-based**: Each visual element is its own class — small, focused, testable (even if TUI tests are manual per spec)
- **Render loop**: `BoardView` coordinates a full redraw on each frame/update. Sub-components render into a buffer or directly to console.
- **Separation**: Rendering components receive data (Board model, NavigationState), they do NOT fetch data themselves. Data flows down from the main loop.

### TUI Component Hierarchy

```
BoardView (full screen)
├── ColumnView × 4 (backlog, in-progress, done, on-hold)
│   ├── Column Header (name + task count)
│   └── TaskCard × N
│       ├── ID + Type prefix
│       ├── Title (truncated to fit)
│       ├── Priority indicator (colored)
│       └── Labels (styled)
├── TaskDetailPanel (right side or overlay when Enter pressed)
│   └── Full markdown content of selected task
├── StatusBar (bottom row)
│   └── Key binding hints
└── NewTaskDialog (modal overlay when 'n' pressed)
    └── Form: type, title, priority, labels
```

### Implementation Steps

1. **Create IBoardRenderer and BoardView**
   - `Render(Board board, NavigationState state)` method
   - Calculate column widths based on terminal width
   - Delegate to ColumnView for each column

2. **Create ColumnView**
   - Render column header with name and task count
   - Render list of TaskCards
   - Highlight selected task if this is the active column

3. **Create TaskCard**
   - Format: `#001 FEATURE: Short title [High] frontend, api`
   - Color the priority indicator
   - Truncate title to fit column width
   - Style labels distinctly

4. **Create TaskDetailPanel**
   - Show when user presses Enter on a task
   - Display full markdown content
   - Scroll support for long content

5. **Create StatusBar**
   - Show available keybindings: `←→ Column | ↑↓ Task | Enter View | n New | m Move | d Delete | q Quit`
   - Show current filter state if active

6. **Create NewTaskDialog**
   - Modal form for creating tasks
   - Fields: Type (dropdown/cycle), Title (text input), Priority (cycle), Labels (text input)
   - Submit creates task via ITaskService

### Risks & Considerations

- **Risk**: TUI framework choice heavily impacts rendering API — **Mitigation**: Task 001 decides framework; adapt rendering to its widget model
- **Risk**: Terminal size variability — **Mitigation**: Calculate widths dynamically, truncate gracefully
- **Risk**: Scope creep — many visual components — **Mitigation**: Start with BoardView + ColumnView + TaskCard (core), add detail panel and dialog as follow-up if needed
- **Per spec**: TUI is not unit tested — manual testing only. Focus on clean, readable code.

## Code References

### From specs.md — TUI Components
```
Tui/
├── BoardView.cs          # Overordnet board-layout (kolonner side om side)
├── ColumnView.cs         # Én kolonne med header + task-liste
├── TaskCard.cs           # Én task-rad (tittel, prioritet-farge, labels)
├── TaskDetailPanel.cs    # Utvidet visning av valgt task
├── StatusBar.cs          # Bunnen: keybindings, filter-status
├── NewTaskDialog.cs      # Opprett ny task
└── InputHandler.cs       # Tastatur-input → kommandoer
```

### From specs.md — Interfaces
```
| IBoardRenderer | Tegn boardet i terminalen | BoardRenderer |
| IColumnView    | Rendrer én kolonne med tasks | ColumnView |
| ITaskCard      | Rendrer én task-rad | TaskCard |
```

### From specs.md — Features
```
- Live board view — columns side by side, tasks listed per column
- Keyboard navigation between columns and tasks
- Color-coded priority indicators
- Label display with distinct styling
- Task preview panel (show file contents when selected)
```

## Progress Log

- 2026-03-04 - Task created via backlog-scan

---

**Next Steps**: Implement after tasks 001, 002, 004, and 005 are complete. Move to `.task-board/in-progress/` when starting work.
