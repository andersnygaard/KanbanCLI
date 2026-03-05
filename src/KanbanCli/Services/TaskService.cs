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

    public TaskItem CreateTask(string title, TaskType type, Priority priority, IReadOnlyList<string> labels)
    {
        var id = _repository.GetNextId();
        var task = new TaskItem
        {
            Id = id,
            Title = title,
            Type = type,
            Priority = priority,
            Status = TaskStatus.Backlog,
            Labels = labels,
            CreatedDate = DateTime.UtcNow
        };

        _repository.Save(task);
        return task;
    }

    public void MoveTask(TaskItem task, TaskStatus targetColumn)
    {
        var updatedTask = task.ChangeStatus(targetColumn);
        _repository.Move(updatedTask, targetColumn);
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
}
