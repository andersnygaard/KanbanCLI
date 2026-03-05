using KanbanCli.Models;

namespace KanbanCli.Services;

public interface IBoardService
{
    Board GetBoard();
    string GeneratePlanningBoard();
}
