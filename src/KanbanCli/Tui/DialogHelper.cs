namespace KanbanCli.Tui;

public static class DialogHelper
{
    /// <summary>
    /// Clears the console, makes cursor visible, and optionally renders a colored header.
    /// </summary>
    public static void SetupDialog(string? header = null, ConsoleColor? headerColor = null)
    {
        Console.Clear();
        Console.CursorVisible = true;

        if (header is not null)
        {
            var windowWidth = TuiHelpers.GetEffectiveWidth();
            TuiHelpers.RenderHeader(header, windowWidth, headerColor ?? Theme.DialogHeader);
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
        Console.ForegroundColor = Theme.DialogPrompt;
        Console.WriteLine($"  {label}:");
        Console.ResetColor();

        for (var i = 0; i < values.Length; i++)
        {
            Console.ForegroundColor = Theme.DialogListNumber;
            Console.Write($"    {i + 1}. ");
            Console.ForegroundColor = Theme.DialogListItem;
            Console.WriteLine(values[i].ToString());
        }

        Console.ResetColor();
        Console.ForegroundColor = Theme.DialogPrompt;

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
    /// Displays an error message in red with ✗ symbol.
    /// </summary>
    public static void ShowError(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = Theme.Error;
        Console.WriteLine($"  \u2717 {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Displays a success message in green with ✓ symbol.
    /// </summary>
    public static void ShowSuccess(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = Theme.Success;
        Console.WriteLine($"  \u2713 {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Renders the top border of a box with an embedded title.
    /// Example: ┌─── Title ────────────┐
    /// </summary>
    public static void RenderBoxTop(string title, int width, ConsoleColor? borderColor = null)
    {
        var border = borderColor ?? Theme.DialogBorder;
        Console.ForegroundColor = border;
        Console.Write("\u250C\u2500\u2500\u2500 ");
        Console.ForegroundColor = Theme.DialogTitle;
        Console.Write(title);
        Console.ForegroundColor = border;
        Console.Write(" ");
        var remaining = width - title.Length - 7;
        if (remaining > 0)
            Console.Write(new string('\u2500', remaining));
        Console.WriteLine("\u2510");
        Console.ResetColor();
    }

    /// <summary>
    /// Renders the bottom border of a box.
    /// Example: └───────────────────────┘
    /// </summary>
    public static void RenderBoxBottom(int width, ConsoleColor? borderColor = null)
    {
        Console.ForegroundColor = borderColor ?? Theme.DialogBorder;
        Console.Write("\u2514");
        Console.Write(new string('\u2500', width - 2));
        Console.WriteLine("\u2518");
        Console.ResetColor();
    }

    /// <summary>
    /// Renders a content line inside a box with left and right borders.
    /// Example: │  content here              │
    /// </summary>
    public static void RenderBoxLine(string content, int width, ConsoleColor? borderColor = null)
    {
        var border = borderColor ?? Theme.DialogBorder;
        Console.ForegroundColor = border;
        Console.Write("\u2502 ");
        Console.ResetColor();
        var padded = content.Length > width - 4
            ? content[..(width - 4)]
            : content.PadRight(width - 4);
        Console.Write(padded);
        Console.ForegroundColor = border;
        Console.WriteLine(" \u2502");
        Console.ResetColor();
    }

    /// <summary>
    /// Renders an empty line inside a box.
    /// Example: │                             │
    /// </summary>
    public static void RenderBoxEmptyLine(int width, ConsoleColor? borderColor = null)
    {
        Console.ForegroundColor = borderColor ?? Theme.DialogBorder;
        Console.Write("\u2502");
        Console.Write(new string(' ', width - 2));
        Console.WriteLine("\u2502");
        Console.ResetColor();
    }

    /// <summary>
    /// Renders a horizontal separator inside a box.
    /// Example: ├───────────────────────┤
    /// </summary>
    public static void RenderBoxSeparator(int width, ConsoleColor? borderColor = null)
    {
        Console.ForegroundColor = borderColor ?? Theme.DialogBorder;
        Console.Write("\u251C");
        Console.Write(new string('\u2500', width - 2));
        Console.WriteLine("\u2524");
        Console.ResetColor();
    }

    /// <summary>
    /// Renders a line inside a box where the content has already been written with custom colors.
    /// Only renders the left border prefix; caller must handle content and right border.
    /// </summary>
    public static void RenderBoxLeftBorder(ConsoleColor? borderColor = null)
    {
        Console.ForegroundColor = borderColor ?? Theme.DialogBorder;
        Console.Write("\u2502 ");
        Console.ResetColor();
    }

    /// <summary>
    /// Renders the right border to close a box line. Pads remaining space.
    /// </summary>
    public static void RenderBoxRightBorder(int currentLength, int width, ConsoleColor? borderColor = null)
    {
        var remaining = width - currentLength - 3;
        if (remaining > 0)
            Console.Write(new string(' ', remaining));
        Console.ForegroundColor = borderColor ?? Theme.DialogBorder;
        Console.WriteLine(" \u2502");
        Console.ResetColor();
    }

    /// <summary>
    /// Prompts for text input inside a box border. Renders the left border, prompt text in cyan,
    /// then reads user input in white.
    /// </summary>
    public static string PromptTextInBox(string prompt, int width, ConsoleColor borderColor)
    {
        RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = Theme.DialogPrompt;
        Console.Write(prompt);
        Console.ResetColor();
        Console.ForegroundColor = Theme.DialogText;
        var input = Console.ReadLine() ?? string.Empty;
        Console.ResetColor();
        return input;
    }

    /// <summary>
    /// Renders a numbered list of items inside a box with left and right borders.
    /// </summary>
    public static void RenderNumberedListInBox(IReadOnlyList<string> items, int width, ConsoleColor borderColor)
    {
        for (var i = 0; i < items.Count; i++)
        {
            RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = Theme.DialogListNumber;
            var numText = $"  {i + 1}. ";
            Console.Write(numText);
            Console.ForegroundColor = Theme.DialogListItem;
            var valText = items[i];
            Console.Write(valText);
            RenderBoxRightBorder(numText.Length + valText.Length, width, borderColor);
        }
    }

    /// <summary>
    /// Prompts the user to enter a numeric choice. Returns the chosen number (1-based),
    /// or null if the input is invalid or cancelled.
    /// </summary>
    public static int? PromptNumericChoice(int maxValue, bool allowZeroCancel = false)
    {
        Console.ForegroundColor = Theme.DialogPrompt;
        var cancelHint = allowZeroCancel ? ", or 0 to cancel" : "";
        Console.Write($"  Enter number (1-{maxValue}{cancelHint}): ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;
        if (!int.TryParse(input, out var choice) || choice < 1 || choice > maxValue)
            return null;

        return choice;
    }

    /// <summary>
    /// Returns the default box width based on Console.WindowWidth.
    /// </summary>
    public static int GetBoxWidth()
    {
        return Math.Min(TuiHelpers.GetEffectiveWidth(), 80);
    }
}
