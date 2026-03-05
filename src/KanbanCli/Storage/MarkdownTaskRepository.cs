using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Storage;

public class MarkdownTaskRepository : ITaskRepository
{
    private readonly IFileSystem _fileSystem;
    private readonly IMarkdownParser _parser;
    private readonly string _boardPath;

    private static readonly IReadOnlyDictionary<TaskStatus, string> FolderNames = BoardConstants.FolderNames;

    public MarkdownTaskRepository(IFileSystem fileSystem, IMarkdownParser parser, string boardPath)
    {
        _fileSystem = fileSystem;
        _parser = parser;
        _boardPath = boardPath;
    }

    public IReadOnlyList<TaskItem> GetAllByColumn(TaskStatus column)
    {
        var folderPath = GetColumnPath(column);

        if (!_fileSystem.DirectoryExists(folderPath))
            return [];

        var files = _fileSystem.GetFiles(folderPath, BoardConstants.TaskFileExtension);
        var tasks = new List<TaskItem>();

        foreach (var file in files)
        {
            var task = TryParseFile(file);
            if (task is not null)
                tasks.Add(task);
        }

        return tasks.AsReadOnly();
    }

    public IReadOnlyList<TaskItem> GetAll()
    {
        var allTasks = new List<TaskItem>();

        foreach (var status in FolderNames.Keys)
        {
            allTasks.AddRange(GetAllByColumn(status));
        }

        return allTasks.AsReadOnly();
    }

    public void Save(TaskItem task)
    {
        var folderPath = GetColumnPath(task.Status);

        if (!_fileSystem.DirectoryExists(folderPath))
            _fileSystem.CreateDirectory(folderPath);

        var fileName = task.GenerateFileName();
        var filePath = Path.Combine(folderPath, fileName);
        var content = _parser.Serialize(task);

        _fileSystem.WriteAllText(filePath, content);
    }

    public void Update(TaskItem task)
    {
        var folderPath = GetColumnPath(task.Status);
        var fileName = task.GenerateFileName();
        var filePath = Path.Combine(folderPath, fileName);
        var content = _parser.Serialize(task);

        _fileSystem.WriteAllText(filePath, content);
    }

    public void Move(TaskItem task, TaskStatus targetColumn)
    {
        var sourceFolderPath = GetColumnPath(task.Status);
        var sourceFileName = task.GenerateFileName();
        var sourcePath = Path.Combine(sourceFolderPath, sourceFileName);

        var updatedTask = task.ChangeStatus(targetColumn);

        var targetFolderPath = GetColumnPath(targetColumn);
        if (!_fileSystem.DirectoryExists(targetFolderPath))
            _fileSystem.CreateDirectory(targetFolderPath);

        var targetFileName = updatedTask.GenerateFileName();
        var targetPath = Path.Combine(targetFolderPath, targetFileName);

        var content = _parser.Serialize(updatedTask);
        _fileSystem.WriteAllText(targetPath, content);

        if (sourcePath != targetPath)
            _fileSystem.DeleteFile(sourcePath);
    }

    public void Delete(TaskItem task)
    {
        var folderPath = GetColumnPath(task.Status);
        var fileName = task.GenerateFileName();
        var filePath = Path.Combine(folderPath, fileName);

        _fileSystem.DeleteFile(filePath);
    }

    public int GetNextId()
    {
        var maxId = 0;

        foreach (var status in FolderNames.Keys)
        {
            var folderPath = GetColumnPath(status);

            if (!_fileSystem.DirectoryExists(folderPath))
                continue;

            var files = _fileSystem.GetFiles(folderPath, BoardConstants.TaskFileExtension);

            foreach (var file in files)
            {
                var id = TryExtractId(file);
                if (id > maxId)
                    maxId = id;
            }
        }

        return maxId + 1;
    }

    private string GetColumnPath(TaskStatus status)
        => Path.Combine(_boardPath, FolderNames[status]);

    private TaskItem? TryParseFile(string filePath)
    {
        try
        {
            var (id, type, _) = _parser.ParseFileName(filePath);
            var content = _fileSystem.ReadAllText(filePath);
            return _parser.Parse(content, id, type);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private int TryExtractId(string filePath)
    {
        try
        {
            var (id, _, _) = _parser.ParseFileName(filePath);
            return id;
        }
        catch (IOException)
        {
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            return 0;
        }
        catch (FormatException)
        {
            return 0;
        }
        catch (ArgumentException)
        {
            return 0;
        }
    }
}
