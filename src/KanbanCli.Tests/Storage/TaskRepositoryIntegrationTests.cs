using FluentAssertions;
using KanbanCli.Models;
using KanbanCli.Storage;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests.Storage;

public class TaskRepositoryIntegrationTests : IDisposable
{
    private readonly string _tempBoardPath;
    private readonly FileSystem _fileSystem;
    private readonly MarkdigMarkdownParser _parser;
    private readonly MarkdownTaskRepository _sut;

    public TaskRepositoryIntegrationTests()
    {
        _tempBoardPath = Path.Combine(Path.GetTempPath(), $"kanban-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempBoardPath);

        _fileSystem = new FileSystem();
        _parser = new MarkdigMarkdownParser();
        _sut = new MarkdownTaskRepository(_fileSystem, _parser, _tempBoardPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempBoardPath))
            Directory.Delete(_tempBoardPath, recursive: true);
    }

    private static TaskItem CreateTask(
        int id = 1,
        string title = "Test task",
        TaskType type = TaskType.Feature,
        TaskStatus status = TaskStatus.Backlog,
        Priority priority = Priority.Medium,
        IReadOnlyList<string>? labels = null,
        DateTime? createdDate = null)
    {
        return new()
        {
            Id = id,
            Title = title,
            Type = type,
            Status = status,
            Priority = priority,
            Labels = labels ?? [],
            CreatedDate = createdDate ?? new DateTime(2026, 3, 5)
        };
    }

    [Fact]
    public void Save_CreatesFileOnDisk_WithCorrectContent()
    {
        var task = CreateTask(id: 1, title: "My new feature", labels: ["ui", "core"]);

        _sut.Save(task);

        var expectedFolder = Path.Combine(_tempBoardPath, "backlog");
        var expectedFile = Path.Combine(expectedFolder, "001-FEATURE-my-new-feature.md");

        File.Exists(expectedFile).Should().BeTrue("the task file should be created on disk");

        var content = File.ReadAllText(expectedFile);
        content.Should().Contain("# FEATURE: My new feature");
        content.Should().Contain("**Status**: Backlog");
        content.Should().Contain("**Priority**: Medium");
        content.Should().Contain("**Labels**: ui, core");
        content.Should().Contain("**Created**: 2026-03-05");
    }

    [Fact]
    public void Move_MovesFileBetweenFolders_SourceRemovedTargetExists()
    {
        var task = CreateTask(id: 2, title: "Move me", status: TaskStatus.Backlog);
        _sut.Save(task);

        var sourceFile = Path.Combine(_tempBoardPath, "backlog", "002-FEATURE-move-me.md");
        File.Exists(sourceFile).Should().BeTrue("task should exist before move");

        _sut.Move(task, TaskStatus.InProgress);

        var targetFile = Path.Combine(_tempBoardPath, "in-progress", "002-FEATURE-move-me.md");

        File.Exists(sourceFile).Should().BeFalse("source file should be removed after move");
        File.Exists(targetFile).Should().BeTrue("target file should exist after move");

        var content = File.ReadAllText(targetFile);
        content.Should().Contain("**Status**: In Progress");
    }

    [Fact]
    public void Delete_RemovesFileFromDisk()
    {
        var task = CreateTask(id: 3, title: "Delete me");
        _sut.Save(task);

        var filePath = Path.Combine(_tempBoardPath, "backlog", "003-FEATURE-delete-me.md");
        File.Exists(filePath).Should().BeTrue("task should exist before deletion");

        _sut.Delete(task);

        File.Exists(filePath).Should().BeFalse("task file should be removed after deletion");
    }

    [Fact]
    public void GetNextId_WithRealFiles_ReturnsCorrectNextId()
    {
        _sut.Save(CreateTask(id: 1, title: "First task"));
        _sut.Save(CreateTask(id: 5, title: "Fifth task"));
        _sut.Save(CreateTask(id: 3, title: "Third task", status: TaskStatus.InProgress));

        var nextId = _sut.GetNextId();

        nextId.Should().Be(6, "next ID should be one more than the highest existing ID");
    }

    [Fact]
    public void Roundtrip_CreateThenReadBack_AllFieldsPreserved()
    {
        var original = CreateTask(
            id: 10,
            title: "Roundtrip test",
            type: TaskType.Bug,
            status: TaskStatus.Backlog,
            priority: Priority.High,
            labels: ["urgent", "backend"],
            createdDate: new DateTime(2026, 1, 15));

        _sut.Save(original);

        var allTasks = _sut.GetAllByColumn(TaskStatus.Backlog);

        allTasks.Should().HaveCount(1);

        var loaded = allTasks[0];
        loaded.Id.Should().Be(original.Id);
        loaded.Title.Should().Be(original.Title);
        loaded.Type.Should().Be(original.Type);
        loaded.Status.Should().Be(original.Status);
        loaded.Priority.Should().Be(original.Priority);
        loaded.Labels.Should().BeEquivalentTo(original.Labels);
        loaded.CreatedDate.Should().Be(original.CreatedDate);
    }
}
