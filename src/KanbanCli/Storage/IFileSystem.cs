namespace KanbanCli.Storage;

public interface IFileSystem
{
    IEnumerable<string> GetFiles(string directory, string pattern);
    string ReadAllText(string path);
    void WriteAllText(string path, string content);
    void MoveFile(string source, string destination);
    void DeleteFile(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
}
