# QUALITY: Title Truncation with Ellipsis Indicator

**Status**: Backlog
**Created**: 2026-03-05
**Priority**: Medium
**Labels**: quality, tui

## Context & Motivation

When task titles are too long for the column width, they're silently truncated with no visual indicator. Users can't tell if a title was cut off. Adding "…" at the end of truncated titles improves readability.

## Acceptance Criteria

- [ ] TaskCard title truncation appends "…" when text is cut off
- [ ] TaskDetailPanel field values show "…" when truncated
- [ ] Truncation leaves room for the ellipsis character (doesn't cut one extra char)
- [ ] All existing tests pass

## Progress Log

- 2026-03-05 - Task created from audit findings
