using KanbanCli.Models;
using KanbanCli.Storage;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    private const int MaxTitleLength = 200;

    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

    public TaskItem CreateTask(string title, TaskType type, Priority priority, IReadOnlyList<string> labels)
    {
        ValidateTitle(title);

        var id = _repository.GetNextId();
        var createdDate = DateTime.UtcNow;
        var task = new TaskItem
        {
            Id = id,
            Title = title,
            Type = type,
            Priority = priority,
            Status = TaskStatus.Backlog,
            Labels = labels,
            CreatedDate = createdDate,
            Sections = new Dictionary<string, string>
            {
                ["Context & Motivation"] = "(No description provided)",
                ["Acceptance Criteria"] = "- [ ] (To be defined)",
                ["Progress Log"] = $"- {createdDate.ToString(BoardConstants.DateFormat)} - Task created"
            }
        };

        _repository.Save(task);
        return task;
    }

    public void UpdateTask(TaskItem task)
    {
        _repository.Update(task);
    }

    public void MoveTask(TaskItem task, TaskStatus targetColumn)
    {
        _repository.Move(task, targetColumn);
    }

    public void DeleteTask(TaskItem task)
    {
        _repository.Delete(task);
    }

    public IReadOnlyList<TaskItem> GetAllByColumn(TaskStatus column)
    {
        return _repository.GetAllByColumn(column);
    }

    public IReadOnlyList<TaskItem> GetAll()
    {
        return _repository.GetAll();
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty or whitespace.", nameof(title));

        if (title.Length > MaxTitleLength)
            throw new ArgumentException($"Task title cannot exceed {MaxTitleLength} characters.", nameof(title));

        if (title.Any(c => InvalidFileNameChars.Contains(c)))
            throw new ArgumentException("Task title contains invalid filename characters.", nameof(title));
    }
}
