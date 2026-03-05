namespace KanbanCli.Tui;
using KanbanCli.Models;

public class TaskCard
{
    public void Render(TaskItem task, int columnX, int row, int columnWidth, bool isSelected)
    {
        Console.SetCursorPosition(columnX, row);

        if (isSelected)
        {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.White;
        }

        var line = BuildCardLine(task, columnWidth);
        var padded = line.PadRight(columnWidth - 1);

        // Write ID and type prefix in default (or selected) color
        Console.Write(padded);

        Console.ResetColor();
    }

    private static string BuildCardLine(TaskItem task, int columnWidth)
    {
        var idPart = $"#{task.Id:D3}";
        var typePart = task.Type.ToString().ToUpperInvariant();
        var priorityPart = $"[{task.Priority}]";

        var labelsText = task.Labels.Count > 0
            ? string.Join(", ", task.Labels)
            : string.Empty;

        // Estimate space used by fixed parts: "#001 TYPE: [High] labels"
        var fixedPart = $"{idPart} {typePart}: {priorityPart}";
        var labelSection = labelsText.Length > 0 ? $" {labelsText}" : string.Empty;

        // Reserve space: id(4) + space(1) + type + colon+space(2) + title + space(1) + priority + space(1) + labels
        var overhead = idPart.Length + 1 + typePart.Length + 2 + 1 + priorityPart.Length + labelSection.Length;
        var titleMaxLen = Math.Max(1, columnWidth - overhead - 2);

        var truncatedTitle = task.Title.Length > titleMaxLen
            ? task.Title[..titleMaxLen]
            : task.Title;

        return $"{idPart} {typePart}: {truncatedTitle} {priorityPart}{labelSection}";
    }

    public void RenderWithColors(TaskItem task, int columnX, int row, int columnWidth, bool isSelected)
    {
        Console.SetCursorPosition(columnX, row);

        var bgColor = isSelected ? ConsoleColor.DarkCyan : ConsoleColor.Black;
        var defaultFg = isSelected ? ConsoleColor.White : ConsoleColor.Gray;

        // ID part
        Console.BackgroundColor = bgColor;
        Console.ForegroundColor = defaultFg;
        Console.Write($"#{task.Id:D3} ");

        // Type part
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.BackgroundColor = bgColor;
        Console.Write(task.Type.ToString().ToUpperInvariant());
        Console.Write(": ");

        // Calculate remaining width for title
        var idPart = $"#{task.Id:D3} ";
        var typePart = task.Type.ToString().ToUpperInvariant() + ": ";
        var priorityPart = $" [{task.Priority}]";
        var labelsText = task.Labels.Count > 0 ? " " + string.Join(", ", task.Labels) : string.Empty;
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
