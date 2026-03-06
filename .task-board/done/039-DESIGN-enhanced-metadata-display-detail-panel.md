# DESIGN: Enhanced Metadata Display in Detail Panel

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: tui, aesthetics, design

## Context & Motivation

The TaskDetailPanel metadata section currently renders as plain key-value pairs. With box-drawing borders already in place, we can enhance the metadata to be more visually informative:
- Priority should show the same colored symbol (●/◐/○) used in task cards
- Type should be color-coded consistently with the board view
- Labels should each have their own styled bracket with color
- Status should use a visual indicator showing progress through the workflow
- ExtraMetadata fields (like "Estimated Effort") should also be rendered

## Current State

`RenderMetadataFields` in `TaskDetailPanel.cs` (lines 72-89):

```csharp
private static void RenderMetadataFields(TaskItem task, int width, ConsoleColor borderColor)
{
    RenderField("ID", $"#{task.Id:D3}", width, borderColor);
    RenderField("Title", task.Title, width, borderColor);
    RenderField("Type", task.Type.ToString(), width, borderColor);
    RenderField("Priority", task.Priority.ToString(), width, borderColor, TuiHelpers.GetPriorityColor(task.Priority));
    RenderField("Status", TuiHelpers.FormatStatus(task.Status), width, borderColor);
    // Labels as plain text...
}
```

Priority has color but no symbol. Type has no color. Labels are joined as plain text. Status is plain text with no workflow visualization. ExtraMetadata is not rendered.

## Desired Outcome

A richer metadata display:
- **Type**: Color-coded with `TuiHelpers.GetTypeColor()` (matching the board view)
- **Priority**: Shows `● High` in red, `◐ Medium` in yellow, `○ Low` in green (matching task cards)
- **Status**: Shows a workflow indicator like `Backlog → [In Progress] → Done → On Hold` with current status highlighted
- **Labels**: Each label in its own `[bracket]` with DarkYellow color (matching task cards)
- **ExtraMetadata**: Any extra fields (Estimated Effort, etc.) rendered below the standard fields

## Technical Approach

### Update RenderMetadataFields

```csharp
private static void RenderMetadataFields(TaskItem task, int width, ConsoleColor borderColor)
{
    RenderField("ID", $"#{task.Id:D3}", width, borderColor);
    RenderField("Title", task.Title, width, borderColor);
    RenderField("Type", task.Type.ToString(), width, borderColor, TuiHelpers.GetTypeColor(task.Type));

    // Priority with symbol
    var prioritySymbol = TuiHelpers.GetPrioritySymbol(task.Priority);
    RenderField("Priority", $"{prioritySymbol} {task.Priority}", width, borderColor, TuiHelpers.GetPriorityColor(task.Priority));

    // Status with workflow indicator
    RenderStatusWorkflow(task.Status, width, borderColor);

    // Labels with colored brackets
    RenderLabelsField(task.Labels, width, borderColor);

    RenderField("Created", task.CreatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "(unknown)", width, borderColor);
    if (task.CompletedDate.HasValue)
        RenderField("Completed", task.CompletedDate.Value.ToString("yyyy-MM-dd HH:mm"), width, borderColor);

    // Extra metadata
    foreach (var meta in task.ExtraMetadata)
        RenderField(meta.Key, meta.Value, width, borderColor);
}
```

### New RenderStatusWorkflow method

```csharp
private static void RenderStatusWorkflow(TaskStatus status, int width, ConsoleColor borderColor)
{
    DialogHelper.RenderBoxLeftBorder(borderColor);
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write($"{"Status",-12}");

    var statuses = new[] {
        (TaskStatus.Backlog, "Backlog"),
        (TaskStatus.InProgress, "In Progress"),
        (TaskStatus.Done, "Done"),
        (TaskStatus.OnHold, "On Hold")
    };

    var written = 12;
    foreach (var (s, name) in statuses)
    {
        if (s == status)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Write($" {name} ");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" {name} ");
        }
        written += name.Length + 2;

        // Arrow between statuses (except last)
        if (s != TaskStatus.OnHold)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("→");
            written += 1;
        }
    }

    DialogHelper.RenderBoxRightBorder(written, width, borderColor);
}
```

## Progress Log

- 2026-03-05 - Task created

## Acceptance Criteria

- [x] Type field uses `TuiHelpers.GetTypeColor()` for color coding
- [x] Priority field shows priority symbol (●/◐/○) from `TuiHelpers.GetPrioritySymbol()`
- [x] Status shows visual workflow indicator with current status highlighted
- [x] Labels rendered with individual colored brackets matching task card style
- [x] ExtraMetadata fields rendered after standard metadata
- [x] Long field values properly truncated to fit box width
- [x] All existing tests pass (`dotnet build src/` and `dotnet test src/`)
