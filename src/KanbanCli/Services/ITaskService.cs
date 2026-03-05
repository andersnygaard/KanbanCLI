using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Services;

public interface ITaskService
{
    TaskItem CreateTask(string title, TaskType type, Priority priority, IReadOnlyList<string> labels);
    void UpdateTask(TaskItem task);
    void MoveTask(TaskItem task, TaskStatus targetColumn);
    void DeleteTask(TaskItem task);
    IReadOnlyList<TaskItem> GetAllByColumn(TaskStatus column);
    IReadOnlyList<TaskItem> GetAll();
}
