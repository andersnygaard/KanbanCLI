# QUALITY: Parser Robustness for Malformed Input

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, tests, storage

## Context & Motivation

The markdown parser handles most edge cases gracefully but lacks tests for truly malformed input: files with no title heading, files with only whitespace, files with duplicate metadata keys, files with markdown inside metadata values. These tests ensure the parser doesn't crash on user-edited files.

## Desired Outcome

Parser gracefully handles all forms of malformed markdown input without throwing exceptions.

## Technical Approach

- Add tests to MarkdownParserTests.cs
- Ensure parser returns default TaskItem values rather than throwing
- Verify ParseFileName handles missing components gracefully

## Progress Log

- 2026-03-05 - Task created from backlog scan round 5

## Acceptance Criteria

- [x] Test: Parse file with no # heading returns sensible defaults
- [x] Test: Parse file with only whitespace returns defaults
- [x] Test: Parse file with duplicate metadata keys uses last value
- [x] Test: Parse file with markdown formatting in metadata values handles correctly
- [x] Test: Parse completely empty file returns defaults
- [x] Test: ParseFileName with malformed filename returns sensible defaults
- [x] All tests pass
