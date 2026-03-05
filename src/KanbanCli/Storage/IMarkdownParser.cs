using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Storage;

public interface IMarkdownParser
{
    TaskItem Parse(string markdown, int id, TaskType type);
    string Serialize(TaskItem task);
    (int Id, TaskType Type, string Description) ParseFileName(string fileName);
}
