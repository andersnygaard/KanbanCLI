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
    private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();

    private BoardService CreateSut() => new(_repository, _fileSystem);

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

    [Fact]
    public void SavePlanningBoard_WritesGeneratedContentToFile()
    {
        var task = CreateTask(1, "Test task", TaskType.Feature, TaskStatus.InProgress);
        _repository.GetAll().Returns([task]);

        var sut = CreateSut();
        sut.SavePlanningBoard("/some/board");

        _fileSystem.Received(1).WriteAllText(
            "/some/board/PLANNING-BOARD.md",
            Arg.Is<string>(s => s.Contains("# Planning Board") && s.Contains("Test task")));
    }

    [Fact]
    public void SavePlanningBoard_EmptyBoard_WritesEmptyBoardContent()
    {
        _repository.GetAll().Returns([]);

        var sut = CreateSut();
        sut.SavePlanningBoard("/my/board");

        _fileSystem.Received(1).WriteAllText(
            "/my/board/PLANNING-BOARD.md",
            Arg.Is<string>(s => s.Contains("# Planning Board") && s.Contains("No tasks yet.")));
    }

    [Fact]
    public void SavePlanningBoard_WritesCorrectFilePath()
    {
        _repository.GetAll().Returns([]);

        var sut = CreateSut();
        sut.SavePlanningBoard(".task-board");

        _fileSystem.Received(1).WriteAllText(
            Path.Combine(".task-board", "PLANNING-BOARD.md"),
            Arg.Any<string>());
    }

    [Fact]
    public void GeneratePlanningBoard_NullCompletedDates_RendersWithoutDate()
    {
        var doneTask = CreateTask(7, "Fix login", TaskType.Bug, TaskStatus.Done, completedDate: null);

        _repository.GetAll().Returns([doneTask]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        result.Should().Contain("**#007** BUG: Fix login - Done ");
        result.Should().NotContain("0001-01-01");
    }

    [Fact]
    public void GeneratePlanningBoard_MixedPriorities_OnlyHighPriorityBacklogInTopPriorities()
    {
        var lowBacklog = CreateTask(1, "Low priority task", TaskType.Feature, TaskStatus.Backlog, Priority.Low);
        var medBacklog = CreateTask(2, "Medium priority task", TaskType.Feature, TaskStatus.Backlog, Priority.Medium);
        var highBacklog = CreateTask(3, "High priority task", TaskType.Bug, TaskStatus.Backlog, Priority.High);

        _repository.GetAll().Returns([lowBacklog, medBacklog, highBacklog]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        result.Should().Contain("High priority task");
        var topPrioritiesSection = result.Substring(
            result.IndexOf("## Top Priorities", StringComparison.Ordinal),
            result.IndexOf("## Recently Completed", StringComparison.Ordinal)
                - result.IndexOf("## Top Priorities", StringComparison.Ordinal));

        topPrioritiesSection.Should().NotContain("Low priority task");
        topPrioritiesSection.Should().NotContain("Medium priority task");
    }

    [Fact]
    public void GeneratePlanningBoard_TopPriorities_LimitedToMaxTopPriorities()
    {
        var tasks = Enumerable.Range(1, 7)
            .Select(i => CreateTask(i, $"Task {i}", TaskType.Feature, TaskStatus.InProgress))
            .ToList();

        _repository.GetAll().Returns(tasks);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        var topPrioritiesSection = result.Substring(
            result.IndexOf("## Top Priorities", StringComparison.Ordinal),
            result.IndexOf("## Recently Completed", StringComparison.Ordinal)
                - result.IndexOf("## Top Priorities", StringComparison.Ordinal));

        topPrioritiesSection.Should().Contain("Task 1");
        topPrioritiesSection.Should().Contain("Task 5");
        topPrioritiesSection.Should().NotContain("Task 6");
        topPrioritiesSection.Should().NotContain("Task 7");
    }

    [Fact]
    public void GeneratePlanningBoard_NoTopPriorities_ShowsNoPrioritiesMessage()
    {
        var lowBacklog = CreateTask(1, "Low task", TaskType.Feature, TaskStatus.Backlog, Priority.Low);

        _repository.GetAll().Returns([lowBacklog]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        result.Should().Contain("No active priorities.");
    }

    [Fact]
    public void GeneratePlanningBoard_NoCompletedTasks_ShowsNoCompletedMessage()
    {
        var inProgressTask = CreateTask(1, "Active", TaskType.Feature, TaskStatus.InProgress);

        _repository.GetAll().Returns([inProgressTask]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        result.Should().Contain("No completed tasks.");
    }

    [Fact]
    public void GetBoard_EmptyBoard_ReturnsColumnsWithNoTasks()
    {
        _repository.GetAllByColumn(Arg.Any<TaskStatus>()).Returns([]);

        var sut = CreateSut();
        var board = sut.GetBoard();

        board.Columns.Should().HaveCount(4);
        board.Columns.Should().AllSatisfy(c => c.Tasks.Should().BeEmpty());
    }

    [Fact]
    public void GeneratePlanningBoard_LongTaskTitle_RendersFullTitle()
    {
        var longTitle = new string('A', 150);
        var task = CreateTask(1, longTitle, TaskType.Feature, TaskStatus.InProgress);

        _repository.GetAll().Returns([task]);

        var sut = CreateSut();
        var result = sut.GeneratePlanningBoard();

        result.Should().Contain(longTitle);
    }
}
