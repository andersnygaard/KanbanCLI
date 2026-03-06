# QUALITY: Add Input Validation to FileSystem and Repository

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, storage, defensive-coding

## Context & Motivation

The `FileSystem` class passes arguments directly to `File`/`Directory` APIs without defensive checks. While .NET APIs throw their own exceptions, adding explicit guards makes error messages clearer and catches issues at the boundary.

Also, `MarkdownTaskRepository.TryParseFile()` and `TryExtractId()` silently swallow exceptions with no feedback. Adding basic logging or structured returns would improve debuggability.

## Desired Outcome

- FileSystem methods validate inputs (null/empty checks)
- Repository parsing errors return structured results instead of silent swallowing

## Technical Approach

**File 1: `src/KanbanCli/Storage/FileSystem.cs`**

Add ArgumentException guards to methods with string parameters.

**Before:**
```csharp
public string ReadAllText(string path)
{
    return File.ReadAllText(path);
}
```

**After:**
```csharp
public string ReadAllText(string path)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    return File.ReadAllText(path);
}
```

Apply same pattern to: `GetFiles`, `ReadAllText`, `WriteAllText`, `MoveFile`, `DeleteFile`, `FileExists`, `DirectoryExists`, `CreateDirectory`.

**File 2: `src/KanbanCli/Storage/MarkdownTaskRepository.cs`**

No changes to existing behavior (still returns null on failure), but add a comment explaining why exceptions are swallowed. This is intentional: malformed files should not crash the board.

**Tests: `src/KanbanCli.Tests/Storage/FileSystemValidationTests.cs`** (new file)

Add tests verifying that FileSystem methods throw ArgumentException for null/empty paths:
```csharp
[Fact]
public void ReadAllText_NullPath_ThrowsArgumentException()
{
    var fileSystem = new FileSystem();
    var act = () => fileSystem.ReadAllText(null!);
    act.Should().Throw<ArgumentException>();
}
```

## Progress Log

- 2026-03-05 - Task created
- 2026-03-05 - Added ArgumentException.ThrowIfNullOrWhiteSpace guards to all FileSystem methods
- 2026-03-05 - Added explanatory comments to TryParseFile and TryExtractId
- 2026-03-05 - Created FileSystemValidationTests.cs with Theory/InlineData tests for all methods

## Acceptance Criteria

- [x] All FileSystem methods validate string parameters with ArgumentException.ThrowIfNullOrWhiteSpace
- [x] New test file FileSystemValidationTests.cs with tests for null/empty path inputs
- [x] Repository TryParseFile/TryExtractId have clear comments explaining silent swallowing rationale
- [ ] All existing tests pass
