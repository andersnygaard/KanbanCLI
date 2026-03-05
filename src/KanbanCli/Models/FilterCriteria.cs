namespace KanbanCli.Models;

/// <summary>
/// Criteria for filtering tasks on the board. All null values mean "match any".
/// </summary>
/// <param name="Type">Filter by task type, or null for all types.</param>
/// <param name="Priority">Filter by priority level, or null for all priorities.</param>
/// <param name="Label">Filter by label name (case-insensitive), or null for all labels.</param>
public record FilterCriteria(TaskType? Type = null, Priority? Priority = null, string? Label = null);
