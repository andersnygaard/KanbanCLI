namespace KanbanCli.Tui;
using KanbanCli.Models;

public class PriorityDialog
{
    public Priority Show(Priority current)
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  Change priority (current: {current}):");
        Console.ResetColor();

        var priorities = Enum.GetValues<Priority>();
        for (var i = 0; i < priorities.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"    {i + 1}. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(priorities[i].ToString());
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  Enter number (1-{priorities.Length}), or 0 to cancel: ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(input, out var choice) || choice == 0 || choice < 1 || choice > priorities.Length)
            return current;

        Console.CursorVisible = false;
        return priorities[choice - 1];
    }
}
