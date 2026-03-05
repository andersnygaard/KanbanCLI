# Task Board

File-based task management for the AI Assistert Kode project. Each task is a markdown file, organized into folders representing board columns.

## Directory Structure

```
.task-board/
  README.md              # This file
  PLANNING-BOARD.md      # Current top 3-5 priorities
  backlog/               # Tasks not yet started
  in-progress/           # Actively being worked on
  done/                  # Completed tasks
  on-hold/               # Deferred/blocked tasks
```

## File Naming

```
{NNN}-{TYPE}-{short-description}.md
```

- **Number**: Sequential ID (never reuse, scan all folders for next number)
- **Type prefixes**: `FEATURE-`, `BUG-`, `REFACTOR-`, `EXPLORE-`, `EPIC-`, `DESIGN-`, `PERF-`, `CLEANUP-`, `DOCS-`, `TEST-`, `SECURITY-`, `A11Y-`, `QUALITY-`
- **Description**: Kebab-case summary

## Workflow

1. **Plan**: Use the `task-board` skill to create plans in `backlog/`
2. **Prioritize**: Add top items to `PLANNING-BOARD.md` (max 3-5)
3. **Start**: Move file to `in-progress/`, update status in file
4. **Complete**: Move file to `done/`, update PLANNING-BOARD.md
5. **Defer**: Move to `on-hold/` if blocked

## Task Statuses

| Status       | Folder         | Meaning                  |
|-------------|----------------|--------------------------|
| Backlog     | `backlog/`     | Planned, not started     |
| In Progress | `in-progress/` | Actively being worked on |
| Done        | `done/`        | Completed                |
| On Hold     | `on-hold/`     | Deferred or blocked      |
