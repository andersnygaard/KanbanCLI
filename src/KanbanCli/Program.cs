using KanbanCli.Services;
using KanbanCli.Storage;
using KanbanCli.Tui;

var boardPath = args.Length > 0 ? args[0] : ".task-board";

var fileSystem = new FileSystem();

// Ensure board directory and all column folders exist on startup
BoardBootstrapper.EnsureBoardDirectories(fileSystem, boardPath);

var markdownParser = new MarkdigMarkdownParser();
var repository = new MarkdownTaskRepository(fileSystem, markdownParser, boardPath);
var taskService = new TaskService(repository);
var boardService = new BoardService(repository, fileSystem);

var app = new KanbanApp(
    taskService,
    boardService,
    inputHandler: new KeyboardInputHandler(),
    boardRenderer: new BoardView(new ColumnView(new TaskCard())),
    taskDetailPanel: new TaskDetailPanel(taskService),
    newTaskDialog: new NewTaskDialog(),
    moveDialog: new MoveDialog(),
    confirmDialog: new ConfirmDialog(),
    priorityDialog: new PriorityDialog(),
    filterDialog: new FilterDialog(),
    boardPath: boardPath);

try
{
    app.Run();
}
catch (IOException ex)
{
    Console.ResetColor();
    Console.Error.WriteLine($"File system error: {ex.Message}");
    Environment.Exit(1);
}
catch (UnauthorizedAccessException ex)
{
    Console.ResetColor();
    Console.Error.WriteLine($"Permission denied: {ex.Message}");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.ResetColor();
    Console.Error.WriteLine($"Unexpected error: {ex.Message}");
    Environment.Exit(1);
}
