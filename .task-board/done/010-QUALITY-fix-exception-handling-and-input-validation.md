# QUALITY: Fix Exception Handling and Add Input Validation

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: High
**Labels**: quality, storage, services

## Context & Motivation

The audit revealed bare `catch` blocks in `MarkdownTaskRepository.cs` and `MarkdigMarkdownParser.cs` that swallow all exceptions (including fatal ones). Additionally, there is no input validation for task titles, labels, or status transitions in the service layer. This hides bugs and data corruption.

## Desired Outcome

- All `catch` blocks catch specific exception types (e.g., `IOException`, `FormatException`)
- `TaskService.CreateTask()` validates title (non-empty, reasonable length, no invalid filename chars)
- `TaskService.MoveTask()` validates status transitions
- `ToKebabCase()` handles edge cases (empty/all-special-char titles)
- Status parsing logs or throws on unknown values instead of silently defaulting

## Acceptance Criteria

- [x] Replace bare `catch` blocks with specific exception types in MarkdownTaskRepository
- [x] Replace bare `catch` blocks with specific exception types in MarkdigMarkdownParser
- [x] Add title validation in TaskService (non-empty, max length, valid chars)
- [x] Add guard for ToKebabCase returning empty string
- [x] Add tests for all validation scenarios
- [x] Add tests for error handling edge cases in parser

## Technical Approach

- In `MarkdownTaskRepository.cs`, catch `IOException` and `UnauthorizedAccessException` specifically
- In `MarkdigMarkdownParser.cs`, catch `FormatException` for date parsing, etc.
- Add a `Validate()` method or use guard clauses in `TaskService.CreateTask()`
- Add `ToKebabCase` edge case handling in `TaskItem.GenerateFileName()`

## Progress Log

- 2026-03-05 - Task created from backlog scan round 1
