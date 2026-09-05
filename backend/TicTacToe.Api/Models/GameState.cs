namespace TicTacToe.Api.Models;

/// <summary>
/// Server-owned representation of a single game session. This is the source of truth;
/// the frontend only ever renders what this object (via GameStateResponse) reports.
/// </summary>
public class GameState
{
    public string GameId { get; set; } = Guid.NewGuid().ToString("N");
    public GameMode Mode { get; set; }

    /// <summary>Board is a flat 9-cell array, index = row * 3 + column. null = empty cell.</summary>
    public Player?[] Board { get; set; } = new Player?[9];

    public Player CurrentPlayer { get; set; } = Player.X;
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Player? Winner { get; set; }
    public List<int>? WinningCells { get; set; }
    public List<MoveHistoryItem> MoveHistory { get; set; } = new();

    /// <summary>
    /// True once this completed game's result has been applied to the scoreboard,
    /// so a completed game can never be double-counted.
    /// </summary>
    public bool ScoreboardUpdatedForThisGame { get; set; }
}
