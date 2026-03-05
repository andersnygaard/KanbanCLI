using FluentAssertions;
using KanbanCli.Models;
using KanbanCli.Storage;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests.Storage;

public class TaskRepositoryTests
{
    private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();
    private readonly IMarkdownParser _parser = Substitute.For<IMarkdownParser>();
    private static readonly string BoardPath = Path.Combine("board");

    private static string BoardFolder(string subfolder) => Path.Combine(BoardPath, subfolder);
    private static string BoardFile(string subfolder, string file) => Path.Combine(BoardPath, subfolder, file);

    private MarkdownTaskRepository CreateSut()
        => new(_fileSystem, _parser, BoardPath);

    private static TaskItem CreateTask(int id = 1, TaskStatus status = TaskStatus.Backlog)
        => new()
        {
            Id = id,
            Title = "Test task",
            Type = TaskType.Feature,
            Status = status,
            Priority = Priority.Medium,
            Labels = [],
            CreatedDate = new DateTime(2026, 3, 4)
        };

    [Fact]
    public void GetAllByColumn_BacklogWithTasks_ReturnsParsedItems()
    {
        var folderPath = BoardFolder("backlog");
        var filePath = BoardFile("backlog", "001-FEATURE-test-task.md");
        var markdown = "# FEATURE: Test task";
        var task = CreateTask(1, TaskStatus.Backlog);

        _fileSystem.DirectoryExists(folderPath).Returns(true);
        _fileSystem.GetFiles(folderPath, "*.md").Returns([filePath]);
        _fileSystem.ReadAllText(filePath).Returns(markdown);
        _parser.ParseFileName(filePath).Returns((1, TaskType.Feature, "test-task"));
        _parser.Parse(markdown, 1, TaskType.Feature).Returns(task);

        var sut = CreateSut();
        var result = sut.GetAllByColumn(TaskStatus.Backlog);

        result.Should().HaveCount(1);
        result[0].Should().Be(task);
    }

    [Fact]
    public void GetAllByColumn_EmptyFolder_ReturnsEmpty()
    {
        var folderPath = BoardFolder("backlog");

        _fileSystem.DirectoryExists(folderPath).Returns(true);
        _fileSystem.GetFiles(folderPath, "*.md").Returns([]);

        var sut = CreateSut();
        var result = sut.GetAllByColumn(TaskStatus.Backlog);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllByColumn_FolderDoesNotExist_ReturnsEmpty()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(false);

        var sut = CreateSut();
        var result = sut.GetAllByColumn(TaskStatus.Backlog);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Save_NewTask_CreatesFileWithCorrectName()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var expectedFolderPath = BoardFolder("backlog");
        var expectedFilePath = BoardFile("backlog", "001-FEATURE-test-task.md");
        var expectedContent = "# FEATURE: Test task\n";

        _fileSystem.DirectoryExists(expectedFolderPath).Returns(true);
        _parser.Serialize(task).Returns(expectedContent);

        var sut = CreateSut();
        sut.Save(task);

        _fileSystem.Received(1).WriteAllText(expectedFilePath, expectedContent);
    }

    [Fact]
    public void Save_NewTask_CreatesDirectoryIfMissing()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var expectedFolderPath = BoardFolder("backlog");

        _fileSystem.DirectoryExists(expectedFolderPath).Returns(false);
        _parser.Serialize(task).Returns("content");

        var sut = CreateSut();
        sut.Save(task);

        _fileSystem.Received(1).CreateDirectory(expectedFolderPath);
    }

    [Fact]
    public void Move_BacklogToInProgress_MovesFileAndUpdatesStatus()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var targetFolder = BoardFolder("in-progress");
        var sourceFile = BoardFile("backlog", "001-FEATURE-test-task.md");
        var targetFile = BoardFile("in-progress", "001-FEATURE-test-task.md");
        var updatedContent = "# FEATURE: Test task (InProgress)\n";

        _fileSystem.DirectoryExists(targetFolder).Returns(true);
        _fileSystem.FileExists(targetFile).Returns(true);
        _parser.Serialize(Arg.Is<TaskItem>(t => t.Status == TaskStatus.InProgress))
               .Returns(updatedContent);

        var sut = CreateSut();
        sut.Move(task, TaskStatus.InProgress);

        _fileSystem.Received(1).WriteAllText(targetFile, updatedContent);
        _fileSystem.Received(1).DeleteFile(sourceFile);
    }

    [Fact]
    public void Delete_ExistingTask_RemovesFile()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var expectedFilePath = BoardFile("backlog", "001-FEATURE-test-task.md");

        var sut = CreateSut();
        sut.Delete(task);

        _fileSystem.Received(1).DeleteFile(expectedFilePath);
    }

    [Fact]
    public void GetNextId_WithExistingTasks_ReturnsIncrementedId()
    {
        var backlogFolder = BoardFolder("backlog");
        var inProgressFolder = BoardFolder("in-progress");
        var doneFolder = BoardFolder("done");
        var onHoldFolder = BoardFolder("on-hold");

        var backlogFile = BoardFile("backlog", "001-FEATURE-task-one.md");
        var inProgressFile = BoardFile("in-progress", "003-BUG-another.md");

        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(true);

        _fileSystem.GetFiles(backlogFolder, "*.md").Returns([backlogFile]);
        _fileSystem.GetFiles(inProgressFolder, "*.md").Returns([inProgressFile]);
        _fileSystem.GetFiles(doneFolder, "*.md").Returns([]);
        _fileSystem.GetFiles(onHoldFolder, "*.md").Returns([]);

        _parser.ParseFileName(backlogFile).Returns((1, TaskType.Feature, "task-one"));
        _parser.ParseFileName(inProgressFile).Returns((3, TaskType.Bug, "another"));

        var sut = CreateSut();
        var result = sut.GetNextId();

        result.Should().Be(4);
    }

    [Fact]
    public void GetNextId_WithNoTasks_ReturnsOne()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(true);
        _fileSystem.GetFiles(Arg.Any<string>(), "*.md").Returns([]);

        var sut = CreateSut();
        var result = sut.GetNextId();

        result.Should().Be(1);
    }

    [Fact]
    public void GetAllByColumn_FileThrowsIOException_SkipsFileGracefully()
    {
        var folderPath = BoardFolder("backlog");
        var filePath = BoardFile("backlog", "001-FEATURE-test-task.md");

        _fileSystem.DirectoryExists(folderPath).Returns(true);
        _fileSystem.GetFiles(folderPath, "*.md").Returns([filePath]);
        _fileSystem.ReadAllText(filePath).Throws(new IOException("disk error"));
        _parser.ParseFileName(filePath).Returns((1, TaskType.Feature, "test-task"));

        var sut = CreateSut();
        var result = sut.GetAllByColumn(TaskStatus.Backlog);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllByColumn_FileThrowsUnauthorizedAccess_SkipsFileGracefully()
    {
        var folderPath = BoardFolder("backlog");
        var filePath = BoardFile("backlog", "001-FEATURE-test-task.md");

        _fileSystem.DirectoryExists(folderPath).Returns(true);
        _fileSystem.GetFiles(folderPath, "*.md").Returns([filePath]);
        _fileSystem.ReadAllText(filePath).Throws(new UnauthorizedAccessException("no permission"));
        _parser.ParseFileName(filePath).Returns((1, TaskType.Feature, "test-task"));

        var sut = CreateSut();
        var result = sut.GetAllByColumn(TaskStatus.Backlog);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllByColumn_ParserThrowsFormatException_SkipsFileGracefully()
    {
        var folderPath = BoardFolder("backlog");
        var filePath = BoardFile("backlog", "001-FEATURE-test-task.md");
        var markdown = "# Bad content";

        _fileSystem.DirectoryExists(folderPath).Returns(true);
        _fileSystem.GetFiles(folderPath, "*.md").Returns([filePath]);
        _fileSystem.ReadAllText(filePath).Returns(markdown);
        _parser.ParseFileName(filePath).Returns((1, TaskType.Feature, "test-task"));
        _parser.Parse(markdown, 1, TaskType.Feature).Throws(new FormatException("bad format"));

        var sut = CreateSut();
        var result = sut.GetAllByColumn(TaskStatus.Backlog);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllByColumn_InvalidFileName_SkipsFileGracefully()
    {
        var folderPath = BoardFolder("backlog");
        var filePath = BoardFile("backlog", "bad-file.md");

        _fileSystem.DirectoryExists(folderPath).Returns(true);
        _fileSystem.GetFiles(folderPath, "*.md").Returns([filePath]);
        _parser.ParseFileName(filePath).Throws(new ArgumentException("bad filename"));

        var sut = CreateSut();
        var result = sut.GetAllByColumn(TaskStatus.Backlog);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetNextId_InvalidFileName_ReturnsZeroForThatFile()
    {
        var backlogFolder = BoardFolder("backlog");
        var badFile = BoardFile("backlog", "bad-file.md");

        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(true);
        _fileSystem.GetFiles(backlogFolder, "*.md").Returns([badFile]);
        _fileSystem.GetFiles(Arg.Is<string>(s => s != backlogFolder), "*.md").Returns([]);
        _parser.ParseFileName(badFile).Throws(new ArgumentException("bad filename"));

        var sut = CreateSut();
        var result = sut.GetNextId();

        result.Should().Be(1);
    }

    [Fact]
    public void Move_ToSameColumn_DoesNotDeleteFile()
    {
        var task = CreateTask(1, TaskStatus.Backlog);
        var folder = BoardFolder("backlog");
        var filePath = BoardFile("backlog", "001-FEATURE-test-task.md");
        var updatedContent = "# FEATURE: Test task (Backlog)\n";

        _fileSystem.DirectoryExists(folder).Returns(true);
        _parser.Serialize(Arg.Is<TaskItem>(t => t.Status == TaskStatus.Backlog))
               .Returns(updatedContent);

        var sut = CreateSut();
        sut.Move(task, TaskStatus.Backlog);

        _fileSystem.Received(1).WriteAllText(filePath, updatedContent);
        _fileSystem.DidNotReceive().DeleteFile(Arg.Any<string>());
    }
}
