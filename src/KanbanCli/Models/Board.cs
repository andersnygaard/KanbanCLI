namespace KanbanCli.Models;

public record Board
{
    public IReadOnlyList<Column> Columns { get; init; } = [];

    public int TotalTaskCount => Columns.Sum(c => c.Tasks.Count);

    public Column? GetColumn(TaskStatus status) =>
        Columns.FirstOrDefault(c => c.Status == status);
}
