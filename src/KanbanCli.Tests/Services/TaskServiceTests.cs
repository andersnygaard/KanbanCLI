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

    private TaskService CreateSut()
    {
        return new(_repository);
    }

    private static TaskItem CreateTask(int id = 1, TaskStatus status = TaskStatus.Backlog, Priority priority = Priority.Medium)
    {
        return new()
        {
            Id = id,
            Title = "Test task",
            Type = TaskType.Feature,
            Status = status,
            Priority = priority,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc)
        };
    }

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
    public void MoveTask_PassesOriginalTaskToRepository()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var sut = CreateSut();

        sut.MoveTask(task, TaskStatus.InProgress);

        _repository.Received(1).Move(
            Arg.Is<TaskItem>(t => t.Status == TaskStatus.Backlog && t.Id == 1),
            TaskStatus.InProgress);
    }

    [Fact]
    public void MoveTask_ToDone_PassesOriginalTaskToRepository()
    {
        var task = CreateTask(1, TaskStatus.InProgress);
        var sut = CreateSut();

        sut.MoveTask(task, TaskStatus.Done);

        _repository.Received(1).Move(
            Arg.Is<TaskItem>(t => t.Status == TaskStatus.InProgress && !t.CompletedDate.HasValue),
            TaskStatus.Done);
    }

    [Fact]
    public void MoveTask_UpdatesStatusAndMovesFile()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var sut = CreateSut();

        sut.MoveTask(task, TaskStatus.InProgress);

        _repository.Received(1).Move(
            Arg.Is<TaskItem>(t => t.Id == 1),
            TaskStatus.InProgress);
    }

    [Fact]
    public void MoveTask_ToDone_SetsCompletedDate()
    {
        var task = CreateTask(1, TaskStatus.InProgress);
        var sut = CreateSut();

        sut.MoveTask(task, TaskStatus.Done);

        _repository.Received(1).Move(
            Arg.Is<TaskItem>(t => t.Id == 1),
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
    public void CreateTask_EmptyTitle_ThrowsArgumentException()
    {
        var sut = CreateSut();

        var act = () => sut.CreateTask("", TaskType.Feature, Priority.Medium, []);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Fact]
    public void CreateTask_WhitespaceTitle_ThrowsArgumentException()
    {
        var sut = CreateSut();

        var act = () => sut.CreateTask("   ", TaskType.Feature, Priority.Medium, []);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Fact]
    public void CreateTask_NullTitle_ThrowsArgumentException()
    {
        var sut = CreateSut();

        var act = () => sut.CreateTask(null!, TaskType.Feature, Priority.Medium, []);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Fact]
    public void CreateTask_TitleExceedsMaxLength_ThrowsArgumentException()
    {
        var sut = CreateSut();
        var longTitle = new string('A', 201);

        var act = () => sut.CreateTask(longTitle, TaskType.Feature, Priority.Medium, []);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
    }

    [Fact]
    public void CreateTask_TitleAtMaxLength_DoesNotThrow()
    {
        _repository.GetNextId().Returns(1);
        var sut = CreateSut();
        var title = new string('A', 200);

        var act = () => sut.CreateTask(title, TaskType.Feature, Priority.Medium, []);

        act.Should().NotThrow();
    }

    [Fact]
    public void CreateTask_TitleWithInvalidFileNameChars_ThrowsArgumentException()
    {
        var sut = CreateSut();

        var act = () => sut.CreateTask("invalid/title", TaskType.Feature, Priority.Medium, []);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("title");
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

        var fileSystem = Substitute.For<IFileSystem>();
        var boardService = new BoardService(_repository, fileSystem);
        var board = boardService.GetBoard();

        board.Columns.Should().HaveCount(4);
        board.GetColumn(TaskStatus.Backlog)!.Tasks.Should().ContainSingle(t => t.Id == 1);
        board.GetColumn(TaskStatus.InProgress)!.Tasks.Should().ContainSingle(t => t.Id == 2);
    }

    [Fact]
    public void GetAllByColumn_DelegatesToRepository()
    {
        var backlogTask = CreateTask(1, TaskStatus.Backlog);
        _repository.GetAllByColumn(TaskStatus.Backlog).Returns([backlogTask]);

        var sut = CreateSut();
        var result = sut.GetAllByColumn(TaskStatus.Backlog);

        result.Should().ContainSingle(t => t.Id == 1);
        _repository.Received(1).GetAllByColumn(TaskStatus.Backlog);
    }

    [Fact]
    public void GetAllByColumn_EmptyColumn_ReturnsEmptyList()
    {
        _repository.GetAllByColumn(TaskStatus.Done).Returns([]);

        var sut = CreateSut();
        var result = sut.GetAllByColumn(TaskStatus.Done);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAll_DelegatesToRepository()
    {
        var tasks = new List<TaskItem> { CreateTask(1), CreateTask(2) };
        _repository.GetAll().Returns(tasks);

        var sut = CreateSut();
        var result = sut.GetAll();

        result.Should().HaveCount(2);
        _repository.Received(1).GetAll();
    }

    [Fact]
    public void UpdateTask_CallsRepositoryUpdate()
    {
        var task = CreateTask(1, TaskStatus.InProgress);
        var sut = CreateSut();

        sut.UpdateTask(task);

        _repository.Received(1).Update(task);
    }

    [Fact]
    public void CreateTask_RepositorySaveThrows_PropagatesException()
    {
        _repository.GetNextId().Returns(1);
        _repository.When(r => r.Save(Arg.Any<TaskItem>()))
            .Do(_ => throw new IOException("Disk full"));

        var sut = CreateSut();

        var act = () => sut.CreateTask("Valid title", TaskType.Feature, Priority.Medium, []);

        act.Should().Throw<IOException>().WithMessage("Disk full");
    }

    [Fact]
    public void MoveTask_RepositoryMoveThrows_PropagatesException()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        _repository.When(r => r.Move(Arg.Any<TaskItem>(), Arg.Any<TaskStatus>()))
            .Do(_ => throw new IOException("Permission denied"));

        var sut = CreateSut();

        var act = () => sut.MoveTask(task, TaskStatus.InProgress);

        act.Should().Throw<IOException>().WithMessage("Permission denied");
    }

    [Fact]
    public void DeleteTask_RepositoryDeleteThrows_PropagatesException()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        _repository.When(r => r.Delete(Arg.Any<TaskItem>()))
            .Do(_ => throw new IOException("File not found"));

        var sut = CreateSut();

        var act = () => sut.DeleteTask(task);

        act.Should().Throw<IOException>().WithMessage("File not found");
    }

    [Fact]
    public void GetAllByColumn_RepositoryThrows_PropagatesException()
    {
        _repository.GetAllByColumn(Arg.Any<TaskStatus>())
            .Returns(_ => throw new IOException("Cannot read directory"));

        var sut = CreateSut();

        var act = () => sut.GetAllByColumn(TaskStatus.Backlog);

        act.Should().Throw<IOException>().WithMessage("Cannot read directory");
    }

    [Fact]
    public void UpdateTask_TitleChange_PersistsViaRepository()
    {
        var original = CreateTask(1, TaskStatus.InProgress);
        var updated = original with { Title = "Updated title" };
        var sut = CreateSut();

        sut.UpdateTask(updated);

        _repository.Received(1).Update(Arg.Is<TaskItem>(t => t.Title == "Updated title" && t.Id == 1));
    }

    [Fact]
    public void UpdateTask_LabelsChange_PersistsViaRepository()
    {
        var original = CreateTask(1, TaskStatus.InProgress);
        var updated = original with { Labels = new List<string> { "frontend", "urgent" } };
        var sut = CreateSut();

        sut.UpdateTask(updated);

        _repository.Received(1).Update(Arg.Is<TaskItem>(t =>
            t.Labels.Count == 2 &&
            t.Labels.Contains("frontend") &&
            t.Labels.Contains("urgent")));
    }

    [Fact]
    public void GetAll_ReturnsExactItemsFromRepository()
    {
        var task1 = CreateTask(1, TaskStatus.Backlog);
        var task2 = CreateTask(2, TaskStatus.InProgress);
        var task3 = CreateTask(3, TaskStatus.Done);
        var allTasks = new List<TaskItem> { task1, task2, task3 };
        _repository.GetAll().Returns(allTasks);

        var sut = CreateSut();
        var result = sut.GetAll();

        result.Should().BeEquivalentTo(allTasks);
        _repository.Received(1).GetAll();
    }

    [Fact]
    public void MoveTask_ToSameStatus_StillCallsRepository()
    {
        var task = CreateTask(1, TaskStatus.InProgress);
        var sut = CreateSut();

        sut.MoveTask(task, TaskStatus.InProgress);

        _repository.Received(1).Move(
            Arg.Is<TaskItem>(t => t.Status == TaskStatus.InProgress && t.Id == 1),
            TaskStatus.InProgress);
    }

    [Fact]
    public void MoveTask_ToSameStatus_DoesNotChangeCompletedDate()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var sut = CreateSut();

        sut.MoveTask(task, TaskStatus.Backlog);

        _repository.Received(1).Move(
            Arg.Is<TaskItem>(t => t.Status == TaskStatus.Backlog && t.CompletedDate == null),
            TaskStatus.Backlog);
    }

    [Fact]
    public void MoveTask_DoesNotCallChangeStatus_RepositoryReceivesOriginalStatus()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var sut = CreateSut();

        sut.MoveTask(task, TaskStatus.Done);

        _repository.Received(1).Move(
            Arg.Is<TaskItem>(t => t.Status == TaskStatus.Backlog),
            TaskStatus.Done);
    }

    [Fact]
    public void CreateTask_HasDefaultSections()
    {
        _repository.GetNextId().Returns(1);
        var sut = CreateSut();

        var task = sut.CreateTask("Test task", TaskType.Feature, Priority.Medium, []);

        task.Sections.Should().ContainKey("Context & Motivation");
        task.Sections.Should().ContainKey("Acceptance Criteria");
        task.Sections.Should().ContainKey("Progress Log");
    }

    [Fact]
    public void CreateTask_DefaultSections_HaveExpectedContent()
    {
        _repository.GetNextId().Returns(1);
        var sut = CreateSut();

        var task = sut.CreateTask("Test task", TaskType.Feature, Priority.Medium, []);

        task.Sections["Context & Motivation"].Should().Be("(No description provided)");
        task.Sections["Acceptance Criteria"].Should().Be("- [ ] (To be defined)");
        task.Sections["Progress Log"].Should().Contain("Task created");
    }

    [Fact]
    public void CreateTask_ProgressLog_ContainsCreatedDate()
    {
        _repository.GetNextId().Returns(1);
        var sut = CreateSut();

        var task = sut.CreateTask("Test task", TaskType.Feature, Priority.Medium, []);

        var dateString = task.CreatedDate!.Value.ToString(BoardConstants.DateFormat);
        task.Sections["Progress Log"].Should().Contain(dateString);
    }
}
