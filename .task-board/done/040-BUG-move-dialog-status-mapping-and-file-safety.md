# BUG: MoveDialog Status Mapping and File Safety in Repository Move

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: bug, storage, tui

## Context & Motivation

The audit identified two critical issues:

1. **MoveDialog** hardcodes a status map of 4 entries but assumes the board always has 4 columns. If the board columns differ, the mapping breaks.

2. **MarkdownTaskRepository.Move()** writes the target file first then deletes the source. If the write succeeds but delete fails, you get duplicate tasks. The file operations should be safer.

## Acceptance Criteria

- [ ] MoveDialog uses the actual board columns to build its selection list dynamically
- [ ] MoveDialog excludes the current column from move targets
- [ ] MarkdownTaskRepository.Move() uses safe file operations (write target, verify, then delete source)
- [ ] Add error handling if target file already exists (don't silently overwrite)
- [ ] All existing tests pass

## Technical Approach

### MoveDialog fix

Read and update MoveDialog to take the board's actual columns and build the list from them, filtering out the current column.

### Repository Move safety

In `MarkdownTaskRepository.Move()`:
1. Write to target path
2. Verify target exists via `_fileSystem.FileExists()`
3. Only then delete source
4. If target already exists before write, throw or handle gracefully

## Progress Log

- 2026-03-05 - Task created from audit findings
