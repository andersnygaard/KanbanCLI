using KanbanCli.Models;
using KanbanCli.Storage;
using System.Text;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Services;

public class BoardService : IBoardService
{
    private readonly ITaskRepository _repository;
    private readonly IFileSystem _fileSystem;

    public BoardService(ITaskRepository repository, IFileSystem fileSystem)
    {
        _repository = repository;
        _fileSystem = fileSystem;
    }

    public Board GetBoard()
    {
        var columns = BoardConstants.ColumnOrder
            .Select(status => new Column
            {
                Name = BoardConstants.ColumnDisplayNames[status],
                Status = status,
                Tasks = _repository.GetAllByColumn(status)
            })
            .ToArray();

        return new Board { Columns = columns };
    }

    public string GeneratePlanningBoard()
    {
        var allTasks = _repository.GetAll();

        if (!allTasks.Any())
            return BuildEmptyBoard();

        var topPriorities = allTasks
            .Where(t => t.Status == TaskStatus.InProgress)
            .Concat(allTasks.Where(t => t.Status == TaskStatus.Backlog && t.Priority == Priority.High))
            .Concat(allTasks.Where(t => t.Status == TaskStatus.Backlog && t.Priority == Priority.Medium))
            .Take(BoardConstants.MaxTopPriorities)
            .ToList();

        var recentlyCompleted = allTasks
            .Where(t => t.Status == TaskStatus.Done)
            .OrderByDescending(t => t.CompletedDate ?? DateTime.MinValue)
            .Take(BoardConstants.MaxRecentlyCompleted)
            .ToList();

        var sb = new StringBuilder();
        sb.Append(BuildHeader());
        sb.Append(BuildPrioritySection(topPriorities));
        sb.Append(BuildCompletedSection(recentlyCompleted));

        return sb.ToString();
    }

    public void SavePlanningBoard(string boardPath)
    {
        var content = GeneratePlanningBoard();
        var filePath = Path.Combine(boardPath, "PLANNING-BOARD.md");
        _fileSystem.WriteAllText(filePath, content);
    }

    private static string BuildHeader()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Planning Board");
        sb.AppendLine();
        sb.AppendLine("**Current Focus**: Feature development");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildPrioritySection(IReadOnlyList<TaskItem> priorities)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Top Priorities");
        sb.AppendLine();

        if (priorities.Count == 0)
        {
            sb.AppendLine("No active priorities.");
        }
        else
        {
            for (var i = 0; i < priorities.Count; i++)
            {
                var task = priorities[i];
                var statusLabel = task.Status.ToDisplayString();
                sb.AppendLine($"{i + 1}. **#{task.Id.ToString(BoardConstants.IdFormat)}** {task.Type.ToString().ToUpperInvariant()}: {task.Title} - {statusLabel}");
            }
        }

        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildCompletedSection(IReadOnlyList<TaskItem> completed)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Recently Completed");
        sb.AppendLine();

        if (completed.Count == 0)
        {
            sb.AppendLine("No completed tasks.");
        }
        else
        {
            foreach (var task in completed)
            {
                var completedDateStr = task.CompletedDate.HasValue
                    ? task.CompletedDate.Value.ToString(BoardConstants.DateFormat)
                    : string.Empty;
                sb.AppendLine($"- **#{task.Id.ToString(BoardConstants.IdFormat)}** {task.Type.ToString().ToUpperInvariant()}: {task.Title} - Done {completedDateStr}");
            }
        }

        return sb.ToString();
    }

    private static string BuildEmptyBoard()
    {
        var sb = new StringBuilder();
        sb.Append(BuildHeader());
        sb.AppendLine("## Top Priorities");
        sb.AppendLine();
        sb.AppendLine("No tasks yet.");
        sb.AppendLine();
        sb.AppendLine("## Recently Completed");
        sb.AppendLine();
        sb.AppendLine("No tasks yet.");
        return sb.ToString();
    }

}
