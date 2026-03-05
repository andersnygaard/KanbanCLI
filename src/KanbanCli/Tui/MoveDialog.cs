namespace KanbanCli.Tui;
using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

public class MoveDialog
{
    public TaskStatus? Show(Board board, int currentColumnIndex)
    {
        Console.Clear();
        Console.CursorVisible = true;

        var width = DialogHelper.GetBoxWidth();
        var borderColor = Theme.DialogBorder;

        DialogHelper.RenderBoxTop("Move Task", width, borderColor);
        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = Theme.DialogPrompt;
        var headerText = "Move task to column:";
        Console.Write(headerText);
        DialogHelper.RenderBoxRightBorder(headerText.Length, width, borderColor);

        for (var i = 0; i < board.Columns.Count; i++)
        {
            var marker = i == currentColumnIndex ? " (current)" : string.Empty;
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = Theme.DialogListNumber;
            var numText = $"  {i + 1}. ";
            Console.Write(numText);
            Console.ForegroundColor = Theme.DialogListItem;
            var colText = $"{board.Columns[i].Name}{marker}";
            Console.Write(colText);
            DialogHelper.RenderBoxRightBorder(numText.Length + colText.Length, width, borderColor);
        }

        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = Theme.DialogPrompt;
        var promptText = $"Enter number (1-{board.Columns.Count}), or 0 to cancel: ";
        Console.Write(promptText);
        Console.ResetColor();

        Console.Out.Flush();
        var input = Console.ReadLine()?.Trim() ?? string.Empty;

        DialogHelper.RenderBoxEmptyLine(width, borderColor);
        DialogHelper.RenderBoxBottom(width, borderColor);

        if (!int.TryParse(input, out var choice) || choice == 0)
            return null;

        if (choice < 1 || choice > board.Columns.Count)
            return null;

        if (choice - 1 == currentColumnIndex)
            return null; // Already in this column

        return board.Columns[choice - 1].Status;
    }
}
