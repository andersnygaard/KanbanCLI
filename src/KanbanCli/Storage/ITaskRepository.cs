using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Storage;

/// <summary>Persists and retrieves task files from the board's folder structure.</summary>
public interface ITaskRepository
{
    /// <summary>Returns all tasks in the specified column folder.</summary>
    IReadOnlyList<TaskItem> GetAllByColumn(TaskStatus column);

    /// <summary>Returns all tasks across all column folders.</summary>
    IReadOnlyList<TaskItem> GetAll();

    /// <summary>Creates a new task file in the folder matching the task's status.</summary>
    void Save(TaskItem task);

    /// <summary>Overwrites an existing task file in place (same folder).</summary>
    void Update(TaskItem task);

    /// <summary>
    /// Moves a task to a different column. Uses write+delete rather than filesystem move
    /// because the task content changes during a move (status field is updated in the markdown).
    /// </summary>
    void Move(TaskItem task, TaskStatus targetColumn);

    /// <summary>Deletes the task file from disk.</summary>
    void Delete(TaskItem task);

    /// <summary>Returns the next available sequential task ID across all columns.</summary>
    int GetNextId();
}
