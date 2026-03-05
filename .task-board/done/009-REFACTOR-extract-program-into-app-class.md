# REFACTOR: Extract Program.cs into Structured Application Class

**Status**: Backlog
**Created**: 2026-03-04
**Priority**: Medium
**Labels**: architecture, refactor, tui
**Estimated Effort**: Medium - 2-3 days

## Context & Motivation

`Program.cs` is currently a 230-line top-level statements file containing the main loop, DI wiring, and three inline helper methods (`PromptMoveTarget`, `ConfirmDelete`, `PromptPriority`). The spec's architecture principle states:

> "Dependency injection: Gjor det enkelt a teste og bytte ut komponenter"

The current structure violates this by tightly coupling all components without a DI container or even a central application class. Additionally, there is code duplication:

- `GetPriorityColor()` is duplicated in `TaskCard.cs` and `TaskDetailPanel.cs`
- `FormatStatus()` is duplicated in `BoardService.cs` and `TaskDetailPanel.cs`
- `RenderHeader()` is duplicated across `BoardView.cs`, `TaskDetailPanel.cs`, and `NewTaskDialog.cs`

The helper methods in Program.cs (PromptMoveTarget, ConfirmDelete, PromptPriority) are UI concerns that belong in the TUI layer, not in the entry point.

## Current State

- `Program.cs` is 230 lines of top-level statements
- No DI container or application class
- Helper functions `PromptMoveTarget`, `ConfirmDelete`, `PromptPriority` are static methods in Program.cs
- `GetPriorityColor()` duplicated in `TaskCard.cs:110` and `TaskDetailPanel.cs:69`
- `FormatStatus()` duplicated in `BoardService.cs:116` and `TaskDetailPanel.cs:77`
- `RenderHeader()` pattern duplicated in `BoardView.cs:44`, `TaskDetailPanel.cs:39`, `NewTaskDialog.cs:45`
- `UnitTest1.cs` exists as an empty test placeholder from scaffolding

## Desired Outcome

A clean, testable application structure where `Program.cs` is minimal (DI setup + run), the main loop lives in a dedicated `KanbanApp` class, helper prompts are proper TUI components, and shared rendering utilities are consolidated.

## Acceptance Criteria

- [x] `Program.cs` reduced to DI wiring and `app.Run()` call (under 20 lines)
- [x] New `KanbanApp` class (or `BoardController`) encapsulates the main loop and command dispatch
- [x] `PromptMoveTarget` extracted to a TUI component (e.g., `MoveDialog.cs`)
- [x] `ConfirmDelete` extracted to a TUI component (e.g., `ConfirmDialog.cs`)
- [x] `PromptPriority` extracted to a TUI component (e.g., `PriorityDialog.cs`)
- [x] `GetPriorityColor` consolidated into a shared `TuiConstants.cs` or `ColorScheme.cs`
- [x] `FormatStatus` consolidated into a shared helper (models or a shared utility)
- [x] Duplicated `RenderHeader` pattern consolidated
- [x] `UnitTest1.cs` placeholder removed
- [x] All existing tests pass
- [x] Application behavior unchanged

## Affected Components

### Files to Create
- `src/KanbanCli/Tui/KanbanApp.cs` -- main loop and command dispatch
- `src/KanbanCli/Tui/MoveDialog.cs` -- extracted from PromptMoveTarget
- `src/KanbanCli/Tui/ConfirmDialog.cs` -- extracted from ConfirmDelete
- `src/KanbanCli/Tui/PriorityDialog.cs` -- extracted from PromptPriority
- `src/KanbanCli/Tui/TuiHelpers.cs` -- shared GetPriorityColor, FormatStatus, RenderHeader

### Files to Modify
- `src/KanbanCli/Program.cs` -- reduce to DI setup + app.Run()
- `src/KanbanCli/Tui/TaskCard.cs` -- use shared GetPriorityColor
- `src/KanbanCli/Tui/TaskDetailPanel.cs` -- use shared helpers
- `src/KanbanCli/Tui/NewTaskDialog.cs` -- use shared RenderHeader
- `src/KanbanCli/Services/BoardService.cs` -- use shared FormatStatus

### Files to Delete
- `src/KanbanCli.Tests/UnitTest1.cs` -- empty placeholder test

### Dependencies
- **External**: None
- **Internal**: All existing TUI and Service components
- **Blocking**: None -- pure refactor of existing code

## Technical Approach

### Architecture Decisions

- **KanbanApp class**: Receives all dependencies via constructor, runs the main loop. This makes the application testable and structured.
- **Dialog classes**: Each prompt interaction becomes its own small class, consistent with the spec's component-based TUI architecture
- **Shared helpers**: Static utility class for cross-cutting TUI concerns (colors, formatting)
- **No DI container yet**: Keep manual DI in Program.cs for now -- a container can be added later if needed

### Implementation Steps

1. **Create TuiHelpers.cs**
   ```csharp
   public static class TuiHelpers
   {
       public static ConsoleColor GetPriorityColor(Priority priority) => ...
       public static string FormatStatus(TaskStatus status) => ...
       public static void RenderHeader(string title, int width, ConsoleColor bg) => ...
   }
   ```

2. **Extract dialog classes**
   - `MoveDialog.cs`: Take `Board` and `currentColumnIndex`, return `TaskStatus?`
   - `ConfirmDialog.cs`: Take `TaskItem`, return `bool`
   - `PriorityDialog.cs`: Take `Priority current`, return `Priority`

3. **Create KanbanApp.cs**
   - Constructor: `KanbanApp(ITaskService, IBoardService, IInputHandler, IBoardRenderer, TaskDetailPanel, NewTaskDialog, MoveDialog, ConfirmDialog, PriorityDialog)`
   - Method: `void Run()` containing the main loop from Program.cs
   - All command dispatch moved from Program.cs switch statement

4. **Simplify Program.cs**
   ```csharp
   var app = new KanbanApp(taskService, boardService, inputHandler, boardRenderer, ...);
   app.Run();
   ```

5. **Update existing classes** to use TuiHelpers instead of duplicated methods

6. **Delete UnitTest1.cs**

### Risks & Considerations

- **Risk**: Behavior regression during refactor -- **Mitigation**: Run all tests after each step; manual smoke test the TUI
- **Risk**: Too many constructor parameters on KanbanApp -- **Mitigation**: Accept this for now; if it grows further, introduce a DI container
- **Readability**: Each file should stay under 50 lines for dialog classes; KanbanApp under 100 lines

## Code References

### Duplicated GetPriorityColor
- `src/KanbanCli/Tui/TaskCard.cs:110-116`
- `src/KanbanCli/Tui/TaskDetailPanel.cs:69-75`

### Duplicated FormatStatus
- `src/KanbanCli/Services/BoardService.cs:116-123`
- `src/KanbanCli/Tui/TaskDetailPanel.cs:77-84`

### Program.cs helper functions to extract
- `PromptMoveTarget` at line 146-186
- `ConfirmDelete` at line 188-199
- `PromptPriority` at line 201-230

## Progress Log

- 2026-03-04 - Task created via backlog-scan