namespace KanbanCli.Tui;
using KanbanCli.Models;

public class BoardView : IBoardRenderer
{
    private readonly IColumnView _columnView;
    private readonly StatusBar _statusBar = new();

    // Box-drawing characters
    private const char TopLeft = '\u250C';     // ┌
    private const char TopRight = '\u2510';    // ┐
    private const char BottomLeft = '\u2514';  // └
    private const char BottomRight = '\u2518'; // ┘
    private const char Horizontal = '\u2500';  // ─
    private const char Vertical = '\u2502';    // │
    private const char TopTee = '\u252C';      // ┬
    private const char BottomTee = '\u2534';   // ┴
    private const char LeftTee = '\u251C';     // ├
    private const char RightTee = '\u2524';    // ┤
    private const char Cross = '\u253C';       // ┼

    public BoardView(IColumnView columnView)
    {
        _columnView = columnView;
    }

    public void Render(Board board, NavigationState state, string? filterInfo = null)
    {
        Console.CursorVisible = false;
        TuiHelpers.SafeSetCursorPosition(0, 0);

        var windowWidth = TuiHelpers.GetEffectiveWidth();
        var windowHeight = TuiHelpers.GetEffectiveHeight();

        var columnCount = board.Columns.Count;
        if (columnCount == 0)
        {
            RenderEmptyBoard(windowWidth);
            return;
        }

        // Calculate column layout
        // Total width: │ col1 │ col2 │ col3 │ col4 │
        // We need (columnCount + 1) vertical bars, each takes 1 char
        // Usable content width per column = (windowWidth - columnCount - 1) / columnCount
        var separatorCount = columnCount + 1;
        var totalContentWidth = windowWidth - separatorCount;
        var baseColumnWidth = totalContentWidth / columnCount;
        var extraPixels = totalContentWidth % columnCount;

        // Build column widths array (distribute extra pixels to first columns)
        var columnWidths = new int[columnCount];
        var columnXPositions = new int[columnCount];
        for (var i = 0; i < columnCount; i++)
        {
            columnWidths[i] = baseColumnWidth + (i < extraPixels ? 1 : 0);
        }

        // Calculate X positions for content (after the left border │)
        var currentX = 1; // Start after left border │
        for (var i = 0; i < columnCount; i++)
        {
            columnXPositions[i] = currentX;
            currentX += columnWidths[i] + 1; // +1 for the separator after this column
        }

        // Row layout:
        // Row 0: Top border       ┌───────┬───────┐
        // Row 1: Title bar        │ ◼ Kanban Board │
        // Row 2: Header separator ├───────┼───────┤
        // Row 3: Column headers   │ Backlog [3] │ ...
        // Row 4: Body separator   ├───────┼───────┤
        // Row 5..N-3: Task cards  │ task  │ task  │
        // Row N-2: Bottom body    ├───────┴───────┤
        // Row N-1: Status bar     │ keybindings   │
        // Row N:   Bottom border  └───────────────┘

        var topBorderRow = 0;
        var titleRow = 1;
        var headerSepRow = 2;
        var headerRow = 3;
        var bodySepRow = 4;
        var bodyStartRow = 5;
        var statusBarRow = windowHeight - 2;
        var bottomBorderRow = windowHeight - 1;
        var bottomBodyRow = statusBarRow - 1;
        var bodyHeight = bottomBodyRow - bodyStartRow;

        // Render frame
        RenderHorizontalLine(topBorderRow, windowWidth, columnWidths, TopLeft, TopTee, TopRight, Theme.BoardBorder);
        RenderTitleBar(titleRow, windowWidth, Theme.BoardBorder);
        RenderHorizontalLine(headerSepRow, windowWidth, columnWidths, LeftTee, Cross, RightTee, Theme.BoardBorder);

        // Render column headers
        for (var i = 0; i < columnCount; i++)
        {
            var column = board.Columns[i];
            var isSelected = state.SelectedColumn == i;
            RenderColumnHeader(column, columnXPositions[i], columnWidths[i], headerRow, isSelected);
        }
        // Draw vertical separators for header row
        RenderVerticalSeparators(headerRow, columnXPositions, columnWidths, columnCount, windowWidth, state.SelectedColumn, Theme.BoardBorder);

        RenderHorizontalLine(bodySepRow, windowWidth, columnWidths, LeftTee, Cross, RightTee, Theme.BoardBorder);

        // Render column body content (tasks)
        var isFiltered = filterInfo is not null;
        for (var i = 0; i < columnCount; i++)
        {
            _columnView.Render(board.Columns[i], i, columnXPositions[i], columnWidths[i], state, bodyStartRow, bodyHeight, isFiltered);
        }

        // Draw vertical separators for all body rows
        for (var row = bodyStartRow; row < bottomBodyRow; row++)
        {
            RenderVerticalSeparators(row, columnXPositions, columnWidths, columnCount, windowWidth, state.SelectedColumn, Theme.BoardBorder);
        }

        // Bottom body border (uses ┴ between columns to close them off)
        RenderHorizontalLine(bottomBodyRow, windowWidth, columnWidths, LeftTee, BottomTee, RightTee, Theme.BoardBorder);

        // Status bar with position info
        string? positionInfo = null;
        if (columnCount > 0 && state.SelectedColumn < columnCount)
        {
            var selectedCol = board.Columns[state.SelectedColumn];
            if (selectedCol.Tasks.Count > 0)
            {
                var taskIndex = Math.Min(state.SelectedTask, selectedCol.Tasks.Count - 1);
                positionInfo = $"Task {taskIndex + 1}/{selectedCol.Tasks.Count}";
            }
        }
        _statusBar.Render(statusBarRow, windowWidth, filterInfo, positionInfo);

        // Bottom border
        RenderSimpleHorizontalLine(bottomBorderRow, windowWidth, BottomLeft, BottomRight, Theme.BoardBorder);
    }

    private static void RenderEmptyBoard(int windowWidth)
    {
        TuiHelpers.SafeSetCursorPosition(0, 0);
        Console.ForegroundColor = Theme.BoardBorder;
        Console.Write(TopLeft + new string(Horizontal, windowWidth - 2) + TopRight);
        TuiHelpers.SafeSetCursorPosition(0, 1);
        Console.Write(Vertical);
        Console.Write(" No columns defined.".PadRight(windowWidth - 2));
        Console.Write(Vertical);
        TuiHelpers.SafeSetCursorPosition(0, 2);
        Console.Write(BottomLeft + new string(Horizontal, windowWidth - 2) + BottomRight);
        Console.ResetColor();
    }

    private static void RenderTitleBar(int row, int windowWidth, ConsoleColor borderColor)
    {
        TuiHelpers.SafeSetCursorPosition(0, row);
        Console.ForegroundColor = borderColor;
        Console.Write(Vertical);

        Console.BackgroundColor = Theme.BoardTitleBg;
        Console.ForegroundColor = Theme.BoardTitleFg;
        var title = " Kanban Board";
        var padded = title.PadRight(windowWidth - 2);
        Console.Write(padded);

        Console.ResetColor();
        Console.ForegroundColor = borderColor;
        Console.Write(Vertical);
        Console.ResetColor();
    }

    private static void RenderHorizontalLine(int row, int windowWidth, int[] columnWidths, char leftChar, char junctionChar, char rightChar, ConsoleColor color)
    {
        TuiHelpers.SafeSetCursorPosition(0, row);
        Console.ForegroundColor = color;

        Console.Write(leftChar);
        for (var i = 0; i < columnWidths.Length; i++)
        {
            Console.Write(new string(Horizontal, columnWidths[i]));
            Console.Write(i < columnWidths.Length - 1 ? junctionChar : rightChar);
        }

        Console.ResetColor();
    }

    private static void RenderSimpleHorizontalLine(int row, int windowWidth, char leftChar, char rightChar, ConsoleColor color)
    {
        TuiHelpers.SafeSetCursorPosition(0, row);
        Console.ForegroundColor = color;
        Console.Write(leftChar);
        Console.Write(new string(Horizontal, windowWidth - 2));
        Console.Write(rightChar);
        Console.ResetColor();
    }

    private static void RenderColumnHeader(Column column, int contentX, int contentWidth, int row, bool isSelected)
    {
        TuiHelpers.SafeSetCursorPosition(contentX, row);

        if (isSelected)
        {
            Console.ForegroundColor = Theme.ColumnHeaderSelectedFg;
            Console.BackgroundColor = Theme.ColumnHeaderSelectedBg;
        }
        else
        {
            Console.ForegroundColor = Theme.ColumnHeaderFg;
            Console.BackgroundColor = Theme.ColumnHeaderBg;
        }

        var headerText = $" {column.Name} [{column.Tasks.Count}]";
        var paddedHeader = headerText.PadRight(contentWidth);
        Console.Write(paddedHeader);
        Console.ResetColor();
    }

    private static void RenderVerticalSeparators(int row, int[] columnXPositions, int[] columnWidths, int columnCount, int windowWidth, int selectedColumn, ConsoleColor defaultColor)
    {
        // Left border
        TuiHelpers.SafeSetCursorPosition(0, row);
        var leftBorderColor = selectedColumn == 0 ? Theme.SelectedBorder : defaultColor;
        Console.ForegroundColor = leftBorderColor;
        Console.Write(Vertical);
        Console.ResetColor();

        // Column separators between columns and right border
        for (var i = 0; i < columnCount; i++)
        {
            var separatorX = columnXPositions[i] + columnWidths[i];
            TuiHelpers.SafeSetCursorPosition(separatorX, row);

            // Separator is highlighted if it borders the selected column
            var isAdjacentToSelected = (i == selectedColumn) || (i + 1 == selectedColumn);
            var isRightBorder = (i == columnCount - 1);
            ConsoleColor sepColor;
            if (isRightBorder)
            {
                sepColor = selectedColumn == columnCount - 1 ? Theme.SelectedBorder : defaultColor;
            }
            else
            {
                sepColor = isAdjacentToSelected ? Theme.SelectedBorder : defaultColor;
            }

            Console.ForegroundColor = sepColor;
            Console.Write(Vertical);
            Console.ResetColor();
        }
    }
}
