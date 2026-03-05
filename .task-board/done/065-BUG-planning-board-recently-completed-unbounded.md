# BUG: Planning Board Recently Completed Section Is Unbounded

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: services, correctness

## Context & Motivation

The `BoardService.GeneratePlanningBoard()` method applies a `.Take(BoardConstants.MaxTopPriorities)` limit to the Top Priorities section, but the Recently Completed section has **no limit at all**. Every completed task ever is rendered into `PLANNING-BOARD.md`. As the project accumulates done tasks (currently 64 and growing), the planning board becomes an ever-expanding dump of completed work rather than a concise "recently completed" summary.

The spec explicitly calls this section "Recently Completed" and shows only a small number of items in its example. A planning board with hundreds of completed items defeats its purpose as a quick-glance summary.

Additionally, the Top Priorities logic only considers `InProgress` tasks and `High`-priority `Backlog` tasks. If there are fewer than `MaxTopPriorities` items in those two groups, the section could appear sparse even though there are `Medium`-priority backlog items that deserve visibility.

## Current State

`BoardService.BuildCompletedSection` renders every task with `TaskStatus.Done` into a bullet list, with no `.Take()` limit. The Top Priorities section only considers `InProgress` + `High`-priority `Backlog` items.

## Desired Outcome

- Recently Completed shows at most 5-10 recent items (configurable via `BoardConstants`).
- Top Priorities falls back to `Medium`-priority backlog tasks when there are fewer than `MaxTopPriorities` items from the `InProgress` + `High` pools.

## Technical Approach

### Change 1: Add constant for max recently completed items

**File:** `src/KanbanCli/Models/BoardConstants.cs`

Before:
```csharp
public const int MaxTopPriorities = 5;
```

After:
```csharp
public const int MaxTopPriorities = 5;

/// <summary>
/// Maximum number of recently completed items shown in the planning board.
/// </summary>
public const int MaxRecentlyCompleted = 10;
```

### Change 2: Limit recently completed in BoardService

**File:** `src/KanbanCli/Services/BoardService.cs`

Before:
```csharp
var recentlyCompleted = allTasks
    .Where(t => t.Status == TaskStatus.Done)
    .OrderByDescending(t => t.CompletedDate ?? DateTime.MinValue)
    .ToList();
```

After:
```csharp
var recentlyCompleted = allTasks
    .Where(t => t.Status == TaskStatus.Done)
    .OrderByDescending(t => t.CompletedDate ?? DateTime.MinValue)
    .Take(BoardConstants.MaxRecentlyCompleted)
    .ToList();
```

### Change 3: Broaden top priorities to include medium-priority backlog as fallback

**File:** `src/KanbanCli/Services/BoardService.cs`

Before:
```csharp
var topPriorities = allTasks
    .Where(t => t.Status == TaskStatus.InProgress)
    .Concat(allTasks.Where(t => t.Status == TaskStatus.Backlog && t.Priority == Priority.High))
    .Take(BoardConstants.MaxTopPriorities)
    .ToList();
```

After:
```csharp
var topPriorities = allTasks
    .Where(t => t.Status == TaskStatus.InProgress)
    .Concat(allTasks.Where(t => t.Status == TaskStatus.Backlog && t.Priority == Priority.High))
    .Concat(allTasks.Where(t => t.Status == TaskStatus.Backlog && t.Priority == Priority.Medium))
    .Take(BoardConstants.MaxTopPriorities)
    .ToList();
```

### Change 4: Add tests for the new limits

**File:** `src/KanbanCli.Tests/Services/BoardServiceTests.cs`

Add tests:
- `GeneratePlanningBoard_ManyCompletedTasks_LimitsToMaxRecentlyCompleted`
- `GeneratePlanningBoard_FewHighPriority_FallsBackToMediumPriority`

## Acceptance Criteria

- [x] Recently Completed section is limited to `BoardConstants.MaxRecentlyCompleted` items
- [x] `MaxRecentlyCompleted` constant is defined in `BoardConstants`
- [x] Top Priorities falls back to medium-priority backlog when high-priority pool is insufficient
- [x] Existing tests continue to pass
- [x] New tests verify the limit on recently completed items
- [x] New test verifies medium-priority fallback behavior

## Progress Log

- 2026-03-05 - Task created from backlog scan
