# BUG: Add Buffered Output to Eliminate TUI Flicker

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: tui, performance

## Context & Motivation

The spec explicitly requires buffered output for the rendering pipeline: "All rendering writes to a BufferedStream (64 KB) around Console.Out. Hundreds of Console.Write and color calls batch to one flush -- eliminates flicker and reduces system calls dramatically."

Currently, every `Console.Write`, `Console.ForegroundColor`, and `Console.BackgroundColor` call in `BoardView`, `ColumnView`, `TaskCard`, `StatusBar`, and `DialogHelper` goes directly to the terminal. During a full board redraw this produces hundreds of individual write system calls, causing visible flicker during navigation (especially on slower terminals or over SSH).

## Current State

All TUI rendering writes directly to `Console.Out` with no buffering layer. Each keystroke triggers a full redraw with hundreds of unbuffered writes.

## Desired Outcome

A single `BufferedStream` wrapping `Console.Out` is set up in `KanbanApp.Run()` before the render loop. Each render cycle flushes once after all writes are complete, producing a single batch write to the terminal per frame.

## Technical Approach

### 1. Wrap Console.Out in a BufferedStream in KanbanApp.Run()

**File: `src/KanbanCli/Tui/KanbanApp.cs`**

Before:
```csharp
public void Run()
{
    _state = new NavigationState();
    _activeFilter = null;
    _running = true;

    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Console.Clear();

    while (_running)
    {
```

After:
```csharp
public void Run()
{
    _state = new NavigationState();
    _activeFilter = null;
    _running = true;

    Console.OutputEncoding = System.Text.Encoding.UTF8;

    var originalOut = Console.Out;
    var bufferedStream = new BufferedStream(Console.OpenStandardOutput(), BufferSize);
    var bufferedWriter = new StreamWriter(bufferedStream) { AutoFlush = false };
    Console.SetOut(bufferedWriter);

    Console.Clear();
    bufferedWriter.Flush();

    while (_running)
    {
```

Add a constant at the top of the class:
```csharp
private const int BufferSize = 65536; // 64 KB
```

### 2. Flush after each render cycle

**File: `src/KanbanCli/Tui/KanbanApp.cs`**

Before:
```csharp
        _boardRenderer.Render(_displayBoard, _state, filterInfo);

        var command = _inputHandler.ReadCommand();
```

After:
```csharp
        _boardRenderer.Render(_displayBoard, _state, filterInfo);
        Console.Out.Flush();

        var command = _inputHandler.ReadCommand();
```

### 3. Restore original Console.Out on exit

**File: `src/KanbanCli/Tui/KanbanApp.cs`**

Before:
```csharp
    Console.Clear();
    Console.CursorVisible = true;
    Console.WriteLine("Goodbye!");
}
```

After:
```csharp
    Console.SetOut(originalOut);
    bufferedWriter.Dispose();
    bufferedStream.Dispose();

    Console.Clear();
    Console.CursorVisible = true;
    Console.WriteLine("Goodbye!");
}
```

### 4. Flush before blocking Console.ReadLine/ReadKey in dialogs

Every dialog that calls `Console.ReadLine()` or `Console.ReadKey()` must flush the buffer first, otherwise the prompt text will not be visible when the user is expected to type. Add `Console.Out.Flush()` before each `Console.ReadLine()` and `Console.ReadKey()` call in:

- `DialogHelper.PromptTextInBox()`
- `DialogHelper.PromptNumericChoice()`
- `DialogHelper.PromptEnumInBox()`
- `DialogHelper.PromptEnum()`
- `NewTaskDialog.Show()`
- `MoveDialog.Show()`
- `FilterDialog.Show()`
- `ConfirmDialog.Confirm()`
- `TaskDetailPanel.Show()` (before the ReadKey in the loop)
- `TaskDetailPanel.HandleEditTitle()`
- `TaskDetailPanel.HandleEditLabels()`
- `TaskDetailPanel.HandleAddLabel()`
- `TaskDetailPanel.HandleRemoveLabel()`

Example pattern in `DialogHelper.PromptTextInBox()`:

Before:
```csharp
Console.ForegroundColor = Theme.DialogText;
var input = Console.ReadLine() ?? string.Empty;
```

After:
```csharp
Console.ForegroundColor = Theme.DialogText;
Console.Out.Flush();
var input = Console.ReadLine() ?? string.Empty;
```

## Dependencies

None -- this is a standalone performance improvement that does not change any rendering logic.

## Risks & Considerations

- Forgetting to flush before a ReadLine/ReadKey will cause the prompt to appear invisible (user sees nothing, but the app is waiting for input). Systematic search for all `Console.ReadLine()` and `Console.ReadKey()` calls is essential.
- The `try/finally` pattern around the run loop should ensure the original Console.Out is restored even if an exception occurs.

## Progress Log

- 2026-03-05 - Task created from backlog scan

## Acceptance Criteria

- [x] `KanbanApp.Run()` wraps `Console.Out` in a 64 KB `BufferedStream`
- [x] Buffer is flushed exactly once per render cycle (after `_boardRenderer.Render()`)
- [x] Buffer is flushed before every `Console.ReadLine()` and `Console.ReadKey()` call across all dialogs
- [x] Original `Console.Out` is restored on exit (in both normal exit and error paths)
- [x] Navigation (arrow keys) produces no visible flicker
- [x] All dialogs (new task, move, filter, delete, priority, detail panel) display prompts correctly before user input
