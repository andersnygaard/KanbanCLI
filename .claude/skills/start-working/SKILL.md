---
name: start-working
description: Autonomously execute ALL tasks from the backlog until complete. Runs in a continuous loop - picks task, implements via subagent, moves to done, repeats. No user prompts between tasks. Follows task-board workflow (backlog → in-progress → done).
---

# Start Working Skill

This skill **autonomously executes ALL tasks** from the task board until the backlog is empty. It runs in a continuous loop without stopping between tasks.

**Task Board Flow**: `.task-board/backlog/` → `.task-board/in-progress/` → `.task-board/done/`

---

## CRITICAL RULES

### 1. USE THE TASK BOARD SYSTEM
**ALL work flows through `.task-board/`** — non-negotiable.
- Tasks live in: `backlog/` → `in-progress/` → `done/`
- `PLANNING-BOARD.md` is the source of truth for priorities
- **NEVER work on something not tracked in the task board**
- Update task files with progress as you work

### 2. NEVER USE GIT COMMANDS
**ABSOLUTELY NO git commands** — not by the main agent, not by subagents.
- No `git add`, `git commit`, `git push`, `git pull`, `git checkout`, `git branch`, etc.
- No `git status`, `git diff`, `git log` (even read-only commands)
- The user handles all git operations manually.

### 3. USE SUBAGENTS FOR EACH TASK
Each task MUST be executed using the **Agent tool** with a subagent:
- **Default model: `haiku`** — fast and efficient for most tasks
- **Use `sonnet` only for**: Complex business logic, architectural decisions, intricate parsing/serialization logic
- **ALWAYS include CLAUDE.md context** in subagent prompts

### 4. TASKS ARE DONE IN ORDER
Tasks are numbered for dependency reasons. Execute them **sequentially, in order**:
- Pick task 001 → Complete → Pick task 002 → Complete → Pick task 003...
- **NEVER skip ahead** unless user explicitly requests it
- **NEVER run tasks in parallel**

### 5. VERIFY WITH DOTNET BUILD AND TEST
Every task MUST be verified before marking complete:
```bash
dotnet build src/
dotnet test src/
```
**Tasks that don't build or have failing tests are INCOMPLETE.**

### 6. ALWAYS PREFER TOOLS OVER BASH
Subagents MUST use built-in tools instead of bash equivalents.

| NEVER USE | ALWAYS USE |
|-----------|------------|
| `cat`, `head`, `tail` | **Read** tool |
| `echo >`, `cat <<EOF` | **Write** tool |
| `sed`, `awk` | **Edit** tool |
| `find`, `ls` (for search) | **Glob** tool |
| `grep`, `rg` | **Grep** tool |

---

## When to Use This Skill

Use when the user says:
- "Start working" / "Start implementing"
- "Continue work" / "Keep going"
- "Pick up the next task"
- "Work on the planning board items"

## The Workflow

### Step 1: Check Current Priorities

Read `.task-board/PLANNING-BOARD.md` to see what's next.

**If PLANNING-BOARD is empty**: Ask if the user wants to populate it from the backlog.

### Step 2: Select Top Priority

Pick the **first numbered item** from the planning board (unless blocked).

**Decision criteria**:
- Is it blocked by dependencies?
- Are all prerequisites met? (check task file's Dependencies section)
- Is the scope clear and actionable?

### Step 3: Move to In-Progress

Move the task file from `.task-board/backlog/` to `.task-board/in-progress/`.

**Important**: Limit in-progress to **1 task**. If in-progress already has tasks, ask the user.

### Step 4: Read the Task File

Understand the full task:
- Context & Motivation
- Acceptance Criteria (checkboxes to complete) — **MUST be at the bottom of the task file**
- Technical Approach with **before and after code examples** for every code change
- Dependencies and risks

**Task File Requirements:**
- All acceptance criteria MUST appear at the bottom of the task file as checkboxes
- All code changes MUST be represented with before/after examples in the Technical Approach section
- Example format:
  ```
  **Before:**
  Console.ForegroundColor = ConsoleColor.Red;

  **After:**
  Console.ForegroundColor = Theme.Error;
  ```

### Step 5: Clarify Uncertainties

**STOP and ask the user if**:
- The task is ambiguous
- Multiple approaches are possible
- Dependencies are unclear
- Scope seems too large

### Step 6: Assess Complexity

**If task is too complex**: Break it into sub-tasks, create new files in `backlog/`, update PLANNING-BOARD.md.

### Step 7: Spawn Subagent to Implement

Use the **Agent tool** to spawn a subagent for the task:

```
Agent tool:
  subagent_type: "general-purpose"
  model: "haiku" (default) or "sonnet" (for complex tasks)
  description: "Short 3-5 word summary"
  prompt: <detailed instructions - see Subagent Prompt Template below>
```

### Step 8: Verify Completion

After subagent returns, verify:
1. All acceptance criteria checked off in the task file
2. `dotnet build src/` succeeds
3. `dotnet test src/` passes
4. Code follows project conventions (see .docs/specs.md)

### Step 9: Complete and Move to Done

1. **Update Resolution section** in the task file with implementation summary
2. **Move file** from `.task-board/in-progress/` to `.task-board/done/`
3. **Update PLANNING-BOARD.md**: remove completed item, add to "Recently Completed"
4. **IMMEDIATELY start next task** — no waiting for user confirmation

### Step 10: Loop

Continue to the next task. **DO NOT STOP between tasks.** Continue until:
- All tasks in PLANNING-BOARD are complete, OR
- A critical blocker prevents progress, OR
- User explicitly interrupts

---

## Subagent Prompt Template

```
Implement the task specified in:
.task-board/in-progress/{TASK-FILE}.md

Read the task file FIRST to understand all requirements.

PROJECT CONTEXT:
- This is a .NET C# Kanban CLI application (console/TUI app)
- Read .docs/specs.md for the full specification
- Read .claude/CLAUDE.md for project conventions (NOTE: CLAUDE.md may be outdated
  if task 001 hasn't updated it yet — trust .docs/specs.md as the source of truth)
- Project structure: src/KanbanCli/ (main app) and src/KanbanCli.Tests/ (tests)

TECH STACK:
- .NET / C# with nullable reference types
- Markdig for markdown parsing
- xUnit or NUnit for testing (check what's in .csproj)
- FluentAssertions for readable assertions
- NSubstitute for mocking
- record types for data, class for services
- Pattern matching and switch expressions

CODE CONVENTIONS:
- PascalCase for public members, camelCase for private
- Interfaces everywhere (ITaskRepository, IMarkdownParser, etc.)
- Dependency injection ready
- Test naming: MethodName_Scenario_ExpectedResult
- Arrange-Act-Assert pattern
- One assertion per test where possible
- Keep methods small and single-purpose
- ALL methods and properties MUST use block bodies (NO expression-bodied members with =>)
  - YES: public int Foo() { return 42; }
  - NO:  public int Foo() => 42;
- ALL TUI colors MUST come from Theme.cs — never hardcode ConsoleColor values

CRITICAL RESTRICTIONS:
1. Work ONLY on this task from .task-board/ — no ad-hoc work
2. NEVER use git commands (git add, commit, push, status, diff, etc.)
3. Use Read tool (NOT cat/head/tail)
4. Use Write tool (NOT echo/cat heredoc)
5. Use Edit tool (NOT sed/awk)
6. Use Glob tool (NOT find/ls for search)
7. Use Grep tool (NOT grep/rg bash commands)
8. CHECK OFF acceptance criteria as you complete them (change [ ] to [x])

ACCEPTANCE CRITERIA: As you complete each criterion, UPDATE THE TASK FILE
to check it off: change "- [ ]" to "- [x]".

When done, run:
  dotnet build src/
  dotnet test src/

Provide a summary of:
- Files created/modified
- All acceptance criteria status (checked off in task file)
- Build and test results
- Any issues encountered
```

---

## Model Selection

| Complexity | Model | Examples |
|-----------|-------|---------|
| **Standard** (default) | `haiku` | Simple classes, interfaces, basic implementations, tests, file operations, enums |
| **Complex** | `sonnet` | Markdig AST parsing, complex business logic, multi-file architectural decisions, TUI rendering logic |

---

## Project-Specific Context

### Architecture (from .docs/specs.md)

```
┌─────────────────────────────────────────────┐
│  TUI (Tui/)                                 │
│  Renders board, handles input               │
├─────────────────────────────────────────────┤
│  Services (Services/)                       │
│  Orchestrates operations                    │
├─────────────────────────────────────────────┤
│  Models (Models/)                           │
│  Rich domain models with own logic          │
├─────────────────────────────────────────────┤
│  Storage (Storage/)                         │
│  Markdown parsing (Markdig), file system IO │
└─────────────────────────────────────────────┘
```

### Project Structure

```
src/
├── KanbanCli/
│   ├── Program.cs
│   ├── Models/        # TaskItem, Board, Column, Enums
│   ├── Storage/       # ITaskRepository, IMarkdownParser, IFileSystem
│   ├── Services/      # ITaskService, IBoardService
│   └── Tui/           # IBoardRenderer, views, input handling
└── KanbanCli.Tests/
    ├── Models/
    ├── Storage/
    └── Services/
```

### Build & Test Commands

```bash
# Build
dotnet build src/

# Test
dotnet test src/

# Run the app
dotnet run --project src/KanbanCli/
```

---

## Handling Edge Cases

### If PLANNING-BOARD is Empty
Ask the user to populate it or offer to add top backlog items.

### If Top Priority is Blocked
Identify the blocker, check if the blocker task exists, and ask the user.

### If Task is Too Large
Break into sub-tasks (e.g., 003a, 003b), create new files, update PLANNING-BOARD.md.

### If Build Fails After Implementation
Fix the issues before marking complete. Re-run build and tests.

---

## Success Criteria

### Per Task:
- [ ] Task comes from `.task-board/`
- [ ] NO git commands used
- [ ] Executed via subagent (Agent tool)
- [ ] Built-in tools used (no bash for file ops)
- [ ] All acceptance criteria checked off in task file
- [ ] `dotnet build` succeeds
- [ ] `dotnet test` passes
- [ ] Task file updated with Progress Log and Resolution
- [ ] Task moved to `.task-board/done/`

### Per Session:
- [ ] All work tracked in `.task-board/`
- [ ] Zero git commands executed
- [ ] ALL tasks processed automatically (no stopping between tasks)
- [ ] Tasks processed IN ORDER from PLANNING-BOARD.md
- [ ] PLANNING-BOARD.md updated after each completion

## See Also

- [`.task-board/PLANNING-BOARD.md`](../../../.task-board/PLANNING-BOARD.md) — Current priorities
- [`.docs/specs.md`](../../../.docs/specs.md) — Full project specification
- [`.claude/skills/task-board/SKILL.md`](../task-board/SKILL.md) — Planning skill
- [`.claude/skills/backlog-scan/SKILL.md`](../backlog-scan/SKILL.md) — Gap detection skill
