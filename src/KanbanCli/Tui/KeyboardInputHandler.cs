namespace KanbanCli.Tui;

public class KeyboardInputHandler : IInputHandler
{
    public BoardCommand ReadCommand()
    {
        var keyInfo = Console.ReadKey(intercept: true);

        return keyInfo.Key switch
        {
            ConsoleKey.LeftArrow => BoardCommand.MoveLeft,
            ConsoleKey.RightArrow => BoardCommand.MoveRight,
            ConsoleKey.UpArrow => BoardCommand.MoveUp,
            ConsoleKey.DownArrow => BoardCommand.MoveDown,
            ConsoleKey.Enter => BoardCommand.ViewDetails,
            ConsoleKey.Q => BoardCommand.Quit,
            _ => MapCharacterKey(keyInfo.KeyChar)
        };
    }

    private static BoardCommand MapCharacterKey(char keyChar)
    {
        return char.ToLower(keyChar) switch
        {
            'n' => BoardCommand.NewTask,
            'm' => BoardCommand.MoveTask,
            'd' => BoardCommand.DeleteTask,
            'p' => BoardCommand.ChangePriority,
            'f' => BoardCommand.ToggleFilter,
            _ => BoardCommand.None
        };
    }
}
