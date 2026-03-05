using FluentAssertions;
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests.Models;

public class TaskItemTests
{
    private static TaskItem CreateSampleTask(
        int id = 1,
        string title = "Sample Task",
        TaskType type = TaskType.Feature,
        Priority priority = Priority.Medium,
        TaskStatus status = TaskStatus.Backlog,
        IReadOnlyList<string>? labels = null,
        DateTime? createdDate = null,
        DateTime? completedDate = null)
    {
        return new TestTaskBuilder()
            .WithId(id)
            .WithTitle(title)
            .WithType(type)
            .WithPriority(priority)
            .WithStatus(status)
            .WithLabels(labels?.ToArray() ?? [])
            .WithCreatedDate(createdDate ?? DateTime.UtcNow)
            .WithCompletedDate(completedDate)
            .Build();
    }

    [Fact]
    public void ChangeStatus_ToDone_SetsCompletedDate()
    {
        // Arrange
        var task = CreateSampleTask(status: TaskStatus.Backlog);

        // Act
        var result = task.ChangeStatus(TaskStatus.Done);

        // Assert
        result.Status.Should().Be(TaskStatus.Done);
        result.CompletedDate.Should().NotBeNull();
        result.CompletedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ChangeStatus_FromDoneToBacklog_ClearsCompletedDate()
    {
        // Arrange
        var task = CreateSampleTask(status: TaskStatus.Done, completedDate: DateTime.UtcNow);

        // Act
        var result = task.ChangeStatus(TaskStatus.Backlog);

        // Assert
        result.Status.Should().Be(TaskStatus.Backlog);
        result.CompletedDate.Should().BeNull();
    }

    [Fact]
    public void AddLabel_NewLabel_AppendsToList()
    {
        // Arrange
        var task = CreateSampleTask(labels: ["existing-label"]);

        // Act
        var result = task.AddLabel("new-label");

        // Assert
        result.Labels.Should().HaveCount(2);
        result.Labels.Should().Contain("new-label");
    }

    [Fact]
    public void AddLabel_DuplicateLabel_DoesNotDuplicate()
    {
        // Arrange
        var task = CreateSampleTask(labels: ["frontend"]);

        // Act
        var result = task.AddLabel("frontend");

        // Assert
        result.Labels.Should().HaveCount(1);
    }

    [Fact]
    public void AddLabel_DuplicateLabelDifferentCase_DoesNotDuplicate()
    {
        // Arrange
        var task = CreateSampleTask(labels: ["Frontend"]);

        // Act
        var result = task.AddLabel("frontend");

        // Assert
        result.Labels.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveLabel_ExistingLabel_RemovesFromList()
    {
        // Arrange
        var task = CreateSampleTask(labels: ["frontend", "backend"]);

        // Act
        var result = task.RemoveLabel("frontend");

        // Assert
        result.Labels.Should().HaveCount(1);
        result.Labels.Should().NotContain("frontend");
        result.Labels.Should().Contain("backend");
    }

    [Fact]
    public void RemoveLabel_NonExistentLabel_ReturnsSameInstance()
    {
        // Arrange
        var task = CreateSampleTask(labels: ["frontend"]);

        // Act
        var result = task.RemoveLabel("backend");

        // Assert
        result.Should().BeSameAs(task);
        result.Labels.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveLabel_DifferentCase_RemovesCaseInsensitive()
    {
        // Arrange
        var task = CreateSampleTask(labels: ["Frontend", "api"]);

        // Act
        var result = task.RemoveLabel("frontend");

        // Assert
        result.Labels.Should().HaveCount(1);
        result.Labels.Should().NotContain("Frontend");
        result.Labels.Should().Contain("api");
    }

    [Fact]
    public void SetPriority_High_UpdatesPriority()
    {
        // Arrange
        var task = CreateSampleTask(priority: Priority.Low);

        // Act
        var result = task.SetPriority(Priority.High);

        // Assert
        result.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public void MatchesFilter_ByLabel_ReturnsTrue()
    {
        // Arrange
        var task = CreateSampleTask(labels: ["api", "backend"]);
        var filter = new FilterCriteria(Label: "api");

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesFilter_ByType_ReturnsTrue()
    {
        // Arrange
        var task = CreateSampleTask(type: TaskType.Bug);
        var filter = new FilterCriteria(Type: TaskType.Bug);

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesFilter_ByPriority_ReturnsTrue()
    {
        // Arrange
        var task = CreateSampleTask(priority: Priority.High);
        var filter = new FilterCriteria(Priority: Priority.High);

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesFilter_ByPriority_NoMatch_ReturnsFalse()
    {
        // Arrange
        var task = CreateSampleTask(priority: Priority.Low);
        var filter = new FilterCriteria(Priority: Priority.High);

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MatchesFilter_NoMatch_ReturnsFalse()
    {
        // Arrange
        var task = CreateSampleTask(type: TaskType.Feature, labels: ["frontend"]);
        var filter = new FilterCriteria(Type: TaskType.Bug);

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateFileName_GeneratesCorrectFormat()
    {
        // Arrange
        var task = CreateSampleTask(id: 5, title: "Add User Login", type: TaskType.Feature);

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("005-FEATURE-add-user-login.md");
    }

    [Fact]
    public void GenerateFileName_BugType_UsesUppercaseBugPrefix()
    {
        // Arrange
        var task = CreateSampleTask(id: 12, title: "Fix crash on save", type: TaskType.Bug);

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("012-BUG-fix-crash-on-save.md");
    }

    [Fact]
    public void ChangeStatus_NonDoneStatus_DoesNotSetCompletedDate()
    {
        // Arrange
        var task = CreateSampleTask(status: TaskStatus.Backlog);

        // Act
        var result = task.ChangeStatus(TaskStatus.InProgress);

        // Assert
        result.Status.Should().Be(TaskStatus.InProgress);
        result.CompletedDate.Should().BeNull();
    }

    [Fact]
    public void GenerateFileName_EmptyTitle_UsesUntitled()
    {
        // Arrange
        var task = CreateSampleTask(id: 1, title: "");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("001-FEATURE-untitled.md");
    }

    [Fact]
    public void GenerateFileName_AllSpecialCharacters_UsesUntitled()
    {
        // Arrange
        var task = CreateSampleTask(id: 2, title: "!@#$%^&*()");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("002-FEATURE-untitled.md");
    }

    [Fact]
    public void GenerateFileName_WhitespaceOnlyTitle_UsesUntitled()
    {
        // Arrange
        var task = CreateSampleTask(id: 3, title: "   ");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("003-FEATURE-untitled.md");
    }

    [Fact]
    public void GenerateFileName_TitleWithMixedSpecialChars_ProducesValidKebab()
    {
        // Arrange
        var task = CreateSampleTask(id: 4, title: "Fix--the  bug!!");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("004-FEATURE-fix-the-bug.md");
    }

    [Fact]
    public void MatchesFilter_CombinedLabelAndType_BothMatch_ReturnsTrue()
    {
        // Arrange
        var task = CreateSampleTask(type: TaskType.Bug, labels: ["backend", "api"]);
        var filter = new FilterCriteria(Type: TaskType.Bug, Label: "backend");

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesFilter_CombinedLabelAndType_TypeMismatch_ReturnsFalse()
    {
        // Arrange
        var task = CreateSampleTask(type: TaskType.Feature, labels: ["backend"]);
        var filter = new FilterCriteria(Type: TaskType.Bug, Label: "backend");

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MatchesFilter_CombinedLabelTypePriority_AllMatch_ReturnsTrue()
    {
        // Arrange
        var task = CreateSampleTask(type: TaskType.Bug, priority: Priority.High, labels: ["api"]);
        var filter = new FilterCriteria(Type: TaskType.Bug, Priority: Priority.High, Label: "api");

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesFilter_CombinedLabelTypePriority_PriorityMismatch_ReturnsFalse()
    {
        // Arrange
        var task = CreateSampleTask(type: TaskType.Bug, priority: Priority.Low, labels: ["api"]);
        var filter = new FilterCriteria(Type: TaskType.Bug, Priority: Priority.High, Label: "api");

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MatchesFilter_EmptyCriteria_ReturnsTrue()
    {
        // Arrange
        var task = CreateSampleTask(type: TaskType.Feature, priority: Priority.Low, labels: ["frontend"]);
        var filter = new FilterCriteria();

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesFilter_CombinedLabelAndPriority_LabelMismatch_ReturnsFalse()
    {
        // Arrange
        var task = CreateSampleTask(priority: Priority.High, labels: ["frontend"]);
        var filter = new FilterCriteria(Priority: Priority.High, Label: "backend");

        // Act
        var result = task.MatchesFilter(filter);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ChangeStatus_FromDoneToInProgress_ClearsCompletedDate()
    {
        // Arrange
        var task = CreateSampleTask(status: TaskStatus.Done, completedDate: DateTime.UtcNow);

        // Act
        var result = task.ChangeStatus(TaskStatus.InProgress);

        // Assert
        result.Status.Should().Be(TaskStatus.InProgress);
        result.CompletedDate.Should().BeNull();
    }

    [Fact]
    public void GenerateFileName_UnicodeTitle_GeneratesValidFilename()
    {
        // Arrange
        var task = CreateSampleTask(id: 10, title: "Implémenter la résolution des données");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("010-FEATURE-implémenter-la-résolution-des-données.md");
    }

    [Fact]
    public void GenerateFileName_ChineseCharacterTitle_GeneratesValidFilename()
    {
        // Arrange
        var task = CreateSampleTask(id: 11, title: "修复登录问题");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("011-FEATURE-修复登录问题.md");
    }

    [Fact]
    public void GenerateFileName_EmojiTitle_StripsEmojisAndUsesUntitled()
    {
        // Arrange
        var task = CreateSampleTask(id: 12, title: "🚀🔥💯");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("012-FEATURE-untitled.md");
    }

    [Fact]
    public void GenerateFileName_MixedUnicodeAndAscii_GeneratesValidFilename()
    {
        // Arrange
        var task = CreateSampleTask(id: 13, title: "Fix café display bug");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("013-FEATURE-fix-café-display-bug.md");
    }

    [Fact]
    public void GenerateFileName_TitleWithTabs_ConvertsToDashes()
    {
        // Arrange
        var task = CreateSampleTask(id: 14, title: "Fix\tthe\tbug");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("014-FEATURE-fix-the-bug.md");
    }

    [Fact]
    public void GenerateFileName_TitleWithLeadingTrailingSpecialChars_TrimsCleanly()
    {
        // Arrange
        var task = CreateSampleTask(id: 15, title: "---Fix the bug---");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("015-FEATURE-fix-the-bug.md");
    }

    [Fact]
    public void GenerateFileName_TitleWithNumbers_PreservesNumbers()
    {
        // Arrange
        var task = CreateSampleTask(id: 16, title: "Add OAuth2 Support v3");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().Be("016-FEATURE-add-oauth2-support-v3.md");
    }

    [Fact]
    public void GenerateFileName_SpecialCharacters_SanitizesCorrectly()
    {
        // Arrange
        var task = CreateSampleTask(id: 20, title: "Fix: login/signup [broken] & auth?");

        // Act
        var fileName = task.GenerateFileName();

        // Assert
        fileName.Should().StartWith("020-FEATURE-");
        fileName.Should().EndWith(".md");
        fileName.Should().NotContain(":");
        fileName.Should().NotContain("/");
        fileName.Should().NotContain("[");
        fileName.Should().NotContain("]");
        fileName.Should().NotContain("&");
        fileName.Should().NotContain("?");
        fileName.Should().NotContain("--");
    }
}
