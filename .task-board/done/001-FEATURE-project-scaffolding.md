# FEATURE: .NET Solution Scaffolding

**Status**: Done
**Created**: 2026-03-04
**Priority**: High
**Labels**: infrastructure, setup
**Estimated Effort**: Simple - 1 day

## Context & Motivation

The project specification (.docs/specs.md) defines a .NET Kanban CLI application, but zero source code exists yet. Nothing can be built — models, storage, services, TUI — until the solution structure is in place. This is the absolute foundation.

Additionally, CLAUDE.md currently describes a React/TypeScript stack which contradicts the spec. This task includes aligning CLAUDE.md with the actual .NET/C# tech stack.

## Current State

- No `src/` folder exists
- No `.sln` or `.csproj` files
- No NuGet packages installed
- CLAUDE.md describes React 18 + Vite + TypeScript — mismatched with the .NET spec

## Desired Outcome

A working .NET solution with proper project structure, ready for feature development. CLAUDE.md updated to reflect the actual tech stack.

## Acceptance Criteria

- [x] `src/KanbanCli/KanbanCli.csproj` exists as a console application
- [x] `src/KanbanCli.Tests/KanbanCli.Tests.csproj` exists as an xUnit test project
- [x] Solution file `KanbanCli.sln` at repo root (or `src/`)
- [x] Folder structure in KanbanCli: `Models/`, `Storage/`, `Services/`, `Tui/`
- [x] Folder structure in KanbanCli.Tests: `Models/`, `Storage/`, `Services/`
- [x] NuGet references: Markdig (main project)
- [x] NuGet references: xUnit, FluentAssertions, NSubstitute (test project)
- [x] `dotnet build` succeeds with zero errors
- [x] `dotnet test` runs (even if no tests yet)
- [x] CLAUDE.md updated to reflect .NET/C# stack, conventions, and commands
- [x] Nullable reference types enabled
- [x] .gitignore includes standard .NET entries (bin/, obj/, etc.)

## Affected Components

### Files to Create
- `src/KanbanCli/KanbanCli.csproj`
- `src/KanbanCli/Program.cs` (minimal entry point)
- `src/KanbanCli.Tests/KanbanCli.Tests.csproj`
- `KanbanCli.sln`
- `.gitignore` (if not present)

### Files to Modify
- `.claude/CLAUDE.md` — update tech stack from React to .NET

### Dependencies
- **External**: .NET SDK (must be installed on machine)
- **Internal**: None — this is the foundation
- **Blocking**: Nothing — first task

## Technical Approach

### Implementation Steps

1. **Create solution and projects**
   ```bash
   dotnet new sln -n KanbanCli -o src
   dotnet new console -n KanbanCli -o src/KanbanCli
   dotnet new xunit -n KanbanCli.Tests -o src/KanbanCli.Tests
   dotnet sln src/KanbanCli.sln add src/KanbanCli src/KanbanCli.Tests
   dotnet add src/KanbanCli.Tests reference src/KanbanCli
   ```

2. **Add NuGet packages**
   ```bash
   dotnet add src/KanbanCli package Markdig
   dotnet add src/KanbanCli.Tests package FluentAssertions
   dotnet add src/KanbanCli.Tests package NSubstitute
   ```

3. **Create folder structure**
   - `src/KanbanCli/Models/`
   - `src/KanbanCli/Storage/`
   - `src/KanbanCli/Services/`
   - `src/KanbanCli/Tui/`
   - `src/KanbanCli.Tests/Models/`
   - `src/KanbanCli.Tests/Storage/`
   - `src/KanbanCli.Tests/Services/`

4. **Enable nullable reference types** in both `.csproj` files:
   ```xml
   <Nullable>enable</Nullable>
   ```

5. **Update CLAUDE.md** to reflect:
   - .NET/C# tech stack
   - C# coding conventions (PascalCase, records, pattern matching)
   - `dotnet build`, `dotnet test`, `dotnet run` commands
   - Project structure matching the spec

6. **Verify**: `dotnet build` and `dotnet test` both pass

### Risks & Considerations

- **Risk**: .NET SDK not installed — **Mitigation**: Check `dotnet --version` first
- **Risk**: CLAUDE.md update may break other skills that reference React — **Mitigation**: Review all skills for React-specific references

## Progress Log

- 2026-03-04 - Task created via backlog-scan
- 2026-03-04 - Implementation completed: Solution scaffolding with .NET 8, xUnit, FluentAssertions, NSubstitute, and Markdig. All acceptance criteria verified. Build and tests pass successfully.

---

**Completed**: Ready for next feature tasks. Solution structure is in place and ready for feature development.
