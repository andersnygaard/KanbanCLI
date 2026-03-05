namespace KanbanCli.Tui;
using KanbanCli.Models;

public class ConfirmDialog
{
    public bool Confirm(TaskItem task)
    {
        Console.Clear();
        Console.CursorVisible = true;

        var width = DialogHelper.GetBoxWidth();
        var borderColor = Theme.DangerBorder;

        DialogHelper.RenderBoxTop("Confirm Delete", width, borderColor);
        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = Theme.DangerText;
        var warningText = $"Delete task #{task.Id:D3}: {task.Title}?";
        Console.Write(warningText);
        DialogHelper.RenderBoxRightBorder(warningText.Length, width, borderColor);

        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ResetColor();
        Console.Write("Type 'yes' to confirm: ");

        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        DialogHelper.RenderBoxEmptyLine(width, borderColor);
        DialogHelper.RenderBoxBottom(width, borderColor);

        Console.CursorVisible = false;
        return string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
