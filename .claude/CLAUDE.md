# Project: Kanban CLI — .NET C# TUI Application

## Tech Stack
- **Runtime:** .NET 8.0 / C#
- **TUI Framework:** TBD (e.g., Spectre.Console, Terminal.Gui)
- **Storage:** Markdown files (task-board system)
- **Markdown Parsing:** Markdig
- **Testing:** xUnit + FluentAssertions + NSubstitute
- **Package Manager:** NuGet (dotnet CLI)

## Project Structure
```
src/
├── KanbanCli/
│   ├── Program.cs                  # Entry point, DI setup
│   ├── Models/                     # Rich domain models (TaskItem, Board, Column, Enums)
│   ├── Storage/                    # Markdown parsing & file I/O
│   │   ├── ITaskRepository.cs
│   │   ├── MarkdownTaskRepository.cs
│   │   ├── IMarkdownParser.cs
│   │   └── MarkdigMarkdownParser.cs
│   ├── Services/                   # Business logic orchestration
│   │   ├── ITaskService.cs
│   │   ├── TaskService.cs
│   │   ├── IBoardService.cs
│   │   └── BoardService.cs
│   └── Tui/                        # Terminal UI components
│       ├── IBoardRenderer.cs
│       ├── BoardView.cs
│       ├── ColumnView.cs
│       ├── TaskCard.cs
│       ├── TaskDetailPanel.cs
│       ├── StatusBar.cs
│       └── InputHandler.cs
└── KanbanCli.Tests/
    ├── Models/                     # Domain logic tests
    ├── Storage/                    # Markdown parsing & repository tests
    └── Services/                   # Business logic tests
```

## C# Code Conventions
- **PascalCase** for public members and types
- **camelCase** for private/local variables and parameters
- Use `record` for immutable data types (models)
- Use `class` for services and stateful objects
- Enable nullable reference types — mark all reference types as nullable (`?`) or non-nullable
- Prefer **pattern matching** and **switch expressions** over traditional if/else
- **No `null` where possible** — use `nullable reference types` for explicit nullability
- Keep methods short and single-purpose — max ~30 lines
- Prefer **early returns** over nested conditionals
- Use **meaningful names** — avoid abbreviations

## Architectural Principles
- **Layered architecture:** Clear separation between TUI, Services, Models, and Storage
- **Dependency injection:** All layers depend on interfaces (contracts)
- **Interface-driven design:** Every major component has an interface (ITaskRepository, IBoardRenderer, etc.)
- **Rich domain models:** Business logic lives in models (ChangeStatus, AddLabel, MatchesFilter, etc.)
- **Testability first:** Mock dependencies via NSubstitute, test domain logic with FluentAssertions

## Testing
- **Framework:** xUnit
- **Assertions:** FluentAssertions (`result.Should().Be(...)`)
- **Mocking:** NSubstitute (for interfaces)
- **Test naming:** `MethodUnderTest_Scenario_ExpectedResult`
- **One assertion per test** (where it makes sense)
- **Arrange-Act-Assert:** Clear three-phase structure
- **What to test:**
  - Models: Pure domain logic (no dependencies)
  - Storage: Markdown roundtrip, file naming conventions
  - Services: Orchestration with mocked repositories
  - TUI: Manual testing only (not unit tested)

## Commands
- `dotnet build src/` — build solution
- `dotnet test src/` — run all tests
- `dotnet run --project src/KanbanCli/` — run application
- `dotnet add <project> package <name>` — install NuGet package
- `dotnet clean src/` — clean build artifacts

## Developers Notes
- Always use tools over bash for file operations (Read/Edit/Write/Glob/Grep)
- No git commands — development focuses on code, not VCS
- Read specs.md for architectural details and layer responsibilities
- Keep .task-board/ in sync — it's the source of truth for planning