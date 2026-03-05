namespace KanbanCli.Tui;
using KanbanCli.Models;

public class TaskDetailPanel
{
    public void Show(TaskItem task)
    {
        Console.Clear();
        Console.CursorVisible = true;

        var windowWidth = Math.Max(Console.WindowWidth, 40);

        TuiHelpers.RenderHeader("Task Details", windowWidth, ConsoleColor.DarkBlue);

        var row = 2;
        row = RenderField("ID", $"#{task.Id:D3}", row);
        row = RenderField("Title", task.Title, row);
        row = RenderField("Type", task.Type.ToString(), row);
        row = RenderField("Priority", task.Priority.ToString(), row, TuiHelpers.GetPriorityColor(task.Priority));
        row = RenderField("Status", TuiHelpers.FormatStatus(task.Status), row);
        row = RenderField("Labels", task.Labels.Count > 0 ? string.Join(", ", task.Labels) : "(none)", row);
        row = RenderField("Created", task.CreatedDate.ToString("yyyy-MM-dd HH:mm"), row);

        if (task.CompletedDate.HasValue)
            row = RenderField("Completed", task.CompletedDate.Value.ToString("yyyy-MM-dd HH:mm"), row);

        row++;
        RenderSeparator(row, windowWidth);
        row++;

        foreach (var section in task.Sections)
        {
            row = RenderSectionHeading(section.Key, row);
            row = RenderSectionContent(section.Value, row);
        }

        Console.SetCursorPosition(0, row);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Press any key to go back...");
        Console.ResetColor();

        Console.ReadKey(intercept: true);
    }

    private static int RenderField(string label, string value, int row, ConsoleColor? valueColor = null)
    {
        Console.SetCursorPosition(0, row);
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write($"  {label,-12}: ");

        Console.ForegroundColor = valueColor ?? ConsoleColor.White;
        Console.WriteLine(value);
        Console.ResetColor();

        return row + 1;
    }

    private static void RenderSeparator(int row, int windowWidth)
    {
        Console.SetCursorPosition(0, row);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(new string('-', Math.Min(windowWidth - 1, 60)));
        Console.ResetColor();
    }

    private static int RenderSectionHeading(string heading, int row)
    {
        Console.SetCursorPosition(0, row);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  ## {heading}");
        Console.ResetColor();
        return row + 1;
    }

    private static int RenderSectionContent(string content, int row)
    {
        if (string.IsNullOrWhiteSpace(content))
            return row;

        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            Console.SetCursorPosition(0, row);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"    {line.TrimEnd('\r')}");
            Console.ResetColor();
            row++;
        }

        return row;
    }

}
