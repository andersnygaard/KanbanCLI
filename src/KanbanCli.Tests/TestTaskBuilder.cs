using KanbanCli.Models;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Tests;

public class TestTaskBuilder
{
    private int _id = 1;
    private string _title = "Test task";
    private TaskType _type = TaskType.Feature;
    private Priority _priority = Priority.Medium;
    private TaskStatus _status = TaskStatus.Backlog;
    private IReadOnlyList<string> _labels = [];
    private DateTime _createdDate = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc);
    private DateTime? _completedDate;

    public TestTaskBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public TestTaskBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TestTaskBuilder WithType(TaskType type)
    {
        _type = type;
        return this;
    }

    public TestTaskBuilder WithPriority(Priority priority)
    {
        _priority = priority;
        return this;
    }

    public TestTaskBuilder WithStatus(TaskStatus status)
    {
        _status = status;
        return this;
    }

    public TestTaskBuilder WithLabels(params string[] labels)
    {
        _labels = labels.ToList().AsReadOnly();
        return this;
    }

    public TestTaskBuilder WithCreatedDate(DateTime createdDate)
    {
        _createdDate = createdDate;
        return this;
    }

    public TestTaskBuilder WithCompletedDate(DateTime? completedDate)
    {
        _completedDate = completedDate;
        return this;
    }

    public TaskItem Build()
    {
        return new TaskItem
        {
            Id = _id,
            Title = _title,
            Type = _type,
            Priority = _priority,
            Status = _status,
            Labels = _labels,
            CreatedDate = _createdDate,
            CompletedDate = _completedDate
        };
    }
}
