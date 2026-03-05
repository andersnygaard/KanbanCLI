namespace KanbanCli.Models;

public record Board
{
    public IReadOnlyList<Column> Columns { get; init; } = [];

    public int TotalTaskCount
    {
        get { return Columns.Sum(c => c.Tasks.Count); }
    }

    public Column? GetColumn(TaskStatus status)
    {
        return Columns.FirstOrDefault(c => c.Status == status);
    }
}
