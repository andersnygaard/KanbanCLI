using FluentAssertions;
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests.Models;

public class BoardAndColumnTests
{
    [Fact]
    public void Column_IsEmpty_WhenNoTasks_ReturnsTrue()
    {
        var column = new Column { Name = "Backlog", Status = TaskStatus.Backlog, Tasks = [] };

        column.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Column_IsEmpty_WhenHasTasks_ReturnsFalse()
    {
        var column = new Column
        {
            Name = "Backlog",
            Status = TaskStatus.Backlog,
            Tasks = [new TaskItem { Id = 1, Title = "Task 1" }]
        };

        column.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Board_TotalTaskCount_EmptyBoard_ReturnsZero()
    {
        var board = new Board
        {
            Columns =
            [
                new Column { Name = "Backlog", Status = TaskStatus.Backlog, Tasks = [] },
                new Column { Name = "Done", Status = TaskStatus.Done, Tasks = [] }
            ]
        };

        board.TotalTaskCount.Should().Be(0);
    }

    [Fact]
    public void Board_TotalTaskCount_SumsAcrossColumns()
    {
        var board = new Board
        {
            Columns =
            [
                new Column
                {
                    Name = "Backlog",
                    Status = TaskStatus.Backlog,
                    Tasks = [new TaskItem { Id = 1 }, new TaskItem { Id = 2 }]
                },
                new Column
                {
                    Name = "In Progress",
                    Status = TaskStatus.InProgress,
                    Tasks = [new TaskItem { Id = 3 }]
                },
                new Column { Name = "Done", Status = TaskStatus.Done, Tasks = [] }
            ]
        };

        board.TotalTaskCount.Should().Be(3);
    }

    [Fact]
    public void Board_GetColumn_ExistingStatus_ReturnsColumn()
    {
        var backlogColumn = new Column { Name = "Backlog", Status = TaskStatus.Backlog, Tasks = [] };
        var doneColumn = new Column { Name = "Done", Status = TaskStatus.Done, Tasks = [] };
        var board = new Board { Columns = [backlogColumn, doneColumn] };

        var result = board.GetColumn(TaskStatus.Backlog);

        result.Should().BeSameAs(backlogColumn);
    }

    [Fact]
    public void Board_GetColumn_NonExistentStatus_ReturnsNull()
    {
        var board = new Board
        {
            Columns = [new Column { Name = "Backlog", Status = TaskStatus.Backlog, Tasks = [] }]
        };

        var result = board.GetColumn(TaskStatus.OnHold);

        result.Should().BeNull();
    }

    [Fact]
    public void Board_TotalTaskCount_NoColumns_ReturnsZero()
    {
        var board = new Board { Columns = [] };

        board.TotalTaskCount.Should().Be(0);
    }

    [Fact]
    public void Board_GetColumn_EmptyBoard_ReturnsNull()
    {
        var board = new Board { Columns = [] };

        board.GetColumn(TaskStatus.Backlog).Should().BeNull();
    }
}
