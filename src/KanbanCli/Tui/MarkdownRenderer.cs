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
    private const int BoxContentPadding = 6;

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
        var maxContentWidth = boxWidth - BoxContentPadding;

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
            2 => Theme.MdHeading,
            3 => Theme.MdSubheading,
            _ => Theme.MdSubheading
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
                Console.ForegroundColor = Theme.MdCheckboxChecked;
                Console.Write("  \u2611 ");
            }
            else
            {
                Console.ForegroundColor = Theme.MdCheckboxUnchecked;
                Console.Write("  \u2610 ");
            }
            writtenLength = 4;
        }
        else
        {
            Console.ForegroundColor = Theme.MdBullet;
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

    private static int RenderInlineSequence(ContainerInline container, int maxWidth)
    {
        var written = 0;
        foreach (var inline in container)
        {
            if (written >= maxWidth)
                break;
            written += RenderSingleInline(inline, maxWidth - written);
        }
        return written;
    }

    private static int RenderSingleInline(Inline inline, int remaining)
    {
        return inline switch
        {
            TaskList => 0,
            EmphasisInline emphasis when emphasis.DelimiterChar == '*' && emphasis.DelimiterCount == 2
                => WriteInlineText(ExtractPlainText(emphasis), remaining, Theme.MdLiteral),
            CodeInline code
                => WriteInlineText(code.Content, remaining, Theme.MdCode),
            LiteralInline literal
                => WriteInlineText(literal.Content.ToString(), remaining, Theme.MdLiteral),
            LineBreakInline => 0,
            _ => WriteInlineText(inline.ToString() ?? string.Empty, remaining, Theme.MdLiteral),
        };
    }

    private static int WriteInlineText(string text, int remaining, ConsoleColor color)
    {
        var truncated = TruncateText(text, remaining);
        Console.ForegroundColor = color;
        Console.Write(truncated);
        return truncated.Length;
    }

    private static void RenderInlineContent(ListItemBlock listItem, int maxWidth, out int writtenLength)
    {
        writtenLength = 0;

        foreach (var subBlock in listItem)
        {
            if (subBlock is not ParagraphBlock paragraph || paragraph.Inline is null)
                continue;

            writtenLength += RenderInlineSequence(paragraph.Inline, maxWidth - writtenLength);
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
            Console.ForegroundColor = Theme.MdCode;
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

        var prefix = "  ";
        Console.Write(prefix);
        var writtenLength = prefix.Length;

        writtenLength += RenderInlineSequence(paragraph.Inline, maxContentWidth - writtenLength);

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
