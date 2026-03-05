namespace KanbanCli.Tui;
using KanbanCli.Models;

public class FilterDialog
{
    private enum FilterMode { ByType = 1, ByLabel = 2, ByPriority = 3 }

    public FilterCriteria? Show(IReadOnlyList<string> availableLabels)
    {
        Console.Clear();
        Console.CursorVisible = true;

        var width = DialogHelper.GetBoxWidth();
        var borderColor = Theme.DialogBorder;

        DialogHelper.RenderBoxTop("Filter Board", width, borderColor);
        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = Theme.DialogPrompt;
        var headerText = "Filter by:";
        Console.Write(headerText);
        DialogHelper.RenderBoxRightBorder(headerText.Length, width, borderColor);

        RenderFilterOption(1, "By Type", width, borderColor);
        RenderFilterOption(2, "By Label", width, borderColor);
        RenderFilterOption(3, "By Priority", width, borderColor);

        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = Theme.DialogPrompt;
        Console.Write("Enter number (1-3), or 0 to cancel: ");
        Console.ResetColor();

        var modeInput = Console.ReadLine()?.Trim() ?? string.Empty;

        if (!int.TryParse(modeInput, out var modeChoice) || modeChoice == 0)
        {
            DialogHelper.RenderBoxEmptyLine(width, borderColor);
            DialogHelper.RenderBoxBottom(width, borderColor);
            Console.CursorVisible = false;
            return null;
        }

        var result = modeChoice switch
        {
            (int)FilterMode.ByType => PromptByType(width, borderColor),
            (int)FilterMode.ByLabel => PromptByLabel(availableLabels, width, borderColor),
            (int)FilterMode.ByPriority => PromptByPriority(width, borderColor),
            _ => null
        };

        DialogHelper.RenderBoxEmptyLine(width, borderColor);
        DialogHelper.RenderBoxBottom(width, borderColor);

        return result;
    }

    private static void RenderFilterOption(int number, string text, int width, ConsoleColor borderColor)
    {
        DialogHelper.RenderBoxLeftBorder(borderColor);
        Console.ForegroundColor = Theme.DialogListNumber;
        var numText = $"  {number}. ";
        Console.Write(numText);
        Console.ForegroundColor = Theme.DialogListItem;
        Console.Write(text);
        DialogHelper.RenderBoxRightBorder(numText.Length + text.Length, width, borderColor);
    }

    private static FilterCriteria? PromptByType(int width, ConsoleColor borderColor)
    {
        var selected = DialogHelper.PromptEnumInBox<TaskType>("Task type", width, borderColor, allowZeroCancel: true);

        Console.CursorVisible = false;

        return selected.HasValue ? new FilterCriteria(Type: selected.Value) : null;
    }

    private static FilterCriteria? PromptByLabel(IReadOnlyList<string> availableLabels, int width, ConsoleColor borderColor)
    {
        DialogHelper.RenderBoxEmptyLine(width, borderColor);

        if (availableLabels.Count > 0)
        {
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = Theme.DialogPrompt;
            var headerText = "Available labels:";
            Console.Write(headerText);
            DialogHelper.RenderBoxRightBorder(headerText.Length, width, borderColor);

            for (var i = 0; i < availableLabels.Count; i++)
            {
                DialogHelper.RenderBoxLeftBorder(borderColor);
                Console.ForegroundColor = Theme.DialogListNumber;
                var numText = $"  {i + 1}. ";
                Console.Write(numText);
                Console.ForegroundColor = Theme.DialogListItem;
                var labelText = availableLabels[i];
                Console.Write(labelText);
                DialogHelper.RenderBoxRightBorder(numText.Length + labelText.Length, width, borderColor);
            }

            DialogHelper.RenderBoxEmptyLine(width, borderColor);

            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = Theme.DialogPrompt;
            Console.Write($"Enter number (1-{availableLabels.Count}), label name, or 0 to cancel: ");
            Console.ResetColor();
        }
        else
        {
            DialogHelper.RenderBoxLeftBorder(borderColor);
            Console.ForegroundColor = Theme.DialogPrompt;
            Console.Write("Enter label name, or leave empty to cancel: ");
            Console.ResetColor();
        }

        Console.ForegroundColor = Theme.DialogListItem;
        var input = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.ResetColor();
        Console.CursorVisible = false;

        if (string.IsNullOrWhiteSpace(input) || input == "0")
            return null;

        // Check if the user entered a number that refers to a label from the list
        if (availableLabels.Count > 0
            && int.TryParse(input, out var labelChoice)
            && labelChoice >= 1
            && labelChoice <= availableLabels.Count)
        {
            return new FilterCriteria(Label: availableLabels[labelChoice - 1]);
        }

        return new FilterCriteria(Label: input);
    }

    private static FilterCriteria? PromptByPriority(int width, ConsoleColor borderColor)
    {
        var selected = DialogHelper.PromptEnumInBox<Priority>("Priority", width, borderColor, allowZeroCancel: true);

        Console.CursorVisible = false;

        return selected.HasValue ? new FilterCriteria(Priority: selected.Value) : null;
    }

}
