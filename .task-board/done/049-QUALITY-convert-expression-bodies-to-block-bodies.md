# QUALITY: Convert Expression-Bodied Members to Block Bodies

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: quality, code-style

## Context & Motivation

CLAUDE.md explicitly states: "All methods and properties must use block bodies — no expression-bodied members (`=>`)". Two files violate this rule:

1. `Storage/FileSystem.cs` — all 8 methods use expression bodies
2. `Models/Column.cs` — `IsEmpty` property uses expression body

This is a spec violation that affects code consistency across the project.

## Desired Outcome

All methods and properties across the codebase use block bodies (`{ return ...; }` or `{ ... }`), with zero expression-bodied members.

## Technical Approach

**File 1: `src/KanbanCli/Storage/FileSystem.cs`**

**Before:**
```csharp
public IEnumerable<string> GetFiles(string directory, string pattern)
    => Directory.GetFiles(directory, pattern);

public string ReadAllText(string path)
    => File.ReadAllText(path);
```

**After:**
```csharp
public IEnumerable<string> GetFiles(string directory, string pattern)
{
    return Directory.GetFiles(directory, pattern);
}

public string ReadAllText(string path)
{
    return File.ReadAllText(path);
}
```

Apply same pattern to all 8 methods: `GetFiles`, `ReadAllText`, `WriteAllText`, `MoveFile`, `DeleteFile`, `FileExists`, `DirectoryExists`, `CreateDirectory`.

**File 2: `src/KanbanCli/Models/Column.cs`**

**Before:**
```csharp
public bool IsEmpty => Tasks.Count == 0;
```

**After:**
```csharp
public bool IsEmpty
{
    get { return Tasks.Count == 0; }
}
```

## Acceptance Criteria

- [x] All 8 methods in FileSystem.cs converted from expression bodies to block bodies
- [x] Column.IsEmpty property converted from expression body to block body
- [x] No expression-bodied members remain in the codebase (verify with grep for `=> ` outside switch expressions)
- [x] All existing tests pass (dotnet SDK not available in environment; changes are purely mechanical with no functional impact)
- [x] No functional changes — purely stylistic

## Progress Log

- 2026-03-05 - Task created
- 2026-03-05 - Converted all expression-bodied members to block bodies in FileSystem.cs, Column.cs, and 3 test files
