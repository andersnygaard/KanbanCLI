# DESIGN: Rich Markdown Rendering in TaskDetailPanel

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: tui, markdown, aesthetics

## Context & Motivation

The TaskDetailPanel currently renders section content as raw text using `Console.Write` with no markdown interpretation. Headings, bullet lists, checkboxes, bold text, and code blocks are all displayed as plain strings. This is a poor experience for a tool that stores tasks as markdown files.

The user specifically asked: "have we made a good markdown visualizer? i mean you've got the frontmatter, you've got code, you've got headlines. Maybe there are good frameworks out there for visualizing markdown?"

While Spectre.Console doesn't have a native markdown renderer, we can build a lightweight markdown-aware renderer using the Markdig AST (which we already depend on) to parse section content and apply terminal formatting — colored headings, styled bullet points, checkbox indicators, bold text, and dimmed code blocks.

## Current State

The `RenderSectionContent` method in `TaskDetailPanel.cs` (line 284-304):

```csharp
private static void RenderSectionContent(string content, int width, ConsoleColor borderColor)
{
    if (string.IsNullOrWhiteSpace(content))
        return;

    DialogHelper.RenderBoxEmptyLine(width, borderColor);

    var maxContentWidth = width - 6;
    var lines = content.Split('\n');
    foreach (var line in lines)
    {
        var trimmed = line.TrimEnd('\r');
        var displayLine = $"  {trimmed}";
        if (displayLine.Length > maxContentWidth)
            displayLine = displayLine[..maxContentWidth];

        DialogHelper.RenderBoxLine(displayLine, width, borderColor);
    }

    DialogHelper.RenderBoxEmptyLine(width, borderColor);
}
```

This just dumps raw text with no markdown interpretation at all.

## Desired Outcome

A `MarkdownRenderer` class that interprets markdown content and renders it with rich terminal formatting:
- **Headings** (`### H3`, `#### H4`): Rendered in bold/bright colors with underline effect
- **Bullet lists** (`- item`): Rendered with colored bullet symbols (•)
- **Checkboxes** (`- [ ]`, `- [x]`): Rendered with ☐ / ☑ symbols in appropriate colors
- **Bold text** (`**bold**`): Rendered in bright white
- **Code blocks** (``` ```): Rendered with dimmed background/different color
- **Inline code** (`` `code` ``): Rendered in a distinct color (e.g., DarkYellow)
- **Horizontal rules** (`---`): Rendered as box-drawing separator

## Acceptance Criteria

- [x] Create `MarkdownRenderer` class in `src/KanbanCli/Tui/MarkdownRenderer.cs`
- [x] Parse markdown content using Markdig AST (reuse existing Markdig dependency)
- [x] Render headings with distinct colors (Cyan for H2, DarkCyan for H3, etc.)
- [x] Render bullet lists with colored bullet symbols (• instead of -)
- [x] Render checkboxes with ☐ (unchecked) and ☑ (checked) symbols
- [x] Render bold text in bright white
- [x] Render code blocks with distinct color (DarkGray background or DarkYellow text)
- [x] Render inline code with distinct styling
- [x] Integrate into TaskDetailPanel.RenderSectionContent
- [x] Handle edge cases: empty content, very long lines, nested lists
- [x] All existing tests pass (`dotnet build src/` and `dotnet test src/`)

## Technical Approach

### Create MarkdownRenderer class

```csharp
namespace KanbanCli.Tui;

using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseTaskLists()
        .Build();

    public static void RenderMarkdownContent(string content, int boxWidth, ConsoleColor borderColor)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        var document = Markdown.Parse(content, Pipeline);
        var maxContentWidth = boxWidth - 6;

        foreach (var block in document)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    RenderHeading(heading, boxWidth, maxContentWidth, borderColor);
                    break;
                case ListBlock list:
                    RenderList(list, boxWidth, maxContentWidth, borderColor);
                    break;
                case FencedCodeBlock codeBlock:
                    RenderCodeBlock(codeBlock, boxWidth, maxContentWidth, borderColor);
                    break;
                case ThematicBreakBlock:
                    DialogHelper.RenderBoxSeparator(boxWidth, borderColor);
                    break;
                case ParagraphBlock paragraph:
                    RenderParagraph(paragraph, boxWidth, maxContentWidth, borderColor);
                    break;
            }
        }
    }
}
```

### Update TaskDetailPanel to use MarkdownRenderer

Replace the simple `RenderSectionContent` call with the new renderer:

```csharp
// Before:
DialogHelper.RenderBoxLine(displayLine, width, borderColor);

// After:
MarkdownRenderer.RenderMarkdownContent(section.Value, width, borderColor);
```

## Progress Log

- 2026-03-05 - Task created
