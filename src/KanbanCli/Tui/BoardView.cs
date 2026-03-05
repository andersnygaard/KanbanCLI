namespace KanbanCli.Tui;
using KanbanCli.Models;

public class BoardView : IBoardRenderer
{
    private readonly ColumnView _columnView = new();
    private readonly StatusBar _statusBar = new();

    public void Render(Board board, NavigationState state, string? filterInfo = null)
    {
        Console.Clear();
        Console.CursorVisible = false;

        var windowWidth = Math.Max(Console.WindowWidth, BoardConstants.MinWindowWidth);
        var windowHeight = Math.Max(Console.WindowHeight, BoardConstants.MinWindowHeight);

        RenderTitle(windowWidth);

        var columnCount = board.Columns.Count;
        if (columnCount == 0)
        {
            Console.SetCursorPosition(0, 2);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("No columns defined.");
            Console.ResetColor();
        }
        else
        {
            var columnWidth = windowWidth / columnCount;
            var boardStartRow = 2;
            var boardHeight = windowHeight - boardStartRow - 1; // reserve 1 row for status bar

            var isFiltered = filterInfo is not null;
            for (var i = 0; i < columnCount; i++)
            {
                var columnX = i * columnWidth;
                _columnView.Render(board.Columns[i], i, columnX, columnWidth, state, boardStartRow, boardHeight, isFiltered);
            }
        }

        var statusRow = windowHeight - 1;
        _statusBar.Render(statusRow, windowWidth, filterInfo);
    }

    private static void RenderTitle(int windowWidth)
    {
        Console.SetCursorPosition(0, 0);
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;

        var title = " Kanban Board";
        var padded = title.PadRight(windowWidth - 1);
        Console.Write(padded);
        Console.ResetColor();
    }
}
