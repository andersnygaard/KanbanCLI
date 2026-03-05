namespace KanbanCli.Storage;

public interface IFileSystem
{
    /// <summary>Returns all files in <paramref name="directory"/> matching <paramref name="pattern"/>.</summary>
    IEnumerable<string> GetFiles(string directory, string pattern);

    /// <summary>Reads the entire contents of the file at <paramref name="path"/>.</summary>
    string ReadAllText(string path);

    /// <summary>Writes <paramref name="content"/> to the file at <paramref name="path"/>, overwriting if it exists.</summary>
    void WriteAllText(string path, string content);

    /// <summary>Moves a file from <paramref name="source"/> to <paramref name="destination"/>.</summary>
    void MoveFile(string source, string destination);

    /// <summary>Deletes the file at <paramref name="path"/>.</summary>
    void DeleteFile(string path);

    /// <summary>Returns true if a file exists at <paramref name="path"/>.</summary>
    bool FileExists(string path);

    /// <summary>Returns true if the directory at <paramref name="path"/> exists.</summary>
    bool DirectoryExists(string path);

    /// <summary>Creates the directory at <paramref name="path"/>, including any parent directories.</summary>
    void CreateDirectory(string path);
}
