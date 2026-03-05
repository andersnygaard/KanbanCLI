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
        DateTime? completedDate = null) =>
        new()
        {
            Id = id,
            Title = title,
            Type = type,
            Priority = priority,
            Status = status,
            Labels = labels ?? [],
            CreatedDate = createdDate ?? DateTime.UtcNow,
            CompletedDate = completedDate
        };

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
}
