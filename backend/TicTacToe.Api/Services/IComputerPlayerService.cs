using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IComputerPlayerService
{
    /// <summary>
    /// Chooses the computer's (O's) next cell index given the current board.
    /// Returns null if no empty cell is available.
    /// </summary>
    int? ChooseMove(Player?[] board);
}
