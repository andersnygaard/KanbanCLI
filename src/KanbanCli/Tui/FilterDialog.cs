namespace KanbanCli.Tui;
using KanbanCli.Models;

public class FilterDialog
{
    private enum FilterMode { ByType = 1, ByLabel = 2 }

    public FilterCriteria? Show(IReadOnlyList<string> availableLabels)
    {
        Console.Clear();
        Console.CursorVisible = true;

        var windowWidth = Math.Max(Console.WindowWidth, 40);
        TuiHelpers.RenderHeader("Filter Board", windowWidth, ConsoleColor.DarkMagenta);
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

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Enter number (1-2), or 0 to cancel: ");
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
            _ => null
        };
    }

    private static FilterCriteria? PromptByType()
    {
        var types = Enum.GetValues<TaskType>();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Task type:");
        Console.ResetColor();

        for (var i = 0; i < types.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"    {i + 1}. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(types[i].ToString());
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  Enter number (1-{types.Length}), or 0 to cancel: ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.CursorVisible = false;

        if (!int.TryParse(input, out var choice) || choice == 0 || choice < 1 || choice > types.Length)
            return null;

        return new FilterCriteria(Type: types[choice - 1]);
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

}
