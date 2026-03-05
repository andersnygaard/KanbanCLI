using FluentAssertions;
using KanbanCli.Models;
using KanbanCli.Services;
using KanbanCli.Storage;
using NSubstitute;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests.Services;

public class TaskServiceTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();

    private TaskService CreateSut() => new(_repository);

    private static TaskItem CreateTask(int id = 1, TaskStatus status = TaskStatus.Backlog, Priority priority = Priority.Medium)
        => new()
        {
            Id = id,
            Title = "Test task",
            Type = TaskType.Feature,
            Status = status,
            Priority = priority,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc)
        };

    [Fact]
    public void CreateTask_ValidInput_SavesAndReturnsTask()
    {
        _repository.GetNextId().Returns(5);
        var sut = CreateSut();

        var task = sut.CreateTask("My Task", TaskType.Feature, Priority.High, new List<string> { "frontend" });

        task.Id.Should().Be(5);
        task.Title.Should().Be("My Task");
        task.Type.Should().Be(TaskType.Feature);
        task.Priority.Should().Be(Priority.High);
        task.Labels.Should().ContainSingle(l => l == "frontend");
        task.Status.Should().Be(TaskStatus.Backlog);
        _repository.Received(1).Save(Arg.Is<TaskItem>(t => t.Id == 5 && t.Title == "My Task"));
    }

    [Fact]
    public void CreateTask_AssignsNextAvailableId()
    {
        _repository.GetNextId().Returns(42);
        var sut = CreateSut();

        var task = sut.CreateTask("Another Task", TaskType.Bug, Priority.Medium, []);

        task.Id.Should().Be(42);
        _repository.Received(1).GetNextId();
    }

    [Fact]
    public void MoveTask_UpdatesStatusAndMovesFile()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var sut = CreateSut();

        sut.MoveTask(task, TaskStatus.InProgress);

        _repository.Received(1).Move(
            Arg.Is<TaskItem>(t => t.Status == TaskStatus.InProgress && t.Id == 1),
            TaskStatus.InProgress);
    }

    [Fact]
    public void MoveTask_ToDone_SetsCompletedDate()
    {
        var task = CreateTask(1, TaskStatus.InProgress);
        var sut = CreateSut();

        sut.MoveTask(task, TaskStatus.Done);

        _repository.Received(1).Move(
            Arg.Is<TaskItem>(t => t.Status == TaskStatus.Done && t.CompletedDate.HasValue),
            TaskStatus.Done);
    }

    [Fact]
    public void DeleteTask_CallsRepositoryDelete()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var sut = CreateSut();

        sut.DeleteTask(task);

        _repository.Received(1).Delete(task);
    }

    [Fact]
    public void GetBoard_ReturnsAllColumnsWithTasks()
    {
        var backlogTask = CreateTask(1, TaskStatus.Backlog);
        var inProgressTask = CreateTask(2, TaskStatus.InProgress);

        _repository.GetAllByColumn(TaskStatus.Backlog).Returns([backlogTask]);
        _repository.GetAllByColumn(TaskStatus.InProgress).Returns([inProgressTask]);
        _repository.GetAllByColumn(TaskStatus.Done).Returns([]);
        _repository.GetAllByColumn(TaskStatus.OnHold).Returns([]);

        var boardService = new BoardService(_repository);
        var board = boardService.GetBoard();

        board.Columns.Should().HaveCount(4);
        board.Columns.First(c => c.Name == "Backlog").Tasks.Should().ContainSingle(t => t.Id == 1);
        board.Columns.First(c => c.Name == "In Progress").Tasks.Should().ContainSingle(t => t.Id == 2);
    }
}
