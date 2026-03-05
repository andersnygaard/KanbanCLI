using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Storage;

public interface ITaskRepository
{
    IReadOnlyList<TaskItem> GetAllByColumn(TaskStatus column);
    IReadOnlyList<TaskItem> GetAll();
    void Save(TaskItem task);
    void Update(TaskItem task);
    void Move(TaskItem task, TaskStatus targetColumn);
    void Delete(TaskItem task);
    int GetNextId();
}
