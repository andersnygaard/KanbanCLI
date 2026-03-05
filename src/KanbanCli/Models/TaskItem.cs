namespace KanbanCli.Models;

public record TaskItem
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public TaskType Type { get; init; }
    public Priority Priority { get; init; }
    public TaskStatus Status { get; init; }
    public IReadOnlyList<string> Labels { get; init; } = [];
    public DateTime? CreatedDate { get; init; }
    public DateTime? CompletedDate { get; init; }
    public IReadOnlyDictionary<string, string> ExtraMetadata { get; init; } = new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> Sections { get; init; } = new Dictionary<string, string>();

    public TaskItem ChangeStatus(TaskStatus newStatus)
    {
        var completedDate = DetermineCompletedDate(newStatus);
        return this with { Status = newStatus, CompletedDate = completedDate };
    }

    public TaskItem AddLabel(string label)
    {
        var alreadyExists = Labels.Any(existing => string.Equals(existing, label, StringComparison.OrdinalIgnoreCase));
        if (alreadyExists)
            return this;

        var updatedLabels = Labels.Append(label).ToList();
        return this with { Labels = updatedLabels };
    }

    public TaskItem RemoveLabel(string label)
    {
        var updatedLabels = Labels
            .Where(existing => !string.Equals(existing, label, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (updatedLabels.Count == Labels.Count)
            return this;

        return this with { Labels = updatedLabels };
    }

    public TaskItem SetPriority(Priority priority)
    {
        return this with { Priority = priority };
    }

    public bool MatchesFilter(FilterCriteria filter)
    {
        if (filter.Type is not null && filter.Type != Type)
            return false;

        if (filter.Priority is not null && filter.Priority != Priority)
            return false;

        if (filter.Label is not null && !Labels.Any(l => string.Equals(l, filter.Label, StringComparison.OrdinalIgnoreCase)))
            return false;

        return true;
    }

    public string GenerateFileName()
    {
        var typePrefix = Type.ToString().ToUpperInvariant();
        var kebabTitle = ToKebabCase(Title);

        if (string.IsNullOrEmpty(kebabTitle))
            kebabTitle = "untitled";

        return $"{Id.ToString(BoardConstants.IdFormat)}-{typePrefix}-{kebabTitle}.md";
    }

    private DateTime? DetermineCompletedDate(TaskStatus newStatus)
    {
        if (newStatus == TaskStatus.Done)
            return DateTime.UtcNow;

        if (Status == TaskStatus.Done)
            return null;

        return CompletedDate;
    }

    private static string ToKebabCase(string title)
    {
        var normalized = title.ToLowerInvariant();
        var kebab = new System.Text.StringBuilder();

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
                kebab.Append(character);
            else if (kebab.Length > 0 && kebab[^1] != '-')
                kebab.Append('-');
        }

        return kebab.ToString().Trim('-');
    }
}
