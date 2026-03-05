namespace KanbanCli.Tui;

public class StatusBar
{
    private const char Vertical = '\u2502'; // │

    private const string KeyBindings =
        " \u2190\u2192 Column | \u2191\u2193 Task | Enter View | n New | m Move | d Delete | p Priority | f Filter | q Quit";

    public void Render(int row, int width, string? filterInfo = null)
    {
        Console.SetCursorPosition(0, row);

        // Left border
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(Vertical);

        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.White;

        var statusText = KeyBindings;
        if (filterInfo is not null)
            statusText += $"  | Filter: {filterInfo}";

        // Content area is width - 2 (for left and right border chars)
        var contentWidth = width - 2;
        string padded;
        if (statusText.Length > contentWidth)
        {
            padded = contentWidth > 3
                ? statusText[..(contentWidth - 3)] + "..."
                : statusText[..contentWidth];
        }
        else
        {
            padded = statusText.PadRight(contentWidth);
        }

        Console.Write(padded);
        Console.ResetColor();

        // Right border
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(Vertical);
        Console.ResetColor();
    }
}
