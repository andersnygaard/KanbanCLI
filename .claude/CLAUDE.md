# Project: AI Assistert Kode

A .NET terminal-based (TUI) kanban board where tasks are individual markdown files organized in folders (backlog/, in-progress/, done/, on-hold/).

# Architecture
- Layered: TUI layer → Services layer → Models layer → Storage layer
- Interface-driven with dependency injection (ITaskRepository, IBoardRenderer, etc.)
- Markdown parsed with Markdig; task metadata stored as frontmatter

# Tech Stack
- Language: C# / .NET
- TUI: Spectre.Console or Terminal.Gui (TBD)
- Testing: NUnit + FluentAssertions + NSubstitute
- Full spec in [.docs/specs.md](.docs/specs.md)

# Developers notes
- Always use tools over bash for simple tasks like copy, find etc.
- Prefer immutable records with `init` properties for domain models
- Readability-first; all conventions documented in specs.md