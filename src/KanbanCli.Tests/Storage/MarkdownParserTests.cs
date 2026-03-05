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

    [Fact]
    public void ParseFileName_InvalidFormat_ReturnsSensibleDefaults()
    {
        var (id, type, description) = _parser.ParseFileName("not-a-valid-filename.txt");

        id.Should().Be(0);
        type.Should().Be(TaskType.Feature);
        description.Should().BeEmpty();
    }

    [Fact]
    public void ParseFileName_UnknownTaskType_ReturnsDefaultType()
    {
        var (id, type, description) = _parser.ParseFileName("001-UNKNOWNTYPE-some-desc.md");

        id.Should().Be(1);
        type.Should().Be(TaskType.Feature);
        description.Should().Be("some-desc");
    }

    [Fact]
    public void Parse_UnknownStatus_ThrowsFormatException()
    {
        var markdown = """
            # FEATURE: Test task

            **Status**: InvalidStatus
            **Created**: 2026-03-04
            **Priority**: Medium
            **Labels**:
            """;

        var act = () => _parser.Parse(markdown, 1, TaskType.Feature);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Parse_InvalidDateFormat_DefaultsToUtcNow()
    {
        var markdown = """
            # FEATURE: Test task

            **Status**: Backlog
            **Created**: not-a-date
            **Priority**: Medium
            **Labels**:
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Parse_MissingCreatedDate_DefaultsToUtcNow()
    {
        var markdown = """
            # FEATURE: Test task

            **Status**: Backlog
            **Priority**: Medium
            **Labels**:
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Parse_EmptyMarkdown_ReturnsTaskWithDefaults()
    {
        var markdown = "";

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.Id.Should().Be(1);
        result.Title.Should().BeEmpty();
        result.Status.Should().Be(TaskStatus.Backlog);
        result.Priority.Should().Be(Priority.Medium);
        result.Labels.Should().BeEmpty();
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Parse_MissingPriority_DefaultsToMedium()
    {
        var markdown = """
            # BUG: No priority

            **Status**: Backlog
            **Created**: 2026-03-04
            **Labels**:
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Bug);

        result.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public void Roundtrip_ParseSerializeParse_ProducesIdenticalTaskItem()
    {
        var markdown = """
            # FEATURE: Full roundtrip identity test

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

            ## Technical Approach
            Implementation details here.

            ## Progress Log
            - 2026-03-04 - Task created
            """;

        var firstParse = _parser.Parse(markdown, 7, TaskType.Feature);
        var serialized = _parser.Serialize(firstParse);
        var secondParse = _parser.Parse(serialized, 7, TaskType.Feature);

        secondParse.Id.Should().Be(firstParse.Id);
        secondParse.Title.Should().Be(firstParse.Title);
        secondParse.Type.Should().Be(firstParse.Type);
        secondParse.Status.Should().Be(firstParse.Status);
        secondParse.Priority.Should().Be(firstParse.Priority);
        secondParse.Labels.Should().BeEquivalentTo(firstParse.Labels);
        secondParse.CreatedDate.Date.Should().Be(firstParse.CreatedDate.Date);
        secondParse.ExtraMetadata.Should().BeEquivalentTo(firstParse.ExtraMetadata);
        secondParse.Sections.Keys.Should().BeEquivalentTo(firstParse.Sections.Keys);

        foreach (var key in firstParse.Sections.Keys)
        {
            secondParse.Sections[key].Trim().Should().Be(firstParse.Sections[key].Trim());
        }
    }

    [Fact]
    public void Serialize_TaskWithNoSections_DoesNotInjectSections()
    {
        var task = new TaskItem
        {
            Id = 1,
            Title = "No sections task",
            Type = TaskType.Bug,
            Status = TaskStatus.Backlog,
            Priority = Priority.Low,
            Labels = new[] { "test" }.ToList().AsReadOnly(),
            CreatedDate = new DateTime(2026, 3, 4),
            Sections = new Dictionary<string, string>()
        };

        var result = _parser.Serialize(task);

        result.Should().NotContain("## ");
    }

    [Fact]
    public void Serialize_TaskWithOnlyOneSection_DoesNotInjectOtherSections()
    {
        var task = new TaskItem
        {
            Id = 2,
            Title = "Single section task",
            Type = TaskType.Feature,
            Status = TaskStatus.InProgress,
            Priority = Priority.Medium,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 4),
            Sections = new Dictionary<string, string>
            {
                ["Progress Log"] = "- 2026-03-04 - Created"
            }
        };

        var result = _parser.Serialize(task);

        result.Should().Contain("## Progress Log");
        result.Should().NotContain("## Context & Motivation");
        result.Should().NotContain("## Acceptance Criteria");
        result.Should().NotContain("## Technical Approach");
    }

    [Fact]
    public void Roundtrip_InProgressStatus_PreservesHumanReadableFormat()
    {
        var task = new TaskItem
        {
            Id = 1,
            Title = "In progress roundtrip",
            Type = TaskType.Feature,
            Status = TaskStatus.InProgress,
            Priority = Priority.Medium,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 4)
        };

        var serialized = _parser.Serialize(task);

        serialized.Should().Contain("**Status**: In Progress");

        var reparsed = _parser.Parse(serialized, task.Id, task.Type);

        reparsed.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public void Roundtrip_OnHoldStatus_PreservesHumanReadableFormat()
    {
        var task = new TaskItem
        {
            Id = 2,
            Title = "On hold roundtrip",
            Type = TaskType.Bug,
            Status = TaskStatus.OnHold,
            Priority = Priority.Low,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 4)
        };

        var serialized = _parser.Serialize(task);

        serialized.Should().Contain("**Status**: On Hold");

        var reparsed = _parser.Parse(serialized, task.Id, task.Type);

        reparsed.Status.Should().Be(TaskStatus.OnHold);
    }

    [Fact]
    public void Parse_InvalidDate_DefaultsToUtcNow()
    {
        var markdown = """
            # FEATURE: Task with bad date

            **Status**: Backlog
            **Created**: 99-99-9999
            **Priority**: Medium
            **Labels**:
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Parse_GarbageDate_DefaultsToUtcNow()
    {
        var markdown = """
            # FEATURE: Task with garbage date

            **Status**: Backlog
            **Created**: lorem-ipsum
            **Priority**: Medium
            **Labels**:
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Parse_EmptySections_PreservesThem()
    {
        var markdown = """
            # FEATURE: Task with empty sections

            **Status**: Backlog
            **Created**: 2026-03-04
            **Priority**: Medium
            **Labels**:

            ## Context & Motivation

            ## Acceptance Criteria

            ## Progress Log
            - 2026-03-04 - Created
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.Sections.Should().ContainKey("Context & Motivation");
        result.Sections.Should().ContainKey("Acceptance Criteria");
        result.Sections.Should().ContainKey("Progress Log");
        result.Sections["Progress Log"].Should().Contain("2026-03-04 - Created");
    }

    [Fact]
    public void Parse_OnlyTitleNoMetadata_UsesDefaults()
    {
        var markdown = """
            # FEATURE: Just a title
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.Id.Should().Be(1);
        result.Title.Should().Be("Just a title");
        result.Status.Should().Be(TaskStatus.Backlog);
        result.Priority.Should().Be(Priority.Medium);
        result.Labels.Should().BeEmpty();
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.ExtraMetadata.Should().BeEmpty();
        result.Sections.Should().BeEmpty();
    }

    // --- Robustness tests for malformed input ---

    [Fact]
    public void Parse_NoHeading_ReturnsSensibleDefaults()
    {
        var markdown = """
            **Status**: Backlog
            **Created**: 2026-03-04
            **Priority**: High
            **Labels**: core

            ## Some Section
            Content here.
            """;

        var result = _parser.Parse(markdown, 5, TaskType.Bug);

        result.Id.Should().Be(5);
        result.Title.Should().BeEmpty();
        result.Type.Should().Be(TaskType.Bug);
        result.Status.Should().Be(TaskStatus.Backlog);
        result.Priority.Should().Be(Priority.High);
        result.Labels.Should().BeEquivalentTo(new[] { "core" });
        result.CreatedDate.Should().Be(new DateTime(2026, 3, 4));
        result.Sections.Should().ContainKey("Some Section");
    }

    [Fact]
    public void Parse_OnlyWhitespace_ReturnsSensibleDefaults()
    {
        var markdown = "   \n  \n\t\n   ";

        var result = _parser.Parse(markdown, 3, TaskType.Feature);

        result.Id.Should().Be(3);
        result.Title.Should().BeEmpty();
        result.Type.Should().Be(TaskType.Feature);
        result.Status.Should().Be(TaskStatus.Backlog);
        result.Priority.Should().Be(Priority.Medium);
        result.Labels.Should().BeEmpty();
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.ExtraMetadata.Should().BeEmpty();
        result.Sections.Should().BeEmpty();
    }

    [Fact]
    public void Parse_DuplicateMetadataKeys_UsesLastValue()
    {
        var markdown = """
            # FEATURE: Duplicate keys

            **Priority**: Low
            **Priority**: High
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public void Parse_MarkdownFormattingInMetadataValues_HandlesCorrectly()
    {
        var markdown = """
            # FEATURE: Formatted metadata

            **Status**: Backlog
            **Created**: 2026-03-04
            **Priority**: Medium
            **Labels**: core
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.Status.Should().Be(TaskStatus.Backlog);
        result.Priority.Should().Be(Priority.Medium);
        result.Labels.Should().Contain("core");
        result.CreatedDate.Should().Be(new DateTime(2026, 3, 4));
    }

    [Fact]
    public void Parse_CompletelyEmptyFile_ReturnsSensibleDefaults()
    {
        var markdown = "";

        var result = _parser.Parse(markdown, 10, TaskType.Explore);

        result.Id.Should().Be(10);
        result.Title.Should().BeEmpty();
        result.Type.Should().Be(TaskType.Explore);
        result.Status.Should().Be(TaskStatus.Backlog);
        result.Priority.Should().Be(Priority.Medium);
        result.Labels.Should().BeEmpty();
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.ExtraMetadata.Should().BeEmpty();
        result.Sections.Should().BeEmpty();
    }

    [Fact]
    public void ParseFileName_MalformedFilename_ReturnsSensibleDefaults()
    {
        var (id, type, description) = _parser.ParseFileName("random-garbage.txt");

        id.Should().Be(0);
        type.Should().Be(TaskType.Feature);
        description.Should().BeEmpty();
    }

    [Fact]
    public void Serialize_WithCompletedDate_IncludesCompletedField()
    {
        var task = new TaskItem
        {
            Id = 1,
            Title = "Done task",
            Type = TaskType.Feature,
            Status = TaskStatus.Done,
            Priority = Priority.Medium,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 1),
            CompletedDate = new DateTime(2026, 3, 5)
        };

        var result = _parser.Serialize(task);

        result.Should().Contain("**Completed**: 2026-03-05");
    }

    [Fact]
    public void Serialize_WithoutCompletedDate_OmitsCompletedField()
    {
        var task = new TaskItem
        {
            Id = 1,
            Title = "Pending task",
            Type = TaskType.Feature,
            Status = TaskStatus.Backlog,
            Priority = Priority.Medium,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 1)
        };

        var result = _parser.Serialize(task);

        result.Should().NotContain("**Completed**");
    }

    [Fact]
    public void Parse_WithCompletedDate_SetsCompletedDate()
    {
        var markdown = """
            # FEATURE: Completed task

            **Status**: Done
            **Created**: 2026-03-01
            **Completed**: 2026-03-05
            **Priority**: Medium
            **Labels**:
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.CompletedDate.Should().Be(new DateTime(2026, 3, 5));
    }

    [Fact]
    public void Parse_CompletedDate_NotInExtraMetadata()
    {
        var markdown = """
            # FEATURE: Completed task

            **Status**: Done
            **Created**: 2026-03-01
            **Completed**: 2026-03-05
            **Priority**: Medium
            **Labels**:
            """;

        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        result.ExtraMetadata.Should().NotContainKey("Completed");
    }

    [Fact]
    public void Roundtrip_CompletedDate_PreservesValue()
    {
        var original = new TaskItem
        {
            Id = 1,
            Title = "Roundtrip completed",
            Type = TaskType.Feature,
            Status = TaskStatus.Done,
            Priority = Priority.High,
            Labels = new[] { "core" }.ToList().AsReadOnly(),
            CreatedDate = new DateTime(2026, 3, 1),
            CompletedDate = new DateTime(2026, 3, 5)
        };

        var serialized = _parser.Serialize(original);
        var reparsed = _parser.Parse(serialized, original.Id, original.Type);

        reparsed.CompletedDate.Should().NotBeNull();
        reparsed.CompletedDate!.Value.Date.Should().Be(new DateTime(2026, 3, 5));
    }

    [Fact]
    public void Parse_AcceptanceCriteria_ParsesCheckboxes()
    {
        var result = _parser.Parse(FullTaskMarkdown, 42, TaskType.Feature);

        result.Sections.Should().ContainKey("Acceptance Criteria");
        var criteria = result.Sections["Acceptance Criteria"];
        criteria.Should().Contain("- [ ] First item");
        criteria.Should().Contain("- [x] Second item");
    }

    [Fact]
    public void ParseFileName_EmptyString_ReturnsSensibleDefaults()
    {
        var (id, type, description) = _parser.ParseFileName("");

        id.Should().Be(0);
        type.Should().Be(TaskType.Feature);
        description.Should().BeEmpty();
    }

    [Fact]
    public void ParseFileName_NoExtension_ReturnsSensibleDefaults()
    {
        var (id, type, description) = _parser.ParseFileName("just-a-name");

        id.Should().Be(0);
        type.Should().Be(TaskType.Feature);
        description.Should().BeEmpty();
    }

    [Fact]
    public void Parse_CrlfLineEndings_HandlesCorrectly()
    {
        // Arrange
        var markdown = "# FEATURE: CRLF task\r\n\r\n**Status**: InProgress\r\n**Created**: 2026-03-04\r\n**Priority**: High\r\n**Labels**: frontend, api\r\n\r\n## Context & Motivation\r\nSome context here.\r\n\r\n## Progress Log\r\n- 2026-03-04 - Created\r\n";

        // Act
        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        // Assert
        result.Title.Should().Be("CRLF task");
        result.Status.Should().Be(TaskStatus.InProgress);
        result.Priority.Should().Be(Priority.High);
        result.Labels.Should().BeEquivalentTo(new[] { "frontend", "api" });
        result.CreatedDate.Should().Be(new DateTime(2026, 3, 4));
        result.Sections.Should().ContainKey("Context & Motivation");
        result.Sections.Should().ContainKey("Progress Log");
    }

    [Fact]
    public void Parse_UnicodeTitle_PreservesContent()
    {
        // Arrange
        var markdown = """
            # FEATURE: Implémenter la résolution des données — café ☕

            **Status**: Backlog
            **Created**: 2026-03-04
            **Priority**: Medium
            **Labels**: i18n
            """;

        // Act
        var result = _parser.Parse(markdown, 1, TaskType.Feature);

        // Assert
        result.Title.Should().Contain("Implémenter");
        result.Title.Should().Contain("résolution");
        result.Title.Should().Contain("données");
        result.Title.Should().Contain("café");
        result.Labels.Should().Contain("i18n");
    }
}
