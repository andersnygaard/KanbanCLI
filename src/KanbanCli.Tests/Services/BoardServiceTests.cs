using FluentAssertions;
using KanbanCli.Models;
using KanbanCli.Services;
using KanbanCli.Storage;
using NSubstitute;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests.Services;

public class BoardServiceTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();

    private BoardService CreateSut() => new(_repository);

    private static TaskItem CreateTask(
        int id,
        string title,
        TaskType type,
        TaskStatus status,
        Priority priority = Priority.Medium,
        DateTime? completedDate = null)
        => new()
        {
            Id = id,
            Title = title,
            Type = type,
            Status = status,
            Priority = priority,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            CompletedDate = completedDate
        };

    [Fact]
    public void GeneratePlanningBoard_TopPriorities_FormatsCorrectly()
    {
        var inProgressTask = CreateTask(3, "Dark mode", TaskType.Feature, TaskStatus.InProgress);
        var highPriorityBacklog = CreateTask(5, "Memory leak in parser", TaskType.Bug, TaskStatus.Backlog, Priority.High);

        _repository.GetAll().Returns([inProgressTask, highPriorityBacklog]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        result.Should().Contain("# Planning Board");
        result.Should().Contain("## Top Priorities");
        result.Should().Contain("**#003** FEATURE: Dark mode - In Progress");
        result.Should().Contain("**#005** BUG: Memory leak in parser - Backlog");
    }

    [Fact]
    public void GeneratePlanningBoard_RecentlyCompleted_IncludesDone()
    {
        var completedDate = new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc);
        var doneTask = CreateTask(4, "Header overlap fix", TaskType.Bug, TaskStatus.Done, completedDate: completedDate);

        _repository.GetAll().Returns([doneTask]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        result.Should().Contain("## Recently Completed");
        result.Should().Contain("**#004** BUG: Header overlap fix - Done 2026-03-03");
    }

    [Fact]
    public void GeneratePlanningBoard_EmptyBoard_ShowsEmptyMessage()
    {
        _repository.GetAll().Returns([]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        result.Should().Contain("# Planning Board");
        result.Should().Contain("No tasks yet.");
    }

    [Fact]
    public void GeneratePlanningBoard_InProgressTasksAppearBeforeHighPriorityBacklog()
    {
        var inProgressTask = CreateTask(2, "Active work", TaskType.Feature, TaskStatus.InProgress);
        var highPriorityBacklog = CreateTask(1, "High prio task", TaskType.Bug, TaskStatus.Backlog, Priority.High);

        _repository.GetAll().Returns([highPriorityBacklog, inProgressTask]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        var inProgressIndex = result.IndexOf("Active work", StringComparison.Ordinal);
        var backlogIndex = result.IndexOf("High prio task", StringComparison.Ordinal);

        inProgressIndex.Should().BeLessThan(backlogIndex);
    }

    [Fact]
    public void GeneratePlanningBoard_RecentlyCompleted_OrderedByCompletedDateDescending()
    {
        var olderTask = CreateTask(1, "Old fix", TaskType.Bug, TaskStatus.Done,
            completedDate: new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        var newerTask = CreateTask(2, "New fix", TaskType.Bug, TaskStatus.Done,
            completedDate: new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc));

        _repository.GetAll().Returns([olderTask, newerTask]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        var newerIndex = result.IndexOf("New fix", StringComparison.Ordinal);
        var olderIndex = result.IndexOf("Old fix", StringComparison.Ordinal);

        newerIndex.Should().BeLessThan(olderIndex);
    }
}
