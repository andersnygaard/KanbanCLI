using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace KanbanCli.Tui;

/// <summary>
/// Renders markdown content with rich terminal formatting inside a box-drawing border.
/// Uses Markdig to parse markdown into an AST and walks the block/inline tree to apply
/// colored headings, styled bullet points, checkbox indicators, bold text, and code blocks.
/// </summary>
public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseTaskLists()
        .Build();

    private static readonly Regex TaskListUncheckedRegex = new(@"^\s*-\s+\[\s\]\s*", RegexOptions.Compiled);
    private static readonly Regex TaskListCheckedRegex = new(@"^\s*-\s+\[x\]\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses markdown content using Markdig and renders it with rich terminal formatting
    /// inside box-drawing borders.
    /// </summary>
    public static void RenderMarkdownContent(string content, int boxWidth, ConsoleColor borderColor)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        var document = Markdown.Parse(content, Pipeline);
        var maxContentWidth = boxWidth - 6;

        DialogHelper.RenderBoxEmptyLine(boxWidth, borderColor);

        foreach (var block in document)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    RenderHeading(heading, boxWidth, maxContentWidth, borderColor);
                    break;

                case ListBlock list:
                    RenderList(list, boxWidth, maxContentWidth, borderColor);
                    break;

                case FencedCodeBlock codeBlock:
                    RenderCodeBlock(codeBlock, boxWidth, maxContentWidth, borderColor);
                    break;

                case ThematicBreakBlock:
                    DialogHelper.RenderBoxSeparator(boxWidth, borderColor);
                    break;

                case ParagraphBlock paragraph:
                    RenderParagraph(paragraph, boxWidth, maxContentWidth, borderColor);
                    break;
            }
        }

        DialogHelper.RenderBoxEmptyLine(boxWidth, borderColor);
    }

    private static void RenderHeading(HeadingBlock heading, int boxWidth, int maxContentWidth, ConsoleColor borderColor)
    {
        var text = ExtractPlainText(heading.Inline);
        var color = heading.Level switch
        {
            2 => ConsoleColor.Cyan,
            3 => ConsoleColor.DarkCyan,
            _ => ConsoleColor.DarkCyan
        };

        var displayText = $"  {text}";
        if (displayText.Length > maxContentWidth)
            displayText = displayText[..maxContentWidth];

        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = color;
        Console.Write(displayText);
        Console.ResetColor();
        DialogHelper.RenderBoxRightBorder(displayText.Length, boxWidth, borderColor);
    }

    private static void RenderList(ListBlock list, int boxWidth, int maxContentWidth, ConsoleColor borderColor)
    {
        foreach (var item in list)
        {
            if (item is not ListItemBlock listItem)
                continue;

            RenderListItem(listItem, boxWidth, maxContentWidth, borderColor);
        }
    }

    private static void RenderListItem(ListItemBlock listItem, int boxWidth, int maxContentWidth, ConsoleColor borderColor)
    {
        // Check for task list checkbox
        var (isTaskList, isChecked) = DetectTaskListCheckbox(listItem);
        var text = ExtractListItemText(listItem);

        DialogHelper.RenderBoxLeftBorder(borderColor);

        var writtenLength = 0;

        if (isTaskList)
        {
            if (isChecked)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  \u2611 ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  \u2610 ");
            }
            writtenLength = 4;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("  \u2022 ");
            Console.ResetColor();
            writtenLength = 4;
        }

        var remainingWidth = maxContentWidth - writtenLength;
        if (remainingWidth > 0)
        {
            RenderInlineContent(listItem, remainingWidth, out var inlineLength);
            writtenLength += inlineLength;
        }

        Console.ResetColor();
        DialogHelper.RenderBoxRightBorder(writtenLength, boxWidth, borderColor);
    }

    private static (bool IsTaskList, bool IsChecked) DetectTaskListCheckbox(ListItemBlock listItem)
    {
        // Try Markdig's TaskList extension first
        foreach (var subBlock in listItem)
        {
            if (subBlock is not ParagraphBlock paragraph || paragraph.Inline is null)
                continue;

            foreach (var inline in paragraph.Inline)
            {
                if (inline is TaskList taskList)
                    return (true, taskList.Checked);
            }
        }

        // Fallback: check the raw text for checkbox patterns
        var rawText = ExtractRawListItemText(listItem);
        if (TaskListCheckedRegex.IsMatch(rawText))
            return (true, true);
        if (TaskListUncheckedRegex.IsMatch(rawText))
            return (true, false);

        return (false, false);
    }

    private static void RenderInlineContent(ListItemBlock listItem, int maxWidth, out int writtenLength)
    {
        writtenLength = 0;

        foreach (var subBlock in listItem)
        {
            if (subBlock is not ParagraphBlock paragraph || paragraph.Inline is null)
                continue;

            foreach (var inline in paragraph.Inline)
            {
                if (writtenLength >= maxWidth)
                    break;

                var remaining = maxWidth - writtenLength;

                switch (inline)
                {
                    case TaskList:
                        // Skip — already handled by the checkbox rendering
                        break;

                    case EmphasisInline emphasis when emphasis.DelimiterChar == '*' && emphasis.DelimiterCount == 2:
                        var boldText = ExtractPlainText(emphasis);
                        boldText = TruncateText(boldText, remaining);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(boldText);
                        writtenLength += boldText.Length;
                        break;

                    case CodeInline code:
                        var codeText = TruncateText(code.Content, remaining);
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(codeText);
                        writtenLength += codeText.Length;
                        break;

                    case LiteralInline literal:
                        var literalText = literal.Content.ToString();
                        literalText = TruncateText(literalText, remaining);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(literalText);
                        writtenLength += literalText.Length;
                        break;

                    case LineBreakInline:
                        break;

                    default:
                        var fallbackText = inline.ToString() ?? string.Empty;
                        fallbackText = TruncateText(fallbackText, remaining);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(fallbackText);
                        writtenLength += fallbackText.Length;
                        break;
                }
            }
        }
    }

    private static void RenderCodeBlock(FencedCodeBlock codeBlock, int boxWidth, int maxContentWidth, ConsoleColor borderColor)
    {
        DialogHelper.RenderBoxEmptyLine(boxWidth, borderColor);

        foreach (var line in codeBlock.Lines)
        {
            var lineText = line.ToString();
            var displayText = $"    {lineText}";
            if (displayText.Length > maxContentWidth)
                displayText = displayText[..maxContentWidth];

            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(displayText);
            Console.ResetColor();
            DialogHelper.RenderBoxRightBorder(displayText.Length, boxWidth, borderColor);
        }

        DialogHelper.RenderBoxEmptyLine(boxWidth, borderColor);
    }

    private static void RenderParagraph(ParagraphBlock paragraph, int boxWidth, int maxContentWidth, ConsoleColor borderColor)
    {
        if (paragraph.Inline is null)
            return;

        DialogHelper.RenderBoxLeftBorder(borderColor);

        var writtenLength = 0;
        var prefix = "  ";
        Console.Write(prefix);
        writtenLength += prefix.Length;

        foreach (var inline in paragraph.Inline)
        {
            if (writtenLength >= maxContentWidth)
                break;

            var remaining = maxContentWidth - writtenLength;

            switch (inline)
            {
                case EmphasisInline emphasis when emphasis.DelimiterChar == '*' && emphasis.DelimiterCount == 2:
                    var boldText = ExtractPlainText(emphasis);
                    boldText = TruncateText(boldText, remaining);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(boldText);
                    writtenLength += boldText.Length;
                    break;

                case CodeInline code:
                    var codeText = TruncateText(code.Content, remaining);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(codeText);
                    writtenLength += codeText.Length;
                    break;

                case LiteralInline literal:
                    var literalText = literal.Content.ToString();
                    literalText = TruncateText(literalText, remaining);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(literalText);
                    writtenLength += literalText.Length;
                    break;

                case LineBreakInline:
                    break;

                default:
                    var fallbackText = inline.ToString() ?? string.Empty;
                    fallbackText = TruncateText(fallbackText, remaining);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(fallbackText);
                    writtenLength += fallbackText.Length;
                    break;
            }
        }

        Console.ResetColor();
        DialogHelper.RenderBoxRightBorder(writtenLength, boxWidth, borderColor);
    }

    private static string ExtractPlainText(ContainerInline? container)
    {
        if (container is null)
            return string.Empty;

        var result = string.Empty;
        foreach (var inline in container)
        {
            result += inline switch
            {
                LiteralInline literal => literal.Content.ToString(),
                EmphasisInline emphasis => ExtractPlainText(emphasis),
                CodeInline code => code.Content,
                _ => string.Empty
            };
        }
        return result;
    }

    private static string ExtractListItemText(ListItemBlock listItem)
    {
        foreach (var subBlock in listItem)
        {
            if (subBlock is ParagraphBlock paragraph)
                return ExtractPlainText(paragraph.Inline);
        }
        return string.Empty;
    }

    private static string ExtractRawListItemText(ListItemBlock listItem)
    {
        foreach (var subBlock in listItem)
        {
            if (subBlock is ParagraphBlock paragraph)
            {
                var span = paragraph.Span;
                return paragraph.Inline?.ToString() ?? string.Empty;
            }
        }
        return string.Empty;
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (maxLength <= 0)
            return string.Empty;

        return text.Length > maxLength ? text[..maxLength] : text;
    }
}
