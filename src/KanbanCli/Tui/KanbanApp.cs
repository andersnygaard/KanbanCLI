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
    private readonly string _boardPath;
    private readonly Dictionary<BoardCommand, Action> _commandDispatch;

    private NavigationState _state = new();
    private FilterCriteria? _activeFilter;
    private Board _board = default!;
    private Board _displayBoard = default!;
    private bool _running;

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
        FilterDialog filterDialog,
        string boardPath)
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
        _boardPath = boardPath;

        _commandDispatch = InitializeCommandDispatch();
    }

    private Dictionary<BoardCommand, Action> InitializeCommandDispatch() => new()
    {
        [BoardCommand.Quit] = HandleQuit,
        [BoardCommand.MoveLeft] = HandleMoveLeft,
        [BoardCommand.MoveRight] = HandleMoveRight,
        [BoardCommand.MoveUp] = HandleMoveUp,
        [BoardCommand.MoveDown] = HandleMoveDown,
        [BoardCommand.ViewDetails] = HandleViewDetails,
        [BoardCommand.NewTask] = HandleNewTask,
        [BoardCommand.MoveTask] = HandleMoveTask,
        [BoardCommand.DeleteTask] = HandleDeleteTask,
        [BoardCommand.ChangePriority] = HandleChangePriority,
        [BoardCommand.ToggleFilter] = HandleToggleFilter,
    };

    public void Run()
    {
        _state = new NavigationState();
        _activeFilter = null;
        _running = true;

        Console.OutputEncoding = System.Text.Encoding.UTF8;

        while (_running)
        {
            _board = _boardService.GetBoard();
            _displayBoard = _activeFilter is not null ? ApplyFilter(_board, _activeFilter) : _board;
            var filterInfo = BuildFilterInfo(_activeFilter);
            _boardRenderer.Render(_displayBoard, _state, filterInfo);

            var command = _inputHandler.ReadCommand();

            if (_commandDispatch.TryGetValue(command, out var handler))
                handler();
        }

        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("Goodbye!");
    }

    private void HandleQuit() => _running = false;

    private void HandleMoveLeft() =>
        _state = _state.MoveToColumn(-1, _displayBoard.Columns.Count);

    private void HandleMoveRight() =>
        _state = _state.MoveToColumn(1, _displayBoard.Columns.Count);

    private void HandleMoveUp()
    {
        if (_displayBoard.Columns.Count == 0) return;
        var safeCol = Math.Clamp(_state.SelectedColumn, 0, _displayBoard.Columns.Count - 1);
        _state = _state with { SelectedColumn = safeCol };
        var col = _displayBoard.Columns[safeCol];
        _state = _state.MoveToTask(-1, col.Tasks.Count);
    }

    private void HandleMoveDown()
    {
        if (_displayBoard.Columns.Count == 0) return;
        var safeCol = Math.Clamp(_state.SelectedColumn, 0, _displayBoard.Columns.Count - 1);
        _state = _state with { SelectedColumn = safeCol };
        var col = _displayBoard.Columns[safeCol];
        _state = _state.MoveToTask(1, col.Tasks.Count);
    }

    private void HandleViewDetails()
    {
        if (_displayBoard.Columns.Count > 0)
        {
            var columnIndex = Math.Clamp(_state.SelectedColumn, 0, _displayBoard.Columns.Count - 1);
            var col = _displayBoard.Columns[columnIndex];
            if (col.Tasks.Count > 0 && _state.SelectedTask < col.Tasks.Count)
            {
                var taskIndex = Math.Clamp(_state.SelectedTask, 0, col.Tasks.Count - 1);
                _taskDetailPanel.Show(col.Tasks[taskIndex]);
            }
        }
        _boardService.SavePlanningBoard(_boardPath);
    }

    private void HandleNewTask()
    {
        var inputs = _newTaskDialog.Show();
        if (inputs is not null)
        {
            _taskService.CreateTask(inputs.Title, inputs.Type, inputs.Priority, inputs.Labels);
            _state = _state.MoveToColumn(0, _displayBoard.Columns.Count);
        }
        _boardService.SavePlanningBoard(_boardPath);
    }

    private void HandleMoveTask()
    {
        if (_displayBoard.Columns.Count > 0)
        {
            var columnIndex = Math.Clamp(_state.SelectedColumn, 0, _displayBoard.Columns.Count - 1);
            var col = _displayBoard.Columns[columnIndex];
            if (col.Tasks.Count > 0 && _state.SelectedTask < col.Tasks.Count)
            {
                var taskIndex = Math.Clamp(_state.SelectedTask, 0, col.Tasks.Count - 1);
                var task = col.Tasks[taskIndex];
                var targetStatus = _moveDialog.Show(_board, columnIndex);
                if (targetStatus is not null)
                {
                    _taskService.MoveTask(task, targetStatus.Value);
                    _state = _state with { SelectedTask = 0 };
                }
            }
        }
        _boardService.SavePlanningBoard(_boardPath);
    }

    private void HandleDeleteTask()
    {
        if (_displayBoard.Columns.Count > 0)
        {
            var columnIndex = Math.Clamp(_state.SelectedColumn, 0, _displayBoard.Columns.Count - 1);
            var col = _displayBoard.Columns[columnIndex];
            if (col.Tasks.Count > 0 && _state.SelectedTask < col.Tasks.Count)
            {
                var taskIndex = Math.Clamp(_state.SelectedTask, 0, col.Tasks.Count - 1);
                var task = col.Tasks[taskIndex];
                if (_confirmDialog.Confirm(task))
                {
                    _taskService.DeleteTask(task);
                    _state = _state with { SelectedTask = Math.Max(0, _state.SelectedTask - 1) };
                }
            }
        }
        _boardService.SavePlanningBoard(_boardPath);
    }

    private void HandleChangePriority()
    {
        if (_displayBoard.Columns.Count == 0) return;
        var columnIndex = Math.Clamp(_state.SelectedColumn, 0, _displayBoard.Columns.Count - 1);
        var col = _displayBoard.Columns[columnIndex];
        if (col.Tasks.Count == 0 || _state.SelectedTask >= col.Tasks.Count)
            return;

        var taskIndex = Math.Clamp(_state.SelectedTask, 0, col.Tasks.Count - 1);
        var task = col.Tasks[taskIndex];
        var newPriority = _priorityDialog.Show(task.Priority);
        if (newPriority == task.Priority)
            return;

        var updatedTask = task.SetPriority(newPriority);
        _taskService.UpdateTask(updatedTask);
    }

    private void HandleToggleFilter()
    {
        if (_activeFilter is not null)
        {
            _activeFilter = null;
            _state = _state with { SelectedTask = 0 };
            return;
        }

        var allLabels = _board.Columns
            .SelectMany(c => c.Tasks)
            .SelectMany(t => t.Labels)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(l => l)
            .ToList()
            .AsReadOnly();

        var newFilter = _filterDialog.Show(allLabels);
        if (newFilter is not null)
        {
            _activeFilter = newFilter;
            _state = _state with { SelectedTask = 0 };
        }
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
