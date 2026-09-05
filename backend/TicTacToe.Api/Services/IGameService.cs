using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameState CreateGame(GameMode mode);
    GameState GetGame(string gameId);
    GameState MakeMove(string gameId, MoveRequest request);
    GameState UndoMove(string gameId);
    GameState ResetGame(string gameId);
}
