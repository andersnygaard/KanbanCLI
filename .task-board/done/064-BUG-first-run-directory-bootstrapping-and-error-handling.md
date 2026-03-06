# BUG: First-Run Directory Bootstrapping and Graceful Error Handling

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: storage, usability

## Context & Motivation

When the app is launched for the first time with no `.task-board/` directory, the experience is fragile. `BoardService.GetBoard()` calls `GetAllByColumn()` for each column status, which returns empty lists when folders do not exist (gracefully handled). However:

1. The board renders with 4 empty columns -- no guidance that this is a fresh board.
2. If the user creates a task, `MarkdownTaskRepository.Save()` calls `_fileSystem.CreateDirectory()` for the `backlog/` folder only. The other 3 column folders remain absent until a task is moved there.
3. The `PLANNING-BOARD.md` file is written by `BoardService.SavePlanningBoard()` after mutations, but the parent `.task-board/` directory might not exist yet on first run (depends on whether `boardPath` already exists).
4. There is no top-level error handling in `Program.cs` -- if the file system throws (permissions, disk full, corrupt files), the app crashes with an unhandled exception and a raw stack trace.

The spec describes a polished workflow: "Creating tasks: TUI prompts for type, title, priority, labels -> generates markdown file in backlog/." This implies the first-run experience should be smooth.

## Current State

`Program.cs` has no error handling and no directory bootstrapping:

```csharp
var boardPath = args.Length > 0 ? args[0] : ".task-board";

var fileSystem = new FileSystem();
var markdownParser = new MarkdigMarkdownParser();
var repository = new MarkdownTaskRepository(fileSystem, markdownParser, boardPath);
// ...
app.Run();
```

`MarkdownTaskRepository.Save()` creates individual column folders on demand, but does not ensure the root board directory or all column folders exist upfront.

## Desired Outcome

1. On first run, the app creates the board root directory and all 4 column folders if they do not exist.
2. A welcome message or visual indicator shows on an empty board.
3. `Program.cs` has top-level error handling that catches file system exceptions and displays a user-friendly error message instead of a stack trace.

## Technical Approach

### 1. Add directory bootstrapping in Program.cs

**File: `src/KanbanCli/Program.cs`**

Before:
```csharp
var boardPath = args.Length > 0 ? args[0] : ".task-board";

var fileSystem = new FileSystem();
var markdownParser = new MarkdigMarkdownParser();
var repository = new MarkdownTaskRepository(fileSystem, markdownParser, boardPath);
```

After:
```csharp
var boardPath = args.Length > 0 ? args[0] : ".task-board";

var fileSystem = new FileSystem();

// Ensure board directory and all column folders exist on startup
EnsureBoardDirectories(fileSystem, boardPath);

var markdownParser = new MarkdigMarkdownParser();
var repository = new MarkdownTaskRepository(fileSystem, markdownParser, boardPath);
```

Add a static method:
```csharp
static void EnsureBoardDirectories(IFileSystem fileSystem, string boardPath)
{
    if (!fileSystem.DirectoryExists(boardPath))
    {
        fileSystem.CreateDirectory(boardPath);
    }

    foreach (var folderName in BoardConstants.FolderNames.Values)
    {
        var columnPath = Path.Combine(boardPath, folderName);
        if (!fileSystem.DirectoryExists(columnPath))
        {
            fileSystem.CreateDirectory(columnPath);
        }
    }
}
```

### 2. Add top-level error handling in Program.cs

**File: `src/KanbanCli/Program.cs`**

Wrap the entire app setup and run in a try-catch:

Before:
```csharp
app.Run();
```

After:
```csharp
try
{
    app.Run();
}
catch (IOException ex)
{
    Console.ResetColor();
    Console.Error.WriteLine($"File system error: {ex.Message}");
    Environment.Exit(1);
}
catch (UnauthorizedAccessException ex)
{
    Console.ResetColor();
    Console.Error.WriteLine($"Permission denied: {ex.Message}");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.ResetColor();
    Console.Error.WriteLine($"Unexpected error: {ex.Message}");
    Environment.Exit(1);
}
```

### 3. Add tests for directory bootstrapping

**File: `src/KanbanCli.Tests/Storage/TaskRepositoryTests.cs`** (or new file)

Add tests verifying that `EnsureBoardDirectories` creates all expected folders:

```csharp
[Fact]
public void EnsureBoardDirectories_MissingRoot_CreatesAllFolders()
{
    var fileSystem = Substitute.For<IFileSystem>();
    fileSystem.DirectoryExists(Arg.Any<string>()).Returns(false);

    // Call the bootstrapping logic
    // Verify CreateDirectory called for root + 4 column folders
    fileSystem.Received(5).CreateDirectory(Arg.Any<string>());
}
```

## Dependencies

None.

## Risks & Considerations

- The bootstrapping logic should be idempotent -- `CreateDirectory` on an existing directory is a no-op for `Directory.CreateDirectory`, so this is safe.
- On systems with strict permissions, `CreateDirectory` may throw. The top-level error handler will catch this gracefully.

## Progress Log

- 2026-03-05 - Task created from backlog scan

## Acceptance Criteria

- [x] On first run with no `.task-board/` directory, the app creates the root directory and all 4 column folders (`backlog/`, `in-progress/`, `done/`, `on-hold/`)
- [x] On subsequent runs with existing directories, no errors occur and no duplicate creation attempts
- [x] If the file system throws an `IOException` or `UnauthorizedAccessException`, the user sees a clean error message (not a stack trace)
- [x] The app exits cleanly with a non-zero exit code on fatal file system errors
- [x] Console colors are reset before printing error messages (no garbled output)
- [x] Unit tests verify the bootstrapping logic creates all expected directories
