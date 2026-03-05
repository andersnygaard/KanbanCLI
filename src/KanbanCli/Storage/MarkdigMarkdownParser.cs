using System.Text;
using System.Text.RegularExpressions;
using KanbanCli.Models;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using TaskStatus = KanbanCli.Models.TaskStatus;

namespace KanbanCli.Storage;

public class MarkdigMarkdownParser : IMarkdownParser
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().Build();
    private static readonly Regex FileNameRegex = new(@"(\d+)-([\w]+)-(.+)\.md", RegexOptions.Compiled);

    private static readonly HashSet<string> KnownMetadataKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Status", "Created", "Completed", "Priority", "Labels"
    };

    public TaskItem Parse(string markdown, int id, TaskType type)
    {
        var document = Markdown.Parse(markdown, Pipeline);

        var title = ExtractTitle(document);
        var metadata = ExtractMetadata(document);
        var extraMetadata = ExtractExtraMetadata(metadata);
        var sections = ExtractSections(markdown, document);

        var status = ParseStatus(metadata.GetValueOrDefault("Status"));
        var priority = ParsePriority(metadata.GetValueOrDefault("Priority"));
        var labels = ParseLabels(metadata.GetValueOrDefault("Labels"));
        var createdDate = ParseCreatedDate(metadata.GetValueOrDefault("Created"));
        var completedDate = ParseCreatedDate(metadata.GetValueOrDefault("Completed"));

        return new TaskItem
        {
            Id = id,
            Title = title,
            Type = type,
            Status = status,
            Priority = priority,
            Labels = labels,
            CreatedDate = createdDate,
            CompletedDate = completedDate,
            ExtraMetadata = extraMetadata,
            Sections = sections
        };
    }

    public string Serialize(TaskItem task)
    {
        var sb = new StringBuilder();

        var typePrefix = task.Type.ToString().ToUpperInvariant();
        sb.AppendLine($"# {typePrefix}: {task.Title}");
        sb.AppendLine();
        sb.AppendLine($"**Status**: {FormatStatus(task.Status)}");
        sb.AppendLine($"**Created**: {task.CreatedDate?.ToString(BoardConstants.DateFormat)}");
        if (task.CompletedDate.HasValue)
            sb.AppendLine($"**Completed**: {task.CompletedDate.Value.ToString(BoardConstants.DateFormat)}");
        sb.AppendLine($"**Priority**: {task.Priority}");

        var labelsValue = task.Labels.Count > 0
            ? string.Join(", ", task.Labels)
            : string.Empty;
        sb.AppendLine($"**Labels**: {labelsValue}");

        foreach (var entry in task.ExtraMetadata)
            sb.AppendLine($"**{entry.Key}**: {entry.Value}");

        foreach (var section in task.Sections)
        {
            sb.AppendLine();
            sb.AppendLine($"## {section.Key}");
            var trimmedContent = section.Value.Trim('\r', '\n');
            if (!string.IsNullOrWhiteSpace(trimmedContent))
            {
                sb.AppendLine();
                sb.AppendLine(trimmedContent);
            }
        }

        return sb.ToString();
    }

    public (int Id, TaskType Type, string Description) ParseFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        var match = FileNameRegex.Match(name);

        if (!match.Success)
            return (0, TaskType.Feature, string.Empty);

        var id = int.Parse(match.Groups[1].Value);
        var typeString = match.Groups[2].Value;
        var description = match.Groups[3].Value;

        if (!Enum.TryParse<TaskType>(typeString, ignoreCase: true, out var taskType))
            return (id, TaskType.Feature, description);

        return (id, taskType, description);
    }

    private static string ExtractTitle(MarkdownDocument document)
    {
        foreach (var block in document)
        {
            if (block is HeadingBlock heading && heading.Level == 1)
            {
                var raw = ExtractInlineText(heading.Inline);
                var colonIndex = raw.IndexOf(':');
                if (colonIndex >= 0 && colonIndex < raw.Length - 1)
                    return raw[(colonIndex + 1)..].Trim();
                return raw.Trim();
            }
        }
        return string.Empty;
    }

    private static Dictionary<string, string> ExtractMetadata(MarkdownDocument document)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in document)
        {
            if (block is not ParagraphBlock paragraph)
                continue;

            var inline = paragraph.Inline;
            if (inline is null)
                continue;

            foreach (var child in inline)
            {
                if (child is not EmphasisInline emphasis || emphasis.DelimiterChar != '*' || emphasis.DelimiterCount != 2)
                    continue;

                var key = ExtractInlineText(emphasis).TrimEnd(':');

                // The value literal follows the bold key as a sibling LiteralInline: ": value"
                var next = child.NextSibling;
                if (next is not LiteralInline literal)
                    continue;

                var value = literal.Content.ToString().TrimStart(':').Trim();
                metadata[key] = value;
            }
        }

        return metadata;
    }

    private static IReadOnlyDictionary<string, string> ExtractExtraMetadata(Dictionary<string, string> allMetadata)
    {
        var extra = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in allMetadata)
        {
            if (!KnownMetadataKeys.Contains(entry.Key))
                extra[entry.Key] = entry.Value;
        }

        return extra;
    }

    private static IReadOnlyDictionary<string, string> ExtractSections(string markdown, MarkdownDocument document)
    {
        var sections = new Dictionary<string, string>();
        var lines = markdown.Split('\n');

        // Collect all level-2 headings with their line positions
        var headingPositions = new List<(string Name, int LineIndex)>();

        foreach (var block in document)
        {
            if (block is HeadingBlock heading && heading.Level == 2)
            {
                var headingName = ExtractInlineText(heading.Inline).Trim();
                var lineIndex = heading.Line;
                headingPositions.Add((headingName, lineIndex));
            }
        }

        // Extract the content between each heading and the next
        for (var i = 0; i < headingPositions.Count; i++)
        {
            var (name, startLine) = headingPositions[i];
            var contentStartLine = startLine + 1;
            var contentEndLine = i + 1 < headingPositions.Count
                ? headingPositions[i + 1].LineIndex
                : lines.Length;

            var contentLines = lines[contentStartLine..contentEndLine];
            var content = string.Join("\n", contentLines).Trim('\r', '\n');

            sections[name] = content;
        }

        return sections;
    }

    private static string ExtractInlineText(ContainerInline? container)
    {
        if (container is null)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var inline in container)
        {
            sb.Append(inline switch
            {
                LiteralInline literal => literal.Content.ToString(),
                EmphasisInline emphasis => ExtractInlineText(emphasis),
                LineBreakInline => "\n",
                _ => string.Empty
            });
        }
        return sb.ToString();
    }

    private static TaskStatus ParseStatus(string? value)
    {
        return value switch
        {
            null or "" => TaskStatus.Backlog,
            "Backlog" => TaskStatus.Backlog,
            "InProgress" or "In Progress" or "In-Progress" => TaskStatus.InProgress,
            "Done" => TaskStatus.Done,
            "OnHold" or "On Hold" or "On-Hold" => TaskStatus.OnHold,
            _ => throw new FormatException($"Unknown task status: '{value}'")
        };
    }

    private static Priority ParsePriority(string? value)
    {
        return value switch
        {
            "High" => Priority.High,
            "Medium" => Priority.Medium,
            "Low" => Priority.Low,
            _ => Priority.Medium
        };
    }

    private static IReadOnlyList<string> ParseLabels(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList()
            .AsReadOnly();
    }

    private static DateTime? ParseCreatedDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var date))
            return date;

        return null;
    }

    private static string FormatStatus(TaskStatus status)
    {
        return status switch
        {
            TaskStatus.Backlog => "Backlog",
            TaskStatus.InProgress => "In Progress",
            TaskStatus.Done => "Done",
            TaskStatus.OnHold => "On Hold",
            _ => status.ToString()
        };
    }
}
