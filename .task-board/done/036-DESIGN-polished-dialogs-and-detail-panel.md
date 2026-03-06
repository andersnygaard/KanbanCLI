# DESIGN: Polished Dialogs and Detail Panel with Borders

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: design, tui

## Context & Motivation

All dialogs (NewTask, Move, Filter, Priority, Confirm) and the TaskDetailPanel are plain text without visual borders. Adding box-drawing frames, colored key highlights, and better spacing would make the app feel cohesive and professional.

## Current State

**BEFORE** — plain dialog (NewTaskDialog):
```
New Task

  Title: _
  Type:
    1. Feature
    2. Bug
    3. Refactor
  Choice: _
```

**BEFORE** — plain detail panel:
```
 Task Details
  ID          : 001
  Type        : FEATURE
  Title       : Add dark mode
  Status      : Backlog
  Priority    : High
  Labels      : ui, frontend
  Created     : 2026-03-05
------------------------------------------------------------
  ## Context & Motivation

    Why this task exists...

  [T]itle  [L]abels  [P]riority  [Esc] back
```

## Desired Outcome

**AFTER** — bordered dialog:
```
┌─── New Task ──────────────────────────┐
│                                       │
│  Title: _                             │
│                                       │
│  Type:                                │
│    1. Feature                         │
│    2. Bug                             │
│    3. Refactor                        │
│                                       │
│  Choice: _                            │
│                                       │
└───────────────────────────────────────┘
```

**AFTER** — bordered detail panel:
```
┌─── Task #001 ─────────────────────────────────────────────┐
│                                                           │
│  Type       FEATURE              Priority    ● High       │
│  Status     Backlog              Labels      [ui] [front] │
│  Created    2026-03-05                                    │
│                                                           │
├───────────────────────────────────────────────────────────┤
│  ## Context & Motivation                                  │
│                                                           │
│  Why this task exists and what problem it solves.         │
│                                                           │
├───────────────────────────────────────────────────────────┤
│  [T]itle  [L]abels  [P]riority  [Esc] back              │
└───────────────────────────────────────────────────────────┘
```

## Technical Approach

### Add RenderBox helper to DialogHelper:
```csharp
public static void RenderBoxTop(string title, int width, ConsoleColor borderColor = ConsoleColor.DarkGray)
{
    Console.ForegroundColor = borderColor;
    Console.Write("┌─── ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write(title);
    Console.ForegroundColor = borderColor;
    Console.Write(" ");
    Console.WriteLine(new string('─', width - title.Length - 6) + "┐");
}

public static void RenderBoxBottom(int width, ConsoleColor borderColor = ConsoleColor.DarkGray)
{
    Console.ForegroundColor = borderColor;
    Console.WriteLine("└" + new string('─', width - 2) + "┘");
}

public static void RenderBoxLine(string content, int width, ConsoleColor borderColor = ConsoleColor.DarkGray)
{
    Console.ForegroundColor = borderColor;
    Console.Write("│ ");
    Console.ResetColor();
    Console.Write(content.PadRight(width - 4));
    Console.ForegroundColor = borderColor;
    Console.WriteLine(" │");
}
```

### Apply to each dialog by wrapping content in box helpers.

## Progress Log

- 2026-03-05 - Task created from visual audit round 9
