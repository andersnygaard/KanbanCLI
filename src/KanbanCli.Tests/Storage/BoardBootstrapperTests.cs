using FluentAssertions;
using KanbanCli.Models;
using KanbanCli.Storage;
using NSubstitute;

namespace KanbanCli.Tests.Storage;

public class BoardBootstrapperTests
{
    private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();
    private const string BoardPath = "test-board";

    [Fact]
    public void EnsureBoardDirectories_MissingRoot_CreatesRootDirectory()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(false);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        _fileSystem.Received(1).CreateDirectory(BoardPath);
    }

    [Fact]
    public void EnsureBoardDirectories_MissingRoot_CreatesAllColumnFolders()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(false);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        foreach (var folderName in BoardConstants.FolderNames.Values)
        {
            var expectedPath = Path.Combine(BoardPath, folderName);
            _fileSystem.Received(1).CreateDirectory(expectedPath);
        }
    }

    [Fact]
    public void EnsureBoardDirectories_MissingRoot_CreatesFiveDirectoriesTotal()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(false);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        // 1 root + 4 column folders
        _fileSystem.Received(5).CreateDirectory(Arg.Any<string>());
    }

    [Fact]
    public void EnsureBoardDirectories_AllDirectoriesExist_DoesNotCreateAny()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(true);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        _fileSystem.DidNotReceive().CreateDirectory(Arg.Any<string>());
    }

    [Fact]
    public void EnsureBoardDirectories_RootExistsButColumnsMissing_CreatesOnlyColumns()
    {
        _fileSystem.DirectoryExists(BoardPath).Returns(true);
        _fileSystem.DirectoryExists(Arg.Is<string>(p => p != BoardPath)).Returns(false);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        _fileSystem.DidNotReceive().CreateDirectory(BoardPath);
        _fileSystem.Received(4).CreateDirectory(Arg.Is<string>(p => p != BoardPath));
    }

    [Fact]
    public void EnsureBoardDirectories_CreatesBacklogFolder()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(false);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        var expectedPath = Path.Combine(BoardPath, "backlog");
        _fileSystem.Received(1).CreateDirectory(expectedPath);
    }

    [Fact]
    public void EnsureBoardDirectories_CreatesInProgressFolder()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(false);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        var expectedPath = Path.Combine(BoardPath, "in-progress");
        _fileSystem.Received(1).CreateDirectory(expectedPath);
    }

    [Fact]
    public void EnsureBoardDirectories_CreatesDoneFolder()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(false);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        var expectedPath = Path.Combine(BoardPath, "done");
        _fileSystem.Received(1).CreateDirectory(expectedPath);
    }

    [Fact]
    public void EnsureBoardDirectories_CreatesOnHoldFolder()
    {
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(false);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        var expectedPath = Path.Combine(BoardPath, "on-hold");
        _fileSystem.Received(1).CreateDirectory(expectedPath);
    }

    [Fact]
    public void EnsureBoardDirectories_PartialDirectoriesExist_CreatesOnlyMissing()
    {
        var backlogPath = Path.Combine(BoardPath, "backlog");
        var donePath = Path.Combine(BoardPath, "done");

        _fileSystem.DirectoryExists(BoardPath).Returns(true);
        _fileSystem.DirectoryExists(backlogPath).Returns(true);
        _fileSystem.DirectoryExists(donePath).Returns(true);
        _fileSystem.DirectoryExists(Arg.Is<string>(p =>
            p != BoardPath && p != backlogPath && p != donePath)).Returns(false);

        BoardBootstrapper.EnsureBoardDirectories(_fileSystem, BoardPath);

        _fileSystem.DidNotReceive().CreateDirectory(BoardPath);
        _fileSystem.DidNotReceive().CreateDirectory(backlogPath);
        _fileSystem.DidNotReceive().CreateDirectory(donePath);
        _fileSystem.Received(2).CreateDirectory(Arg.Any<string>());
    }
}
