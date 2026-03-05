namespace KanbanCli.Tui;
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

public static class TuiHelpers
{
    public static ConsoleColor GetPriorityColor(Priority priority) => priority switch
    {
        Priority.High => ConsoleColor.Red,
        Priority.Medium => ConsoleColor.Yellow,
        Priority.Low => ConsoleColor.Green,
        _ => ConsoleColor.Gray
    };

    public static string FormatStatus(TaskStatus status) => status.ToDisplayString();

    public static void RenderHeader(string title, int windowWidth, ConsoleColor backgroundColor)
    {
        Console.SetCursorPosition(0, 0);
        Console.BackgroundColor = backgroundColor;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($" {title}".PadRight(windowWidth - 1));
        Console.ResetColor();
    }
}
