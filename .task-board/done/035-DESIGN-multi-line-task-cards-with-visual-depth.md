# DESIGN: Multi-Line Task Cards with Visual Depth and Icons

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: design, tui

## Context & Motivation

Task cards are currently single-line text with all information crammed together (ID, type, title, priority, labels). This causes visual overload. Moving to a multi-line card layout with priority indicators and proper spacing would make the board much more scannable and visually pleasing.

## Current State

**BEFORE** — single-line cards, no borders:
```
 #001 FEATURE: Add dark mode support [HIGH] [ui, frontend]
 #002 BUG: Fix login crash on timeout [MEDIUM] [api]
```

Everything competes for attention on one cramped line.

## Desired Outcome

**AFTER** — multi-line cards with visual hierarchy:
```
 ● #001 FEATURE                    ← Type color-coded, priority dot (● ◐ ○)
   Add dark mode support           ← Title on its own line, prominent
   ⬤ High  [ui] [frontend]         ← Priority + labels on separate line
                                   ← Blank line between cards
 ● #002 BUG
   Fix login crash on timeout
   ◐ Medium  [api]
```

Or for selected card (with highlight background):
```
 ▶ #001 FEATURE                    ← Arrow indicator for selected
   Add dark mode support
   ⬤ High  [ui] [frontend]
```

## Technical Approach

### TaskCard.RenderWithColors() changes:
**BEFORE:**
```csharp
// Single line: everything on one row
Console.Write($"#{task.Id:D3} ");
Console.Write($"{typePrefix}");
Console.Write(": ");
Console.Write(truncatedTitle);
Console.Write($"[{task.Priority}]");
// labels...
```

**AFTER:**
```csharp
// Line 1: Selection indicator + ID + Type
var indicator = isSelected ? "▶" : " ";
Console.Write($" {indicator} #{task.Id:D3} ");
Console.ForegroundColor = TuiHelpers.GetTypeColor(task.Type);
Console.Write(task.Type.ToString().ToUpperInvariant());
// pad to end of line

// Line 2: Title (indented)
Console.Write($"   {truncatedTitle}");
// pad to end of line

// Line 3: Priority indicator + Labels
var prioritySymbol = task.Priority switch { High => "●", Medium => "◐", _ => "○" };
Console.ForegroundColor = TuiHelpers.GetPriorityColor(task.Priority);
Console.Write($"   {prioritySymbol} {task.Priority}");
// render labels in brackets
```

### ColumnView changes:
- Account for 3-4 lines per card (3 content + 1 blank separator)
- Calculate visible card count based on available vertical space
- Adjust scroll/clipping for multi-line cards

## Progress Log

- 2026-03-05 - Task created from visual audit round 9

## Acceptance Criteria

- [x] Task cards render as 3 lines: (1) ID + Type, (2) Title, (3) Priority + Labels
- [x] Priority shown with Unicode indicators: ● High, ◐ Medium, ○ Low
- [x] Selected card uses ▶ arrow prefix and highlighted background
- [x] Unselected cards use space prefix (or ● dot)
- [x] Blank line between cards for visual separation
- [x] Labels displayed as individual [bracketed] items in DarkYellow
- [x] Card content properly truncated to column width
- [x] All tests pass
