using FluentAssertions;
using KanbanCli.Storage;

namespace KanbanCli.Tests.Storage;

public class FileSystemValidationTests
{
    private readonly FileSystem _fileSystem = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GetFiles_InvalidDirectory_ThrowsArgumentException(string? directory)
    {
        var act = () => _fileSystem.GetFiles(directory!, "*.md");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void GetFiles_InvalidPattern_ThrowsArgumentException(string? pattern)
    {
        var act = () => _fileSystem.GetFiles("/tmp", pattern!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ReadAllText_InvalidPath_ThrowsArgumentException(string? path)
    {
        var act = () => _fileSystem.ReadAllText(path!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void WriteAllText_InvalidPath_ThrowsArgumentException(string? path)
    {
        var act = () => _fileSystem.WriteAllText(path!, "content");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void WriteAllText_InvalidContent_ThrowsArgumentException(string? content)
    {
        var act = () => _fileSystem.WriteAllText("/tmp/test.md", content!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void MoveFile_InvalidSource_ThrowsArgumentException(string? source)
    {
        var act = () => _fileSystem.MoveFile(source!, "/tmp/dest.md");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void MoveFile_InvalidDestination_ThrowsArgumentException(string? destination)
    {
        var act = () => _fileSystem.MoveFile("/tmp/source.md", destination!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void DeleteFile_InvalidPath_ThrowsArgumentException(string? path)
    {
        var act = () => _fileSystem.DeleteFile(path!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void FileExists_InvalidPath_ThrowsArgumentException(string? path)
    {
        var act = () => _fileSystem.FileExists(path!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void DirectoryExists_InvalidPath_ThrowsArgumentException(string? path)
    {
        var act = () => _fileSystem.DirectoryExists(path!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void CreateDirectory_InvalidPath_ThrowsArgumentException(string? path)
    {
        var act = () => _fileSystem.CreateDirectory(path!);
        act.Should().Throw<ArgumentException>();
    }
}
