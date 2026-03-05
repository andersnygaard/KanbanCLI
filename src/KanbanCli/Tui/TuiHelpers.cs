namespace KanbanCli.Tui;
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

public static class TuiHelpers
{
    /// <summary>Number of rendered lines per task card (ID+Type, Title, Priority+Labels).</summary>
    public const int CardLineCount = 3;

    /// <summary>Blank separator lines between cards.</summary>
    public const int CardSeparatorLines = 1;

    /// <summary>Total vertical space per card (content lines + separator).</summary>
    public const int CardTotalHeight = CardLineCount + CardSeparatorLines;

    /// <summary>Returns the Unicode priority indicator symbol for the given <paramref name="priority"/>.</summary>
    public static string GetPrioritySymbol(Priority priority)
    {
        return priority switch
        {
            Priority.High => "\u25CF",    // ●
            Priority.Medium => "\u25D0",  // ◐
            Priority.Low => "\u25CB",     // ○
            _ => " "
        };
    }

    /// <summary>Returns the console color associated with the given <paramref name="priority"/> level.</summary>
    public static ConsoleColor GetPriorityColor(Priority priority)
    {
        return priority switch
        {
            Priority.High => ConsoleColor.Red,
            Priority.Medium => ConsoleColor.Yellow,
            Priority.Low => ConsoleColor.Green,
            _ => ConsoleColor.Gray
        };
    }

    /// <summary>Returns the console color associated with the given <paramref name="type"/>.</summary>
    public static ConsoleColor GetTypeColor(TaskType type)
    {
        return type switch
        {
            TaskType.Feature => ConsoleColor.Cyan,
            TaskType.Bug => ConsoleColor.Red,
            TaskType.Security => ConsoleColor.DarkYellow,
            TaskType.Refactor => ConsoleColor.Green,
            TaskType.Test => ConsoleColor.Magenta,
            TaskType.Perf => ConsoleColor.DarkCyan,
            TaskType.Docs => ConsoleColor.Blue,
            TaskType.Design => ConsoleColor.DarkMagenta,
            TaskType.Epic => ConsoleColor.White,
            TaskType.Explore => ConsoleColor.DarkGreen,
            TaskType.Cleanup => ConsoleColor.DarkGray,
            TaskType.A11y => ConsoleColor.DarkYellow,
            TaskType.Quality => ConsoleColor.Yellow,
            _ => ConsoleColor.Gray
        };
    }

    /// <summary>Formats a <paramref name="status"/> value as a human-readable display string.</summary>
    public static string FormatStatus(TaskStatus status)
    {
        return status.ToDisplayString();
    }

    /// <summary>
    /// Sets the cursor position, clamping coordinates to valid console bounds.
    /// Prevents ArgumentOutOfRangeException on very small terminals.
    /// </summary>
    public static void SafeSetCursorPosition(int x, int y)
    {
        try
        {
            var safeX = Math.Clamp(x, 0, Math.Max(Console.WindowWidth - 1, 0));
            var safeY = Math.Clamp(y, 0, Math.Max(Console.WindowHeight - 1, 0));
            Console.SetCursorPosition(safeX, safeY);
        }
        catch (IOException)
        {
            // Terminal may have been resized between check and set
        }
    }

    /// <summary>
    /// Returns the effective board width by reading the actual terminal width,
    /// capping it to BoardConstants.MaxBoardWidth, and respecting the KANBAN_WIDTH
    /// environment variable if set.
    /// </summary>
    public static int GetEffectiveWidth()
    {
        var envValue = Environment.GetEnvironmentVariable(BoardConstants.WidthEnvVar);
        if (!string.IsNullOrEmpty(envValue) && int.TryParse(envValue, out var envWidth) && envWidth >= BoardConstants.MinWindowWidth)
        {
            return envWidth;
        }

        var consoleWidth = TryGetConsoleWidth();
        return Math.Clamp(consoleWidth, BoardConstants.MinWindowWidth, BoardConstants.MaxBoardWidth);
    }

    /// <summary>
    /// Returns the effective board height from the terminal.
    /// </summary>
    public static int GetEffectiveHeight()
    {
        try
        {
            return Math.Max(Console.WindowHeight, BoardConstants.MinWindowHeight);
        }
        catch (IOException)
        {
            return BoardConstants.MinWindowHeight;
        }
    }

    private static int TryGetConsoleWidth()
    {
        try
        {
            return Console.WindowWidth;
        }
        catch (IOException)
        {
            return BoardConstants.MaxBoardWidth;
        }
    }

    /// <summary>Renders a colored header bar with <paramref name="title"/> spanning the full <paramref name="windowWidth"/>.</summary>
    public static void RenderHeader(string title, int windowWidth, ConsoleColor backgroundColor)
    {
        SafeSetCursorPosition(0, 0);
        Console.BackgroundColor = backgroundColor;
        Console.ForegroundColor = Theme.BoardTitleFg;
        Console.Write($" {title}".PadRight(windowWidth - 1));
        Console.ResetColor();
    }
}
