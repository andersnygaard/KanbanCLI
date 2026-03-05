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
        var borderColor = ConsoleColor.DarkGray;

        DialogHelper.RenderBoxTop("New Task", width, borderColor);
        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        var title = PromptText("Title: ", width, borderColor);
        if (string.IsNullOrWhiteSpace(title))
        {
            DialogHelper.RenderBoxEmptyLine(width, borderColor);
            DialogHelper.RenderBoxBottom(width, borderColor);
            DialogHelper.ShowError("Title cannot be empty. Press any key to cancel.");
            Console.ReadKey(intercept: true);
            return null;
        }

        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        var type = PromptEnumInBox<TaskType>("Type", width, borderColor);
        if (type is null)
        {
            DialogHelper.RenderBoxBottom(width, borderColor);
            return null;
        }

        var priority = PromptEnumInBox<Priority>("Priority", width, borderColor);
        if (priority is null)
        {
            DialogHelper.RenderBoxBottom(width, borderColor);
            return null;
        }

        DialogHelper.RenderBoxEmptyLine(width, borderColor);
        var labelsInput = PromptText("Labels (comma-separated, or empty): ", width, borderColor);
        var labels = ParseLabels(labelsInput);

        DialogHelper.RenderBoxEmptyLine(width, borderColor);
        DialogHelper.RenderBoxBottom(width, borderColor);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  \u2713 Creating task: [{type}] {title} ({priority})...");
        Console.ResetColor();

        return new NewTaskInputs(title.Trim(), type.Value, priority.Value, labels);
    }

    private static string PromptText(string prompt, int width, ConsoleColor borderColor)
    {
        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(prompt);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;
        var input = Console.ReadLine() ?? string.Empty;
        Console.ResetColor();
        return input;
    }

    private static T? PromptEnumInBox<T>(string label, int width, ConsoleColor borderColor) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();

        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = ConsoleColor.Cyan;
        var labelText = $"{label}:";
        Console.Write(labelText);
        DialogHelper.RenderBoxRightBorder(labelText.Length, width, borderColor);

        for (var i = 0; i < values.Length; i++)
        {
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            var numText = $"  {i + 1}. ";
            Console.Write(numText);
            Console.ForegroundColor = ConsoleColor.White;
            var valText = values[i].ToString();
            Console.Write(valText);
            DialogHelper.RenderBoxRightBorder(numText.Length + valText.Length, width, borderColor);
        }

        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = ConsoleColor.Cyan;
        var promptText = $"Enter number (1-{values.Length}): ";
        Console.Write(promptText);
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(input, out var choice) || choice < 1 || choice > values.Length)
        {
            DialogHelper.ShowError("Invalid selection. Press any key to cancel.");
            Console.ReadKey(intercept: true);
            return null;
        }

        return values[choice - 1];
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
