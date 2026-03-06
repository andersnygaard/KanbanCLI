# CLEANUP: Remove Dead Code and Unused Methods

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Low
**Labels**: cleanup, quality

## Context & Motivation

The audit identified unused methods and dead code:
- `TaskDetailPanel.RenderSectionContent()` — replaced by `BuildContentLines` but may still exist
- Any other unused private methods left over from refactoring iterations
- The comment "// RenderMetadataFields and RenderSections replaced by BuildContentLines above" should be removed

## Progress Log

- 2026-03-05 - Task created from audit findings

## Acceptance Criteria

- [ ] Remove any unused private methods in TUI classes
- [ ] Remove stale comments referencing removed code
- [ ] Verify no compilation warnings about unused members
- [ ] All existing tests pass
