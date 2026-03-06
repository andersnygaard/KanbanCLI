# FEATURE: TUI Input Handler & Keyboard Navigation

**Status**: Complete
**Created**: 2026-03-04
**Priority**: High
**Labels**: tui, input, core
**Estimated Effort**: Medium - 2-3 days

## Context & Motivation

The spec defines a fully interactive TUI with keyboard navigation. The Input Handler is the foundation of the TUI layer — it translates raw keyboard input into application commands. Without it, no user interaction is possible: no navigating columns, no moving tasks, no creating or deleting.

The spec defines a clear key binding table and an `IInputHandler` interface. This component sits at the boundary between user input and application logic, dispatching commands to `ITaskService` and `IBoardService`.

## Current State

No TUI code exists. The spec defines:
- `IInputHandler` / `KeyboardInputHandler`
- Key binding table (arrow keys, Enter, n, m, d, p, f, q)
- Command pattern: keyboard input maps to board operations

## Desired Outcome

A working input handler that maps keyboard events to typed commands, with board navigation state (selected column, selected task). Ready to be wired into the board rendering loop.

## Affected Components

### Files to Create
- `src/KanbanCli/Tui/IInputHandler.cs`
- `src/KanbanCli/Tui/InputHandler.cs`
- `src/KanbanCli/Tui/BoardCommand.cs` (command enum or type)
- `src/KanbanCli/Tui/NavigationState.cs` (selected column/task tracking)

### Dependencies
- **External**: TUI framework (Terminal.Gui or Spectre.Console — decided in task 001)
- **Internal**: ITaskService, IBoardService from task 004
- **Internal**: Models (TaskStatus for column mapping) from task 002
- **Blocking**: Task 001 (scaffolding), Task 004 (services)

## Technical Approach

### Architecture Decisions

- **Command pattern**: Input handler produces commands, not side effects. The main loop reads commands and dispatches to services. This keeps input testable and decoupled.
- **Navigation state as value type**: `NavigationState` record with SelectedColumn and SelectedTask indices — immutable, updated via `with` expressions.
- **Boundary handling**: Clamp indices (don't wrap) — more intuitive UX.

### Key Bindings (from spec)

| Key       | Command                    |
|-----------|----------------------------|
| `←` `→`  | SwitchColumn (Left/Right)  |
| `↑` `↓`  | NavigateTask (Up/Down)     |
| `Enter`   | ViewTaskDetails           |
| `n`       | NewTask                   |
| `m`       | MoveTask                  |
| `d`       | DeleteTask                |
| `p`       | ChangePriority            |
| `f`       | ToggleFilter              |
| `q`       | Quit                      |

### Implementation Steps

1. **Define BoardCommand enum**
   ```csharp
   public enum BoardCommand
   {
       None,
       MoveLeft, MoveRight, MoveUp, MoveDown,
       ViewDetails, NewTask, MoveTask,
       DeleteTask, ChangePriority, ToggleFilter,
       Quit
   }
   ```

2. **Define NavigationState record**
   ```csharp
   public record NavigationState
   {
       public int SelectedColumn { get; init; }
       public int SelectedTask { get; init; }

       public NavigationState MoveToColumn(int column, int maxColumns) => ...
       public NavigationState MoveToTask(int task, int maxTasks) => ...
   }
   ```

3. **Create IInputHandler and KeyboardInputHandler**
   ```csharp
   public interface IInputHandler
   {
       BoardCommand ReadCommand();
   }
   ```
   - Map `ConsoleKey` values to `BoardCommand` enum
   - Handle special keys (arrows) and regular keys (n, m, d, p, f, q)

4. **Wire into main loop** (conceptual — actual loop built in TUI rendering task)
   ```csharp
   while (true)
   {
       var command = inputHandler.ReadCommand();
       if (command == BoardCommand.Quit) break;
       // dispatch command...
   }
   ```

### Risks & Considerations

- **Risk**: TUI framework choice affects key reading API — **Mitigation**: Abstract behind IInputHandler so framework can be swapped
- **Risk**: Terminal.Gui vs Spectre.Console have very different input models — **Mitigation**: Decide framework in task 001, adapt input handler to its event model
- **Readability**: Keep the key mapping simple — a switch expression, not a dictionary of lambdas

## Code References

### From specs.md — Key Bindings
```
| Key       | Action                     |
|-----------|----------------------------|
| ← →      | Switch column              |
| ↑ ↓      | Navigate tasks             |
| Enter     | View/edit task details     |
| n         | New task (opens editor)    |
| m         | Move task to column        |
| d         | Delete task                |
| p         | Change priority            |
| f         | Filter by label/type       |
| q         | Quit                       |
```

### From specs.md — Interface
```
| IInputHandler | Håndterer tastatur-input | KeyboardInputHandler |
```

## Progress Log

- 2026-03-04 - Task created via backlog-scan

## Acceptance Criteria

- [x] `IInputHandler` interface defined
- [x] `KeyboardInputHandler` implementation using Console.ReadKey or TUI framework equivalent
- [x] Arrow key navigation: left/right switches column, up/down navigates tasks
- [x] Board navigation state: tracks selected column index and selected task index
- [x] `Enter` key: view/edit task details
- [x] `n` key: trigger new task creation flow
- [x] `m` key: trigger move task to column flow
- [x] `d` key: trigger delete task flow
- [x] `p` key: trigger change priority flow
- [x] `f` key: trigger filter by label/type flow
- [x] `q` key: quit application
- [x] Command pattern: input produces typed commands (enum or command objects)
- [x] Navigation wraps or clamps at boundaries (first/last column, first/last task)
- [x] Clean separation between input reading and command execution

---

**Next Steps**: Implement after tasks 001 and 004 are complete. Move to `.task-board/in-progress/` when starting work.
