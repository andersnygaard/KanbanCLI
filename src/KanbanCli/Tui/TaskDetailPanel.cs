namespace KanbanCli.Tui;
using KanbanCli.Models;
using KanbanCli.Services;

public class TaskDetailPanel
{
    private readonly ITaskService _taskService;

    public TaskDetailPanel(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Shows task details and allows editing. Returns the (possibly updated) task.
    /// </summary>
    public TaskItem Show(TaskItem task)
    {
        var current = task;

        while (true)
        {
            RenderDetailView(current);
            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.T:
                    current = HandleEditTitle(current);
                    break;

                case ConsoleKey.L:
                    current = HandleEditLabels(current);
                    break;

                case ConsoleKey.P:
                    current = HandleEditPriority(current);
                    break;

                case ConsoleKey.Escape:
                    return current;

                default:
                    return current;
            }
        }
    }

    private void RenderDetailView(TaskItem task)
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
        row = RenderField("Created", task.CreatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "(unknown)", row);

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

        row++;
        RenderEditHints(row);
    }

    private static void RenderEditHints(int row)
    {
        Console.SetCursorPosition(0, row);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("  [T]");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("itle  ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[L]");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("abels  ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[P]");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("riority  ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[Esc]");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" back");
        Console.ResetColor();
        Console.WriteLine();
    }

    private TaskItem HandleEditTitle(TaskItem task)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  New title (empty to cancel): ");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;
        var newTitle = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.ResetColor();

        if (string.IsNullOrWhiteSpace(newTitle))
            return task;

        var updated = task with { Title = newTitle };
        _taskService.UpdateTask(updated);
        return updated;
    }

    private TaskItem HandleEditLabels(TaskItem task)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Labels: [A]dd or [R]emove?");
        Console.ResetColor();

        var key = Console.ReadKey(intercept: true);

        return key.Key switch
        {
            ConsoleKey.A => HandleAddLabel(task),
            ConsoleKey.R => HandleRemoveLabel(task),
            _ => task
        };
    }

    private TaskItem HandleAddLabel(TaskItem task)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Label to add: ");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;
        var label = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.ResetColor();

        if (string.IsNullOrWhiteSpace(label))
            return task;

        var updated = task.AddLabel(label);
        if (updated != task)
            _taskService.UpdateTask(updated);

        return updated;
    }

    private TaskItem HandleRemoveLabel(TaskItem task)
    {
        if (task.Labels.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  No labels to remove. Press any key...");
            Console.ResetColor();
            Console.ReadKey(intercept: true);
            return task;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Select label to remove:");
        Console.ResetColor();

        for (var i = 0; i < task.Labels.Count; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"    {i + 1}. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(task.Labels[i]);
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  Enter number (1-{task.Labels.Count}), or 0 to cancel: ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(input, out var choice) || choice < 1 || choice > task.Labels.Count)
            return task;

        var updated = task.RemoveLabel(task.Labels[choice - 1]);
        if (updated != task)
            _taskService.UpdateTask(updated);

        return updated;
    }

    private TaskItem HandleEditPriority(TaskItem task)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  Change priority (current: {task.Priority}):");
        Console.ResetColor();

        var priorities = Enum.GetValues<Priority>();
        for (var i = 0; i < priorities.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"    {i + 1}. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(priorities[i].ToString());
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  Enter number (1-{priorities.Length}), or 0 to cancel: ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(input, out var choice) || choice < 1 || choice > priorities.Length)
            return task;

        var newPriority = priorities[choice - 1];
        if (newPriority == task.Priority)
            return task;

        var updated = task.SetPriority(newPriority);
        _taskService.UpdateTask(updated);
        return updated;
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
