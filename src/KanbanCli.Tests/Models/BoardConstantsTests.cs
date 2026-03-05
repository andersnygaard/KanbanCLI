using FluentAssertions;
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests.Models;

public class BoardConstantsTests
{
    [Fact]
    public void FolderNames_ContainsAllTaskStatuses()
    {
        // Arrange
        var allStatuses = Enum.GetValues<TaskStatus>();

        // Act & Assert
        foreach (var status in allStatuses)
        {
            BoardConstants.FolderNames.Should().ContainKey(status);
        }
    }

    [Fact]
    public void FolderNames_BacklogFolder_IsCorrect()
    {
        BoardConstants.FolderNames[TaskStatus.Backlog].Should().Be("backlog");
    }

    [Fact]
    public void FolderNames_InProgressFolder_IsCorrect()
    {
        BoardConstants.FolderNames[TaskStatus.InProgress].Should().Be("in-progress");
    }

    [Fact]
    public void FolderNames_DoneFolder_IsCorrect()
    {
        BoardConstants.FolderNames[TaskStatus.Done].Should().Be("done");
    }

    [Fact]
    public void FolderNames_OnHoldFolder_IsCorrect()
    {
        BoardConstants.FolderNames[TaskStatus.OnHold].Should().Be("on-hold");
    }

    [Fact]
    public void ColumnDisplayNames_ContainsAllTaskStatuses()
    {
        var allStatuses = Enum.GetValues<TaskStatus>();

        foreach (var status in allStatuses)
        {
            BoardConstants.ColumnDisplayNames.Should().ContainKey(status);
        }
    }

    [Fact]
    public void ColumnOrder_ContainsAllTaskStatuses()
    {
        var allStatuses = Enum.GetValues<TaskStatus>();

        BoardConstants.ColumnOrder.Should().HaveCount(allStatuses.Length);
        foreach (var status in allStatuses)
        {
            BoardConstants.ColumnOrder.Should().Contain(status);
        }
    }

    [Fact]
    public void ColumnOrder_FirstColumnIsBacklog()
    {
        BoardConstants.ColumnOrder[0].Should().Be(TaskStatus.Backlog);
    }

    [Fact]
    public void IdFormat_ProducesThreeDigitPaddedId()
    {
        var result = 5.ToString(BoardConstants.IdFormat);

        result.Should().Be("005");
    }

    [Fact]
    public void DateFormat_ProducesIso8601Date()
    {
        var date = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc);

        var result = date.ToString(BoardConstants.DateFormat);

        result.Should().Be("2026-03-05");
    }

    [Fact]
    public void TaskFileExtension_IsMarkdownGlob()
    {
        BoardConstants.TaskFileExtension.Should().Be("*.md");
    }

    [Fact]
    public void MinWindowWidth_IsPositive()
    {
        BoardConstants.MinWindowWidth.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MinWindowHeight_IsPositive()
    {
        BoardConstants.MinWindowHeight.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MaxTopPriorities_IsPositive()
    {
        BoardConstants.MaxTopPriorities.Should().BeGreaterThan(0);
    }
}
