namespace KanbanCli.Tui;
using KanbanCli.Models;

public record NewTaskInputs(string Title, TaskType Type, Priority Priority, IReadOnlyList<string> Labels);

public class NewTaskDialog
{
    public NewTaskInputs? Show()
    {
        Console.Clear();
        Console.CursorVisible = true;

        var windowWidth = Math.Max(Console.WindowWidth, 40);
        TuiHelpers.RenderHeader("New Task", windowWidth, ConsoleColor.DarkGreen);
        Console.WriteLine();
        Console.WriteLine();

        var title = PromptText("  Title: ");
        if (string.IsNullOrWhiteSpace(title))
        {
            ShowError("Title cannot be empty. Press any key to cancel.");
            Console.ReadKey(intercept: true);
            return null;
        }

        var type = PromptEnum<TaskType>("  Type");
        if (type is null)
            return null;

        var priority = PromptEnum<Priority>("  Priority");
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

    private static T? PromptEnum<T>(string label) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {label}:");
        Console.ResetColor();

        for (var i = 0; i < values.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"    {i + 1}. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(values[i].ToString());
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  Enter number (1-{values.Length}): ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(input, out var choice) || choice < 1 || choice > values.Length)
        {
            ShowError("Invalid selection. Press any key to cancel.");
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

    private static void ShowError(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }
}
