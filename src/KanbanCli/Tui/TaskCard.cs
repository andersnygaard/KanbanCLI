namespace KanbanCli.Tui;
using KanbanCli.Models;

public class TaskCard : ITaskCard
{
    public void RenderWithColors(TaskItem task, int columnX, int row, int columnWidth, bool isSelected)
    {
        var bgColor = isSelected ? ConsoleColor.DarkCyan : ConsoleColor.Black;
        var defaultFg = isSelected ? ConsoleColor.White : ConsoleColor.Gray;

        // Line 1: Selection indicator + ID + Type
        RenderLine1(task, columnX, row, columnWidth, isSelected, bgColor, defaultFg);

        // Line 2: Title (indented)
        RenderLine2(task, columnX, row + 1, columnWidth, bgColor, defaultFg);

        // Line 3: Priority indicator + Labels
        RenderLine3(task, columnX, row + 2, columnWidth, bgColor, defaultFg);
    }

    private static void RenderLine1(TaskItem task, int columnX, int row, int columnWidth, bool isSelected, ConsoleColor bgColor, ConsoleColor defaultFg)
    {
        TuiHelpers.SafeSetCursorPosition(columnX, row);
        Console.BackgroundColor = bgColor;
        Console.ForegroundColor = defaultFg;

        var indicator = isSelected ? "\u25B6" : " "; // ▶ or space
        var prefix = $" {indicator} #{task.Id.ToString(BoardConstants.IdFormat)} ";
        Console.Write(prefix);

        // Type part (color-coded)
        Console.ForegroundColor = TuiHelpers.GetTypeColor(task.Type);
        Console.BackgroundColor = bgColor;
        var typeText = task.Type.ToString().ToUpperInvariant();
        Console.Write(typeText);

        // Pad remainder
        var written = prefix.Length + typeText.Length;
        PadToWidth(written, columnWidth, bgColor);
    }

    private static void RenderLine2(TaskItem task, int columnX, int row, int columnWidth, ConsoleColor bgColor, ConsoleColor defaultFg)
    {
        TuiHelpers.SafeSetCursorPosition(columnX, row);
        Console.BackgroundColor = bgColor;
        Console.ForegroundColor = defaultFg;

        const string indent = "   ";
        var titleMaxLen = Math.Max(1, columnWidth - indent.Length - 1);
        var truncatedTitle = task.Title.Length > titleMaxLen
            ? task.Title[..titleMaxLen]
            : task.Title;

        var lineText = indent + truncatedTitle;
        Console.Write(lineText);

        PadToWidth(lineText.Length, columnWidth, bgColor);
    }

    private static void RenderLine3(TaskItem task, int columnX, int row, int columnWidth, ConsoleColor bgColor, ConsoleColor defaultFg)
    {
        TuiHelpers.SafeSetCursorPosition(columnX, row);
        Console.BackgroundColor = bgColor;

        const string indent = "   ";
        Console.Write(indent);
        var written = indent.Length;

        // Priority symbol + text
        var prioritySymbol = TuiHelpers.GetPrioritySymbol(task.Priority);
        Console.ForegroundColor = TuiHelpers.GetPriorityColor(task.Priority);
        Console.BackgroundColor = bgColor;
        var priorityText = $"{prioritySymbol} {task.Priority}";
        Console.Write(priorityText);
        written += priorityText.Length;

        // Labels
        if (task.Labels.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.BackgroundColor = bgColor;
            foreach (var label in task.Labels)
            {
                var labelText = $"  [{label}]";
                if (written + labelText.Length >= columnWidth)
                    break;
                Console.Write(labelText);
                written += labelText.Length;
            }
        }

        PadToWidth(written, columnWidth, bgColor);
    }

    private static void PadToWidth(int written, int columnWidth, ConsoleColor bgColor)
    {
        var remaining = columnWidth - written;
        if (remaining > 0)
        {
            Console.BackgroundColor = bgColor;
            Console.Write(new string(' ', remaining));
        }

        Console.ResetColor();
    }
}
