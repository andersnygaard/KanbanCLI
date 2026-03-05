# QUALITY: Add Integration Tests for File I/O Operations

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, tests, storage

## Context & Motivation

Current tests mock IFileSystem. We need integration-style tests that verify actual file creation, moves, and deletions using a real (temporary) directory to catch issues that mocking might miss.

## Desired Outcome

Integration tests using a temporary directory that verify end-to-end file operations for task CRUD and moves.

## Acceptance Criteria

- [x] Create integration test class for MarkdownTaskRepository using real temp directory
- [x] Test: Create a task file, verify it exists on disk with correct content
- [x] Test: Move a task file between folders, verify source removed and target exists
- [x] Test: Delete a task file, verify it's removed from disk
- [x] Test: GetNextId with real files returns correct next ID
- [x] Test: Roundtrip — create task, read it back, verify all fields preserved
- [x] Tests clean up temp directories after execution

## Technical Approach

- Use `Path.GetTempPath()` and unique subdirectory for each test
- Create a real `FileSystem` instance and `MarkdownTaskRepository`
- Use `[Fact]` tests with proper setup/teardown via IDisposable
- Verify file contents using File.ReadAllText assertions

## Progress Log

- 2026-03-05 - Task created from backlog scan round 4
