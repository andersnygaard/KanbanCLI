namespace KanbanCli.Tui;
using KanbanCli.Models;
using KanbanCli.Services;
using TaskStatus = KanbanCli.Models.TaskStatus;

public class KanbanApp
{
    private readonly ITaskService _taskService;
    private readonly IBoardService _boardService;
    private readonly IInputHandler _inputHandler;
    private readonly IBoardRenderer _boardRenderer;
    private readonly TaskDetailPanel _taskDetailPanel;
    private readonly NewTaskDialog _newTaskDialog;
    private readonly MoveDialog _moveDialog;
    private readonly ConfirmDialog _confirmDialog;
    private readonly PriorityDialog _priorityDialog;
    private readonly FilterDialog _filterDialog;

    public KanbanApp(
        ITaskService taskService,
        IBoardService boardService,
        IInputHandler inputHandler,
        IBoardRenderer boardRenderer,
        TaskDetailPanel taskDetailPanel,
        NewTaskDialog newTaskDialog,
        MoveDialog moveDialog,
        ConfirmDialog confirmDialog,
        PriorityDialog priorityDialog,
        FilterDialog filterDialog)
    {
        _taskService = taskService;
        _boardService = boardService;
        _inputHandler = inputHandler;
        _boardRenderer = boardRenderer;
        _taskDetailPanel = taskDetailPanel;
        _newTaskDialog = newTaskDialog;
        _moveDialog = moveDialog;
        _confirmDialog = confirmDialog;
        _priorityDialog = priorityDialog;
        _filterDialog = filterDialog;
    }

    public void Run()
    {
        var state = new NavigationState();
        FilterCriteria? activeFilter = null;
        var running = true;

        Console.OutputEncoding = System.Text.Encoding.UTF8;

        while (running)
        {
            var board = _boardService.GetBoard();
            var displayBoard = activeFilter is not null ? ApplyFilter(board, activeFilter) : board;
            var filterInfo = BuildFilterInfo(activeFilter);
            _boardRenderer.Render(displayBoard, state, filterInfo);

            var command = _inputHandler.ReadCommand();

            switch (command)
            {
                case BoardCommand.Quit:
                    running = false;
                    break;

                case BoardCommand.MoveLeft:
                    state = state.MoveToColumn(-1, displayBoard.Columns.Count);
                    break;

                case BoardCommand.MoveRight:
                    state = state.MoveToColumn(1, displayBoard.Columns.Count);
                    break;

                case BoardCommand.MoveUp:
                {
                    var col = displayBoard.Columns[state.SelectedColumn];
                    state = state.MoveToTask(-1, col.Tasks.Count);
                    break;
                }

                case BoardCommand.MoveDown:
                {
                    var col = displayBoard.Columns[state.SelectedColumn];
                    state = state.MoveToTask(1, col.Tasks.Count);
                    break;
                }

                case BoardCommand.ViewDetails:
                    HandleViewDetails(displayBoard, state);
                    break;

                case BoardCommand.NewTask:
                    state = HandleNewTask(displayBoard, state);
                    break;

                case BoardCommand.MoveTask:
                    state = HandleMoveTask(board, displayBoard, state);
                    break;

                case BoardCommand.DeleteTask:
                    state = HandleDeleteTask(displayBoard, state);
                    break;

                case BoardCommand.ChangePriority:
                    HandleChangePriority(displayBoard, state);
                    break;

                case BoardCommand.ToggleFilter:
                    (activeFilter, state) = HandleToggleFilter(board, activeFilter, state);
                    break;

                case BoardCommand.None:
                default:
                    break;
            }
        }

        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("Goodbye!");
    }

    private void HandleViewDetails(Board displayBoard, NavigationState state)
    {
        var col = displayBoard.Columns[state.SelectedColumn];
        if (col.Tasks.Count > 0 && state.SelectedTask < col.Tasks.Count)
            _taskDetailPanel.Show(col.Tasks[state.SelectedTask]);
    }

    private NavigationState HandleNewTask(Board displayBoard, NavigationState state)
    {
        var inputs = _newTaskDialog.Show();
        if (inputs is null)
            return state;

        _taskService.CreateTask(inputs.Title, inputs.Type, inputs.Priority, inputs.Labels);
        return state.MoveToColumn(0, displayBoard.Columns.Count);
    }

    private NavigationState HandleMoveTask(Board board, Board displayBoard, NavigationState state)
    {
        var col = displayBoard.Columns[state.SelectedColumn];
        if (col.Tasks.Count == 0 || state.SelectedTask >= col.Tasks.Count)
            return state;

        var task = col.Tasks[state.SelectedTask];
        var targetStatus = _moveDialog.Show(board, state.SelectedColumn);
        if (targetStatus is null)
            return state;

        _taskService.MoveTask(task, targetStatus.Value);
        return state with { SelectedTask = 0 };
    }

    private NavigationState HandleDeleteTask(Board displayBoard, NavigationState state)
    {
        var col = displayBoard.Columns[state.SelectedColumn];
        if (col.Tasks.Count == 0 || state.SelectedTask >= col.Tasks.Count)
            return state;

        var task = col.Tasks[state.SelectedTask];
        if (!_confirmDialog.Confirm(task))
            return state;

        _taskService.DeleteTask(task);
        return state with { SelectedTask = Math.Max(0, state.SelectedTask - 1) };
    }

    private void HandleChangePriority(Board displayBoard, NavigationState state)
    {
        var col = displayBoard.Columns[state.SelectedColumn];
        if (col.Tasks.Count == 0 || state.SelectedTask >= col.Tasks.Count)
            return;

        var task = col.Tasks[state.SelectedTask];
        var newPriority = _priorityDialog.Show(task.Priority);
        if (newPriority == task.Priority)
            return;

        var updatedTask = task.SetPriority(newPriority);
        _taskService.MoveTask(updatedTask, updatedTask.Status);
    }

    private (FilterCriteria? filter, NavigationState state) HandleToggleFilter(
        Board board, FilterCriteria? activeFilter, NavigationState state)
    {
        if (activeFilter is not null)
            return (null, state with { SelectedTask = 0 });

        var allLabels = board.Columns
            .SelectMany(c => c.Tasks)
            .SelectMany(t => t.Labels)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(l => l)
            .ToList()
            .AsReadOnly();

        var newFilter = _filterDialog.Show(allLabels);
        if (newFilter is null)
            return (null, state);

        return (newFilter, state with { SelectedTask = 0 });
    }

    private static Board ApplyFilter(Board board, FilterCriteria filter)
    {
        var filteredColumns = board.Columns
            .Select(col => col with { Tasks = col.Tasks.Where(t => t.MatchesFilter(filter)).ToList().AsReadOnly() })
            .ToList()
            .AsReadOnly();

        return board with { Columns = filteredColumns };
    }

    private static string? BuildFilterInfo(FilterCriteria? filter)
    {
        if (filter is null)
            return null;

        if (filter.Type is not null)
            return filter.Type.Value.ToString();

        if (filter.Label is not null)
            return filter.Label;

        if (filter.Priority is not null)
            return filter.Priority.Value.ToString();

        return null;
    }
}
