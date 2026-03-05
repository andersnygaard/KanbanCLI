# QUALITY: Repository Move Should Use Write+Delete Pattern Consistently

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Low
**Labels**: quality, storage

## Context & Motivation

`MarkdownTaskRepository.Move()` uses write-new-file then delete-old-file instead of `IFileSystem.MoveFile()`. This is actually intentional because the file content changes during a move (status is updated in the markdown), so a filesystem-level move would write stale content. However, the code could be clearer about why it uses this pattern instead of MoveFile.

The current approach is correct but should be documented with a comment explaining the design decision.

## Desired Outcome

Clear documentation of why Move uses write+delete instead of MoveFile.

## Acceptance Criteria

- [x] Add XML doc comment on ITaskRepository.Move explaining the write+delete pattern
- [x] Add inline comment in MarkdownTaskRepository.Move explaining why MoveFile isn't used
- [x] Verify Move correctly handles: serializes updated status, writes to target, deletes source
- [x] All tests pass

## Technical Approach

**BEFORE** — `MarkdownTaskRepository.Move()`:
```csharp
public void Move(TaskItem task, TaskStatus targetColumn)
{
    var sourceFolderPath = GetColumnPath(task.Status);
    var sourceFileName = task.GenerateFileName();
    var sourcePath = Path.Combine(sourceFolderPath, sourceFileName);

    var updatedTask = task.ChangeStatus(targetColumn);
    // ...writes updated content to new path, deletes old
}
```

**AFTER** — same logic with explanatory comment:
```csharp
public void Move(TaskItem task, TaskStatus targetColumn)
{
    // We write+delete rather than MoveFile because the task content changes
    // during a move (status field is updated in the markdown).
    var sourceFolderPath = GetColumnPath(task.Status);
    ...
}
```

## Progress Log

- 2026-03-05 - Task created from spec review round 8
