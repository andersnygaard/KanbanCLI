namespace KanbanCli.Tui;
using KanbanCli.Models;
using KanbanCli.Services;
using TaskStatus = KanbanCli.Models.TaskStatus;

public class TaskDetailPanel
{
    private const int PageScrollSize = 10;
    private const int DetailHeightReserve = 4;

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
            var (updatedTask, updatedScroll, shouldExit) = HandleKeyPress(current, scrollOffset, key);
            current = updatedTask;
            scrollOffset = updatedScroll;

            if (shouldExit)
            {
                return current;
            }
        }
    }

    private (TaskItem Task, int ScrollOffset, bool ShouldExit) HandleKeyPress(
        TaskItem current, int scrollOffset, ConsoleKeyInfo key)
    {
        return key.Key switch
        {
            ConsoleKey.T => (HandleEditTitle(current), 0, false),
            ConsoleKey.L => (HandleEditLabels(current), 0, false),
            ConsoleKey.P => (HandleEditPriority(current), 0, false),
            ConsoleKey.UpArrow => (current, Math.Max(0, scrollOffset - 1), false),
            ConsoleKey.DownArrow => (current, scrollOffset + 1, false),
            ConsoleKey.PageUp => (current, Math.Max(0, scrollOffset - PageScrollSize), false),
            ConsoleKey.PageDown => (current, scrollOffset + PageScrollSize, false),
            ConsoleKey.Home => (current, 0, false),
            ConsoleKey.Escape => (current, scrollOffset, true),
            _ => (current, scrollOffset, false)
        };
    }

    private void RenderDetailView(TaskItem task, int scrollOffset)
    {
        Console.Clear();
        Console.CursorVisible = false;

        var width = DialogHelper.GetBoxWidth();
        var borderColor = Theme.DetailBorder;
        var visibleHeight = Math.Max(TuiHelpers.GetEffectiveHeight() - DetailHeightReserve, BoardConstants.MinWindowHeight); // Reserve space for top border, edit hints, bottom border

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
        lines.Add(ContentLine.Field("Type", task.Type.ToString(), width, borderColor, TuiHelpers.GetTypeColor(task.Type)));

        var prioritySymbol = TuiHelpers.GetPrioritySymbol(task.Priority);
        lines.Add(ContentLine.Field("Priority", $"{prioritySymbol} {task.Priority}", width, borderColor, TuiHelpers.GetPriorityColor(task.Priority)));

        lines.Add(ContentLine.StatusWorkflow(task.Status, width, borderColor));

        lines.Add(ContentLine.Labels(task.Labels, width, borderColor));
        lines.Add(ContentLine.Field("Created", task.CreatedDate.ToString("yyyy-MM-dd HH:mm"), width, borderColor));
        if (task.CompletedDate.HasValue)
            lines.Add(ContentLine.Field("Completed", task.CompletedDate.Value.ToString("yyyy-MM-dd HH:mm"), width, borderColor));

        // Extra metadata fields
        foreach (var meta in task.ExtraMetadata)
            lines.Add(ContentLine.Field(meta.Key, meta.Value, width, borderColor));

        lines.Add(ContentLine.Empty(width, borderColor));
        lines.Add(ContentLine.Separator(width, borderColor));

        // Sections
        foreach (var section in task.Sections)
        {
            lines.Add(ContentLine.Heading(section.Key, width, borderColor));
            lines.Add(ContentLine.Empty(width, borderColor));

            if (!string.IsNullOrWhiteSpace(section.Value))
            {
                lines.Add(ContentLine.Markdown(section.Value, width, borderColor));
            }

            lines.Add(ContentLine.Empty(width, borderColor));
        }

        return lines;
    }

    private static void RenderScrollStatus(int scrollOffset, int maxScroll, int width, ConsoleColor borderColor)
    {
        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = Theme.DetailMuted;
        var scrollText = $"\u2191\u2193 Scroll  PgUp/PgDn  Home  ({scrollOffset + 1}/{maxScroll + 1})";
        Console.Write(scrollText);
        DialogHelper.RenderBoxRightBorder(scrollText.Length, width, borderColor);
    }

    private static void RenderEditHints(int width, ConsoleColor borderColor)
    {
        DialogHelper.RenderBoxLeftBorder(borderColor);

        Console.ForegroundColor = Theme.HintKey;
        Console.Write("[T]");
        Console.ForegroundColor = Theme.HintText;
        Console.Write("itle  ");
        Console.ForegroundColor = Theme.HintKey;
        Console.Write("[L]");
        Console.ForegroundColor = Theme.HintText;
        Console.Write("abels  ");
        Console.ForegroundColor = Theme.HintKey;
        Console.Write("[P]");
        Console.ForegroundColor = Theme.HintText;
        Console.Write("riority  ");
        Console.ForegroundColor = Theme.HintKey;
        Console.Write("[Esc]");
        Console.ForegroundColor = Theme.HintText;
        Console.Write(" back");

        // "[T]itle  [L]abels  [P]riority  [Esc] back" = 42 chars
        DialogHelper.RenderBoxRightBorder(42, width, borderColor);
    }

    private TaskItem HandleEditTitle(TaskItem task)
    {
        Console.WriteLine();
        Console.ForegroundColor = Theme.DialogPrompt;
        Console.Write("  New title (empty to cancel): ");
        Console.ResetColor();
        Console.ForegroundColor = Theme.DialogText;

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
        Console.ForegroundColor = Theme.DialogPrompt;
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
        Console.ForegroundColor = Theme.DialogPrompt;
        Console.Write("  Label to add: ");
        Console.ResetColor();
        Console.ForegroundColor = Theme.DialogText;

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
            Console.ForegroundColor = Theme.DetailMuted;
            Console.WriteLine("  No labels to remove. Press any key...");
            Console.ResetColor();

            Console.ReadKey(intercept: true);
            return task;
        }

        Console.ForegroundColor = Theme.DialogPrompt;
        Console.WriteLine("  Select label to remove:");
        Console.ResetColor();

        var width = DialogHelper.GetBoxWidth();
        DialogHelper.RenderNumberedListInBox(task.Labels, width, Theme.DialogBorder);

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
        Console.ForegroundColor = Theme.DialogPrompt;
        Console.WriteLine($"  Change priority (current: {task.Priority}):");
        Console.ResetColor();

        var priorities = Enum.GetValues<Priority>();
        var priorityNames = priorities.Select(p => p.ToString()).ToList();
        var width = DialogHelper.GetBoxWidth();
        DialogHelper.RenderNumberedListInBox(priorityNames, width, Theme.DialogBorder);

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

    public void Render()
    {
        _renderAction();
    }

    public static ContentLine Empty(int width, ConsoleColor borderColor)
    {
        return new ContentLine(() => DialogHelper.RenderBoxEmptyLine(width, borderColor));
    }

    public static ContentLine Separator(int width, ConsoleColor borderColor)
    {
        return new ContentLine(() => DialogHelper.RenderBoxSeparator(width, borderColor));
    }

    public static ContentLine Text(string text, int width, ConsoleColor borderColor)
    {
        return new ContentLine(() => DialogHelper.RenderBoxLine(text, width, borderColor));
    }

    public static ContentLine Markdown(string markdownContent, int width, ConsoleColor borderColor)
    {
        return new ContentLine(() =>
        {
            MarkdownRenderer.RenderMarkdownContent(markdownContent, width, borderColor);
        });
    }

    public static ContentLine Heading(string heading, int width, ConsoleColor borderColor)
    {
        return new ContentLine(() =>
        {
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = Theme.DetailHeading;
            var text = $"\u2550\u2550 {heading}";
            Console.Write(text);
            DialogHelper.RenderBoxRightBorder(text.Length, width, borderColor);
        });
    }

    public static ContentLine Field(string label, string value, int width, ConsoleColor borderColor, ConsoleColor? valueColor = null)
    {
        return new ContentLine(() =>
        {
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = Theme.DetailFieldLabel;
            Console.Write($"{label,-12}");
            Console.ForegroundColor = valueColor ?? Theme.DetailFieldValue;
            var maxValueLen = width - 15; // 2 border + 12 label + 1 padding
            var truncatedValue = value.Length > maxValueLen
                ? value[..Math.Max(0, maxValueLen - 1)] + "\u2026"
                : value;
            Console.Write(truncatedValue);
            var contentLen = Math.Max(label.Length, 12) + truncatedValue.Length;
            DialogHelper.RenderBoxRightBorder(contentLen, width, borderColor);
        });
    }

    public static ContentLine StatusWorkflow(TaskStatus status, int width, ConsoleColor borderColor)
    {
        return new ContentLine(() =>
        {
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = Theme.DetailFieldLabel;
            Console.Write($"{"Status",-12}");

            var statuses = new[]
            {
                (TaskStatus.Backlog, "Backlog"),
                (TaskStatus.InProgress, "In Progress"),
                (TaskStatus.Done, "Done"),
                (TaskStatus.OnHold, "On Hold")
            };

            var written = 12;
            for (var i = 0; i < statuses.Length; i++)
            {
                var (s, name) = statuses[i];
                if (s == status)
                {
                    Console.ForegroundColor = Theme.WorkflowActiveFg;
                    Console.BackgroundColor = Theme.WorkflowActiveBg;
                    Console.Write($" {name} ");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = Theme.WorkflowInactive;
                    Console.Write($" {name} ");
                }
                written += name.Length + 2;

                if (i < statuses.Length - 1)
                {
                    Console.ForegroundColor = Theme.WorkflowInactive;
                    Console.Write("\u2192");
                    written += 1;
                }
            }

            DialogHelper.RenderBoxRightBorder(written, width, borderColor);
        });
    }

    public static ContentLine Labels(IReadOnlyList<string> labels, int width, ConsoleColor borderColor)
    {
        return new ContentLine(() =>
        {
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = Theme.DetailFieldLabel;
            Console.Write($"{"Labels",-12}");

            var written = 12;

            if (labels.Count == 0)
            {
                Console.ForegroundColor = Theme.DetailMuted;
                Console.Write("(none)");
                written += 6;
            }
            else
            {
                for (var i = 0; i < labels.Count; i++)
                {
                    if (i > 0)
                    {
                        Console.Write("  ");
                        written += 2;
                    }

                    Console.ForegroundColor = Theme.LabelBracket;
                    Console.Write("[");
                    Console.ForegroundColor = Theme.LabelText;
                    Console.Write(labels[i]);
                    Console.ForegroundColor = Theme.LabelBracket;
                    Console.Write("]");
                    written += labels[i].Length + 2;
                }
            }

            DialogHelper.RenderBoxRightBorder(written, width, borderColor);
        });
    }
}
