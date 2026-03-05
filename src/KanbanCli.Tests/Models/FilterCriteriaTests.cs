using FluentAssertions;
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests.Models;

public class FilterCriteriaTests
{
    private static TaskItem CreateTask(
        TaskType type = TaskType.Feature,
        Priority priority = Priority.Medium,
        IReadOnlyList<string>? labels = null) =>
        new()
        {
            Id = 1,
            Title = "Test Task",
            Type = type,
            Priority = priority,
            Status = TaskStatus.Backlog,
            Labels = labels ?? [],
            CreatedDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)
        };

    [Fact]
    public void EmptyCriteria_MatchesAllTasks()
    {
        var filter = new FilterCriteria();

        var featureTask = CreateTask(type: TaskType.Feature, priority: Priority.High, labels: ["api"]);
        var bugTask = CreateTask(type: TaskType.Bug, priority: Priority.Low);

        featureTask.MatchesFilter(filter).Should().BeTrue();
        bugTask.MatchesFilter(filter).Should().BeTrue();
    }

    [Fact]
    public void PriorityOnlyFilter_MatchesTasksWithSamePriority()
    {
        var filter = new FilterCriteria(Priority: Priority.High);

        var highTask = CreateTask(priority: Priority.High);
        var lowTask = CreateTask(priority: Priority.Low);
        var medTask = CreateTask(priority: Priority.Medium);

        highTask.MatchesFilter(filter).Should().BeTrue();
        lowTask.MatchesFilter(filter).Should().BeFalse();
        medTask.MatchesFilter(filter).Should().BeFalse();
    }

    [Fact]
    public void MultiCriteria_TypeAndPriority_BothMustMatch()
    {
        var filter = new FilterCriteria(Type: TaskType.Bug, Priority: Priority.High);

        var match = CreateTask(type: TaskType.Bug, priority: Priority.High);
        var wrongType = CreateTask(type: TaskType.Feature, priority: Priority.High);
        var wrongPriority = CreateTask(type: TaskType.Bug, priority: Priority.Low);

        match.MatchesFilter(filter).Should().BeTrue();
        wrongType.MatchesFilter(filter).Should().BeFalse();
        wrongPriority.MatchesFilter(filter).Should().BeFalse();
    }

    [Fact]
    public void MultiCriteria_TypeAndLabel_BothMustMatch()
    {
        var filter = new FilterCriteria(Type: TaskType.Feature, Label: "frontend");

        var match = CreateTask(type: TaskType.Feature, labels: ["frontend", "api"]);
        var wrongType = CreateTask(type: TaskType.Bug, labels: ["frontend"]);
        var wrongLabel = CreateTask(type: TaskType.Feature, labels: ["backend"]);

        match.MatchesFilter(filter).Should().BeTrue();
        wrongType.MatchesFilter(filter).Should().BeFalse();
        wrongLabel.MatchesFilter(filter).Should().BeFalse();
    }

    [Fact]
    public void MultiCriteria_PriorityAndLabel_BothMustMatch()
    {
        var filter = new FilterCriteria(Priority: Priority.High, Label: "api");

        var match = CreateTask(priority: Priority.High, labels: ["api"]);
        var wrongPriority = CreateTask(priority: Priority.Low, labels: ["api"]);
        var wrongLabel = CreateTask(priority: Priority.High, labels: ["frontend"]);

        match.MatchesFilter(filter).Should().BeTrue();
        wrongPriority.MatchesFilter(filter).Should().BeFalse();
        wrongLabel.MatchesFilter(filter).Should().BeFalse();
    }

    [Fact]
    public void MultiCriteria_AllThree_AllMustMatch()
    {
        var filter = new FilterCriteria(Type: TaskType.Bug, Priority: Priority.High, Label: "backend");

        var match = CreateTask(type: TaskType.Bug, priority: Priority.High, labels: ["backend"]);
        var wrongType = CreateTask(type: TaskType.Feature, priority: Priority.High, labels: ["backend"]);
        var wrongPriority = CreateTask(type: TaskType.Bug, priority: Priority.Low, labels: ["backend"]);
        var wrongLabel = CreateTask(type: TaskType.Bug, priority: Priority.High, labels: ["frontend"]);

        match.MatchesFilter(filter).Should().BeTrue();
        wrongType.MatchesFilter(filter).Should().BeFalse();
        wrongPriority.MatchesFilter(filter).Should().BeFalse();
        wrongLabel.MatchesFilter(filter).Should().BeFalse();
    }

    [Fact]
    public void LabelFilter_CaseInsensitive_Matches()
    {
        var filter = new FilterCriteria(Label: "API");

        var task = CreateTask(labels: ["api"]);

        task.MatchesFilter(filter).Should().BeTrue();
    }

    [Fact]
    public void LabelFilter_NoLabelsOnTask_ReturnsFalse()
    {
        var filter = new FilterCriteria(Label: "frontend");

        var task = CreateTask(labels: []);

        task.MatchesFilter(filter).Should().BeFalse();
    }
}
