namespace KanbanCli.Tui;
using KanbanCli.Models;

public record NewTaskInputs(string Title, TaskType Type, Priority Priority, IReadOnlyList<string> Labels);

public class NewTaskDialog
{
    public NewTaskInputs? Show()
    {
        Console.Clear();
        Console.CursorVisible = true;

        var width = DialogHelper.GetBoxWidth();
        var borderColor = Theme.DialogBorder;

        DialogHelper.RenderBoxTop("New Task", width, borderColor);
        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        var title = DialogHelper.PromptTextInBox("Title: ", width, borderColor);
        if (string.IsNullOrWhiteSpace(title))
        {
            DialogHelper.RenderBoxEmptyLine(width, borderColor);
            DialogHelper.RenderBoxBottom(width, borderColor);
            DialogHelper.ShowError("Title cannot be empty. Press any key to cancel.");

            Console.ReadKey(intercept: true);
            return null;
        }

        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        var type = DialogHelper.PromptEnumInBox<TaskType>("Type", width, borderColor);
        if (type is null)
        {
            DialogHelper.RenderBoxBottom(width, borderColor);
            return null;
        }

        var priority = DialogHelper.PromptEnumInBox<Priority>("Priority", width, borderColor);
        if (priority is null)
        {
            DialogHelper.RenderBoxBottom(width, borderColor);
            return null;
        }

        DialogHelper.RenderBoxEmptyLine(width, borderColor);
        var labelsInput = DialogHelper.PromptTextInBox("Labels (comma-separated, or empty): ", width, borderColor);
        var labels = ParseLabels(labelsInput);

        DialogHelper.RenderBoxEmptyLine(width, borderColor);
        DialogHelper.RenderBoxBottom(width, borderColor);

        Console.ForegroundColor = Theme.Success;
        Console.WriteLine($"  \u2713 Creating task: [{type}] {title} ({priority})...");
        Console.ResetColor();

        return new NewTaskInputs(title.Trim(), type.Value, priority.Value, labels);
    }

    private static IReadOnlyList<string> ParseLabels(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        return input
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList()
            .AsReadOnly();
    }
}
