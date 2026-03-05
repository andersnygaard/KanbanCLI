# QUALITY: Review Task File Format Against Spec and Fix Discrepancies

**Status**: In Progress
**Created**: 2026-03-05
**Priority**: High
**Labels**: quality, storage, spec-gap

## Context & Motivation

Careful comparison of the spec's task file format (.docs/specs.md lines 47-93) against what our serializer actually produces reveals several discrepancies. Newly created tasks are empty shells with no sections, CompletedDate is tracked but never serialized, and there's a subtle bug in the Move operation where ChangeStatus is called by both the service and repository causing the source path lookup to fail.

## Findings: Spec vs Implementation

### Issue 1: New tasks created via the app have NO default sections

**Spec says** a task file should have sections like "Context & Motivation", "Acceptance Criteria", "Progress Log". When creating a new task through the TUI, `TaskService.CreateTask()` produces a TaskItem with empty `Sections = {}`.

**BEFORE** — what a newly created task file looks like on disk:
```markdown
# FEATURE: Add dark mode

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: frontend
```

**AFTER** — what the spec format expects (specs.md lines 47-93):
```markdown
# FEATURE: Add dark mode

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: frontend

## Context & Motivation

(No description provided)

## Acceptance Criteria

- [ ] (To be defined)

## Progress Log

- 2026-03-05 - Task created
```

**Fix location**: `src/KanbanCli/Services/TaskService.cs` — `CreateTask()` method should populate default Sections dictionary.

### Issue 2: CompletedDate is tracked in the model but never serialized

`TaskItem.CompletedDate` (src/KanbanCli/Models/TaskItem.cs line 12) is set when moving to Done via `ChangeStatus()`, but `MarkdigMarkdownParser.Serialize()` (line 49-81) never writes it. The parser also never reads it back. This means CompletedDate is **lost on roundtrip**.

**BEFORE** — `MarkdigMarkdownParser.Serialize()` outputs:
```csharp
sb.AppendLine($"**Status**: {FormatStatus(task.Status)}");
sb.AppendLine($"**Created**: {task.CreatedDate?.ToString(BoardConstants.DateFormat)}");
sb.AppendLine($"**Priority**: {task.Priority}");
sb.AppendLine($"**Labels**: {labelsValue}");
// CompletedDate is NEVER written!
```

**AFTER** — should also include CompletedDate when present:
```csharp
sb.AppendLine($"**Status**: {FormatStatus(task.Status)}");
sb.AppendLine($"**Created**: {task.CreatedDate?.ToString(BoardConstants.DateFormat)}");
if (task.CompletedDate.HasValue)
    sb.AppendLine($"**Completed**: {task.CompletedDate.Value.ToString(BoardConstants.DateFormat)}");
sb.AppendLine($"**Priority**: {task.Priority}");
sb.AppendLine($"**Labels**: {labelsValue}");
```

**Parser fix** — in `Parse()` method, add after parsing CreatedDate:
```csharp
var completedDate = ParseCreatedDate(metadata.GetValueOrDefault("Completed"));
```

And add `"Completed"` to `KnownMetadataKeys` (line 16-19) so it's not treated as ExtraMetadata.

### Issue 3: Double ChangeStatus in Move — source path bug

`TaskService.MoveTask()` (src/KanbanCli/Services/TaskService.cs line 45-49) calls `task.ChangeStatus(targetColumn)` then passes the UPDATED task to `_repository.Move()`. But `MarkdownTaskRepository.Move()` (line 77-97) gets a task whose Status is ALREADY the target column, then calls `GetColumnPath(task.Status)` to get the SOURCE folder path — which incorrectly points to the TARGET folder.

The code appears to work by accident because the repository ALSO calls `ChangeStatus()` again on line 83, but by then `task.Status` is already the target status.

**BEFORE** — `TaskService.MoveTask()`:
```csharp
public void MoveTask(TaskItem task, TaskStatus targetColumn)
{
    var updatedTask = task.ChangeStatus(targetColumn);
    _repository.Move(updatedTask, targetColumn);
    // BUG: updatedTask.Status == targetColumn, so source path = target path
}
```

**AFTER** — pass original task, let repository handle status change:
```csharp
public void MoveTask(TaskItem task, TaskStatus targetColumn)
{
    _repository.Move(task, targetColumn);
    // Repository already calls ChangeStatus internally (line 83)
}
```

## Acceptance Criteria

- [x] Fix TaskService.MoveTask to pass original task to repository (remove ChangeStatus from service)
- [x] Verify repository.Move correctly computes source path from original status
- [x] Add default template sections when creating a new task (Context & Motivation, Acceptance Criteria, Progress Log)
- [x] Serialize CompletedDate as `**Completed**` metadata field when present
- [x] Parse `**Completed**` metadata field back into CompletedDate
- [x] Add "Completed" to KnownMetadataKeys set
- [x] Add test: newly created task has default sections in serialized output
- [x] Add test: serialize/parse roundtrip preserves CompletedDate
- [x] Add test: MoveTask correctly resolves source and target folder paths

## Technical Approach

1. **TaskService.CreateTask()**: Build default Sections dictionary with "Context & Motivation", "Acceptance Criteria", and "Progress Log" entries
2. **MarkdigMarkdownParser.Serialize()**: Add CompletedDate line after Created when value exists
3. **MarkdigMarkdownParser.Parse()**: Read "Completed" metadata and set on TaskItem
4. **MarkdigMarkdownParser.KnownMetadataKeys**: Add "Completed"
5. **TaskService.MoveTask()**: Remove ChangeStatus call, pass original task to repository

## Progress Log

- 2026-03-05 - Task created from backlog scan round 7
- 2026-03-05 - Detailed analysis with before/after code examples
