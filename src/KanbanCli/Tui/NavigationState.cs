namespace KanbanCli.Tui;

public record NavigationState
{
    public int SelectedColumn { get; init; }
    public int SelectedTask { get; init; }

    public NavigationState MoveToColumn(int direction, int maxColumns)
    {
        if (maxColumns <= 0) return this;
        var newColumn = Math.Clamp(SelectedColumn + direction, 0, maxColumns - 1);
        return this with { SelectedColumn = newColumn, SelectedTask = 0 };
    }

    public NavigationState MoveToTask(int direction, int maxTasks)
    {
        if (maxTasks <= 0) return this;
        var newTask = Math.Clamp(SelectedTask + direction, 0, maxTasks - 1);
        return this with { SelectedTask = newTask };
    }
}
