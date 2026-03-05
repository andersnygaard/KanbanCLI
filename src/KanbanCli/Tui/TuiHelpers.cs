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
            Priority.High => Theme.PriorityHigh,
            Priority.Medium => Theme.PriorityMedium,
            Priority.Low => Theme.PriorityLow,
            _ => Theme.PriorityDefault
        };
    }

    /// <summary>Returns the console color associated with the given <paramref name="type"/>.</summary>
    public static ConsoleColor GetTypeColor(TaskType type)
    {
        return type switch
        {
            TaskType.Feature => Theme.TypeFeature,
            TaskType.Bug => Theme.TypeBug,
            TaskType.Security => Theme.TypeSecurity,
            TaskType.Refactor => Theme.TypeRefactor,
            TaskType.Test => Theme.TypeTest,
            TaskType.Perf => Theme.TypePerf,
            TaskType.Docs => Theme.TypeDocs,
            TaskType.Design => Theme.TypeDesign,
            TaskType.Epic => Theme.TypeEpic,
            TaskType.Explore => Theme.TypeExplore,
            TaskType.Cleanup => Theme.TypeCleanup,
            TaskType.A11y => Theme.TypeA11y,
            TaskType.Quality => Theme.TypeQuality,
            _ => Theme.TypeDefault
        };
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
