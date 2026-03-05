namespace KanbanCli.Tui;
using KanbanCli.Models;

/// <summary>Renders a single column with its task list.</summary>
public interface IColumnView
{
    void Render(Column column, int columnIndex, int columnX, int columnWidth, NavigationState state, int startRow, int maxRows, bool isFiltered = false);
}
