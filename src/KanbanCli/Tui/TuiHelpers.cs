namespace KanbanCli.Tui;
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

public static class TuiHelpers
{
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
