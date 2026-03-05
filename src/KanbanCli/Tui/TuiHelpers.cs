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
    public static string GetPrioritySymbol(Priority priority) => priority switch
    {
        Priority.High => "\u25CF",    // ●
        Priority.Medium => "\u25D0",  // ◐
        Priority.Low => "\u25CB",     // ○
        _ => " "
    };

    /// <summary>Returns the console color associated with the given <paramref name="priority"/> level.</summary>
    public static ConsoleColor GetPriorityColor(Priority priority) => priority switch
    {
        Priority.High => ConsoleColor.Red,
        Priority.Medium => ConsoleColor.Yellow,
        Priority.Low => ConsoleColor.Green,
        _ => ConsoleColor.Gray
    };

    /// <summary>Returns the console color associated with the given <paramref name="type"/>.</summary>
    public static ConsoleColor GetTypeColor(TaskType type) => type switch
    {
        TaskType.Feature => ConsoleColor.Cyan,
        TaskType.Bug => ConsoleColor.Red,
        TaskType.Security => ConsoleColor.DarkYellow,
        TaskType.Refactor => ConsoleColor.Green,
        TaskType.Test => ConsoleColor.Magenta,
        TaskType.Perf => ConsoleColor.DarkCyan,
        TaskType.Docs => ConsoleColor.Blue,
        _ => ConsoleColor.Gray
    };

    /// <summary>Formats a <paramref name="status"/> value as a human-readable display string.</summary>
    public static string FormatStatus(TaskStatus status) => status.ToDisplayString();

    /// <summary>Renders a colored header bar with <paramref name="title"/> spanning the full <paramref name="windowWidth"/>.</summary>
    public static void RenderHeader(string title, int windowWidth, ConsoleColor backgroundColor)
    {
        Console.SetCursorPosition(0, 0);
        Console.BackgroundColor = backgroundColor;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($" {title}".PadRight(windowWidth - 1));
        Console.ResetColor();
    }
}
