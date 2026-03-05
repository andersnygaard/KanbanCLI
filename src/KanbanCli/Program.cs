using KanbanCli.Services;
using KanbanCli.Storage;
using KanbanCli.Tui;

var boardPath = args.Length > 0 ? args[0] : ".task-board";

var fileSystem = new FileSystem();
var markdownParser = new MarkdigMarkdownParser();
var repository = new MarkdownTaskRepository(fileSystem, markdownParser, boardPath);
var taskService = new TaskService(repository);
var boardService = new BoardService(repository);

var app = new KanbanApp(
    taskService,
    boardService,
    inputHandler: new KeyboardInputHandler(),
    boardRenderer: new BoardView(),
    taskDetailPanel: new TaskDetailPanel(),
    newTaskDialog: new NewTaskDialog(),
    moveDialog: new MoveDialog(),
    confirmDialog: new ConfirmDialog(),
    priorityDialog: new PriorityDialog(),
    filterDialog: new FilterDialog());

app.Run();
