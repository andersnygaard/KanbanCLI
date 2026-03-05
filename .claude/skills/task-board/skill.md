---
name: task-board
description: Planning specialist that creates structured implementation plans for the AI Assistert Kode project. Use this skill to transform user requests into comprehensive, well-researched plan files stored in .task-board/backlog/. This skill plans without implementing.
---

# Task-Board Planning Skill

This skill provides specialized workflows for creating and managing implementation plans in the AI Assistert Kode project. It transforms user requests into comprehensive, well-researched plan files that guide future implementation.

**CRITICAL CONSTRAINT**: This skill is for planning and documentation ONLY. Never implement fixes, write code changes, or modify the codebase. The sole responsibility is creating thorough plan documentation in `.task-board/backlog/`.

## When to Use This Skill

**Use this skill for**:
- Feature planning requiring technical design
- Refactoring plans needing impact assessment
- Exploration and research documentation
- Breaking down epics into implementation phases
- User requests that need structured planning

**DO NOT use this skill for**:
- Quick bug fixes (just implement directly)
- Simple changes with obvious implementation
- Active code implementation (skill is planning-only)
- Trivial updates that don't need planning
- AI scaffolding (CLAUDE.md, rules, skills, commands) - update directly, no task

## Core Planning Principles

- **Research before planning**: Thoroughly explore the codebase before creating plan files
- **Ask clarifying questions**: Never assume -- gather complete information from users
- **Break down complexity**: Decompose large features into manageable phases
- **Identify dependencies**: Map out external packages, internal dependencies, and blocking work
- **Document technical approach**: Include architecture decisions, file paths, and code references
- **Assess risks**: Identify what could go wrong and mitigation strategies
- **Plan-only**: Focus solely on designing the approach, not implementing

## Planning Workflow

### Phase 1: Initial Understanding (Gather Context)

1. **Listen carefully**: Read the user's request completely
2. **Ask clarifying questions** to understand scope:
   - What problem are you trying to solve?
   - What does success look like?
   - Are there any constraints or preferences?
   - What's the priority level?
3. **Identify plan type**: Feature, refactor, exploration, or epic
4. **Assess complexity**: Simple (1-2 days), Medium (3-5 days), or Complex (1+ weeks)

### Phase 2: Codebase Research (Deep Exploration)

**CRITICAL**: Conduct thorough research before creating the plan file.

1. **Search for relevant code**:
   - Use search tools to find related features, components, or patterns
   - Look for similar implementations in the codebase
   - Check for existing utilities or shared components to reuse

2. **Read relevant files**:
   - Examine the feature area (components, pages, hooks)
   - Review related components and dependencies
   - Check for existing patterns and conventions

3. **Understand architecture**:
   - **Components**: `src/components/` - Reusable UI components
   - **Pages**: `src/pages/` - Page-level components
   - **Hooks**: `src/hooks/` - Custom React hooks
   - **Utils**: `src/utils/` - Utility functions
   - **Types**: `src/types/` - Shared TypeScript types
   - **Styling**: Styled Components co-located with components

4. **Map dependencies**:
   - What npm packages might be needed?
   - What internal features does this depend on?
   - Are there any blocking tasks?

5. **Identify risks**:
   - What could go wrong?
   - Are there performance concerns?
   - Security considerations?

### Phase 3: Approach Design (Technical Solution)

1. **Define architecture decisions**:
   - Where should code live? (component, page, hook, utility)
   - What patterns to follow? (existing patterns in the codebase)
   - State management approach? (useState, context, custom hooks)

2. **Break down into phases**:
   - Phase 1: Core functionality
   - Phase 2: Testing
   - Phase 3: Polish and edge cases

3. **Plan implementation steps**:
   - List specific files to create or modify
   - Describe key changes needed in each file
   - Identify test scenarios

4. **Consider project-specific context**:
   - React 18+ with TypeScript strict mode
   - Styled Components for all styling (StyledX naming)
   - Functional components with hooks only
   - Named exports preferred
   - ~150 line max per component file

### Phase 4: Documentation (Create Plan File)

Create a comprehensive plan file in `.task-board/backlog/` with:

1. **Descriptive filename** following conventions:
   - `FEATURE-[short-description].md` - New functionality
   - `REFACTOR-[short-description].md` - Code improvements
   - `EXPLORE-[short-description].md` - Research/investigation
   - `EPIC-[short-description].md` - Major multi-phase features

2. **Complete template** with all sections filled (see template below)

3. **Specific technical details**:
   - File paths: `src/components/MyComponent.tsx`
   - Code snippets showing relevant patterns
   - Architecture context
   - Dependencies and integration points

### Phase 5: Validation (Confirm Completeness)

Before finishing, verify:
- [ ] User's request is fully understood
- [ ] All clarifying questions answered
- [ ] Technical approach is clear and feasible
- [ ] Specific file locations and paths included
- [ ] Dependencies and risks identified
- [ ] Priority and effort estimate set
- [ ] Plan file created in `backlog/` folder
- [ ] User informed that plan is ready for implementation

## File Naming Convention

Use numbered, descriptive, kebab-case names with type prefix:

**Format**: `[NNN]-[TYPE]-[short-description].md`

### Task Numbering - CRITICAL

**ALWAYS scan ALL folders to find the next task number:**

```
1. Glob pattern: .task-board/**/*.md
2. Scan: backlog/, in-progress/, done/, AND on-hold/
3. Extract numbers from filenames (e.g., 003-FEATURE-xxx.md -> 003)
4. Find highest number across ALL folders
5. Next task = highest + 1
```

**Why include `done/`**: Completed tasks retain their numbers. Reusing numbers breaks history tracking and causes confusion.

### Type Prefixes

- `FEATURE-` - New functionality
- `BUG-` - Bug fixes
- `REFACTOR-` - Code improvements
- `TEST-` - Testing additions
- `SECURITY-` - Security work
- `PERF-` - Performance improvements
- `DESIGN-` - Design/styling work
- `DOCS-` - Documentation
- `EPIC-` - Major multi-phase features
- `EXPLORE-` - Research/investigation
- `CLEANUP-` - Code cleanup
- `A11Y-` - Accessibility improvements
- `QUALITY-` - Code quality improvements

## Project Context

### Technology Stack

- **Framework**: React 18+ with Vite
- **Language**: TypeScript (strict mode)
- **Styling**: Styled Components
- **Package manager**: npm

### Architecture Patterns

**Project Structure**:
```
src/
  components/    # Reusable UI components
  pages/         # Page-level components
  hooks/         # Custom React hooks
  utils/         # Utility functions
  types/         # Shared TypeScript types
  assets/        # Static assets (images, fonts)
  App.tsx        # Root component
  main.tsx       # Entry point
```

**Conventions**:
- Functional components with hooks (no class components)
- `const` arrow functions for components
- Named exports over default exports
- Styled components co-located, prefixed with `Styled`
- ~150 line max per component file
- No `any` - use `unknown` or proper types
- Early returns over nested conditionals

## Enhanced Plan Template

Use this comprehensive template for all plan files. Fill in ALL sections based on research:

```markdown
# [Type]: [Short Description]

**Status**: Backlog
**Created**: [YYYY-MM-DD]
**Priority**: [High/Medium/Low]
**Labels**: [components, pages, hooks, utils, styling, etc.]
**Estimated Effort**: [Simple/Medium/Complex - X days/weeks]

## Context & Motivation

[Why this work is needed - business value, user need, or technical debt]

## Current State

[What exists today - relevant background, current implementation]

## Desired Outcome

[What we want to achieve after this is complete - specific goals]

## Affected Components

### Files to Create
- [Specific file paths]

### Files to Modify
- [Specific file paths]

### Dependencies
- **External**: [npm packages needed]
- **Internal**: [Other features/components this depends on]
- **Blocking**: [Other tasks that must be completed first]

## Technical Approach

### Architecture Decisions

[Key architectural choices and rationale]

### Implementation Steps

1. **[Phase 1: Core Implementation]**
   - Files to create: [specific paths]
   - Files to modify: [specific paths]
   - Key changes: [what needs to be done]

2. **[Phase 2: Testing & Verification]**
   - How to verify the implementation works

3. **[Phase 3: Polish]**
   - Edge cases, error handling, responsive design

### Risks & Considerations

- **Risk 1**: [What could go wrong] - **Mitigation**: [How to address]
- **Performance**: [Any performance concerns]

## Before / After Examples

Show concrete code comparisons to clarify the intended changes. Include these liberally — they make the plan actionable and unambiguous.

### Example 1: [Brief description]

**Before** (`path/to/File.cs`):
```csharp
// Current code that will change
```

**After**:
```csharp
// How it should look after implementation
```

### Example 2: [Brief description]
[Add more before/after pairs as needed]

## Code References

### Relevant Existing Code

[Point to existing code that follows similar patterns]

### Similar Patterns

[Examples of patterns to follow in the codebase]

## Design Notes

### UI/UX Considerations (if applicable)
- Styled Components to create
- Responsive design needs
- Theme tokens to use

## Progress Log

- [YYYY-MM-DD] - Task created

## Resolution

[This section added when complete - summary of implementation]

## Acceptance Criteria

- [ ] [Specific, measurable criterion 1]
- [ ] [Specific, measurable criterion 2]
- [ ] [Specific, measurable criterion 3]

---

**Next Steps**: Ready for implementation. Move to `.task-board/in-progress/` when starting work.
```

## Best Practices

### Research Quality
1. **Thorough exploration**: Search multiple ways (keywords, file patterns, component names)
2. **Read, don't skim**: Actually read files to understand patterns
3. **Follow the trail**: Find imports, usages, related files
4. **Check similar features**: Learn from existing implementations

### Documentation Quality
1. **Be specific**: Use exact file paths, not "the component code"
2. **Include evidence**: Show code snippets, not just descriptions
3. **Show before/after liberally**: Concrete code diffs are worth more than paragraphs of prose — include them for every non-trivial change
4. **Quantify scope**: "Affects 3 components" vs "affects components"
5. **Link everything**: Cross-reference files, plans, documentation

## Limitations and Boundaries

### What This Skill Does
- Creates structured implementation plans
- Researches codebase and identifies patterns
- Designs technical approaches
- Breaks down complex work into phases
- Identifies dependencies and risks
- Documents architecture decisions
- Asks clarifying questions

### What This Skill Does NOT Do
- Implement code or write features
- Modify existing files (except creating plan files)
- Run tests or execute commands
- Create pull requests
- Move plans between folders (stays in backlog)

## Handoff to Implementation

After creating a plan file, inform the user:

```
Plan documented: .task-board/backlog/[PLAN-NAME].md

Next steps:
1. Review the plan to ensure accuracy and completeness
2. Add to PLANNING-BOARD.md if this is a top priority (max 3-5 items)
3. When ready to implement, move file to .task-board/in-progress/
4. Implement the feature following the plan
5. Move to .task-board/done/ when complete
```

## Integration with Workflow

This skill creates plans in `backlog/` folder. The implementation workflow then:
1. Adds plan to PLANNING-BOARD.md if it's a priority (max 3-5 items)
2. Moves file to `in-progress/` when starting work
3. Adds detailed implementation breakdown
4. Updates progress log during work
5. Moves to `done/` when complete
6. Updates PLANNING-BOARD.md to remove completed item

## See Also

- [`.task-board/README.md`](../../../.task-board/README.md) - Board overview and workflow
- [`.task-board/PLANNING-BOARD.md`](../../../.task-board/PLANNING-BOARD.md) - Current priorities
- [`.claude/CLAUDE.md`](../../CLAUDE.md) - Project-wide instructions
