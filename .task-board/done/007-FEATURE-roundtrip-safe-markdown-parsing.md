# FEATURE: Roundtrip-Safe Markdown Parsing (ExtraMetadata & Sections)

**Status**: Done
**Created**: 2026-03-04
**Priority**: High
**Labels**: storage, models, core
**Estimated Effort**: Medium - 2-3 days

## Context & Motivation

The spec explicitly defines two critical properties on `TaskItem` that are not implemented: `ExtraMetadata` (preserves unknown metadata fields like "Estimated Effort") and `Sections` (preserves all `## Heading` body sections dynamically). The spec states:

> "Parseren skal ikke hardkode seksjonsnavn. Alle `## Heading`-seksjoner leses inn dynamisk og lagres i `TaskItem.Sections`. Dette gjor formatet utvidbart uten kodeendringer."

Without these, the parser silently discards data when round-tripping task files. This is a data loss bug that affects every read-write cycle. The Serialize method currently hardcodes empty section stubs ("## Context & Motivation", "## Acceptance Criteria", "## Progress Log") and ignores the actual content.

## Current State

- `TaskItem` record has no `ExtraMetadata` or `Sections` properties
- `MarkdigMarkdownParser.Parse()` extracts only Status, Priority, Labels, Created -- all other metadata fields are discarded
- `MarkdigMarkdownParser.Parse()` does not extract `## Heading` body sections at all
- `MarkdigMarkdownParser.Serialize()` hardcodes three empty section stubs, discarding original content
- The roundtrip test (`Roundtrip_ParseThenSerialize_PreservesContent`) does not verify section content preservation because sections are not implemented
- `TaskDetailPanel.Show()` only displays metadata fields, not body sections

## Desired Outcome

Full roundtrip fidelity: parse a task file, serialize it back, and the content is preserved. Unknown metadata fields and all body sections survive the round trip. This makes the format extensible without code changes.

## Acceptance Criteria

- [x] `TaskItem` record gains `ExtraMetadata` property: `IReadOnlyDictionary<string, string>`
- [x] `TaskItem` record gains `Sections` property: `IReadOnlyDictionary<string, string>`
- [x] `MarkdigMarkdownParser.Parse()` collects unknown metadata fields into `ExtraMetadata`
- [x] `MarkdigMarkdownParser.Parse()` collects all `## Heading` sections dynamically into `Sections` (heading name as key, content as value)
- [x] `MarkdigMarkdownParser.Serialize()` outputs ExtraMetadata fields after known metadata
- [x] `MarkdigMarkdownParser.Serialize()` outputs all Sections in order (not hardcoded stubs)
- [x] Roundtrip test verifies section content and extra metadata are preserved
- [x] New tests: `Parse_ExtraMetadata_PreservesUnknownFields`, `Parse_Sections_ExtractsAllHeadings`, `Roundtrip_WithSectionsAndExtraMetadata_PreservesAll`
- [x] Existing tests continue to pass
- [x] `TaskDetailPanel.Show()` displays body sections (at minimum Context & Motivation)

## Affected Components

### Files to Modify
- `src/KanbanCli/Models/TaskItem.cs` -- add ExtraMetadata and Sections properties with empty defaults
- `src/KanbanCli/Storage/MarkdigMarkdownParser.cs` -- update Parse() and Serialize() for full roundtrip
- `src/KanbanCli/Tui/TaskDetailPanel.cs` -- display section content
- `src/KanbanCli.Tests/Storage/MarkdownParserTests.cs` -- add roundtrip tests for sections and extra metadata

### Dependencies
- **External**: None (Markdig already available)
- **Internal**: Models layer (TaskItem), Storage layer (MarkdigMarkdownParser)
- **Blocking**: None -- all prerequisite layers exist

## Technical Approach

### Architecture Decisions

- **Ordered dictionary for Sections**: Use `IReadOnlyList<KeyValuePair<string, string>>` or `IReadOnlyDictionary<string, string>` (backed by insertion-ordered dictionary) to preserve section order
- **Known vs unknown metadata**: Status, Priority, Labels, Created are "known" fields parsed into typed properties. Everything else goes to ExtraMetadata.
- **Section extraction via Markdig AST**: Walk the document, detect `HeadingBlock` with level 2, capture all content between consecutive level-2 headings as section body text

### Implementation Steps

1. **Update TaskItem.cs**
   - Add `IReadOnlyDictionary<string, string> ExtraMetadata { get; init; } = new Dictionary<string, string>();`
   - Add `IReadOnlyDictionary<string, string> Sections { get; init; } = new Dictionary<string, string>();`

2. **Update MarkdigMarkdownParser.Parse()**
   - After extracting known metadata keys, put remaining key-value pairs into ExtraMetadata
   - After metadata, walk the AST for all level-2 HeadingBlocks
   - For each heading, capture the text between it and the next level-2 heading (or end of document)
   - Store in a dictionary: heading text -> body content

3. **Update MarkdigMarkdownParser.Serialize()**
   - After known metadata lines, output each ExtraMetadata entry as `**Key**: Value`
   - Instead of hardcoded section stubs, iterate Sections dictionary and output `## Heading\n\nContent`
   - If Sections is empty, output minimal default sections (for new tasks)

4. **Update TaskDetailPanel.Show()**
   - After metadata fields, render each section heading and content

5. **Add/update tests**
   - `Parse_ExtraMetadata_PreservesUnknownFields` -- parse markdown with "Estimated Effort" field
   - `Parse_Sections_ExtractsAllHeadings` -- verify all ## headings captured with content
   - `Roundtrip_WithSectionsAndExtraMetadata_PreservesAll` -- full roundtrip including extra fields

### Risks & Considerations

- **Risk**: Section content extraction from Markdig AST can be tricky (need to capture raw text between headings) -- **Mitigation**: Use line-based extraction from the original markdown string, using heading positions from the AST as boundaries
- **Risk**: Existing tests may break if Serialize output changes -- **Mitigation**: Update assertions to match new output format
- **Readability**: Keep the section extraction method small and well-named

## Code References

### From specs.md -- TaskItem definition
```csharp
// Ekstra metadata (Estimated Effort, etc.) -- bevares ved roundtrip
public IReadOnlyDictionary<string, string> ExtraMetadata { get; init; }

// Body-seksjoner: nokkel = heading, verdi = innhold
// Bevarer alle seksjoner (Context, Acceptance Criteria, Technical Approach, osv.)
public IReadOnlyDictionary<string, string> Sections { get; init; }
```

### From specs.md -- Markdown parsing requirements
> "Ukjente metadata-felter (f.eks. Estimated Effort) lagres i TaskItem.ExtraMetadata -- bevares ved roundtrip"
> "Ekstraher alle ## Heading-seksjoner dynamisk via AST -- lagres i TaskItem.Sections"
> "Ikke hardkode seksjonsnavn -- nye seksjoner handteres automatisk"
> "Serialiser tilbake til markdown ved endringer -- bevar formatering og ukjente felter"

## Progress Log

- 2026-03-04 - Task created via backlog-scan
- 2026-03-05 - Implemented: ExtraMetadata and Sections on TaskItem, full roundtrip in MarkdigMarkdownParser, section display in TaskDetailPanel, 9 new tests added. All 49 tests pass.