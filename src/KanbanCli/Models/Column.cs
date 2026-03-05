namespace KanbanCli.Models;

public record Column
{
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<TaskItem> Tasks { get; init; } = [];
}
