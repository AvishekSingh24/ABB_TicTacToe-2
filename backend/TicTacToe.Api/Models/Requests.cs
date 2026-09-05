namespace TicTacToe.Api.Models;

public class CreateGameRequest
{
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
}

public class MoveRequest
{
    public Player Player { get; set; }

    /// <summary>Preferred way to submit a move: flat index 0-8.</summary>
    public int? CellIndex { get; set; }

    /// <summary>Alternative to CellIndex: 0-based row.</summary>
    public int? Row { get; set; }

    /// <summary>Alternative to CellIndex: 0-based column.</summary>
    public int? Column { get; set; }
}
