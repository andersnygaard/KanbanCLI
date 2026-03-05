namespace KanbanCli.Tui;
using KanbanCli.Models;

public class PriorityDialog
{
    public Priority Show(Priority current)
    {
        DialogHelper.SetupDialog();

        var selected = DialogHelper.PromptEnum<Priority>($"Change priority (current: {current})", allowZeroCancel: true);

        Console.CursorVisible = false;

        return selected ?? current;
    }
}
