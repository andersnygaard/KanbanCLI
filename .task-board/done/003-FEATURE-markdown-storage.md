# FEATURE: Markdown Storage Layer

**Status**: Done
**Created**: 2026-03-04
**Priority**: High
**Labels**: storage, markdown, core
**Estimated Effort**: Medium - 2-3 days

## Context & Motivation

The Storage layer is the bridge between domain models and the file system. It parses markdown task files into `TaskItem` records and serializes them back. This is the second infrastructure layer (after Models) and must be solid — every read/write operation flows through it.

The spec emphasizes robust Markdig-based AST parsing, roundtrip safety (parse then serialize preserves content), and a clean `IFileSystem` abstraction for testability.

## Current State

No storage code exists. The `.task-board/` directory structure is already in place with the correct folders (backlog/, in-progress/, done/, on-hold/).

## Desired Outcome

Complete Storage layer with interfaces, implementations, and full test coverage. Can read, write, move, and delete task markdown files. Markdown parsing is roundtrip-safe.

## Affected Components

### Files to Create
- `src/KanbanCli/Storage/IMarkdownParser.cs`
- `src/KanbanCli/Storage/MarkdigMarkdownParser.cs`
- `src/KanbanCli/Storage/ITaskRepository.cs`
- `src/KanbanCli/Storage/MarkdownTaskRepository.cs`
- `src/KanbanCli/Storage/IFileSystem.cs`
- `src/KanbanCli/Storage/FileSystem.cs`
- `src/KanbanCli.Tests/Storage/MarkdownParserTests.cs`
- `src/KanbanCli.Tests/Storage/TaskRepositoryTests.cs`

### Dependencies
- **External**: Markdig NuGet package (added in task 001)
- **Internal**: Models layer (TaskItem, Enums) from task 002
- **Blocking**: Task 001 (scaffolding) and Task 002 (domain models)

## Technical Approach

### Architecture Decisions

- **Markdig AST parsing**: Use Markdig's abstract syntax tree, not regex, for robust markdown parsing
- **IFileSystem abstraction**: All file I/O goes through interface — tests use mocks, production uses real filesystem
- **Separation of concerns**: `IMarkdownParser` handles markdown ↔ TaskItem conversion, `ITaskRepository` handles file operations and organization
- **Graceful defaults**: Missing Priority defaults to Medium, missing Labels defaults to empty list

### Interfaces

```csharp
public interface IMarkdownParser
{
    TaskItem Parse(string markdown, int id, TaskType type);
    string Serialize(TaskItem task);
    (int Id, TaskType Type, string Description) ParseFileName(string fileName);
}

public interface ITaskRepository
{
    IReadOnlyList<TaskItem> GetAllByColumn(TaskStatus column);
    IReadOnlyList<TaskItem> GetAll();
    void Save(TaskItem task);
    void Move(TaskItem task, TaskStatus targetColumn);
    void Delete(TaskItem task);
    int GetNextId();
}

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

### Implementation Steps

1. **Create IFileSystem and FileSystem**
   - Thin wrapper around System.IO
   - All file operations go through this interface

2. **Create IMarkdownParser and MarkdigMarkdownParser**
   - Parse metadata block (Status, Priority, Labels, Created)
   - Parse body sections via Markdig AST (heading detection)
   - Serialize back to markdown with consistent formatting
   - ParseFileName using regex: `(\d+)-([\w]+)-(.+)\.md`

3. **Create ITaskRepository and MarkdownTaskRepository**
   - Maps TaskStatus enum to folder names (Backlog → "backlog/", etc.)
   - GetAllByColumn: list files in folder, parse each
   - Save: serialize and write to correct folder
   - Move: move file between folders, update status in content
   - Delete: remove file
   - GetNextId: scan all folders for highest number + 1

4. **Create MarkdownParserTests** following spec:
   - `Parse_FullTaskFile_ReturnsCorrectTaskItem`
   - `Parse_MinimalFile_DefaultsMissingFields`
   - `Parse_AcceptanceCriteria_ParsesCheckboxes`
   - `Parse_UnknownType_HandlesGracefully`
   - `Serialize_TaskItem_ProducesValidMarkdown`
   - `Roundtrip_ParseThenSerialize_PreservesContent`
   - `ParseFileName_ExtractsIdTypeAndDescription`

5. **Create TaskRepositoryTests** following spec:
   - `GetAllByColumn_BacklogWithTasks_ReturnsParsedItems`
   - `GetAllByColumn_EmptyFolder_ReturnsEmpty`
   - `Save_NewTask_CreatesFileWithCorrectName`
   - `Move_BacklogToInProgress_MovesFileAndUpdatesStatus`
   - `Delete_ExistingTask_RemovesFile`
   - `GetNextId_WithExistingTasks_ReturnsIncrementedId`

### Risks & Considerations

- **Risk**: Markdig API complexity — **Mitigation**: Start with simple heading/paragraph detection, iterate
- **Risk**: Roundtrip fidelity — markdown formatting may drift — **Mitigation**: Roundtrip test catches this early
- **Risk**: File path handling across OS — **Mitigation**: Use `Path.Combine` consistently
- **Readability**: Parser code can get dense — keep methods small, one section per parse method

## Progress Log

- 2026-03-04 - Task created via backlog-scan
- 2026-03-04 - Implementation complete: all 8 files created, 29 tests passing

## Acceptance Criteria

- [x] `IMarkdownParser` interface defined
- [x] `MarkdigMarkdownParser` implementation using Markdig library
- [x] Parses metadata header: Status, Priority, Labels, Created date
- [x] Parses sections: Context & Motivation, Desired Outcome, Acceptance Criteria, Technical Approach, Progress Log
- [x] Serializes `TaskItem` back to valid markdown preserving format
- [x] Roundtrip test: parse → serialize → parse produces identical TaskItem
- [x] `ITaskRepository` interface defined (GetAll, GetByColumn, Save, Move, Delete, GetNextId)
- [x] `MarkdownTaskRepository` implementation
- [x] `IFileSystem` abstraction for testability
- [x] `FileSystem` production implementation
- [x] Handles edge cases: missing fields default gracefully, unknown format doesn't crash
- [x] `ParseFileName` extracts Id, Type, and Description from filename
- [x] Full test suite with mocked file system
- [x] Tests use FluentAssertions and NSubstitute
- [x] Test naming follows `MethodName_Scenario_ExpectedResult` convention

---

**Next Steps**: Implement after tasks 001 and 002 are complete. Move to `.task-board/in-progress/` when starting work.
