namespace KanbanCli.Tui;

public class StatusBar
{
    private const string KeyBindings =
        " \u2190\u2192 Column | \u2191\u2193 Task | Enter View | n New | m Move | d Delete | p Priority | f Filter | q Quit";

    public void Render(int row, int width, string? filterInfo = null)
    {
        Console.SetCursorPosition(0, row);
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.White;

        var statusText = KeyBindings;
        if (filterInfo is not null)
            statusText += $"  | Filter: {filterInfo}";

        var maxLen = width - 1;
        string padded;
        if (statusText.Length > maxLen)
        {
            padded = maxLen > 3
                ? statusText[..(maxLen - 3)] + "..."
                : statusText[..maxLen];
        }
        else
        {
            padded = statusText.PadRight(maxLen);
        }

        Console.Write(padded);
        Console.ResetColor();
    }
}
