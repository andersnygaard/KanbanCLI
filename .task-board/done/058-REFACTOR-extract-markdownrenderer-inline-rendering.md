# REFACTOR: Extract MarkdownRenderer Inline Rendering and Add Constants

**Status**: Done
**Created**: 2026-03-05
**Priority**: High
**Labels**: refactor, tui, readability

## Context & Motivation

`MarkdownRenderer.cs` has two critical issues:
1. `RenderInlineContent()` (59 lines) and `RenderParagraph()` (60 lines) both exceed the 30-line limit and contain nearly identical switch logic for handling inline markdown elements
2. The magic number `boxWidth - 6` is repeated 5+ times without a named constant

## Desired Outcome

- Shared inline rendering helper eliminates duplication
- Both methods are under 30 lines
- Magic number extracted to constant

## Technical Approach

**File: `src/KanbanCli/Tui/MarkdownRenderer.cs`**

**Step 1: Add constant**

**Before:**
```csharp
var maxWidth = boxWidth - 6;
```

**After:**
```csharp
private const int BoxContentPadding = 6;

// In methods:
var maxWidth = boxWidth - BoxContentPadding;
```

**Step 2: Extract shared inline rendering**

Create a shared helper that handles the common inline rendering switch:

```csharp
private static int RenderInlineSequence(ContainerInline container, int maxWidth, ConsoleColor defaultColor)
{
    var written = 0;
    foreach (var inline in container)
    {
        if (written >= maxWidth) break;
        written += RenderSingleInline(inline, maxWidth - written, defaultColor);
    }
    return written;
}

private static int RenderSingleInline(Inline inline, int remaining, ConsoleColor defaultColor)
{
    return inline switch
    {
        // Handle EmphasisInline, CodeInline, LiteralInline, LineBreakInline
    };
}
```

**Step 3: Simplify RenderInlineContent and RenderParagraph to use shared helper**

## Acceptance Criteria

- [x] BoxContentPadding constant replaces all `boxWidth - 6` occurrences
- [x] Shared inline rendering helper eliminates duplication between RenderInlineContent and RenderParagraph
- [x] RenderInlineContent is under 30 lines
- [x] RenderParagraph is under 30 lines
- [x] All new methods are under 30 lines
- [x] No functional changes — rendering is identical
- [x] All existing tests pass

## Progress Log

- 2026-03-05 - Task created
- 2026-03-05 - Refactoring completed: BoxContentPadding constant added, RenderInlineSequence/RenderSingleInline/WriteInlineText helpers extracted, both RenderInlineContent and RenderParagraph simplified
