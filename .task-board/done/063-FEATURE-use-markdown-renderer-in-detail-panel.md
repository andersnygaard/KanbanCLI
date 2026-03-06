# FEATURE: Use MarkdownRenderer for Rich Section Content in Detail Panel

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: tui, usability

## Context & Motivation

The spec lists `MarkdownRenderer.cs` as the component that "renders markdown content in the detail panel." The `MarkdownRenderer` class exists and has full rich rendering support: colored headings, styled bullet points, checkbox indicators (checked/unchecked), bold text, code blocks, and paragraph formatting.

However, `TaskDetailPanel.BuildContentLines()` renders all section content as plain unformatted text via `ContentLine.Text()`. This means acceptance criteria checkboxes appear as raw `- [ ]` text instead of styled checkbox symbols, code blocks have no syntax highlighting, and headings within sections are not visually distinguished. This significantly hurts readability and usability of the detail panel -- the most important view for understanding a task.

## Current State

In `TaskDetailPanel.BuildContentLines()`, section content is rendered as plain text:

```csharp
var sectionLines = section.Value.Split('\n');
foreach (var line in sectionLines)
{
    var trimmed = line.TrimEnd('\r');
    var displayLine = $"  {trimmed}";
    if (displayLine.Length > maxContentWidth)
        displayLine = displayLine[..maxContentWidth];
    lines.Add(ContentLine.Text(displayLine, width, borderColor));
}
```

The `MarkdownRenderer.RenderMarkdownContent()` method exists but is never called from anywhere in the codebase.

## Desired Outcome

Section content in the detail panel uses `MarkdownRenderer` to render rich formatted output: colored headings, styled checkboxes, bullet points, bold text, and code blocks.

## Technical Approach

The current `ContentLine` approach (pre-build all lines, render visible ones for scroll support) is incompatible with `MarkdownRenderer.RenderMarkdownContent()` which writes directly to the console. The solution is to create a `ContentLine.Markdown()` factory that captures the section content and renders it using `MarkdownRenderer` when visible.

### 1. Add a Markdown ContentLine factory

**File: `src/KanbanCli/Tui/TaskDetailPanel.cs`**

Add to the `ContentLine` class:

Before (no Markdown factory exists):
```csharp
public static ContentLine Text(string text, int width, ConsoleColor borderColor)
{
    return new ContentLine(() => DialogHelper.RenderBoxLine(text, width, borderColor));
}
```

After (add new factory alongside Text):
```csharp
public static ContentLine Text(string text, int width, ConsoleColor borderColor)
{
    return new ContentLine(() => DialogHelper.RenderBoxLine(text, width, borderColor));
}

public static ContentLine Markdown(string markdownContent, int width, ConsoleColor borderColor)
{
    return new ContentLine(() =>
    {
        MarkdownRenderer.RenderMarkdownContent(markdownContent, width, borderColor);
    });
}
```

### 2. Replace plain text rendering with Markdown rendering in BuildContentLines

**File: `src/KanbanCli/Tui/TaskDetailPanel.cs`**

In `BuildContentLines()`, replace the section content rendering:

Before:
```csharp
if (!string.IsNullOrWhiteSpace(section.Value))
{
    var maxContentWidth = width - 6;
    var sectionLines = section.Value.Split('\n');
    foreach (var line in sectionLines)
    {
        var trimmed = line.TrimEnd('\r');
        var displayLine = $"  {trimmed}";
        if (displayLine.Length > maxContentWidth)
            displayLine = displayLine[..maxContentWidth];
        lines.Add(ContentLine.Text(displayLine, width, borderColor));
    }
}
```

After:
```csharp
if (!string.IsNullOrWhiteSpace(section.Value))
{
    lines.Add(ContentLine.Markdown(section.Value, width, borderColor));
}
```

### 3. Handle scroll line counting

The `Markdown` ContentLine renders multiple visual lines but counts as a single `ContentLine` object. This means scroll offset calculations will be imprecise for sections with many lines. This is an acceptable trade-off for the initial implementation -- the detail panel already has imprecise scrolling for very long sections. A future task can refine line counting if needed.

An alternative is to split the markdown rendering into individual lines by pre-rendering to a string buffer and creating one `ContentLine.Text()` per output line. This approach preserves accurate scroll counting but loses color information. For the initial implementation, the simpler single-ContentLine approach is recommended.

## Dependencies

None -- `MarkdownRenderer` is already fully implemented and tested.

## Risks & Considerations

- The `ContentLine`/scroll architecture assumes each `ContentLine` is exactly one visual line. A `ContentLine.Markdown()` that renders multiple lines will make scroll offset arithmetic inaccurate. For long sections this could mean the scroll indicator shows incorrect position. This is acceptable for an initial implementation.
- If buffered output (task #062) is implemented first, the markdown rendering will also benefit from the buffering.

## Progress Log

- 2026-03-05 - Task created from backlog scan

## Acceptance Criteria

- [x] Section content in TaskDetailPanel renders using `MarkdownRenderer.RenderMarkdownContent()`
- [x] Acceptance Criteria checkboxes display as styled checkbox symbols (checked/unchecked) instead of raw `- [ ]` text
- [x] Bullet points display with colored bullet symbols
- [x] Bold text and code spans render with distinct colors
- [x] Headings within sections render with heading colors
- [x] Plain text sections (no markdown formatting) still render correctly
- [x] Scrolling through the detail panel still works (may be imprecise for long markdown sections)
- [x] `MarkdownRenderer` class is no longer dead code
