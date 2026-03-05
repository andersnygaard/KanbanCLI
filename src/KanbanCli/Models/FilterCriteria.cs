namespace KanbanCli.Models;

public record FilterCriteria(TaskType? Type = null, Priority? Priority = null, string? Label = null);
