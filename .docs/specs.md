# Kanban CLI — Specification

## Overview

A .NET interactive terminal (TUI) application for visualizing and managing a kanban board directly in the terminal. The board uses a file-based storage system where each task is an individual markdown file, organized into folders representing board columns.

## Tech Stack

- **Runtime:** .NET (C#)
- **TUI Framework:** TBD (e.g. Spectre.Console, Terminal.Gui)
- **Storage:** Markdown files (one file per task, folders as columns)

## Storage Format

Inspired by the `.task-board` system in `ettsted2`. Board data is stored as individual markdown files in column folders — human-readable and git-friendly.

### Directory Structure

```
.task-board/
├── README.md                # Board documentation & workflow guide
├── PLANNING-BOARD.md        # Current top 3-5 priorities overview
├── backlog/                 # Tasks not yet started
│   ├── 001-FEATURE-user-login.md
│   └── 002-BUG-crash-on-save.md
├── in-progress/             # Actively being worked on
│   └── 003-FEATURE-dark-mode.md
├── done/                    # Completed tasks
│   └── 004-BUG-header-overlap.md
└── on-hold/                 # Deferred/blocked tasks
```

### File Naming Convention

```
{number}-{TYPE}-{short-description}.md
```

- **Number:** Sequential ID (auto-incremented)
- **Type prefixes:** `FEATURE-`, `BUG-`, `REFACTOR-`, `TEST-`, `SECURITY-`, `PERF-`, `DESIGN-`, `A11Y-`, `QUALITY-`, `CLEANUP-`, `DOCS-`, `EPIC-`, `EXPLORE-`
- **Description:** Kebab-case summary

### Task File Format

Each task is a standalone markdown file with structured sections. The parser must handle both the core fields below and any additional metadata/sections — unknown fields are preserved for roundtrip safety.

```markdown
# TYPE: Short descriptive title

**Status**: Backlog | In Progress | Done | On Hold
**Created**: 2026-03-04
**Priority**: High | Medium | Low
**Labels**: frontend, api, database
**Estimated Effort**: Medium - 2-3 days          ← optional extra field

## Context & Motivation

Why this task exists and what problem it solves.

## Current State

What exists today (optional section).

## Desired Outcome

What success looks like.

## Acceptance Criteria

- [ ] First requirement
- [ ] Second requirement
- [ ] Third requirement

## Affected Components

Files, dependencies, blocking tasks (optional section).

## Technical Approach

Implementation details, affected files, code snippets.

## Risks & Considerations

Known risks and mitigations (optional section).

## Code References

Relevant snippets from existing code (optional section).

## Progress Log

- 2026-03-04 - Task created
```

**Important:** The parser must not hardcode section names. All `## Heading` sections are read dynamically and stored in `TaskItem.Sections`. This makes the format extensible without code changes.

### PLANNING-BOARD.md

A summary view showing the current top priorities and recently completed work:

```markdown
# Planning Board

**Current Focus**: Feature development

## Top Priorities

1. **#003** FEATURE: Dark mode - In Progress
2. **#005** BUG: Memory leak in parser - Backlog
3. **#006** FEATURE: Export to CSV - Backlog

## Recently Completed

- **#004** BUG: Header overlap fix - Done 2026-03-03
```

## Features

### Core

- **Columns as folders:** `backlog/`, `in-progress/`, `done/`, `on-hold/`
- **Tasks as files:** Create, edit, move, delete task files
- **Labels:** Metadata in task file header (`**Labels**: frontend, api`)
- **Priorities:** High / Medium / Low with color coding
- **Auto-numbering:** Sequential task IDs
- **Type prefixes:** Categorize tasks (FEATURE, BUG, REFACTOR, etc.)

### Interactive TUI

- Live board view — columns side by side, tasks listed per column
- Keyboard navigation between columns and tasks
- Move tasks between columns (moves the file between folders)
- Color-coded priority indicators
- Label display with distinct styling
- Task preview panel (show file contents when selected)
- PLANNING-BOARD.md auto-generation

### Key Bindings (draft)

| Key       | Action                     |
|-----------|----------------------------|
| `←` `→`  | Switch column              |
| `↑` `↓`  | Navigate tasks             |
| `Enter`   | View/edit task details    |
| `n`       | New task (opens editor)    |
| `m`       | Move task to column        |
| `d`       | Delete task                |
| `p`       | Change priority            |
| `f`       | Filter by label/type       |
| `q`       | Quit                       |

## Workflow

1. **Creating tasks:** TUI prompts for type, title, priority, labels → generates markdown file in `backlog/`
2. **Starting work:** Move task from `backlog/` → `in-progress/` (updates status in file)
3. **Completing work:** Move from `in-progress/` → `done/` (updates status, adds completion date)
4. **Deferring:** Move to `on-hold/`
5. **Planning board:** Auto-maintained summary of top priorities

## Technical Principles

### Code Readability First

Readability is the top priority. Code should be easy to understand for other developers (and your future self).

- **Descriptive names:** Classes, methods, and variables should have names that explain what they do — avoid abbreviations and cryptic names
- **Small, focused methods:** Each method does one thing. If a method needs a comment to be explained, it should be split up
- **Self-documenting code:** Code should read like prose. Comments only for *why*, not *what*
- **Consistent style:** One way of doing things throughout the project

### Architecture

- **Clear layering:** Separate TUI rendering, domain logic, and file system I/O
- **Interfaces everywhere:** All layers communicate via interfaces (`ITaskRepository`, `IBoardRenderer`, `IMarkdownParser`, etc.) for full testability and flexibility
- **Dependency injection:** Makes it easy to test and swap components
- **No magic strings:** Use enums/constants for type prefixes, statuses, column names

### Layer Overview

```
┌─────────────────────────────────────────────┐
│  TUI (Tui/)                                 │
│  Renders board, handles input               │
│  Depends on: ITaskService, IBoardService    │
├─────────────────────────────────────────────┤
│  Services (Services/)                       │
│  Orchestrates operations                    │
│  Depends on: ITaskRepository                │
├─────────────────────────────────────────────┤
│  Models (Models/)                           │
│  Rich domain models with own logic          │
│  No dependencies                            │
├─────────────────────────────────────────────┤
│  Storage (Storage/)                         │
│  Markdown parsing (Markdig), file system I/O│
│  Implements: ITaskRepository                │
└─────────────────────────────────────────────┘
```

### Interfaces

| Interface | Responsibility | Implementation |
|-----------|----------------|----------------|
| `ITaskRepository` | Read/write/move/delete task files | `MarkdownTaskRepository` |
| `IMarkdownParser` | Parse/serialize markdown ↔ TaskItem | `MarkdigMarkdownParser` |
| `IBoardRenderer` | Render the board in the terminal | `BoardRenderer` |
| `IColumnView` | Render a single column with tasks | `ColumnView` |
| `ITaskCard` | Render a single task row | `TaskCard` |
| `IInputHandler` | Handle keyboard input | `KeyboardInputHandler` |

### Domain Models (rich)

Models contain their own logic — not just data:

```csharp
public record TaskItem
{
    // Data
    public int Id { get; init; }
    public string Title { get; init; }
    public TaskType Type { get; init; }
    public Priority Priority { get; init; }
    public TaskStatus Status { get; init; }
    public IReadOnlyList<string> Labels { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? CompletedDate { get; init; }

    // Extra metadata (Estimated Effort, etc.) — preserved on roundtrip
    public IReadOnlyDictionary<string, string> ExtraMetadata { get; init; }

    // Body sections: key = heading, value = content
    // Preserves all sections (Context, Acceptance Criteria, Technical Approach, etc.)
    public IReadOnlyDictionary<string, string> Sections { get; init; }

    // Logic
    public TaskItem ChangeStatus(TaskStatus newStatus) => ...
    public TaskItem AddLabel(string label) => ...
    public TaskItem SetPriority(Priority priority) => ...
    public bool MatchesFilter(FilterCriteria filter) => ...
    public string GenerateFileName() => ...
}
```

- Records with `init` properties — immutable, but with `with` expressions for changes
- Logic that belongs to the data lives in the model (ChangeStatus, AddLabel, etc.)
- Complex orchestration (move file + update PLANNING-BOARD) lives in Services

### TUI Components

Component-based: small classes that each render their part of the screen.

```
Tui/
├── KanbanApp.cs              # Main loop: input → dispatch → render
├── BoardView.cs              # Overall board layout (columns side by side)
├── ColumnView.cs             # One column with header + task list + scroll
├── TaskCard.cs               # One task row (title, priority color, labels)
├── TaskDetailPanel.cs        # Expanded view of selected task (scrollable)
├── StatusBar.cs              # Bottom: keybindings, filter status
├── NavigationState.cs        # Immutable record: SelectedColumn + SelectedTask
├── Theme.cs                  # Centralized color palette — all TUI colors from here
├── TuiHelpers.cs             # Shared rendering utilities (SafeSetCursorPosition, etc.)
├── KeyboardInputHandler.cs   # Console.ReadKey → BoardCommand mapping
├── NewTaskDialog.cs          # Create new task
├── MoveDialog.cs             # Move task to another column
├── ConfirmDialog.cs          # Confirm deletion
├── PriorityDialog.cs         # Change priority
├── FilterDialog.cs           # Filter board by label/type/priority
├── DialogHelper.cs           # Shared dialog rendering (box borders, etc.)
└── MarkdownRenderer.cs       # Render markdown content in detail panel
```

### TUI Performance

The rendering loop in `KanbanApp` uses two techniques to avoid lag during navigation:

- **Board caching:** `BoardService.GetBoard()` (file system reads) is only called after mutations (create, move, delete, priority, details). Pure navigation (arrow keys) reuses the cached board — no disk I/O.
- **Buffered output:** All rendering is written to a `BufferedStream` (64 KB) around `Console.Out`. Hundreds of `Console.Write` and color calls are batched into a single flush — eliminates flicker and dramatically reduces system calls.
- **Full body clearing:** Columns fill their entire height with blank lines after the last task card. This prevents ghost content from previous screen renders (e.g., detail panel text "bleeding through" in empty columns).

### Markdown Parsing

Use the **Markdig** library for robust AST parsing of task files:

- Parse metadata header (Status, Priority, Labels, Created) from markdown
- Unknown metadata fields (e.g., `Estimated Effort`) are stored in `TaskItem.ExtraMetadata` — preserved on roundtrip
- Extract all `## Heading` sections dynamically via AST — stored in `TaskItem.Sections`
- Do not hardcode section names — new sections are handled automatically
- Serialize back to markdown on changes — preserve formatting and unknown fields
- Handle edge cases (missing fields, unknown format) gracefully

### Project Structure

```
src/
├── KanbanCli/
│   ├── Program.cs                  # Entry point, DI setup
│   ├── Models/
│   │   ├── TaskItem.cs             # Rich domain model for tasks
│   │   ├── Board.cs                # Board with columns
│   │   ├── Column.cs               # Column with tasks
│   │   └── Enums.cs                # TaskType, Priority, TaskStatus
│   ├── Storage/
│   │   ├── ITaskRepository.cs
│   │   ├── MarkdownTaskRepository.cs
│   │   ├── IMarkdownParser.cs
│   │   └── MarkdigMarkdownParser.cs
│   ├── Services/
│   │   ├── ITaskService.cs
│   │   ├── TaskService.cs
│   │   ├── IBoardService.cs
│   │   └── BoardService.cs
│   └── Tui/
│       ├── KanbanApp.cs
│       ├── IBoardRenderer.cs
│       ├── BoardView.cs
│       ├── IColumnView.cs
│       ├── ColumnView.cs
│       ├── ITaskCard.cs
│       ├── TaskCard.cs
│       ├── TaskDetailPanel.cs
│       ├── StatusBar.cs
│       ├── NavigationState.cs
│       ├── Theme.cs
│       ├── TuiHelpers.cs
│       ├── IInputHandler.cs
│       ├── KeyboardInputHandler.cs
│       ├── BoardCommand.cs
│       ├── NewTaskDialog.cs
│       ├── MoveDialog.cs
│       ├── ConfirmDialog.cs
│       ├── PriorityDialog.cs
│       ├── FilterDialog.cs
│       ├── DialogHelper.cs
│       └── MarkdownRenderer.cs
└── KanbanCli.Tests/
    ├── Models/
    │   └── TaskItemTests.cs
    ├── Storage/
    │   ├── MarkdownParserTests.cs
    │   └── TaskRepositoryTests.cs
    └── Services/
        └── TaskServiceTests.cs
```

### Code Conventions

- Follow standard C#/.NET conventions (PascalCase for public, camelCase for private)
- Use `nullable reference types` — avoid null where possible
- Prefer pattern matching and switch expressions
- Use `record` for data types, `class` for services with behavior
- Markdig for markdown parsing
- Test project with xUnit

## Test Strategy

### Frameworks

- **xUnit** — test runner
- **FluentAssertions** — readable assertions (`result.Should().Be(...)`)
- **NSubstitute** — mocking interfaces

### What is Tested Where

| Layer | What is tested | Mocking |
|-------|---------------|---------|
| **Models** | Domain logic: ChangeStatus, AddLabel, SetPriority, MatchesFilter | None — pure functions |
| **Storage** | Markdown parsing ↔ TaskItem roundtrip, filename conventions | File system mocked with `IFileSystem` |
| **Services** | Orchestration: create task, move between columns, update PLANNING-BOARD | `ITaskRepository` mocked |
| **TUI** | Not unit tested — manual testing | — |

### Test Conventions

- **Naming:** `MethodUnderTest_Scenario_ExpectedResult`
  ```
  ChangeStatus_ToDone_SetsCompletedDate
  Parse_MissingPriority_DefaultsToMedium
  MoveTask_ToInProgress_UpdatesStatusInFile
  ```
- **Arrange-Act-Assert:** Clear three-part structure in each test
- **One assertion per test** (where it makes sense) — makes error messages precise
- **Test data:** Use builder pattern or factory methods for creating TaskItems — avoid duplication

### Model Tests

Rich models mean lots of testable logic without dependencies:

```
TaskItemTests
├── ChangeStatus_ToDone_SetsCompletedDate
├── ChangeStatus_FromDoneToBacklog_ClearsCompletedDate
├── AddLabel_NewLabel_AppendsToList
├── AddLabel_DuplicateLabel_DoesNotDuplicate
├── SetPriority_High_UpdatesPriority
├── MatchesFilter_ByLabel_ReturnsTrue
├── MatchesFilter_ByType_ReturnsTrue
├── MatchesFilter_NoMatch_ReturnsFalse
└── FileName_GeneratesCorrectFormat
```

### Storage Tests

Verify that markdown parsing is robust and roundtrip-safe:

```
MarkdownParserTests
├── Parse_FullTaskFile_ReturnsCorrectTaskItem
├── Parse_MinimalFile_DefaultsMissingFields
├── Parse_AcceptanceCriteria_ParsesCheckboxes
├── Parse_UnknownType_HandlesGracefully
├── Serialize_TaskItem_ProducesValidMarkdown
├── Roundtrip_ParseThenSerialize_PreservesContent
└── ParseFileName_ExtractsIdTypeAndDescription

TaskRepositoryTests
├── GetAllByColumn_BacklogWithTasks_ReturnsParsedItems
├── GetAllByColumn_EmptyFolder_ReturnsEmpty
├── Save_NewTask_CreatesFileWithCorrectName
├── Move_BacklogToInProgress_MovesFileAndUpdatesStatus
├── Delete_ExistingTask_RemovesFile
└── GetNextId_WithExistingTasks_ReturnsIncrementedId
```

### Service Tests

Orchestration with mocked dependencies:

```
TaskServiceTests
├── CreateTask_ValidInput_SavesAndReturnsTask
├── CreateTask_AssignsNextAvailableId
├── MoveTask_UpdatesStatusAndMovesFile
├── MoveTask_ToDone_SetsCompletedDate
├── DeleteTask_CallsRepositoryDelete
└── GetBoard_ReturnsAllColumnsWithTasks

BoardServiceTests
├── GeneratePlanningBoard_TopPriorities_FormatsCorrectly
├── GeneratePlanningBoard_RecentlyCompleted_IncludesDone
└── GeneratePlanningBoard_EmptyBoard_ShowsEmptyMessage
```

### File System Abstraction

To avoid reading/writing real files in tests:

```csharp
public interface IFileSystem
{
    IEnumerable<string> GetFiles(string directory, string pattern);
    string ReadAllText(string path);
    void WriteAllText(string path, string content);
    void MoveFile(string source, string destination);
    void DeleteFile(string path);
    bool DirectoryExists(string path);
}
```

Production uses `FileSystem : IFileSystem`, tests use mock/in-memory.

## Non-Goals (for now)

- Multi-user / collaboration
- Cloud sync
- Due dates and reminders
- CLI subcommand mode (may add later)
