using KanbanCli.Models;
using KanbanCli.Storage;
using System.Text;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Services;

public class BoardService : IBoardService
{
    private readonly ITaskRepository _repository;

    public BoardService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public Board GetBoard()
    {
        var columns = new[]
        {
            new Column { Name = "Backlog", Tasks = _repository.GetAllByColumn(TaskStatus.Backlog) },
            new Column { Name = "In Progress", Tasks = _repository.GetAllByColumn(TaskStatus.InProgress) },
            new Column { Name = "Done", Tasks = _repository.GetAllByColumn(TaskStatus.Done) },
            new Column { Name = "On Hold", Tasks = _repository.GetAllByColumn(TaskStatus.OnHold) }
        };

        return new Board { Columns = columns };
    }

    public string GeneratePlanningBoard()
    {
        var allTasks = _repository.GetAll();

        if (!allTasks.Any())
            return BuildEmptyBoard();

        var inProgressTasks = allTasks
            .Where(t => t.Status == TaskStatus.InProgress)
            .ToList();

        var highPriorityBacklog = allTasks
            .Where(t => t.Status == TaskStatus.Backlog && t.Priority == Priority.High)
            .ToList();

        var topPriorities = inProgressTasks
            .Concat(highPriorityBacklog)
            .Take(5)
            .ToList();

        var recentlyCompleted = allTasks
            .Where(t => t.Status == TaskStatus.Done)
            .OrderByDescending(t => t.CompletedDate ?? DateTime.MinValue)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("# Planning Board");
        sb.AppendLine();
        sb.AppendLine("**Current Focus**: Feature development");
        sb.AppendLine();
        sb.AppendLine("## Top Priorities");
        sb.AppendLine();

        if (topPriorities.Count == 0)
        {
            sb.AppendLine("No active priorities.");
        }
        else
        {
            for (var i = 0; i < topPriorities.Count; i++)
            {
                var task = topPriorities[i];
                var statusLabel = task.Status.ToDisplayString();
                sb.AppendLine($"{i + 1}. **#{task.Id:D3}** {task.Type.ToString().ToUpperInvariant()}: {task.Title} - {statusLabel}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("## Recently Completed");
        sb.AppendLine();

        if (recentlyCompleted.Count == 0)
        {
            sb.AppendLine("No completed tasks.");
        }
        else
        {
            foreach (var task in recentlyCompleted)
            {
                var completedDateStr = task.CompletedDate.HasValue
                    ? task.CompletedDate.Value.ToString("yyyy-MM-dd")
                    : string.Empty;
                sb.AppendLine($"- **#{task.Id:D3}** {task.Type.ToString().ToUpperInvariant()}: {task.Title} - Done {completedDateStr}");
            }
        }

        return sb.ToString();
    }

    private static string BuildEmptyBoard()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Planning Board");
        sb.AppendLine();
        sb.AppendLine("**Current Focus**: Feature development");
        sb.AppendLine();
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
