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

        var width = DialogHelper.GetBoxWidth();
        var borderColor = ConsoleColor.DarkGray;

        DialogHelper.RenderBoxTop($"Task #{task.Id:D3}", width, borderColor);
        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        RenderMetadataFields(task, width, borderColor);

        DialogHelper.RenderBoxEmptyLine(width, borderColor);
        DialogHelper.RenderBoxSeparator(width, borderColor);

        RenderSections(task.Sections, width, borderColor);

        DialogHelper.RenderBoxSeparator(width, borderColor);
        RenderEditHints(width, borderColor);
        DialogHelper.RenderBoxBottom(width, borderColor);
    }

    private static void RenderMetadataFields(TaskItem task, int width, ConsoleColor borderColor)
    {
        RenderField("ID", $"#{task.Id:D3}", width, borderColor);
        RenderField("Title", task.Title, width, borderColor);
        RenderField("Type", task.Type.ToString(), width, borderColor);
        RenderField("Priority", task.Priority.ToString(), width, borderColor, TuiHelpers.GetPriorityColor(task.Priority));
        RenderField("Status", TuiHelpers.FormatStatus(task.Status), width, borderColor);

        var labelsText = task.Labels.Count > 0
            ? string.Join("  ", task.Labels.Select(l => $"[{l}]"))
            : "(none)";
        RenderField("Labels", labelsText, width, borderColor);

        RenderField("Created", task.CreatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "(unknown)", width, borderColor);

        if (task.CompletedDate.HasValue)
            RenderField("Completed", task.CompletedDate.Value.ToString("yyyy-MM-dd HH:mm"), width, borderColor);
    }

    private static void RenderSections(IReadOnlyDictionary<string, string> sections, int width, ConsoleColor borderColor)
    {
        foreach (var section in sections)
        {
            RenderSectionHeading(section.Key, width, borderColor);
            RenderSectionContent(section.Value, width, borderColor);
        }
    }

    private static void RenderEditHints(int width, ConsoleColor borderColor)
    {
        DialogHelper.RenderBoxLeftBorder(borderColor);

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[T]");
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

        // "[T]itle  [L]abels  [P]riority  [Esc] back" = 42 chars
        DialogHelper.RenderBoxRightBorder(42, width, borderColor);
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

        RenderNumberedList(task.Labels);

        var choice = PromptForChoice(task.Labels.Count);
        if (choice is null)
            return task;

        var updated = task.RemoveLabel(task.Labels[choice.Value - 1]);
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
        var priorityNames = priorities.Select(p => p.ToString()).ToList();
        RenderNumberedList(priorityNames);

        var choice = PromptForChoice(priorities.Length);
        if (choice is null)
            return task;

        var newPriority = priorities[choice.Value - 1];
        if (newPriority == task.Priority)
            return task;

        var updated = task.SetPriority(newPriority);
        _taskService.UpdateTask(updated);
        return updated;
    }

    private static void RenderNumberedList(IReadOnlyList<string> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"    {i + 1}. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(items[i]);
        }

        Console.ResetColor();
    }

    private static int? PromptForChoice(int maxValue)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  Enter number (1-{maxValue}), or 0 to cancel: ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(input, out var choice) || choice < 1 || choice > maxValue)
            return null;

        return choice;
    }

    private static void RenderField(string label, string value, int width, ConsoleColor borderColor, ConsoleColor? valueColor = null)
    {
        DialogHelper.RenderBoxLeftBorder(borderColor);

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write($"{label,-12}");

        Console.ForegroundColor = valueColor ?? ConsoleColor.White;
        Console.Write(value);

        var contentLen = label.Length + value.Length;
        if (label.Length < 12)
            contentLen = 12 + value.Length;

        DialogHelper.RenderBoxRightBorder(contentLen, width, borderColor);
    }

    private static void RenderSectionHeading(string heading, int width, ConsoleColor borderColor)
    {
        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = ConsoleColor.Cyan;
        var text = $"## {heading}";
        Console.Write(text);
        DialogHelper.RenderBoxRightBorder(text.Length, width, borderColor);
    }

    private static void RenderSectionContent(string content, int width, ConsoleColor borderColor)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        var maxContentWidth = width - 6;
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.TrimEnd('\r');
            var displayLine = $"  {trimmed}";
            if (displayLine.Length > maxContentWidth)
                displayLine = displayLine[..maxContentWidth];

            DialogHelper.RenderBoxLine(displayLine, width, borderColor);
        }

        DialogHelper.RenderBoxEmptyLine(width, borderColor);
    }
}
