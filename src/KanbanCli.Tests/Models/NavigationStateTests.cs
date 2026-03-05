using FluentAssertions;
using KanbanCli.Tui;

namespace KanbanCli.Tests.Models;

public class NavigationStateTests
{
    [Fact]
    public void MoveToColumn_NegativeDirection_ClampsToZero()
    {
        // Arrange
        var state = new NavigationState { SelectedColumn = 0, SelectedTask = 0 };

        // Act
        var result = state.MoveToColumn(-1, 4);

        // Assert
        result.SelectedColumn.Should().Be(0);
    }

    [Fact]
    public void MoveToColumn_BeyondMax_ClampsToLastColumn()
    {
        // Arrange
        var state = new NavigationState { SelectedColumn = 3, SelectedTask = 0 };

        // Act
        var result = state.MoveToColumn(1, 4);

        // Assert
        result.SelectedColumn.Should().Be(3);
    }

    [Fact]
    public void MoveToColumn_ResetsSelectedTask()
    {
        // Arrange
        var state = new NavigationState { SelectedColumn = 0, SelectedTask = 5 };

        // Act
        var result = state.MoveToColumn(1, 4);

        // Assert
        result.SelectedTask.Should().Be(0);
    }

    [Fact]
    public void MoveToTask_ZeroTasks_ReturnsSameState()
    {
        // Arrange
        var state = new NavigationState { SelectedColumn = 0, SelectedTask = 0 };

        // Act
        var result = state.MoveToTask(1, 0);

        // Assert
        result.SelectedTask.Should().Be(0);
    }

    [Fact]
    public void MoveToTask_BeyondMax_ClampsToLastTask()
    {
        // Arrange
        var state = new NavigationState { SelectedColumn = 0, SelectedTask = 2 };

        // Act
        var result = state.MoveToTask(1, 3);

        // Assert
        result.SelectedTask.Should().Be(2);
    }

    [Fact]
    public void MoveToTask_NegativeDirection_ClampsToZero()
    {
        // Arrange
        var state = new NavigationState { SelectedColumn = 0, SelectedTask = 0 };

        // Act
        var result = state.MoveToTask(-1, 5);

        // Assert
        result.SelectedTask.Should().Be(0);
    }

    [Fact]
    public void MoveToTask_ValidDirection_MovesCorrectly()
    {
        // Arrange
        var state = new NavigationState { SelectedColumn = 0, SelectedTask = 1 };

        // Act
        var result = state.MoveToTask(1, 5);

        // Assert
        result.SelectedTask.Should().Be(2);
    }
}
