# BUG: CreatedDate Is Nullable Allowing Tasks Without Creation Timestamps

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: models, correctness, storage

## Context & Motivation

The spec defines `CreatedDate` as a non-nullable `DateTime` field (`public DateTime CreatedDate { get; init; }`), but the implementation declares it as `DateTime?` (nullable). This means tasks can be constructed or parsed without a creation date, leading to `(unknown)` displays in the detail panel and potential `null` propagation throughout the system.

The `MarkdigMarkdownParser.Parse` method returns `null` for `CreatedDate` when the `Created` field is missing or malformed, and `TaskService.CreateTask` always sets it to `DateTime.UtcNow`. However, the model itself does not enforce this invariant. Any code that constructs a `TaskItem` directly (including tests and future callers) can omit the date.

The `Serialize` method already has a latent bug: `task.CreatedDate?.ToString(BoardConstants.DateFormat)` produces a blank `**Created**:` line when `CreatedDate` is null, which then round-trips as a null date -- silently losing information.

## Current State

- `TaskItem.CreatedDate` is `DateTime?` (nullable)
- Parser returns `null` for malformed or missing dates
- `TaskDetailPanel` shows `(unknown)` when `CreatedDate` is null
- Serializer outputs `**Created**:` with empty value when null

## Desired Outcome

`CreatedDate` should be non-nullable `DateTime`, with a sensible default applied during parsing when the source file is missing the field. This enforces the spec's intent and prevents null propagation.

## Technical Approach

### Change 1: Make CreatedDate non-nullable in the model

**File:** `src/KanbanCli/Models/TaskItem.cs`

Before:
```csharp
public DateTime? CreatedDate { get; init; }
public DateTime? CompletedDate { get; init; }
```

After:
```csharp
public DateTime CreatedDate { get; init; }
public DateTime? CompletedDate { get; init; }
```

### Change 2: Parser provides a default when Created is missing

**File:** `src/KanbanCli/Storage/MarkdigMarkdownParser.cs`

Before:
```csharp
var createdDate = ParseCreatedDate(metadata.GetValueOrDefault("Created"));
```

After:
```csharp
var createdDate = ParseCreatedDate(metadata.GetValueOrDefault("Created")) ?? DateTime.UtcNow;
```

### Change 3: Remove null-conditional in Serialize

**File:** `src/KanbanCli/Storage/MarkdigMarkdownParser.cs`

Before:
```csharp
sb.AppendLine($"**Created**: {task.CreatedDate?.ToString(BoardConstants.DateFormat)}");
```

After:
```csharp
sb.AppendLine($"**Created**: {task.CreatedDate.ToString(BoardConstants.DateFormat)}");
```

### Change 4: Remove null-conditional in TaskDetailPanel

**File:** `src/KanbanCli/Tui/TaskDetailPanel.cs`

Before:
```csharp
lines.Add(ContentLine.Field("Created", task.CreatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "(unknown)", width, borderColor));
```

After:
```csharp
lines.Add(ContentLine.Field("Created", task.CreatedDate.ToString("yyyy-MM-dd HH:mm"), width, borderColor));
```

### Change 5: Update tests that assert null CreatedDate

**File:** `src/KanbanCli.Tests/Storage/MarkdownParserTests.cs`

Before:
```csharp
[Fact]
public void Parse_InvalidDateFormat_ReturnsNullCreatedDate()
{
    // ...
    result.CreatedDate.Should().BeNull();
}
```

After:
```csharp
[Fact]
public void Parse_InvalidDateFormat_DefaultsToUtcNow()
{
    // ...
    result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
}
```

Similar updates needed for:
- `Parse_MissingCreatedDate_ReturnsNullCreatedDate` -> `Parse_MissingCreatedDate_DefaultsToUtcNow`
- `Parse_InvalidDate_ReturnsNullInsteadOfSilentDefault` -> `Parse_InvalidDate_DefaultsToUtcNow`
- `Parse_GarbageDate_ReturnsNullInsteadOfSilentDefault` -> `Parse_GarbageDate_DefaultsToUtcNow`
- `Parse_EmptyMarkdown_ReturnsTaskWithDefaults` -> assert `CreatedDate` is close to now
- `Parse_OnlyTitleNoMetadata_UsesDefaults` -> assert `CreatedDate` is close to now
- `Parse_OnlyWhitespace_ReturnsSensibleDefaults` -> assert `CreatedDate` is close to now
- `Parse_CompletelyEmptyFile_ReturnsSensibleDefaults` -> assert `CreatedDate` is close to now

## Acceptance Criteria

- [x] `TaskItem.CreatedDate` changed from `DateTime?` to `DateTime`
- [x] `MarkdigMarkdownParser.Parse` defaults to `DateTime.UtcNow` when date is missing or malformed
- [x] `Serialize` no longer uses null-conditional on `CreatedDate`
- [x] `TaskDetailPanel` no longer shows `(unknown)` for created date
- [x] All affected tests updated to expect a non-null date
- [x] Roundtrip tests still pass with the new non-nullable type
- [x] No compiler warnings related to nullable reference types on `CreatedDate`

## Progress Log

- 2026-03-05 - Task created from backlog scan
