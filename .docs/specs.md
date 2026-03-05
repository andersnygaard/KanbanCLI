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
**Estimated Effort**: Medium - 2-3 days          ← valgfritt ekstra-felt

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

**Viktig:** Parseren skal ikke hardkode seksjonsnavn. Alle `## Heading`-seksjoner leses inn dynamisk og lagres i `TaskItem.Sections`. Dette gjør formatet utvidbart uten kodeendringer.

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

Lesbarhet er hovedprioritet. Koden skal være lett å forstå for andre utviklere (og fremtidig deg selv).

- **Beskrivende navn:** Klasser, metoder og variabler skal ha navn som forklarer hva de gjør — unngå forkortelser og kryptiske navn
- **Små, fokuserte metoder:** Hver metode gjør én ting. Hvis en metode trenger en kommentar for å forklares, bør den splittes opp
- **Selvdokumenterende kode:** Koden skal leses som prosa. Kommentarer kun for *hvorfor*, ikke *hva*
- **Konsistent stil:** Én måte å gjøre ting på gjennom hele prosjektet

### Arkitektur

- **Tydelig lagdeling:** Separer TUI-rendering, domenelogikk og filsystem-I/O
- **Interfaces overalt:** Alle lag kommuniserer via interfaces (`ITaskRepository`, `IBoardRenderer`, `IMarkdownParser` etc.) for full testbarhet og fleksibilitet
- **Dependency injection:** Gjør det enkelt å teste og bytte ut komponenter
- **Ingen magiske strenger:** Bruk enums/constants for type-prefikser, statuser, kolonnenavn

### Lag-oversikt

```
┌─────────────────────────────────────────────┐
│  TUI (Tui/)                                 │
│  Rendrer board, håndterer input             │
│  Avhenger av: ITaskService, IBoardService   │
├─────────────────────────────────────────────┤
│  Services (Services/)                       │
│  Orkestrerer operasjoner                    │
│  Avhenger av: ITaskRepository               │
├─────────────────────────────────────────────┤
│  Models (Models/)                           │
│  Rike domenemodeller med egen logikk        │
│  Ingen avhengigheter                        │
├─────────────────────────────────────────────┤
│  Storage (Storage/)                         │
│  Markdown-parsing (Markdig), filsystem-I/O  │
│  Implementerer: ITaskRepository             │
└─────────────────────────────────────────────┘
```

### Interfaces

| Interface | Ansvar | Implementasjon |
|-----------|--------|----------------|
| `ITaskRepository` | Les/skriv/flytt/slett task-filer | `MarkdownTaskRepository` |
| `IMarkdownParser` | Parse/serialiser markdown ↔ TaskItem | `MarkdigMarkdownParser` |
| `IBoardRenderer` | Tegn boardet i terminalen | `BoardRenderer` |
| `IColumnView` | Rendrer én kolonne med tasks | `ColumnView` |
| `ITaskCard` | Rendrer én task-rad | `TaskCard` |
| `IInputHandler` | Håndterer tastatur-input | `KeyboardInputHandler` |

### Domenemodeller (rike)

Modellene inneholder egen logikk — ikke bare data:

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

    // Ekstra metadata (Estimated Effort, etc.) — bevares ved roundtrip
    public IReadOnlyDictionary<string, string> ExtraMetadata { get; init; }

    // Body-seksjoner: nøkkel = heading, verdi = innhold
    // Bevarer alle seksjoner (Context, Acceptance Criteria, Technical Approach, osv.)
    public IReadOnlyDictionary<string, string> Sections { get; init; }

    // Logikk
    public TaskItem ChangeStatus(TaskStatus newStatus) => ...
    public TaskItem AddLabel(string label) => ...
    public TaskItem SetPriority(Priority priority) => ...
    public bool MatchesFilter(FilterCriteria filter) => ...
    public string GenerateFileName() => ...
}
```

- Records med `init`-properties — immutable, men med `with`-expressions for endringer
- Logikk som hører til dataene lever i modellen (ChangeStatus, AddLabel, etc.)
- Kompleks orkestrering (flytt fil + oppdater PLANNING-BOARD) lever i Services

### TUI-komponenter

Komponent-basert: små klasser som hver rendrer sin del av skjermen.

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

### Markdown-parsing

Bruk **Markdig**-biblioteket for robust AST-parsing av task-filer:

- Parser metadata-header (Status, Priority, Labels, Created) fra markdown
- Ukjente metadata-felter (f.eks. `Estimated Effort`) lagres i `TaskItem.ExtraMetadata` — bevares ved roundtrip
- Ekstraher alle `## Heading`-seksjoner dynamisk via AST — lagres i `TaskItem.Sections`
- Ikke hardkode seksjonsnavn — nye seksjoner håndteres automatisk
- Serialiser tilbake til markdown ved endringer — bevar formatering og ukjente felter
- Håndter edge cases (manglende felter, ukjent format) gracefully

### Prosjektstruktur

```
src/
├── KanbanCli/
│   ├── Program.cs                  # Entry point, DI-oppsett
│   ├── Models/
│   │   ├── TaskItem.cs             # Rik domenemodell for tasks
│   │   ├── Board.cs                # Board med kolonner
│   │   ├── Column.cs               # Kolonne med tasks
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
│       ├── IBoardRenderer.cs
│       ├── BoardView.cs
│       ├── ColumnView.cs
│       ├── TaskCard.cs
│       ├── TaskDetailPanel.cs
│       ├── StatusBar.cs
│       └── InputHandler.cs
└── KanbanCli.Tests/
    ├── Models/
    │   └── TaskItemTests.cs
    ├── Storage/
    │   ├── MarkdownParserTests.cs
    │   └── TaskRepositoryTests.cs
    └── Services/
        └── TaskServiceTests.cs
```

### Kodekonvensjoner

- Følg standard C#/.NET-konvensjoner (PascalCase for public, camelCase for private)
- Bruk `nullable reference types` — unngå null der det er mulig
- Foretrekk pattern matching og switch expressions
- Bruk `record` for datatyper, `class` for tjenester med oppførsel
- Markdig for markdown-parsing
- Testprosjekt med xUnit

## Test-strategi

### Rammeverk

- **xUnit** — test-runner
- **FluentAssertions** — lesbare assertions (`result.Should().Be(...)`)
- **NSubstitute** — mocking av interfaces

### Hva testes hvor

| Lag | Hva testes | Mocking |
|-----|-----------|---------|
| **Models** | Domenelogikk: ChangeStatus, AddLabel, SetPriority, MatchesFilter | Ingen — pure funksjoner |
| **Storage** | Markdown-parsing ↔ TaskItem roundtrip, filnavn-konvensjoner | Filsystem mockes med `IFileSystem` |
| **Services** | Orkestrering: opprett task, flytt mellom kolonner, oppdater PLANNING-BOARD | `ITaskRepository` mockes |
| **TUI** | Ikke enhetstestet — manuell testing | — |

### Testkonvensjoner

- **Navngivning:** `MetodeSomTestes_Scenario_ForventetResultat`
  ```
  ChangeStatus_ToDone_SetsCompletedDate
  Parse_MissingPriority_DefaultsToMedium
  MoveTask_ToInProgress_UpdatesStatusInFile
  ```
- **Arrange-Act-Assert:** Tydelig tredeling i hver test
- **Én assertion per test** (der det gir mening) — gjør feilmeldinger presise
- **Testdata:** Bruk builder-pattern eller factory-metoder for å lage TaskItems — unngå duplisering

### Models-tester

Rike modeller betyr mye testbar logikk uten avhengigheter:

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

### Storage-tester

Verifiser at markdown-parsing er robust og roundtrip-safe:

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

### Service-tester

Orkestrering med mockede avhengigheter:

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

### Filsystem-abstraksjon

For å unngå å lese/skrive ekte filer i tester:

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

Produksjon bruker `FileSystem : IFileSystem`, tester bruker mock/in-memory.

## Non-Goals (for now)

- Multi-user / collaboration
- Cloud sync
- Due dates and reminders
- CLI subcommand mode (may add later)
