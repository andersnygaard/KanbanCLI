namespace KanbanCli.Models;

public record Column
{
    public string Name { get; init; } = string.Empty;
    public TaskStatus Status { get; init; }
    public IReadOnlyList<TaskItem> Tasks { get; init; } = [];

    public bool IsEmpty => Tasks.Count == 0;
}
