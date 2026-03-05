namespace KanbanCli.Tui;
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

public class MoveDialog
{
    private static readonly TaskStatus[] StatusMap =
    [
        TaskStatus.Backlog,
        TaskStatus.InProgress,
        TaskStatus.Done,
        TaskStatus.OnHold
    ];

    public TaskStatus? Show(Board board, int currentColumnIndex)
    {
        Console.Clear();
        Console.CursorVisible = true;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Move task to column:");
        Console.ResetColor();

        for (var i = 0; i < board.Columns.Count; i++)
        {
            var marker = i == currentColumnIndex ? " (current)" : string.Empty;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"    {i + 1}. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{board.Columns[i].Name}{marker}");
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  Enter number (1-{board.Columns.Count}), or 0 to cancel: ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(input, out var choice) || choice == 0)
            return null;

        if (choice < 1 || choice > StatusMap.Length)
            return null;

        return StatusMap[choice - 1];
    }
}
