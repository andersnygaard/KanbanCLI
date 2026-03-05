namespace KanbanCli.Models;

public enum TaskType
{
    Feature,
    Bug,
    Refactor,
    Test,
    Security,
    Perf,
    Design,
    Docs,
    Epic,
    Explore,
    Cleanup,
    A11y,
    Quality
}

public enum Priority
{
    High,
    Medium,
    Low
}

public enum TaskStatus
{
    Backlog,
    InProgress,
    Done,
    OnHold
}

public static class TaskStatusExtensions
{
    public static string ToDisplayString(this TaskStatus status) => status switch
    {
        TaskStatus.Backlog => "Backlog",
        TaskStatus.InProgress => "In Progress",
        TaskStatus.Done => "Done",
        TaskStatus.OnHold => "On Hold",
        _ => status.ToString()
    };
}
