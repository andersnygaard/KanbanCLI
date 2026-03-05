using FluentAssertions;
using KanbanCli.Models;
using KanbanCli.Tui;

namespace KanbanCli.Tests.Tui;

public class TuiHelpersTests
{
    [Fact]
    public void GetEffectiveWidth_WithNoEnvVar_ReturnsValueWithinClampedRange()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable(BoardConstants.WidthEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(BoardConstants.WidthEnvVar, null);

            // Act
            var result = TuiHelpers.GetEffectiveWidth();

            // Assert
            result.Should().BeGreaterThanOrEqualTo(BoardConstants.MinWindowWidth);
            result.Should().BeLessThanOrEqualTo(BoardConstants.MaxBoardWidth);
        }
        finally
        {
            Environment.SetEnvironmentVariable(BoardConstants.WidthEnvVar, originalValue);
        }
    }

    [Fact]
    public void GetEffectiveWidth_WithValidEnvVar_ReturnsEnvVarValue()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable(BoardConstants.WidthEnvVar);
        var expectedWidth = 80;

        try
        {
            Environment.SetEnvironmentVariable(BoardConstants.WidthEnvVar, expectedWidth.ToString());

            // Act
            var result = TuiHelpers.GetEffectiveWidth();

            // Assert
            result.Should().Be(expectedWidth);
        }
        finally
        {
            Environment.SetEnvironmentVariable(BoardConstants.WidthEnvVar, originalValue);
        }
    }

    [Fact]
    public void GetEffectiveWidth_WithEnvVarBelowMinWidth_FallsBackToConsoleWidth()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable(BoardConstants.WidthEnvVar);
        var tooSmallWidth = BoardConstants.MinWindowWidth - 1;

        try
        {
            Environment.SetEnvironmentVariable(BoardConstants.WidthEnvVar, tooSmallWidth.ToString());

            // Act
            var result = TuiHelpers.GetEffectiveWidth();

            // Assert — should fall back to clamped console width, not the env var value
            result.Should().BeGreaterThanOrEqualTo(BoardConstants.MinWindowWidth);
            result.Should().BeLessThanOrEqualTo(BoardConstants.MaxBoardWidth);
        }
        finally
        {
            Environment.SetEnvironmentVariable(BoardConstants.WidthEnvVar, originalValue);
        }
    }

    [Fact]
    public void GetEffectiveWidth_WithNonNumericEnvVar_FallsBackToConsoleWidth()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable(BoardConstants.WidthEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(BoardConstants.WidthEnvVar, "not-a-number");

            // Act
            var result = TuiHelpers.GetEffectiveWidth();

            // Assert
            result.Should().BeGreaterThanOrEqualTo(BoardConstants.MinWindowWidth);
            result.Should().BeLessThanOrEqualTo(BoardConstants.MaxBoardWidth);
        }
        finally
        {
            Environment.SetEnvironmentVariable(BoardConstants.WidthEnvVar, originalValue);
        }
    }

    [Fact]
    public void GetEffectiveHeight_ReturnsAtLeastMinWindowHeight()
    {
        // Act
        var result = TuiHelpers.GetEffectiveHeight();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(BoardConstants.MinWindowHeight);
    }
}
