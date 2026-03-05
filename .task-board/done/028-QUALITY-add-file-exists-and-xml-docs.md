# QUALITY: Add FileExists to IFileSystem and XML Doc Comments

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Low
**Labels**: quality, storage

## Context & Motivation

IFileSystem interface (src/KanbanCli/Storage/IFileSystem.cs) is missing a `FileExists()` method, forcing callers to use `GetFiles()` with patterns to check file existence — which is both slow and unclear. Also, several public APIs lack XML doc comments, hurting IntelliSense and developer experience.

## Desired Outcome

Complete IFileSystem abstraction with FileExists, and consistent XML documentation on public API surfaces.

## Acceptance Criteria

- [x] Add FileExists(string path) to IFileSystem interface
- [x] Implement FileExists in FileSystem class wrapping File.Exists
- [x] Add XML doc comments to all IFileSystem interface methods
- [x] Add XML doc comments to TuiHelpers public methods
- [x] Add XML doc comments to FilterCriteria record
- [x] All tests pass

## Technical Approach

### IFileSystem — BEFORE:
```csharp
// src/KanbanCli/Storage/IFileSystem.cs
public interface IFileSystem
{
    IEnumerable<string> GetFiles(string directory, string pattern);
    string ReadAllText(string path);
    void WriteAllText(string path, string content);
    void MoveFile(string source, string destination);
    void DeleteFile(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
}
```

### IFileSystem — AFTER:
```csharp
// src/KanbanCli/Storage/IFileSystem.cs
public interface IFileSystem
{
    /// <summary>Returns all files in <paramref name="directory"/> matching <paramref name="pattern"/>.</summary>
    IEnumerable<string> GetFiles(string directory, string pattern);

    /// <summary>Reads the entire contents of the file at <paramref name="path"/>.</summary>
    string ReadAllText(string path);

    /// <summary>Writes <paramref name="content"/> to the file at <paramref name="path"/>, overwriting if it exists.</summary>
    void WriteAllText(string path, string content);

    /// <summary>Moves a file from <paramref name="source"/> to <paramref name="destination"/>.</summary>
    void MoveFile(string source, string destination);

    /// <summary>Deletes the file at <paramref name="path"/>.</summary>
    void DeleteFile(string path);

    /// <summary>Returns true if a file exists at <paramref name="path"/>.</summary>
    bool FileExists(string path);

    /// <summary>Returns true if the directory at <paramref name="path"/> exists.</summary>
    bool DirectoryExists(string path);

    /// <summary>Creates the directory at <paramref name="path"/>, including any parent directories.</summary>
    void CreateDirectory(string path);
}
```

### FileSystem — add implementation:
```csharp
public bool FileExists(string path) => File.Exists(path);
```

### FilterCriteria — BEFORE:
```csharp
public record FilterCriteria(TaskType? Type = null, Priority? Priority = null, string? Label = null);
```

### FilterCriteria — AFTER:
```csharp
/// <summary>
/// Criteria for filtering tasks on the board. All null values mean "match any".
/// </summary>
/// <param name="Type">Filter by task type, or null for all types.</param>
/// <param name="Priority">Filter by priority level, or null for all priorities.</param>
/// <param name="Label">Filter by label name (case-insensitive), or null for all labels.</param>
public record FilterCriteria(TaskType? Type = null, Priority? Priority = null, string? Label = null);
```

## Progress Log

- 2026-03-05 - Task created from backlog scan round 7
- 2026-03-05 - Added before/after code examples
