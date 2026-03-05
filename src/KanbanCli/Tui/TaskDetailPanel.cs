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
        var scrollOffset = 0;

        while (true)
        {
            RenderDetailView(current, scrollOffset);
            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.T:
                    current = HandleEditTitle(current);
                    scrollOffset = 0;
                    break;

                case ConsoleKey.L:
                    current = HandleEditLabels(current);
                    scrollOffset = 0;
                    break;

                case ConsoleKey.P:
                    current = HandleEditPriority(current);
                    scrollOffset = 0;
                    break;

                case ConsoleKey.UpArrow:
                    scrollOffset = Math.Max(0, scrollOffset - 1);
                    break;

                case ConsoleKey.DownArrow:
                    scrollOffset++;
                    break;

                case ConsoleKey.PageUp:
                    scrollOffset = Math.Max(0, scrollOffset - 10);
                    break;

                case ConsoleKey.PageDown:
                    scrollOffset += 10;
                    break;

                case ConsoleKey.Home:
                    scrollOffset = 0;
                    break;

                case ConsoleKey.Escape:
                    return current;

                default:
                    return current;
            }
        }
    }

    private void RenderDetailView(TaskItem task, int scrollOffset)
    {
        Console.Clear();
        Console.CursorVisible = false;

        var width = DialogHelper.GetBoxWidth();
        var borderColor = ConsoleColor.DarkGray;
        var visibleHeight = Math.Max(Console.WindowHeight - 4, 10); // Reserve space for top border, edit hints, bottom border

        // Build all content lines into a list
        var contentLines = BuildContentLines(task, width, borderColor);

        // Clamp scroll offset
        var maxScroll = Math.Max(0, contentLines.Count - visibleHeight);
        if (scrollOffset > maxScroll) scrollOffset = maxScroll;

        // Render top border
        DialogHelper.RenderBoxTop($"Task #{task.Id:D3}", width, borderColor);

        // Render visible content lines
        var endLine = Math.Min(scrollOffset + visibleHeight, contentLines.Count);
        for (var i = scrollOffset; i < endLine; i++)
        {
            contentLines[i].Render();
        }

        // Fill remaining space with empty lines
        var rendered = endLine - scrollOffset;
        for (var i = rendered; i < visibleHeight; i++)
        {
            DialogHelper.RenderBoxEmptyLine(width, borderColor);
        }

        // Scroll indicator
        DialogHelper.RenderBoxSeparator(width, borderColor);
        if (contentLines.Count > visibleHeight)
        {
            RenderScrollStatus(scrollOffset, maxScroll, width, borderColor);
        }

        RenderEditHints(width, borderColor);
        DialogHelper.RenderBoxBottom(width, borderColor);
    }

    private static List<ContentLine> BuildContentLines(TaskItem task, int width, ConsoleColor borderColor)
    {
        var lines = new List<ContentLine>();

        // Empty line
        lines.Add(ContentLine.Empty(width, borderColor));

        // Metadata fields
        lines.Add(ContentLine.Field("ID", $"#{task.Id:D3}", width, borderColor));
        lines.Add(ContentLine.Field("Title", task.Title, width, borderColor));
        lines.Add(ContentLine.Field("Type", task.Type.ToString(), width, borderColor));
        lines.Add(ContentLine.Field("Priority", task.Priority.ToString(), width, borderColor, TuiHelpers.GetPriorityColor(task.Priority)));
        lines.Add(ContentLine.Field("Status", TuiHelpers.FormatStatus(task.Status), width, borderColor));

        var labelsText = task.Labels.Count > 0
            ? string.Join("  ", task.Labels.Select(l => $"[{l}]"))
            : "(none)";
        lines.Add(ContentLine.Field("Labels", labelsText, width, borderColor));
        lines.Add(ContentLine.Field("Created", task.CreatedDate?.ToString("yyyy-MM-dd HH:mm") ?? "(unknown)", width, borderColor));
        if (task.CompletedDate.HasValue)
            lines.Add(ContentLine.Field("Completed", task.CompletedDate.Value.ToString("yyyy-MM-dd HH:mm"), width, borderColor));

        lines.Add(ContentLine.Empty(width, borderColor));
        lines.Add(ContentLine.Separator(width, borderColor));

        // Sections
        foreach (var section in task.Sections)
        {
            lines.Add(ContentLine.Heading(section.Key, width, borderColor));
            lines.Add(ContentLine.Empty(width, borderColor));

            if (!string.IsNullOrWhiteSpace(section.Value))
            {
                var maxContentWidth = width - 6;
                var sectionLines = section.Value.Split('\n');
                foreach (var line in sectionLines)
                {
                    var trimmed = line.TrimEnd('\r');
                    var displayLine = $"  {trimmed}";
                    if (displayLine.Length > maxContentWidth)
                        displayLine = displayLine[..maxContentWidth];
                    lines.Add(ContentLine.Text(displayLine, width, borderColor));
                }
            }

            lines.Add(ContentLine.Empty(width, borderColor));
        }

        return lines;
    }

    private static void RenderScrollStatus(int scrollOffset, int maxScroll, int width, ConsoleColor borderColor)
    {
        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        var scrollText = $"\u2191\u2193 Scroll  PgUp/PgDn  Home  ({scrollOffset + 1}/{maxScroll + 1})";
        Console.Write(scrollText);
        DialogHelper.RenderBoxRightBorder(scrollText.Length, width, borderColor);
    }

    // RenderMetadataFields and RenderSections replaced by BuildContentLines above

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

        var width = DialogHelper.GetBoxWidth();
        DialogHelper.RenderNumberedListInBox(task.Labels, width, ConsoleColor.DarkGray);

        var choice = DialogHelper.PromptNumericChoice(task.Labels.Count, allowZeroCancel: true);
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
        var width = DialogHelper.GetBoxWidth();
        DialogHelper.RenderNumberedListInBox(priorityNames, width, ConsoleColor.DarkGray);

        var choice = DialogHelper.PromptNumericChoice(priorities.Length, allowZeroCancel: true);
        if (choice is null)
            return task;

        var newPriority = priorities[choice.Value - 1];
        if (newPriority == task.Priority)
            return task;

        var updated = task.SetPriority(newPriority);
        _taskService.UpdateTask(updated);
        return updated;
    }

}

/// <summary>
/// Represents a single renderable line in the detail panel content buffer.
/// Used for scroll support — all content is pre-built as ContentLines, then only visible lines are rendered.
/// </summary>
internal class ContentLine
{
    private readonly Action _renderAction;

    private ContentLine(Action renderAction)
    {
        _renderAction = renderAction;
    }

    public void Render() => _renderAction();

    public static ContentLine Empty(int width, ConsoleColor borderColor) =>
        new(() => DialogHelper.RenderBoxEmptyLine(width, borderColor));

    public static ContentLine Separator(int width, ConsoleColor borderColor) =>
        new(() => DialogHelper.RenderBoxSeparator(width, borderColor));

    public static ContentLine Text(string text, int width, ConsoleColor borderColor) =>
        new(() => DialogHelper.RenderBoxLine(text, width, borderColor));

    public static ContentLine Heading(string heading, int width, ConsoleColor borderColor) =>
        new(() =>
        {
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = ConsoleColor.Cyan;
            var text = $"\u2550\u2550 {heading}";
            Console.Write(text);
            DialogHelper.RenderBoxRightBorder(text.Length, width, borderColor);
        });

    public static ContentLine Field(string label, string value, int width, ConsoleColor borderColor, ConsoleColor? valueColor = null) =>
        new(() =>
        {
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"{label,-12}");
            Console.ForegroundColor = valueColor ?? ConsoleColor.White;
            Console.Write(value);
            var contentLen = Math.Max(label.Length, 12) + value.Length;
            DialogHelper.RenderBoxRightBorder(contentLen, width, borderColor);
        });
}
