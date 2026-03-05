namespace KanbanCli.Storage;

public class FileSystem : IFileSystem
{
    public IEnumerable<string> GetFiles(string directory, string pattern)
    {
        return Directory.GetFiles(directory, pattern);
    }

    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public void MoveFile(string source, string destination)
    {
        File.Move(source, destination, overwrite: true);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }
}
