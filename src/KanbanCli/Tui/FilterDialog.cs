namespace KanbanCli.Tui;
using KanbanCli.Models;

public class FilterDialog
{
    private enum FilterMode { ByType = 1, ByLabel = 2, ByPriority = 3 }

    public FilterCriteria? Show(IReadOnlyList<string> availableLabels)
    {
        DialogHelper.SetupDialog("Filter Board", ConsoleColor.DarkMagenta);
        Console.WriteLine();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Filter by:");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("    1. ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("By Type");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("    2. ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("By Label");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("    3. ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("By Priority");

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Enter number (1-3), or 0 to cancel: ");
        Console.ResetColor();

        var modeInput = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(modeInput, out var modeChoice) || modeChoice == 0)
        {
            Console.CursorVisible = false;
            return null;
        }

        return modeChoice switch
        {
            (int)FilterMode.ByType => PromptByType(),
            (int)FilterMode.ByLabel => PromptByLabel(availableLabels),
            (int)FilterMode.ByPriority => PromptByPriority(),
            _ => null
        };
    }

    private static FilterCriteria? PromptByType()
    {
        var selected = DialogHelper.PromptEnum<TaskType>("Task type", allowZeroCancel: true);

        Console.CursorVisible = false;

        return selected.HasValue ? new FilterCriteria(Type: selected.Value) : null;
    }

    private static FilterCriteria? PromptByLabel(IReadOnlyList<string> availableLabels)
    {
        Console.WriteLine();

        if (availableLabels.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Available labels:");
            Console.ResetColor();

            for (var i = 0; i < availableLabels.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"    {i + 1}. ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(availableLabels[i]);
            }

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  Enter number (1-{availableLabels.Count}), label name, or 0 to cancel: ");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  Enter label name, or leave empty to cancel: ");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.White;
        var input = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.ResetColor();
        Console.CursorVisible = false;

        if (string.IsNullOrWhiteSpace(input) || input == "0")
            return null;

        // Check if the user entered a number that refers to a label from the list
        if (availableLabels.Count > 0
            && int.TryParse(input, out var labelChoice)
            && labelChoice >= 1
            && labelChoice <= availableLabels.Count)
        {
            return new FilterCriteria(Label: availableLabels[labelChoice - 1]);
        }

        return new FilterCriteria(Label: input);
    }

    private static FilterCriteria? PromptByPriority()
    {
        var selected = DialogHelper.PromptEnum<Priority>("Priority", allowZeroCancel: true);

        Console.CursorVisible = false;

        return selected.HasValue ? new FilterCriteria(Priority: selected.Value) : null;
    }

}
