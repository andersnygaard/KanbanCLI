# QUALITY: Complete TaskType Color Mapping

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Low
**Labels**: quality, tui

## Context & Motivation

6 of 13 TaskType enum values (Design, Epic, Explore, Cleanup, A11y, Quality) fall through to the default gray color in TuiHelpers.GetTypeColor(). Each type should have a distinct color.

## Acceptance Criteria

- [ ] All 13 TaskType values have explicit color mappings
- [ ] All existing tests pass

## Progress Log

- 2026-03-05 - Task created
