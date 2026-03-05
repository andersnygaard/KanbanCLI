namespace KanbanCli.Models;

public record Board
{
    public IReadOnlyList<Column> Columns { get; init; } = [];
}
