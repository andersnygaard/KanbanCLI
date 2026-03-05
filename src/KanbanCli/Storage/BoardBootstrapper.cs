using KanbanCli.Models;

namespace KanbanCli.Storage;

/// <summary>
/// Ensures the board directory structure exists on startup.
/// Creates the root board directory and all column folders if they are missing.
/// </summary>
public static class BoardBootstrapper
{
    /// <summary>
    /// Creates the board root directory and all column subdirectories if they do not exist.
    /// This method is idempotent — calling it on an existing board structure is a no-op.
    /// </summary>
    public static void EnsureBoardDirectories(IFileSystem fileSystem, string boardPath)
    {
        if (!fileSystem.DirectoryExists(boardPath))
        {
            fileSystem.CreateDirectory(boardPath);
        }

        foreach (var folderName in BoardConstants.FolderNames.Values)
        {
            var columnPath = Path.Combine(boardPath, folderName);
            if (!fileSystem.DirectoryExists(columnPath))
            {
                fileSystem.CreateDirectory(columnPath);
            }
        }
    }
}
