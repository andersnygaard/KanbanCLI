namespace KanbanCli.Tui;
using KanbanCli.Models;

/// <summary>Renders a single task row in a column.</summary>
public interface ITaskCard
{
    void RenderWithColors(TaskItem task, int columnX, int row, int columnWidth, bool isSelected);
}
