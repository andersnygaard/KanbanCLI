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
            ClearRemainingRows(columnX, columnWidth, startRow + 1, startRow + maxRows);
            return;
        }

        var visibleCardCount = Math.Max(1, (maxRows + TuiHelpers.CardSeparatorLines) / TuiHelpers.CardTotalHeight);
        var scrollOffset = CalculateScrollOffset(isSelectedColumn, state.SelectedTask, visibleCardCount);
        var endIndex = Math.Min(column.Tasks.Count, scrollOffset + visibleCardCount);

        var lastRenderedRow = RenderVisibleCards(column, columnX, columnWidth, state, isSelectedColumn, startRow, maxRows, scrollOffset, endIndex);
        ClearRemainingRows(columnX, columnWidth, lastRenderedRow, startRow + maxRows);
        RenderScrollIndicators(column, columnX, columnWidth, startRow, maxRows, scrollOffset, endIndex);
    }

    private static int CalculateScrollOffset(bool isSelectedColumn, int selectedTask, int visibleCardCount)
    {
        if (isSelectedColumn && selectedTask >= visibleCardCount)
        {
            return selectedTask - visibleCardCount + 1;
        }

        return 0;
    }

    private int RenderVisibleCards(Column column, int columnX, int columnWidth, NavigationState state, bool isSelectedColumn, int startRow, int maxRows, int scrollOffset, int endIndex)
    {
        var lastRenderedRow = startRow;

        for (var i = scrollOffset; i < endIndex; i++)
        {
            var displayIndex = i - scrollOffset;
            var cardStartRow = startRow + (displayIndex * TuiHelpers.CardTotalHeight);

            if (cardStartRow + TuiHelpers.CardLineCount > startRow + maxRows)
            {
                break;
            }

            var task = column.Tasks[i];
            var isSelectedTask = isSelectedColumn && state.SelectedTask == i;
            _taskCard.RenderWithColors(task, columnX, cardStartRow, columnWidth, isSelectedTask);

            lastRenderedRow = cardStartRow + TuiHelpers.CardLineCount;

            var separatorRow = cardStartRow + TuiHelpers.CardLineCount;
            if (separatorRow < startRow + maxRows)
            {
                RenderBlankLine(columnX, columnWidth, separatorRow);
                lastRenderedRow = separatorRow + 1;
            }
        }

        return lastRenderedRow;
    }

    private static void RenderScrollIndicators(Column column, int columnX, int columnWidth, int startRow, int maxRows, int scrollOffset, int endIndex)
    {
        var lastUsedRow = startRow + ((endIndex - scrollOffset) * TuiHelpers.CardTotalHeight) - 1;

        if (scrollOffset > 0)
        {
            RenderScrollIndicator(columnX, columnWidth, startRow, $" \u25B2 {scrollOffset} more");
        }

        var hiddenBelow = column.Tasks.Count - endIndex;
        if (hiddenBelow > 0)
        {
            var indicatorRow = Math.Min(lastUsedRow, startRow + maxRows - 1);
            if (indicatorRow >= startRow)
            {
                RenderScrollIndicator(columnX, columnWidth, indicatorRow, $" \u25BC {hiddenBelow} more");
            }
        }
    }

    private static void ClearRemainingRows(int columnX, int columnWidth, int fromRow, int toRow)
    {
        var blank = new string(' ', columnWidth);
        for (var row = fromRow; row < toRow; row++)
        {
            TuiHelpers.SafeSetCursorPosition(columnX, row);
            Console.Write(blank);
        }
    }

    private static void RenderBlankLine(int columnX, int columnWidth, int row)
    {
        TuiHelpers.SafeSetCursorPosition(columnX, row);
        Console.BackgroundColor = Theme.ColumnBg;
        Console.Write(new string(' ', columnWidth));
        Console.ResetColor();
    }

    private static void RenderScrollIndicator(int columnX, int columnWidth, int row, string text)
    {
        if (row < 0) return;
        TuiHelpers.SafeSetCursorPosition(columnX, row);
        Console.ForegroundColor = Theme.ScrollIndicator;
        var padded = text.Length > columnWidth
            ? text[..columnWidth]
            : text.PadRight(columnWidth);
        Console.Write(padded);
        Console.ResetColor();
    }

    private static void RenderEmptyPlaceholder(int columnX, int columnWidth, int row, bool isFiltered)
    {
        TuiHelpers.SafeSetCursorPosition(columnX, row);
        Console.ForegroundColor = Theme.ColumnEmptyText;
        var placeholder = isFiltered ? " (no matching tasks)" : " (no tasks)";
        var padded = placeholder.Length > columnWidth
            ? placeholder[..columnWidth]
            : placeholder.PadRight(columnWidth);
        Console.Write(padded);
        Console.ResetColor();
    }
}
