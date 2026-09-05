namespace TicTacToe.Api.Models;

/// <summary>
/// What the frontend actually renders from. Includes a scoreboard snapshot so the
/// board screen can show live scores without a second round trip after every move.
/// </summary>
public class GameStateResponse
{
    public string GameId { get; set; } = "";
    public GameMode Mode { get; set; }
    public List<string?> Board { get; set; } = new();
    public Player CurrentPlayer { get; set; }
    public GameStatus Status { get; set; }
    public Player? Winner { get; set; }
    public List<int>? WinningCells { get; set; }
    public List<MoveHistoryItem> MoveHistory { get; set; } = new();
    public bool CanUndo { get; set; }
    public Scoreboard Scoreboard { get; set; } = new();

    public static GameStateResponse FromGameState(GameState state, Scoreboard scoreboard)
    {
        return new GameStateResponse
        {
            GameId = state.GameId,
            Mode = state.Mode,
            Board = state.Board.Select(c => c?.ToString()).ToList(),
            CurrentPlayer = state.CurrentPlayer,
            Status = state.Status,
            Winner = state.Winner,
            WinningCells = state.WinningCells,
            MoveHistory = state.MoveHistory,
            // Undo is disabled once the game is complete (see README: Clarification 2, Option A)
            CanUndo = state.Status == GameStatus.InProgress && state.MoveHistory.Count > 0,
            Scoreboard = scoreboard
        };
    }
}

public class ApiError
{
    public string Message { get; set; } = "";
}
