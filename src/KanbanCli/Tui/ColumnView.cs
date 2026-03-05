namespace KanbanCli.Tui;
using KanbanCli.Models;

public class ColumnView : IColumnView
{
    private readonly ITaskCard _taskCard;

    public ColumnView(ITaskCard taskCard)
    {
        _taskCard = taskCard;
    }

    public void Render(Column column, int columnIndex, int columnX, int columnWidth, NavigationState state, int startRow, int maxRows, bool isFiltered = false)
    {
        var isSelectedColumn = state.SelectedColumn == columnIndex;

        if (column.IsEmpty)
        {
            RenderEmptyPlaceholder(columnX, columnWidth, startRow, isFiltered);
            return;
        }

        // Each card takes CardLineCount lines + CardSeparatorLines blank line
        // Last card doesn't need a separator, but we keep the math simple
        var visibleCardCount = Math.Max(1, (maxRows + TuiHelpers.CardSeparatorLines) / TuiHelpers.CardTotalHeight);

        for (var i = 0; i < column.Tasks.Count && i < visibleCardCount; i++)
        {
            var cardStartRow = startRow + (i * TuiHelpers.CardTotalHeight);

            // Don't render if the card would overflow the available space
            if (cardStartRow + TuiHelpers.CardLineCount > startRow + maxRows)
                break;

            var task = column.Tasks[i];
            var isSelectedTask = isSelectedColumn && state.SelectedTask == i;
            _taskCard.RenderWithColors(task, columnX, cardStartRow, columnWidth, isSelectedTask);

            // Render blank separator line between cards (not after the last visible card)
            var separatorRow = cardStartRow + TuiHelpers.CardLineCount;
            if (separatorRow < startRow + maxRows)
            {
                RenderBlankLine(columnX, columnWidth, separatorRow);
            }
        }
    }

    private static void RenderBlankLine(int columnX, int columnWidth, int row)
    {
        Console.SetCursorPosition(columnX, row);
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(new string(' ', columnWidth));
        Console.ResetColor();
    }

    private static void RenderEmptyPlaceholder(int columnX, int columnWidth, int row, bool isFiltered)
    {
        Console.SetCursorPosition(columnX, row);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        var placeholder = isFiltered ? " (no matching tasks)" : " (no tasks)";
        var padded = placeholder.Length > columnWidth
            ? placeholder[..columnWidth]
            : placeholder.PadRight(columnWidth);
        Console.Write(padded);
        Console.ResetColor();
    }
}
