using FluentAssertions;
using KanbanCli.Models;
using KanbanCli.Storage;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests.Storage;

public class MarkdownParserTests
{
    private readonly MarkdigMarkdownParser _parser = new();

    private const string FullTaskMarkdown = """
        # FEATURE: Short title

        **Status**: InProgress
        **Created**: 2026-03-04
        **Priority**: High
        **Labels**: frontend, api

        ## Context & Motivation
        Some context here.

        ## Acceptance Criteria
        - [ ] First item
        - [x] Second item

        ## Progress Log
        - 2026-03-04 - Created
        """;

    private const string MinimalTaskMarkdown = """
        # BUG: Simple bug

        **Status**: Backlog
        **Created**: 2026-01-01

        ## Progress Log
        - 2026-01-01 - Created
        """;

    private const string ExtraMetadataMarkdown = """
        # FEATURE: Task with extra metadata

        **Status**: Backlog
        **Created**: 2026-03-04
        **Priority**: High
        **Labels**: storage, core
        **Estimated Effort**: Medium - 2-3 days
        **Reviewer**: Alice

        ## Progress Log
        - 2026-03-04 - Created
        """;

    private const string MultipleSectionsMarkdown = """
        # FEATURE: Task with multiple sections

        **Status**: Backlog
        **Created**: 2026-03-04
        **Priority**: Medium
        **Labels**: core

        ## Context & Motivation
        Why this task exists.

        ## Acceptance Criteria
        - [ ] First requirement
        - [ ] Second requirement

        ## Technical Approach
        Implementation details here.

        ## Progress Log
        - 2026-03-04 - Task created
        """;

    [Fact]
    public void Parse_FullTaskFile_ReturnsCorrectTaskItem()
    {
        var result = _parser.Parse(FullTaskMarkdown, 42, TaskType.Feature);

        result.Id.Should().Be(42);
        result.Title.Should().Be("Short title");
        result.Type.Should().Be(TaskType.Feature);
        result.Status.Should().Be(TaskStatus.InProgress);
        result.Priority.Should().Be(Priority.High);
        result.Labels.Should().BeEquivalentTo(new[] { "frontend", "api" });
        result.CreatedDate.Should().Be(new DateTime(2026, 3, 4));
    }

    [Fact]
    public void Parse_MinimalFile_DefaultsMissingFields()
    {
        var result = _parser.Parse(MinimalTaskMarkdown, 1, TaskType.Bug);

        result.Id.Should().Be(1);
        result.Title.Should().Be("Simple bug");
        result.Type.Should().Be(TaskType.Bug);
        result.Status.Should().Be(TaskStatus.Backlog);
        result.Priority.Should().Be(Priority.Medium);
        result.Labels.Should().BeEmpty();
    }

    [Fact]
    public void Parse_UnknownType_HandlesGracefully()
    {
        var markdown = """
            # UNKNOWN: Some task

            **Status**: Backlog
            **Created**: 2026-03-04
            **Priority**: Low
            **Labels**:
            """;

        var act = () => _parser.Parse(markdown, 99, TaskType.Explore);

        act.Should().NotThrow();
        var result = _parser.Parse(markdown, 99, TaskType.Explore);
        result.Id.Should().Be(99);
        result.Title.Should().Be("Some task");
    }

    [Fact]
    public void Parse_ExtraMetadata_PreservesUnknownFields()
    {
        var result = _parser.Parse(ExtraMetadataMarkdown, 1, TaskType.Feature);

        result.ExtraMetadata.Should().ContainKey("Estimated Effort");
        result.ExtraMetadata["Estimated Effort"].Should().Be("Medium - 2-3 days");
    }

    [Fact]
    public void Parse_ExtraMetadata_DoesNotIncludeKnownFields()
    {
        var result = _parser.Parse(ExtraMetadataMarkdown, 1, TaskType.Feature);

        result.ExtraMetadata.Should().NotContainKey("Status");
        result.ExtraMetadata.Should().NotContainKey("Priority");
        result.ExtraMetadata.Should().NotContainKey("Labels");
        result.ExtraMetadata.Should().NotContainKey("Created");
    }

    [Fact]
    public void Parse_ExtraMetadata_PreservesMultipleUnknownFields()
    {
        var result = _parser.Parse(ExtraMetadataMarkdown, 1, TaskType.Feature);

        result.ExtraMetadata.Should().ContainKey("Reviewer");
        result.ExtraMetadata["Reviewer"].Should().Be("Alice");
    }

    [Fact]
    public void Parse_Sections_ExtractsAllHeadings()
    {
        var result = _parser.Parse(MultipleSectionsMarkdown, 1, TaskType.Feature);

        result.Sections.Should().ContainKey("Context & Motivation");
        result.Sections.Should().ContainKey("Acceptance Criteria");
        result.Sections.Should().ContainKey("Technical Approach");
        result.Sections.Should().ContainKey("Progress Log");
    }

    [Fact]
    public void Parse_Sections_CapturesSectionContent()
    {
        var result = _parser.Parse(MultipleSectionsMarkdown, 1, TaskType.Feature);

        result.Sections["Context & Motivation"].Should().Contain("Why this task exists.");
    }

    [Fact]
    public void Parse_Sections_CapturesAcceptanceCriteriaContent()
    {
        var result = _parser.Parse(MultipleSectionsMarkdown, 1, TaskType.Feature);

        result.Sections["Acceptance Criteria"].Should().Contain("First requirement");
        result.Sections["Acceptance Criteria"].Should().Contain("Second requirement");
    }

    [Fact]
    public void Serialize_TaskItem_ProducesValidMarkdown()
    {
        var task = new TaskItem
        {
            Id = 5,
            Title = "My feature",
            Type = TaskType.Feature,
            Status = TaskStatus.Backlog,
            Priority = Priority.High,
            Labels = new[] { "backend", "api" }.ToList().AsReadOnly(),
            CreatedDate = new DateTime(2026, 3, 4)
        };

        var result = _parser.Serialize(task);

        result.Should().Contain("# FEATURE: My feature");
        result.Should().Contain("**Status**: Backlog");
        result.Should().Contain("**Priority**: High");
        result.Should().Contain("**Labels**: backend, api");
        result.Should().Contain("**Created**: 2026-03-04");
        result.Should().Contain("## Context & Motivation");
        result.Should().Contain("## Acceptance Criteria");
        result.Should().Contain("## Progress Log");
    }

    [Fact]
    public void Serialize_WithExtraMetadata_OutputsExtraFields()
    {
        var task = new TaskItem
        {
            Id = 1,
            Title = "Task with extras",
            Type = TaskType.Feature,
            Status = TaskStatus.Backlog,
            Priority = Priority.Medium,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 4),
            ExtraMetadata = new Dictionary<string, string>
            {
                ["Estimated Effort"] = "Medium - 2-3 days",
                ["Reviewer"] = "Alice"
            }
        };

        var result = _parser.Serialize(task);

        result.Should().Contain("**Estimated Effort**: Medium - 2-3 days");
        result.Should().Contain("**Reviewer**: Alice");
    }

    [Fact]
    public void Serialize_WithSections_OutputsSectionContent()
    {
        var task = new TaskItem
        {
            Id = 1,
            Title = "Task with sections",
            Type = TaskType.Feature,
            Status = TaskStatus.Backlog,
            Priority = Priority.Medium,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 4),
            Sections = new Dictionary<string, string>
            {
                ["Context & Motivation"] = "Why this task exists.",
                ["Progress Log"] = "- 2026-03-04 - Created"
            }
        };

        var result = _parser.Serialize(task);

        result.Should().Contain("## Context & Motivation");
        result.Should().Contain("Why this task exists.");
        result.Should().Contain("## Progress Log");
        result.Should().Contain("- 2026-03-04 - Created");
    }

    [Fact]
    public void Roundtrip_ParseThenSerialize_PreservesContent()
    {
        var original = new TaskItem
        {
            Id = 10,
            Title = "Roundtrip task",
            Type = TaskType.Refactor,
            Status = TaskStatus.InProgress,
            Priority = Priority.Medium,
            Labels = new[] { "core" }.ToList().AsReadOnly(),
            CreatedDate = new DateTime(2026, 3, 4)
        };

        var serialized = _parser.Serialize(original);
        var reparsed = _parser.Parse(serialized, original.Id, original.Type);

        reparsed.Id.Should().Be(original.Id);
        reparsed.Title.Should().Be(original.Title);
        reparsed.Type.Should().Be(original.Type);
        reparsed.Status.Should().Be(original.Status);
        reparsed.Priority.Should().Be(original.Priority);
        reparsed.Labels.Should().BeEquivalentTo(original.Labels);
        reparsed.CreatedDate.Date.Should().Be(original.CreatedDate.Date);
    }

    [Fact]
    public void Roundtrip_WithSectionsAndExtraMetadata_PreservesAll()
    {
        var markdown = """
            # FEATURE: Full roundtrip task

            **Status**: InProgress
            **Created**: 2026-03-04
            **Priority**: High
            **Labels**: storage, core
            **Estimated Effort**: Medium - 2-3 days

            ## Context & Motivation
            Why this task exists and what problem it solves.

            ## Acceptance Criteria
            - [ ] First requirement
            - [x] Second requirement

            ## Progress Log
            - 2026-03-04 - Task created
            """;

        var parsed = _parser.Parse(markdown, 7, TaskType.Feature);
        var serialized = _parser.Serialize(parsed);
        var reparsed = _parser.Parse(serialized, 7, TaskType.Feature);

        reparsed.ExtraMetadata.Should().ContainKey("Estimated Effort");
        reparsed.ExtraMetadata["Estimated Effort"].Should().Be("Medium - 2-3 days");
        reparsed.Sections.Should().ContainKey("Context & Motivation");
        reparsed.Sections["Context & Motivation"].Should().Contain("Why this task exists");
        reparsed.Sections.Should().ContainKey("Acceptance Criteria");
        reparsed.Sections.Should().ContainKey("Progress Log");
    }

    [Fact]
    public void ParseFileName_ExtractsIdTypeAndDescription()
    {
        var (id, type, description) = _parser.ParseFileName("003-FEATURE-markdown-storage.md");

        id.Should().Be(3);
        type.Should().Be(TaskType.Feature);
        description.Should().Be("markdown-storage");
    }

    [Fact]
    public void ParseFileName_WithFullPath_ExtractsCorrectly()
    {
        var (id, type, description) = _parser.ParseFileName("/some/path/001-BUG-fix-login.md");

        id.Should().Be(1);
        type.Should().Be(TaskType.Bug);
        description.Should().Be("fix-login");
    }
}
