# KanbanCli

A terminal-based (TUI) kanban board built with .NET, where tasks are stored as individual markdown files organized in folders.

**Created for AI timen episode 13, an internal training program at Bouvet.**

## What is this?

KanbanCli is an interactive terminal application for visualizing and managing a kanban board directly in the terminal. The board uses file-based storage where each task is a markdown file — human-readable and git-friendly.

## Project Structure

```
src/
├── KanbanCli/              # Main application
│   ├── Models/             # Domain models (Board, TaskItem, Column)
│   ├── Services/           # Business logic (BoardService, TaskService)
│   ├── Storage/            # File handling and markdown parsing
│   └── Tui/                # Terminal UI (views, dialogs, navigation)
└── KanbanCli.Tests/        # Unit tests (NUnit + FluentAssertions)
```

Tasks are stored in a `.task-board/` directory:

```
.task-board/
├── backlog/        # Tasks not yet started
├── in-progress/    # Tasks being worked on
├── done/           # Completed tasks
└── on-hold/        # Deferred/blocked tasks
```

## Tech Stack

- **Language:** C# / .NET 8
- **Markdown parsing:** Markdig
- **Testing:** NUnit, FluentAssertions, NSubstitute

## Getting Started

```bash
# Run the application
run.bat

# Run tests
dotnet test
```

## About

This project demonstrates how AI tools (Claude Code) can be used as a development assistant to:

- Plan and structure a code project
- Implement functionality iteratively via a task board
- Write tests and debug code
- Keep track of technical debt and backlog
