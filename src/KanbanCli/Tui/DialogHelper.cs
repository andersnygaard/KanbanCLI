namespace KanbanCli.Tui;

public static class DialogHelper
{
    /// <summary>
    /// Clears the console, makes cursor visible, and optionally renders a colored header.
    /// </summary>
    public static void SetupDialog(string? header = null, ConsoleColor headerColor = ConsoleColor.DarkGreen)
    {
        Console.Clear();
        Console.CursorVisible = true;

        if (header is not null)
        {
            var windowWidth = Math.Max(Console.WindowWidth, 40);
            TuiHelpers.RenderHeader(header, windowWidth, headerColor);
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Displays an enum's values as a numbered list and prompts the user to select one.
    /// Returns null if the user cancels or enters an invalid selection.
    /// </summary>
    public static T? PromptEnum<T>(string label, bool allowZeroCancel = false) where T : struct, Enum
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

        var cancelHint = allowZeroCancel ? ", or 0 to cancel" : "";
        Console.Write($"  Enter number (1-{values.Length}{cancelHint}): ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(input, out var choice) || choice < 1 || choice > values.Length)
        {
            if (!allowZeroCancel)
            {
                ShowError("Invalid selection. Press any key to cancel.");
                Console.ReadKey(intercept: true);
            }

            return null;
        }

        return values[choice - 1];
    }

    /// <summary>
    /// Displays an error message in red.
    /// </summary>
    public static void ShowError(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }
}
