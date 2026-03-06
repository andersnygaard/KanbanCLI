# CLEANUP: Remove Dead Code and Add Board Width Tests

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: cleanup, tests

## Context & Motivation

Two cleanup items identified in the code audit:

1. **Dead code**: `MarkdigMarkdownParser.ParseTaskType()` (line 283) is a private method that is never called. The type parsing is done inline in `ParseFileName()` via `Enum.TryParse`. This dead code should be removed.

2. **Missing tests for board width**: The recently added `TuiHelpers.GetEffectiveWidth()` and `GetEffectiveHeight()` methods that cap the board width and support the `KANBAN_WIDTH` environment variable have no test coverage. These are pure logic methods (with injectable Console values) that should be tested.

## Desired Outcome

- No dead code in the parser
- Board width logic tested including env var override and clamping behavior

## Technical Approach

**Step 1: Remove dead method from `src/KanbanCli/Storage/MarkdigMarkdownParser.cs`**

**Before:**
```csharp
private static TaskType ParseTaskType(string value)
{
    if (Enum.TryParse<TaskType>(value, ignoreCase: true, out var result))
        return result;

    throw new FormatException($"Unknown task type: '{value}'");
}
```

**After:**
(method removed entirely)

**Step 2: Add tests in `src/KanbanCli.Tests/Models/BoardConstantsTests.cs`**

Add tests verifying:
- `GetEffectiveWidth()` clamps to `[MinWindowWidth, MaxBoardWidth]` range
- `KANBAN_WIDTH` environment variable overrides the width when set
- `KANBAN_WIDTH` values below MinWindowWidth are rejected (falls back to console)
- `GetEffectiveHeight()` returns at least MinWindowHeight

## Progress Log

- 2026-03-05 - Task created

## Acceptance Criteria

- [x] `ParseTaskType` method removed from MarkdigMarkdownParser.cs
- [x] No remaining dead/unused private methods in the parser
- [x] Test added: GetEffectiveWidth returns value within MinWindowWidth..MaxBoardWidth range
- [x] Test added: GetEffectiveWidth respects KANBAN_WIDTH environment variable
- [x] Test added: GetEffectiveHeight returns at least MinWindowHeight
- [ ] All existing tests pass
