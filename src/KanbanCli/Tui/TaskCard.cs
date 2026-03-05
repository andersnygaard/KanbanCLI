namespace KanbanCli.Tui;
using KanbanCli.Models;

public class TaskCard
{
    public void RenderWithColors(TaskItem task, int columnX, int row, int columnWidth, bool isSelected)
    {
        Console.SetCursorPosition(columnX, row);

        var bgColor = isSelected ? ConsoleColor.DarkCyan : ConsoleColor.Black;
        var defaultFg = isSelected ? ConsoleColor.White : ConsoleColor.Gray;

        // ID part
        Console.BackgroundColor = bgColor;
        Console.ForegroundColor = defaultFg;
        Console.Write($"#{task.Id.ToString(BoardConstants.IdFormat)} ");

        // Type part (color-coded by task type)
        Console.ForegroundColor = TuiHelpers.GetTypeColor(task.Type);
        Console.BackgroundColor = bgColor;
        Console.Write(task.Type.ToString().ToUpperInvariant());
        Console.ForegroundColor = defaultFg;
        Console.Write(": ");

        // Calculate remaining width for title
        var idPart = $"#{task.Id.ToString(BoardConstants.IdFormat)} ";
        var typePart = task.Type.ToString().ToUpperInvariant() + ": ";
        var priorityPart = $" [{task.Priority}]";
        var labelsText = task.Labels.Count > 0 ? " " + string.Join(" ", task.Labels.Select(l => $"[{l}]")) : string.Empty;
        var overhead = idPart.Length + typePart.Length + priorityPart.Length + labelsText.Length;
        var titleMaxLen = Math.Max(1, columnWidth - overhead - 2);

        var truncatedTitle = task.Title.Length > titleMaxLen
            ? task.Title[..titleMaxLen]
            : task.Title;

        // Title
        Console.ForegroundColor = defaultFg;
        Console.BackgroundColor = bgColor;
        Console.Write(truncatedTitle);

        // Priority color
        Console.ForegroundColor = TuiHelpers.GetPriorityColor(task.Priority);
        Console.BackgroundColor = bgColor;
        Console.Write(priorityPart);

        // Labels
        if (task.Labels.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.BackgroundColor = bgColor;
            Console.Write(labelsText);
        }

        // Pad to fill column width
        var currentLen = idPart.Length + typePart.Length + truncatedTitle.Length + priorityPart.Length + labelsText.Length;
        var remaining = columnWidth - 1 - currentLen;
        if (remaining > 0)
        {
            Console.BackgroundColor = bgColor;
            Console.Write(new string(' ', remaining));
        }

        Console.ResetColor();
    }

}
