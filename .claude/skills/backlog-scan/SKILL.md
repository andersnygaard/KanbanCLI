---
name: backlog-scan
description: Scans the codebase against .docs/specs.md to find gaps between specification and implementation, checks code quality and readability best practices, and generates 3 prioritized tasks per round via the task-board skill.
---

# Backlog Scan

Autonomous scanner that compares specification vs implementation, evaluates code quality, and generates a focused batch of tasks.

**Pattern**: Spec-first gap analysis + best practice review

**PLANNING ONLY - NO IMPLEMENTATION**
This skill creates task files via the `task-board` skill. It does NOT write code.

## When to Use This Skill

**Use this skill when**:
- Starting a new development phase
- Backlog is stale or empty
- Want to discover gaps between spec and implementation
- Need to assess code quality and readability
- Running `/backlog-scan` slash command

**DO NOT use this skill for**:
- Creating a single specific task -> use `task-board` skill directly
- Implementing tasks -> this skill only discovers and plans
- Quick bug fixes -> just fix them directly

## Scan Workflow

### Phase 1: Load Context

Before any analysis, load ALL context:

1. **Read specification**: `.docs/specs.md` completely
2. **Read project instructions**: `.claude/CLAUDE.md`
3. **Scan completed tasks**: `.task-board/done/` (learn what's already done)
4. **Scan existing backlog**: `.task-board/backlog/` + `.task-board/in-progress/` + `.task-board/on-hold/`
5. **Note available skills**: Check `.claude/skills/` for delegation

### Phase 2: Spec vs Implementation Gap Analysis

**Goal**: Produce a single, comprehensive gap report by comparing `.docs/specs.md` against the actual codebase.

Use a **subagent** (Agent tool, model: `sonnet`) to perform this analysis. The subagent should:

1. **Read `.docs/specs.md`** end-to-end and extract every specified feature, interface, model, behavior, and non-functional requirement into a checklist.

2. **Scan all source code in `src/`** — list every `.cs` file, read key files, and map what exists:
   - Classes, records, enums, interfaces
   - Method signatures and their behaviors
   - Test files and what they cover
   - TODOs, FIXMEs, HACK comments

3. **Build a Spec Coverage Matrix** — for each item from the spec, mark its status:

   | Spec Item | Status | Evidence |
   |-----------|--------|----------|
   | Feature X | Implemented / Partial / Missing / N/A | File path or note |

   Categories to cover:
   - Core features (columns, tasks, labels, priorities, auto-numbering, type prefixes)
   - Architecture layers (TUI, Services, Models, Storage)
   - Interfaces specified in the spec
   - Test coverage specified in the spec
   - Non-functional requirements (readability, DI, immutability, block bodies, Theme.cs)

4. **Return the matrix** and a summary of the top gaps, ordered by impact.

### Phase 3: Code Quality & Readability Review

**Goal**: Evaluate existing code against best practices from the spec

Check for:
- **Naming**: Descriptive names, no abbreviations, consistent casing conventions
- **Method size**: Small, focused, single-purpose methods
- **Self-documenting code**: Code reads like prose, comments only for "why"
- **Consistent style**: One way of doing things throughout the project
- **SOLID principles**: Single responsibility, interface segregation, dependency inversion
- **Immutability**: Records with init properties, no unnecessary mutation
- **Error handling**: Graceful handling of edge cases
- **Test quality**: Arrange-Act-Assert, descriptive test names, one assertion per test
- **Code smells**: Magic strings, god classes, long parameter lists, deep nesting

### Phase 4: Task Generation (3 tasks per round)

From the gaps and quality issues found, select the **3 most impactful tasks**.

**Selection criteria** (prioritize in order):
1. **Blocking dependencies first**: Infrastructure/foundation that other features need
2. **Spec gaps over quality**: Missing features before polish
3. **High impact, right size**: Not epic-sized, not trivial
4. **Progressive**: Build on what exists, don't leap ahead

**Quality bar** (ALL must be met):
- Clear value: Obvious benefit or spec alignment
- Well-scoped: Completable in one focused session
- Actionable: Can implement without major unknowns
- Non-redundant: Not covered by existing task

**For each of the 3 tasks**:
1. Invoke `task-board` skill with full context
2. Include: what spec section it addresses, why it's prioritized now, technical approach hints
3. Assign sequential number (scan ALL `.task-board/**/*.md` for highest existing number)

**Task file format requirements**:
- Acceptance criteria MUST appear at the bottom of the task file as checkboxes (`- [ ]`)
- All code changes MUST be represented with **before and after examples** in the Technical Approach section
- Example:
  ```
  **Before:**
  Console.ForegroundColor = ConsoleColor.Red;

  **After:**
  Console.ForegroundColor = Theme.Error;
  ```

### Phase 5: Update Planning Board

After creating tasks, update `.task-board/PLANNING-BOARD.md` with:
- Current top 3 priorities (the tasks just created)
- Any recently completed work from `done/`
- Current focus area

## Numbering Protocol

**CRITICAL**: Never reuse numbers.

```
1. Glob: .task-board/**/*.md
2. Scan ALL folders: backlog/ + in-progress/ + done/ + on-hold/
3. Find highest number
4. Next task = highest + 1
```

File format: `NNN-TYPE-description.md`
- `001-FEATURE-project-scaffolding.md`
- `002-REFACTOR-naming-consistency.md`
- `003-QUALITY-test-coverage.md`

## Output

Summary report with:
- Spec coverage overview (X of Y spec items implemented)
- Top code quality findings
- 3 tasks created (number, type, title, priority)
- Recommended implementation order
- What to scan for next round

## Delegates To

- `task-board` skill — for creating detailed plan files