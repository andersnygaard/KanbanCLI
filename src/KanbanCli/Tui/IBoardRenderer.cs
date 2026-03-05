namespace KanbanCli.Tui;
using KanbanCli.Models;

public interface IBoardRenderer
{
    void Render(Board board, NavigationState state, string? filterInfo = null);
}
