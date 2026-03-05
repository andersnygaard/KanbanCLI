namespace KanbanCli.Tui;
using KanbanCli.Models;

public class ConfirmDialog
{
    public bool Confirm(TaskItem task)
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Delete task #{task.Id:D3}: {task.Title}?");
        Console.ResetColor();
        Console.Write("  Type 'yes' to confirm: ");
        var input = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.CursorVisible = false;
        return string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
