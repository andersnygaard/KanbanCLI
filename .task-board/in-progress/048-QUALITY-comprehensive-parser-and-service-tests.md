# QUALITY: Comprehensive Parser and Service Test Coverage

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, tests

## Context & Motivation

Add test coverage for areas identified in the audit but not yet covered:
- BoardService: empty board, single column, planning board with no completed tasks
- Parser: Unicode titles, CRLF line endings, missing sections
- TaskItem: GenerateFileName with special characters edge cases

## Acceptance Criteria

- [ ] Add BoardService test: GetBoard_EmptyRepository_ReturnsEmptyColumns
- [ ] Add Parser test: Parse_CrlfLineEndings_HandlesCorrectly
- [ ] Add Parser test: Parse_UnicodeTitle_PreservesContent
- [ ] Add TaskItem test: GenerateFileName_SpecialCharacters_SanitizesCorrectly
- [ ] All existing tests pass

## Progress Log

- 2026-03-05 - Task created
