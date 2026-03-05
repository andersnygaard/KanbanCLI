using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Models;

public static class BoardConstants
{
    /// <summary>
    /// Mapping from TaskStatus to the folder name used on disk.
    /// </summary>
    public static readonly IReadOnlyDictionary<TaskStatus, string> FolderNames =
        new Dictionary<TaskStatus, string>
        {
            [TaskStatus.Backlog] = "backlog",
            [TaskStatus.InProgress] = "in-progress",
            [TaskStatus.Done] = "done",
            [TaskStatus.OnHold] = "on-hold"
        };

    /// <summary>
    /// Display names for each column shown in the board UI.
    /// </summary>
    public static readonly IReadOnlyDictionary<TaskStatus, string> ColumnDisplayNames =
        new Dictionary<TaskStatus, string>
        {
            [TaskStatus.Backlog] = "Backlog",
            [TaskStatus.InProgress] = "In Progress",
            [TaskStatus.Done] = "Done",
            [TaskStatus.OnHold] = "On Hold"
        };

    /// <summary>
    /// The ordered list of columns as they appear on the board.
    /// </summary>
    public static readonly IReadOnlyList<TaskStatus> ColumnOrder =
    [
        TaskStatus.Backlog,
        TaskStatus.InProgress,
        TaskStatus.Done,
        TaskStatus.OnHold
    ];

    /// <summary>
    /// Format string for zero-padded task IDs (e.g. 001, 012).
    /// </summary>
    public const string IdFormat = "D3";

    /// <summary>
    /// Date format used for serializing dates in markdown files.
    /// </summary>
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// File extension for task markdown files.
    /// </summary>
    public const string TaskFileExtension = "*.md";

    /// <summary>
    /// Minimum window width used when rendering the board.
    /// </summary>
    public const int MinWindowWidth = 40;

    /// <summary>
    /// Minimum window height used when rendering the board.
    /// </summary>
    public const int MinWindowHeight = 10;

    /// <summary>
    /// Maximum number of top-priority items shown in the planning board.
    /// </summary>
    public const int MaxTopPriorities = 5;
}
