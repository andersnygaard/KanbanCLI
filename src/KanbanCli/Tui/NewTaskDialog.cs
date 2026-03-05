namespace KanbanCli.Tui;
using KanbanCli.Models;

public record NewTaskInputs(string Title, TaskType Type, Priority Priority, IReadOnlyList<string> Labels);

public class NewTaskDialog
{
    public NewTaskInputs? Show()
    {
        DialogHelper.SetupDialog("New Task", ConsoleColor.DarkGreen);
        Console.WriteLine();

        var title = PromptText("  Title: ");
        if (string.IsNullOrWhiteSpace(title))
        {
            DialogHelper.ShowError("Title cannot be empty. Press any key to cancel.");
            Console.ReadKey(intercept: true);
            return null;
        }

        var type = DialogHelper.PromptEnum<TaskType>("Type");
        if (type is null)
            return null;

        var priority = DialogHelper.PromptEnum<Priority>("Priority");
        if (priority is null)
            return null;

        var labelsInput = PromptText("  Labels (comma-separated, or leave empty): ");
        var labels = ParseLabels(labelsInput);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Creating task: [{type}] {title} ({priority})...");
        Console.ResetColor();

        return new NewTaskInputs(title.Trim(), type.Value, priority.Value, labels);
    }

    private static string PromptText(string prompt)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(prompt);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;
        var input = Console.ReadLine() ?? string.Empty;
        Console.ResetColor();
        return input;
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
