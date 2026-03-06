# DESIGN: Board Layout with Box-Drawing Borders and Column Separators

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: design, tui

## Context & Motivation

The board currently has NO visual structure — columns blend together without separators, there are no borders around the board, and column headers use plain dashes. Adding Unicode box-drawing characters (─ │ ┌ ┐ └ ┘ ├ ┤ ┬ ┴) would dramatically improve visual clarity and make the app feel polished and professional.

## Current State

**BEFORE** — what the board looks like now:
```
 Kanban Board                                          ← Blue background bar
 Backlog (3)       In Progress (2)  Done (1)   On Hold ← Column headers
 ---------------   ---------------  ---------  ------- ← Dash separators
 #001 FEAT: Task   #003 BUG: Fix    #004 ...          ← Task cards (no borders)
 #002 BUG: Other                                       ← Cards blend together
                                                       ← No column separators!
 ←→ Column | ↑↓ Task | Enter View | n New | q Quit    ← Status bar
```

## Desired Outcome

**AFTER** — polished board with box-drawing structure:
```
┌──────────────────────────────────────────────────────────────────────┐
│  ◼ Kanban Board                                                     │
├─────────────────┬──────────────────┬─────────────────┬──────────────┤
│  Backlog [3]    │  In Progress [2] │  Done [1]       │  On Hold [0] │
├─────────────────┼──────────────────┼─────────────────┼──────────────┤
│  #001 FEAT:     │  #003 BUG:       │  #004 FEAT:     │              │
│  Add dark mode  │  Fix login crash │  Export to CSV  │  (no tasks)  │
│  ⬤ High [ui]    │  ⬤ High [api]    │  ○ Low          │              │
│                 │                  │                 │              │
│  #002 BUG:      │                  │                 │              │
│  Crash on save  │                  │                 │              │
│  ⬤ Med [core]   │                  │                 │              │
├─────────────────┴──────────────────┴─────────────────┴──────────────┤
│  ←→ Column │ ↑↓ Task │ Enter View │ n New │ m Move │ d Del │ q Quit│
└──────────────────────────────────────────────────────────────────────┘
```

## Technical Approach

### BoardView changes:
- Calculate column positions (x coordinates) for vertical separators
- Render top border: `┌` + `─` * colWidth + `┬` between columns + `┐`
- Between header and body: `├` + `─` * colWidth + `┼` between columns + `┤`
- Bottom before status: `├` + `─` * colWidth + `┴` between columns + `┤`
- Board bottom: `└` + `─` * totalWidth + `┘`
- Pass column positions to ColumnView for vertical border rendering

### ColumnView changes:
- Render `│` at left edge of each column
- Last column also gets `│` at right edge
- Content area is `columnWidth - 2` to account for borders

### StatusBar changes:
- Integrate into bottom border frame
- Use `│` for left/right edges

## Progress Log

- 2026-03-05 - Task created from visual audit round 9

## Acceptance Criteria

- [x] Add top border with box-drawing around entire board (┌─┐)
- [x] Add vertical column separators (│) between each column
- [x] Add box-drawing junction characters at header intersections (┬ ┼ ┤ ├)
- [x] Replace dash column separators with box-drawing horizontal lines (─)
- [x] Add bottom border connecting to status bar (└─┘)
- [x] Selected column separator highlighted in brighter color (Cyan)
- [x] Column headers formatted with task count badge: "Backlog [3]"
- [x] All tests pass
