namespace KanbanCli.Tui;
using KanbanCli.Models;

public class ColumnView
{
    private readonly TaskCard _taskCard = new();

    public void Render(Column column, int columnIndex, int columnX, int columnWidth, NavigationState state, int startRow, int maxRows, bool isFiltered = false)
    {
        var isSelectedColumn = state.SelectedColumn == columnIndex;

        RenderHeader(column, columnX, columnWidth, startRow, isSelectedColumn);

        var taskStartRow = startRow + 2;

        if (column.IsEmpty)
        {
            RenderEmptyPlaceholder(columnX, columnWidth, taskStartRow, isFiltered);
            return;
        }

        for (var i = 0; i < column.Tasks.Count; i++)
        {
            var taskRow = taskStartRow + i;
            if (taskRow >= startRow + maxRows)
                break;

            var task = column.Tasks[i];
            var isSelectedTask = isSelectedColumn && state.SelectedTask == i;
            _taskCard.RenderWithColors(task, columnX, taskRow, columnWidth, isSelectedTask);
        }
    }

    private static void RenderHeader(Column column, int columnX, int columnWidth, int startRow, bool isSelected)
    {
        Console.SetCursorPosition(columnX, startRow);

        if (isSelected)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        var headerText = $" {column.Name} ({column.Tasks.Count})";
        var paddedHeader = headerText.PadRight(columnWidth - 1);
        Console.Write(paddedHeader);
        Console.ResetColor();

        // Separator line
        Console.SetCursorPosition(columnX, startRow + 1);
        Console.ForegroundColor = isSelected ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
        Console.Write(new string('-', columnWidth - 1));
        Console.ResetColor();
    }

    private static void RenderEmptyPlaceholder(int columnX, int columnWidth, int row, bool isFiltered)
    {
        Console.SetCursorPosition(columnX, row);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        var placeholder = isFiltered ? " (no matching tasks)" : " (no tasks)";
        Console.Write(placeholder.PadRight(columnWidth - 1));
        Console.ResetColor();
    }
}
